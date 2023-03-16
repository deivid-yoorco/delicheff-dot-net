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
using Teed.Plugin.Payments.Visa.Models;
using Teed.Plugin.Payments.Visa.Utils;
using Nop.Web.Framework.Security;
using Nop.Services.Discounts;
using Nop.Services.Tax;
using Nop.Services.Catalog;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Net;
using System.Security.Authentication;

namespace Teed.Plugin.Payments.Visa.Controllers
{
    [Area(AreaNames.Admin)]
    public class PaymentVisaController : BasePaymentController
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
        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;

        private List<string> ErrorMessages = new List<string>();

        #endregion

        #region Ctor

        public PaymentVisaController(IStoreService storeService,
            IWorkContext workContext,
            ISettingService settingService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IStoreContext storeContext,
            IStateProvinceService stateProvinceService,
            IAddressService addressService,
            ILogger logger,
            IShoppingCartModelFactory shoppingCartModelFactory,
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
            var visaPaymentSettings = _settingService.LoadSetting<VisaPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseSandbox = visaPaymentSettings.UseSandbox,
                ApiKeyProduction = visaPaymentSettings.ApiKeyProduction,
                ApiKeySandbox = visaPaymentSettings.ApiKeySandbox,
                MerchantId = visaPaymentSettings.MerchantId,
                SharedSecretKeyProduction = visaPaymentSettings.SharedSecretKeyProduction,
                SharedSecretKeySandbox = visaPaymentSettings.SharedSecretKeySandbox,
                OrganizationIdProduction = visaPaymentSettings.OrganizationIdProduction,
                OrganizationIdSandbox = visaPaymentSettings.OrganizationIdSandbox
            };
            if (storeScope > 0)
            {
                model.UseSandbox_OverrideForStore = _settingService.SettingExists(visaPaymentSettings, x => x.UseSandbox, storeScope);
                model.SharedSecretKeyProduction_OverrideForStore = _settingService.SettingExists(visaPaymentSettings, x => x.SharedSecretKeyProduction, storeScope);
                model.SharedSecretKeySandbox_OverrideForStore = _settingService.SettingExists(visaPaymentSettings, x => x.SharedSecretKeySandbox, storeScope);
                model.ApiKeyProduction_OverrideForStore = _settingService.SettingExists(visaPaymentSettings, x => x.ApiKeyProduction, storeScope);
                model.ApiKeySandbox_OverrideForStore = _settingService.SettingExists(visaPaymentSettings, x => x.ApiKeySandbox, storeScope);
                model.MerchantId_OverrideForStore = _settingService.SettingExists(visaPaymentSettings, x => x.MerchantId, storeScope);
                model.OrganizationIdProduction_OverrideForStore = _settingService.SettingExists(visaPaymentSettings, x => x.OrganizationIdProduction, storeScope);
                model.OrganizationIdSandbox_OverrideForStore = _settingService.SettingExists(visaPaymentSettings, x => x.OrganizationIdSandbox, storeScope);
            }

            return View("~/Plugins/Payments.Visa/Views/Configure.cshtml", model);
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
            var visaPaymentSettings = _settingService.LoadSetting<VisaPaymentSettings>(storeScope);

