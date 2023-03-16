using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System.Linq;
using Teed.Plugin.RecentProducts.Models;
using Teed.Plugin.RecentProducts.Security;

namespace Teed.Plugin.RecentProducts.Controllers
{
    [Area(AreaNames.Admin)]
    public class RecentProductsConfigureController : BasePluginController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly RecentProductsSettings _recentProductsSettings;


        public RecentProductsConfigureController(IPermissionService permissionService,
            RecentProductsSettings recentProductsSettings,
            IWorkContext workContext,
            ISettingService settingService)
        {
            _recentProductsSettings = recentProductsSettings;
            _workContext = workContext;
            _settingService = settingService;
            _permissionService = permissionService;
        }

        #endregion

        [AuthorizeAdmin]
        public IActionResult Configure()
        {
            //whether user has the authority
            if (!_permissionService.Authorize(RecentProductsPermissionProvider.Recent))
                return AccessDeniedView();

            //prepare model
            var model = new ConfigurationModel
            {
                Body = _recentProductsSettings.Body,
                TextMenu = _recentProductsSettings.TextMenu,
                Active = _recentProductsSettings.Active,
                ProductsBeforeDays = _recentProductsSettings.ProductsBeforeDays,
                ProductsPerPage = _recentProductsSettings.ProductsPerPage
            };
            return View("~/Plugins/Teed.Plugin.RecentProducts/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            //whether user has the authority
            if (!_permissionService.Authorize(RecentProductsPermissionProvider.Recent))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _recentProductsSettings.Body = model.Body;
            _recentProductsSettings.Active = model.Active;
            _recentProductsSettings.ProductsBeforeDays = model.ProductsBeforeDays;
            _recentProductsSettings.ProductsPerPage = model.ProductsPerPage;
            _recentProductsSettings.TextMenu = model.TextMenu;
            _settingService.SaveSetting(_recentProductsSettings);

            return Configure();
        }
    }
}