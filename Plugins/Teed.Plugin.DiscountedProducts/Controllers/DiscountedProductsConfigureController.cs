using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System.Linq;
using Teed.Plugin.DiscountedProducts.Models;
using Teed.Plugin.DiscountedProducts.Security;

namespace Teed.Plugin.DiscountedProducts.Controllers
{
    [Area(AreaNames.Admin)]
    public class DiscountedProductsConfigureController : BasePluginController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly DiscountedProductsSettings _discountedProductsSettings;


        public DiscountedProductsConfigureController(IPermissionService permissionService,
            DiscountedProductsSettings discountedProductsSettings,
            IWorkContext workContext,
            ISettingService settingService)
        {
            _discountedProductsSettings = discountedProductsSettings;
            _workContext = workContext;
            _settingService = settingService;
            _permissionService = permissionService;
        }

        #endregion

        [AuthorizeAdmin]
        public IActionResult Configure()
        {
            //whether user has the authority
            if (!_permissionService.Authorize(DiscountedProductsPermissionProvider.Product))
                return AccessDeniedView();

            //prepare model
            var model = new ConfigurationModel
            {
                Body = _discountedProductsSettings.Body,
                TextMenu = _discountedProductsSettings.TextMenu,
                Active = _discountedProductsSettings.Active,
                ProductsPerPage = _discountedProductsSettings.ProductsPerPage
            };
            return View("~/Plugins/Teed.Plugin.DiscountedProducts/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            //whether user has the authority
            if (!_permissionService.Authorize(DiscountedProductsPermissionProvider.Product))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _discountedProductsSettings.Body = model.Body;
            _discountedProductsSettings.Active = model.Active;
            _discountedProductsSettings.ProductsPerPage = model.ProductsPerPage;
            _discountedProductsSettings.TextMenu = model.TextMenu;
            _settingService.SaveSetting(_discountedProductsSettings);

            return Configure();
        }
    }
}