using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Teed.Plugin.Payments.PagoFacil.Models;

namespace Teed.Plugin.Payments.PagoFacil.Controller
{
    public class PaymentPagoFacilController : BasePaymentController
    {
        #region Fields

        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public PaymentPagoFacilController(IStoreService storeService,
            IWorkContext workContext,
            ISettingService settingService,
            ILocalizationService localizationService,
            IPermissionService permissionService)
        {
            _storeService = storeService;
            _workContext = workContext;
            _settingService = settingService;
            _permissionService = permissionService;
            _localizationService = localizationService;
        }

        #endregion

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var pagoFacilPaymentSettings = _settingService.LoadSetting<PagoFacilPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                IdServicio = pagoFacilPaymentSettings.IdServicio,
                IdSucursal = pagoFacilPaymentSettings.IdSucursal,
                IdUsuario = pagoFacilPaymentSettings.IdUsuario
            };
            if (storeScope > 0)
            {
                model.IdServicio_OverrideForStore = _settingService.SettingExists(pagoFacilPaymentSettings, x => x.IdServicio, storeScope);
                model.IdSucursal_OverrideForStore = _settingService.SettingExists(pagoFacilPaymentSettings, x => x.IdSucursal, storeScope);
                model.IdUsuario_OverrideForStore = _settingService.SettingExists(pagoFacilPaymentSettings, x => x.IdUsuario, storeScope);
            }

            return View("~/Plugins/Payments.PagoFacil/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var pagoFacilPaymentSettings = _settingService.LoadSetting<PagoFacilPaymentSettings>(storeScope);

            //save settings
            pagoFacilPaymentSettings.IdSucursal = model.IdSucursal;
            pagoFacilPaymentSettings.IdUsuario = model.IdUsuario;
            pagoFacilPaymentSettings.IdServicio = model.IdServicio;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(pagoFacilPaymentSettings, x => x.IdSucursal, model.IdSucursal_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(pagoFacilPaymentSettings, x => x.IdUsuario, model.IdUsuario_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(pagoFacilPaymentSettings, x => x.IdServicio, model.IdServicio_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }
    }
}
