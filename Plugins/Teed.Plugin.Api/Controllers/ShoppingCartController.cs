using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Models.ShoppingCart;
using Nop.Web.Factories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Cart;
using Teed.Plugin.Api.Dtos.Products;
using Teed.Plugin.Api.Dtos.ShoppingCart;
using Teed.Plugin.Api.Helper;
using Teed.Plugin.Api.Services;
using Teed.Plugin.Api.Utils;

namespace Teed.Plugin.Api.Controllers
{
    public class ShoppingCartController : ApiBaseController
    {
        #region Fields

        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly IAddressService _addressService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IShippingService _shippingService;
        private readonly IWorkContext _workContext;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly IOrderService _orderService;
        private readonly CotizadorEstafetaService _cotizadorEstafetaService;
        private readonly ProductAttributeConverter _productAttributeConverter;
        private readonly TaxSettings _taxSettings;
        private readonly OrderSettings _orderSettings;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IDiscountService _discountService;

        private static Random rng = new Random();

        #endregion

        #region Ctor

        public ShoppingCartController(IShoppingCartService shoppingCartService,
            ICustomerService customerService,
            ICurrencyService currencyService,
            IPictureService pictureService,
            IShippingService shippingService,
            IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IStoreContext storeContext,
            IAddressService addressService,
            IProductService productService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            IPaymentService paymentService,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService,
            IOrderService orderService,
            ProductAttributeConverter productAttributeConverter,
            CotizadorEstafetaService cotizadorEstafetaService,
            TaxSettings taxSettings,
            OrderSettings orderSettings,
            IOrderProcessingService orderProcessingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPriceFormatter priceFormatter,
            ShoppingCartSettings shoppingCartSettings,
            IDiscountService discountService)
        {
            _shoppingCartService = shoppingCartService;
            _customerService = customerService;
            _productService = productService;
            _productAttributeConverter = productAttributeConverter;
            _cotizadorEstafetaService = cotizadorEstafetaService;
            _pictureService = pictureService;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _storeContext = storeContext;
            _shippingService = shippingService;
            _workContext = workContext;
            _addressService = addressService;
            _storeService = storeService;
            _settingService = settingService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _paymentService = paymentService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _taxService = taxService;
            _priceCalculationService = priceCalculationService;
            _currencyService = currencyService;
            _orderService = orderService;
            _taxSettings = taxSettings;
            _orderProcessingService = orderProcessingService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _orderSettings = orderSettings;
            _priceFormatter = priceFormatter;
            _shoppingCartSettings = shoppingCartSettings;
            _discountService = discountService;
        }

        #endregion

        #region Methods

        public virtual void CheckDiscountRemoveByCartItems(ShoppingCartItem sci, Customer customer)
        {
            if (!string.IsNullOrEmpty(sci.AppliedByCodeByCoupon))
            {
                var cart = customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .ToList();
                var cartWithoutCurrentItem = cart.Where(x => x.Id != sci.Id &&
                    x.AppliedByCodeByCoupon == sci.AppliedByCodeByCoupon.ToLower() &&
                    x.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
                if (!cartWithoutCurrentItem.Any())
                    customer.RemoveDiscountCouponCode(sci.AppliedByCodeByCoupon);
            }
        }

        public virtual void AddCouponCodeToCartItem(ShoppingCartItem sci, Customer customer)
        {
            var couponCodes = customer.ParseAppliedDiscountCouponCodes();
            foreach (var couponCode in couponCodes)
            {
                var discount = _discountService.GetAllDiscounts(null, couponCode).FirstOrDefault();
                if (discount != null)
                {
                    if (discount.AppliedToProducts.Any())
                    {
                        var productOfDiscount = discount.AppliedToProducts
                            .Where(x => x.Published && !x.Deleted && x.Id == sci.ProductId)
                            .FirstOrDefault();
                        if (productOfDiscount != null)
                        {
                            sci.AppliedByCodeByCoupon = couponCode.ToLower();
                            _customerService.UpdateCustomer(customer);
                            break;
                        }
                    }
                }
            }
        }

        [HttpPost]
        public IActionResult AddProductToCart([FromBody] UpdateShoppingCartDto dto)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (dto == null) return BadRequest();

            int.TryParse(UserId, out int userId);
            Customer customer = _customerService.GetCustomerById(userId);
            if (customer == null) return Unauthorized();

            Product product = _productService.GetProductById(dto.ProductId);
            if (product == null) return NotFound();

            var addToCartWarnings = AddNewProductToCart(customer, product, dto.ProductAttributeIds, dto.BuyingBySecondary, dto.SelectedPropertyOption);

            if (addToCartWarnings.Any())
                return BadRequest(addToCartWarnings.FirstOrDefault());

            return NoContent();
        }