            //save settings
            visaPaymentSettings.UseSandbox = model.UseSandbox;
            visaPaymentSettings.ApiKeySandbox = model.ApiKeySandbox;
            visaPaymentSettings.SharedSecretKeySandbox = model.SharedSecretKeySandbox;
            visaPaymentSettings.ApiKeyProduction = model.ApiKeyProduction;
            visaPaymentSettings.SharedSecretKeyProduction = model.SharedSecretKeyProduction;
            visaPaymentSettings.MerchantId = model.MerchantId;
            visaPaymentSettings.OrganizationIdProduction = model.OrganizationIdProduction;
            visaPaymentSettings.OrganizationIdSandbox = model.OrganizationIdSandbox;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(visaPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(visaPaymentSettings, x => x.ApiKeyProduction, model.ApiKeyProduction_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(visaPaymentSettings, x => x.SharedSecretKeyProduction, model.SharedSecretKeyProduction_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(visaPaymentSettings, x => x.ApiKeySandbox, model.ApiKeySandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(visaPaymentSettings, x => x.SharedSecretKeySandbox, model.SharedSecretKeySandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(visaPaymentSettings, x => x.MerchantId, model.MerchantId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(visaPaymentSettings, x => x.OrganizationIdProduction, model.OrganizationIdProduction_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(visaPaymentSettings, x => x.OrganizationIdSandbox, model.OrganizationIdSandbox_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetPaymentData()
        {
            //var settings = _settingService.LoadSetting<VisaPaymentSettings>();
            //string publishableKey = settings.UseSandbox ? settings.PublishableKeySandbox : settings.PublishableKeyProduction;
            //return Ok(publishableKey);
            return Ok();
        }

        [HttpPost]
        [ValidateIpAddress]
        [ValidateAntiForgeryToken]
        [Route("[controller]/[action]")]
        [HttpsRequirement(SslRequirement.Yes)]
        public async Task<IActionResult> CaptureWebPayment(CaptureWebPaymentModel model)
        {
            var request = string.Empty;
            var settings = _settingService.LoadSetting<VisaPaymentSettings>();
            try
            {
                if (settings == null)
                    BadRequest("No fue posible procesar el pago. El método de pago no está activo.");

                _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Information, "[VISA INFO] - Settings taken",
                    "",
                    _workContext.CurrentCustomer);

                var customer = _workContext.CurrentCustomer;
                var cart = customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart);
                var totalsModel = _shoppingCartModelFactory.PrepareOrderTotalsModel(cart.ToList(), false);
                var billingAddress = customer.Addresses.Where(x => x.Id == model.SelectedBillingAddressId).FirstOrDefault();
                var shippingAddress = customer.Addresses.LastOrDefault();

                var body = new
                {
                    clientReferenceInformation = new
                    {
                        code = customer.Id.ToString()
                    },
                    orderInformation = new
                    {
                        amountDetails = new
                        {
                            totalAmount = totalsModel.OrderTotalValue.ToString(),
                            currency = "MXN"
                        },
                        lineItems = cart.Select(x => new
                        {
                            productCode = x.Product.ProductCategories.Where(y => y.Category?.ParentCategoryId == 0).FirstOrDefault()?.Category?.Name,
                            productName = x.Product.Name.Substring(0, x.Product.Name.Length > 255 ? 255 : x.Product.Name.Length),
                            productSku = x.Product.Sku.Substring(0, x.Product.Sku.Length > 255 ? 255 : x.Product.Sku.Length),
                            totalAmount = _taxService.GetProductPrice(x.Product, _priceCalculationService.GetSubTotal(x, true, out decimal shoppingCartItemDiscountBase, out List<DiscountForCaching> _, out int? maximumDiscountQty, out decimal rewardPointsRequired), out decimal taxRate).ToString(),
                            unitPrice = _taxService.GetProductPrice(x.Product, _priceCalculationService.GetSubTotal(x, true, out decimal shoppingCartItemDiscountBase2, out List<DiscountForCaching> _, out int? maximumDiscountQty2, out decimal rewardPointsRequired2), out decimal taxRate2).ToString(),
                            quantity = Nop.Services.TeedCommerceStores.CurrentStore != Nop.Services.TeedStores.CentralEnLinea ? x.Quantity : 1,
                            productDescription = x.SelectedPropertyOption
                        }).ToList(),
                        shipTo = shippingAddress == null ? null : new
                        {
                            firstName = shippingAddress.FirstName,
                            lastName = shippingAddress.LastName,
                            address1 = shippingAddress.Address1,
                            address2 = shippingAddress.Address2,
                            locality = shippingAddress.City,
                            administrativeArea = shippingAddress.StateProvince?.Name,
                            postalCode = shippingAddress.ZipPostalCode,
                            country = shippingAddress.Country?.Name,
                            phoneNumber = shippingAddress.PhoneNumber
                        },
                        billTo = billingAddress == null ? null : new
                        {
                            firstName = billingAddress.FirstName,
                            lastName = billingAddress.LastName,
                            address1 = billingAddress.Address1,
                            address2 = billingAddress.Address2,
                            locality = billingAddress.City,
                            administrativeArea = billingAddress.StateProvince?.Name,
                            postalCode = billingAddress.ZipPostalCode,
                            country = billingAddress.Country?.Name,
                            phoneNumber = billingAddress.PhoneNumber,
                            email = billingAddress.Email
                        }
                    },
                    processingInformation = new
                    {
                        capture = "true",
                        commerceIndicator = "internet",
                        authorizationOptions = new
                        {
                            initiator = new
                            {
                                storedCredentialUsed = false
                            }
                        }
                    },
                    deviceInformation = new
                    {
                        fingerprintSessionId = model.SessionId
                    },
                    paymentInformation = new
                    {
                        card = new
                        {
                            expirationYear = model.Card.ExpirationYear,
                            number = model.Card.CardNumber.Replace(" ", ""),
                            securityCode = model.Card.SecurityCode,
                            expirationMonth = model.Card.ExpirationMonth,
                            type = model.Card.CardNumber.Trim().FirstOrDefault() == '6' ? "042" : null // If card's number start with 6, we set 042 as type to make vales card work
                        }
                    }
                };

                _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Information, "[VISA INFO] - Body created",
                    "",
                    _workContext.CurrentCustomer);

                request = Newtonsoft.Json.JsonConvert.SerializeObject(body);
                var responseResult = await CallCyberSourceAPI(request, settings);

                if ((int)responseResult.ResponseCode >= 200 && (int)responseResult.ResponseCode <= 299)
                {
                    var resultObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseResult.ResultJson);

                    _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Information, "[VISA INFO] - Call made",
                        responseResult.ResultJson,
                        _workContext.CurrentCustomer);
                    if (resultObject.status == "AUTHORIZED")
                    {
                        return Ok(responseResult.ResultJson);
                    }
                }
                else
                {
                    _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Error,
                        "[VISA ERROR] - Couldn't process payment\n\n",
                        "Request:\n" + request + "\n\nResponse:\n" + responseResult.ResultJson,
                        _workContext.CurrentCustomer);
                }

                return BadRequest("El pago no pudo ser procesado. Por favor, selecciona otra forma de pago.");

            }
            catch (Exception e)
            {
                var test = Newtonsoft.Json.JsonConvert.SerializeObject(e);
                _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Error, "[VISA ERROR] - " + e.Message + ", " + e.InnerException.Message,
                    test + (settings.UseSandbox ?  "\n\n" + request : string.Empty),
                    _workContext.CurrentCustomer);
                return BadRequest("El pago no pudo ser procesado. Por favor, selecciona otra forma de pago.");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("[controller]/[action]")]
        public IActionResult DeviceFingerprint(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId)) return NotFound();
            var settings = _settingService.LoadSetting<VisaPaymentSettings>();
            string organizationId = settings.UseSandbox ? settings.OrganizationIdSandbox : settings.OrganizationIdProduction;

            DeviceFingerprintModel model = new DeviceFingerprintModel()
            {
                SessionId = sessionId,
                MerchantId = settings.MerchantId,
                OrganizationId = organizationId
            };

            return View("~/Plugins/Payments.Visa/Views/DeviceFingerprint.cshtml", model);
        }

