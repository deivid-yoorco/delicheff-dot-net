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
using Teed.Plugin.Payments.OpenPay.Models;
using Nop.Core.Domain.Common;
using Nop.Services.Events;
using Openpay;
using Openpay.Entities.Request;
using Openpay.Entities;
using System.Net.Http.Headers;
using System.Dynamic;
using Nop.Services;

namespace Teed.Plugin.Payments.OpenPay.Controllers
{
    [Area(AreaNames.Admin)]
    public class PaymentOpenPayController : BasePaymentController
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
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IEventPublisher _eventPublisher;

        private List<string> ErrorMessages = new List<string>();

        #endregion

        #region Ctor

        public PaymentOpenPayController(IStoreService storeService,
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
            var openPayPaymentSettings = _settingService.LoadSetting<OpenPayPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseSandbox = openPayPaymentSettings.UseSandbox,
                AdditionalFee = openPayPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = openPayPaymentSettings.AdditionalFeePercentage,

                Production_MerchantId = openPayPaymentSettings.Production_MerchantId,
                Production_PublicKey = openPayPaymentSettings.Production_PublicKey,
                Production_PrivateKey = openPayPaymentSettings.Production_PrivateKey,
                Production_Url = openPayPaymentSettings.Production_Url,

                Sandbox_MerchantId = openPayPaymentSettings.Sandbox_MerchantId,
                Sandbox_PublicKey = openPayPaymentSettings.Sandbox_PublicKey,
                Sandbox_PrivateKey = openPayPaymentSettings.Sandbox_PrivateKey,
                Sandbox_Url = openPayPaymentSettings.Sandbox_Url,

                ActiveStoreScopeConfiguration = storeScope
            };
            model.UseSandbox_OverrideForStore = _settingService.SettingExists(openPayPaymentSettings, x => x.UseSandbox, storeScope);

            model.Production_MerchantId_OverrideForStore = _settingService.SettingExists(openPayPaymentSettings, x => x.Production_MerchantId, storeScope);
            model.Production_PublicKey_OverrideForStore = _settingService.SettingExists(openPayPaymentSettings, x => x.Production_PublicKey, storeScope);
            model.Production_PrivateKey_OverrideForStore = _settingService.SettingExists(openPayPaymentSettings, x => x.Production_PrivateKey, storeScope);
            model.Production_Url_OverrideForStore = _settingService.SettingExists(openPayPaymentSettings, x => x.Production_Url, storeScope);

            model.Sandbox_MerchantId_OverrideForStore = _settingService.SettingExists(openPayPaymentSettings, x => x.Sandbox_MerchantId, storeScope);
            model.Sandbox_PublicKey_OverrideForStore = _settingService.SettingExists(openPayPaymentSettings, x => x.Sandbox_PublicKey, storeScope);
            model.Sandbox_PrivateKey_OverrideForStore = _settingService.SettingExists(openPayPaymentSettings, x => x.Sandbox_PrivateKey, storeScope);
            model.Sandbox_Url_OverrideForStore = _settingService.SettingExists(openPayPaymentSettings, x => x.Sandbox_Url, storeScope);

            model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(openPayPaymentSettings, x => x.AdditionalFeePercentage, storeScope);


            return View("~/Plugins/Payments.OpenPay/Views/Configure.cshtml", model);
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
            var openPayPaymentSettings = _settingService.LoadSetting<OpenPayPaymentSettings>(storeScope);

            //save settings
            openPayPaymentSettings.UseSandbox = model.UseSandbox;
            openPayPaymentSettings.AdditionalFee = model.AdditionalFee;
            openPayPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            openPayPaymentSettings.Production_MerchantId = model.Production_MerchantId;
            openPayPaymentSettings.Production_PublicKey = model.Production_PublicKey;
            openPayPaymentSettings.Production_PrivateKey = model.Production_PrivateKey;
            openPayPaymentSettings.Production_Url = model.Production_Url;

