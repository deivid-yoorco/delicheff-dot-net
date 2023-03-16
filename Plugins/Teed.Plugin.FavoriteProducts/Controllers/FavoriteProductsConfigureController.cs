using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System.Linq;
using Teed.Plugin.FavoriteProducts.Models;
using Teed.Plugin.FavoriteProducts.Security;

namespace Teed.Plugin.FavoriteProducts.Controllers
{
    [Area(AreaNames.Admin)]
    public class FavoriteProductsConfigureController : BasePluginController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly FavoriteProductsSettings _favoriteProductsSettings;


        public FavoriteProductsConfigureController(IPermissionService permissionService,
            FavoriteProductsSettings favoriteProductsSettings,
            IWorkContext workContext,
            ISettingService settingService)
        {
            _favoriteProductsSettings = favoriteProductsSettings;
            _workContext = workContext;
            _settingService = settingService;
            _permissionService = permissionService;
        }

        #endregion

        [AuthorizeAdmin]
        public IActionResult Configure()
        {
            //whether user has the authority
            if (!_permissionService.Authorize(FavoriteProductsPermissionProvider.Product))
                return AccessDeniedView();

            //prepare model
            var model = new ConfigurationModel
            {
                Body = _favoriteProductsSettings.Body,
                TextMenu = _favoriteProductsSettings.TextMenu,
                Active = _favoriteProductsSettings.Active,
                ProductsPerPage = _favoriteProductsSettings.ProductsPerPage
            };
            return View("~/Plugins/Teed.Plugin.FavoriteProducts/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            //whether user has the authority
            if (!_permissionService.Authorize(FavoriteProductsPermissionProvider.Product))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _favoriteProductsSettings.Body = model.Body;
            _favoriteProductsSettings.Active = model.Active;
            _favoriteProductsSettings.ProductsPerPage = model.ProductsPerPage;
            _favoriteProductsSettings.TextMenu = model.TextMenu;
            _settingService.SaveSetting(_favoriteProductsSettings);

            return Configure();
        }
    }
}