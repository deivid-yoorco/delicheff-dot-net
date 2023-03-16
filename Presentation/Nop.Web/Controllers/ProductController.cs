using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.Rss;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.Security.Captcha;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using Nop.Web.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Nop.Services;
using System.Diagnostics;
using System.Net;

namespace Nop.Web.Controllers
{
    public partial class ProductController : BasePublicController
    {
        #region Fields

        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly ICompareProductsService _compareProductsService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IOrderService _orderService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly CatalogSettings _catalogSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPictureService _pictureService;
        private readonly MediaSettings _mediaSettings;

        #endregion

        #region Ctor

        public ProductController(IProductModelFactory productModelFactory,
            IProductService productService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            ICompareProductsService compareProductsService,
            IWorkflowMessageService workflowMessageService,
            IOrderService orderService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            ICustomerActivityService customerActivityService,
            IEventPublisher eventPublisher,
            IProductAttributeParser productAttributeParser,
            IProductAttributeFormatter productAttributeFormatter,
            CatalogSettings catalogSettings,
            ShoppingCartSettings shoppingCartSettings,
            LocalizationSettings localizationSettings,
            CaptchaSettings captchaSettings,
            IStaticCacheManager cacheManager,
            IProductAttributeService productAttributeService,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IPictureService pictureService,
            MediaSettings mediaSettings)
        {
            this._productModelFactory = productModelFactory;
            this._productService = productService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
            this._recentlyViewedProductsService = recentlyViewedProductsService;
            this._compareProductsService = compareProductsService;
            this._workflowMessageService = workflowMessageService;
            this._orderService = orderService;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
            this._permissionService = permissionService;
            this._customerActivityService = customerActivityService;
            this._eventPublisher = eventPublisher;
            this._productAttributeParser = productAttributeParser;
            this._productAttributeFormatter = productAttributeFormatter;
            this._catalogSettings = catalogSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._localizationSettings = localizationSettings;
            this._captchaSettings = captchaSettings;
            this._cacheManager = cacheManager;
            this._productAttributeService = productAttributeService;
            this._priceCalculationService = priceCalculationService;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
            this._pictureService = pictureService;
            this._mediaSettings = mediaSettings;
        }

        #endregion

        #region Product details page

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult ProductDetails(int productId, int updatecartitemid = 0)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted)
                return InvokeHttp404();

            var notAvailable =
                //published?
                (!product.Published && !_catalogSettings.AllowViewUnpublishedProductPage) ||
                //ACL (access control list) 
                !_aclService.Authorize(product) ||
                //Store mapping
                !_storeMappingService.Authorize(product) ||
                //availability dates
                !product.IsAvailable();
            //Check whether the current user has a "Manage products" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            if (notAvailable && !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return InvokeHttp404();

            //visible individually?
            if (!product.VisibleIndividually)
            {
                //is this one an associated products?
                var parentGroupedProduct = _productService.GetProductById(product.ParentGroupedProductId);
                if (parentGroupedProduct == null)
                    return RedirectToRoute("HomePage");

                return RedirectToRoute("Product", new { SeName = parentGroupedProduct.GetSeName() });
            }

            //update existing shopping cart or wishlist  item?
            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
            {
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                //not found?
                if (updatecartitem == null)
                {
                    return RedirectToRoute("Product", new { SeName = product.GetSeName() });
                }
                //is it this product?
                if (product.Id != updatecartitem.ProductId)
                {
                    return RedirectToRoute("Product", new { SeName = product.GetSeName() });
                }
            }

