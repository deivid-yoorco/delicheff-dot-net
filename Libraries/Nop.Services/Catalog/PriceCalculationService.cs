using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog.Cache;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Payments;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Price calculation service
    /// </summary>
    public partial class PriceCalculationService : IPriceCalculationService
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IDiscountService _discountService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductService _productService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IPaymentService _paymentService;
        private readonly ISpecialDiscountTakeXPayYService _specialDiscountTakeXPayYService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="workContext">Work context</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="discountService">Discount service</param>
        /// <param name="categoryService">Category service</param>
        /// <param name="manufacturerService">Manufacturer service</param>
        /// <param name="productAttributeParser">Product atrribute parser</param>
        /// <param name="productService">Product service</param>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="shoppingCartSettings">Shopping cart settings</param>
        /// <param name="catalogSettings">Catalog settings</param>
        public PriceCalculationService(IWorkContext workContext,
            IStoreContext storeContext,
            IDiscountService discountService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductAttributeParser productAttributeParser,
            IProductService productService,
            IStaticCacheManager cacheManager,
            ShoppingCartSettings shoppingCartSettings,
            CatalogSettings catalogSettings,
            IPaymentService paymentService,
            ISpecialDiscountTakeXPayYService specialDiscountTakeXPayYService)
        {
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._discountService = discountService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productAttributeParser = productAttributeParser;
            this._productService = productService;
            this._cacheManager = cacheManager;
            this._shoppingCartSettings = shoppingCartSettings;
            this._catalogSettings = catalogSettings;
            this._paymentService = paymentService;
            this._specialDiscountTakeXPayYService = specialDiscountTakeXPayYService;
        }

        #endregion

        #region Nested classes

        /// <summary>
        /// Product price (for caching)
        /// </summary>
        [Serializable]
        protected class ProductPriceForCaching
        {
            /// <summary>
            /// Ctor
            /// </summary>
            public ProductPriceForCaching()
            {
                this.AppliedDiscounts = new List<DiscountForCaching>();
            }

            /// <summary>
            /// Price
            /// </summary>
            public decimal Price { get; set; }
            /// <summary>
            /// Applied discount amount
            /// </summary>
            public decimal AppliedDiscountAmount { get; set; }
            /// <summary>
            /// Applied discounts
            /// </summary>
            public List<DiscountForCaching> AppliedDiscounts { get; set; }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets allowed discounts applied to product
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <returns>Discounts</returns>
        protected virtual IList<DiscountForCaching> GetAllowedDiscountsAppliedToProduct(Product product, Customer customer)
        {
            var allowedDiscounts = new List<DiscountForCaching>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            if (product.HasDiscountsApplied)
            {
                //we use this property ("HasDiscountsApplied") for performance optimization to avoid unnecessary database calls
                foreach (var discount in product.AppliedDiscounts)
                {
                    if (_discountService.ValidateDiscount(discount, customer).IsValid &&
                        discount.DiscountType == DiscountType.AssignedToSkus)
                        allowedDiscounts.Add(discount.MapDiscount());
                }
            }

            return allowedDiscounts;
        }

        /// <summary>
        /// Gets allowed discounts applied to categories
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <returns>Discounts</returns>
        protected virtual IList<DiscountForCaching> GetAllowedDiscountsAppliedToCategories(Product product, Customer customer)
        {
            var allowedDiscounts = new List<DiscountForCaching>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            //load cached discount models (performance optimization)
            foreach (var discount in _discountService.GetAllDiscountsForCaching(DiscountType.AssignedToCategories))
            {
                //load identifier of categories with this discount applied to
                var discountCategoryIds = _discountService.GetAppliedCategoryIds(discount, customer);

                //compare with categories of this product
                var productCategoryIds = new List<int>();
                if (discountCategoryIds.Any())
                {
                    //load identifier of categories of this product
                    var cacheKey = string.Format(PriceCacheEventConsumer.PRODUCT_CATEGORY_IDS_MODEL_KEY,
                        product.Id,
                        string.Join(",", customer.GetCustomerRoleIds()),
                        _storeContext.CurrentStore.Id);
                    productCategoryIds = _cacheManager.Get(cacheKey, () =>
                        _categoryService
                        .GetProductCategoriesByProductId(product.Id)
                        .Select(x => x.CategoryId)
                        .ToList());
                }

                foreach (var categoryId in productCategoryIds)
                {
                    if (discountCategoryIds.Contains(categoryId))
                    {
                        if (_discountService.ValidateDiscount(discount, customer).IsValid &&
                            !allowedDiscounts.ContainsDiscount(discount))
                            allowedDiscounts.Add(discount);
                    }
                }
            }

            return allowedDiscounts;
        }

        /// <summary>
        /// Gets allowed discounts applied to manufacturers
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <returns>Discounts</returns>
        protected virtual IList<DiscountForCaching> GetAllowedDiscountsAppliedToManufacturers(Product product, Customer customer)
        {
            var allowedDiscounts = new List<DiscountForCaching>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            foreach (var discount in _discountService.GetAllDiscountsForCaching(DiscountType.AssignedToManufacturers))
            {
                //load identifier of manufacturers with this discount applied to
                var discountManufacturerIds = _discountService.GetAppliedManufacturerIds(discount, customer);

                //compare with manufacturers of this product
                var productManufacturerIds = new List<int>();
                if (discountManufacturerIds.Any())
                {
                    //load identifier of manufacturers of this product
                    var cacheKey = string.Format(PriceCacheEventConsumer.PRODUCT_MANUFACTURER_IDS_MODEL_KEY,
                        product.Id,
                        string.Join(",", customer.GetCustomerRoleIds()),
                        _storeContext.CurrentStore.Id);
                    productManufacturerIds = _cacheManager.Get(cacheKey, () =>
                        _manufacturerService
                        .GetProductManufacturersByProductId(product.Id)
                        .Select(x => x.ManufacturerId)
                        .ToList());
                }

                foreach (var manufacturerId in productManufacturerIds)
                {
                    if (discountManufacturerIds.Contains(manufacturerId))
                    {
                        if (_discountService.ValidateDiscount(discount, customer).IsValid &&
                            !allowedDiscounts.ContainsDiscount(discount))
                            allowedDiscounts.Add(discount);
                    }
                }
            }

            return allowedDiscounts;
        }

        /// <summary>
        /// Gets allowed discounts
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <param name="applyCoupons">Value to know if to take coupons with code</param>
        /// <returns>Discounts</returns>
        protected virtual IList<DiscountForCaching> GetAllowedDiscounts(Product product, Customer customer, bool applyCoupons = true)
        {
            var allowedDiscounts = new List<DiscountForCaching>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            //discounts applied to products
            foreach (var discount in GetAllowedDiscountsAppliedToProduct(product, customer))
                if (!allowedDiscounts.ContainsDiscount(discount))
                    allowedDiscounts.Add(discount);

            //discounts applied to categories
            foreach (var discount in GetAllowedDiscountsAppliedToCategories(product, customer))
                if (!allowedDiscounts.ContainsDiscount(discount))
                    allowedDiscounts.Add(discount);

            //discounts applied to manufacturers
            foreach (var discount in GetAllowedDiscountsAppliedToManufacturers(product, customer))
                if (!allowedDiscounts.ContainsDiscount(discount))
                    allowedDiscounts.Add(discount);

            //check if coupons can be used
            if (!applyCoupons)
                allowedDiscounts = allowedDiscounts.Where(x => !x.RequiresCouponCode).ToList();

            return allowedDiscounts;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="productPriceWithoutDiscount">Already calculated product price without discount</param>
        /// <param name="appliedDiscounts">Applied discounts</param>
        /// <param name="doneDiscounts">Discounts already applied somewhere else, used to calculate exact value discounts</param>
        /// <param name="applyCoupons">Value to know if to take coupons with code</param>
        /// <returns>Discount amount</returns>
        public virtual decimal GetDiscountAmount(Product product,
            Customer customer,
            decimal productPriceWithoutDiscount,
            out List<DiscountForCaching> appliedDiscounts,
            List<DiscountAppliedTemp> doneDiscounts = null,
            bool applyCoupons = true)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            appliedDiscounts = null;
            var appliedDiscountAmount = decimal.Zero;

            //we don't apply discounts to products with price entered by a customer
            if (product.CustomerEntersPrice)
                return appliedDiscountAmount;

            //discounts are disabled
            if (_catalogSettings.IgnoreDiscounts)
                return appliedDiscountAmount;

            var allowedDiscounts = GetAllowedDiscounts(product, customer, applyCoupons);

            //no discounts
            if (!allowedDiscounts.Any())
                return appliedDiscountAmount;

            appliedDiscounts = allowedDiscounts.GetPreferredDiscount(productPriceWithoutDiscount, out appliedDiscountAmount, doneDiscounts);
            return appliedDiscountAmount;
        }

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="productPriceWithoutDiscount">Already calculated product price without discount</param>
        /// <returns>Discount amount</returns>
        public virtual decimal GetDiscountAmount(Product product,
            Customer customer,
            decimal productPriceWithoutDiscount)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var appliedDiscountAmount = decimal.Zero;

            //we don't apply discounts to products with price entered by a customer
            if (product.CustomerEntersPrice)
                return appliedDiscountAmount;

            //discounts are disabled
            if (_catalogSettings.IgnoreDiscounts)
                return appliedDiscountAmount;

            var allowedDiscounts = GetAllowedDiscounts(product, customer);

            //no discounts
            if (!allowedDiscounts.Any())
                return appliedDiscountAmount;

            return appliedDiscountAmount;
        }

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <param name="quantity">Shopping cart item quantity</param>
        /// <param name="doneDiscounts">Discounts already applied somewhere else, used to calculate exact value discounts</param>
        /// <returns>Final price</returns>
        public virtual decimal GetFinalPrice(Product product,
            Customer customer,
            decimal additionalCharge = decimal.Zero,
            bool includeDiscounts = true,
            int quantity = 1,
            List<DiscountAppliedTemp> doneDiscounts = null)
        {
            decimal discountAmount;
            List<DiscountForCaching> appliedDiscounts;
            return GetFinalPrice(product, customer, additionalCharge, includeDiscounts,
                quantity, out discountAmount, out appliedDiscounts, doneDiscounts);
        }
        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <param name="quantity">Shopping cart item quantity</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscounts">Applied discounts</param>
        /// <param name="doneDiscounts">Discounts already applied somewhere else, used to calculate exact value discounts</param>
        /// <returns>Final price</returns>
        public virtual decimal GetFinalPrice(Product product,
            Customer customer,
            decimal additionalCharge,
            bool includeDiscounts,
            int quantity,
            out decimal discountAmount,
            out List<DiscountForCaching> appliedDiscounts,
            List<DiscountAppliedTemp> doneDiscounts = null)
        {
            return GetFinalPrice(product, customer,
                additionalCharge, includeDiscounts, quantity,
                null, null,
                out discountAmount, out appliedDiscounts, doneDiscounts);
        }

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <param name="quantity">Shopping cart item quantity</param>
        /// <param name="rentalStartDate">Rental period start date (for rental products)</param>
        /// <param name="rentalEndDate">Rental period end date (for rental products)</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscounts">Applied discounts</param>
        /// <param name="doneDiscounts">Discounts already applied somewhere else, used to calculate exact value discounts</param>
        /// <returns>Final price</returns>
        public virtual decimal GetFinalPrice(Product product,
            Customer customer,
            decimal additionalCharge,
            bool includeDiscounts,
            int quantity,
            DateTime? rentalStartDate,
            DateTime? rentalEndDate,
            out decimal discountAmount,
            out List<DiscountForCaching> appliedDiscounts,
            List<DiscountAppliedTemp> doneDiscounts = null)
        {
            return GetFinalPrice(product, customer, null, additionalCharge, includeDiscounts, quantity,
                rentalStartDate, rentalEndDate, out discountAmount, out appliedDiscounts, doneDiscounts);
        }

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="overriddenProductPrice">Overridden product price. If specified, then it'll be used instead of a product price. For example, used with product attribute combinations</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <param name="quantity">Shopping cart item quantity</param>
        /// <param name="rentalStartDate">Rental period start date (for rental products)</param>
        /// <param name="rentalEndDate">Rental period end date (for rental products)</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscounts">Applied discounts</param>
        /// <param name="doneDiscounts">Discounts already applied somewhere else, used to calculate exact value discounts</param>
        /// <returns>Final price</returns>
        public virtual decimal GetFinalPrice(Product product,
            Customer customer,
            decimal? overriddenProductPrice,
            decimal additionalCharge,
            bool includeDiscounts,
            int quantity,
            DateTime? rentalStartDate,
            DateTime? rentalEndDate,
            out decimal discountAmount,
            out List<DiscountForCaching> appliedDiscounts,
            List<DiscountAppliedTemp> doneDiscounts = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            discountAmount = decimal.Zero;
            appliedDiscounts = new List<DiscountForCaching>();

            var cacheKey = string.Format(PriceCacheEventConsumer.PRODUCT_PRICE_MODEL_KEY,
                product.Id,
                overriddenProductPrice.HasValue ? overriddenProductPrice.Value.ToString(CultureInfo.InvariantCulture) : null,
                additionalCharge.ToString(CultureInfo.InvariantCulture),
                includeDiscounts,
                quantity,
                string.Join(",", customer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            var cacheTime = _catalogSettings.CacheProductPrices ? 60 : 0;
            //we do not cache price for rental products
            //otherwise, it can cause memory leaks (to store all possible date period combinations)
            if (product.IsRental)
                cacheTime = 0;
            var cachedPrice = _cacheManager.Get(cacheKey, cacheTime, () =>
            {
                var result = new ProductPriceForCaching();

                //initial price
                var price = overriddenProductPrice.HasValue ? overriddenProductPrice.Value : product.Price;
                var sci = customer.ShoppingCartItems.Where(x => x.ProductId == product.Id && x.ShoppingCartType == ShoppingCartType.ShoppingCart).FirstOrDefault();

                if ((product.EquivalenceCoefficient > 0 || product.WeightInterval > 0) && sci != null)
                {
                    if (TeedCommerceStores.CurrentStore == TeedStores.CentralEnLinea)
                        price = _paymentService.CalculateGroceryPrice(product, sci, 0, false);
                    else
                        price = sci.CustomerEnteredPrice;
                }

                //tier prices
                var tierPrice = product.GetPreferredTierPrice(customer, _storeContext.CurrentStore.Id, quantity);
                if (tierPrice != null)
                    price = tierPrice.Price;

                //additional charge
                price = price + additionalCharge;

                //rental products
                if (product.IsRental)
                    if (rentalStartDate.HasValue && rentalEndDate.HasValue)
                        price = price * product.GetRentalPeriods(rentalStartDate.Value, rentalEndDate.Value);

                if (includeDiscounts)
                {
                    //discount
                    decimal priceForDiscount = 0;
                    if (product.EquivalenceCoefficient > 0 || product.WeightInterval > 0)
                        priceForDiscount = price;
                    else
                        priceForDiscount = price * (sci == null ? 1 : sci.Quantity);

                    var tmpDiscountAmount = GetDiscountAmount(product, customer, priceForDiscount, out List<DiscountForCaching> tmpAppliedDiscounts, doneDiscounts);
                    if (tmpDiscountAmount > priceForDiscount)
                        tmpDiscountAmount = priceForDiscount;

                    if (product.EquivalenceCoefficient > 0 || product.WeightInterval > 0)
                        price -= tmpDiscountAmount;
                    else
                        price -= tmpDiscountAmount / (sci == null ? 1 : sci.Quantity);

                    if (tmpAppliedDiscounts != null)
                    {
                        result.AppliedDiscounts = tmpAppliedDiscounts;
                        result.AppliedDiscountAmount = tmpDiscountAmount;
                    }
                }

                if (price < decimal.Zero)
                    price = decimal.Zero;

                result.Price = price;
                return result;
            });

            if (includeDiscounts)
            {
                if (cachedPrice.AppliedDiscounts.Any())
                {
                    appliedDiscounts.AddRange(cachedPrice.AppliedDiscounts);
                    discountAmount = cachedPrice.AppliedDiscountAmount;
                }
            }

            return cachedPrice.Price;
        }

        /// <summary>
        /// Gets the shopping cart unit price (one item)
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <returns>Shopping cart unit price (one item)</returns>
        public virtual decimal GetUnitPrice(ShoppingCartItem shoppingCartItem,
            bool includeDiscounts = true)
        {
            decimal discountAmount;
            List<DiscountForCaching> appliedDiscounts;
            return GetUnitPrice(shoppingCartItem, includeDiscounts,
                out discountAmount, out appliedDiscounts);
        }

        /// <summary>
        /// Gets the shopping cart unit price (one item)
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscounts">Applied discounts</param>
        /// <param name="doneDiscounts">Discounts already applied somewhere else, used to calculate exact value discounts</param>
        /// <returns>Shopping cart unit price (one item)</returns>
        public virtual decimal GetUnitPrice(ShoppingCartItem shoppingCartItem,
            bool includeDiscounts,
            out decimal discountAmount,
            out List<DiscountForCaching> appliedDiscounts,
            List<DiscountAppliedTemp> doneDiscounts = null)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException(nameof(shoppingCartItem));

            return GetUnitPrice(shoppingCartItem.Product,
                shoppingCartItem.Customer,
                shoppingCartItem.ShoppingCartType,
                shoppingCartItem.Quantity,
                shoppingCartItem.AttributesXml,
                shoppingCartItem.CustomerEnteredPrice,
                shoppingCartItem.RentalStartDateUtc,
                shoppingCartItem.RentalEndDateUtc,
                includeDiscounts,
                out discountAmount,
                out appliedDiscounts,
                doneDiscounts);
        }

        /// <summary>
        /// Gets the shopping cart unit price (one item)
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="attributesXml">Product attributes (XML format)</param>
        /// <param name="customerEnteredPrice">Customer entered price (if specified)</param>
        /// <param name="rentalStartDate">Rental start date (null for not rental products)</param>
        /// <param name="rentalEndDate">Rental end date (null for not rental products)</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscounts">Applied discounts</param>
        /// <param name="doneDiscounts">Discounts already applied somewhere else, used to calculate exact value discounts</param>
        /// <returns>Shopping cart unit price (one item)</returns>
        public virtual decimal GetUnitPrice(Product product,
            Customer customer,
            ShoppingCartType shoppingCartType,
            int quantity,
            string attributesXml,
            decimal customerEnteredPrice,
            DateTime? rentalStartDate, DateTime? rentalEndDate,
            bool includeDiscounts,
            out decimal discountAmount,
            out List<DiscountForCaching> appliedDiscounts,
            List<DiscountAppliedTemp> doneDiscounts = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            discountAmount = decimal.Zero;
            appliedDiscounts = new List<DiscountForCaching>();

            if (product.RewardPointsRequired > 0)
            {
                return product.RewardPointsRequired;
            }

            decimal finalPrice;

            var combination = _productAttributeParser.FindProductAttributeCombination(product, attributesXml);
            if (combination != null && combination.OverriddenPrice.HasValue)
            {
                finalPrice = GetFinalPrice(product,
                        customer,
                        combination.OverriddenPrice.Value,
                        decimal.Zero,
                        includeDiscounts,
                        quantity,
                        product.IsRental ? rentalStartDate : null,
                        product.IsRental ? rentalEndDate : null,
                        out discountAmount, out appliedDiscounts,
                        doneDiscounts);
            }
            else
            {
                //summarize price of all attributes
                var attributesTotalPrice = decimal.Zero;
                var attributeValues = _productAttributeParser.ParseProductAttributeValues(attributesXml);
                if (attributeValues != null)
                {
                    foreach (var attributeValue in attributeValues)
                    {
                        attributesTotalPrice += GetProductAttributeValuePriceAdjustment(attributeValue);
                    }
                }

                //get price of a product (with previously calculated price of all attributes)
                if (product.CustomerEntersPrice)
                {
                    finalPrice = customerEnteredPrice;
                }
                else
                {
                    int qty;
                    if (_shoppingCartSettings.GroupTierPricesForDistinctShoppingCartItems)
                    {
                        //the same products with distinct product attributes could be stored as distinct "ShoppingCartItem" records
                        //so let's find how many of the current products are in the cart
                        qty = customer.ShoppingCartItems
                            .Where(x => x.ProductId == product.Id && x.ShoppingCartType == ShoppingCartType.ShoppingCart)
                            .Where(x => x.ShoppingCartType == shoppingCartType)
                            .Sum(x => x.Quantity);
                        if (qty == 0)
                        {
                            qty = quantity;
                        }
                    }
                    else
                    {
                        qty = quantity;
                    }
                    finalPrice = GetFinalPrice(product,
                        customer,
                        attributesTotalPrice,
                        includeDiscounts,
                        qty,
                        product.IsRental ? rentalStartDate : null,
                        product.IsRental ? rentalEndDate : null,
                        out discountAmount, out appliedDiscounts,
                        doneDiscounts);
                }
            }

            //rounding
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                finalPrice = RoundingHelper.RoundPrice(finalPrice);

            return finalPrice;
        }

        /// <summary>
        /// Gets the shopping cart item sub total
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <returns>Shopping cart item sub total</returns>
        public virtual decimal GetSubTotal(ShoppingCartItem shoppingCartItem,
            bool includeDiscounts = true)
        {
            return GetSubTotal(shoppingCartItem, includeDiscounts, out decimal _, out List<DiscountForCaching> _, out int? _, out decimal _);
        }

        /// <summary>
        /// Gets the shopping cart item sub total
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscounts">Applied discounts</param>
        /// <param name="maximumDiscountQty">Maximum discounted qty. Return not nullable value if discount cannot be applied to ALL items</param>
        /// <param name="rewardPointsRequired">Reward Points Required</param>
        /// <param name="doneDiscounts">Discounts already applied somewhere else, used to calculate exact value discounts</param>
        /// <param name="specialPromotionItems">Special Promotion Items to override pricing for given elements</param>
        /// <returns>Shopping cart item sub total</returns>
        public virtual decimal GetSubTotal(ShoppingCartItem shoppingCartItem,
            bool includeDiscounts,
            out decimal discountAmount,
            out List<DiscountForCaching> appliedDiscounts,
            out int? maximumDiscountQty,
            out decimal rewardPointsRequired,
            List<DiscountAppliedTemp> doneDiscounts = null,
            List<SpecialPromotionItem> specialPromotionItems = null)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException(nameof(shoppingCartItem));

            decimal subTotal = 0;
            appliedDiscounts = new List<DiscountForCaching>();
            maximumDiscountQty = null;

            rewardPointsRequired = decimal.Zero;

            if (shoppingCartItem.Product.RewardPointsRequired > 0)
            {
                rewardPointsRequired = shoppingCartItem.Product.RewardPointsRequired * shoppingCartItem.Quantity;
            }

            //unit price
            var unitPrice = GetUnitPrice(shoppingCartItem, includeDiscounts,
                out discountAmount, out appliedDiscounts, doneDiscounts);

            var redoDiscounts = false;
            //discount
            if (appliedDiscounts.Any())
            {
                //if (unitPrice <= 0)
                //{
                //    redoDiscounts = true;
                //    unitPrice = GetUnitPrice(shoppingCartItem, false,
                //        out discountAmount, out appliedDiscounts, doneDiscounts);

                //    subTotal = unitPrice * shoppingCartItem.Quantity;
                //    //discount
                //    var tmpDiscountAmount = GetDiscountAmount(shoppingCartItem.Product, shoppingCartItem.Customer, subTotal, out List<DiscountForCaching> tmpAppliedDiscounts, doneDiscounts);
                //    if (tmpDiscountAmount > subTotal)
                //        tmpDiscountAmount = subTotal;
                //    subTotal -= tmpDiscountAmount;

                //    if (tmpAppliedDiscounts != null)
                //    {
                //        appliedDiscounts = tmpAppliedDiscounts;
                //        discountAmount = tmpDiscountAmount;
                //    }
                //}

                //we can properly use "MaximumDiscountedQuantity" property only for one discount (not cumulative ones)
                DiscountForCaching oneAndOnlyDiscount = null;
                if (appliedDiscounts.Count == 1)
                    oneAndOnlyDiscount = appliedDiscounts.First();

                if (oneAndOnlyDiscount != null &&
                    oneAndOnlyDiscount.MaximumDiscountedQuantity.HasValue &&
                    shoppingCartItem.Quantity > oneAndOnlyDiscount.MaximumDiscountedQuantity.Value)
                {
                    maximumDiscountQty = oneAndOnlyDiscount.MaximumDiscountedQuantity.Value;
                    //we cannot apply discount for all shopping cart items
                    var discountedQuantity = oneAndOnlyDiscount.MaximumDiscountedQuantity.Value;
                    var discountedSubTotal = unitPrice * discountedQuantity;
                    discountAmount = discountAmount * discountedQuantity;

                    var notDiscountedQuantity = shoppingCartItem.Quantity - discountedQuantity;
                    var notDiscountedUnitPrice = GetUnitPrice(shoppingCartItem, false);
                    var notDiscountedSubTotal = notDiscountedUnitPrice * notDiscountedQuantity;

                    subTotal = discountedSubTotal + notDiscountedSubTotal;
                }
                else
                {
                    if (TeedCommerceStores.CurrentStore == TeedStores.CentralEnLinea)
                    {
                        if (!redoDiscounts && (shoppingCartItem.Product.WeightInterval > 0 || shoppingCartItem.Product.EquivalenceCoefficient > 0))
                            subTotal = unitPrice;
                        else if (!redoDiscounts)
                        {
                            subTotal = unitPrice * shoppingCartItem.Quantity;
                        }
                    }
                    else
                    {
                        //discount is applied to all items (quantity)
                        //calculate discount amount for all items
                        //discountAmount = discountAmount * shoppingCartItem.Quantity;
                        subTotal = unitPrice * shoppingCartItem.Quantity;

                        //recheck discount for products where unit price gets all discounts applied
                        if (unitPrice <= 0)
                        {
                            unitPrice = GetUnitPrice(shoppingCartItem, false);
                            var totalPrice = unitPrice * shoppingCartItem.Quantity;
                            var allowedDiscounts = GetAllowedDiscounts(shoppingCartItem.Product, shoppingCartItem.Customer);
                            _ = allowedDiscounts.GetPreferredDiscount(totalPrice, out decimal appliedDiscountAmount, doneDiscounts);
                            if (appliedDiscountAmount > totalPrice)
                                appliedDiscountAmount = totalPrice;
                            discountAmount = appliedDiscountAmount;
                            subTotal = totalPrice - appliedDiscountAmount;
                        }
                    }
                }
            }
            else
            {
                if ((shoppingCartItem.Product.EquivalenceCoefficient > 0 || shoppingCartItem.Product.WeightInterval > 0) && TeedCommerceStores.CurrentStore == TeedStores.CentralEnLinea)
                {
                    subTotal = unitPrice;
                }
                else
                {
                    subTotal = unitPrice * shoppingCartItem.Quantity;
                }

            }

            if (specialPromotionItems != null)
            {
                var specialPromotions = specialPromotionItems.Where(x => x.ProductId == shoppingCartItem.ProductId &&
                x.AttributesXml == shoppingCartItem.AttributesXml).ToList();
                foreach (var specialPromotion in specialPromotions)
                    subTotal -= specialPromotion.Price;
            }

            return subTotal;
        }

        /// <summary>
        /// Gets the product cost (one item)
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Shopping cart item attributes in XML</param>
        /// <returns>Product cost (one item)</returns>
        public virtual decimal GetProductCost(Product product, string attributesXml)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var cost = product.ProductCost;
            var attributeValues = _productAttributeParser.ParseProductAttributeValues(attributesXml);
            foreach (var attributeValue in attributeValues)
            {
                switch (attributeValue.AttributeValueType)
                {
                    case AttributeValueType.Simple:
                        {
                            //simple attribute
                            cost += attributeValue.Cost;
                        }
                        break;
                    case AttributeValueType.AssociatedToProduct:
                        {
                            //bundled product
                            var associatedProduct = _productService.GetProductById(attributeValue.AssociatedProductId);
                            if (associatedProduct != null)
                                cost += associatedProduct.ProductCost * attributeValue.Quantity;
                        }
                        break;
                    default:
                        break;
                }
            }

            return cost;
        }

        /// <summary>
        /// Get a price adjustment of a product attribute value
        /// </summary>
        /// <param name="value">Product attribute value</param>
        /// <returns>Price adjustment</returns>
        public virtual decimal GetProductAttributeValuePriceAdjustment(ProductAttributeValue value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var adjustment = decimal.Zero;
            switch (value.AttributeValueType)
            {
                case AttributeValueType.Simple:
                    {
                        //simple attribute
                        adjustment = value.PriceAdjustment;
                    }
                    break;
                case AttributeValueType.AssociatedToProduct:
                    {
                        //bundled product
                        var associatedProduct = _productService.GetProductById(value.AssociatedProductId);
                        if (associatedProduct != null)
                        {
                            adjustment = GetFinalPrice(associatedProduct, _workContext.CurrentCustomer, includeDiscounts: true) * value.Quantity;
                        }
                    }
                    break;
                default:
                    break;
            }

            return adjustment;
        }

        #endregion
    }
}
