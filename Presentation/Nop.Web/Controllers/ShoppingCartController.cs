using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Html;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Rewards;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping.Date;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.Security.Captcha;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Web.Controllers
{
    public partial class ShoppingCartController : BasePublicController
    {
        #region Fields

        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly IDiscountService _discountService;
        private readonly ICustomerService _customerService;
        private readonly IGiftCardService _giftCardService;
        private readonly IDateRangeService _dateRangeService;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IPermissionService _permissionService;
        private readonly IDownloadService _downloadService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly ICustomerPointService _customerPointService;
        private readonly IRewardItemService _rewardItemService;
        private readonly ISpecialDiscountTakeXPayYService _specialDiscountTakeXPayYService;

        private readonly MediaSettings _mediaSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly OrderSettings _orderSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly CustomerSettings _customerSettings;

        #endregion

        #region Ctor

        public ShoppingCartController(IShoppingCartModelFactory shoppingCartModelFactory,
            IProductService productService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IShoppingCartService shoppingCartService,
            IPictureService pictureService,
            ILocalizationService localizationService,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            ITaxService taxService, ICurrencyService currencyService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            ICheckoutAttributeParser checkoutAttributeParser,
            IDiscountService discountService,
            ICustomerService customerService,
            IGiftCardService giftCardService,
            IDateRangeService dateRangeService,
            ICheckoutAttributeService checkoutAttributeService,
            IWorkflowMessageService workflowMessageService,
            IPermissionService permissionService,
            IDownloadService downloadService,
            IStaticCacheManager cacheManager,
            IWebHelper webHelper,
            ICustomerPointService customerPointService,
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            MediaSettings mediaSettings,
            ShoppingCartSettings shoppingCartSettings,
            OrderSettings orderSettings,
            CaptchaSettings captchaSettings,
            CustomerSettings customerSettings,
            IPaymentService paymentService,
            IOrderService orderService,
            IRewardItemService rewardItemService,
            ISpecialDiscountTakeXPayYService specialDiscountTakeXPayYService)
        {
            this._shoppingCartModelFactory = shoppingCartModelFactory;
            this._productService = productService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._shoppingCartService = shoppingCartService;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._productAttributeService = productAttributeService;
            this._productAttributeParser = productAttributeParser;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._discountService = discountService;
            this._customerService = customerService;
            this._giftCardService = giftCardService;
            this._dateRangeService = dateRangeService;
            this._checkoutAttributeService = checkoutAttributeService;
            this._workflowMessageService = workflowMessageService;
            this._permissionService = permissionService;
            this._downloadService = downloadService;
            this._cacheManager = cacheManager;
            this._webHelper = webHelper;
            this._customerActivityService = customerActivityService;
            this._genericAttributeService = genericAttributeService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._customerPointService = customerPointService;
            this._rewardItemService = rewardItemService;
            this._specialDiscountTakeXPayYService = specialDiscountTakeXPayYService;

            this._mediaSettings = mediaSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._orderSettings = orderSettings;
            this._captchaSettings = captchaSettings;
            this._customerSettings = customerSettings;
        }

        #endregion

        #region Utilities

        protected virtual void ParseAndSaveCheckoutAttributes(List<ShoppingCartItem> cart, IFormCollection form)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = string.Empty;
            var excludeShippableAttributes = !cart.RequiresShipping(_productService, _productAttributeParser);
            var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes(_storeContext.CurrentStore.Id, excludeShippableAttributes);
            foreach (var attribute in checkoutAttributes)
            {
                var controlId = $"checkout_attribute_{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = _checkoutAttributeService.GetCheckoutAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var date = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(date));
                            }
                            catch { }
                            if (selectedDate.HasValue)
                            {
                                attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                    attribute, selectedDate.Value.ToString("D"));
                            }
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            Guid.TryParse(form[controlId], out Guid downloadGuid);
                            var download = _downloadService.GetDownloadByGuid(downloadGuid);
                            if (download != null)
                            {
                                attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                           attribute, download.DownloadGuid.ToString());
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            //validate conditional attributes (if specified)
            foreach (var attribute in checkoutAttributes)
            {
                var conditionMet = _checkoutAttributeParser.IsConditionMet(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                    attributesXml = _checkoutAttributeParser.RemoveCheckoutAttribute(attributesXml, attribute);
            }

            //save checkout attributes
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.CheckoutAttributes, attributesXml, _storeContext.CurrentStore.Id);
        }

        /// <summary>
        /// Parse product attributes on the product details page
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="form">Form</param>
        /// <param name="errors">Errors</param>
        /// <returns>Parsed attributes</returns>
        protected virtual string ParseProductAttributes(Product product, IFormCollection form, List<string> errors)
        {
            //product attributes
            var attributesXml = GetProductAttributesXml(product, form, errors);

            //gift cards
            AddGiftCardsAttributesXml(product, form, ref attributesXml);

            return attributesXml;
        }

        /// <summary>
        /// Parse product rental dates on the product details page
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="form">Form</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        protected virtual void ParseRentalDates(Product product, IFormCollection form,
            out DateTime? startDate, out DateTime? endDate)
        {
            startDate = null;
            endDate = null;

            var startControlId = $"rental_start_date_{product.Id}";
            var endControlId = $"rental_end_date_{product.Id}";
            var ctrlStartDate = form[startControlId];
            var ctrlEndDate = form[endControlId];
            try
            {
                //currenly we support only this format (as in the \Views\Product\_RentalInfo.cshtml file)
                const string datePickerFormat = "MM/dd/yyyy";
                startDate = DateTime.ParseExact(ctrlStartDate, datePickerFormat, CultureInfo.InvariantCulture);
                endDate = DateTime.ParseExact(ctrlEndDate, datePickerFormat, CultureInfo.InvariantCulture);
            }
            catch
            {
            }
        }

        protected virtual void AddGiftCardsAttributesXml(Product product, IFormCollection form, ref string attributesXml)
        {
            if (!product.IsGiftCard) return;

            var recipientName = "";
            var recipientEmail = "";
            var senderName = "";
            var senderEmail = "";
            var giftCardMessage = "";
            foreach (var formKey in form.Keys)
            {
                if (formKey.Equals($"giftcard_{product.Id}.RecipientName", StringComparison.InvariantCultureIgnoreCase))
                {
                    recipientName = form[formKey];
                    continue;
                }
                if (formKey.Equals($"giftcard_{product.Id}.RecipientEmail", StringComparison.InvariantCultureIgnoreCase))
                {
                    recipientEmail = form[formKey];
                    continue;
                }
                if (formKey.Equals($"giftcard_{product.Id}.SenderName", StringComparison.InvariantCultureIgnoreCase))
                {
                    senderName = form[formKey];
                    continue;
                }
                if (formKey.Equals($"giftcard_{product.Id}.SenderEmail", StringComparison.InvariantCultureIgnoreCase))
                {
                    senderEmail = form[formKey];
                    continue;
                }
                if (formKey.Equals($"giftcard_{product.Id}.Message", StringComparison.InvariantCultureIgnoreCase))
                {
                    giftCardMessage = form[formKey];
                }
            }

            attributesXml = _productAttributeParser.AddGiftCardAttribute(attributesXml, recipientName, recipientEmail, senderName, senderEmail, giftCardMessage);
        }

        protected virtual string GetProductAttributesXml(Product product, IFormCollection form, List<string> errors)
        {
            var attributesXml = string.Empty;
            var productAttributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            foreach (var attribute in productAttributes)
            {
                var controlId = $"product_attribute_{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            StringValues ctrlAttributes = form[controlId];
                            var aux = ctrlAttributes.ToString().Split(',');
                            if (!StringValues.IsNullOrEmpty(aux[0]))
                            {
                                try
                                {
                                    var selectedAttributeId = int.Parse(aux[0]);
                                    if (selectedAttributeId > 0)
                                    {
                                        //get quantity entered by customer
                                        var quantity = 1;
                                        var quantityStr = form[$"product_attribute_{attribute.Id}_{selectedAttributeId}_qty"];
                                        if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                            (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                            errors.Add(_localizationService.GetResource("ShoppingCart.QuantityShouldPositive"));

                                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                                    }
                                }
                                catch (Exception e)
                                {
                                    var er = e;
                                }
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                foreach (var item in ctrlAttributes.ToString()
                                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                    {
                                        //get quantity entered by customer
                                        var quantity = 1;
                                        var quantityStr = form[$"product_attribute_{attribute.Id}_{item}_qty"];
                                        if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                            (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                            errors.Add(_localizationService.GetResource("ShoppingCart.QuantityShouldPositive"));

                                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                                    }
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                //get quantity entered by customer
                                var quantity = 1;
                                var quantityStr = form[$"product_attribute_{attribute.Id}_{selectedAttributeId}_qty"];
                                if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                    (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                    errors.Add(_localizationService.GetResource("ShoppingCart.QuantityShouldPositive"));

                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var day = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                            }
                            catch
                            {
                            }
                            if (selectedDate.HasValue)
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedDate.Value.ToString("D"));
                            }
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            Guid.TryParse(form[controlId], out Guid downloadGuid);
                            var download = _downloadService.GetDownloadByGuid(downloadGuid);
                            if (download != null)
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, download.DownloadGuid.ToString());
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            //validate conditional attributes (if specified)
            foreach (var attribute in productAttributes)
            {
                var conditionMet = _productAttributeParser.IsConditionMet(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                {
                    attributesXml = _productAttributeParser.RemoveProductAttribute(attributesXml, attribute);
                }
            }
            return attributesXml;
        }

        protected virtual void SaveItem(ShoppingCartItem updatecartitem, List<string> addToCartWarnings, Product product,
           ShoppingCartType cartType, string attributes, decimal customerEnteredPriceConverted, DateTime? rentalStartDate,
           DateTime? rentalEndDate, int quantity, string fancy = null)
        {
            if (product.Customizable)
            {
                addToCartWarnings.AddRange(_shoppingCartService.AddToCart(_workContext.CurrentCustomer,
                    product, cartType, _storeContext.CurrentStore.Id,
                    attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate, quantity, true, fancy));
            }
            else
            {
                if (updatecartitem == null)
                {
                    //add to the cart
                    addToCartWarnings.AddRange(_shoppingCartService.AddToCart(_workContext.CurrentCustomer,
                        product, cartType, _storeContext.CurrentStore.Id,
                        attributes, customerEnteredPriceConverted,
                        rentalStartDate, rentalEndDate, quantity, true, fancy));
                }
                else
                {
                    var cart = _workContext.CurrentCustomer.ShoppingCartItems
                        .Where(x => x.ShoppingCartType == updatecartitem.ShoppingCartType)
                        .LimitPerStore(_storeContext.CurrentStore.Id)
                        .ToList();
                    var otherCartItemWithSameParameters = _shoppingCartService.FindShoppingCartItemInTheCart(
                        cart, updatecartitem.ShoppingCartType, product, attributes, customerEnteredPriceConverted,
                        rentalStartDate, rentalEndDate);
                    if (otherCartItemWithSameParameters != null &&
                        otherCartItemWithSameParameters.Id == updatecartitem.Id)
                    {
                        //ensure it's some other shopping cart item
                        otherCartItemWithSameParameters = null;
                    }
                    //update existing item
                    addToCartWarnings.AddRange(_shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                        updatecartitem.Id, attributes, customerEnteredPriceConverted,
                        rentalStartDate, rentalEndDate, quantity, true));
                    if (otherCartItemWithSameParameters != null && !addToCartWarnings.Any())
                    {
                        //delete the same shopping cart item (the other one)
                        _shoppingCartService.DeleteShoppingCartItem(otherCartItemWithSameParameters);
                    }
                }
            }
        }

        protected virtual IActionResult GetProductToCartDetails(List<string> addToCartWarnings, ShoppingCartType cartType,
            Product product)
        {
            if (addToCartWarnings.Any())
            {
                //cannot be added to the cart/wishlist
                //let's display warnings
                return Json(new
                {
                    success = false,
                    message = addToCartWarnings.ToArray()
                });
            }

            //added to the cart/wishlist
            switch (cartType)
            {
                case ShoppingCartType.Wishlist:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToWishlist",
                            _localizationService.GetResource("ActivityLog.PublicStore.AddToWishlist"), product.Name);

                        if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct)
                        {
                            //redirect to the wishlist page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("Wishlist")
                            });
                        }

                        //display notification message and update appropriate blocks
                        var updatetopwishlistsectionhtml = string.Format(
                            _localizationService.GetResource("Wishlist.HeaderQuantity"),
                            _workContext.CurrentCustomer.ShoppingCartItems
                                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                                .LimitPerStore(_storeContext.CurrentStore.Id)
                                .ToList()
                                .GetTotalProducts());

                        return Json(new
                        {
                            success = true,
                            message = string.Format(
                                _localizationService.GetResource("Products.ProductHasBeenAddedToTheWishlist.Link"),
                                Url.RouteUrl("Wishlist")),
                            updatetopwishlistsectionhtml,
                            productId = product.Id
                        });
                    }
                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToShoppingCart",
                            _localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart"), product.Name);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct)
                        {
                            //redirect to the shopping cart page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("ShoppingCart")
                            });
                        }

                        //display notification message and update appropriate blocks
                        var updatetopcartsectionhtml = string.Format(
                            _localizationService.GetResource("ShoppingCart.HeaderQuantity"),
                            _workContext.CurrentCustomer.ShoppingCartItems
                                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                                .LimitPerStore(_storeContext.CurrentStore.Id)
                                .ToList()
                                .GetTotalProducts());

                        var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                            ? RenderViewComponentToString("FlyoutShoppingCart")
                            : "";

                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheCart.Link"),
                                Url.RouteUrl("ShoppingCart")),
                            updatetopcartsectionhtml,
                            updateflyoutcartsectionhtml
                        });
                    }
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult GetCouponsOfOrder(int orderId)
        {
            var final = string.Empty;
            var discounts = new List<string>();
            var order = _orderService.GetOrderById(orderId);
            if (order != null)
            {
                var discountIds = order.DiscountUsageHistory.Select(x => x.DiscountId).ToList();
                foreach (var discountId in discountIds)
                {
                    var discount = _discountService.GetDiscountById(discountId);
                    if (discount != null)
                        discounts.Add(discount.Name + " (" + discount.Id + ")");
                }
                final = string.Join(",", discounts);
            }
            return Ok(final);
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult GetCurrentQuantitiesInCart()
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems?
                .Select(x => new
                {
                    quantity = x.Quantity,
                    sku = x.Product.Sku
                });
            return Json(cart);
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult SetBalanceUsageValue(bool value)
        {
            _workContext.CurrentCustomer.IsPaymentWithBalanceActive = value;
            _customerService.UpdateCustomer(_workContext.CurrentCustomer);
            return Ok();
        }

        public virtual void CheckDiscountRemoveByCartItems(ShoppingCartItem sci, List<ShoppingCartItem> cart)
        {
            /*
            if (!string.IsNullOrEmpty(sci.AppliedByCodeByCoupon))
            {
                var cartWithoutCurrentItem = cart.Where(x => x.Id != sci.Id &&
                    x.AppliedByCodeByCoupon == sci.AppliedByCodeByCoupon.ToLower() &&
                    x.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
                if (!cartWithoutCurrentItem.Any())
                    _workContext.CurrentCustomer.RemoveDiscountCouponCode(sci.AppliedByCodeByCoupon);
            }
            */
        }

        public virtual void AddCouponCodeToCartItem(ShoppingCartItem sci)
        {
            var couponCodes = _workContext.CurrentCustomer.ParseAppliedDiscountCouponCodes();
            foreach (var couponCode in couponCodes)
            {
                var discount = _discountService.GetAllDiscounts(null, couponCode).FirstOrDefault();
                if (discount != null)
                {
                    if (/*(discount.ShouldAddProducts ?? false) &&*/ discount.AppliedToProducts.Any())
                    {
                        var productOfDiscount = discount.AppliedToProducts
                            .Where(x => x.Published && !x.Deleted && x.Id == sci.ProductId)
                            .FirstOrDefault();
                        if (productOfDiscount != null)
                        {
                            //sci.AppliedByCodeByCoupon = couponCode.ToLower();
                            _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                            break;
                        }
                    }
                }
            }
        }

        #endregion

        #region Shopping cart

        //add product to cart using AJAX
        //currently we use this method on catalog pages (category/manufacturer/etc)
        [HttpPost]
        public virtual IActionResult AddProductToCart_Catalog(int productId, int shoppingCartTypeId,
            int quantity, bool forceredirection = false, bool buyingBySecondary = false, string selectedPropertyOption = null, bool rewardItemFlag = false)
        {
            var cartType = (ShoppingCartType)shoppingCartTypeId;

            if (selectedPropertyOption == "null") selectedPropertyOption = null;

            var product = _productService.GetProductById(productId);
            if (product == null)
                //no product found
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });


            //Check if the product comes from an apapacho so that the price is $0

            if (product.RewardPointsRequired > 0)
            {
                var currentCartPoints = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(x => x.Product.RewardPointsRequired > 0)
                    .Select(x => x.Product.RewardPointsRequired)
                    .DefaultIfEmpty()
                    .Sum();

                currentCartPoints -= product.RewardPointsRequired;

                var customerAvailablePoints = _customerPointService.GetCustomerPointsBalance(_workContext.CurrentCustomer.Id);
                if ((customerAvailablePoints - currentCartPoints) < product.RewardPointsRequired)
                    return Json(new
                    {
                        success = false,
                        message = $"No tienes puntos suficientes para canjear esta recompensa. Te faltan {(product.RewardPointsRequired - (customerAvailablePoints - currentCartPoints)).ToString("N")} puntos."
                    });
            }

            //we can add only simple products
            if (product.ProductType != ProductType.SimpleProduct)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() })
                });
            }

            //products with "minimum order quantity" more than a specified qty
            if (product.OrderMinimumQuantity > quantity)
            {
                //we cannot add to the cart such products from category pages
                //it can confuse customers. That's why we redirect customers to the product details page
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() })
                });
            }

            if (product.CustomerEntersPrice)
            {
                //cannot be added to the cart (requires a customer to enter price)
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() })
                });
            }

            if (product.IsRental)
            {
                //rental products require start/end dates to be entered
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() })
                });
            }

            var allowedQuantities = product.ParseAllowedQuantities();
            if (allowedQuantities.Length > 0)
            {
                //cannot be added to the cart (requires a customer to select a quantity from dropdownlist)
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() })
                });
            }

            //allow a product to be added to the cart when all attributes are with "read-only checkboxes" type
            var productAttributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            if (productAttributes.Any(pam => pam.AttributeControlType != AttributeControlType.ReadonlyCheckboxes))
            {
                //product has some attributes. let a customer see them
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() })
                });
            }

            //creating XML for "read-only checkboxes" attributes
            var attXml = productAttributes.Aggregate(string.Empty, (attributesXml, attribute) =>
            {
                var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                foreach (var selectedAttributeId in attributeValues
                    .Where(v => v.IsPreSelected)
                    .Select(v => v.Id)
                    .ToList())
                {
                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                        attribute, selectedAttributeId.ToString());
                }
                return attributesXml;
            });

            //get standard warnings without attribute validations
            //first, try to find existing shopping cart item
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == cartType)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart, cartType, product);
            //if we already have the same product in the cart, then use the total quantity to validate
            var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + quantity : quantity;
            var addToCartWarnings = _shoppingCartService
                .GetShoppingCartItemWarnings(_workContext.CurrentCustomer, cartType,
                product, _storeContext.CurrentStore.Id, string.Empty,
                decimal.Zero, null, null, quantityToValidate, false, true, false, false, false);
            if (addToCartWarnings.Any())
            {
                //cannot be added to the cart
                //let's display standard warnings
                return Json(new
                {
                    success = false,
                    message = addToCartWarnings.ToArray()
                });
            }

            //now let's try adding product to the cart (now including product attribute validation, etc)
            decimal customerEnteredPrice = _paymentService.CalculateGroceryPrice(product, shoppingCartItem, quantity, false); //calculate price

            addToCartWarnings = _shoppingCartService.AddToCart(customer: _workContext.CurrentCustomer,
                product: product,
                shoppingCartType: cartType,
                storeId: _storeContext.CurrentStore.Id,
                attributesXml: attXml,
                quantity: quantity,
                customerEnteredPrice: customerEnteredPrice,
                buyingBySecondary: buyingBySecondary,
                selectedPropertyOption: selectedPropertyOption,
                rewardItemFlag: rewardItemFlag);
            if (addToCartWarnings.Any())
            {
                //cannot be added to the cart
                //but we do not display attribute and gift card warnings here. let's do it on the product details page
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() })
                });
            }
            else
            {
                var currentSci = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(x => x.ProductId == productId && x.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .FirstOrDefault();
                AddCouponCodeToCartItem(currentSci);
            }

            //added to the cart/wishlist
            switch (cartType)
            {
                case ShoppingCartType.Wishlist:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToWishlist", _localizationService.GetResource("ActivityLog.PublicStore.AddToWishlist"), product.Name);

                        if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct || forceredirection)
                        {
                            //redirect to the wishlist page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("Wishlist")
                            });
                        }

                        //display notification message and update appropriate blocks
                        var updatetopwishlistsectionhtml = string.Format(_localizationService.GetResource("Wishlist.HeaderQuantity"),
                        _workContext.CurrentCustomer.ShoppingCartItems
                        .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                        .LimitPerStore(_storeContext.CurrentStore.Id)
                        .ToList()
                        .GetTotalProducts());
                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheWishlist.Link"), Url.RouteUrl("Wishlist")),
                            updatetopwishlistsectionhtml
                        });
                    }
                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToShoppingCart", _localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart"), product.Name);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct || forceredirection)
                        {
                            //redirect to the shopping cart page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("ShoppingCart")
                            });
                        }

                        //display notification message and update appropriate blocks
                        var updatetopcartsectionhtml = string.Format(_localizationService.GetResource("ShoppingCart.HeaderQuantity"),
                        _workContext.CurrentCustomer.ShoppingCartItems
                        .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                        .LimitPerStore(_storeContext.CurrentStore.Id)
                        .ToList()
                        .GetTotalProducts());

                        var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                            ? this.RenderViewComponentToString("FlyoutShoppingCart")
                            : "";
                        //if(!rewardItemFlag)
                        //{
                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart")),
                            updatetopcartsectionhtml,
                            updateflyoutcartsectionhtml
                        });
                        //}
                        //else
                        //{
                        //    return RedirectToAction("Cart");
                        //}
                    }
            }
        }

        [HttpGet]
        public virtual IActionResult RemoveProductFromCart_Catalog(int productId, bool buyingBySecondary, string selectedPropertyOption = null)
        {
            if (selectedPropertyOption == "null") selectedPropertyOption = null;
            var sci = _workContext.CurrentCustomer.ShoppingCartItems.Where(x => x.ProductId == productId && x.ShoppingCartType == ShoppingCartType.ShoppingCart).FirstOrDefault();
            if (sci == null)
                //no product found
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });

            if (sci.Quantity <= 1)
            {
                CheckDiscountRemoveByCartItems(sci, _workContext.CurrentCustomer.ShoppingCartItems.ToList());
                _shoppingCartService.DeleteShoppingCartItem(sci, ensureOnlyActiveCheckoutAttributes: true);
            }
            else
            {
                var newQuantity = sci.Quantity - 1;
                decimal customerEnteredPrice = _paymentService.CalculateGroceryPrice(sci.Product, sci, 1, true); ; //calculate price

                _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                        sci.Id, sci.AttributesXml, customerEnteredPrice,
                                        sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                        newQuantity, true, buyingBySecondary, selectedPropertyOption);
            }

            //display notification message and update appropriate blocks
            var updatetopcartsectionhtml = string.Format(_localizationService.GetResource("ShoppingCart.HeaderQuantity"),
            _workContext.CurrentCustomer.ShoppingCartItems
            .Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart)
            .LimitPerStore(_storeContext.CurrentStore.Id)
            .ToList()
            .GetTotalProducts());

            var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                ? this.RenderViewComponentToString("FlyoutShoppingCart")
                : "";

            return Json(new
            {
                success = true,
                message = string.Format("Se quitó el producto del carrito.", Url.RouteUrl("ShoppingCart")),
                updatetopcartsectionhtml,
                updateflyoutcartsectionhtml
            });
        }

        [HttpGet]
        public virtual IActionResult UpdateProductQty_Catalog(int productId, bool buyingBySecondary, int qty, string selectedPropertyOption = null)
        {
            var sciDiscountCoupon = string.Empty;
            var currentSci = _workContext.CurrentCustomer.ShoppingCartItems.Where(x => x.ProductId == productId && x.ShoppingCartType == ShoppingCartType.ShoppingCart).FirstOrDefault();
            var isRemoving = false;
            if (currentSci != null)
            {
                isRemoving = currentSci.Quantity > qty;
                if (qty == 0)
                    CheckDiscountRemoveByCartItems(currentSci, _workContext.CurrentCustomer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList());
                _shoppingCartService.DeleteShoppingCartItem(currentSci, ensureOnlyActiveCheckoutAttributes: true);
            }

            if (qty == 0)
            {
                //display notification message and update appropriate blocks
                var updatetopcartsectionhtml = string.Format(_localizationService.GetResource("ShoppingCart.HeaderQuantity"),
                _workContext.CurrentCustomer.ShoppingCartItems
                .Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList()
                .GetTotalProducts());

                var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                    ? this.RenderViewComponentToString("FlyoutShoppingCart")
                    : "";

                return Json(new
                {
                    success = true,
                    message = string.Format("Se quitó el producto del carrito.", Url.RouteUrl("ShoppingCart")),
                    updatetopcartsectionhtml,
                    updateflyoutcartsectionhtml
                });
            }

            var cartType = ShoppingCartType.ShoppingCart;

            if (selectedPropertyOption == "null") selectedPropertyOption = null;

            var product = _productService.GetProductById(productId);
            if (product == null)
                //no product found
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });

            //we can add only simple products
            if (product.ProductType != ProductType.SimpleProduct)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() })
                });
            }

            //products with "minimum order quantity" more than a specified qty
            if (product.OrderMinimumQuantity > qty)
            {
                //we cannot add to the cart such products from category pages
                //it can confuse customers. That's why we redirect customers to the product details page
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() })
                });
            }

            if (product.CustomerEntersPrice)
            {
                //cannot be added to the cart (requires a customer to enter price)
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() })
                });
            }

            if (product.IsRental)
            {
                //rental products require start/end dates to be entered
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() })
                });
            }

            var allowedQuantities = product.ParseAllowedQuantities();
            if (allowedQuantities.Length > 0)
            {
                //cannot be added to the cart (requires a customer to select a quantity from dropdownlist)
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() })
                });
            }

            //allow a product to be added to the cart when all attributes are with "read-only checkboxes" type
            var productAttributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            if (productAttributes.Any(pam => pam.AttributeControlType != AttributeControlType.ReadonlyCheckboxes))
            {
                //product has some attributes. let a customer see them
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() })
                });
            }

            //creating XML for "read-only checkboxes" attributes
            var attXml = productAttributes.Aggregate(string.Empty, (attributesXml, attribute) =>
            {
                var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                foreach (var selectedAttributeId in attributeValues
                    .Where(v => v.IsPreSelected)
                    .Select(v => v.Id)
                    .ToList())
                {
                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                        attribute, selectedAttributeId.ToString());
                }
                return attributesXml;
            });

            //get standard warnings without attribute validations
            //first, try to find existing shopping cart item
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == cartType)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart, cartType, product);
            //if we already have the same product in the cart, then use the total quantity to validate
            var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + qty : qty;
            var addToCartWarnings = _shoppingCartService
                .GetShoppingCartItemWarnings(_workContext.CurrentCustomer, cartType,
                product, _storeContext.CurrentStore.Id, string.Empty,
                decimal.Zero, null, null, quantityToValidate, false, true, false, false, false);
            if (addToCartWarnings.Any())
            {
                //cannot be added to the cart
                //let's display standard warnings
                return Json(new
                {
                    success = false,
                    message = addToCartWarnings.ToArray()
                });
            }

            //now let's try adding product to the cart (now including product attribute validation, etc)
            decimal customerEnteredPrice = _paymentService.CalculateGroceryPrice(product, shoppingCartItem, qty, isRemoving); //calculate price

            addToCartWarnings = _shoppingCartService.AddToCart(customer: _workContext.CurrentCustomer,
                product: product,
                shoppingCartType: cartType,
                storeId: _storeContext.CurrentStore.Id,
                attributesXml: attXml,
                quantity: qty, customerEnteredPrice: customerEnteredPrice, buyingBySecondary: buyingBySecondary, selectedPropertyOption: selectedPropertyOption);

            if (addToCartWarnings.Any())
            {
                //cannot be added to the cart
                //but we do not display attribute and gift card warnings here. let's do it on the product details page
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() })
                });
            }
            else
            {
                currentSci = _workContext.CurrentCustomer.ShoppingCartItems.Where(x => x.ProductId == productId && x.ShoppingCartType == ShoppingCartType.ShoppingCart).FirstOrDefault();
                AddCouponCodeToCartItem(currentSci);
            }

            //added to the cart/wishlist
            switch (cartType)
            {
                case ShoppingCartType.Wishlist:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToWishlist", _localizationService.GetResource("ActivityLog.PublicStore.AddToWishlist"), product.Name);

                        if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct)
                        {
                            //redirect to the wishlist page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("Wishlist")
                            });
                        }

                        //display notification message and update appropriate blocks
                        var updatetopwishlistsectionhtml = string.Format(_localizationService.GetResource("Wishlist.HeaderQuantity"),
                        _workContext.CurrentCustomer.ShoppingCartItems
                        .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                        .LimitPerStore(_storeContext.CurrentStore.Id)
                        .ToList()
                        .GetTotalProducts());
                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheWishlist.Link"), Url.RouteUrl("Wishlist")),
                            updatetopwishlistsectionhtml
                        });
                    }
                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToShoppingCart", _localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart"), product.Name);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct)
                        {
                            //redirect to the shopping cart page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("ShoppingCart")
                            });
                        }

                        //display notification message and update appropriate blocks
                        var updatetopcartsectionhtml = string.Format(_localizationService.GetResource("ShoppingCart.HeaderQuantity"),
                        _workContext.CurrentCustomer.ShoppingCartItems
                        .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                        .LimitPerStore(_storeContext.CurrentStore.Id)
                        .ToList()
                        .GetTotalProducts());

                        var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                            ? this.RenderViewComponentToString("FlyoutShoppingCart")
                            : "";

                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart")),
                            updatetopcartsectionhtml,
                            updateflyoutcartsectionhtml
                        });
                    }
            }
        }

        [HttpGet]
        public virtual IActionResult UpdateCartProductBuyingType(int productId, bool buyingBySecondary, string selectedPropertyOption = null)
        {
            var sci = _workContext.CurrentCustomer.ShoppingCartItems.Where(x => x.ProductId == productId && x.ShoppingCartType == ShoppingCartType.ShoppingCart).FirstOrDefault();
            if (sci == null)
                return NoContent();

            if (selectedPropertyOption == "null") selectedPropertyOption = null;

            _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                    sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice,
                                    sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                    sci.Quantity, true, buyingBySecondary, selectedPropertyOption);

            return NoContent();
        }

        //add product to cart using AJAX
        //currently we use this method on the product details pages
        [HttpPost]
        public virtual IActionResult AddProductToCart_Details(int productId, int shoppingCartTypeId, IFormCollection form)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("HomePage")
                });
            }

            //we can add only simple products
            if (product.ProductType != ProductType.SimpleProduct)
            {
                return Json(new
                {
                    success = false,
                    message = "Only simple products could be added to the cart"
                });
            }

            //update existing shopping cart item
            var updatecartitemid = 0;
            foreach (var formKey in form.Keys)
                if (formKey.Equals($"addtocart_{productId}.UpdatedShoppingCartItemId", StringComparison.InvariantCultureIgnoreCase))
                {
                    int.TryParse(form[formKey], out updatecartitemid);
                    break;
                }
            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
            {
                //search with the same cart type as specified
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(x => x.ShoppingCartTypeId == shoppingCartTypeId)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                //not found? let's ignore it. in this case we'll add a new item
                //if (updatecartitem == null)
                //{
                //    return Json(new
                //    {
                //        success = false,
                //        message = "No shopping cart item found to update"
                //    });
                //}
                //is it this product?
                if (updatecartitem != null && product.Id != updatecartitem.ProductId)
                {
                    return Json(new
                    {
                        success = false,
                        message = "This product does not match a passed shopping cart item identifier"
                    });
                }
            }

            //customer entered price
            var customerEnteredPriceConverted = decimal.Zero;
            if (product.CustomerEntersPrice)
            {
                foreach (var formKey in form.Keys)
                {
                    if (formKey.Equals($"addtocart_{productId}.CustomerEnteredPrice", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (decimal.TryParse(form[formKey], out decimal customerEnteredPrice))
                            customerEnteredPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(customerEnteredPrice, _workContext.WorkingCurrency);
                        break;
                    }
                }
            }

            //quantity
            var quantity = 1;
            foreach (var formKey in form.Keys)
                if (formKey.Equals($"addtocart_{productId}.EnteredQuantity", StringComparison.InvariantCultureIgnoreCase))
                {
                    int.TryParse(form[formKey], out quantity);
                    break;
                }

            var addToCartWarnings = new List<string>();

            //product and gift card attributes
            var attributes = ParseProductAttributes(product, form, addToCartWarnings);

            //rental attributes
            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            if (product.IsRental)
            {
                ParseRentalDates(product, form, out rentalStartDate, out rentalEndDate);
            }

            var cartType = updatecartitem == null ? (ShoppingCartType)shoppingCartTypeId :
                //if the item to update is found, then we ignore the specified "shoppingCartTypeId" parameter
                updatecartitem.ShoppingCartType;

            string fancy = form["input-product-fancy"];

            SaveItem(updatecartitem, addToCartWarnings, product, cartType, attributes, customerEnteredPriceConverted, rentalStartDate, rentalEndDate, quantity, fancy);

            if (addToCartWarnings.Count() > 0 && TeedCommerceStores.CurrentStore == TeedStores.Lamy)
            {
                var outOfStockMessage = _localizationService.GetResource("ShoppingCart.OutOfStock");
                var selectAttributeMessage = _localizationService.GetResource("ShoppingCart.SelectAttribute");
                if (selectAttributeMessage.Contains("{"))
                    selectAttributeMessage = selectAttributeMessage.Split('{').FirstOrDefault();
                if (addToCartWarnings.Where(x => x == outOfStockMessage).Any() &&
                    addToCartWarnings.Where(x => x.Contains(selectAttributeMessage)).Any())
                    addToCartWarnings.Remove(addToCartWarnings.Where(x => x == outOfStockMessage).FirstOrDefault());
            }

            //return result
            return GetProductToCartDetails(addToCartWarnings, cartType, product);
        }

        public virtual IActionResult Productdetails_Stockquantity(int productId, IFormCollection form)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return new NullJsonResult();

            var errors = new List<string>();
            var attributeXml = ParseProductAttributes(product, form, errors);

            var combination = _productAttributeParser.FindProductAttributeCombination(product, attributeXml);
            var sQty = combination != null ? combination.StockQuantity : 50;

            return Json(sQty);
        }

        //handle product attribute selection event. this way we return new price, overridden gtin/sku/mpn
        //currently we use this method on the product details pages
        [HttpPost]
        public virtual IActionResult ProductDetails_AttributeChange(int productId, bool validateAttributeConditions,
            bool loadPicture, IFormCollection form, bool getCombinationAttributes = false)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return new NullJsonResult();

            var errors = new List<string>();
            var attributeXml = ParseProductAttributes(product, form, errors);

            //rental attributes
            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            if (product.IsRental)
            {
                ParseRentalDates(product, form, out rentalStartDate, out rentalEndDate);
            }

            //sku, mpn, gtin
            var sku = product.FormatSku(attributeXml, _productAttributeParser);
            var mpn = product.FormatMpn(attributeXml, _productAttributeParser);
            var gtin = product.FormatGtin(attributeXml, _productAttributeParser);

            //price
            var price = "";
            var priceWithoutDiscount = "";
            if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices) && !product.CustomerEntersPrice)
            {
                //we do not calculate price of "customer enters price" option is enabled
                List<DiscountForCaching> scDiscounts;
                var finalPrice = _priceCalculationService.GetUnitPrice(product,
                    _workContext.CurrentCustomer,
                    ShoppingCartType.ShoppingCart,
                    1, attributeXml, 0,
                    rentalStartDate, rentalEndDate,
                    true, out decimal _, out scDiscounts);
                var finalPriceWithDiscountBase = _taxService.GetProductPrice(product, finalPrice, out decimal _);
                var finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);
                price = _priceFormatter.FormatPrice(finalPriceWithDiscount);

                var finalWithoutDiscount = _priceCalculationService.GetUnitPrice(product,
                    _workContext.CurrentCustomer,
                    ShoppingCartType.ShoppingCart,
                    1, attributeXml, 0,
                    rentalStartDate, rentalEndDate,
                    false, out decimal _, out scDiscounts);
                var finalPriceWithoutDiscountBase = _taxService.GetProductPrice(product, finalWithoutDiscount, out decimal _);
                var finalPriceWithoutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase, _workContext.WorkingCurrency);
                priceWithoutDiscount = _priceFormatter.FormatPrice(finalPriceWithoutDiscount);
            }

            //stock
            var stockAvailability = product.FormatStockMessage(attributeXml, _localizationService, _productAttributeParser, _dateRangeService);

            //conditional attributes
            var enabledAttributeMappingIds = new List<int>();
            var disabledAttributeMappingIds = new List<int>();
            if (validateAttributeConditions)
            {
                var attributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
                foreach (var attribute in attributes)
                {
                    var conditionMet = _productAttributeParser.IsConditionMet(attribute, attributeXml);
                    if (conditionMet.HasValue)
                    {
                        if (conditionMet.Value)
                            enabledAttributeMappingIds.Add(attribute.Id);
                        else
                            disabledAttributeMappingIds.Add(attribute.Id);
                    }
                }
            }

            //picture. used when we want to override a default product picture when some attribute is selected
            var pictureFullSizeUrl = "";
            var pictureDefaultSizeUrl = "";
            var boundingX = "";
            var boundingY = "";
            var boundingHeight = "";
            var boundingWidth = "";
            if (loadPicture)
            {
                //just load (return) the first found picture (in case if we have several distinct attributes with associated pictures)
                //actually we're going to support pictures associated to attribute combinations (not attribute values) soon. it'll more flexible approach
                var attributeValues = _productAttributeParser.ParseProductAttributeValues(attributeXml);
                var attributeValueWithPicture = attributeValues.FirstOrDefault(x => x.PictureId > 0);
                if (attributeValueWithPicture != null)
                {
                    var productAttributePictureCacheKey = string.Format(ModelCacheEventConsumer.PRODUCTATTRIBUTE_PICTURE_MODEL_KEY,
                                    attributeValueWithPicture.PictureId,
                                    _webHelper.IsCurrentConnectionSecured(),
                                    _storeContext.CurrentStore.Id);
                    var pictureModel = _cacheManager.Get(productAttributePictureCacheKey, () =>
                    {
                        var valuePicture = _pictureService.GetPictureById(attributeValueWithPicture.PictureId);
                        if (valuePicture != null)
                        {
                            return new PictureModel
                            {
                                FullSizeImageUrl = _pictureService.GetPictureUrl(valuePicture),
                                ImageUrl = _pictureService.GetPictureUrl(valuePicture, _mediaSettings.ProductDetailsPictureSize),
                                BoundingX = valuePicture.BoundingX,
                                BoundingY = valuePicture.BoundingY,
                                BoundingHeight = valuePicture.BoundingHeight,
                                BoundingWidth = valuePicture.BoundingWidth
                            };
                        }
                        return new PictureModel();
                    });
                    pictureFullSizeUrl = pictureModel.FullSizeImageUrl;
                    pictureDefaultSizeUrl = pictureModel.ImageUrl;
                    boundingX = pictureModel.BoundingX;
                    boundingY = pictureModel.BoundingY;
                    boundingHeight = pictureModel.BoundingHeight;
                    boundingWidth = pictureModel.BoundingWidth;
                }

            }

            var isFreeShipping = product.IsFreeShipping;
            if (isFreeShipping && !string.IsNullOrEmpty(attributeXml))
            {
                isFreeShipping = _productAttributeParser.ParseProductAttributeValues(attributeXml)
                    .Where(attributeValue => attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                    .Select(attributeValue => _productService.GetProductById(attributeValue.AssociatedProductId))
                    .All(associatedProduct => associatedProduct == null || !associatedProduct.IsShipEnabled || associatedProduct.IsFreeShipping);
            }

            var availableAttributes = new List<AttributeCombinationInfo>();
            if (TeedCommerceStores.CurrentStore == TeedStores.Lamy && getCombinationAttributes)
            {
                var currentAttribute = _productAttributeParser.ParseProductAttributeValues(attributeXml)
                    .Where(x => !string.IsNullOrEmpty(x.ColorSquaresRgb)).FirstOrDefault();

                var combinations = _productAttributeParser.GetCombinationsByProductAttributeValue(product, currentAttribute);
                if (combinations.Any() && !string.IsNullOrEmpty(currentAttribute?.ColorSquaresRgb))
                {
                    foreach (var combination in combinations)
                    {
                        var combinationValues = _productAttributeParser.ParseProductAttributeValues(combination.AttributesXml);
                        availableAttributes.AddRange(combinationValues.Where(x => x.Id != currentAttribute.Id)
                            .Select(x => new AttributeCombinationInfo { Id = x.Id, IsPreSelected = combination.IsPreSelected ?? false }));
                    }
                }
            }

            return Json(new
            {
                gtin,
                mpn,
                sku,
                price,
                priceWithoutDiscount,
                stockAvailability,
                enabledattributemappingids = enabledAttributeMappingIds.ToArray(),
                disabledattributemappingids = disabledAttributeMappingIds.ToArray(),
                pictureFullSizeUrl,
                pictureDefaultSizeUrl,
                boundingX,
                boundingY,
                boundingHeight,
                boundingWidth,
                isFreeShipping,
                message = errors.Any() ? errors.ToArray() : null,
                availableAttributes = availableAttributes.ToArray()
            });
        }

        [HttpGet]
        public virtual IActionResult ProductDetails_AttributePrice(int productId, string attributeData)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return Ok();

            //price
            var price = "";
            if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices) && !product.CustomerEntersPrice)
            {
                var attributeDatas = attributeData.Split('-');
                var attributeXml = "<Attributes><ProductAttribute ID=\"" +
                    attributeDatas[0] + "\"><ProductAttributeValue><Value>" +
                    attributeDatas[1] + "</Value></ProductAttributeValue></ProductAttribute></Attributes>";
                //we do not calculate price of "customer enters price" option is enabled
                List<DiscountForCaching> scDiscounts;
                var finalPrice = _priceCalculationService.GetUnitPrice(product,
                    _workContext.CurrentCustomer,
                    ShoppingCartType.ShoppingCart,
                    1, attributeXml, 0,
                    null, null,
                    true, out decimal _, out scDiscounts);
                var finalPriceWithDiscountBase = _taxService.GetProductPrice(product, finalPrice, out decimal _);
                var finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);
                price = _priceFormatter.FormatPrice(finalPriceWithDiscount);
                return Json(price);
            }
            return Ok();
        }

        [HttpPost]
        public virtual IActionResult CheckoutAttributeChange(IFormCollection form, bool isEditable)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            //save selected attributes
            ParseAndSaveCheckoutAttributes(cart, form);
            var attributeXml = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes,
                _genericAttributeService, _storeContext.CurrentStore.Id);

            //conditions
            var enabledAttributeIds = new List<int>();
            var disabledAttributeIds = new List<int>();
            var excludeShippableAttributes = !cart.RequiresShipping(_productService, _productAttributeParser);
            var attributes = _checkoutAttributeService.GetAllCheckoutAttributes(_storeContext.CurrentStore.Id, excludeShippableAttributes);
            foreach (var attribute in attributes)
            {
                var conditionMet = _checkoutAttributeParser.IsConditionMet(attribute, attributeXml);
                if (conditionMet.HasValue)
                {
                    if (conditionMet.Value)
                        enabledAttributeIds.Add(attribute.Id);
                    else
                        disabledAttributeIds.Add(attribute.Id);
                }
            }

            //update blocks
            var ordetotalssectionhtml = this.RenderViewComponentToString("OrderTotals", new { isEditable });
            var selectedcheckoutattributesssectionhtml = this.RenderViewComponentToString("SelectedCheckoutAttributes");

            return Json(new
            {
                ordetotalssectionhtml,
                selectedcheckoutattributesssectionhtml,
                enabledattributeids = enabledAttributeIds.ToArray(),
                disabledattributeids = disabledAttributeIds.ToArray()
            });
        }

        [HttpPost]
        public virtual IActionResult UploadFileProductAttribute(int attributeId)
        {
            var attribute = _productAttributeService.GetProductAttributeMappingById(attributeId);
            if (attribute == null || attribute.AttributeControlType != AttributeControlType.FileUpload)
            {
                return Json(new
                {
                    success = false,
                    downloadGuid = Guid.Empty
                });
            }

            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if (httpPostedFile == null)
            {
                return Json(new
                {
                    success = false,
                    message = "No file uploaded",
                    downloadGuid = Guid.Empty
                });
            }

            var fileBinary = httpPostedFile.GetDownloadBits();

            var qqFileNameParameter = "qqfilename";
            var fileName = httpPostedFile.FileName;
            if (string.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
                fileName = Request.Form[qqFileNameParameter].ToString();
            //remove path (passed in IE)
            fileName = Path.GetFileName(fileName);

            var contentType = httpPostedFile.ContentType;

            var fileExtension = Path.GetExtension(fileName);
            if (!string.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            if (attribute.ValidationFileMaximumSize.HasValue)
            {
                //compare in bytes
                var maxFileSizeBytes = attribute.ValidationFileMaximumSize.Value * 1024;
                if (fileBinary.Length > maxFileSizeBytes)
                {
                    //when returning JSON the mime-type must be set to text/plain
                    //otherwise some browsers will pop-up a "Save As" dialog.
                    return Json(new
                    {
                        success = false,
                        message = string.Format(_localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"), attribute.ValidationFileMaximumSize.Value),
                        downloadGuid = Guid.Empty
                    });
                }
            }

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = "",
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = Path.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            _downloadService.InsertDownload(download);

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Json(new
            {
                success = true,
                message = _localizationService.GetResource("ShoppingCart.FileUploaded"),
                downloadUrl = Url.Action("GetFileUpload", "Download", new { downloadId = download.DownloadGuid }),
                downloadGuid = download.DownloadGuid
            });
        }

        [HttpPost]
        public virtual IActionResult UploadFileCheckoutAttribute(int attributeId)
        {
            var attribute = _checkoutAttributeService.GetCheckoutAttributeById(attributeId);
            if (attribute == null || attribute.AttributeControlType != AttributeControlType.FileUpload)
            {
                return Json(new
                {
                    success = false,
                    downloadGuid = Guid.Empty
                });
            }

            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if (httpPostedFile == null)
            {
                return Json(new
                {
                    success = false,
                    message = "No file uploaded",
                    downloadGuid = Guid.Empty
                });
            }

            var fileBinary = httpPostedFile.GetDownloadBits();

            var qqFileNameParameter = "qqfilename";
            var fileName = httpPostedFile.FileName;
            if (string.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
                fileName = Request.Form[qqFileNameParameter].ToString();
            //remove path (passed in IE)
            fileName = Path.GetFileName(fileName);

            var contentType = httpPostedFile.ContentType;

            var fileExtension = Path.GetExtension(fileName);
            if (!string.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            if (attribute.ValidationFileMaximumSize.HasValue)
            {
                //compare in bytes
                var maxFileSizeBytes = attribute.ValidationFileMaximumSize.Value * 1024;
                if (fileBinary.Length > maxFileSizeBytes)
                {
                    //when returning JSON the mime-type must be set to text/plain
                    //otherwise some browsers will pop-up a "Save As" dialog.
                    return Json(new
                    {
                        success = false,
                        message = string.Format(_localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"), attribute.ValidationFileMaximumSize.Value),
                        downloadGuid = Guid.Empty
                    });
                }
            }

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = "",
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = Path.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            _downloadService.InsertDownload(download);

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Json(new
            {
                success = true,
                message = _localizationService.GetResource("ShoppingCart.FileUploaded"),
                downloadUrl = Url.Action("GetFileUpload", "Download", new { downloadId = download.DownloadGuid }),
                downloadGuid = download.DownloadGuid
            });
        }


        public virtual IActionResult DeleteShopingCart()
        {
            ////_shoppingCartService.DeleteShoppingCartItem(_workContext.CurrentCustomer);
            //_workContext.CurrentCustomer.ShoppingCartItems.Clear();
            //return RedirectToAction("Cart", "ShoppingCart");

            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            //updated cart
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
               .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
               .LimitPerStore(_storeContext.CurrentStore.Id)
               .ToList();
            foreach (var item in cart)
            {
                _shoppingCartService.DeleteShoppingCartItem(item, ensureOnlyActiveCheckoutAttributes: true);
            }
            cart.Clear();


            return RedirectToAction("Cart");
        }

        [HttpsRequirement(SslRequirement.Yes)]
        public virtual IActionResult Cart()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            _shoppingCartService.AddGiftProduct(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id);

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = new ShoppingCartModel();

            var couponCodes = _workContext.CurrentCustomer.ParseAppliedDiscountCouponCodes();
            var discountsWithMinimumAmount = _discountService.GetAllDiscountsForCaching()
                    .Where(d => d.RequiresCouponCode && couponCodes.Contains(d.CouponCode?.ToLower()) && d.OrderMinimumAmount > 0)
                    .ToList();
            string minimumAmountError = null;
            if (discountsWithMinimumAmount.Any())
            {
                foreach (var discount in discountsWithMinimumAmount)
                {
                    minimumAmountError = ValidateMinimumAmount(discount, cart);
                    if (!string.IsNullOrWhiteSpace(minimumAmountError))
                        _workContext.CurrentCustomer.RemoveDiscountCouponCode(discount.CouponCode);
                }
            }

            //check if cart has deleted products and remove them
            cart = _shoppingCartModelFactory.CheckIfCartHasDeletedProducts(cart);

            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);
            return View(model);
        }

        public virtual int CartAjax()
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id);
            if (Nop.Services.TeedCommerceStores.CurrentStore == Nop.Services.TeedStores.CentralEnLinea)
            {
                return cart.Count();
            }
            else
            {
                return cart.Select(x => x.Quantity).DefaultIfEmpty().Sum();
            }
        }

        [HttpPost, ActionName("Cart")]
        [FormValueRequired("updatecart")]
        public virtual IActionResult UpdateCart(IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var allIdsToRemove = form.ContainsKey("removefromcart") ?
                form["removefromcart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList() :
                new List<int>();

            //current warnings <cart item identifier, warnings>
            var innerWarnings = new Dictionary<int, IList<string>>();
            foreach (var sci in cart)
            {
                var remove = allIdsToRemove.Contains(sci.Id);
                if (remove)
                {
                    CheckDiscountRemoveByCartItems(sci, cart);
                    _shoppingCartService.DeleteShoppingCartItem(sci, ensureOnlyActiveCheckoutAttributes: true);
                }
                else
                {
                    foreach (var formKey in form.Keys)
                        if (formKey.Equals($"itemquantity{sci.Id}", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int newQuantity;
                            if (int.TryParse(form[formKey], out newQuantity))
                            {
                                var currSciWarnings = _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                    sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice,
                                    sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                    newQuantity, true);
                                innerWarnings.Add(sci.Id, currSciWarnings);
                            }
                            break;
                        }
                }
            }

            //parse and save checkout attributes
            ParseAndSaveCheckoutAttributes(cart, form);

            //updated cart
            cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            if (cart.Count == 1 && cart.FirstOrDefault().Product.Sku.Contains("Empaque-"))
            {
                CheckDiscountRemoveByCartItems(cart.FirstOrDefault(), cart);
                _shoppingCartService.DeleteShoppingCartItem(cart.FirstOrDefault(), ensureOnlyActiveCheckoutAttributes: true);
                cart.Clear();
            }

            return RedirectToAction("Cart");

            //var model = new ShoppingCartModel();
            //model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);
            ////update current warnings
            //foreach (var kvp in innerWarnings)
            //{
            //    //kvp = <cart item identifier, warnings>
            //    var sciId = kvp.Key;
            //    var warnings = kvp.Value;
            //    //find model
            //    var sciModel = model.Items.FirstOrDefault(x => x.Id == sciId);
            //    if (sciModel != null)
            //        foreach (var w in warnings)
            //            if (!sciModel.Warnings.Contains(w))
            //                sciModel.Warnings.Add(w);
            //}

            //return View(model);
        }

        [HttpPost, ActionName("Cart")]
        [FormValueRequired("responsiveupdatecart")]
        public virtual IActionResult ResponsiveUpdateCart(IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var allIdsToRemove = form.ContainsKey("responsiveremovefromcart") ?
                form["responsiveremovefromcart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList() :
                new List<int>();

            //current warnings <cart item identifier, warnings>
            var innerWarnings = new Dictionary<int, IList<string>>();
            foreach (var sci in cart)
            {
                var remove = allIdsToRemove.Contains(sci.Id);
                if (remove)
                {
                    CheckDiscountRemoveByCartItems(sci, cart);
                    _shoppingCartService.DeleteShoppingCartItem(sci, ensureOnlyActiveCheckoutAttributes: true);
                }
                else
                {
                    foreach (var formKey in form.Keys)
                        if (formKey.Equals($"itemquantity{sci.Id}", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int newQuantity;
                            if (int.TryParse(form[formKey], out newQuantity))
                            {
                                var currSciWarnings = _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                    sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice,
                                    sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                    newQuantity, true);
                                innerWarnings.Add(sci.Id, currSciWarnings);
                            }
                            break;
                        }
                }
            }

            //parse and save checkout attributes
            ParseAndSaveCheckoutAttributes(cart, form);

            //updated cart
            cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            return RedirectToAction("Cart");
            //var model = new ShoppingCartModel();
            //model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);
            ////update current warnings
            //foreach (var kvp in innerWarnings)
            //{
            //    //kvp = <cart item identifier, warnings>
            //    var sciId = kvp.Key;
            //    var warnings = kvp.Value;
            //    //find model
            //    var sciModel = model.Items.FirstOrDefault(x => x.Id == sciId);
            //    if (sciModel != null)
            //        foreach (var w in warnings)
            //            if (!sciModel.Warnings.Contains(w))
            //                sciModel.Warnings.Add(w);
            //}
            //return View(model);
        }

        [HttpPost]
        public virtual IActionResult UpdateCartAjax(IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var allIdsToRemove = form.ContainsKey("removefromcart") ?
                form["removefromcart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList() :
                new List<int>();

            //current warnings <cart item identifier, warnings>
            var innerWarnings = new Dictionary<int, IList<string>>();
            foreach (var sci in cart)
            {
                var remove = allIdsToRemove.Contains(sci.Id);
                if (remove)
                {
                    CheckDiscountRemoveByCartItems(sci, cart);
                    _shoppingCartService.DeleteShoppingCartItem(sci, ensureOnlyActiveCheckoutAttributes: true);
                }
                else
                {
                    foreach (var formKey in form.Keys)
                        if (formKey.Equals($"itemquantity{sci.Id}", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int newQuantity;
                            if (int.TryParse(form[formKey], out newQuantity))
                            {
                                var currSciWarnings = _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                    sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice,
                                    sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                    newQuantity, true);
                                innerWarnings.Add(sci.Id, currSciWarnings);
                            }
                            break;
                        }
                }
            }

            //parse and save checkout attributes
            ParseAndSaveCheckoutAttributes(cart, form);

            //updated cart
            cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = new ShoppingCartModel();
            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);

            //update current warnings
            foreach (var kvp in innerWarnings)
            {
                //kvp = <cart item identifier, warnings>
                var sciId = kvp.Key;
                var warnings = kvp.Value;
                //find model
                var sciModel = model.Items.FirstOrDefault(x => x.Id == sciId);
                if (sciModel != null)
                    foreach (var w in warnings)
                        if (!sciModel.Warnings.Contains(w))
                            sciModel.Warnings.Add(w);
            }

            /*
             * https://codepen.io/kvzivn/pen/waKoWq
             */
            return Ok();
        }

        [HttpPost]
        public virtual IActionResult ResponsiveUpdateCartAjax(IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var allIdsToRemove = form.ContainsKey("responsiveremovefromcart") ?
                form["responsiveremovefromcart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList() :
                new List<int>();

            //current warnings <cart item identifier, warnings>
            var innerWarnings = new Dictionary<int, IList<string>>();
            foreach (var sci in cart)
            {
                var remove = allIdsToRemove.Contains(sci.Id);
                if (remove)
                {
                    CheckDiscountRemoveByCartItems(sci, cart);
                    _shoppingCartService.DeleteShoppingCartItem(sci, ensureOnlyActiveCheckoutAttributes: true);
                }
                else
                {
                    foreach (var formKey in form.Keys)
                        if (formKey.Equals($"responsiveitemquantity{sci.Id}", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int newQuantity;
                            if (int.TryParse(form[formKey], out newQuantity))
                            {
                                var currSciWarnings = _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                    sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice,
                                    sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                    newQuantity, true);
                                innerWarnings.Add(sci.Id, currSciWarnings);
                            }
                            break;
                        }
                }
            }

            //parse and save checkout attributes
            ParseAndSaveCheckoutAttributes(cart, form);

            //updated cart
            cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = new ShoppingCartModel();
            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);
            //update current warnings
            foreach (var kvp in innerWarnings)
            {
                //kvp = <cart item identifier, warnings>
                var sciId = kvp.Key;
                var warnings = kvp.Value;
                //find model
                var sciModel = model.Items.FirstOrDefault(x => x.Id == sciId);
                if (sciModel != null)
                    foreach (var w in warnings)
                        if (!sciModel.Warnings.Contains(w))
                            sciModel.Warnings.Add(w);
            }

            /*
             * https://codepen.io/kvzivn/pen/waKoWq
             */
            return Ok();
        }

        [HttpPost, ActionName("Cart")]
        [FormValueRequired("continueshopping")]
        public virtual IActionResult ContinueShopping()
        {
            var returnUrl = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastContinueShoppingPage, _storeContext.CurrentStore.Id);
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToRoute("HomePage");
        }

        [HttpPost, ActionName("Cart")]
        [FormValueRequired("checkout")]
        public virtual IActionResult StartCheckout(IFormCollection form)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            //parse and save checkout attributes
            ParseAndSaveCheckoutAttributes(cart, form);

            //validate attributes
            var checkoutAttributes = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService, _storeContext.CurrentStore.Id);
            var checkoutAttributeWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributes, true);
            if (checkoutAttributeWarnings.Any())
            {
                //something wrong, redisplay the page with warnings
                var model = new ShoppingCartModel();
                model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart, validateCheckoutAttributes: true);
                return View(model);
            }

            //everything is OK

            return RedirectToRoute("Checkout");
        }

        [HttpPost, ActionName("Cart")]
        [FormValueRequired("applydiscountcouponcode")]
        public virtual IActionResult ApplyDiscountCoupon(string discountcouponcode, IFormCollection form)
        {
            var model = ApplyDiscountCouponProcess(discountcouponcode, true, form);
            return View("Cart", model);
        }

        [HttpGet]
        [Route("[controller]/[action]/{discountcouponcode}")]
        public virtual IActionResult ApplyDiscountCouponFromClick(string discountcouponcode)
        {
            var model = ApplyDiscountCouponProcess(discountcouponcode);
            return View("Cart", model);
        }

        public virtual ShoppingCartModel ApplyDiscountCouponProcess(string discountcouponcode, bool doParseAndSaveAttributes = false, IFormCollection form = null)
        {
            //trim
            if (discountcouponcode != null)
                discountcouponcode = discountcouponcode.Trim();

            //cart
            List<ShoppingCartItem> cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            //parse and save checkout attributes
            if (doParseAndSaveAttributes)
                ParseAndSaveCheckoutAttributes(cart, form);

            var model = new ShoppingCartModel();
            if (!string.IsNullOrWhiteSpace(discountcouponcode))
            {
                //we find even hidden records here. this way we can display a user-friendly message if it's expired
                var discounts = _discountService.GetAllDiscountsForCaching(couponCode: discountcouponcode, showHidden: true)
                    .Where(d => d.RequiresCouponCode)
                    .ToList();
                if (discounts.Any())
                {
                    var userErrors = new List<string>();
                    if (discounts.Where(x => !x.IsCumulative && x.RequiresCouponCode).Any() && discounts.Where(x => x.RequiresCouponCode).Count() > 1)
                    {
                        userErrors.Add("Los cupones que ya tienes agregados no pueden ser utilizados con el cupón ingresado.");
                        model.DiscountBox.Messages = userErrors;
                    }

                    if (userErrors.Count == 0)
                    {
                        var anyValidDiscount = discounts.Any(discount =>
                        {
                            var validationResult = _discountService.ValidateDiscount(discount, _workContext.CurrentCustomer, new[] { discountcouponcode });
                            userErrors.AddRange(validationResult.Errors);

                            return validationResult.IsValid;
                        });

                        List<DiscountForCaching> discountsWithMinimumAmount = discounts.Where(x => x.OrderMinimumAmount > 0).ToList();
                        string minimumAmountError = null;
                        if (discountsWithMinimumAmount.Any())
                        {
                            foreach (var discount in discountsWithMinimumAmount)
                            {
                                minimumAmountError = ValidateMinimumAmount(discount, cart);
                                if (!string.IsNullOrWhiteSpace(minimumAmountError)) userErrors.Add(minimumAmountError);
                            }
                        }

                        if (anyValidDiscount && string.IsNullOrWhiteSpace(minimumAmountError))
                        {
                            var discountsHavAddProducts = discounts.Where(x => x.ShouldAddProducts &&
                            x.DiscountTypeId == (int)DiscountType.AssignedToSkus).ToList();
                            if (discounts.Where(x => x.ShouldAddProducts).Any())
                            {
                                if (!_workContext.CurrentCustomer.ParseAppliedDiscountCouponCodes().Contains(discountcouponcode.ToLower()))
                                    foreach (var discountHavAddProducts in discountsHavAddProducts)
                                    {
                                        var productIds = _discountService.GetAppliedProductIds(discountHavAddProducts, _workContext.CurrentCustomer).ToArray();
                                        var products = _productService.GetProductsByIds(productIds).Where(x => x.Published).ToList();
                                        if (products.Any())
                                        {
                                            foreach (var product in products)
                                            {
                                                var cartItemWithProduct = _workContext.CurrentCustomer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart &&
                                                    x.ProductId == product.Id).FirstOrDefault();
                                                var newQuantity = (cartItemWithProduct?.Quantity ?? 0) + 1;
                                                decimal customerEnteredPrice = _paymentService.CalculateGroceryPrice(product, cartItemWithProduct, newQuantity, true);
                                                SaveItem(cartItemWithProduct, new List<string>(), product, ShoppingCartType.ShoppingCart, string.Empty, customerEnteredPrice, null, null,
                                                    newQuantity);
                                                cartItemWithProduct = _workContext.CurrentCustomer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart &&
                                                    x.ProductId == product.Id).FirstOrDefault();
                                                //cartItemWithProduct.AppliedByCodeByCoupon = discountcouponcode.ToLower();
                                                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                                            }

                                            //valid, added products
                                            _workContext.CurrentCustomer.ApplyDiscountCouponCode(discountcouponcode);
                                            model.DiscountBox.IsApplied = true;
                                        }
                                    }
                                else
                                {
                                    //valid, already added products
                                    _workContext.CurrentCustomer.ApplyDiscountCouponCode(discountcouponcode);
                                    model.DiscountBox.IsApplied = true;
                                }
                            }
                            else
                            {
                                //valid
                                _workContext.CurrentCustomer.ApplyDiscountCouponCode(discountcouponcode);
                                model.DiscountBox.IsApplied = true;
                            }

                            if (discounts.Where(x => x.CustomerOwnerId > 0).Any())
                            {
                                var customerId = discounts.Where(x => x.CustomerOwnerId > 0).Select(x => x.CustomerOwnerId).FirstOrDefault();
                                var customerOfDiscount = _customerService.GetCustomerById(customerId);
                                model.DiscountBox.Messages.Add($"Se aplicó el código de amigo de {customerOfDiscount.GetFullName()} correctamente.");
                            }
                            else if (model.DiscountBox.IsApplied)
                                model.DiscountBox.Messages.Add(_localizationService.GetResource("ShoppingCart.DiscountCouponCode.Applied"));
                        }
                        else
                        {
                            if (userErrors.Any())
                                //some user errors
                                model.DiscountBox.Messages = userErrors;
                            else
                                //general error text
                                model.DiscountBox.Messages.Add(_localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));
                        }
                    }
                }
                else
                    //discount cannot be found
                    model.DiscountBox.Messages.Add(_localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));
            }
            else
                //empty coupon code
                model.DiscountBox.Messages.Add(_localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));


            if (model.DiscountBox.IsApplied)
            {
                cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
            }

            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);
            return model;
        }

        private string ValidateMinimumAmount(DiscountForCaching discountsWithMinimumAmount, List<ShoppingCartItem> cart)
        {
            string isValid = string.Empty;
            var doneDiscounts = new List<DiscountAppliedTemp>();
            var specialPromotionItems = DiscountHelper.GetSpecialPromotionItems(cart.GetCustomer(), cart,
                _discountService, _specialDiscountTakeXPayYService, _priceCalculationService);
            decimal subtotalWithDiscount = cart.Select(x =>
            {
                var subtotal = _priceCalculationService.GetSubTotal(x, true, out _, out List<DiscountForCaching> appliedDiscounts, out _, out _, doneDiscounts, specialPromotionItems);
                doneDiscounts.AddRange(DiscountHelper.GetListOfCouponsBeingUsed(appliedDiscounts));
                return subtotal;
            }).DefaultIfEmpty().Sum();
            if (subtotalWithDiscount < discountsWithMinimumAmount.OrderMinimumAmount)
            {
                _workContext.CurrentCustomer.RemoveDiscountCouponCode(discountsWithMinimumAmount.CouponCode);
                decimal diff = discountsWithMinimumAmount.OrderMinimumAmount - subtotalWithDiscount;
                return $"El mínimo de compra del cupón es {discountsWithMinimumAmount.OrderMinimumAmount:C} MXN. Agrega {diff:C} MXN más para poder utilizar este cupón.";
            }
            return string.Empty;
        }

        [HttpPost, ActionName("Cart")]
        [FormValueRequired("applygiftcardcouponcode")]
        public virtual IActionResult ApplyGiftCard(string giftcardcouponcode, IFormCollection form)
        {
            //trim
            if (giftcardcouponcode != null)
                giftcardcouponcode = giftcardcouponcode.Trim();

            //cart
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            //parse and save checkout attributes
            ParseAndSaveCheckoutAttributes(cart, form);

            var model = new ShoppingCartModel();
            if (!cart.IsRecurring())
            {
                if (!string.IsNullOrWhiteSpace(giftcardcouponcode))
                {
                    var giftCard = _giftCardService.GetAllGiftCards(giftCardCouponCode: giftcardcouponcode).FirstOrDefault();
                    var isGiftCardValid = giftCard != null && giftCard.IsGiftCardValid();
                    if (isGiftCardValid)
                    {
                        _workContext.CurrentCustomer.ApplyGiftCardCouponCode(giftcardcouponcode);
                        model.GiftCardBox.Message = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.Applied");
                        model.GiftCardBox.IsApplied = true;
                    }
                    else
                    {
                        model.GiftCardBox.Message = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                        model.GiftCardBox.IsApplied = false;
                    }
                }
                else
                {
                    model.GiftCardBox.Message = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                    model.GiftCardBox.IsApplied = false;
                }
            }
            else
            {
                model.GiftCardBox.Message = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.DontWorkWithAutoshipProducts");
                model.GiftCardBox.IsApplied = false;
            }

            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);
            return View(model);
        }

        [PublicAntiForgery]
        [HttpPost]
        public virtual IActionResult GetEstimateShipping(int? countryId, int? stateProvinceId, string zipPostalCode, IFormCollection form)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            //parse and save checkout attributes
            ParseAndSaveCheckoutAttributes(cart, form);

            var errors = new StringBuilder();

            if (string.IsNullOrEmpty(zipPostalCode))
            {
                errors.Append(_localizationService.GetResource("ShoppingCart.EstimateShipping.ZipPostalCode.Required"));
            }

            if (countryId == null || countryId == 0)
            {
                if (errors.Length > 0)
                    errors.Append("<br>");

                errors.Append(_localizationService.GetResource("ShoppingCart.EstimateShipping.Country.Required"));
            }

            if (errors.Length > 0)
            {
                return Content(errors.ToString());
            }

            var model = _shoppingCartModelFactory.PrepareEstimateShippingResultModel(cart, countryId, stateProvinceId, zipPostalCode);
            return PartialView("_EstimateShippingResult", model);
        }

        [HttpPost, ActionName("Cart")]
        [FormValueRequired(FormValueRequirement.StartsWith, "removediscount-")]
        public virtual IActionResult RemoveDiscountCoupon(IFormCollection form)
        {
            var model = new ShoppingCartModel();

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            //get discount identifier
            var discountId = 0;
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("removediscount-", StringComparison.InvariantCultureIgnoreCase))
                    discountId = Convert.ToInt32(formValue.Substring("removediscount-".Length));
            var discount = _discountService.GetDiscountById(discountId);
            if (discount != null)
            {
                //var itemsOfDiscount = cart.Where(x => x.AppliedByCodeByCoupon == discount.CouponCode.ToLower()).ToList();
                //foreach (var sci in itemsOfDiscount)
                //{
                    //if (sci.Quantity <= 1)
                    //{
                    //_shoppingCartService.DeleteShoppingCartItem(sci, ensureOnlyActiveCheckoutAttributes: true);
                    //}
                    //else
                    //{
                    //    var newQuantity = sci.Quantity - 1;
                    //    decimal customerEnteredPrice = _paymentService.CalculateGroceryPrice(sci.Product, sci, 1, true); ; //calculate price

                    //    _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                    //                            sci.Id, sci.AttributesXml, customerEnteredPrice,
                    //                            sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                    //                            newQuantity, true, sci.BuyingBySecondary, sci.SelectedPropertyOption);
                    //}
                //}
                _workContext.CurrentCustomer.RemoveDiscountCouponCode(discount.CouponCode);
                cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
            }
            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);
            return View(model);
        }

        [HttpPost, ActionName("Cart")]
        [FormValueRequired(FormValueRequirement.StartsWith, "removegiftcard-")]
        public virtual IActionResult RemoveGiftCardCode(IFormCollection form)
        {
            var model = new ShoppingCartModel();

            //get gift card identifier
            var giftCardId = 0;
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("removegiftcard-", StringComparison.InvariantCultureIgnoreCase))
                    giftCardId = Convert.ToInt32(formValue.Substring("removegiftcard-".Length));
            var gc = _giftCardService.GetGiftCardById(giftCardId);
            if (gc != null)
                _workContext.CurrentCustomer.RemoveGiftCardCouponCode(gc.GiftCardCouponCode);

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);
            return View(model);
        }

        #endregion

        #region Wishlist

        [HttpsRequirement(SslRequirement.Yes)]
        public virtual IActionResult Wishlist(Guid? customerGuid)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToRoute("HomePage");

            var customer = customerGuid.HasValue ?
                _customerService.GetCustomerByGuid(customerGuid.Value)
                : _workContext.CurrentCustomer;
            if (customer == null)
                return RedirectToRoute("HomePage");
            var cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = new WishlistModel();
            model = _shoppingCartModelFactory.PrepareWishlistModel(model, cart, !customerGuid.HasValue);
            return View(model);
        }

        [HttpPost, ActionName("Wishlist")]
        [FormValueRequired("updatecart")]
        public virtual IActionResult UpdateWishlist(IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToRoute("HomePage");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var allIdsToRemove = form.ContainsKey("removefromcart")
                ? form["removefromcart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList()
                : new List<int>();

            //current warnings <cart item identifier, warnings>
            var innerWarnings = new Dictionary<int, IList<string>>();
            foreach (var sci in cart)
            {
                var remove = allIdsToRemove.Contains(sci.Id);
                if (remove)
                {
                    CheckDiscountRemoveByCartItems(sci, cart);
                    _shoppingCartService.DeleteShoppingCartItem(sci);
                }
                else
                {
                    foreach (var formKey in form.Keys)
                        if (formKey.Equals($"itemquantity{sci.Id}", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int newQuantity;
                            if (int.TryParse(form[formKey], out newQuantity))
                            {
                                var currSciWarnings = _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                    sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice,
                                    sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                    newQuantity, true);
                                innerWarnings.Add(sci.Id, currSciWarnings);
                            }
                            break;
                        }
                }
            }

            //updated wishlist
            cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = new WishlistModel();
            model = _shoppingCartModelFactory.PrepareWishlistModel(model, cart);
            //update current warnings
            foreach (var kvp in innerWarnings)
            {
                //kvp = <cart item identifier, warnings>
                var sciId = kvp.Key;
                var warnings = kvp.Value;
                //find model
                var sciModel = model.Items.FirstOrDefault(x => x.Id == sciId);
                if (sciModel != null)
                    foreach (var w in warnings)
                        if (!sciModel.Warnings.Contains(w))
                            sciModel.Warnings.Add(w);
            }
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult UpdateWishlistAjax(IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToRoute("HomePage");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var allIdsToRemove = form.ContainsKey("removefromcart")
                ? form["removefromcart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList()
                : new List<int>();

            //current warnings <cart item identifier, warnings>
            var innerWarnings = new Dictionary<int, IList<string>>();
            foreach (var sci in cart)
            {
                var remove = allIdsToRemove.Contains(sci.Id);
                if (remove)
                    _shoppingCartService.DeleteShoppingCartItem(sci);
                else
                {
                    foreach (var formKey in form.Keys)
                        if (formKey.Equals($"itemquantity{sci.Id}", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int newQuantity;
                            if (int.TryParse(form[formKey], out newQuantity))
                            {
                                var currSciWarnings = _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                    sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice,
                                    sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                    newQuantity, true);
                                innerWarnings.Add(sci.Id, currSciWarnings);
                            }
                            break;
                        }
                }
            }

            //updated wishlist
            cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = new WishlistModel();
            model = _shoppingCartModelFactory.PrepareWishlistModel(model, cart);
            //update current warnings
            foreach (var kvp in innerWarnings)
            {
                //kvp = <cart item identifier, warnings>
                var sciId = kvp.Key;
                var warnings = kvp.Value;
                //find model
                var sciModel = model.Items.FirstOrDefault(x => x.Id == sciId);
                if (sciModel != null)
                    foreach (var w in warnings)
                        if (!sciModel.Warnings.Contains(w))
                            sciModel.Warnings.Add(w);
            }
            return Ok();
        }

        [HttpPost, ActionName("Wishlist")]
        [FormValueRequired("addtocartbutton")]
        public virtual IActionResult AddItemsToCartFromWishlist(Guid? customerGuid, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToRoute("HomePage");

            var pageCustomer = customerGuid.HasValue
                ? _customerService.GetCustomerByGuid(customerGuid.Value)
                : _workContext.CurrentCustomer;
            if (pageCustomer == null)
                return RedirectToRoute("HomePage");

            var pageCart = pageCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var allWarnings = new List<string>();
            var numberOfAddedItems = 0;
            var allIdsToAdd = form.ContainsKey("addtocart")
                ? form["addtocart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList()
                : new List<int>();
            foreach (var sci in pageCart)
            {
                if (allIdsToAdd.Contains(sci.Id))
                {
                    var warnings = _shoppingCartService.AddToCart(_workContext.CurrentCustomer,
                        sci.Product, ShoppingCartType.ShoppingCart,
                        _storeContext.CurrentStore.Id,
                        sci.AttributesXml, sci.CustomerEnteredPrice,
                        sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, true);
                    if (!warnings.Any())
                        numberOfAddedItems++;
                    if (_shoppingCartSettings.MoveItemsFromWishlistToCart && //settings enabled
                        !customerGuid.HasValue && //own wishlist
                        !warnings.Any()) //no warnings ( already in the cart)
                    {
                        //let's remove the item from wishlist
                        _shoppingCartService.DeleteShoppingCartItem(sci);
                    }
                    allWarnings.AddRange(warnings);
                }
            }

            if (numberOfAddedItems > 0)
            {
                //redirect to the shopping cart page

                if (allWarnings.Any())
                {
                    ErrorNotification(_localizationService.GetResource("Wishlist.AddToCart.Error"), true);
                }

                return RedirectToRoute("ShoppingCart");
            }
            //no items added. redisplay the wishlist page

            if (allWarnings.Any())
            {
                ErrorNotification(_localizationService.GetResource("Wishlist.AddToCart.Error"), false);
            }

            var cart = pageCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = new WishlistModel();
            model = _shoppingCartModelFactory.PrepareWishlistModel(model, cart, !customerGuid.HasValue);
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public virtual IActionResult AddItemsToCartFromWishlistAjax(IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToRoute("HomePage");

            var pageCustomer = _workContext.CurrentCustomer;
            if (pageCustomer == null)
                return RedirectToRoute("HomePage");

            var pageCart = pageCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var allWarnings = new List<string>();
            var numberOfAddedItems = 0;
            var allIdsToAdd = form.ContainsKey("addtocart")
                ? form["addtocart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList()
                : new List<int>();
            foreach (var sci in pageCart)
            {
                if (allIdsToAdd.Contains(sci.Id))
                {
                    var warnings = _shoppingCartService.AddToCart(_workContext.CurrentCustomer,
                        sci.Product, ShoppingCartType.ShoppingCart,
                        _storeContext.CurrentStore.Id,
                        sci.AttributesXml, sci.CustomerEnteredPrice,
                        sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, true);
                    if (!warnings.Any())
                        numberOfAddedItems++;
                    if (_shoppingCartSettings.MoveItemsFromWishlistToCart && //settings enabled
                        true && //own wishlist
                        !warnings.Any()) //no warnings ( already in the cart)
                    {
                        //let's remove the item from wishlist
                        //_shoppingCartService.DeleteShoppingCartItem(sci);
                    }
                    allWarnings.AddRange(warnings);
                }
            }

            if (numberOfAddedItems > 0)
            {
                //redirect to the shopping cart page

                if (allWarnings.Any())
                {
                    ErrorNotification(_localizationService.GetResource("Wishlist.AddToCart.Error"), true);
                }

                return Json(new
                {
                    redirect = Url.RouteUrl("ShoppingCart")
                });
            }
            //no items added. redisplay the wishlist page

            if (allWarnings.Any())
            {
                ErrorNotification(_localizationService.GetResource("Wishlist.AddToCart.Error"), false);
            }

            var cart = pageCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = new WishlistModel();
            model = _shoppingCartModelFactory.PrepareWishlistModel(model, cart, true);
            return Ok();
        }

        [HttpsRequirement(SslRequirement.Yes)]
        public virtual IActionResult EmailWishlist()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist) || !_shoppingCartSettings.EmailWishlistEnabled)
                return RedirectToRoute("HomePage");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            if (!cart.Any())
                return RedirectToRoute("HomePage");

            var model = new WishlistEmailAFriendModel();
            model = _shoppingCartModelFactory.PrepareWishlistEmailAFriendModel(model, false);
            return View(model);
        }

        [HttpPost, ActionName("EmailWishlist")]
        [PublicAntiForgery]
        [FormValueRequired("send-email")]
        [ValidateCaptcha]
        public virtual IActionResult EmailWishlistSend(WishlistEmailAFriendModel model, bool captchaValid)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist) || !_shoppingCartSettings.EmailWishlistEnabled)
                return RedirectToRoute("HomePage");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
                return RedirectToRoute("HomePage");

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnEmailWishlistToFriendPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            //check whether the current customer is guest and ia allowed to email wishlist
            if (_workContext.CurrentCustomer.IsGuest() && !_shoppingCartSettings.AllowAnonymousUsersToEmailWishlist)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Wishlist.EmailAFriend.OnlyRegisteredUsers"));
            }

            if (ModelState.IsValid)
            {
                //email
                _workflowMessageService.SendWishlistEmailAFriendMessage(_workContext.CurrentCustomer,
                        _workContext.WorkingLanguage.Id, model.YourEmailAddress,
                        model.FriendEmail, HtmlHelper.FormatText(model.PersonalMessage, false, true, false, false, false, false));

                model.SuccessfullySent = true;
                model.Result = _localizationService.GetResource("Wishlist.EmailAFriend.SuccessfullySent");

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            model = _shoppingCartModelFactory.PrepareWishlistEmailAFriendModel(model, true);
            return View(model);
        }


        public virtual IActionResult MergeCart(int[] cartInSesionIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var model = new ShoppingCartModel();

            var couponCodes = _workContext.CurrentCustomer.ParseAppliedDiscountCouponCodes();
            var discountsWithMinimumAmount = _discountService.GetAllDiscountsForCaching()
                    .Where(d => d.RequiresCouponCode && couponCodes.Contains(d.CouponCode?.ToLower()) && d.OrderMinimumAmount > 0)
                    .ToList();
            string minimumAmountError = null;
            if (discountsWithMinimumAmount.Any())
            {
                foreach (var discount in discountsWithMinimumAmount)
                {
                    minimumAmountError = ValidateMinimumAmount(discount, cart);
                    if (!string.IsNullOrWhiteSpace(minimumAmountError))
                        _workContext.CurrentCustomer.RemoveDiscountCouponCode(discount.CouponCode);
                }
            }

            //check if cart has deleted products and remove them
            cart = _shoppingCartModelFactory.CheckIfCartHasDeletedProducts(cart);

            string cartInSesionJson = JsonConvert.SerializeObject(cartInSesionIds);
            model.CartInSesionJson = cartInSesionJson;
            cart = cart.Where(x => cartInSesionIds.Contains(x.Id)).ToList();
            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);
            return View(model);
        }

        [HttpPost, ActionName("Cart")]
        [FormValueRequired("merge-products")]
        public virtual IActionResult MergeProductsCart(IFormCollection form, ShoppingCartModel model)
        {
            return RedirectToRoute("ShoppingCart");
        }

        [HttpPost, ActionName("Cart")]
        [FormValueRequired("decline-merge")]
        public virtual IActionResult DeclineMergeProductsCart(IFormCollection form, ShoppingCartModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.CartInSesionJson))
            {
                int[] cartItemsIdToRemove = JsonConvert.DeserializeObject<int[]>(model.CartInSesionJson);
                _shoppingCartService.DeleteMigrateShoppingCart(_workContext.CurrentCustomer, cartItemsIdToRemove);
            }

            return RedirectToRoute("ShoppingCart");
        }

        #endregion
    }

    public class AttributeCombinationInfo
    {
        public int Id { get; set; }
        public bool IsPreSelected { get; set; }
    }
}