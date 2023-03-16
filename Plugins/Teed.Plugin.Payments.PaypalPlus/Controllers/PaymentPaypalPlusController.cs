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
using Teed.Plugin.Payments.PaypalPlus.Domain;
using Teed.Plugin.Payments.PaypalPlus.Models;
using Teed.Plugin.Payments.PaypalPlus.Services;
using Nop.Services.Logging;
using static Nop.Web.Models.ShoppingCart.ShoppingCartModel;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using Nop.Web.Factories;
using Nop.Services.Discounts;
using Nop.Services.Tax;
using Nop.Services.Catalog;

namespace Teed.Plugin.Payments.PaypalPlus.Controllers
{
    [Area(AreaNames.Admin)]
    public class PaymentPaypalPlusController : BasePaymentController
    {
        #region Fields
        private const string API_URL = "https://api.paypal.com";
        private const string API_SANDBOX_URL = "https://api.sandbox.paypal.com";

        private const string GET_IFRAME = "/v1/payments/payment";

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
        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;

        private readonly RememberedCardsService _rememberedCardsService;

        private List<string> ErrorMessages = new List<string>();

        #endregion

        #region Ctor

        public PaymentPaypalPlusController(IStoreService storeService,
            IWorkContext workContext,
            ISettingService settingService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IStoreContext storeContext,
            IStateProvinceService stateProvinceService,
            IAddressService addressService,
            ILogger logger,
            IShoppingCartModelFactory shoppingCartModelFactory,
            RememberedCardsService rememberedCardsService,
            ITaxService taxService,
            IPriceCalculationService priceCalculationService)
        {
            _storeService = storeService;
            _workContext = workContext;
            _settingService = settingService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _storeContext = storeContext;
            _addressService = addressService;
            _stateProvinceService = stateProvinceService;
            _rememberedCardsService = rememberedCardsService;
            _logger = logger;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _taxService = taxService;
            _priceCalculationService = priceCalculationService;
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
            var paypalPlusPaymentSettings = _settingService.LoadSetting<PaypalPlusPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseSandbox = paypalPlusPaymentSettings.UseSandbox,
                AdditionalFee = paypalPlusPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = paypalPlusPaymentSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope,
                SecretKey = paypalPlusPaymentSettings.SecretKey,
                ClientId = paypalPlusPaymentSettings.ClientId
            };
            if (storeScope > 0)
            {
                model.UseSandbox_OverrideForStore = _settingService.SettingExists(paypalPlusPaymentSettings, x => x.UseSandbox, storeScope);
                model.SecretKey_OverrideForStore = _settingService.SettingExists(paypalPlusPaymentSettings, x => x.SecretKey, storeScope);
                model.ClientId_OverrideForStore = _settingService.SettingExists(paypalPlusPaymentSettings, x => x.ClientId, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(paypalPlusPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(paypalPlusPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            }

            return View("~/Plugins/Payments.PaypalPlus/Views/Configure.cshtml", model);
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
            var paypalPlusPaymentSettings = _settingService.LoadSetting<PaypalPlusPaymentSettings>(storeScope);

            //save settings
            paypalPlusPaymentSettings.UseSandbox = model.UseSandbox;
            paypalPlusPaymentSettings.SecretKey = model.SecretKey;
            paypalPlusPaymentSettings.ClientId = model.ClientId;
            paypalPlusPaymentSettings.AdditionalFee = model.AdditionalFee;
            paypalPlusPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(paypalPlusPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paypalPlusPaymentSettings, x => x.SecretKey, model.SecretKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paypalPlusPaymentSettings, x => x.ClientId, model.ClientId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paypalPlusPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paypalPlusPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetPaymentData(PaypalPlusRequest model)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            model.TotalModel = _shoppingCartModelFactory.PrepareOrderTotalsModel(cart, false);

            ErrorMessages = new List<string>();
            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<PaypalPlusPaymentSettings>(storeScope);

            await CheckToken(settings);

            if (model.AddressId > 0)
            {
                var address = _addressService.GetAddressById(model.AddressId);
                model.Address = new PaypalPlusShippingAddress()
                {
                    Address1 = address.Address1,
                    Address2 = address.Address2,
                    City = address.City,
                    FirstName = address.FirstName,
                    LastName = address.LastName,
                    PhoneNumber = address.PhoneNumber,
                    StateProvinceName = address.StateProvince?.Name,
                    ZipPostalCode = address.ZipPostalCode
                };
            }
            else
            {
                model.Address.StateProvinceName = _stateProvinceService.GetStateProvinceById(model.Address.StateProvinceId).Name;
            }
            model.Items = cart;
            var data = await GetPaymentDataResult(settings, model);

            if (ErrorMessages.Count > 0 || data == null)
            {
                return BadRequest(ErrorMessages);
            }

            var rememberedCards = string.Empty;
            if (_workContext.CurrentCustomer.IsRegistered())
            {
                rememberedCards = _rememberedCardsService.GetAll().Where(x => x.CustomerId == _workContext.CurrentCustomer.Id).Select(x => x.RememberedCardsHash).FirstOrDefault();
            }

            data.isSandbox = settings.UseSandbox;

            return Ok(new { data.links, data.id, rememberedCards, data.isSandbox });
        }

        [HttpPost]
        public async Task<IActionResult> CompletePayment(CompletePaymentModel model)
        {
            ErrorMessages = new List<string>();
            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<PaypalPlusPaymentSettings>(storeScope);

            await CheckToken(settings);

            using (HttpClient client = new HttpClient())
            {
                string url = (settings.UseSandbox ? API_SANDBOX_URL : API_URL) + "/v1/payments/payment/" + model.PaymentId + "/execute";
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + settings.Token);
                var body = new { payer_id = model.PayerId };
                var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                var result = await client.PostAsync(url, content);

                if (result.IsSuccessStatusCode)
                {
                    if (_workContext.CurrentCustomer.IsRegistered())
                    {
                        var userCards = _rememberedCardsService.GetAll().Where(x => x.CustomerId == _workContext.CurrentCustomer.Id).FirstOrDefault();
                        if (userCards == null)
                        {
                            RememberedCards rememberedCards = new RememberedCards()
                            {
                                CustomerId = _workContext.CurrentCustomer.Id,
                                RememberedCardsHash = model.RememberedCard
                            };
                            _rememberedCardsService.Insert(rememberedCards);
                        }
                        else
                        {
                            userCards.RememberedCardsHash = model.RememberedCard;
                            _rememberedCardsService.Update(userCards);
                        }
                    }

                    string json = await result.Content.ReadAsStringAsync();
                    return Ok(json);
                }
                else
                {
                    string json = await result.Content.ReadAsStringAsync();
                    _logger.Error("PaypalPlus: " + json);
                    Debugger.Break();
                    return BadRequest("Ocurrió un problema al procesar el pago.");
                }
            }
        }

        #endregion

        #region Private Methos

        private async Task CheckToken(PaypalPlusPaymentSettings settings)
        {
            if (settings.TokenExpirationDateUtc == default(DateTime) || settings.TokenExpirationDateUtc.AddMinutes(-5) < DateTime.UtcNow)
            {
                await RequestAccessToken(settings);
            }
        }

        private async Task RequestAccessToken(PaypalPlusPaymentSettings settings)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(settings.ClientId + ":" + settings.SecretKey)));
                string url = (settings.UseSandbox ? API_SANDBOX_URL : API_URL) + "/v1/oauth2/token";
                var body = new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") };
                var content = new FormUrlEncodedContent(body);
                var result = await client.PostAsync(url, content);
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    PaypalTokenResponse jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject<PaypalTokenResponse>(json);
                    settings.Token = jsonObject.access_token;
                    settings.TokenExpirationDateUtc = DateTime.UtcNow.AddSeconds(jsonObject.expires_in);
                    _settingService.SaveSetting(settings);
                }
                else
                {
                    string json = await result.Content.ReadAsStringAsync();
                    _logger.Error("PaypalPlus: Error obteniendo el token " + json);
                    ErrorMessages.Add("No se pudo obtener el Token.");
                }
            }
        }

        private async Task<PaypalDataResponse> GetPaymentDataResult(PaypalPlusPaymentSettings settings, PaypalPlusRequest model)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + settings.Token);
                string url = (settings.UseSandbox ? API_SANDBOX_URL : API_URL) + GET_IFRAME;

                // Check if adress is bigger than 100 chars
                var address = model.Address.Address1;
                if (!string.IsNullOrWhiteSpace(address) && address.Length > 100)
                {
                    address = address.Substring(0, 100);
                    model.Address.Address1 = address;
                }

                var body = new
                {
                    intent = "sale",
                    application_context = new { shipping_preference = "SET_PROVIDED_ADDRESS" },
                    payer = GetPayer(model.Address),
                    transactions = GetTransaction(model.Items, model.TotalModel, model.Address),
                    redirect_urls = GetRedirectUrls()
                };
                var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                var result = await client.PostAsync(url, content);
                if (result.IsSuccessStatusCode)
                {
                    var json = await result.Content.ReadAsStringAsync();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<PaypalDataResponse>(json);
                }
                else
                {
                    var json = await result.Content.ReadAsStringAsync();
                    _logger.Error("PaypalPlus: " + json);
                    ErrorMessages.Add("No se pudo conectar a Paypal");
                    return null;
                }
            }
        }

        private object[] GetItems(List<ShoppingCartItem> items, OrderTotalsModel prices)
        {
            var list = new List<object>();
            var itemPrices = new List<decimal>();
            foreach (var item in items)
            {
                var price = _taxService.GetProductPrice(item.Product, _priceCalculationService.GetSubTotal(item, true, out decimal shoppingCartItemDiscountBase, out List<DiscountForCaching> _, out int? maximumDiscountQty, out decimal rewardPointsRequired), out decimal taxRate);
                itemPrices.Add(price);
                list.Add(new
                {
                    name = item.Product.Name,
                    description = item.Product.Name,
                    quantity = 1,
                    price = price.ToString(),
                    sku = item.Product.Sku,
                    currency = "MXN"
                });
            }

            if (prices.OrderTotalDiscountValue > 0)
            {
                list.Add(new
                {
                    name = "Descuento 1",
                    description = "Descuento de la orden",
                    quantity = "1",
                    price = (prices.OrderTotalDiscountValue * -1).ToString(CultureInfo.InvariantCulture),
                    sku = "descuento1",
                    currency = "MXN"
                });
            }

            if (prices.SubTotalDiscountValue > 0)
            {
                list.Add(new
                {
                    name = "Descuento 2",
                    description = "Descuento de la orden",
                    quantity = "1",
                    price = (prices.SubTotalDiscountValue * -1).ToString(CultureInfo.InvariantCulture),
                    sku = "descuento2",
                    currency = "MXN"
                });
            }

            string orderShipping = prices.Shipping?.Replace("$", "");
            decimal.TryParse(orderShipping, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal shippingParsed);
            if (prices.ShippingValue > 0)
            {
                list.Add(new
                {
                    name = "Envío",
                    description = "Envío de la orden",
                    quantity = "1",
                    price = prices.ShippingValue.ToString(CultureInfo.InvariantCulture),
                    sku = "envio",
                    currency = "MXN"
                });
            }

            decimal currentTotal = itemPrices.Sum() - prices.OrderTotalDiscountValue - prices.SubTotalDiscountValue + prices.ShippingValue;
            if (prices.OrderTotalValue != currentTotal)
            {
                decimal diff = prices.OrderTotalValue - currentTotal;
                if (Math.Abs(diff) < 100)
                    list.Add(new
                    {
                        name = "Ajuste",
                        description = "Ajuste de precio",
                        quantity = "1",
                        price = diff.ToString(CultureInfo.InvariantCulture),
                        sku = "ajuste",
                        currency = "MXN"
                    });
            }


            return list.ToArray();
        }

        private object GetPayer(PaypalPlusShippingAddress address)
        {
            return new
            {
                payment_method = "paypal",
                payer_info = new
                {
                    billing_address = new
                    {
                        line1 = string.IsNullOrWhiteSpace(address.Address1) ? address.City : address.Address1,
                        line2 = address.Address2,
                        city = address.City,
                        country_code = "MX",
                        postal_code = address.ZipPostalCode,
                        state = address.StateProvinceName
                    }
                }
            };
        }

        private object[] GetTransaction(List<ShoppingCartItem> items, OrderTotalsModel prices, PaypalPlusShippingAddress address)
        {
            return new[]
            {
                new {
                    amount = new
                    {
                        currency = "MXN",
                        total = prices.OrderTotal.Replace("$", "")
                    },
                    description = "Pedido en " + _storeContext.CurrentStore.Name,
                    payment_options = new
                    {
                        allowed_payment_method = "IMMEDIATE_PAY"
                    },
                    invoice_number = Guid.NewGuid().ToString() + "-" + _workContext.CurrentCustomer.Id,
                    item_list = new
                    {
                        items = GetItems(items, prices),
                        shipping_address = new
                        {
                            recipient_name = address.FirstName + " " + address.LastName,
                            line1 = string.IsNullOrWhiteSpace(address.Address1) ? address.City : address.Address1,
                            line2 = address.Address2,
                            city = address.City,
                            country_code = "MX",
                            postal_code = address.ZipPostalCode,
                            state = address.StateProvinceName,
                            phone = address.PhoneNumber
                        }
                    }
                }
            };
        }

        private object GetRedirectUrls()
        {
            return new
            {
                cancel_url = _storeContext.CurrentStore.SecureUrl,
                return_url = _storeContext.CurrentStore.SecureUrl
            };
        }

        #endregion
    }

    public class CompletePaymentModel
    {
        public string PaymentId { get; set; }
        public string RememberedCard { get; set; }
        public string PayerId { get; set; }
    }

    public class PaypalPlusRequest
    {
        public List<ShoppingCartItem> Items { get; set; }
        public PaypalPlusShippingAddress Address { get; set; }
        public OrderTotalsModel TotalModel { get; set; }
        public int AddressId { get; set; }
    }

    public class PaypalPlusShippingAddress
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string ZipPostalCode { get; set; }
        public int StateProvinceId { get; set; }
        public string StateProvinceName { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class PaypalTokenResponse
    {
        public string scope { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string app_id { get; set; }
        public int expires_in { get; set; }
        public string nonce { get; set; }
    }

    public class PaypalDataResponse
    {
        public string id { get; set; }
        public string create_time { get; set; }
        public List<PaypalDataResponseLink> links { get; set; }
        public bool isSandbox { get; set; }
    }

    public class PaypalDataResponseLink
    {
        public string href { get; set; }
        public string rel { get; set; }
        public string method { get; set; }
    }
}