        private IList<string> AddNewProductToCart(Customer customer, Product product, int[] productAttributeIds, bool buyingBySecondary, string selectedPropertyOption)
        {
            //get standard warnings without attribute validations
            //first, try to find existing shopping cart item
            var cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart, ShoppingCartType.ShoppingCart, product);

            //if we already have the same product in the cart, then use the total quantity to validate
            var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + 1 : 1;
            var addToCartWarnings = _shoppingCartService
                .GetShoppingCartItemWarnings(customer, ShoppingCartType.ShoppingCart,
                product, _storeContext.CurrentStore.Id, string.Empty,
                decimal.Zero, null, null, quantityToValidate, false, true, false, false, false);

            if (addToCartWarnings.Any()) return addToCartWarnings;

            decimal customerEnteredPrice = _paymentService.CalculateGroceryPrice(product, shoppingCartItem, 1, false); //calculate price
            //now let's try adding product to the cart (now including product attribute validation, etc)
            addToCartWarnings = _shoppingCartService.AddToCart(customer: customer,
                product: product,
                shoppingCartType: ShoppingCartType.ShoppingCart,
                storeId: _storeContext.CurrentStore.Id,
                attributesXml: _productAttributeConverter.ConvertToXml(productAttributeIds, product.Id),
                quantity: 1, buyingBySecondary: buyingBySecondary, selectedPropertyOption: selectedPropertyOption, customerEnteredPrice: customerEnteredPrice);
            cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart, ShoppingCartType.ShoppingCart, product);
            AddCouponCodeToCartItem(shoppingCartItem, customer);

            _customerActivityService.InsertActivity("PublicStore.AddToShoppingCart", _localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart") + " Desde la app.", product.Name);

            return new List<string>();
        }

        /// DEPRECATED 20-12-2020
        [HttpPost]
        public IActionResult UpdateShoppingCart([FromBody] UpdateShoppingCartDto dto)
        {
            Customer customer = _customerService.GetCustomerById(int.Parse(UserId));
            if (customer == null) return NotFound();

            Product product = _productService.GetProductById(dto.ProductId);
            if (product == null) return NotFound();

            var sci = customer.ShoppingCartItems.Where(x => x.ProductId == dto.ProductId).FirstOrDefault();
            if (sci == null)
            {
                var warnings = AddNewProductToCart(customer, product, dto.ProductAttributeIds, dto.BuyingBySecondary, dto.SelectedPropertyOption);
                if (warnings.Any()) return BadRequest(warnings.FirstOrDefault());
                sci = customer.ShoppingCartItems.Where(x => x.ProductId == dto.ProductId).FirstOrDefault();
            }

            if (dto.NewQuantity <= 0)
            {
                CheckDiscountRemoveByCartItems(sci, customer);
                _shoppingCartService.DeleteShoppingCartItem(sci, ensureOnlyActiveCheckoutAttributes: true);
            }
            else
            {
                bool isRemoving = sci.Quantity > dto.NewQuantity;
                decimal customerEnteredPrice = _paymentService.CalculateGroceryPrice(sci.Product, sci, dto.NewQuantity, isRemoving); //calculate price

                _shoppingCartService.UpdateShoppingCartItem(customer,
                                        sci.Id, sci.AttributesXml, customerEnteredPrice,
                                        sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                        dto.NewQuantity, true, dto.BuyingBySecondary, dto.SelectedPropertyOption);
            }

            return NoContent();
        }

