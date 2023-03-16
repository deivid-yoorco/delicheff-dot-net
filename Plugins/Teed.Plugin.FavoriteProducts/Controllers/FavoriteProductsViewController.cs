using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Web.Controllers;
using System.IO;
using Teed.Plugin.FavoriteProducts.Models;
using Nop.Web.Models.Catalog;
using Nop.Services.Catalog;
using Nop.Core;
using System.Linq;
using System.Linq.Dynamic;
using Teed.Plugin.FavoriteProducts.Security;
using Nop.Web.Factories;
using System;
using Nop.Core.Domain.Catalog;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Localization;
using System.Text;
using Nop.Web.Framework.Extensions;
using Microsoft.AspNetCore.Routing;
using Nop.Core.Infrastructure;
using Teed.Plugin.FavoriteProducts.Dtos.Products;
using Nop.Services.Discounts;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Teed.Plugin.FavoriteProducts.Helpers;
using Teed.Plugin.FavoriteProducts.Controllers;
using Teed.Plugin.Api.Controllers;
using Nop.Services.Orders;
using System.Data.Services.Client;


namespace Teed.Plugin.FavoriteProducts.Controllers
{
    public class FavoriteProductsViewController : BasePublicController
    {
        private readonly IPermissionService _permissionService;
        private readonly FavoriteProductsSettings _favoriteProductsSettings;
        private readonly ISettingService _favoriteProductsSettingConfiguration;
        private readonly IProductService _productService;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IWebHelper _webHelper;
        private readonly CatalogSettings _catalogSettings;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IDiscountService _discountService;
        private readonly IOrderService _orderService;
        private readonly IStoreContext _storeContext;

        /// <summary>
        /// ViewContext
        /// </summary>
        protected readonly ViewContext viewContext = new ViewContext();
        /// <summary>
        /// A value indicating whether to render empty query string parameters (without values)
        /// </summary>
        protected bool renderEmptyParameters = true;
        /// <summary>
	    /// Page query string prameter name
	    /// </summary>
        protected string pageQueryName = "page";
        /// <summary>
        /// Boolean parameter names
        /// </summary>
        protected IList<string> booleanParameterNames;

        public FavoriteProductsViewController(IPermissionService permissionService, FavoriteProductsSettings favoriteProductsSettings, ISettingService settingService,
            IProductService productService, ICatalogModelFactory catalogModelFactory, IProductModelFactory productModelFactory, IWebHelper webHelper, CatalogSettings catalogSettings,
            IWorkContext workContext, ICustomerService customerService, IPriceCalculationService priceCalculationService, IDiscountService discountService, IOrderService orderService,
            IStoreContext storeContext)
        {
            _permissionService = permissionService;
            _favoriteProductsSettings = favoriteProductsSettings;
            _favoriteProductsSettingConfiguration = settingService;
            _productService = productService;
            _catalogModelFactory = catalogModelFactory;
            _productModelFactory = productModelFactory;
            _webHelper = webHelper;
            _catalogSettings = catalogSettings;
            _workContext = workContext;
            _customerService = customerService;
            _priceCalculationService = priceCalculationService;
            _discountService = discountService;
            _orderService = orderService;
            _storeContext = storeContext;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("FavoriteProducts")]
        public IActionResult FavoriteProducts(int pageNumber = 1)
        {
            if (!_workContext.CurrentCustomer.IsRegistered()) return RedirectToRoute("Login", new {returnUrl = "/FavoriteProducts"});

            var pageIndex = pageNumber;
            FavoriteProductsSettings settings = _favoriteProductsSettingConfiguration.LoadSetting<FavoriteProductsSettings>();
            var products = FavoriteProductsHelper.GetCustomerFavoriteProducts(_workContext.CurrentCustomer.Id, _orderService, _customerService);
            var favoriteProdModel = new FavoriteProductsModel();
            var pagination = new CatalogPagingFilteringModel
            {
                TotalItems = products.Count(),
                PageSize = settings.ProductsPerPage,
                ViewMode = "grid"
            };

            IPagedList<Product>  productsList = new PagedList<Product>(products, pageIndex -1, settings.ProductsPerPage);
            favoriteProdModel.configModel = settings;
            favoriteProdModel.ListProducts = _productModelFactory.PrepareProductOverviewModels(productsList).ToList();
            favoriteProdModel.PagingFilteringContext.LoadPagedList(productsList);
            favoriteProdModel.pager = GenerateHtmlStringRP(productsList.TotalCount, productsList.TotalPages, pageIndex -1);

            return View("~/Plugins/Teed.Plugin.FavoriteProducts/Views/FavoriteProductsView.cshtml", favoriteProdModel);
        }