        #endregion

        #region Private Methos

        /// <summary>
        /// This demonstrates what a generic API request helper method would look like.
        /// </summary>
        /// <param name="request">Request to send to API endpoint</param>
        /// <returns>Task</returns>
        private async Task<ProcessPaymentResult> CallCyberSourceAPI(string request, VisaPaymentSettings settings)
        {
            TaskStatus responseCode;
            string responseContent = string.Empty;

            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

            // HTTP POST request
            using (var client = new HttpClient(httpClientHandler))
            {
                client.DefaultRequestHeaders.Clear();
                //System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                string resource = "/pts/v2/payments";
                StringContent content = new StringContent(request);

                content.Headers.ContentType = new MediaTypeHeaderValue("application/json"); // content-type header

                /* Add Request Header :: "v-c-merchant-id" set value to Cybersource Merchant ID or v-c-merchant-id
                 * This ID can be found on EBC portal
                 */
                client.DefaultRequestHeaders.Add("v-c-merchant-id", settings.MerchantId);

                /* Add Request Header :: "Date" The date and time that the message was originated from.
                 * "HTTP-date" format as defined by RFC7231.
                 */
                string gmtDateTime = DateTime.Now.ToUniversalTime().ToString("r");
                client.DefaultRequestHeaders.Add("Date", gmtDateTime);

                /* Add Request Header :: "Host"
                 * Sandbox Host: apitest.cybersource.com
                 * Production Host: api.cybersource.com
                 */
                string requestHost = settings.UseSandbox ? RequestUtils.SANDBOX_HOST : RequestUtils.PRODUCTION_HOST;
                client.DefaultRequestHeaders.Add("Host", requestHost);

                /* Add Request Header :: "Digest"
                 * Digest is SHA-256 hash of payload that is BASE64 encoded
                 */
                var digest = GenerateDigest(request);
                client.DefaultRequestHeaders.Add("Digest", digest);

                /* Add Request Header :: "Signature"
                 * Signature header contains keyId, algorithm, headers and signature as paramters
                 * method getSignatureHeader() has more details
                 */
                string secretKey = settings.UseSandbox ? settings.SharedSecretKeySandbox : settings.SharedSecretKeyProduction;
                string publicKey = settings.UseSandbox ? settings.ApiKeySandbox : settings.ApiKeyProduction;
                StringBuilder signature = GenerateSignature(request, digest, string.Empty, gmtDateTime, "post", resource, requestHost, settings.MerchantId, secretKey, publicKey);
                client.DefaultRequestHeaders.Add("Signature", signature.ToString());

                var postUrl = "https://" + requestHost + resource;
                _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Information, "[VISA INFO] - Signature created, sending to: " + postUrl,
                    "",
                    _workContext.CurrentCustomer);

                var test = Newtonsoft.Json.JsonConvert.SerializeObject(content);
                var response = await client.PostAsync(postUrl, content);
                responseCode = (TaskStatus)response.StatusCode;
                responseContent = await response.Content.ReadAsStringAsync();
            }