        [HttpPost]
        public IActionResult UpdateShoppingCartItems([FromBody] List<UpdateShoppingCartDto> dtos)
        {
            Customer customer = _customerService.GetCustomerById(int.Parse(UserId));
            if (customer == null) return NotFound();

            foreach (var dto in dtos)
            {
                Product product = _productService.GetProductById(dto.ProductId);
                if (product == null) return NotFound();

                var sci = customer.ShoppingCartItems.Where(x => x.ProductId == dto.ProductId && x.ShoppingCartType == ShoppingCartType.ShoppingCart).FirstOrDefault();
                if (sci == null)
                {
                    var warnings = AddNewProductToCart(customer, product, dto.ProductAttributeIds, dto.BuyingBySecondary, dto.SelectedPropertyOption);
                    if (warnings.Any()) return BadRequest(warnings.FirstOrDefault());
                    sci = customer.ShoppingCartItems.Where(x => x.ProductId == dto.ProductId && x.ShoppingCartType == ShoppingCartType.ShoppingCart).FirstOrDefault();
                }

                if (dto.NewQuantity <= 0)
                {
                    CheckDiscountRemoveByCartItems(sci, customer);
                    _shoppingCartService.DeleteShoppingCartItem(sci, ensureOnlyActiveCheckoutAttributes: true);
                }
                else
                {
                    bool isRemoving = sci.Quantity > dto.NewQuantity;
                    decimal customerEnteredPrice = _paymentService.CalculateGroceryPrice(sci.Product, sci, dto.NewQuantity, isRemoving); //calculate price

                    _shoppingCartService.UpdateShoppingCartItem(customer,
                                            sci.Id, sci.AttributesXml, customerEnteredPrice,
                                            sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                            dto.NewQuantity, true, dto.BuyingBySecondary, dto.SelectedPropertyOption);
                }
            }

            return NoContent();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetProductStock(int productId, string attributes)
        {
            Product product = _productService.GetProductById(productId);
            if (product == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(attributes))
            {
                int[] attributeIds = Array.ConvertAll(attributes.Split(','), int.Parse);
                string attributesXml = _productAttributeConverter.ConvertToXml(attributeIds, product.Id);
                ProductAttributeCombination combination = _productAttributeParser.FindProductAttributeCombination(product, attributesXml);

                return Ok(combination != null ? combination.StockQuantity : product.StockQuantity);
            }

            return Ok(product.StockQuantity);
        }

        [HttpGet]
        public IActionResult GetShoppingCartData()
        {
            int userId = int.Parse(UserId);
            Customer customer = _customerService.GetCustomerById(userId);
            if (customer == null) return NotFound();

            List<ShoppingCartDataDto> dto = customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(x => new ShoppingCartDataDto()
            {
                ProductId = x.ProductId,
                BuyingBySecondary = x.BuyingBySecondary,
                Quantity = x.Quantity,
                SelectedPropertyOption = x.SelectedPropertyOption,
                PictureUrl = "/Product/ProductImage?id=" + x.ProductId,
            }).ToList();

            return Ok(dto);
        }

        [HttpGet]
        public IActionResult GetExtraCartProducts()
        {
            Customer customer = _customerService.GetCustomerById(int.Parse(UserId));
            if (customer == null) return NotFound();

            var unpublishedExtraProduct = customer.ShoppingCartItems.Where(x => !x.Product.Published && x.Product.IsExtraCartProduct && x.ShoppingCartType == ShoppingCartType.ShoppingCart);
            if (unpublishedExtraProduct.Count() > 0)
            {
                foreach (var unpublishedItem in unpublishedExtraProduct)
                {
                    CheckDiscountRemoveByCartItems(unpublishedItem, customer);
                    _shoppingCartService.DeleteShoppingCartItem(unpublishedItem, ensureOnlyActiveCheckoutAttributes: true);
                }
            }

            var extraCartProducts = _productService.GetAllProductsQuery().Where(x => x.Published && x.IsExtraCartProduct);
            if (extraCartProducts.Count() == 0) return NoContent();

            List<ProductDto> dto = extraCartProducts.ToList().Select(x => ProductHelper.GetProductDto(x, customer, _priceCalculationService)).ToList();

            var currentExtraProductsInCart = customer.ShoppingCartItems.Where(x => x.Product.IsExtraCartProduct && x.ShoppingCartType == ShoppingCartType.ShoppingCart);
            if (currentExtraProductsInCart.Count() > 0)
            {
                var ids = currentExtraProductsInCart.Select(x => x.ProductId);
                foreach (var item in dto.Where(x => ids.Contains(x.Id)))
                {
                    var sci = currentExtraProductsInCart.Where(x => x.ProductId == item.Id).FirstOrDefault();
                    item.SubTotal = _taxService.GetProductPrice(sci.Product, _priceCalculationService.GetSubTotal(sci, true, out decimal shoppingCartItemDiscountBase, out List<DiscountForCaching> _, out int? maximumDiscountQty, out decimal rewadPointsRequired), out decimal taxRate);
                    item.UnitPrice = _taxService.GetProductPrice(sci.Product, _priceCalculationService.GetUnitPrice(sci), out decimal _);
                    item.CartItemId = sci.Id;
                    item.CurrentCartQuantity = sci.Quantity;
                    item.SelectedPropertyOption = sci.SelectedPropertyOption;
                    item.Warnings = _shoppingCartService.GetShoppingCartItemWarnings(
                            customer,
                            sci.ShoppingCartType,
                            sci.Product,
                            sci.StoreId,
                            sci.AttributesXml,
                            sci.CustomerEnteredPrice,
                            sci.RentalStartDateUtc,
                            sci.RentalEndDateUtc,
                            sci.Quantity,
                            false);
                }
            }

            return Ok(dto);
        }

        [HttpGet]
        public IActionResult GetProductsQty()
        {
            int userId = int.Parse(UserId);
            Customer customer = _customerService.GetCustomerById(userId);
            if (customer == null) return NotFound();

            List<int> productIds = customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(x => x.ProductId).ToList();
            return Ok(productIds);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetComplementaryProducts(string productsInCart)
        {
            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var homePageSettings = _settingService.LoadSetting<TeedApiPluginSettings>(storeScope);

            IEnumerable<Product> products;
            if (!string.IsNullOrWhiteSpace(productsInCart))
            {
                int[] productsInCartIds = Array.ConvertAll(productsInCart.Split(','), int.Parse);
                products = _productService.SearchProducts(categoryIds: new int[] { homePageSettings.CategoryId }).Where(x => !productsInCartIds.Contains(x.Id)).OrderBy(x => rng.Next()).Take(3);
            }
            else
            {
                products = _productService.SearchProducts(categoryIds: new int[] { homePageSettings.CategoryId }).OrderBy(x => rng.Next()).Take(3);
            }

            var dto = products.Select(x => new GetComplementProductsDto()
            {
                Id = x.Id,
                Name = x.Name,
                ShortDescription = x.ShortDescription,
                Price = x.Price,
                OldPrice = x.OldPrice,
                Weight = x.Weight,
                ProductStockQuantity = x.StockQuantity,
                WarehouseId = x.WarehouseId,
                FullDescription = x.FullDescription,
                ProductCategories = x.ProductCategories.Select(c => new Dtos.Products.ProductCategory()
                {
                    Id = c.Category.Id,
                    Name = c.Category.Name
                }).ToList(),
                Pictures = _pictureService.GetPicturesByProductId(x.Id).Count == 0 ? new List<string>() { _pictureService.GetDefaultPictureUrl() } : _pictureService.GetPicturesByProductId(x.Id).Select(y => _pictureService.GetPictureUrl(y.Id)).ToList(),
                ProductAttributes = x.ProductAttributeMappings.Select(y => new Dtos.Products.ProductAttribute()
                {
                    AttributeId = y.ProductAttributeId,
                    AttributeName = y.TextPrompt,
                    ProductAttributeValues = y.ProductAttributeValues.Select(z => new Dtos.Products.ProductAttributeValue()
                    {
                        ProductAttributeValueId = z.Id,
                        ProductAttributeValueName = z.Name
                    }).ToList()
                }).ToList()
            });

            return Ok(dto);
        }

        [HttpPost]
        public IActionResult UpdateWebShoppingCart([FromBody] UpdateWebShoppingCartDto dto)
        {
            Customer customer = _customerService.GetCustomerById(dto.CustomerId);
            if (customer == null) return NotFound();

            foreach (var item in dto.ShoppingCartProducts)
            {
                var product = _productService.GetProductById(item.Id);
                if (product.OrderMinimumQuantity <= item.Quantity)
                {
                    //get standard warnings without attribute validations
                    //first, try to find existing shopping cart item
                    var cart = customer.ShoppingCartItems
                        .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                        .LimitPerStore(_storeContext.CurrentStore.Id)
                        .ToList();
                    var shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart, ShoppingCartType.ShoppingCart, product);
                    //if we already have the same product in the cart, then use the total quantity to validate
                    var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + item.Quantity : item.Quantity;
                    var addToCartWarnings = _shoppingCartService
                        .GetShoppingCartItemWarnings(customer, ShoppingCartType.ShoppingCart,
                        product, _storeContext.CurrentStore.Id, string.Empty,
                        decimal.Zero, null, null, quantityToValidate, false, true, false, false, false);
                    if (addToCartWarnings.Any())
                    {
                        return BadRequest(addToCartWarnings.FirstOrDefault());
                    }

                    //now let's try adding product to the cart (now including product attribute validation, etc)
                    var attributeIds = new List<int>();
                    foreach (var att in item.SelectedAttributes)
                    {
                        attributeIds.Add(att.AttributeValueId);
                    }

                    addToCartWarnings = _shoppingCartService.AddToCart(customer: customer,
                        product: product,
                        shoppingCartType: ShoppingCartType.ShoppingCart,
                        storeId: _storeContext.CurrentStore.Id,
                        attributesXml: _productAttributeConverter.ConvertToXml(attributeIds.ToArray(), product.Id),
                        quantity: item.Quantity);
                    if (addToCartWarnings.Any())
                    {
                        return BadRequest("No fue posible sincronizar el carrito");
                    }
                }
                else
                {
                    return BadRequest("No fue posible agregar el producto al carrito");
                }
            }

            return NoContent();
        }

        [HttpPost]
        public IActionResult UpdateCartQuantity([FromBody] RemoveFromCartDto dto)
        {
            Customer customer = _customerService.GetCustomerById(int.Parse(dto.CustomerId));
            if (customer == null) return NotFound();

            Product product = _productService.GetProductById(dto.ProductId);
            if (product == null) return NotFound();

            string attXml = _productAttributeConverter.ConvertToXml(dto.ProductAttributeIds, product.Id);
            var shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(customer.ShoppingCartItems.ToList(), ShoppingCartType.ShoppingCart, product, attXml);
            var warnings = _shoppingCartService.UpdateShoppingCartItem(customer, shoppingCartItem.Id, shoppingCartItem.AttributesXml, 0, quantity: dto.NewQuantity);

            if (warnings.Any())
            {
                return BadRequest(warnings.FirstOrDefault());
            }

            return NoContent();
        }

        [HttpGet]
        public IActionResult GetLoggedUserCart()
        {
            Customer customer = _customerService.GetCustomerById(int.Parse(UserId));
            if (customer == null) return NotFound("No se encontró al cliente");

            _shoppingCartService.AddGiftProduct(customer, 1);

            //var model = new Nop.Web.Models.ShoppingCart.ShoppingCartModel();
            //model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, customer.ShoppingCartItems.ToList(),
            //    isEditable: false);

            var dto = customer.ShoppingCartItems
                .Where(x => !x.Product.IsExtraCartProduct && x.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .Select(x => new ProductDto()
                {
                    Id = x.ProductId,
                    CartItemId = x.Id,
                    BuyingBySecondary = x.BuyingBySecondary,
                    EquivalenceCoefficient = x.Product.EquivalenceCoefficient,
                    PictureUrl = "/Product/ProductImage?id=" + x.ProductId,
                    ItemDiscount = GetItemDiscount(x),
                    Name = x.Product.Name,
                    CurrentCartQuantity = x.Quantity,
                    SelectedPropertyOption = x.SelectedPropertyOption,
                    Sku = x.Product.Sku,
                    Price = x.Product.Price,
                    GiftProductEnable = x.Product.GiftProductEnable,
                    //AppliedByCodeByCoupon = x.AppliedByCodeByCoupon,
                    OldPrice = x.Product.OldPrice > x.Product.Price ? x.Product.OldPrice : 0,
                    PropertyOptions = x.Product.PropertiesOptions?.Split(',').Select(y => y.ToUpper().First() + y.ToLower().Substring(1)).ToArray(),
                    SubTotal = _taxService.GetProductPrice(x.Product, _priceCalculationService.GetSubTotal(x, true, out decimal shoppingCartItemDiscountBase, out List<DiscountForCaching> _, out int? maximumDiscountQty, out decimal rewadPointsRequired), out decimal taxRate),
                    UnitPrice = _taxService.GetProductPrice(x.Product, _priceCalculationService.GetUnitPrice(x), out decimal _),
                    Warnings = _shoppingCartService.GetShoppingCartItemWarnings(
                            customer,
                            x.ShoppingCartType,
                            x.Product,
                            x.StoreId,
                            x.AttributesXml,
                            x.CustomerEnteredPrice,
                            x.RentalStartDateUtc,
                            x.RentalEndDateUtc,
                            x.Quantity,
                            false),
                    WeightInterval = ProductHelper.GetWeightInterval(x.Product),
                    IsInWishlist = customer.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.Wishlist).Any(),
                    Stock = x.Product.ManageInventoryMethod == ManageInventoryMethod.DontManageStock ? 1000 : x.Product.StockQuantity
                }).ToList();

            return Ok(dto);
        }

        [HttpPut]
        public IActionResult DeleteShopingCart()
        {
            int userId = int.Parse(UserId);
            Customer customer = _customerService.GetCustomerById(userId);
            if (customer == null) return NotFound();

            var cart = customer.ShoppingCartItems.Where(x => x.ShoppingCartTypeId == 1).ToList();
            foreach (var item in cart)
            {
                CheckDiscountRemoveByCartItems(item, customer);
                _shoppingCartService.DeleteShoppingCartItem(item, ensureOnlyActiveCheckoutAttributes: true);
            }

            return NoContent();
        }

        private decimal GetItemDiscount(ShoppingCartItem sci)
        {
            _taxService.GetProductPrice(sci.Product, _priceCalculationService.GetSubTotal(sci, true, out decimal shoppingCartItemDiscountBase, out List<DiscountForCaching> _, out int? maximumDiscountQty, out decimal rewadPointsRequired), out decimal taxRate);
            if (shoppingCartItemDiscountBase > decimal.Zero)
            {
                shoppingCartItemDiscountBase = _taxService.GetProductPrice(sci.Product, shoppingCartItemDiscountBase, out taxRate);
                if (shoppingCartItemDiscountBase > decimal.Zero)
                {
                    return _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, _workContext.WorkingCurrency);
                }
            }
            return 0;
        }

