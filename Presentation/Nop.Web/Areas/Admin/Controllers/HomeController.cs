using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Areas.Admin.Models.Home;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Helpers;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Services.Customers;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class HomeController : BaseAdminController
    {
        #region Fields

        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public HomeController(AdminAreaSettings adminAreaSettings,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IWorkContext workContext,
            ICustomerService customerService)
        {
            this._adminAreaSettings = adminAreaSettings;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._settingService = settingService;
            this._workContext = workContext;
            this._customerService = customerService;
        }

        #endregion

        #region Methods

        public virtual IActionResult Index()
        {
            //display a warning to a store owner if there are some error
            if (_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
            {
                var errors = WarningsHelper.GetWarnings().Where(x => x.Level == SystemWarningLevel.Fail).ToList();
                if (errors.Any())
                    WarningNotification(_localizationService.GetResource("Admin.System.Warnings.Errors"), false);
            }

            var userRolesIds = _workContext.CurrentCustomer.CustomerRoles.Select(x => x.Id);
            var franchiseRole = _customerService.GetCustomerRoleBySystemName("franchise");
            if (franchiseRole != null && userRolesIds.Contains(franchiseRole.Id))
                return RedirectToAction("Info", "Franchise", new { area = "admin" });

            var model = new DashboardModel
            {
                IsLoggedInAsVendor = _workContext.CurrentVendor != null
            };
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult NopCommerceNewsHideAdv()
        {
            _adminAreaSettings.HideAdvertisementsOnAdminArea = !_adminAreaSettings.HideAdvertisementsOnAdminArea;
            _settingService.SaveSetting(_adminAreaSettings);

            return Content("Setting changed");
        }

        #endregion
    }
}