            return new ProcessPaymentResult() { ResponseCode = responseCode, ResultJson = responseContent };
        }

        /// <summary>
        /// This method return Digest value which is SHA-256 hash of payload that is BASE64 encoded
        /// </summary>
        /// <param name="request">Value from which to generate digest</param>
        /// <returns>String referring to a digest</returns>
        private string GenerateDigest(string request)
        {
            string digest = "DIGEST_PLACEHOLDER";
            try
            {
                // Generate the Digest
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    byte[] payloadBytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(request));
                    digest = Convert.ToBase64String(payloadBytes);
                    digest = "SHA-256=" + digest;
                }
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }

            return digest;
        }

        // This method returns value for paramter Signature which is then passed to Signature header
        // paramter 'Signature' is calucated based on below key values and then signed with SECRET KEY -
        // host: Sandbox (apitest.cybersource.com) or Production (api.cybersource.com) hostname
        // date: "HTTP-date" format as defined by RFC7231.
        // (request-target): Should be in format of httpMethod: path
        //    Example: "post /pts/v2/payments"
        // Digest: Only needed for POST calls.
        //    digestString = BASE64( HMAC-SHA256 ( Payload ));
        //    Digest: “SHA-256=“ + digestString;
        // v-c-merchant-id: set value to Cybersource Merchant ID
        //    This ID can be found on EBC portal
        private StringBuilder GenerateSignature(string request,
            string digest,
            string keyid,
            string gmtDateTime,
            string method,
            string resource,
            string requestHost,
            string merchantId,
            string merchantsecretKey,
            string merchantKeyId)
        {
            StringBuilder signatureHeaderValue = new StringBuilder();
            string algorithm = "HmacSHA256";
            string postHeaders = "host date (request-target) digest v-c-merchant-id";
            string getHeaders = "host date (request-target) v-c-merchant-id";
            string url = "https://" + requestHost + resource;
            string getRequestTarget = method + " " + resource;
            string postRequestTarget = method + " " + resource;

            try
            {
                // Generate the Signature
                StringBuilder signatureString = new StringBuilder();
                signatureString.Append('\n');
                signatureString.Append("host");
                signatureString.Append(": ");
                signatureString.Append(requestHost);
                signatureString.Append('\n');
                signatureString.Append("date");
                signatureString.Append(": ");
                signatureString.Append(gmtDateTime);
                signatureString.Append('\n');
                signatureString.Append("(request-target)");
                signatureString.Append(": ");

                if (method.Equals("post"))
                {
                    signatureString.Append(postRequestTarget);
                    signatureString.Append('\n');
                    signatureString.Append("digest");
                    signatureString.Append(": ");
                    signatureString.Append(digest);
                }
                else
                {
                    signatureString.Append(getRequestTarget);
                }

                signatureString.Append('\n');
                signatureString.Append("v-c-merchant-id");
                signatureString.Append(": ");
                signatureString.Append(merchantId);
                signatureString.Remove(0, 1);

                byte[] signatureByteString = Encoding.UTF8.GetBytes(signatureString.ToString());
                byte[] decodedKey = Convert.FromBase64String(merchantsecretKey);

                HMACSHA256 aKeyId = new HMACSHA256(decodedKey);

                byte[] hashmessage = aKeyId.ComputeHash(signatureByteString);
                string base64EncodedSignature = Convert.ToBase64String(hashmessage);

                signatureHeaderValue.Append("keyid=\"" + merchantKeyId + "\"");
                signatureHeaderValue.Append(", algorithm=\"" + algorithm + "\"");

                if (method.Equals("post"))
                {
                    signatureHeaderValue.Append(", headers=\"" + postHeaders + "\"");
                }
                else if (method.Equals("get"))
                {
                    signatureHeaderValue.Append(", headers=\"" + getHeaders + "\"");
                }

                signatureHeaderValue.Append(", signature=\"" + base64EncodedSignature + "\"");

                // Writing Generated Token to file.
                //File.WriteAllText(@".\Resource\" + "signatureHeaderValue.txt", signatureHeaderValue.ToString());
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }

            return signatureHeaderValue;
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
    }

    public class ProcessPaymentResult
    {
        public TaskStatus ResponseCode { get; set; }
        public string ResultJson { get; set; }
    }
}