        //[HttpGet]
        //public IActionResult GetLoggedUserCart(string customerId)
        //{
        //    Customer customer = _customerService.GetCustomerById(int.Parse(customerId));
        //    if (customer == null) return NotFound("No se encontró al cliente");

        //    var dto = customer.ShoppingCartItems.Select(x => new GetLoggedUserCartDto()
        //    {
        //        Id = x.Product.Id,
        //        Name = x.Product.Name,
        //        ShortDescription = x.Product.ShortDescription,
        //        Price = x.Product.Price,
        //        Weight = x.Product.Weight,
        //        Quantity = x.Quantity,
        //        ProductStockQuantity = x.Product.StockQuantity,
        //        WarehouseId = x.Product.WarehouseId,
        //        FullDescription = x.Product.FullDescription,
        //        SelectedAttributes = _productAttributeParser.ParseProductAttributeMappings(x.AttributesXml).Select(a => new Dtos.Cart.SelectedAttribute()
        //        {
        //            AttributeName = a.ProductAttribute.Name,
        //            AttributeId = a.ProductAttribute.Id,
        //            AttributeValueId = _productAttributeParser.ParseProductAttributeValues(x.AttributesXml, a.Id).FirstOrDefault().Id,
        //            AttributeValueName = _productAttributeParser.ParseProductAttributeValues(x.AttributesXml, a.Id).FirstOrDefault().Name,
        //        }).ToList(),
        //        ProductCategories = x.Product.ProductCategories.Select(c => new Dtos.Products.ProductCategory()
        //        {
        //            Id = c.Category.Id,
        //            Name = c.Category.Name
        //        }).ToList(),
        //        Pictures = _pictureService.GetPicturesByProductId(x.Product.Id).Count == 0 ? new List<string>() { _pictureService.GetDefaultPictureUrl() } : _pictureService.GetPicturesByProductId(x.Product.Id).Select(y => _pictureService.GetPictureUrl(y.Id)).ToList(),
        //        ProductAttributes = x.Product.ProductAttributeMappings.Select(y => new Dtos.Products.ProductAttribute()
        //        {
        //            AttributeId = y.ProductAttributeId,
        //            AttributeName = y.TextPrompt,
        //            ProductAttributeValues = y.ProductAttributeValues.Select(z => new Dtos.Products.ProductAttributeValue()
        //            {
        //                ProductAttributeValueId = z.Id,
        //                ProductAttributeValueName = z.Name
        //            }).ToList()
        //        }).ToList()
        //    }).ToList();