        /// <summary>
        /// Generate HTML control
        /// </summary>
        /// <returns>HTML control</returns>
        public virtual string GenerateHtmlStringRP(int TotalItems, int TotalPages, int PageIndex)
        {
            if (TotalItems == 0)
                return null;

            int individualPagesDisplayedCount = 5;
            var links = new StringBuilder();
            if (TotalPages > 1)
            {
                    //previous page
                        if (PageIndex > 0)
                    {
                        links.Append(CreatePageLinkRP(PageIndex, "<i class=\"material-icons\">chevron_left</i>", "previous-page"));
                    }

                    //individual pages
                    var firstIndividualPageIndex = GetFirstIndividualPageIndexRP(TotalPages, PageIndex, individualPagesDisplayedCount);
                    var lastIndividualPageIndex = GetLastIndividualPageIndexRP(TotalPages, PageIndex, individualPagesDisplayedCount);
                    for (var i = firstIndividualPageIndex; i <= lastIndividualPageIndex; i++)
                    {
                        if (PageIndex == i)
                        {
                            links.AppendFormat("<li class=\"active\"><a href=\"#!\">{0}</a></li>", (i+1));
                        }
                        else
                        {
                            links.Append(CreatePageLinkRP(i+1, (i+1).ToString(), "individual-page"));
                        }
                    }

                    //next page
                        if ((PageIndex + 1) < TotalPages)
                    {
                        links.Append(CreatePageLinkRP(PageIndex + 2, "<i class=\"material-icons\">chevron_right</i>", "next-page"));
                    }
            }

            var result = links.ToString();
            if (!string.IsNullOrEmpty(result))
            {
                result = "<ul class=\"pagination\">" + result + "</ul>";
            }
            return result;
        }

        /// <summary>
        /// Get first individual page index
        /// </summary>
        /// <returns>Page index</returns>
        protected virtual int GetFirstIndividualPageIndexRP(int TotalPages, int PageIndex, int individualPagesDisplayedCount)
        {
            if ((TotalPages < individualPagesDisplayedCount) ||
                ((PageIndex - (individualPagesDisplayedCount / 2)) < 0))
            {
                return 0;
            }
            if ((PageIndex + (individualPagesDisplayedCount / 2)) >= TotalPages)
            {
                return (TotalPages - individualPagesDisplayedCount);
            }
            return (PageIndex - (individualPagesDisplayedCount / 2));
        }
        /// <summary>
        /// Get last individual page index
        /// </summary>
        /// <returns>Page index</returns>
        protected virtual int GetLastIndividualPageIndexRP(int TotalPages, int PageIndex, int individualPagesDisplayedCount)
        {
            var num = individualPagesDisplayedCount / 2;
            if ((individualPagesDisplayedCount % 2) == 0)
            {
                num--;
            }
            if ((TotalPages < individualPagesDisplayedCount) ||
                ((PageIndex + num) >= TotalPages))
            {
                return (TotalPages - 1);
            }
            if ((PageIndex - (individualPagesDisplayedCount / 2)) < 0)
            {
                return (individualPagesDisplayedCount - 1);
            }
            return (PageIndex + num);
        }

        /// <summary>
        /// Create page link
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <param name="text">Text</param>
        /// <param name="cssClass">CSS class</param>
        /// <returns>Link</returns>
        protected virtual string CreatePageLinkRP(int pageNumber, string text, string cssClass)
        {
            var liBuilder = new TagBuilder("li");
            if (!string.IsNullOrWhiteSpace(cssClass))
                liBuilder.AddCssClass(cssClass);

            var aBuilder = new TagBuilder("a");
            aBuilder.InnerHtml.AppendHtml(text);
            aBuilder.MergeAttribute("href", CreateDefaultUrl(pageNumber));

            liBuilder.InnerHtml.AppendHtml(aBuilder);
            return liBuilder.RenderHtmlContent();
        }

        /// <summary>
        /// Create default URL
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <returns>URL</returns>
        protected virtual string CreateDefaultUrl(int pageNumber)
        {
            var routeValues = new RouteValueDictionary();

            var parametersWithEmptyValues = new List<string>();

            if (pageNumber > 1)
            {
                routeValues[pageQueryName] = pageNumber;
            }
            else
            {
                //SEO. we do not render pageindex query string parameter for the first page
                if (routeValues.ContainsKey(pageQueryName))
                {
                    routeValues.Remove(pageQueryName);
                }
            }

            var webHelper = EngineContext.Current.Resolve<IWebHelper>();
            var url = webHelper.GetThisPageUrl(false);
            foreach (var routeValue in routeValues)
            {
                url = webHelper.ModifyQueryString(url, "pageNumber=" + routeValue.Value, null);
            }
            return url;
        }

    }
}
