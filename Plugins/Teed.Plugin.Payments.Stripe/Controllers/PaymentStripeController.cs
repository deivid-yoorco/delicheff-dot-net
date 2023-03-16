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
using Teed.Plugin.Payments.Stripe.Models;
using Stripe;
using Nop.Services.Customers;
using CustomerService = Stripe.CustomerService;

namespace Teed.Plugin.Payments.Stripe.Controllers
{
    [Area(AreaNames.Admin)]
    public class PaymentStripeController : BasePaymentController
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

        public PaymentStripeController(IStoreService storeService,
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
            var StripePaymentSettings = _settingService.LoadSetting<StripePaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseSandbox = StripePaymentSettings.UseSandbox,
                AdditionalFee = StripePaymentSettings.AdditionalFee,
                AdditionalFeePercentage = StripePaymentSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope,
                PublishableKeyProduction = StripePaymentSettings.PublishableKeyProduction,
                PublishableKeySandbox = StripePaymentSettings.PublishableKeySandbox,
                SecretKeySandbox = StripePaymentSettings.SecretKeySandbox,
                SecretKeyProduction = StripePaymentSettings.SecretKeyProduction
            };
            if (storeScope > 0)
            {
                model.UseSandbox_OverrideForStore = _settingService.SettingExists(StripePaymentSettings, x => x.UseSandbox, storeScope);
                model.PublishableKeyProduction_OverrideForStore = _settingService.SettingExists(StripePaymentSettings, x => x.PublishableKeyProduction, storeScope);
                model.PublishableKeySandbox_OverrideForStore = _settingService.SettingExists(StripePaymentSettings, x => x.PublishableKeySandbox, storeScope);
                model.SecretKeyProduction_OverrideForStore = _settingService.SettingExists(StripePaymentSettings, x => x.SecretKeyProduction, storeScope);
                model.SecretKeySandbox_OverrideForStore = _settingService.SettingExists(StripePaymentSettings, x => x.SecretKeySandbox, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(StripePaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(StripePaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            }

            return View("~/Plugins/Payments.Stripe/Views/Configure.cshtml", model);
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
            var StripePaymentSettings = _settingService.LoadSetting<StripePaymentSettings>(storeScope);

            //save settings
            StripePaymentSettings.UseSandbox = model.UseSandbox;
            StripePaymentSettings.PublishableKeyProduction = model.PublishableKeyProduction;
            StripePaymentSettings.PublishableKeySandbox = model.PublishableKeySandbox;
            StripePaymentSettings.SecretKeyProduction = model.SecretKeyProduction;
            StripePaymentSettings.SecretKeySandbox = model.SecretKeySandbox;
            StripePaymentSettings.AdditionalFee = model.AdditionalFee;
            StripePaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(StripePaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(StripePaymentSettings, x => x.PublishableKeyProduction, model.PublishableKeyProduction_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(StripePaymentSettings, x => x.PublishableKeySandbox, model.PublishableKeySandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(StripePaymentSettings, x => x.SecretKeyProduction, model.SecretKeyProduction_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(StripePaymentSettings, x => x.SecretKeySandbox, model.SecretKeySandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(StripePaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(StripePaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetPaymentData()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<StripePaymentSettings>(storeScope);
            string publishableKey = settings.UseSandbox ? settings.PublishableKeySandbox : settings.PublishableKeyProduction;
            return Ok(publishableKey);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(ProcessPaymentModel model)
        {
            ErrorMessages = new List<string>();
            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var stripeSettings = _settingService.LoadSetting<StripePaymentSettings>(storeScope);
            if (stripeSettings == null)
                BadRequest("No fue posible realizar el pago. El método de pago no está activo.");

            //Secret key
            if (stripeSettings.UseSandbox)
                StripeConfiguration.ApiKey = stripeSettings.SecretKeySandbox;
            else
                StripeConfiguration.ApiKey = stripeSettings.SecretKeyProduction;

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var totals = _shoppingCartModelFactory.PrepareOrderTotalsModel(cart, false);
            decimal.TryParse(totals.OrderTotal.Replace("$", ""), out decimal orderTotal);
            var billingAddress = _addressService.GetAddressById(model.BillingAddressId);
            if (billingAddress == null) return BadRequest("El pago no pudo ser procesado: Ocurrió un problema con la dirección de la tarjeta");

            var customerService = new CustomerService();
            var customer = await customerService.CreateAsync(new CustomerCreateOptions()
            {
                Email = billingAddress?.Email,
                Name = billingAddress?.FirstName + " " + billingAddress?.LastName,
                Address = new AddressOptions()
                {
                    City = billingAddress?.City,
                    Line1 = billingAddress?.Address1,
                    Line2 = billingAddress?.Address2,
                    PostalCode = billingAddress?.ZipPostalCode,
                    State = billingAddress?.StateProvince?.Name,
                    Country = billingAddress?.Country?.TwoLetterIsoCode
                },
                Phone = billingAddress?.PhoneNumber
            });

            var cardService = new CardService();
            var card = await cardService.CreateAsync(customer.Id, new CardCreateOptions()
            {
                Source = model.Token
            });

            await cardService.UpdateAsync(customer.Id, card.Id, new CardUpdateOptions()
            {
                AddressCity = billingAddress?.City,
                AddressCountry = billingAddress?.Country?.TwoLetterIsoCode,
                AddressLine1 = billingAddress?.Address1,
                AddressLine2 = billingAddress?.Address2,
                AddressState = billingAddress?.StateProvince?.Name,
                AddressZip = billingAddress?.ZipPostalCode,
                Name = billingAddress?.FirstName + " " + billingAddress?.LastName
            });

            var shippingAddress = _workContext.CurrentCustomer.ShippingAddress;
            AddressOptions stripeShippingAddress = null;
            if (shippingAddress == null)
            {
                stripeShippingAddress = new AddressOptions()
                {
                    City = billingAddress?.City,
                    Line1 = billingAddress?.Address1,
                    Line2 = billingAddress?.Address2,
                    PostalCode = billingAddress?.ZipPostalCode,
                    State = billingAddress?.StateProvince?.Name,
                    Country = billingAddress?.Country?.TwoLetterIsoCode
                };
            }
            else
            {
                stripeShippingAddress = new AddressOptions()
                {
                    City = shippingAddress?.City,
                    Line1 = shippingAddress?.Address1,
                    Line2 = shippingAddress?.Address2,
                    PostalCode = shippingAddress?.ZipPostalCode,
                    State = shippingAddress?.StateProvince?.Name,
                    Country = shippingAddress?.Country?.TwoLetterIsoCode
                };
            }

            ChargeCreateOptions options = new ChargeCreateOptions()
            {
                Amount = (long)(orderTotal * 100), // Debemos multiplicar el monto por 100 porque stripe recibe el monto en centavos
                Currency = "mxn",
                ReceiptEmail = _workContext.CurrentCustomer.Email,
                Description = $"Compra en {_storeContext.CurrentStore.Name}. Cliente: {_workContext.CurrentCustomer.Email}",
                Capture = true,
                Shipping = new ChargeShippingOptions()
                {
                    Address = stripeShippingAddress,
                    Name = shippingAddress?.FirstName + " " + shippingAddress?.LastName,
                    Phone = shippingAddress?.PhoneNumber
                },
                Customer = customer.Id
            };

            var stripeService = new ChargeService();
            try
            {
                Charge charge = await stripeService.CreateAsync(options);
                switch (charge.Status)
                {
                    case "succeeded":
                        return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(charge));
                    default:
                        return BadRequest("El pago no pudo ser procesado. Por favor, selecciona otra forma de pago.");
                }
            }
            catch (Exception e)
            {
                return BadRequest("El pago no pudo ser procesado: " + e.Message);
                throw;
            }
            
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
        public int BillingAddressId { get; set; }
    }
}