        //    return Ok(dto);
        //}

        [HttpPost]
        public IActionResult CalculateShipping([FromBody] CalculateShippingDto dto)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (dto == null) return BadRequest();

            Customer customer = _customerService.GetCustomerById(int.Parse(dto.CustomerId));
            var cart = new List<ShoppingCartItem>();

            foreach (var item in dto.Items)
            {
                Product product = _productService.GetProductById(item.Id);
                string attXml = _productAttributeConverter.ConvertToXml(item.SelectedAttributes.Select(x => x.AttributeValueId).ToArray(), item.Id);
                var shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(customer.ShoppingCartItems.ToList(), ShoppingCartType.ShoppingCart, product, attXml);
                cart.Add(shoppingCartItem);
            }

            var shippingAddress = _addressService.GetAddressById(dto.ShippingAddress.Id);
            var getShippingOptionResponse = _shippingService.GetShippingOptions(cart, shippingAddress, customer, storeId: _storeContext.CurrentStore.Id);
            var optionResponse = getShippingOptionResponse.ShippingOptions.FirstOrDefault();

            var shippingResponse = new ShippingResponseDto()
            {
                Name = optionResponse.Name,
                Cost = optionResponse.Rate,
                ComputationMethod = optionResponse.ShippingRateComputationMethodSystemName
            };

