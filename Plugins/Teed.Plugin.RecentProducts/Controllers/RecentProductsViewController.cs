using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Web.Controllers;
using System.IO;
using Teed.Plugin.RecentProducts.Models;
using Nop.Web.Models.Catalog;
using Nop.Services.Catalog;
using Nop.Core;
using System.Linq;
using System.Linq.Dynamic;
using Teed.Plugin.RecentProducts.Security;
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
using Teed.Plugin.RecentProducts.Dtos.Products;
using Nop.Services.Discounts;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Teed.Plugin.RecentProducts.Helpers;

namespace Teed.Plugin.RecentProducts.Controllers
{
    public class RecentProductsViewController : BasePublicController
    {
        private readonly IPermissionService _permissionService;
        private readonly RecentProductsSettings _recentProductsSettings;
        private readonly ISettingService _recentProductsSettingConfiguration;
        private readonly IProductService _productService;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IWebHelper _webHelper;
        private readonly CatalogSettings _catalogSettings;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IPriceCalculationService _priceCalculationService;
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

        public RecentProductsViewController(IPermissionService permissionService, RecentProductsSettings recentProductsSettings, ISettingService settingService,
            IProductService productService, ICatalogModelFactory catalogModelFactory, IProductModelFactory productModelFactory, IWebHelper webHelper, CatalogSettings catalogSettings,
            IWorkContext workContext, ICustomerService customerService, IPriceCalculationService priceCalculationService)
        {
            _permissionService = permissionService;
            _recentProductsSettings = recentProductsSettings;
            _recentProductsSettingConfiguration = settingService;
            _productService = productService;
            _catalogModelFactory = catalogModelFactory;
            _productModelFactory = productModelFactory;
            _webHelper = webHelper;
            _catalogSettings = catalogSettings;
            _workContext = workContext;
            _customerService = customerService;
            _priceCalculationService = priceCalculationService;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("RecentProducts")]
        public IActionResult RecentProducts(int pageNumber = 1)
        {
            var pageIndex = pageNumber;
            //prepare model
            RecentProductsSettings settings = _recentProductsSettingConfiguration.LoadSetting<RecentProductsSettings>();
            var days = DateTime.Now.AddDays(-settings.ProductsBeforeDays);

            var model = _productService.GetAllProductsQuery()
                .Where(x => x.VisibleIndividually && !x.GiftProductEnable && x.Published && !x.Deleted && (x.CreatedOnUtc >= days || x.PublishedDateUtc.Value >= days))
                .OrderByDescending(x => x.PublishedDateUtc == null ? x.CreatedOnUtc : x.PublishedDateUtc).ToList();
            var recentModel = new RecentProductsModel();
            var pagination = new CatalogPagingFilteringModel
            {
                TotalItems = model.Count(),
                PageSize = settings.ProductsPerPage,
                ViewMode = "grid"
            };

            recentModel.configModel = settings;
            var productOverViews = _productModelFactory.PrepareProductOverviewModels(model).ToList();

            var categories = model.SelectMany(x => x.ProductCategories.Where(y => y.Category.ParentCategoryId > 0).Select(y => y.Category))
                .Distinct().ToList();

            var categoryGroups = categories.Select(x => new CategoryProducsGroup
            {
                CategoryId = x.Id,
                CategoryName = x.Name,
                ListProducts = productOverViews.Where(y => y.Product.ProductCategories.Any(z => z.CategoryId == x.Id)).ToList()
            }).ToList();

            recentModel.CategoryProducsGroups = categoryGroups;
            IPagedList<CategoryProducsGroup> categoryProducsGroups = new PagedList<CategoryProducsGroup>(categoryGroups, pageIndex -1, 20);
            recentModel.PagingFilteringContext.LoadPagedList(categoryProducsGroups);
            recentModel.pager = GenerateHtmlStringRP(categoryProducsGroups.TotalCount, categoryProducsGroups.TotalPages, pageIndex -1);

            return View("~/Plugins/Teed.Plugin.RecentProducts/Views/RecentProducts.cshtml", recentModel);
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