            openPayPaymentSettings.Sandbox_MerchantId = model.Sandbox_MerchantId;
            openPayPaymentSettings.Sandbox_PublicKey = model.Sandbox_PublicKey;
            openPayPaymentSettings.Sandbox_PrivateKey = model.Sandbox_PrivateKey;
            openPayPaymentSettings.Sandbox_Url = model.Sandbox_Url;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(openPayPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(openPayPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(openPayPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(openPayPaymentSettings, x => x.Production_MerchantId, model.Production_MerchantId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(openPayPaymentSettings, x => x.Production_PublicKey, model.Production_PublicKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(openPayPaymentSettings, x => x.Production_PrivateKey, model.Production_PrivateKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(openPayPaymentSettings, x => x.Production_Url, model.Production_Url_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(openPayPaymentSettings, x => x.Sandbox_MerchantId, model.Sandbox_MerchantId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(openPayPaymentSettings, x => x.Sandbox_PublicKey, model.Sandbox_PublicKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(openPayPaymentSettings, x => x.Sandbox_PrivateKey, model.Sandbox_PrivateKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(openPayPaymentSettings, x => x.Sandbox_Url, model.Sandbox_Url_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult SetGetActive(int storeId, bool value, bool set)
        {
            var oof = set;
            var settings = _settingService.LoadSetting<OpenPayPaymentSettings>(storeId);
            if (storeId < 1)
                return BadRequest("Store value has to be bigger than 0");
            if (set)
            {
                var isActive = value;
                settings.IsActive = isActive;
                _settingService.SaveSettingOverridablePerStore(settings, x => x.IsActive, true, storeId, false);
                _settingService.ClearCache();
                return Ok();
            }
            else
                return Ok(settings.IsActive);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetActives()
        {
            var StoreActiveInfo = new List<string>();
            var stores = _storeService.GetAllStores();
            foreach (var store in stores)
            {
                var settings = _settingService.LoadSetting<OpenPayPaymentSettings>(store.Id);
                if (store != null)
                    StoreActiveInfo.Add(
                        store.Name + "<i class=\"fa fa-" + (settings.IsActive ? "check true" : "close false") + "-icon\"></i>");
            }
            return Json(string.Join("", StoreActiveInfo));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetPaymentData()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<OpenPayPaymentSettings>(storeScope);

            if (settings.UseSandbox)
                return Ok(new { merchantId = settings.Sandbox_MerchantId, publicKey = settings.Sandbox_PublicKey, sandboxMode = settings.UseSandbox });
            else
                return Ok(new { merchantId = settings.Production_MerchantId, publicKey = settings.Production_PublicKey, sandboxMode = settings.UseSandbox });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetMsiOptions()
        {
            string options = string.Empty;
            var settings = _settingService.LoadSetting<OpenPayPaymentSettings>();
            return Ok(options);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ProcessPayment(Create3DSTokenDto model)
        {
            var filteredSettings = GetFilteredSettings();
            var order = _orderService.GetOrderById(model.orderId);
            var jsonRequest = string.Empty;
            var jsonCharge = string.Empty;
            var responseJson = string.Empty;

            if (TeedCommerceStores.CurrentStore == TeedStores.ElPomito)
                model.use3ds = true;

            using (HttpClient client = new HttpClient())
            {
                var body = new Create3DSTokenForSending
                {
                    holder_name = model.holder_name,
                    card_number = model.card_number,
                    cvv2 = model.cvv2,
                    expiration_month = model.expiration_month,
                    expiration_year = model.expiration_year,
                    address = new address
                    {
                        city = model.city ?? "",
                        country_code = model.country_code ?? "",
                        postal_code = model.postal_code ?? "",
                        line1 = model.line1 ?? "",
                        line2 = model.line2 ?? "",
                        line3 = model.line3 ?? "",
                        state = model.state ?? "",
                    }
                };
                var url = filteredSettings.siteUrl + $"{filteredSettings.merchantId}/tokens";
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                {
                    jsonRequest = Newtonsoft.Json.JsonConvert.SerializeObject(body);
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{filteredSettings.privateKey}:"));
                    request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

                    request.Content = new StringContent(jsonRequest);
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                    var result = await client.SendAsync(request);
                    var json = await result.Content.ReadAsStringAsync();
                    responseJson = json;
                    if (result.IsSuccessStatusCode)
                    {
                        // card was created correctly
                        AddOrderNote(order, "[OPENPAY SUCCESS] - Card registered correctly to get token" +
                            (model.use3ds ? " (3DS enabled):" : ":")
                            + "\n\n"
                            + json);
                        var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Create3DSTokenResponse>(json);
                        url = filteredSettings.siteUrl + $"{filteredSettings.merchantId}/charges";
                        var redirectUrl = _storeContext.CurrentStore.SecureUrl
                                        //"https://localhost:44387"
                                        + "/Api/PaymentOpenPay/Process3dsPayment";
                        using (var requestCharge = new HttpRequestMessage(new HttpMethod("POST"), url))
                        {
                            if (order != null)
                            {
                                var bodyForCharge = new ChargeWithToken
                                {
                                    source_id = data.id,
                                    method = "card",
                                    amount = order.OrderTotal,
                                    currency = "MXN",
                                    description = $"Pago de orden #{order.Id} ({order.OrderTotal:C}) con OpenPay - Customer Id: {_workContext.CurrentCustomer.Id}",
                                    order_id = model.orderId.ToString(),
                                    device_session_id = model.deviceId,
                                    customer = new customerOfCharge
                                    {
                                        name = model.first_names,
                                        last_name = model.last_names,
                                        email = model.email,
                                        phone_number = model.phone
                                    },
                                    redirect_url = model.use3ds ?
                                            redirectUrl
                                            : string.Empty,
                                    use_3d_secure = model.use3ds
                                };

                                jsonRequest = Newtonsoft.Json.JsonConvert.SerializeObject(bodyForCharge);
                                content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                                base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{filteredSettings.privateKey}:"));
                                requestCharge.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

                                requestCharge.Content = new StringContent(jsonRequest);
                                requestCharge.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                                var resultCharge = await client.SendAsync(requestCharge);
                                jsonCharge = await resultCharge.Content.ReadAsStringAsync();
                                responseJson = jsonCharge;
                                if (resultCharge.IsSuccessStatusCode)
                                {
                                    var chargeRedirectData = Newtonsoft.Json.JsonConvert.DeserializeObject<ChargeWithTokenResponse>(jsonCharge);
                                    if (chargeRedirectData.status == "charge_pending")
                                        return Ok(new
                                        {
                                            redirect = chargeRedirectData.payment_method.type == "redirect",
                                            url = chargeRedirectData.payment_method.url
                                        });
                                    else if (chargeRedirectData.status == "completed")
                                    {
                                        MarkOrderAsPaid(order, responseJson);
                                        return RedirectToAction("Completed", "Checkout", new { area = "", orderId = order.Id });
                                    }

                                    return RedirectToAction("Index", "Home", new { area = "" });
                                }
                                else
                                {
                                    var errors = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseWithErrors>(jsonCharge);
                                    if (errors.error_code == 3005)
                                    {
                                        // Redo payment with 3DS
                                        return Ok(new
                                        {
                                            redoWith3ds = true
                                        });
                                    }
                                    else
                                    {
                                        _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Error,
                                            "[OPENPAY ERROR] - Couldn't process payment\n\n",
                                            "Request:\n" + jsonRequest + "\n\nResponse:\n" + responseJson,
                                            _workContext.CurrentCustomer);
                                        if (errors.error_code == 3001 || errors.error_code == 3002 ||
                                            errors.error_code == 3003 || errors.error_code == 3004)
                                            errors.description = "Tarjeta declinada";
                                        return BadRequest(errors);
                                    }
                                }
                            }
                            else
                                return BadRequest();
                        }
                    }
                    else
                    {
                        _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Error,
                            "[OPENPAY ERROR] - Couldn't register card\n\n",
                            "Request:\n" + jsonRequest + "\n\nResponse:\n" + responseJson,
                            _workContext.CurrentCustomer);
                        var data = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseWithErrors>(json);
                        return BadRequest(data);
                    }
                }
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Api/[controller]/[action]")]
        public async Task<IActionResult> Process3dsPayment(string id)
        {
            using (var httpClient = new HttpClient())
            {
                var filteredSettings = GetFilteredSettings();
                var json = string.Empty;

                var url = filteredSettings.siteUrl + $"{filteredSettings.merchantId}/charges/" + id;
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{filteredSettings.privateKey}:"));
                    request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

                    var response = await httpClient.SendAsync(request);
                    json = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        var data = Newtonsoft.Json.JsonConvert.DeserializeObject<ChargeWithTokenResponse>(json);
                        if (data.status == "completed")
                        {
                            var order = _orderService.GetOrderById(int.Parse(data.order_id));
                            MarkOrderAsPaid(order, json);
                            return RedirectToAction("Completed", "Checkout", new { area = "", orderId = data.order_id });
                        }
                        else if (data.status == "in_progress")
                            return RedirectToAction("Pending", "Checkout", new { area = "", orderId = data.order_id });
                        else if (data.status == "failed")
                            return RedirectToAction("Failure", "Checkout", new { area = "", orderId = data.order_id });
                    }
                    else
                    {
                        _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Error,
                            "[OPENPAY ERROR] - Couldn't check pyament of charge: " + id + "\n\n",
                            "Response:\n" + json,
                            _workContext.CurrentCustomer);
                        var data = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseWithErrors>(json);
                        return BadRequest(data);
                    }
                    return RedirectToAction("Index", "Home", new { area = "" });
                }
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> ProcessPayment(ProcessPaymentModel model)
        //{
        //    try
        //    {
        //        var settings = _settingService.LoadSetting<OpenPayPaymentSettings>();
        //        OpenpayAPI api = new OpenpayAPI(
        //            settings.UseSandbox ? settings.Sandbox_PrivateKey : settings.Production_PrivateKey,
        //            settings.UseSandbox ? settings.Sandbox_MerchantId : settings.Production_MerchantId
        //            );

        //        Openpay.Entities.Customer customer = new Openpay.Entities.Customer();
        //        customer.Name = model.name;
        //        customer.LastName = model.lastName;
        //        customer.PhoneNumber = model.phoneNumber;
        //        customer.Email = model.email;

        //        ChargeRequest request = new ChargeRequest();
        //        request.Method = "card";
        //        request.SourceId = model.tokenId;
        //        request.Amount = model.amount;
        //        request.Description = model.description;
        //        request.DeviceSessionId = model.deviceSessionId;
        //        request.Customer = customer;

        //        // Opcional, si estamos usando puntos
        //        //request.UseCardPoints = useCardPoints;

        //        Charge charge = api.ChargeService.Create(request);
        //        if (charge.Status == "completed" &&
        //            string.IsNullOrEmpty(charge.ErrorMessage))
        //            return Ok(new CompletePaymentModel
        //            {
        //                Status = charge.Status,
        //                PaymentId = charge.Id
        //            });
        //        else
        //            return BadRequest(charge.ErrorMessage);
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest(e);
        //    }
        //}

        protected dynamic GetFilteredSettings()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<OpenPayPaymentSettings>(storeScope);
            var merchantId = settings.UseSandbox ? settings.Sandbox_MerchantId : settings.Production_MerchantId;
            var publicKey = settings.UseSandbox ? settings.Sandbox_PublicKey : settings.Production_PublicKey;
            var privateKey = settings.UseSandbox ? settings.Sandbox_PrivateKey : settings.Production_PrivateKey;
            var siteUrl = settings.UseSandbox ? settings.Sandbox_Url : settings.Production_Url;

            dynamic filteredSettings = new ExpandoObject();
            filteredSettings.UseSandbox = settings.UseSandbox;
            filteredSettings.merchantId = merchantId;
            filteredSettings.publicKey = publicKey;
            filteredSettings.privateKey = privateKey;
            filteredSettings.siteUrl = siteUrl;

            return filteredSettings;
        }

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

        protected virtual void MarkOrderAsPaid(Order order, string json)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            _orderProcessingService.MarkOrderAsPaid(order);

            //add a note
            AddOrderNote(order, "Pago registrado por OpenPay\n\nFinal JSON:\n" + json);
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

    #region 3D token creation

    public class Create3DSTokenForSending
    {
        public string holder_name { get; set; }
        public string card_number { get; set; }
        public string cvv2 { get; set; }
        public string expiration_month { get; set; }
        public string expiration_year { get; set; }
        public address address { get; set; }
    }

    public class Create3DSTokenDto
    {
        public string email { get; set; }
        public string phone { get; set; }
        public string first_names { get; set; }
        public string last_names { get; set; }
        public string holder_name { get; set; }
        public string card_number { get; set; }
        public string cvv2 { get; set; }
        public string expiration_month { get; set; }
        public string expiration_year { get; set; }

        //address from view
        public string city { get; set; }
        public string country_code { get; set; }
        public string postal_code { get; set; }
        public string line1 { get; set; }
        public string line2 { get; set; }
        public string line3 { get; set; }
        public string state { get; set; }
        public string deviceId { get; set; }
        public int orderId { get; set; }
        public bool use3ds { get; set; }
    }

    public class Create3DSTokenResponse
    {
        public string id { get; set; }
        public card card { get; set; }
    }

    public class card
    {
        public string card_number { get; set; }
        public string holder_name { get; set; }
        public string expiration_year { get; set; }
        public string expiration_month { get; set; }
        public string creation_date { get; set; }
        public string brand { get; set; }
        public address address { get; set; }
    }

    public class address
    {
        public string city { get; set; }
        public string country_code { get; set; }
        public string postal_code { get; set; }
        public string line1 { get; set; }
        public string line2 { get; set; }
        public string line3 { get; set; }
        public string state { get; set; }
    }

    #endregion

    #region Charge with token

    public class ChargeWithToken
    {
        public string source_id { get; set; }
        public string method { get; set; }
        public decimal amount { get; set; }
        public string currency { get; set; }
        public string description { get; set; }
        public string order_id { get; set; }
        public string device_session_id { get; set; }
        public string redirect_url { get; set; }
        public bool use_3d_secure { get; set; }
        public customerOfCharge customer { get; set; }
    }

    public class ChargeWithTokenResponse
    {
        public string id { get; set; }
        public decimal amount { get; set; }
        public string authorization { get; set; }
        public string method { get; set; }
        public string operation_type { get; set; }
        public string transaction_type { get; set; }
        public cardAfter3ds card { get; set; }
        public string status { get; set; }
        public string currency { get; set; }
        public exchange_rate exchange_rate { get; set; }
        public string creation_date { get; set; }
        public string operation_date { get; set; }
        public string description { get; set; }
        public string error_message { get; set; }
        public string order_id { get; set; }
        public paymentMethod payment_method { get; set; }
    }

    public class cardAfter3ds
    {
        public string id { get; set; }
        public string type { get; set; }
        public string brand { get; set; }
        public string card_number { get; set; }
        public string holder_name { get; set; }
        public string expiration_year { get; set; }
        public string expiration_month { get; set; }
        public string creation_date { get; set; }
        public bool allows_charges { get; set; }
        public bool allows_payouts { get; set; }
        public string bank_name { get; set; }
        public string bank_code { get; set; }
        public address address { get; set; }
    }

    public class exchange_rate
    {
        public string from { get; set; }
        public string date { get; set; }
        public decimal value { get; set; }
        public string to { get; set; }
    }

    public class customerOfCharge
    {
        public string name { get; set; }
        public string last_name { get; set; }
        public string phone_number { get; set; }
        public string email { get; set; }
    }

    public class paymentMethod
    {
        public string type { get; set; }
        public string url { get; set; }
    }

    #endregion

    public class ResponseWithErrors
    {
        public int http_code { get; set; }
        public int error_code { get; set; }
        public string category { get; set; }
        public string description { get; set; }
        public string request_id { get; set; }
    }

    public class CompletePaymentModel
    {
        public string PaymentId { get; set; }
        public string Status { get; set; }
    }

    public class ProcessPaymentModel
    {
        public string name { get; set; }
        public string lastName { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string tokenId { get; set; }
        public decimal amount { get; set; }
        public string description { get; set; }
        public string deviceSessionId { get; set; }
    }

    public class Store
    {
        public string reference { get; set; }
        public string barcode_url { get; set; }
    }

    public class OpenpayClient
    {
        public string id { get; set; }
        public DateTime creation_date { get; set; }
        public string name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string phone_number { get; set; }
        public string status { get; set; }
        public double balance { get; set; }
        public Store store { get; set; }
        public string clabe { get; set; }
    }


}
