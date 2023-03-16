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
using Teed.Plugin.Payments.Stripe.Dtos;
using Nop.Services.Customers;
using Customer = Nop.Core.Domain.Customers.Customer;
using CustomerService = Stripe.CustomerService;

namespace Teed.Plugin.Payments.Stripe.Controllers
{
    public class MobilePaymentController : BasePaymentController
    {
        #region Fields

        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly IAddressService _addressService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILogger _logger;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly ICustomerService _customerService;

        private List<string> ErrorMessages = new List<string>();

        #endregion

        #region Ctor

        public MobilePaymentController(IStoreService storeService,
            ISettingService settingService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IStoreContext storeContext,
            IStateProvinceService stateProvinceService,
            IAddressService addressService,
            ILogger logger,
            IShoppingCartModelFactory shoppingCartModelFactory,
            ICustomerService customerService)
        {
            _storeService = storeService;
            _settingService = settingService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _storeContext = storeContext;
            _addressService = addressService;
            _stateProvinceService = stateProvinceService;
            _logger = logger;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _customerService = customerService;
        }

        #endregion

        #region Methods

        [HttpGet]
        [AllowAnonymous]
        [Route("Api/[controller]/[action]")]
        public IActionResult GetStripeKey()
        {
            var stripeSettings = _settingService.LoadSetting<StripePaymentSettings>();
            if (stripeSettings == null)
                BadRequest("No fue posible realizar el pago. El método de pago no está activo.");

            return Ok(stripeSettings.UseSandbox ? stripeSettings.PublishableKeySandbox : stripeSettings.PublishableKeyProduction);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Api/[controller]/[action]")]
        public async Task<IActionResult> ProcessStripePayment([FromBody] ProcessPaymentDto dto)
        {
            ErrorMessages = new List<string>();

            Customer customer = _customerService.GetCustomerById(dto.UserId);
            if (customer == null) return NotFound();

            var stripeSettings = _settingService.LoadSetting<StripePaymentSettings>();
            if (stripeSettings == null)
                BadRequest("No fue posible realizar el pago. El método de pago no está activo.");

            //Secret key
            if (stripeSettings.UseSandbox)
                StripeConfiguration.ApiKey = stripeSettings.SecretKeySandbox;
            else
                StripeConfiguration.ApiKey = stripeSettings.SecretKeyProduction;

            var cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var totals = _shoppingCartModelFactory.PrepareOrderTotalsModel(cart, false);

            ChargeCreateOptions options = new ChargeCreateOptions()
            {
                Amount = (long)totals.OrderTotalValue * 100, // Debemos multiplicar el monto por 100 porque stripe recibe el monto en centavos
                Currency = "mxn",
                ReceiptEmail = customer.Email,
                Customer = dto.StripeCustomerId,
                Description = $"Compra desde aplicación móvil de {_storeContext.CurrentStore.Name}. Cliente: {customer.Email}).",
                Capture = true
            };

            var stripeService = new ChargeService();
            try
            {
                var charge = await stripeService.CreateAsync(options);
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
                _logger.Error($"[APP MOVIL]: ERROR REALIZANDO PAGO CON STRIPE", e, customer);
                return BadRequest("El pago no pudo ser procesado: " + e.Message);
                throw;
            }

        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Api/[controller]/[action]")]
        public async Task<IActionResult> CreateStripeCustomer(string token, int userId)
        {
            Customer customer = _customerService.GetCustomerById(userId);
            if (customer == null) return NotFound();

            var stripeSettings = _settingService.LoadSetting<StripePaymentSettings>();
            if (stripeSettings == null)
                BadRequest("No fue posible realizar el pago. El método de pago no está activo.");

            //Secret key
            if (stripeSettings.UseSandbox)
                StripeConfiguration.ApiKey = stripeSettings.SecretKeySandbox;
            else
                StripeConfiguration.ApiKey = stripeSettings.SecretKeyProduction;

            var customerOptions = new CustomerCreateOptions
            {
                Email = customer.Email,
                Name = customer.GetFullName(),
                Description = $"Cliente creado desde aplicación móvil de {_storeContext.CurrentStore.Name}."
            };

            CustomerService stripeCustomerService = new CustomerService();
            try
            {
                var result = await stripeCustomerService.CreateAsync(customerOptions);
                var stripeCustomerId = result.Id;
                var cardOptions = new CardCreateOptions
                {
                    Source = token
                };
                var cardService = new CardService();
                var cardResult = await cardService.CreateAsync(stripeCustomerId, cardOptions);
                return Ok(stripeCustomerId);

            }
            catch (Exception e)
            {
                _logger.Error($"[APP MOVIL]: ERROR CREANDO CUSTOMER DE STRIPE", e, customer);
                return BadRequest();
                throw;
            }
        }

        #endregion

        #region Private Methos



        #endregion
    }
}
