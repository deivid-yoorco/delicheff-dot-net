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
using Teed.Plugin.Payments.NetPay.Models;
using Nop.Core.Domain.Common;
using Nop.Services.Events;

namespace Teed.Plugin.Payments.NetPay.Controllers
{
    [Area(AreaNames.Admin)]
    public class PaymentNetPayController : BasePaymentController
    {
        #region Fields
        private readonly string NETPAY_API_PRODUCTION_URL = "https://suite.netpay.com.mx";
        private readonly string NETPAY_API_SANDBOX_URL = "https://gateway-154.netpaydev.com";
        private readonly string NETPAY_API_CHARGE_URL = "/gateway-ecommerce/v3/charges/";
        private readonly string NETPAY_API_TRANSACTION_URL = "/gateway-ecommerce/v3/transactions/";

        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly IAddressService _addressService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IEventPublisher _eventPublisher;

        private List<string> ErrorMessages = new List<string>();

        #endregion

        #region Ctor

        public PaymentNetPayController(IStoreService storeService,
            IWorkContext workContext,
            ISettingService settingService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IStoreContext storeContext,
            IStateProvinceService stateProvinceService,
            IAddressService addressService,
            IOrderProcessingService orderProcessingService,
            ILogger logger,
            IEventPublisher eventPublisher,
            IOrderService orderService,
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
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _eventPublisher = eventPublisher;
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
            var netPayPaymentSettings = _settingService.LoadSetting<NetPayPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseSandbox = netPayPaymentSettings.UseSandbox,
                AdditionalFee = netPayPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = netPayPaymentSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope,
                PublicKeyProduction = netPayPaymentSettings.PublicKeyProduction,
                SecretKeyProduction = netPayPaymentSettings.SecretKeyProduction,
                PublicKeySandbox = netPayPaymentSettings.PublicKeySandbox,
                SecretKeySandbox = netPayPaymentSettings.SecretKeySandbox,
                MinimumMsiAmount = netPayPaymentSettings.MinimumMsiAmount,
                Allow3Msi = netPayPaymentSettings.Allow3Msi,
                Allow6Msi = netPayPaymentSettings.Allow6Msi,
                Allow9Msi = netPayPaymentSettings.Allow9Msi,
                Allow12Msi = netPayPaymentSettings.Allow12Msi,
                Allow18Msi = netPayPaymentSettings.Allow18Msi,
            };
            if (storeScope > 0)
            {
                model.UseSandbox_OverrideForStore = _settingService.SettingExists(netPayPaymentSettings, x => x.UseSandbox, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(netPayPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(netPayPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.PublicKeyProduction_OverrideForStore = _settingService.SettingExists(netPayPaymentSettings, x => x.PublicKeyProduction, storeScope);
                model.SecretKeyProduction_OverrideForStore = _settingService.SettingExists(netPayPaymentSettings, x => x.SecretKeyProduction, storeScope);
                model.PublicKeySandbox_OverrideForStore = _settingService.SettingExists(netPayPaymentSettings, x => x.PublicKeySandbox, storeScope);
                model.SecretKeySandbox_OverrideForStore = _settingService.SettingExists(netPayPaymentSettings, x => x.SecretKeySandbox, storeScope);
                model.MinimumMsiAmount_OverrideForStore = _settingService.SettingExists(netPayPaymentSettings, x => x.MinimumMsiAmount, storeScope);
                model.Allow3Msi = _settingService.SettingExists(netPayPaymentSettings, x => x.Allow3Msi, storeScope);
                model.Allow6Msi = _settingService.SettingExists(netPayPaymentSettings, x => x.Allow6Msi, storeScope);
                model.Allow9Msi = _settingService.SettingExists(netPayPaymentSettings, x => x.Allow9Msi, storeScope);
                model.Allow12Msi = _settingService.SettingExists(netPayPaymentSettings, x => x.Allow12Msi, storeScope);
                model.Allow18Msi = _settingService.SettingExists(netPayPaymentSettings, x => x.Allow18Msi, storeScope);
            }

            return View("~/Plugins/Payments.NetPay/Views/Configure.cshtml", model);
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
            var netPayPaymentSettings = _settingService.LoadSetting<NetPayPaymentSettings>(storeScope);

            //save settings
            netPayPaymentSettings.UseSandbox = model.UseSandbox;
            netPayPaymentSettings.AdditionalFee = model.AdditionalFee;
            netPayPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            netPayPaymentSettings.SecretKeyProduction = model.SecretKeyProduction;
            netPayPaymentSettings.PublicKeyProduction = model.PublicKeyProduction;
            netPayPaymentSettings.SecretKeySandbox = model.SecretKeySandbox;
            netPayPaymentSettings.PublicKeySandbox = model.PublicKeySandbox;
            netPayPaymentSettings.MinimumMsiAmount = model.MinimumMsiAmount;
            netPayPaymentSettings.Allow3Msi = model.Allow3Msi;
            netPayPaymentSettings.Allow6Msi = model.Allow6Msi;
            netPayPaymentSettings.Allow9Msi = model.Allow9Msi;
            netPayPaymentSettings.Allow12Msi = model.Allow12Msi;
            netPayPaymentSettings.Allow18Msi = model.Allow18Msi;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(netPayPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(netPayPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(netPayPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(netPayPaymentSettings, x => x.PublicKeyProduction, model.PublicKeyProduction_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(netPayPaymentSettings, x => x.SecretKeyProduction, model.SecretKeyProduction_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(netPayPaymentSettings, x => x.PublicKeySandbox, model.PublicKeySandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(netPayPaymentSettings, x => x.SecretKeySandbox, model.SecretKeySandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(netPayPaymentSettings, x => x.MinimumMsiAmount, model.MinimumMsiAmount_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(netPayPaymentSettings, x => x.Allow3Msi, model.Allow3Msi_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(netPayPaymentSettings, x => x.Allow6Msi, model.Allow6Msi_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(netPayPaymentSettings, x => x.Allow9Msi, model.Allow9Msi_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(netPayPaymentSettings, x => x.Allow12Msi, model.Allow12Msi_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(netPayPaymentSettings, x => x.Allow18Msi, model.Allow18Msi_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetPaymentData()
        {
            var settings = _settingService.LoadSetting<NetPayPaymentSettings>();
            return Ok(new { publicKey = settings.UseSandbox ? settings.PublicKeySandbox : settings.PublicKeyProduction, sandboxMode = settings.UseSandbox });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetMsiOptions()
        {
            string options = string.Empty;
            var settings = _settingService.LoadSetting<NetPayPaymentSettings>();
            if (settings.Allow3Msi || settings.Allow6Msi || settings.Allow9Msi || settings.Allow12Msi || settings.Allow18Msi)
            {
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                                   .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                                   .LimitPerStore(_storeContext.CurrentStore.Id)
                                   .ToList();

                var totals = _shoppingCartModelFactory.PrepareOrderTotalsModel(cart, false);
                var orderTotal = totals.OrderTotalValue;

                if (orderTotal >= settings.MinimumMsiAmount)
                {
                    options += "<option value=\"0\">Selecciona una opción...</option>";

                    if (orderTotal >= 300 && settings.Allow3Msi)
                    {
                        options += "<option value=\"3\">3 meses sin intereses</option>";
                    }

                    if (orderTotal >= 600 && settings.Allow6Msi)
                    {
                        options += "<option value=\"6\">6 meses sin intereses</option>";
                    }

                    if (orderTotal >= 900 && settings.Allow9Msi)
                    {
                        options += "<option value=\"9\">9 meses sin intereses</option>";
                    }

                    if (orderTotal >= 1200 && settings.Allow12Msi)
                    {
                        options += "<option value=\"12\">12 meses sin intereses</option>";
                    }

                    if (orderTotal >= 1800 && settings.Allow18Msi)
                    {
                        options += "<option value=\"18\">18 meses sin intereses</option>";
                    }
                }
            }

            return Ok(options);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(ProcessPaymentModel model)
        {
            ErrorMessages = new List<string>();
            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var netPaySettings = _settingService.LoadSetting<NetPayPaymentSettings>(storeScope);
            if (netPaySettings == null)
                BadRequest("No fue posible realizar el pago. El método de pago no está activo.");

            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null) return NotFound();

            Address selectedBillingAddress = _addressService.GetAddressById(model.BillingAddressId);
            if (selectedBillingAddress == null)
            {
                selectedBillingAddress = _workContext.CurrentCustomer.Addresses.FirstOrDefault();
            }

            var body = new
            {
                description = "Carrito de compras de " + _storeContext.CurrentStore.Name,
                source = model.Token,
                paymentMethod = "card",
                amount = order.OrderTotal,
                currency = "MXN",
                installments = model.MsiValue == 0 ? null : new
                {
                    plan = new
                    {
                        count = model.MsiValue,
                        interval = "month"
                    }
                },
                billing = new
                {
                    firstName = selectedBillingAddress.FirstName,
                    lastName = selectedBillingAddress.LastName,
                    email = selectedBillingAddress.Email,
                    phone = selectedBillingAddress.PhoneNumber,
                    address = new
                    {
                        city = selectedBillingAddress.City,
                        country = "MX",
                        postalCode = selectedBillingAddress.ZipPostalCode,
                        state = selectedBillingAddress.StateProvince.Name,
                        street1 = selectedBillingAddress.Address1
                    },
                    merchantReferenceCode = order.Id
                },
                redirect3dsUri = _storeContext.CurrentStore.SecureUrl + "Api/PaymentNetPay/Process3dsPayment"
            };
            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            string url = (netPaySettings.UseSandbox ? NETPAY_API_SANDBOX_URL : NETPAY_API_PRODUCTION_URL) + NETPAY_API_CHARGE_URL;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", netPaySettings.UseSandbox ? netPaySettings.SecretKeySandbox : netPaySettings.SecretKeyProduction);
                var result = await client.PostAsync(url, content);
                string resultJson = await result.Content.ReadAsStringAsync();
                if (result.IsSuccessStatusCode)
                {
                    var resultData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(resultJson);
                    AddOrderNote(order, resultJson);
                    if (resultData.status == "success")
                    {
                        MarkOrderAsPaid(order);
                        return Ok(new ResultModel { ResultJson = resultJson, RedirectUrl = "Checkout/completed/" });
                    }
                    else if (resultData.status == "review")
                    {
                        return Ok(new ResultModel { ResultJson = resultJson, RedirectUrl = resultData.returnUrl });
                    }
                    else if (resultData.status == "failed")
                    {
                        return Ok(new ResultModel { ResultJson = resultJson, RedirectUrl = "Checkout/failure/" });
                    }
                }

                _logger.Error("[NETPAY] Error procesando el pago", new Exception(resultJson), _workContext.CurrentCustomer);
                return BadRequest(resultJson);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Api/[controller]/[action]")]
        public async Task<IActionResult> Process3dsPayment(string transaction_token)
        {
            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var netPaySettings = _settingService.LoadSetting<NetPayPaymentSettings>(storeScope);
            if (netPaySettings == null)
                BadRequest("No fue posible realizar el pago. El método de pago no está activo.");

            string transactionUrl = (netPaySettings.UseSandbox ? NETPAY_API_SANDBOX_URL : NETPAY_API_PRODUCTION_URL) + NETPAY_API_TRANSACTION_URL + transaction_token;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", netPaySettings.UseSandbox ? netPaySettings.SecretKeySandbox : netPaySettings.SecretKeyProduction);
                var transactionResult = await client.GetAsync(transactionUrl);
                string transactionResultJson = await transactionResult.Content.ReadAsStringAsync();
                if (transactionResult.IsSuccessStatusCode)
                {
                    var transactionResultData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(transactionResultJson);
                    string merchantReferenceCode = transactionResultData.merchantReferenceCode;
                    int.TryParse(merchantReferenceCode, out int orderId);
                    Order order = _orderService.GetOrderById(orderId);
                    AddOrderNote(order, transactionResultJson);
                    if (transactionResultData.status == "CHARGEABLE")
                    {
                        string confirmUrl = (netPaySettings.UseSandbox ? NETPAY_API_SANDBOX_URL : NETPAY_API_PRODUCTION_URL) + NETPAY_API_CHARGE_URL + transactionResultData.transactionTokenId + "/confirm";
                        var confirmResult = await client.PostAsync(confirmUrl, null);
                        string confirmResultJson = await confirmResult.Content.ReadAsStringAsync();
                        AddOrderNote(order, confirmResultJson);
                        if (confirmResult.IsSuccessStatusCode)
                        {
                            var confirmResultData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(confirmResultJson);
                            if (order != null && confirmResultData.status == "success")
                            {
                                MarkOrderAsPaid(order);
                                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                            }
                        }
                        else
                        {
                            _logger.Error("[NETPAY] Error confirmando el cargo", new Exception(confirmResultJson), _workContext.CurrentCustomer);
                        }
                    }

                    //MarkOrderAsCancelled(order, "Pago rechazado por NetPay");
                    return RedirectToAction("Failure", "Checkout", new { area = "" });

                }
                else
                {
                    _logger.Error("[NETPAY] Error verificando la transacción", new Exception(transactionResultJson), _workContext.CurrentCustomer);
                }
            }

            return BadRequest();
        }

        #endregion

        #region Private Methos

        protected virtual void AddOrderNote(Order order, string note)
        {
            order.OrderNotes.Add(new OrderNote
            {
                Note = note,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            _orderService.UpdateOrder(order);
        }

        protected virtual void MarkOrderAsPaid(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            _orderProcessingService.MarkOrderAsPaid(order);

            //add a note
            AddOrderNote(order, "Pago registrado por NetPay");
        }

        protected virtual void MarkOrderAsCancelled(Order order, string reason)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            _orderProcessingService.CancelOrder(order, true);
            AddOrderNote(order, "La orden fue cancelada de forma automática" + (string.IsNullOrWhiteSpace(reason) ? "." : $" ({reason})"));
        }

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
        public int OrderId { get; set; }
        public int MsiValue { get; set; }
        public int BillingAddressId { get; set; }
        public string LastFourDigits { get; set; }
        public string Brand { get; set; }
    }

    public class ResultModel
    {
        public string ResultJson { get; set; }
        public string RedirectUrl { get; set; }
    }
}
