using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Logging;
using static Nop.Web.Models.ShoppingCart.ShoppingCartModel;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using Nop.Web.Factories;
using Teed.Plugin.Payments.PaypalMobile.Models;
using Stripe;

namespace Teed.Plugin.Payments.PaypalMobile.Controllers
{
    [Area(AreaNames.Admin)]
    public class PaymentPaypalMobileController : BasePaymentController
    {
        #region Fields

        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly IAddressService _addressService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILogger _logger;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        
        private List<string> ErrorMessages = new List<string>();

        #endregion

        #region Ctor

        public PaymentPaypalMobileController(IStoreService storeService,
            IWorkContext workContext,
            ISettingService settingService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IStoreContext storeContext,
            IStateProvinceService stateProvinceService,
            IAddressService addressService,
            ILogger logger,
            IShoppingCartModelFactory shoppingCartModelFactory)
        {
            _storeService = storeService;
            _workContext = workContext;
            _settingService = settingService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _storeContext = storeContext;
            _addressService = addressService;
            _stateProvinceService = stateProvinceService;
            _logger = logger;
            _shoppingCartModelFactory = shoppingCartModelFactory;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var paymentSettings = _settingService.LoadSetting<PaypalMobilePaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseSandbox = paymentSettings.UseSandbox,
                AdditionalFee = paymentSettings.AdditionalFee,
                AdditionalFeePercentage = paymentSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope,
                TokenizationKeyProduction = paymentSettings.TokenizationKeyProduction,
                TokenizationKeySandbox = paymentSettings.TokenizationKeySandbox,
            };
            if (storeScope > 0)
            {
                model.UseSandbox_OverrideForStore = _settingService.SettingExists(paymentSettings, x => x.UseSandbox, storeScope);
                model.TokenizationKeySandbox_OverrideForStore = _settingService.SettingExists(paymentSettings, x => x.TokenizationKeySandbox, storeScope);
                model.TokenizationKeyProduction_OverrideForStore = _settingService.SettingExists(paymentSettings, x => x.TokenizationKeyProduction, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(paymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(paymentSettings, x => x.AdditionalFeePercentage, storeScope);
            }

            return View("~/Plugins/Payments.PaypalMobile/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var paymentSettings = _settingService.LoadSetting<PaypalMobilePaymentSettings>(storeScope);

            //save settings
            paymentSettings.UseSandbox = model.UseSandbox;
            paymentSettings.TokenizationKeyProduction = model.TokenizationKeyProduction;
            paymentSettings.TokenizationKeySandbox = model.TokenizationKeySandbox;
            paymentSettings.AdditionalFee = model.AdditionalFee;
            paymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(paymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paymentSettings, x => x.TokenizationKeySandbox, model.TokenizationKeySandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paymentSettings, x => x.TokenizationKeyProduction, model.TokenizationKeyProduction_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Api/[controller]/[action]")]
        public IActionResult GetPaypalToken()
        {
            var settings = _settingService.LoadSetting<PaypalMobilePaymentSettings>();
            string key = settings.UseSandbox ? settings.TokenizationKeySandbox : settings.TokenizationKeyProduction;
            return Ok(key);
        }

        #endregion

        #region Private Methos

        

        #endregion
    }

    public class CompletePaymentModel
    {
        public string PaymentId { get; set; }
        public string RememberedCard { get; set; }
        public string PayerId { get; set; }
    }

    public class ProcessPaymentModel
    {
        public string Token { get; set; }
    }
}