            //save as recently viewed
            _recentlyViewedProductsService.AddProductToRecentlyViewedList(product.Id);

            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) &&
                _permissionService.Authorize(StandardPermissionProvider.ManageProducts))
            {
                //a vendor should have access only to his products
                if (_workContext.CurrentVendor == null || _workContext.CurrentVendor.Id == product.VendorId)
                {
                    DisplayEditLink(Url.Action("Edit", "Product", new { id = product.Id, area = AreaNames.Admin }));
                }
            }

            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewProduct", _localizationService.GetResource("ActivityLog.PublicStore.ViewProduct"), product.Name);

            //model
            var model = _productModelFactory.PrepareProductDetailsModel(product, updatecartitem, false);
            //template
            var productTemplateViewPath = _productModelFactory.PrepareProductTemplateViewPath(product);

            return View(productTemplateViewPath, model);
        }

        #endregion

        #region Recently viewed products

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult RecentlyViewedProducts()
        {
            if (!_catalogSettings.RecentlyViewedProductsEnabled)
                return Content("");

            var products = _recentlyViewedProductsService.GetRecentlyViewedProducts(_catalogSettings.RecentlyViewedProductsNumber);

            var model = new List<ProductOverviewModel>();
            model.AddRange(_productModelFactory.PrepareProductOverviewModels(products));

            return View(model);
        }

        #endregion

        #region New (recently added) products page

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult NewProducts()
        {
            if (!_catalogSettings.NewProductsEnabled)
                return Content("");

            var products = _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                markedAsNewOnly: true,
                orderBy: ProductSortingEnum.CreatedOn,
                pageSize: _catalogSettings.NewProductsNumber);

            var model = new List<ProductOverviewModel>();
            model.AddRange(_productModelFactory.PrepareProductOverviewModels(products));

            return View(model);
        }

        public virtual IActionResult NewProductsRss()
        {
            var feed = new RssFeed(
                $"{_storeContext.CurrentStore.GetLocalized(x => x.Name)}: New products",
                "Information about products",
                new Uri(_webHelper.GetStoreLocation()),
                DateTime.UtcNow);

            if (!_catalogSettings.NewProductsEnabled)
                return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));

            var items = new List<RssItem>();

            var products = _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                markedAsNewOnly: true,
                orderBy: ProductSortingEnum.CreatedOn,
                pageSize: _catalogSettings.NewProductsNumber);
            foreach (var product in products)
            {
                var productUrl = Url.RouteUrl("Product", new { SeName = product.GetSeName() }, _webHelper.IsCurrentConnectionSecured() ? "https" : "http");
                var productName = product.GetLocalized(x => x.Name);
                var productDescription = product.GetLocalized(x => x.ShortDescription);
                var item = new RssItem(productName, productDescription, new Uri(productUrl), $"urn:store:{_storeContext.CurrentStore.Id}:newProducts:product:{product.Id}", product.CreatedOnUtc);
                items.Add(item);
                //uncomment below if you want to add RSS enclosure for pictures
                //var picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
                //if (picture != null)
                //{
                //    var imageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.ProductDetailsPictureSize);
                //    item.ElementExtensions.Add(new XElement("enclosure", new XAttribute("type", "image/jpeg"), new XAttribute("url", imageUrl), new XAttribute("length", picture.PictureBinary.Length)));
                //}

            }
            feed.Items = items;
            return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));
        }

        #endregion

        #region Product reviews

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult ProductReviews(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                return RedirectToRoute("HomePage");

            var model = new ProductReviewsModel();
            model = _productModelFactory.PrepareProductReviewsModel(model, product);
            //only registered users can leave reviews
            if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
                ModelState.AddModelError("", _localizationService.GetResource("Reviews.OnlyRegisteredUsersCanWriteReviews"));

            if (_catalogSettings.ProductReviewPossibleOnlyAfterPurchasing)
            {
                var hasCompletedOrders = _orderService.SearchOrders(customerId: _workContext.CurrentCustomer.Id,
                    productId: productId,
                    osIds: new List<int> { (int)OrderStatus.Complete },
                    pageSize: 1).Any();
                if (!hasCompletedOrders)
                    ModelState.AddModelError(string.Empty, _localizationService.GetResource("Reviews.ProductReviewPossibleOnlyAfterPurchasing"));
            }

            //default value
            model.AddProductReview.Rating = _catalogSettings.DefaultProductRatingValue;
            return View(model);
        }

        [HttpPost, ActionName("ProductReviews")]
        [PublicAntiForgery]
        [FormValueRequired("add-review")]
        [ValidateCaptcha]
        public virtual IActionResult ProductReviewsAdd(int productId, ProductReviewsModel model, bool captchaValid)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                return RedirectToRoute("HomePage");

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnProductReviewPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Reviews.OnlyRegisteredUsersCanWriteReviews"));
            }

            if (_catalogSettings.ProductReviewPossibleOnlyAfterPurchasing)
            {
                var hasCompletedOrders = _orderService.SearchOrders(customerId: _workContext.CurrentCustomer.Id,
                    productId: productId,
                    osIds: new List<int> { (int)OrderStatus.Complete },
                    pageSize: 1).Any();
                if (!hasCompletedOrders)
                    ModelState.AddModelError(string.Empty, _localizationService.GetResource("Reviews.ProductReviewPossibleOnlyAfterPurchasing"));
            }

            if (ModelState.IsValid)
            {
                //save review
                var rating = model.AddProductReview.Rating;
                if (rating < 1 || rating > 5)
                    rating = _catalogSettings.DefaultProductRatingValue;
                var isApproved = !_catalogSettings.ProductReviewsMustBeApproved;

                var productReview = new ProductReview
                {
                    ProductId = product.Id,
                    CustomerId = _workContext.CurrentCustomer.Id,
                    Title = model.AddProductReview.Title,
                    ReviewText = model.AddProductReview.ReviewText,
                    Rating = rating,
                    HelpfulYesTotal = 0,
                    HelpfulNoTotal = 0,
                    IsApproved = isApproved,
                    CreatedOnUtc = DateTime.UtcNow,
                    StoreId = _storeContext.CurrentStore.Id,
                };
                product.ProductReviews.Add(productReview);
                _productService.UpdateProduct(product);

                //update product totals
                _productService.UpdateProductReviewTotals(product);

                //notify store owner
                if (_catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
                    _workflowMessageService.SendProductReviewNotificationMessage(productReview, _localizationSettings.DefaultAdminLanguageId);

                //activity log
                _customerActivityService.InsertActivity("PublicStore.AddProductReview", _localizationService.GetResource("ActivityLog.PublicStore.AddProductReview"), product.Name);

                //raise event
                if (productReview.IsApproved)
                    _eventPublisher.Publish(new ProductReviewApprovedEvent(productReview));

                model = _productModelFactory.PrepareProductReviewsModel(model, product);
                model.AddProductReview.Title = null;
                model.AddProductReview.ReviewText = null;

                model.AddProductReview.SuccessfullyAdded = true;
                if (!isApproved)
                    model.AddProductReview.Result = _localizationService.GetResource("Reviews.SeeAfterApproving");
                else
                    model.AddProductReview.Result = _localizationService.GetResource("Reviews.SuccessfullyAdded");

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            model = _productModelFactory.PrepareProductReviewsModel(model, product);
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult SetProductReviewHelpfulness(int productReviewId, bool washelpful)
        {
            var productReview = _productService.GetProductReviewById(productReviewId);
            if (productReview == null)
                throw new ArgumentException("No product review found with the specified id");

            if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
            {
                return Json(new
                {
                    Result = _localizationService.GetResource("Reviews.Helpfulness.OnlyRegistered"),
                    TotalYes = productReview.HelpfulYesTotal,
                    TotalNo = productReview.HelpfulNoTotal
                });
            }

            //customers aren't allowed to vote for their own reviews
            if (productReview.CustomerId == _workContext.CurrentCustomer.Id)
            {
                return Json(new
                {
                    Result = _localizationService.GetResource("Reviews.Helpfulness.YourOwnReview"),
                    TotalYes = productReview.HelpfulYesTotal,
                    TotalNo = productReview.HelpfulNoTotal
                });
            }

            //delete previous helpfulness
            var prh = productReview.ProductReviewHelpfulnessEntries
                .FirstOrDefault(x => x.CustomerId == _workContext.CurrentCustomer.Id);
            if (prh != null)
            {
                //existing one
                prh.WasHelpful = washelpful;
            }
            else
            {
                //insert new helpfulness
                prh = new ProductReviewHelpfulness
                {
                    ProductReviewId = productReview.Id,
                    CustomerId = _workContext.CurrentCustomer.Id,
                    WasHelpful = washelpful,
                };
                productReview.ProductReviewHelpfulnessEntries.Add(prh);
            }
            _productService.UpdateProduct(productReview.Product);

            //new totals
            productReview.HelpfulYesTotal = productReview.ProductReviewHelpfulnessEntries.Count(x => x.WasHelpful);
            productReview.HelpfulNoTotal = productReview.ProductReviewHelpfulnessEntries.Count(x => !x.WasHelpful);
            _productService.UpdateProduct(productReview.Product);

            return Json(new
            {
                Result = _localizationService.GetResource("Reviews.Helpfulness.SuccessfullyVoted"),
                TotalYes = productReview.HelpfulYesTotal,
                TotalNo = productReview.HelpfulNoTotal
            });
        }

        public virtual IActionResult CustomerProductReviews(int? pageNumber)
        {
            if (_workContext.CurrentCustomer.IsGuest())
                return Challenge();

            if (!_catalogSettings.ShowProductReviewsTabOnAccountPage)
            {
                return RedirectToRoute("CustomerInfo");
            }

            var model = _productModelFactory.PrepareCustomerProductReviewsModel(pageNumber);
            return View(model);
        }

        #endregion

        #region Email a friend

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult ProductEmailAFriend(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published || !_catalogSettings.EmailAFriendEnabled)
                return RedirectToRoute("HomePage");

            var model = new ProductEmailAFriendModel();
            model = _productModelFactory.PrepareProductEmailAFriendModel(model, product, false);
            return View(model);
        }

        [HttpPost, ActionName("ProductEmailAFriend")]
        [PublicAntiForgery]
        [FormValueRequired("send-email")]
        [ValidateCaptcha]
        public virtual IActionResult ProductEmailAFriendSend(ProductEmailAFriendModel model, bool captchaValid)
        {
            var product = _productService.GetProductById(model.ProductId);
            if (product == null || product.Deleted || !product.Published || !_catalogSettings.EmailAFriendEnabled)
                return RedirectToRoute("HomePage");

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnEmailProductToFriendPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            //check whether the current customer is guest and ia allowed to email a friend
            if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToEmailAFriend)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Products.EmailAFriend.OnlyRegisteredUsers"));
            }

            if (ModelState.IsValid)
            {
                //email
                _workflowMessageService.SendProductEmailAFriendMessage(_workContext.CurrentCustomer,
                        _workContext.WorkingLanguage.Id, product,
                        model.YourEmailAddress, model.FriendEmail,
                        Core.Html.HtmlHelper.FormatText(model.PersonalMessage, false, true, false, false, false, false));

                model = _productModelFactory.PrepareProductEmailAFriendModel(model, product, true);
                model.SuccessfullySent = true;
                model.Result = _localizationService.GetResource("Products.EmailAFriend.SuccessfullySent");

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            model = _productModelFactory.PrepareProductEmailAFriendModel(model, product, true);
            return View(model);
        }

        #endregion

        #region Comparing products

        [HttpPost]
        public virtual IActionResult AddProductToCompareList(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published)
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });

            if (!_catalogSettings.CompareProductsEnabled)
                return Json(new
                {
                    success = false,
                    message = "Product comparison is disabled"
                });

            _compareProductsService.AddProductToCompareList(productId);

            //activity log
            _customerActivityService.InsertActivity("PublicStore.AddToCompareList", _localizationService.GetResource("ActivityLog.PublicStore.AddToCompareList"), product.Name);

            return Json(new
            {
                success = true,
                message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToCompareList.Link"), Url.RouteUrl("CompareProducts"))
                //use the code below (commented) if you want a customer to be automatically redirected to the compare products page
                //redirect = Url.RouteUrl("CompareProducts"),
            });
        }

        public virtual IActionResult RemoveProductFromCompareList(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return RedirectToRoute("HomePage");

            if (!_catalogSettings.CompareProductsEnabled)
                return RedirectToRoute("HomePage");

            _compareProductsService.RemoveProductFromCompareList(productId);

            return RedirectToRoute("CompareProducts");
        }

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult CompareProducts()
        {
            if (!_catalogSettings.CompareProductsEnabled)
                return RedirectToRoute("HomePage");

            var model = new CompareProductsModel
            {
                IncludeShortDescriptionInCompareProducts = _catalogSettings.IncludeShortDescriptionInCompareProducts,
                IncludeFullDescriptionInCompareProducts = _catalogSettings.IncludeFullDescriptionInCompareProducts,
            };

            model.CustomProperties["RecentlyViewProducts"] = "";

            var products = _compareProductsService.GetComparedProducts();

            if (products.Count == 0)
            {
                products = _recentlyViewedProductsService.GetRecentlyViewedProducts(_catalogSettings.RecentlyViewedProductsNumber);
                model.CustomProperties["RecentlyViewProducts"] = "Comparando los productos vistos recientemente";
            }

            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            //prepare model
            _productModelFactory.PrepareProductOverviewModels(products, prepareSpecificationAttributes: true)
                .ToList()
                .ForEach(model.Products.Add);
            return View(model);
        }

        public virtual IActionResult ClearCompareList()
        {
            if (!_catalogSettings.CompareProductsEnabled)
                return RedirectToRoute("HomePage");

            _compareProductsService.ClearCompareProducts();

            return RedirectToRoute("CompareProducts");
        }

        #endregion

        #region Combination
        [HttpGet]
        public virtual IActionResult ProductCombinations(int productId, string attr)
        {
            List<AttrCombinationProduct> combinations = new List<AttrCombinationProduct>();
            var product = _productService.GetProductById(productId);
            var xmlAttr = _productAttributeParser.GenerateAllCombinations(product);

            for (int i = 0; i < xmlAttr.Count; i++)
            {
                var combination = _productAttributeParser.FindProductAttributeCombination(product, xmlAttr[i]);

                if (combination != null)
                {
                    var splXml = Regex.Split(combination.AttributesXml, "<ProductAttributeValue><Value>");
                    List<string> attrIds = new List<string>();

                    for (var m = 1; m < splXml.Length; m++)
                    {
                        var aux = Regex.Split(splXml[m], "</Value>");

                        attrIds.Add(aux[0]);
                    }
                    if (attrIds[0] == attr)
                    {
                        var format = _productAttributeFormatter.FormatAttributes(product, xmlAttr[i]);

                        var attrFormat = Regex.Split(format, "<br />");
                        List<string> attrNames = new List<string>();
                        foreach (var item in attrFormat)
                        {
                            var attrNew = Regex.Split(item, ": ");
                            foreach (var it in attrNew)
                            {
                                attrNames.Add(it);
                            }
                        }

                        if (attrIds.Count > 1 && attrNames.Count > 3)
                        {
                            combinations.Add(new AttrCombinationProduct
                            {
                                AttrId = attrIds[1],
                                AttrName = attrNames[2],
                                AttrNameValue = attrNames[3],
                                StockQuantity = combination.StockQuantity
                            });
                        }
                        else
                        {
                            combinations.Add(new AttrCombinationProduct
                            {
                                AttrId = attrIds[0],
                                AttrName = attrNames[0],
                                AttrNameValue = attrNames[1],
                                StockQuantity = combination.StockQuantity
                            });
                        }
                    }
                }
            }

            var jCombination = Newtonsoft.Json.JsonConvert.SerializeObject(combinations);
            return Ok(jCombination);
        }

        #endregion

        #region Attributes get

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult GetAttributesModels(int productId, bool notOnlyColors = false)
        {
            if (productId <= 0)
                return NoContent();

            var product = _productService.GetProductById(productId);

            //performance optimization
            //We cache a value indicating whether a product has attributes
            IList<ProductAttributeMapping> productAttributeMapping = null;
            var cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_HAS_PRODUCT_ATTRIBUTES_KEY, product.Id);
            var hasProductAttributesCache = _cacheManager.Get<bool?>(cacheKey);
            if (!hasProductAttributesCache.HasValue)
            {
                //no value in the cache yet
                //let's load attributes and cache the result (true/false)
                productAttributeMapping = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
                hasProductAttributesCache = productAttributeMapping.Any();
                _cacheManager.Set(cacheKey, hasProductAttributesCache, 60);
            }
            if (hasProductAttributesCache.Value && productAttributeMapping == null)
            {
                //cache indicates that the product has attributes
                //let's load them
                productAttributeMapping = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            }
            if (productAttributeMapping == null)
            {
                productAttributeMapping = new List<ProductAttributeMapping>();
            }

            var model = new List<ProductDetailsModel.ProductAttributeModel>();

            foreach (var attribute in productAttributeMapping)
            {
                if (attribute.AttributeControlType == AttributeControlType.ColorSquares || 
                    (notOnlyColors && 
                    productAttributeMapping.Where(x => x.AttributeControlType == AttributeControlType.ColorSquares).Count() < 1))
                {
                    var attributeModel = new ProductDetailsModel.ProductAttributeModel
                    {
                        Id = attribute.Id,
                        ProductId = product.Id,
                        ProductAttributeId = attribute.ProductAttributeId,
                        Name = attribute.ProductAttribute.GetLocalized(x => x.Name),
                        Description = attribute.ProductAttribute.GetLocalized(x => x.Description),
                        TextPrompt = attribute.GetLocalized(x => x.TextPrompt),
                        IsRequired = attribute.IsRequired
                    };

                    if (attribute.ShouldHaveValues())
                    {
                        //values
                        var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                        foreach (var attributeValue in attributeValues)
                        {
                            var valueModel = new ProductDetailsModel.ProductAttributeValueModel
                            {
                                Id = attributeValue.Id,
                                Name = attributeValue.GetLocalized(x => x.Name),
                                ColorSquaresRgb = attributeValue.ColorSquaresRgb, //used with "Color squares" attribute type
                                IsPreSelected = attributeValue.IsPreSelected,
                                PriceAdjustmentValue = attributeValue.PriceAdjustment,
                                IsSpecialEdition = TeedCommerceStores.CurrentStore == TeedStores.Lamy ? attributeValue.IsSpecialEdition : false,
                                IsNew = TeedCommerceStores.CurrentStore == TeedStores.Lamy ? attributeValue.IsNew : false,
                                NumberOfCombinations = TeedCommerceStores.CurrentStore == TeedStores.Lamy ? _productAttributeParser.GetCombinationsByProductAttributeValue(product, attributeValue).Count() : 0
                            };
                            attributeModel.Values.Add(valueModel);

                            //picture of a product attribute value
                            valueModel.PictureId = attributeValue.PictureId;
                            valueModel.ImageSquaresPictureModel.ImageUrl = _pictureService.GetPictureUrl(attributeValue.PictureId);
                        }
                    }
                    model.Add(attributeModel);
                }
            }

            return Json(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public virtual IActionResult GetAttributesModels([FromBody] AttributeRequestModel body)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (body.ProductIds.Count <= 0)
                return NoContent();

            var products = _productService.GetAllProductsQuery().Where(x => body.ProductIds.Contains(x.Id)).ToList();
            var result = new List<List<ProductDetailsModel.ProductAttributeModel>>();
            var attributes = _productAttributeService.GetAllAttributeMappings().Where(x => body.ProductIds.Contains(x.ProductId)).ToList();
            var attributeIds = attributes.Select(x => x.Id).ToList();
            var allAttributeValues = _productAttributeService.GetAllProductAttributeValues().Where(x => attributeIds.Contains(x.ProductAttributeMappingId)).ToList();
            var pictureIds = allAttributeValues.Select(x => x.AssociatedProductId).ToList();

            foreach (var product in products)
            {
                //performance optimization
                //We cache a value indicating whether a product has attributes
                IList<ProductAttributeMapping> productAttributeMapping = null;
                var cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_HAS_PRODUCT_ATTRIBUTES_KEY, product.Id);
                var hasProductAttributesCache = _cacheManager.Get<bool?>(cacheKey);
                if (!hasProductAttributesCache.HasValue)
                {
                    //no value in the cache yet
                    //let's load attributes and cache the result (true/false)
                    productAttributeMapping = attributes.Where(x => x.ProductId == product.Id).ToList();
                    hasProductAttributesCache = productAttributeMapping.Any();
                    _cacheManager.Set(cacheKey, hasProductAttributesCache, 60);
                }
                if (hasProductAttributesCache.Value && productAttributeMapping == null)
                {
                    //cache indicates that the product has attributes
                    //let's load them
                    productAttributeMapping = attributes.Where(x => x.ProductId == product.Id).ToList();
                }
                if (productAttributeMapping == null)
                {
                    productAttributeMapping = new List<ProductAttributeMapping>();
                }

                var model = new List<ProductDetailsModel.ProductAttributeModel>();
                foreach (var attribute in productAttributeMapping)
                {
                    if (attribute.AttributeControlType == AttributeControlType.ColorSquares)
                    {
                        var attributeModel = new ProductDetailsModel.ProductAttributeModel
                        {
                            Id = attribute.Id,
                            ProductId = product.Id,
                            ProductAttributeId = attribute.ProductAttributeId,
                            Name = attribute.ProductAttribute.GetLocalized(x => x.Name),
                            Description = attribute.ProductAttribute.GetLocalized(x => x.Description),
                            TextPrompt = attribute.GetLocalized(x => x.TextPrompt),
                            IsRequired = attribute.IsRequired,
                            ProductSeName = product.GetSeName()
                        };

                        if (attribute.ShouldHaveValues())
                        {
                            //values
                            var attributeValues = allAttributeValues.Where(x => x.ProductAttributeMappingId == attribute.Id).ToList();
                            foreach (var attributeValue in attributeValues)
                            {
                                var valueModel = new ProductDetailsModel.ProductAttributeValueModel
                                {
                                    Id = attributeValue.Id,
                                    Name = attributeValue.GetLocalized(x => x.Name),
                                    ColorSquaresRgb = attributeValue.ColorSquaresRgb, //used with "Color squares" attribute type
                                    IsPreSelected = attributeValue.IsPreSelected,
                                    PriceAdjustmentValue = attributeValue.PriceAdjustment,
                                    IsSpecialEdition = TeedCommerceStores.CurrentStore == TeedStores.Lamy && attributeValue.IsSpecialEdition,
                                    IsNew = TeedCommerceStores.CurrentStore == TeedStores.Lamy && attributeValue.IsNew,
                                    NumberOfCombinations = TeedCommerceStores.CurrentStore == TeedStores.Lamy ? _productAttributeParser.GetCombinationsByProductAttributeValue(product, attributeValue).Count() : 1
                                };
                                attributeModel.Values.Add(valueModel);

                                //picture of a product attribute value
                                valueModel.PictureId = attributeValue.PictureId;
                                //valueModel.ImageSquaresPictureModel.ImageUrl = _pictureService.GetPictureUrl(attributeValue.PictureId);
                                //valueModel.ImageSquaresPictureModel.ImageUrl = "";
                            }
                        }
                        model.Add(attributeModel);
                    }
                }

                result.Add(model);
            }

            stopwatch.Stop();
            var test = stopwatch.ElapsedMilliseconds;

            return Json(result);
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult GetImage(int id)
        {
            var picture = _pictureService.GetPictureById(id);
            byte[] bytes = null;
            string mimeType = string.Empty;
            if (picture == null) 
            {
                using (WebClient client = new WebClient())
                {
                    bytes = client.DownloadData(_pictureService.GetDefaultPictureUrl());
                    mimeType = "image/png";
                }
            }
            else
            {
                bytes = picture.PictureBinary;
                mimeType = picture.MimeType;
            }           

            return File(bytes, mimeType);
        }

        #endregion
    }

    public class AttributeRequestModel
    {
        public List<int> ProductIds { get; set; }
    }

    public class AttrCombinationProduct
    {
        public string AttrNameValue { get; set; }
        public string AttrName { get; set; }
        public string AttrId { get; set; }
        public int StockQuantity { get; set; }
    }
}