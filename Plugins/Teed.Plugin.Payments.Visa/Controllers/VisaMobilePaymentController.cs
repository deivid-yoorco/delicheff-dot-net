using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using System.Collections.Generic;
using Nop.Services.Logging;
using Nop.Web.Factories;
using Nop.Services.Customers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Teed.Plugin.Payments.Visa.Dtos;
using System;
using Nop.Core.Domain.Customers;
using System.Net.Http;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Linq;
using Nop.Services.Tax;
using Nop.Services.Discounts;
using Nop.Services.Catalog;
using Teed.Plugin.Payments.Visa.Utils;
using Nop.Core.Domain.Orders;

namespace Teed.Plugin.Payments.Visa.Controllers
{
    public class VisaMobilePaymentController : ApiBaseController
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
        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;

        private List<string> ErrorMessages = new List<string>();

        #endregion

        #region Ctor

        public VisaMobilePaymentController(IStoreService storeService,
            ISettingService settingService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IStoreContext storeContext,
            IStateProvinceService stateProvinceService,
            IAddressService addressService,
            ILogger logger,
            IShoppingCartModelFactory shoppingCartModelFactory,
            ICustomerService customerService,
            ITaxService taxService, IPriceCalculationService priceCalculationService)
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
            _taxService = taxService;
            _priceCalculationService = priceCalculationService;
        }

        #endregion

        #region Methods

        [HttpGet]
        public IActionResult GetPaymentData()
        {
            var settings = _settingService.LoadSetting<VisaPaymentSettings>();
            if (settings == null)
                BadRequest("No fue posible realizar el pago. El método de pago no está activo.");
            var dto = new PaymentDataDto()
            {
                Url = settings.UseSandbox ? RequestUtils.SANDBOX_HOST : RequestUtils.PRODUCTION_HOST,
                ApiKey = settings.UseSandbox ? settings.ApiKeySandbox : settings.ApiKeyProduction,
                MerchantId = settings.MerchantId,
                SecretSharedKey = settings.UseSandbox ? settings.SharedSecretKeySandbox : settings.SharedSecretKeyProduction
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> SaveCardInDb(string paymentInstrumentId)
        {
            var settings = _settingService.LoadSetting<VisaPaymentSettings>();
            if (settings == null)
                BadRequest("No fue posible guardar la información de pago. El método de pago no está activo.");

            var customerId = 1;
            var customer = _customerService.GetCustomerById(customerId);

            await CreateAndSaveCustomer(customer, settings, paymentInstrumentId);

            return NoContent();
        }

        [HttpPost]
        public IActionResult GetSignature([FromBody] GetSignatureDto dto)
        {
            if (dto == null) return BadRequest();

            var settings = _settingService.LoadSetting<VisaPaymentSettings>();
            if (settings == null)
                BadRequest("No fue posible guardar la información de pago. El método de pago no está activo.");

            string requestHost = settings.UseSandbox ? RequestUtils.SANDBOX_HOST : RequestUtils.PRODUCTION_HOST;
            string gmtDateTimeParsed = dto.GmtDateTime.ToString("r");
            string merchantApiKey = settings.UseSandbox ? settings.ApiKeySandbox : settings.ApiKeyProduction;
            string merchantSecretKey = settings.UseSandbox ? settings.SharedSecretKeySandbox : settings.SharedSecretKeyProduction;

            StringBuilder signature = RequestUtils.GenerateSignature(dto.Digest,
                gmtDateTimeParsed,
                dto.Method,
                dto.Resource,
                requestHost,
                settings.MerchantId,
                merchantSecretKey,
                merchantApiKey);

            var result = new SignatureResultDto()
            {
                DateParsed = gmtDateTimeParsed,
                Host = requestHost,
                MerchantId = settings.MerchantId,
                Signature = signature.ToString()
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> ExcecuteDecisionManager([FromBody] SavedCardDataDto dto)
        {
            var customerId = int.Parse(UserId);
            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null) return NotFound();

            var cart = customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart);
            var model = _shoppingCartModelFactory.PrepareOrderTotalsModel(cart.ToList(), false);

            var settings = _settingService.LoadSetting<VisaPaymentSettings>();
            if (settings == null)
                return BadRequest("No fue posible realizar el pago. El método de pago no está activo.");

            var body = new
            {
                clientReferenceInformation = new
                {
                    code = customer.Id.ToString()
                },
                paymentInformation = new
                {
                    customer = new
                    {
                        customerId = dto.ServiceCustomerId
                    }
                },
                orderInformation = new
                {
                    amountDetails = new
                    {
                        totalAmount = model.OrderTotalValue.ToString(),
                        currency = "MXN"
                    },
                    billTo = new
                    {
                        firstName = dto.BillFirstName,
                        lastName = dto.BillLastName,
                        address1 = dto.BillAddress1,
                        locality = dto.BillLocality,
                        administrativeArea = dto.BillAdministrativeArea,
                        postalCode = dto.BillPostalCode,
                        country = dto.BillCountry,
                        email = dto.BillEmail,
                        phoneNumber = dto.BillPhoneNumber
                    }
                }
            };

            string bodyJson = Newtonsoft.Json.JsonConvert.SerializeObject(body);
            StringContent content = new StringContent(bodyJson);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using (var client = new HttpClient())
            {
                string merchantApiKey = settings.UseSandbox ? settings.ApiKeySandbox : settings.ApiKeyProduction;
                string merchantSecretKey = settings.UseSandbox ? settings.SharedSecretKeySandbox : settings.SharedSecretKeyProduction;
                string requestHost = settings.UseSandbox ? RequestUtils.SANDBOX_HOST : RequestUtils.PRODUCTION_HOST;
                string resource = RequestUtils.DECISION_MANAGER_RESOURCE;

                PrepareHeaders(client,
                    settings.MerchantId,
                    requestHost,
                    resource,
                    null,
                    "post",
                    merchantSecretKey,
                    merchantApiKey,
                    bodyJson);

                string url = "https://" + requestHost + resource;
                var result = await client.PostAsync(url, content);
                var resultJson = await result.Content.ReadAsStringAsync();
                if (result.IsSuccessStatusCode)
                {
                    var resultData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(resultJson);
                    var dMStatusResult = resultData.status == "ACCEPTED";
                    if (!dMStatusResult)
                        _logger.Information($"[VISA] La transacción ha sido rechazada por desition manager (Usuario Id {customerId})", new Exception(resultJson), customer);
                    return Ok(dMStatusResult);
                }
                else
                {
                    _logger.Error($"[VISA] Ha ocurrido un error al ejecutar el desition manager (Usuario Id {customerId})", new Exception(resultJson), customer);
                    return BadRequest();
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> CapturePayment([FromBody] SavedCardDataDto dto)
        {
            var customerId = int.Parse(UserId);
            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null) return NotFound();

            var cart = customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart);
            var model = _shoppingCartModelFactory.PrepareOrderTotalsModel(cart.ToList(), false);
            var shippingAddress = customer.Addresses.Where(x => x.Id == dto.SelectedShippingAddressId).FirstOrDefault();

            var settings = _settingService.LoadSetting<VisaPaymentSettings>();
            if (settings == null)
                return BadRequest("No fue posible realizar el pago. El método de pago no está activo.");

            var body = new
            {
                clientReferenceInformation = new
                {
                    code = customer.Id.ToString()
                },
                paymentInformation = new
                {
                    customer = new
                    {
                        id = dto.ServiceCustomerId
                    },
                    card = new
                    {
                        securityCode = dto.CustomerEnteredSecurityCode
                    }
                },
                orderInformation = new
                {
                    amountDetails = new
                    {
                        totalAmount = model.OrderTotalValue.ToString(),
                        currency = "MXN"
                    },
                    lineItems = cart.Select(x => new
                    {
                        productCode = x.Product.ProductCategories.Where(y => y.Category?.ParentCategoryId == 0).FirstOrDefault()?.Category?.Name,
                        productName = x.Product.Name.Substring(0, x.Product.Name.Length > 255 ? 255 : x.Product.Name.Length),
                        productSku = x.Product.Sku.Substring(0, x.Product.Sku.Length > 255 ? 255 : x.Product.Sku.Length),
                        totalAmount = _taxService.GetProductPrice(x.Product, _priceCalculationService.GetSubTotal(x, true, out decimal shoppingCartItemDiscountBase, out List<DiscountForCaching> _, out int? maximumDiscountQty, out decimal rewardPointsRequired), out decimal taxRate).ToString(),
                        unitPrice = _taxService.GetProductPrice(x.Product, _priceCalculationService.GetSubTotal(x, true, out decimal shoppingCartItemDiscountBase2, out List<DiscountForCaching> _, out int? maximumDiscountQty2, out decimal rewardPointsRequired2), out decimal taxRate2).ToString(),
                        quantity = 1,
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
                    fingerprintSessionId = dto.DeviceFingerprintSessionId
                }
            };

            string bodyJson = Newtonsoft.Json.JsonConvert.SerializeObject(body);
            StringContent content = new StringContent(bodyJson);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using (var client = new HttpClient())
            {
                string merchantApiKey = settings.UseSandbox ? settings.ApiKeySandbox : settings.ApiKeyProduction;
                string merchantSecretKey = settings.UseSandbox ? settings.SharedSecretKeySandbox : settings.SharedSecretKeyProduction;
                string requestHost = settings.UseSandbox ? RequestUtils.SANDBOX_HOST : RequestUtils.PRODUCTION_HOST;
                string resource = RequestUtils.PAYMENT_AUTHORIZATION_RESOURCE;

                PrepareHeaders(client,
                    settings.MerchantId,
                    requestHost,
                    resource,
                    null,
                    "post",
                    merchantSecretKey,
                    merchantApiKey,
                    bodyJson);

                string url = "https://" + requestHost + resource;
                var result = await client.PostAsync(url, content);
                var resultJson = await result.Content.ReadAsStringAsync();
                if (result.IsSuccessStatusCode)
                {
                    var resultData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(resultJson);
                    if (resultData.status == "AUTHORIZED")
                    {
                        return Ok(resultJson);
                    }
                    else
                    {
                        _logger.Information($"[VISA] Una transacción ha sido rechazada (Usuario Id {customerId})", new Exception(resultJson), customer);
                        return BadRequest();
                    }
                }
                else
                {
                    Debugger.Break();
                    _logger.Error($"[VISA] Ha ocurrido un error al tratar de autorizar el pago (Usuario Id {customerId})", new Exception(resultJson), customer);
                    return BadRequest();
                }
            }
        }

        #endregion

        #region Private Methods

        private async Task CreateAndSaveCustomer(Customer customer, VisaPaymentSettings settings, string paymentInstrumentId)
        {
            var body = new
            {
                buyerInformation = new
                {
                    merchantCustomerID = customer.Id,
                    email = customer.Email
                },
                clientReferenceInformation = new
                {
                    code = customer.Id
                },
                defaultPaymentInstrument = new
                {
                    id = paymentInstrumentId
                }
            };

            string bodyJson = Newtonsoft.Json.JsonConvert.SerializeObject(body);
            StringContent content = new StringContent(bodyJson);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using (HttpClient client = new HttpClient())
            {
                string merchantApiKey = settings.UseSandbox ? settings.ApiKeySandbox : settings.ApiKeyProduction;
                string merchantSecretKey = settings.UseSandbox ? settings.SharedSecretKeySandbox : settings.SharedSecretKeyProduction;
                string requestHost = settings.UseSandbox ? RequestUtils.SANDBOX_HOST : RequestUtils.PRODUCTION_HOST;
                string resource = RequestUtils.CREATE_CUSTOMER_RESOURCE;

                PrepareHeaders(client,
                    settings.MerchantId,
                    requestHost,
                    resource,
                    null,
                    "post",
                    merchantSecretKey,
                    merchantApiKey,
                    bodyJson);
                var result = await client.PostAsync("https://" + requestHost + resource, content);
                string resultJson = await result.Content.ReadAsStringAsync();
                if (result.IsSuccessStatusCode)
                {
                    Debugger.Break();
                }
                else
                {
                    Debugger.Break();
                }
            }
        }

        private void PrepareHeaders(HttpClient client,
            string merchandId,
            string requestHost,
            string resource,
            string digest,
            string method,
            string merchantSecretKey,
            string merchantApiKey,
            string requestBody)
        {
            client.DefaultRequestHeaders.Add("v-c-merchant-id", merchandId);

            string gmtDateTime = DateTime.Now.ToUniversalTime().ToString("r");
            client.DefaultRequestHeaders.Add("Date", gmtDateTime);

            client.DefaultRequestHeaders.Add("Host", requestHost);

            if (!method.Equals("get"))
            {
                digest = string.IsNullOrWhiteSpace(digest) ? GenerateDigest(requestBody) : digest;
                client.DefaultRequestHeaders.Add("Digest", digest);
            }

            StringBuilder signature = RequestUtils.GenerateSignature(digest,
                gmtDateTime,
                method,
                resource,
                requestHost,
                merchandId,
                merchantSecretKey,
                merchantApiKey);
            client.DefaultRequestHeaders.Add("Signature", signature.ToString());
        }

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

        #endregion
    }
}