            //var useEstafeta = true;
            //if (useEstafeta)
            //{
            //    var response = _cotizadorEstafetaService.CreateRateRequest(dto).Select(x => x.TipoServicio);
            //    foreach (var resp in response)
            //    {
            //        if (resp != null)
            //        {
            //            var shippings = resp.Where(x => x.DescripcionServicio == "Terrestre").Select(x => x.CostoTotal);
            //            foreach (var shipping in shippings)
            //            {
            //                cost = cost + shipping;
            //            }
            //        }
            //    }
            //}

            return Ok(shippingResponse);
        }

        [HttpGet]
        public IActionResult GetCrossSellingProducts()
        {
            Customer customer = _customerService.GetCustomerById(int.Parse(UserId));
            IEnumerable<ShoppingCartItem> currentCart = customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart);
            List<int> currentCartProductIds = currentCart.Select(x => x.ProductId).ToList();
            List<int> crossSellProductIds = _orderService.SearchOrders(customerId: customer.Id)
                .OrderByDescending(x => x.CreatedOnUtc)
                .Take(10)
                .SelectMany(x => x.OrderItems)
                .Where(x => !x.Product.GiftProductEnable && x.Product.Published)
                .Select(x => x.ProductId)
                .Where(x => !currentCartProductIds.Contains(x))
                .GroupBy(x => x)
                .OrderByDescending(x => x.Count())
                .Take(10)
                .Select(x => x.Key)
                .ToList();
            List<Product> products = _productService.GetAllProductsQuery().Where(x => x.Published && crossSellProductIds.Contains(x.Id)).ToList();
            var dto = products.Select(x => new ProductDto()
            {
                Id = x.Id,
                Sku = x.Sku,
                Name = x.Name,
                OldPrice = x.OldPrice > x.Price ? x.OldPrice : 0,
                Price = x.Price,
                Discount = _priceCalculationService.GetDiscountAmount(x, customer ?? new Customer(), x.Price, out List<DiscountForCaching> appliedDiscounts),
                PictureUrl = "/Product/ProductImage?id=" + x.Id,
                EquivalenceCoefficient = x.EquivalenceCoefficient,
                WeightInterval = ProductHelper.GetWeightInterval(x),
                CurrentCartQuantity = customer != null ? customer.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.Quantity).FirstOrDefault() : 0,
                BuyingBySecondary = customer != null && customer.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.BuyingBySecondary).FirstOrDefault(),
                SelectedPropertyOption = customer?.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.SelectedPropertyOption).FirstOrDefault(),
                PropertyOptions = x.PropertiesOptions?.Split(',').Select(z => z.ToUpper().First() + z.ToLower().Substring(1)).ToArray(),
                IsInWishlist = customer.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.Wishlist).Any(),
                Stock = x.ManageInventoryMethod == ManageInventoryMethod.DontManageStock ? 1000 : x.StockQuantity
            }).ToList();

            return Ok(dto);
        }

        [HttpGet]
        public IActionResult SetOrUnsetWishlistProduct(int productId)
        {
            var isSet = false;
            var warnings = new List<string>();
            var storeId = _storeContext.CurrentStore.Id;
            Customer customer = _customerService.GetCustomerById(int.Parse(UserId));
            if (customer == null) return Unauthorized();

            Product product = _productService.GetProductById(productId);
            if (product == null) return NotFound();

            //reset checkout info
            _customerService.ResetCheckoutData(customer, storeId);

            var cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(storeId)
                .ToList();

            ShoppingCartItem shoppingCartItem = null;

            if (!product.Customizable)
            {
                shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart,
                ShoppingCartType.Wishlist, product);
            }

            if (shoppingCartItem != null)
            {
                _shoppingCartService.DeleteShoppingCartItem(shoppingCartItem, true);
            }
            else
            {
                //new shopping cart item
                warnings.AddRange(_shoppingCartService.GetShoppingCartItemWarnings(customer, ShoppingCartType.Wishlist, product, storeId, string.Empty, 0));
                if (cart.Count >= _shoppingCartSettings.MaximumWishlistItems)
                {
                    warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MaximumWishlistItems"), _shoppingCartSettings.MaximumWishlistItems));
                }
                if (!warnings.Any())
                {
                    var now = DateTime.UtcNow;
                    shoppingCartItem = new ShoppingCartItem
                    {
                        ShoppingCartType = ShoppingCartType.Wishlist,
                        StoreId = storeId,
                        Product = product,
                        AttributesXml = null,
                        CustomerEnteredPrice = 0,
                        Quantity = 1,
                        RentalStartDateUtc = null,
                        RentalEndDateUtc = null,
                        CreatedOnUtc = now,
                        UpdatedOnUtc = now,
                        FancyDesign = string.Empty,
                        BuyingBySecondary = false,
                        SelectedPropertyOption = null
                    };
                    customer.ShoppingCartItems.Add(shoppingCartItem);
                    _customerService.UpdateCustomer(customer);

                    //updated "HasShoppingCartItems" property used for performance optimization
                    customer.HasShoppingCartItems = customer.ShoppingCartItems.Any();
                    _customerService.UpdateCustomer(customer);
                    isSet = true;
                }
                else
                    return BadRequest(warnings);
            }

            return Ok(isSet);
        }

        [HttpGet]
        public IActionResult GetWishlistData(int page, int elementsPerPage, bool asProducts = false)
        {
            Customer customer = _customerService.GetCustomerById(int.Parse(UserId));
            List<ShoppingCartItem> currentCart = customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.Wishlist).ToList();
            List<int> currentCartProductIds = currentCart.Select(x => x.ProductId).ToList();
            List<Product> products = _productService.GetAllProductsQuery().Where(x => x.Published && currentCartProductIds.Contains(x.Id)).ToList();
            if (!asProducts)
            {
                var dto = products.Select(x => new WishlistDataDto()
                {
                    ProductId = x.Id,
                    ProductSku = x.Sku,
                    ProductName = x.Name,
                    ProductSubtotal = x.Price,
                }).ToList();
                return Ok(dto);
            }
            else
            {
                var paged = new PagedList<Product>(products, page - 1, elementsPerPage).ToList();
                var dto = paged.Select(x => new ProductDto()
                {
                    Id = x.Id,
                    Sku = x.Sku,
                    Name = x.Name,
                    OldPrice = x.OldPrice > x.Price ? x.OldPrice : 0,
                    Price = x.Price,
                    Discount = _priceCalculationService.GetDiscountAmount(x, customer ?? new Customer(), x.Price, out List<DiscountForCaching> appliedDiscounts),
                    PictureUrl = "/Product/ProductImage?id=" + x.Id,
                    EquivalenceCoefficient = x.EquivalenceCoefficient,
                    WeightInterval = ProductHelper.GetWeightInterval(x),
                    CurrentCartQuantity = customer != null ? customer.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.Quantity).FirstOrDefault() : 0,
                    BuyingBySecondary = customer != null && customer.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.BuyingBySecondary).FirstOrDefault(),
                    SelectedPropertyOption = customer?.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.SelectedPropertyOption).FirstOrDefault(),
                    PropertyOptions = x.PropertiesOptions?.Split(',').Select(z => z.ToUpper().First() + z.ToLower().Substring(1)).ToArray(),
                    IsInWishlist = customer.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.Wishlist).Any(),
                    Stock = x.ManageInventoryMethod == ManageInventoryMethod.DontManageStock ? 1000 : x.StockQuantity
                }).OrderBy(x => x.Name).ToList();
                return Ok(dto);
            }
        }

        // DEPRECATED 02-01-2021
        [HttpGet]
        public IActionResult GetMinOrderAmountErrorMessage()
        {
            int userId = int.Parse(UserId);
            Customer currentCustomer = _customerService.GetCustomerById(userId);
            if (!currentCustomer.Active) return Unauthorized();

            if (_orderProcessingService.ShouldCheckForOrderMinimumValidation(currentCustomer))
            {
                Customer customer = _customerService.GetCustomerById(int.Parse(UserId));
                var cart = customer.ShoppingCartItems.ToList();
                var orderSubtotalErrorMessage = _orderProcessingService.GetMinOrderSubtotalAmountMessage(cart);
                var orderTotalErrorMessage = _orderProcessingService.GetMinOrderTotalAmountMessage(cart);

                return Ok(!string.IsNullOrEmpty(orderSubtotalErrorMessage) ? orderSubtotalErrorMessage :
                    !string.IsNullOrEmpty(orderTotalErrorMessage) ? orderTotalErrorMessage :
                    string.Empty);
            }
            return Ok(string.Empty);
        }

        #endregion
    }
}
