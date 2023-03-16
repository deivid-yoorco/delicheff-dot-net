using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Facturify.Models;
using Nop.Services.Logging;
using Nop.Services.Helpers;
using Teed.Plugin.Facturify.Services;
using Teed.Plugin.Facturify.Domain;
//using static Nop.Web.Areas.Admin.Models.Orders.OrderModel;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using Nop.Services.Catalog;
using OfficeOpenXml;
using Nop.Core.Domain.Tax;
using Newtonsoft.Json;
using Teed.Plugin.Facturify.Security;

namespace Teed.Plugin.Facturify.Controllers
{
    [Area(AreaNames.Admin)]
    public class FacturifyController : BasePluginController
    {
        const string API_URL = "https://api.facturify.com";
        const string API_SANDBOX_URL = "https://api-sandbox.facturify.com";

        const string API_GET_TOKEN = "/api/v1/auth";
        const string API_VALIDATE_TOKEN = "/api/v1/token/validate";
        const string API_REFRESH_TOKEN = "/api/v1/token/refresh";
        const string API_REGISTER_COMPANY = "/api/v1/empresa";
        const string API_UPDATE_COMPANY = "/api/v1/empresa/";
        const string API_REGISTER_CUSTOMER = "/api/v1/cliente";
        const string API_UPDATE_CUSTOMER = "/api/v1/cliente/";
        const string API_CREATE_BILL = "/api/v1/factura";
        const string API_CANCEL_BILL = "/api/v1/factura/";
        const string API_PDF_CONFIGURATION = "/api/v1/configuracion";

        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly IPermissionService _permissionService;
        private readonly ILogger _logger;
        private readonly CustomerBillingAddressService _customerBillingAddressService;
        private readonly BillService _billService;
        private readonly CategoryProductCatalogService _categoryProductCatalogService;
        private readonly ProductSatCodeService _productSatCodeService;
        private readonly IProductService _productService;
        private readonly TaxSettings _taxSettings;

        public List<string> ErrorMessages { get; set; }

        public FacturifyController(ISettingService settingService, IPermissionService permissionService,
            ILogger logger, CustomerBillingAddressService customerBillingAddressService, BillService billService,
            IOrderService orderService, CategoryProductCatalogService categoryProductCatalogService,
            ProductSatCodeService productSatCodeService, IProductService productService,
            TaxSettings taxSettings)
        {
            _settingService = settingService;
            _permissionService = permissionService;
            _logger = logger;
            _customerBillingAddressService = customerBillingAddressService;
            _orderService = orderService;
            _billService = billService;
            _categoryProductCatalogService = categoryProductCatalogService;
            _productSatCodeService = productSatCodeService;
            _productService = productService;
            _taxSettings = taxSettings;
        }

        public IActionResult Index()
        {
            if (!_permissionService.Authorize(FacturifyPermissionProvider.Facturify))
                return AccessDeniedView();

            FacturifySettings facturifySettings = _settingService.LoadSetting<FacturifySettings>();

            ConfigureModel model = new ConfigureModel()
            {
                AddressCity = facturifySettings.AddressCity,
                AddressExteriorNumber = facturifySettings.AddressExteriorNumber,
                AddressInternalNumber = facturifySettings.AddressInternalNumber,
                AddressMunicipality = facturifySettings.AddressMunicipality,
                AddressPostalCode = facturifySettings.AddressPostalCode,
                AddressState = facturifySettings.AddressState,
                AddressStreet = facturifySettings.AddressStreet,
                AddressSuburb = facturifySettings.AddressSuburb,
                BusinessName = facturifySettings.BusinessName,
                CsdCerFileBase64 = facturifySettings.CsdCerFileBase64,
                CsdKeyFileBase64 = facturifySettings.CsdKeyFileBase64,
                CsdKeyFilePassword = facturifySettings.CsdKeyFilePassword,
                Email = facturifySettings.Email,
                FacturifyUuid = facturifySettings.FacturifyUuid,
                FielCerFileBase64 = facturifySettings.FielCerFileBase64,
                FielKeyFileBase64 = facturifySettings.FielKeyFileBase64,
                FielKeyFilePassword = facturifySettings.FielKeyFilePassword,
                Regime = facturifySettings.Regime,
                RFC = facturifySettings.RFC,
                ApiKey = facturifySettings.ApiKey,
                ApiSecret = facturifySettings.ApiSecret,
                Sandbox = facturifySettings.Sandbox,
                DaysAllowed = facturifySettings.DaysAllowed.ToString(),
                AllowBillGeneration = facturifySettings.AllowBillGeneration
            };

            return View("~/Plugins/Teed.Plugin.Facturify/Views/Facturify/Index.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ConfigureModel model)
        {
            if (!_permissionService.Authorize(FacturifyPermissionProvider.Facturify))
                return AccessDeniedView();

            ErrorMessages = new List<string>();

            FacturifySettings facturifySettings = _settingService.LoadSetting<FacturifySettings>();
            FacturifySettings originalSettings = facturifySettings;

            facturifySettings.AddressCity = model.AddressCity;
            facturifySettings.AddressExteriorNumber = model.AddressExteriorNumber;
            facturifySettings.AddressInternalNumber = model.AddressInternalNumber;
            facturifySettings.AddressMunicipality = model.AddressMunicipality;
            facturifySettings.AddressPostalCode = model.AddressPostalCode;
            facturifySettings.AddressState = model.AddressState;
            facturifySettings.AddressStreet = model.AddressStreet;
            facturifySettings.AddressSuburb = model.AddressSuburb;
            facturifySettings.BusinessName = model.BusinessName;
            facturifySettings.CsdKeyFilePassword = model.CsdKeyFilePassword;
            facturifySettings.Email = model.Email;
            facturifySettings.FacturifyUuid = model.FacturifyUuid;
            facturifySettings.FielKeyFilePassword = model.FielKeyFilePassword;
            facturifySettings.Regime = model.Regime;
            facturifySettings.RFC = model.RFC;
            facturifySettings.Sandbox = model.Sandbox;
            facturifySettings.ApiSecret = model.ApiSecret;
            facturifySettings.ApiKey = model.ApiKey;
            facturifySettings.FielCerFileBase64 = ConvertFileToBase64(model.FielCerFile);
            facturifySettings.FielKeyFileBase64 = ConvertFileToBase64(model.FielKeyFile);
            facturifySettings.CsdCerFileBase64 = ConvertFileToBase64(model.CsdCerFile);
            facturifySettings.CsdKeyFileBase64 = ConvertFileToBase64(model.CsdKeyFile);

            _settingService.SaveSetting(facturifySettings);

            await RegisterOrUpdateCompany(facturifySettings, originalSettings);

            if (ErrorMessages.Count > 0)
            {
                foreach (var error in ErrorMessages)
                {
                    ModelState.AddModelError("", error);
                }
            }
            else
            {
                return RedirectToAction("Index");
            }

            return View("~/Plugins/Teed.Plugin.Facturify/Views/Facturify/Index.cshtml", model);
        }

        [HttpGet]
        public IActionResult SaveCategoryProductCatalog(int categoryId, string productCatalogId)
        {
            CategoryProductCatalog categoryProductCatalog = _categoryProductCatalogService.GetByCategoryId(categoryId);
            if (categoryProductCatalog == null)
            {
                categoryProductCatalog = new CategoryProductCatalog()
                {
                    CategoryId = categoryId,
                    ProductCatalogId = productCatalogId
                };
                _categoryProductCatalogService.Insert(categoryProductCatalog);
            }
            else if (categoryProductCatalog.ProductCatalogId != productCatalogId)
            {
                categoryProductCatalog.ProductCatalogId = productCatalogId;
                _categoryProductCatalogService.Update(categoryProductCatalog);
            }

            return Ok();
        }

        [HttpGet]
        public string GetCategoryProductCatalog(int categoryId)
        {
            return _categoryProductCatalogService.GetByCategoryId(categoryId)?.ProductCatalogId;
        }

        [HttpGet]
        public IActionResult GetCustomerAddresses(int customerId)
        {
            return Ok(_customerBillingAddressService.GetByCustomerId(customerId).ToList());
        }

        [HttpGet]
        public IActionResult GetPaymentForms()
        {
            List<object> list = new List<object>();
            foreach (var item in Enum.GetValues(typeof(PaymentForms)))
            {
                list.Add(new
                {
                    id = (int)item,
                    value = EnumHelper.GetDisplayName((PaymentForms)item)
                });
            }

            return Ok(list);
        }

        [HttpGet]
        public IActionResult GetCfdiUses()
        {
            List<object> list = new List<object>();
            foreach (var item in Enum.GetNames(typeof(CfdiUse)))
            {
                list.Add(new
                {
                    id = item,
                    value = EnumHelper.GetDisplayName((CfdiUse)Enum.Parse(typeof(CfdiUse), item))
                });
            }

            return Ok(list);
        }

        [HttpGet]
        public IActionResult GetBill(int orderId)
        {
            return Ok(_billService.GetAll().Where(x => x.OrderId == orderId).FirstOrDefault());
        }

        [HttpGet]
        public IActionResult GetAllow()
        {
            FacturifySettings facturifySettings = _settingService.LoadSetting<FacturifySettings>();
            return Ok(facturifySettings.AllowBillGeneration);
        }

        private async Task RegisterOrUpdateCompany(FacturifySettings settings, FacturifySettings originalSettings = null)
        {
            if (string.IsNullOrWhiteSpace(settings.FacturifyUuid))
            {
                await RegisterCompany(settings);
            }
            else
            {
                await UpdateCompany(settings, originalSettings);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReregisterCompany(ConfigureModel model)
        {
            ErrorMessages = new List<string>();
            FacturifySettings settings = new FacturifySettings
            {
                ApiKey = model.ApiKey,
                ApiSecret = model.ApiSecret,
                Sandbox = model.Sandbox,
                AddressCity = model.AddressCity,
                AddressExteriorNumber = model.AddressExteriorNumber,
                AddressInternalNumber = model.AddressInternalNumber,
                AddressMunicipality = model.AddressMunicipality,
                AddressPostalCode = model.AddressPostalCode,
                AddressState = model.AddressState,
                AddressStreet = model.AddressStreet,
                AddressSuburb = model.AddressSuburb,
                BusinessName = model.BusinessName,
                CsdCerFileBase64 = ConvertFileToBase64(model.CsdCerFile),
                CsdKeyFileBase64 = ConvertFileToBase64(model.CsdKeyFile),
                CsdKeyFilePassword = model.CsdKeyFilePassword,
                FielCerFileBase64 = ConvertFileToBase64(model.FielCerFile),
                FielKeyFileBase64 = ConvertFileToBase64(model.FielKeyFile),
                FielKeyFilePassword = model.FielKeyFilePassword,
                Email = model.Email,
                Regime = model.Regime,
                RFC = model.RFC
            };
            await RegisterCompany(settings);
            if (ErrorMessages.Count > 0)
            {
                return BadRequest("No se pudo registrar la empresa. " + string.Join(". ", ErrorMessages).Replace("..", "."));
            }
            else
            {
                return Ok("Se registró la empresa correctamente");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RedoToken(ConfigureModel model)
        {
            ErrorMessages = new List<string>();
            var settings = new FacturifySettings
            {
                ApiKey = model.ApiKey,
                ApiSecret = model.ApiSecret,
                Sandbox = model.Sandbox
            };
            var newToken = await SaveNewToken(settings, true);
            if (!string.IsNullOrEmpty(newToken))
                return Ok("El token se actualizó correctamente");
            else
                return BadRequest("No se pudo actualizar el token");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeBillsLogo(ConfigureModel model)
        {
            ErrorMessages = new List<string>();
            try
            {
                if (model.Image != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        model.Image.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        string img64 = Convert.ToBase64String(fileBytes);

                        FacturifySettings facturifySettings = _settingService.LoadSetting<FacturifySettings>();
                        PdfConfiguration body = new PdfConfiguration()
                        {
                            empresa_uuid = facturifySettings.FacturifyUuid,
                            configuraciones = new List<PdfConfigurationImage>()
                        };
                        body.configuraciones.Add(new PdfConfigurationImage
                        {
                            key = "logo_pdf",
                            value = new PdfConfigurationImageValue
                            {
                                body = img64,
                                mimetype = model.Image.ContentType
                            }
                        });
                        string token = await ValidateAndGetToken(facturifySettings);
                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            using (HttpClient client = new HttpClient())
                            {
                                AddTokenToRequest(client, token);
                                var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                                var result = await client.PostAsync((model.Sandbox ? API_SANDBOX_URL : API_URL) + API_PDF_CONFIGURATION, content);
                                if (result.IsSuccessStatusCode)
                                {
                                    return Ok("El logotipo se actualizó correctamente");
                                }
                                else
                                {
                                    var json = await result.Content.ReadAsStringAsync();
                                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<GetBillResult>(json);
                                    await RegisterErrors(result);
                                    _logger.Error("[Facturify - CNB] Error making a request to API.");
                                    if (ErrorMessages.Count > 0)
                                    {
                                        return BadRequest("No se pudo cambiar el logotipo de las facturas. " + string.Join(". ", ErrorMessages).Replace("..", "."));
                                    }
                                    else
                                    {
                                        return BadRequest("No se pudo cambiar el logotipo de las facturas");
                                    }
                                }
                            }
                        }
                        return BadRequest("No se pudo cambiar el logotipo de las facturas");
                    }
                }
                return BadRequest("La imagen es requerida");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public IActionResult ChangeDaysAllowed(ConfigureModel model)
        {
            if (!_permissionService.Authorize(FacturifyPermissionProvider.Facturify))
                return AccessDeniedView();
            try
            {
                FacturifySettings facturifySettings = _settingService.LoadSetting<FacturifySettings>();
                if (model.DaysAllowedInt > 0)
                {
                    facturifySettings.DaysAllowed = model.DaysAllowedInt;
                    _settingService.SaveSetting(facturifySettings);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok("El número de días se actualizó correctamente");
        }

        [HttpPost]
        public IActionResult ChangeAllowGeneration(ConfigureModel model)
        {
            if (!_permissionService.Authorize(FacturifyPermissionProvider.Facturify))
                return AccessDeniedView();
            try
            {
                FacturifySettings facturifySettings = _settingService.LoadSetting<FacturifySettings>();
                facturifySettings.AllowBillGeneration = model.AllowBillGeneration;
                _settingService.SaveSetting(facturifySettings);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok("El permiso para generar facturas se actualizó correctamente");
        }

        [HttpPost]
        public async Task<IActionResult> CreateBill(CreateBillModel model)
        {
            ErrorMessages = new List<string>();
            FacturifySettings facturifySettings = _settingService.LoadSetting<FacturifySettings>();

            CustomerBillingAddress facturifyCustomer = null;
            if (model.FaturifyAddressId == 0)
            {
                var existingFacturifyCostumer = _customerBillingAddressService.GetAll().Where(x => x.RFC.ToUpper() == model.Rfc.ToUpper()).ToList();
                if (existingFacturifyCostumer.Count == 0)
                {
                    CustomerResult customer = await RegisterCustomer(facturifySettings, model);
                    if (customer != null)
                    {
                        if (model.Rfc != customer.data.rfc || model.Email != customer.data.email || model.BusinessName != customer.data.razon_social)
                        {
                            // Old data, updating customer
                            customer = await UpdateCustomer(facturifySettings, model, customer);
                        }
                    }
                    if (customer == null)
                    {
                        _logger.Error("ERROR FACTURIFY: " + string.Join(". ", ErrorMessages).Replace("..", "."));
                        return BadRequest("No fue posible registrar al cliente en el servicio de facturación. Por favor contáctanos o inténtalo de nuevo más tarde.");
                    }
                    facturifyCustomer = new CustomerBillingAddress()
                    {
                        BusinessName = customer.data.razon_social,
                        FacturifyUuid = customer.data.uuid,
                        RFC = customer.data.rfc,
                        Email = customer.data.email,
                        CustomerId = model.CustomerId
                    };
                    _customerBillingAddressService.Insert(facturifyCustomer);
                }
                else
                {
                    var newFacturifyCustomer = existingFacturifyCostumer.Where(x => x.CustomerId == model.CustomerId).FirstOrDefault();
                    if (newFacturifyCustomer == null)
                    {
                        var existingData = existingFacturifyCostumer.FirstOrDefault();
                        newFacturifyCustomer = new CustomerBillingAddress()
                        {
                            BusinessName = existingData.BusinessName,
                            FacturifyUuid = existingData.FacturifyUuid,
                            RFC = existingData.RFC,
                            Email = existingData.Email,
                            CustomerId = model.CustomerId
                        };
                        _customerBillingAddressService.Insert(newFacturifyCustomer);
                    }
                    facturifyCustomer = newFacturifyCustomer;
                }
            }
            else
            {
                facturifyCustomer = _customerBillingAddressService.GetById(model.FaturifyAddressId);
                if (facturifyCustomer == null) return NotFound("La información seleccionada no está registrada.");
            }

            Order order = _orderService.GetOrderById(model.OrderId);
            if (order == null) return NotFound("No se encontró la orden.");

            if (order.OrderStatusId != 30 || !order.PaidDateUtc.HasValue)
                return NotFound("La orden debe estar marcada como pagada y entregada para poder generar la factura. Por favor contáctanos o inténtalo de nuevo más tarde.");

            var daysAllowed = facturifySettings.DaysAllowed > 0 ? facturifySettings.DaysAllowed : 90;
            var daysInDifference = DateTime.Now.Date - order.PaidDateUtc.Value.ToLocalTime().Date;
            if (daysInDifference.Days > daysAllowed)
                return NotFound($"Solo puedes generar tu factura hasta {daysAllowed} días después de la fecha de pago.");

            await CreateNewBill(order, facturifySettings, facturifyCustomer, model.PaymentForm, model.CardDigits, model.CfdiUse);

            if (ErrorMessages.Count > 0)
            {
                return BadRequest(string.Join(". ", ErrorMessages).Replace("..", "."));
            }

            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult BillWebHook(BillWebHookModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.data.cfdi_uuid) && !string.IsNullOrWhiteSpace(model.data.job_id))
            {
                Bill bill = _billService.GetAll().Where(x => x.JobId == model.data.job_id).FirstOrDefault();
                if (bill == null) return NotFound("job_id is not registered");

                bill.PdfBase64 = model.data.pdf;
                bill.Serie = model.data.serie;
                bill.CfdiUuid = model.data.cfdi_uuid;
                bill.Folio = model.data.folio;
                bill.XmlBase64 = model.data.xml;

                _billService.Update(bill);
            }

            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> CancelBill(string uuid)
        {
            Bill bill = _billService.GetAll().Where(x => x.CfdiUuid == uuid).FirstOrDefault();
            if (bill == null) return NotFound("El identificador de la factura no es válido.");

            ErrorMessages = new List<string>();
            FacturifySettings settings = _settingService.LoadSetting<FacturifySettings>();

            string token = await ValidateAndGetToken(settings);
            if (!string.IsNullOrWhiteSpace(token))
            {
                using (HttpClient client = new HttpClient())
                {
                    AddTokenToRequest(client, token);
                    var url = (settings.Sandbox ? API_SANDBOX_URL : API_URL) + API_CANCEL_BILL + uuid + "/cancel";
                    var result = await client.PutAsync(url, null);
                    if (result.IsSuccessStatusCode)
                    {
                        bill.Deleted = true;
                        _billService.Update(bill);
                        return Ok();
                    }
                    else
                    {
                        await RegisterErrors(result);
                    }
                }
            }
            return BadRequest(string.Join(". ", ErrorMessages).Replace("..", "."));
        }

        private async Task CreateNewBill(Order order, FacturifySettings settings, CustomerBillingAddress facturifyCustomer, string paymentForm, string cardDigits, string cfdiUse)
        {
            string token = await ValidateAndGetToken(settings);
            if (!string.IsNullOrWhiteSpace(token))
            {
                using (HttpClient client = new HttpClient())
                {
                    AddTokenToRequest(client, token);
                    var rate =
                        _settingService.GetSettingByKey<decimal>(string.Format("Tax.TaxProvider.FixedOrByCountryStateZip.TaxCategoryId{0}", _taxSettings.DefaultTaxCategoryId));
                    GetBillRequest body = new GetBillRequest()
                    {
                        emisor = new { uuid = settings.FacturifyUuid },
                        receptor = new
                        {
                            uuid = facturifyCustomer.FacturifyUuid,
                            tarjeta_ultimos_4digitos = string.IsNullOrWhiteSpace(cardDigits) ? null : cardDigits,
                            forma_de_pago = int.Parse(paymentForm).ToString("00"),
                            uso_cfdi = cfdiUse
                        },
                        factura = new GetBillData()
                        {
                            fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            generacion_automatica = true,
                            tipo = "ingreso",
                            tipo_de_cambio = "1.00",
                            moneda = "MXN",
                            //subtotal = Math.Round(order.OrderTotal, 2),
                            //impuesto_federal = Math.Round(order.OrderTotal * (decimal)0.16, 2),
                            //total = Math.Round(order.OrderTotal, 2),
                            conceptos = new List<BillConcept>()
                        },
                        simulacion_error = 0
                    };

                    var conceptsWithOrderItem = GetConceptsWithOrderItem(order);
                    FixValues(conceptsWithOrderItem);
                    MapConcepts(conceptsWithOrderItem, body);

                    if (order.OrderShippingInclTax > 0)
                    {
                        // Checking if delivery has same values (tuple<incl, excl>)
                        var shippingsInclExcl = CheckShipping(order);
                        body.factura.conceptos.Add(new BillConcept()
                        {
                            cantidad = 1,
                            clave_unidad_de_medida = MeasurementUnits.H87.ToString(),
                            clave_producto_servicio = "80141703",
                            descripcion = "Envío de la orden",
                            valor_unitario = shippingsInclExcl.Item2,
                            total = shippingsInclExcl.Item2,
                            exento_de_impuestos = shippingsInclExcl.Item1 - shippingsInclExcl.Item2 == 0,
                        });
                    }

                    AddTotals(body, rate, order);
                    SumAll(body);
                    var canContinue = true;
                    canContinue = CheckCeilingAndFix(order, body, rate);
                    if (canContinue)
                        canContinue = CheckDiscounts(order, body, rate);
                    FixValues(null, true, body);
                    GetRidOfIds(body);
                    SumAll(body);

                    if (canContinue)
                    {
                        body.factura.conceptos = body.factura.conceptos.Where(x => x.valor_unitario > 0).ToList();
                        var jsonRequest = Newtonsoft.Json.JsonConvert.SerializeObject(body);
                        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                        var result = await client.PostAsync((settings.Sandbox ? API_SANDBOX_URL : API_URL) + API_CREATE_BILL, content);
                        if (result.IsSuccessStatusCode)
                        {
                            var json = await result.Content.ReadAsStringAsync();
                            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<GetBillResult>(json);
                            if (data.code == "70")
                            {
                                ErrorMessages.Add("La factura está pendiente de procesar. Por favor espera unas horas y regresa para verificar si ya culminó el proceso.");
                                Bill bill = new Bill()
                                {
                                    CustomerBillingAddressId = facturifyCustomer.Id,
                                    JobId = data.data.job_id,
                                    OrderId = order.Id
                                };
                                _billService.Insert(bill);
                            }
                            else
                            {
                                Bill bill = new Bill()
                                {
                                    CustomerBillingAddressId = facturifyCustomer.Id,
                                    CfdiUuid = data.data.cfdi_uuid,
                                    OrderId = order.Id,
                                    Folio = data.data.folio,
                                    PdfBase64 = data.data.pdf,
                                    Serie = data.data.serie,
                                    Type = data.data.factura,
                                    XmlBase64 = data.data.xml
                                };
                                _billService.Insert(bill);
                            }
                        }
                        else
                        {
                            var json = await result.Content.ReadAsStringAsync();
                            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<GetBillResult>(json);
                            await RegisterErrors(result);
                            _logger.Error("[Facturify - CNB] Error making a request to API.");
                        }
                    }
                    else
                        ErrorMessages.Add("La factura no pudo ser procesada, error de totales");
                }
            }
        }

        private List<BillConceptWithOrderItem> GetConceptsWithOrderItem(Order order)
        {
            return order.OrderItems.Where(x => 
                (x.IsRewardItem == null || 
                x.IsRewardItem == false) &&
                !x.Product.GiftProductEnable
                ).Select(x => new BillConceptWithOrderItem()
            {
                descripcion = x.Product.Name + "|" + x.Id,
                clave_unidad_de_medida = (x.BuyingBySecondary && x.EquivalenceCoefficient > 0) || x.WeightInterval > 0 ? MeasurementUnits.KGM.ToString() : MeasurementUnits.H87.ToString(),
                cantidad = GetQuantity(x),
                total =
                    GetProductPrice(x)
                     //x.PriceInclTax
                     ,
                valor_unitario = GetUnitValue(x),
                clave_producto_servicio = GetProductCatalogId(x),
                exento_de_impuestos = IsProductTaxExempt(x),
                OrderItem = x
            }).ToList();
        }

        private decimal GetProductPrice(OrderItem item, decimal byThisPrice = 0)
        {
            var price = item.PriceExclTax;
            var exempt = IsProductTaxExempt(item);
            if (exempt)
            {
                if (byThisPrice > 0)
                    return byThisPrice;
                else
                    return item.PriceInclTax;
            }
            else
            {
                if (!item.Product.IsTaxExempt && item.Product.TaxCategoryId > 0)
                {
                    var rate =
                        _settingService.GetSettingByKey<decimal>
                        (string.Format("Tax.TaxProvider.FixedOrByCountryStateZip.TaxCategoryId{0}", item.Product.TaxCategoryId));
                    if (rate > 0)
                    {
                        if (byThisPrice > 0)
                            byThisPrice = byThisPrice / (1 + (rate / 100));
                        else
                            price = item.PriceInclTax / (1 + (rate / 100));
                    }
                }
                return byThisPrice > 0 ? byThisPrice : price;
            }
        }

        private bool IsProductTaxExempt(OrderItem item)
        {
            if (item.Product.TaxCategoryId > 0 && !item.Product.IsTaxExempt)
                return false;
            else if (item.Product.TaxCategoryId < 1)
                return true;
            else if (item.Product.IsTaxExempt)
                return true;
            else
                return true;
        }

        private void FixValues(List<BillConceptWithOrderItem> concepts, bool isRecheck = false, GetBillRequest body = null)
        {
            if (!isRecheck)
                foreach (var item in concepts)
                {
                    if (item.valor_unitario * item.cantidad != item.total)
                    {
                        item.cantidad = 1;
                        item.valor_unitario =
                            //GetProductPrice(item.OrderItem, item.total)
                            item.total
                            ;
                        var split = item.descripcion.Split('|');
                        item.descripcion =
                            split[0] + GetComplementItemName(item.OrderItem) + "|" + item.OrderItem.Id;
                    }
                }
            else
                foreach (var item in body.factura.conceptos)
                {
                    if (item.valor_unitario * item.cantidad != item.total)
                    {
                        item.cantidad = 1;
                        item.valor_unitario =
                            //GetProductPrice(item.OrderItem, item.total)
                            item.total
                            ;
                        var split = item.descripcion.Split('|');
                        var orderItem = _orderService.GetOrderItemById(int.Parse(split[1]));
                        item.descripcion =
                            split[0] + GetComplementItemName(orderItem) + "|" + split[1];
                    }
                }
        }

        private void MapConcepts(List<BillConceptWithOrderItem> concepts, GetBillRequest body)
        {
            body.factura.conceptos = concepts.Select(x => new BillConcept()
            {
                cantidad = x.cantidad,
                clave_producto_servicio = x.clave_producto_servicio,
                clave_unidad_de_medida = x.clave_unidad_de_medida,
                descripcion = x.descripcion,
                total = x.total,
                exento_de_impuestos = x.exento_de_impuestos,
                valor_unitario = x.valor_unitario
            }).ToList();
        }

        private void AddTotals(GetBillRequest body, decimal rate, Order order)
        {
            RoundFinalPrices(body);
            var subtotal = body.factura.conceptos.Select(x => x.total).DefaultIfEmpty().Sum();
            body.factura.subtotal = subtotal;
            var totalIva = (decimal)0;
            foreach (var concept in body.factura.conceptos
                .Where(x => !x.exento_de_impuestos && x.clave_producto_servicio != "80141703"))
            {
                var split = concept.descripcion.Split('|');
                var conceptId =
                    Int32.Parse(split[1]);
                var orderItem = order.OrderItems.Where(x => x.Id == conceptId).FirstOrDefault();
                var originalPrice = orderItem.PriceInclTax;
                totalIva += Math.Round(originalPrice - concept.total, 2);
            }
            var shippingsInclExcl = CheckShipping(order);
            var shippingIva = Math.Round(shippingsInclExcl.Item1 - shippingsInclExcl.Item2, 2);
            totalIva += shippingIva;
            body.factura.impuesto_federal = totalIva;
            body.factura.total =
                (subtotal + totalIva)
                - body.factura.descuento;
        }

        private void RoundFinalPrices(GetBillRequest body)
        {
            body.factura.descuento = Math.Round(body.factura.descuento, 2);
            body.factura.total = Math.Round(body.factura.total, 2);
            body.factura.subtotal = Math.Round(body.factura.subtotal, 2);
            body.factura.impuesto_federal = Math.Round(body.factura.impuesto_federal, 2);
            foreach (var concept in body.factura.conceptos)
            {
                concept.cantidad = Math.Round(concept.cantidad, 2);
                concept.total = Math.Round(concept.total, 2);
                concept.valor_unitario = Math.Round(concept.valor_unitario, 2);
                concept.descuento = Math.Round(concept.descuento, 2);
            }
        }

        private void SumAll(GetBillRequest body)
        {
            var conceptTotals = body.factura.conceptos.Select(x => x.total).DefaultIfEmpty().Sum();
            var subtotalPlusTax = body.factura.subtotal + body.factura.impuesto_federal;
            var totalPrint = body.factura.total;
        }
        private bool CheckDiscounts(Order order, GetBillRequest body, decimal rate)
        {
            var continuar = true;
            if (order.DiscountUsageHistory.Select(x => x.Discount).Any())
            {
                var currentTotal = body.factura.total;
                var orderTotal = order.OrderTotal + (order.CustomerBalanceUsedAmount ?? 0);
                var discounted = currentTotal - orderTotal;
                if (discounted > 0)
                {
                    body.factura.descuento = discounted;
                    body.factura.total = body.factura.total - discounted;
                    var concepts = body.factura.conceptos.Where(x => x.clave_producto_servicio != "80141703").ToList();
                    var doNormal = true;
                    var conceptsBiggerThanDiscount = concepts.Where(x => x.total >= discounted && x.exento_de_impuestos).ToList();
                    if (conceptsBiggerThanDiscount.Count() > 0)
                    {
                        var discountForEachProduct =
                             discounted / 1;
                        conceptsBiggerThanDiscount[0].descuento = discountForEachProduct;
                        doNormal = false;
                    }
                    if (doNormal)
                    {
                        // Prorrateo
                        var totalOfConcepts = concepts.Select(x => x.total).DefaultIfEmpty().Sum();
                        var totalOriginal = order.OrderItems.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
                        var totalPorcentage = (decimal)0;
                        if (concepts.Where(x => !x.exento_de_impuestos).Any())
                            totalOfConcepts = totalOriginal;
                        body.factura.impuesto_federal = 0;
                        body.factura.descuento = 0;
                        foreach (var concept in concepts)
                        {
                            if (concept.exento_de_impuestos)
                            {
                                var porcentage = (concept.total * 100) / totalOfConcepts;
                                totalPorcentage += porcentage;
                                var currentDiscount = discounted * (porcentage / 100);
                                concept.descuento = currentDiscount;
                                body.factura.descuento += currentDiscount;
                            }
                            else
                            {
                                var split = concept.descripcion.Split('|');
                                var conceptId =
                                    Int32.Parse(split[1]);
                                var orderItem = order.OrderItems.Where(x => x.Id == conceptId).FirstOrDefault();
                                // Get original price
                                var originalPrice = orderItem.PriceInclTax;
                                // Get discount
                                var porcentage = (originalPrice * 100) / totalOriginal;
                                totalPorcentage += porcentage;
                                var currentDiscount = discounted * (porcentage / 100);
                                // Get discount without rate
                                var discountWithoutIva = GetProductPrice(orderItem, currentDiscount);
                                concept.descuento = discountWithoutIva;
                                body.factura.descuento += discountWithoutIva;
                                // Get new Iva of concept
                                var newIva =
                                     (concept.total - discountWithoutIva) * (rate / 100);
                                body.factura.impuesto_federal += newIva;
                            }
                        }
                        // Re-add shipping rate to total
                        var shippingsInclExcl = CheckShipping(order);
                        var shippingIva = shippingsInclExcl.Item1 - shippingsInclExcl.Item2;
                        body.factura.impuesto_federal += shippingIva;
                    }
                    RoundFinalPrices(body);
                    var total = Math.Round(body.factura.subtotal + body.factura.impuesto_federal - body.factura.descuento, 2);
                    continuar = RedoDiscount(body, total, orderTotal);
                }
            }
            else
            {
                RoundFinalPrices(body);
            }
            return continuar;
        }

        public void GetRidOfIds(GetBillRequest body)
        {
            foreach (var concept in body.factura.conceptos)
            {
                concept.descripcion = concept.descripcion.Split('|')[0];
            }
        }

        public bool RedoDiscount(GetBillRequest body, decimal newTotal, decimal oldTotal)
        {
            var contin = true;
            var doWithIva = false;
            var totalDiscounts =
                Math.Round(body.factura.conceptos.Select(x => x.descuento).DefaultIfEmpty().Sum(), 2);
            var discount = body.factura.descuento;
            if ((newTotal - oldTotal > 0 && newTotal - oldTotal < 1) ||
                (oldTotal - newTotal > 0 && oldTotal - newTotal < 1))
            {
                var concepts =
                    body.factura.conceptos
                    .Where(x => x.clave_producto_servicio != "80141703" && x.exento_de_impuestos)
                    .ToList();
                if (!concepts.Any())
                {
                    concepts =
                        body.factura.conceptos
                        .Where(x => x.clave_producto_servicio != "80141703" && !x.exento_de_impuestos)
                        .ToList();
                }
                var left = newTotal - oldTotal;
                foreach (var concept in concepts)
                {
                    if (left > 0)
                    {

                        if (concept.descuento + left <= concept.total)
                        {
                            concept.descuento = Math.Round(concept.descuento + left, 2);
                            body.factura.descuento = Math.Round(body.factura.descuento + left, 2);
                            break;
                        }
                    }
                    else
                    {
                        var currentLeft = left * -1;
                        if (concept.descuento - currentLeft >= 0)
                        {
                            concept.descuento = Math.Round(concept.descuento - currentLeft, 2);
                            body.factura.descuento = Math.Round(body.factura.descuento - currentLeft, 2);
                            break;
                        }
                    }
                }
                var total =
                     body.factura.subtotal + body.factura.impuesto_federal - body.factura.descuento;
                if (Math.Round(total, 2) != Math.Round(oldTotal, 2))
                    contin = false;
            }

            totalDiscounts =
                Math.Round(body.factura.conceptos.Select(x => x.descuento).DefaultIfEmpty().Sum(), 2);
            discount = body.factura.descuento;
            if ((totalDiscounts - discount > 0 && totalDiscounts - discount < 1) ||
                (discount - totalDiscounts > 0 && discount - totalDiscounts < 1))
            {
                var concepts =
                    body.factura.conceptos
                    .Where(x => x.clave_producto_servicio != "80141703" && x.exento_de_impuestos)
                    .ToList();
                if (!concepts.Any())
                {
                    concepts =
                        body.factura.conceptos
                        .Where(x => x.clave_producto_servicio != "80141703" && !x.exento_de_impuestos)
                        .ToList();
                }
                var left = totalDiscounts - discount;
                foreach (var concept in concepts)
                {
                    if (left > 0)
                    {
                        if (concept.descuento - left >= 0)
                        {
                            concept.descuento = Math.Round(concept.descuento - left, 2);
                            break;
                        }
                    }
                    else
                    {
                        var currentLeft = left * -1;
                        if (concept.descuento + currentLeft <= concept.total)
                        {
                            concept.descuento = Math.Round(concept.descuento + currentLeft, 2);
                            break;
                        }
                    }
                }
                totalDiscounts =
                    Math.Round(body.factura.conceptos.Select(x => x.descuento).DefaultIfEmpty().Sum(), 2);
                if (Math.Round(totalDiscounts, 2) != Math.Round(discount, 2))
                    contin = false;
            }
            return contin;
        }

        private bool CheckCeilingAndFix(Order order, GetBillRequest body, decimal rate)
        {
            var canDo = true;
            if (body.factura.total != Math.Round(order.OrderSubtotalInclTax + order.OrderShippingInclTax, 2))
            {
                var orderTaxOnly = (decimal)0;
                // Check tax
                foreach (var orderItem in order.OrderItems)
                {
                    orderTaxOnly +=
                         orderItem.PriceInclTax - GetProductPrice(orderItem);
                }
                orderTaxOnly += order.OrderShippingInclTax - order.OrderShippingExclTax;

                var taxDifference = body.factura.impuesto_federal - orderTaxOnly;

                body.factura.impuesto_federal =
                         body.factura.impuesto_federal - taxDifference;
                body.factura.total =
                     body.factura.total - taxDifference;

                if (Math.Round(body.factura.total, 2) !=
                    Math.Round(order.OrderSubtotalInclTax, 2) + Math.Round(order.OrderShippingInclTax, 2))
                    canDo = false;
            }
            return canDo;
        }

        private Tuple<decimal, decimal> CheckShipping(Order order)
        {
            var shippingIncl = order.OrderShippingInclTax;
            var shippingExcl = order.OrderShippingExclTax;
            var tuple = new Tuple<decimal, decimal>(shippingIncl, shippingExcl);
            if (shippingIncl == shippingExcl)
            {
                var rate =
                    _settingService.GetSettingByKey<decimal>
                    (string.Format("Tax.TaxProvider.FixedOrByCountryStateZip.TaxCategoryId{0}", _taxSettings.DefaultTaxCategoryId));
                shippingExcl = shippingIncl / (1 + (rate / 100));
                tuple = new Tuple<decimal, decimal>(shippingIncl, shippingExcl);
            }
            return tuple;
        }

        private string GetProductCatalogId(OrderItem item)
        {
            ProductSatCode productSatCode = _productSatCodeService.GetByProductId(item.ProductId).FirstOrDefault();
            if (productSatCode != null)
            {
                return productSatCode.ProductCatalogId;
            }
            //var categoryId = item.Product.ProductCategories.Select(x => x.Category).OrderBy(x => x.ParentCategoryId).Select(x => x.Id).FirstOrDefault();
            //if (categoryId != 0)
            //{
            //    var categoryProductCatalog = _categoryProductCatalogService.GetByCategoryId(categoryId);
            //    if (categoryProductCatalog != null) return categoryProductCatalog.ProductCatalogId;
            //}
            return "01010101"; // Default value: No existe en el catálogo
        }

        private decimal GetQuantity(OrderItem item)
        {
            if (item.WeightInterval > 0)
            {
                decimal value = item.Quantity * item.WeightInterval;
                return value / 1000;
            }
            else if (item.BuyingBySecondary && item.EquivalenceCoefficient > 0)
            {
                decimal value = (item.Quantity * 1000) / item.EquivalenceCoefficient;
                return value / 1000;
            }
            else
            {
                return item.Quantity;
            }
        }

        private string GetComplementItemName(OrderItem item)
        {
            decimal value = 0;
            if (item.WeightInterval > 0)
            {
                value = (item.Quantity * item.WeightInterval) / 1000;
            }
            else if (item.BuyingBySecondary && item.EquivalenceCoefficient > 0)
            {
                value = ((item.Quantity * 1000) / item.EquivalenceCoefficient) / 1000;
            }

            string unit = value > 0 ? "kilos" : "unidades";
            value = value > 0 ? value : item.Quantity;

            return $" (paquete de {Math.Round(value, 2).ToString(CultureInfo.InvariantCulture)} {unit})";
        }

        private decimal GetUnitValue(OrderItem item)
        {
            var qty = GetQuantity(item);
            var exempt = IsProductTaxExempt(item);
            if (qty > 0)
                if (exempt)
                    return GetProductPrice(item, item.PriceInclTax / qty);
                else
                    return GetProductPrice(item, item.PriceExclTax / qty);
            else
                return 0;
            //return qty > 0 ? item.PriceExclTax / qty : 0;
        }

        private async Task<CustomerResult> RegisterCustomer(FacturifySettings settings, CreateBillModel model)
        {
            string token = await ValidateAndGetToken(settings);
            if (!string.IsNullOrWhiteSpace(token))
            {
                using (HttpClient client = new HttpClient())
                {
                    AddTokenToRequest(client, token);
                    var body = new Dictionary<string, string>
                    {
                        { "razon_social", model.BusinessName },
                        { "rfc", model.Rfc },
                        { "email", model.Email },
                        { "uso_cfdi", CfdiUse.G03.ToString() },
                        { "metodo_de_pago", PaymentMethods.PUE.ToString() },
                        { "forma_de_pago", ((int)PaymentForms.Efectivo).ToString("00") }
                    };
                    var content = new FormUrlEncodedContent(body);
                    var result = await client.PostAsync((settings.Sandbox ? API_SANDBOX_URL : API_URL) + API_REGISTER_CUSTOMER, content);
                    if (result.IsSuccessStatusCode)
                    {
                        var json = await result.Content.ReadAsStringAsync();
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<CustomerResult>(json);
                    }
                    else
                    {
                        await RegisterErrors(result);
                        _logger.Error("[Facturify - RClient] Error making a request to API.");
                    }
                }
            }
            return null;
        }

        private async Task<CustomerResult> UpdateCustomer(FacturifySettings settings, CreateBillModel model, CustomerResult customer)
        {
            string token = await ValidateAndGetToken(settings);
            if (!string.IsNullOrWhiteSpace(token))
            {
                using (HttpClient client = new HttpClient())
                {
                    AddTokenToRequest(client, token);
                    var body = new Dictionary<string, string>
                    {
                        { "uuid", customer.data.uuid },
                        { "razon_social", model.BusinessName },
                        { "rfc", model.Rfc },
                        { "email", model.Email },
                        { "forma_de_pago", customer.data.forma_de_pago},
                        { "tarjeta_ultimos_4digitos", model.CardDigits },
                        { "uso_cfdi", CfdiUse.G03.ToString() }
                    };
                    var content = new FormUrlEncodedContent(body);
                    var result = await client.PutAsync((settings.Sandbox ? API_SANDBOX_URL : API_URL) + API_UPDATE_CUSTOMER + customer.data.uuid, content);
                    if (result.IsSuccessStatusCode)
                    {
                        var json = await result.Content.ReadAsStringAsync();
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<CustomerResult>(json);
                    }
                    else
                    {
                        await RegisterErrors(result);
                        _logger.Error("[Facturify - RClient] Error making a request to API.");
                    }
                }
            }
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanyInformation(string uuid, bool isSandbox = true)
        {
            FacturifySettings settings = _settingService.LoadSetting<FacturifySettings>();
            string token = await ValidateAndGetToken(settings);
            if (!string.IsNullOrWhiteSpace(token))
            {
                using (HttpClient client = new HttpClient())
                {
                    AddTokenToRequest(client, token);
                    var result = await client.GetAsync((isSandbox ? API_SANDBOX_URL : API_URL) + API_UPDATE_COMPANY + settings.FacturifyUuid);

                    var json = await result.Content.ReadAsStringAsync();
                    return Ok(json);
                }
            }
            return Ok("Token is null");
        }

        private async Task UpdateCompany(FacturifySettings settings, FacturifySettings originalSettings)
        {
            string token = await ValidateAndGetToken(settings);
            if (!string.IsNullOrWhiteSpace(token))
            {
                using (HttpClient client = new HttpClient())
                {
                    AddTokenToRequest(client, token);
                    var body = new Dictionary<string, string>
                        {
                            { "razon_social", settings.BusinessName },
                            { "regimen", EnumHelper.GetDisplayName(settings.Regime) },
                            { "email", settings.Email },
                            { "cp", settings.AddressPostalCode },
                            { "calle", settings.AddressStreet },
                            { "num_ext", settings.AddressExteriorNumber },
                            { "num_int", settings.AddressInternalNumber },
                            { "colonia", settings.AddressSuburb },
                            { "delegacion_municipio", settings.AddressMunicipality },
                            { "ciudad", settings.AddressCity },
                            { "estado", settings.AddressState }
                        };

                    if (originalSettings?.RFC != settings.RFC)
                    {
                        body.Add("rfc", settings.RFC);
                        body.Add("csd_key_file", settings.CsdKeyFileBase64);
                        body.Add("csd_cer_file", settings.CsdCerFileBase64);
                        body.Add("csd_key_file_password", settings.CsdKeyFilePassword);
                        body.Add("fiel_key_file", settings.FielKeyFileBase64);
                        body.Add("fiel_cer_file", settings.FielCerFileBase64);
                        body.Add("fiel_key_file_password", settings.FielKeyFilePassword);
                    }

                    var content = new FormUrlEncodedContent(body);
                    var result = await client.PutAsync((settings.Sandbox ? API_SANDBOX_URL : API_URL) + API_UPDATE_COMPANY + settings.FacturifyUuid, content);
                    if (result.IsSuccessStatusCode)
                    {
                        var json = await result.Content.ReadAsStringAsync();
                        CompanyResult data = Newtonsoft.Json.JsonConvert.DeserializeObject<CompanyResult>(json);
                        if (data.code != 0)
                        {
                            _logger.Error("[Facturify - UC] Error registering company. " + data?.message);
                        }
                    }
                    else
                    {
                        await RegisterErrors(result);
                        _logger.Error("[Facturify - UC] Error making a request to API.");
                    }
                }
            }
        }

        private async Task RegisterCompany(FacturifySettings settings)
        {
            string token = await ValidateAndGetToken(settings);
            if (!string.IsNullOrWhiteSpace(token))
            {
                using (HttpClient client = new HttpClient())
                {
                    AddTokenToRequest(client, token);

                    var body = new Dictionary<string, string>
                        {
                            { "razon_social", settings.BusinessName },
                            { "rfc", settings.RFC },
                            { "regimen", EnumHelper.GetDisplayName(settings.Regime) },
                            { "email", settings.Email },
                            { "cp", settings.AddressPostalCode },
                            { "calle", settings.AddressStreet },
                            { "num_ext", settings.AddressExteriorNumber },
                            { "num_int", settings.AddressInternalNumber },
                            { "colonia", settings.AddressSuburb },
                            { "delegacion_municipio", settings.AddressMunicipality },
                            { "ciudad", settings.AddressCity },
                            { "estado", settings.AddressState },
                            { "csd_key_file", settings.CsdKeyFileBase64 },
                            { "csd_cer_file", settings.CsdCerFileBase64 },
                            { "csd_key_file_password", settings.CsdKeyFilePassword },
                            { "fiel_key_file", settings.FielKeyFileBase64 },
                            { "fiel_cer_file", settings.FielCerFileBase64 },
                            { "fiel_key_file_password", settings.FielKeyFilePassword }
                        };

                    var content = new FormUrlEncodedContent(body);
                    var result = await client.PostAsync((settings.Sandbox ? API_SANDBOX_URL : API_URL) + API_REGISTER_COMPANY, content);
                    if (result.IsSuccessStatusCode)
                    {
                        var json = await result.Content.ReadAsStringAsync();
                        CompanyResult data = Newtonsoft.Json.JsonConvert.DeserializeObject<CompanyResult>(json);
                        if (data.code == 0 && !string.IsNullOrWhiteSpace(data.data.empresa.uuid))
                        {
                            settings.FacturifyUuid = data.data.empresa.uuid;
                            _settingService.SaveSetting(settings);
                        }
                        else
                        {
                            _logger.Error("[Facturify - RC] Error registering company. " + data?.message);
                        }
                    }
                    else
                    {
                        await RegisterErrors(result);
                    }
                }
            }
        }

        private async Task RegisterErrors(HttpResponseMessage result)
        {
            var json = await result.Content.ReadAsStringAsync();
            TokenResult data = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResult>(json);
            if (data.errors != null)
            {
                ErrorMessages.AddRange(data.errors.Select(x => x.message).ToList());
            }
            else
            {
                ErrorMessages.Add(data.message);
            }
        }

        private async Task<string> ValidateAndGetToken(FacturifySettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Token))
            {
                return await SaveNewToken(settings);
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    AddTokenToRequest(client, settings.Token);
                    var result = await client.GetAsync((settings.Sandbox ? API_SANDBOX_URL : API_URL) + API_VALIDATE_TOKEN);
                    if (result.IsSuccessStatusCode)
                    {
                        string json = await result.Content.ReadAsStringAsync();
                        TokenResult tokenResult = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResult>(json);
                        if (string.IsNullOrWhiteSpace(tokenResult.code)) // Token is valid
                        {
                            return settings.Token;
                        }
                        else
                        {
                            _logger.Error("[Facturify - VT] Error validating Token. " + tokenResult?.message);
                            return null;
                        }
                    }
                    else
                    {
                        var json = await result.Content.ReadAsStringAsync();
                        TokenResult data = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResult>(json);
                        if (data.code == "51")
                        {
                            return await RefreshToken(settings);
                        }
                        else
                        {
                            if (data.errors != null)
                                ErrorMessages.AddRange(data.errors.Select(x => x.message));
                            else
                            {
                                if (data.code == "400" || data.code == null)
                                {
                                    await SaveNewToken(settings);
                                    ErrorMessages.AddRange(new List<string> {
                                        "Para poder continuar con su factura es necesario refrescar la pagina y volver a intentarlo"
                                    });
                                }
                                else
                                    ErrorMessages.AddRange(new List<string> {
                                        "No errors from request, showing regular message if any",
                                        data.message
                                    });

                            }
                            _logger.Error("[Facturify - RC] Error making a request to API.");
                            return null;
                        }
                    }
                }
            }
        }

        private async Task<string> RefreshToken(FacturifySettings settings)
        {
            using (HttpClient client = new HttpClient())
            {
                AddTokenToRequest(client, settings.Token);
                var result = await client.PostAsync((settings.Sandbox ? API_SANDBOX_URL : API_URL) + API_REFRESH_TOKEN, null);
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    TokenResult tokenResult = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResult>(json);
                    if (!string.IsNullOrWhiteSpace(tokenResult.jwt.token))
                    {
                        settings.Token = tokenResult.jwt.token;
                        _settingService.SaveSetting(settings);
                        return tokenResult.jwt.token;
                    }
                    else
                    {
                        _logger.Error("[Facturify - RT] Error getting refresh token. " + tokenResult?.message);
                        return null;
                    }
                }
                else
                {
                    var json = await result.Content.ReadAsStringAsync();
                    TokenResult data = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResult>(json);
                    if (data.code == "51")
                    {
                        return await SaveNewToken(settings);
                    }
                    else
                    {
                        ErrorMessages.AddRange(data.errors.Select(x => x.message));
                        _logger.Error("[Facturify - RC] Error making a request to API.");
                        return null;
                    }
                }
            }
        }

        private async Task<string> SaveNewToken(FacturifySettings settings, bool saveWithPrevious = false)
        {
            TokenResult tokenResult = await GetToken(settings.ApiKey, settings.ApiSecret, settings.Sandbox);
            if (tokenResult == null || !string.IsNullOrEmpty(tokenResult?.code))
            {
                _logger.Error("[Facturify] Error getting new token. " + tokenResult?.message);
                return null;
            };
            if (saveWithPrevious)
            {
                settings = _settingService.LoadSetting<FacturifySettings>();
                settings.Token = tokenResult.jwt.token;
                _settingService.SaveSetting(settings);
            }
            else
            {
                settings.Token = tokenResult.jwt.token;
                _settingService.SaveSetting(settings);
            }
            return tokenResult.jwt.token;
        }

        private async Task<TokenResult> GetToken(string apiKey, string apiSecret, bool sandbox = true)
        {
            using (HttpClient client = new HttpClient())
            {
                var body = new Dictionary<string, string>
                {
                    { "api_key", apiKey },
                    { "api_secret", apiSecret }
                };
                var content = new FormUrlEncodedContent(body);

                var result = await client.PostAsync((sandbox ? API_SANDBOX_URL : API_URL) + API_GET_TOKEN, content);
                if (result.IsSuccessStatusCode)
                {
                    string resultJson = await result.Content.ReadAsStringAsync();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResult>(resultJson);
                }
                else
                {
                    await RegisterErrors(result);
                }

                return null;
            }
        }

        private void AddTokenToRequest(HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        }

        private string ConvertFileToBase64(IFormFile file)
        {
            if (file != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    byte[] fileBytes = ms.ToArray();
                    return Convert.ToBase64String(fileBytes);
                }
            }
            return null;
        }

        public IActionResult CreateProductSatCode(int productId, string productCodeId)
        {
            if (productId == 0 || productCodeId == null)
            { return BadRequest(); }

            ProductSatCode productSatCode = new ProductSatCode();
            productSatCode.ProductId = productId;
            productSatCode.ProductCatalogId = productCodeId;

            _productSatCodeService.Insert(productSatCode);

            return Ok();
        }

        public string GetProductSatCode(int productId)
        {
            if (productId == 0) { return null; }

            ProductSatCode productSatCode = _productSatCodeService.GetByProductId(productId).FirstOrDefault();

            if (productSatCode == null)
            {
                return null;
            }
            else
            {
                return productSatCode.ProductCatalogId;
            }
        }

        public string GetAllProductSatCode()
        {
            List<ProductSatCode> productsSatCode = _productSatCodeService.GetAll().ToList();

            if (productsSatCode == null)
            {
                return null;
            }
            else
            {
                var jsonSatCodes = JsonConvert.SerializeObject(productsSatCode);
                return jsonSatCodes;
            }
        }

        public IActionResult UpdateProductSatCode(int productId, string productCodeId)
        {
            if (productId == 0 || productCodeId == null)
            { return BadRequest(); }

            ProductSatCode productSatCode = _productSatCodeService.GetByProductId(productId).FirstOrDefault();

            if (productSatCode != null)
            {
                productSatCode.ProductCatalogId = productCodeId;
                _productSatCodeService.Update(productSatCode);
            }
            else
            {
                productSatCode = new ProductSatCode();
                productSatCode.ProductId = productId;
                productSatCode.ProductCatalogId = productCodeId;
                _productSatCodeService.Insert(productSatCode);
            }

            return Ok();
        }

        public IActionResult UpdateManyProductSatCode(string productSATCodesList)
        {
            if (string.IsNullOrEmpty(productSATCodesList))
            { return BadRequest(); }

            List<ProductSatCode> productSatCodesJson = JsonConvert.DeserializeObject<List<ProductSatCode>>(productSATCodesList);
            var productIds = productSatCodesJson.Select(x => x.ProductId).ToList();
            var productSatCodes = _productSatCodeService.GetAll().Where(x => productIds.Contains(x.ProductId)).ToList();
            if (productSatCodesJson.Count() > 0)
            {
                foreach (var item in productSatCodesJson)
                {
                    ProductSatCode productSatCode = productSatCodes.Where(x => x.ProductId == item.ProductId).FirstOrDefault();
                    if (productSatCode != null)
                    {
                        if (productSatCode.ProductCatalogId == item.ProductCatalogId) continue;
                        productSatCode.ProductCatalogId = item.ProductCatalogId;
                        _productSatCodeService.Update(productSatCode);
                    }
                    else
                    {
                        productSatCode = new ProductSatCode();
                        productSatCode.ProductId = item.ProductId;
                        productSatCode.ProductCatalogId = item.ProductCatalogId;
                        _productSatCodeService.Insert(productSatCode);
                    }
                }
            }

            return Ok();
        }

        int currentRow = 0;
        [HttpGet]
        public IActionResult SetSatCodesFromExcel()
        {
            var products = _productService.GetAllProductsQuery().Where(x => !x.Deleted).ToList();

            // path to your excel file
            string path = "C:/Users/Ivan Salazar/Source/TeedCommerce_Web/Presentation/Nop.Web/Plugins/Teed.Plugin.Facturify/files/cel_productos_SAT.xlsx";
            FileInfo fileInfo = new FileInfo(path);
            var errorList = new List<CellErrorModel>();

            using (var memoryStream = new MemoryStream())
            {
                using (var package = new ExcelPackage(fileInfo.OpenRead()))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet != null)
                    {
                        int init = 0;
                        List<string> headers = new List<string>();
                        int totalRows = worksheet.Dimension.End.Row;
                        int totalColumns = worksheet.Dimension.End.Column;
                        var range = worksheet.Cells[1, 1, 1, totalColumns];

                        GetHeaders(ref init, ref headers, totalColumns, range, worksheet);
                        if (!headers.Contains("Id") ||
                            !headers.Contains("Sku") ||
                            !headers.Contains("Nombre") ||
                            !headers.Contains("Categorías padre") ||
                            !headers.Contains("Categorías hijo") ||
                            !headers.Contains("Código del SAT")
                            )
                        {
                            return BadRequest("El archivo no tiene las columnas correctas.");
                        }

                        try
                        {
                            var cells = worksheet.Cells.ToList();
                            var groups = GetCellGroups(cells, worksheet.Dimension.End.Row - 1);
                            if (groups == null) return BadRequest("Ocurrió un problema creando los grupos para la carga de datos");
                            var dataList = new List<CellObjectModel>();
                            for (int g = 0; g < groups.Count; g++)
                            {
                                currentRow = g;
                                int currentColumn = 0;
                                var data = new CellObjectModel();
                                data.Id = groups[g][currentColumn].Value != null && !string.IsNullOrWhiteSpace(groups[g][currentColumn].Value.ToString()) ? (int?)int.Parse(groups[g][currentColumn].Value.ToString()) : null;
                                currentColumn++;
                                data.Sku = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.Name = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.ParentCategories = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.ChildCategories = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.SatCode = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                dataList.Add(data);
                            }

                            // Start uploading data
                            int count = 0;
                            foreach (var data in dataList)
                            {
                                count++;
                                if (data.Id.HasValue && data.Id.Value > 0)
                                {
                                    var productId = data.Id ?? 0;
                                    var product = products.Where(x => x.Id == productId).FirstOrDefault();
                                    if (product == null)
                                        errorList.Add(new CellErrorModel
                                        {
                                            Error = "Producto no encontrado (SKU: " + data.Sku + ") - En linea del xls: " + count,
                                            CellObjectModel = data
                                        });
                                    else
                                    {
                                        var productSatCode = _productSatCodeService.GetByProductId(productId).FirstOrDefault();
                                        if (productSatCode == null)
                                        {
                                            var newProductSatCode = new ProductSatCode
                                            {
                                                ProductId = productId,
                                                ProductCatalogId = data.SatCode
                                            };
                                            _productSatCodeService.Insert(newProductSatCode);
                                        }
                                        else
                                        {
                                            productSatCode.ProductCatalogId = data.SatCode;
                                            _productSatCodeService.Update(productSatCode);
                                        }
                                    }
                                }
                                else
                                {
                                    errorList.Add(new CellErrorModel
                                    {
                                        Error = "Producto sin Id especificado (SKU: " + data.Sku + ") - En linea del xls: " + count,
                                        CellObjectModel = data
                                    });
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            return BadRequest("Ocurrió un problema cargando los datos: " + e.Message);
                        }
                    }
                    else
                    {
                        return BadRequest("No fue posible cargar el excel");
                    }
                }
            }
            if (errorList.Any())
                return Ok("Se actualizaron codigos del sat pero algunos tuvieron errores: \n"
                    + string.Join("\n", errorList.Select(x => x.Error)));
            else
                return Ok("Los codigos del sat se actualizaron correctamente.");
        }

        private void GetHeaders(ref int init, ref List<string> headerList, int totalColumns, ExcelRange range, ExcelWorksheet sheet)
        {
            string[] initCol;
            for (int i = 1; i <= totalColumns; ++i)
            {
                if (!string.IsNullOrEmpty(sheet.Cells[1, i].Text))
                {
                    if (headerList.Count() == 0)
                    {
                        initCol = System.Text.RegularExpressions.Regex.Split(range[1, i].Address, @"\D+");
                        init = int.Parse(initCol[1]);
                    }
                    headerList.Add(sheet.Cells[1, i].Text);
                }
            }
        }

        private List<List<CellDataModel>> GetCellGroups(List<ExcelRangeBase> elements, int finalRow)
        {
            int i = 0;
            int g = 0;
            try
            {
                var list = new List<List<CellDataModel>>();
                var headerLetters = elements.Where(x => x.Start.Row == 1).Select(x => x.Address).Select(x => new String(x.Where(y => Char.IsLetter(y)).ToArray())).ToList();
                for (i = 0; i < finalRow; i++)
                {
                    var listData = new List<CellDataModel>();
                    for (g = 0; g < headerLetters.Count; g++)
                    {
                        var address = headerLetters[g] + (i + 2).ToString();
                        var element = elements.Where(x => x.Address == address).FirstOrDefault();
                        if (element == null || element.Value == null)
                        {
                            listData.Add(new CellDataModel() { Address = address, Value = null });
                        }
                        else
                        {
                            listData.Add(new CellDataModel() { Address = address, Value = element.Value.ToString() });
                        }
                    }
                    list.Add(listData);
                }

                return list;
            }
            catch (Exception w)
            {
                return null;
            }
        }
    }

    public class CellDataModel
    {
        public string Address { get; set; }
        public string Value { get; set; }
    }

    public class CellObjectModel
    {
        public int? Id { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public string ParentCategories { get; set; }
        public string ChildCategories { get; set; }
        public string SatCode { get; set; }
    }

    public class CellErrorModel
    {
        public CellObjectModel CellObjectModel { get; set; }
        public string Error { get; set; }
    }

    public class GetBillRequest
    {
        public object emisor { get; set; }
        public object receptor { get; set; }
        public GetBillData factura { get; set; }
        public int simulacion_error { get; set; }
    }

    public class GetBillResult
    {
        public string code { get; set; }
        public string message { get; set; }
        public GetBillResultData data { get; set; }
    }

    public class GetBillResultData
    {
        public string created_at { get; set; }
        public string cfdi_uuid { get; set; }
        public string xml { get; set; }
        public string pdf { get; set; }
        public string serie { get; set; }
        public int folio { get; set; }
        public string factura { get; set; }
        public string job_id { get; set; }
    }

    public class GetBillData
    {
        public string fecha { get; set; }
        public string tipo { get; set; }
        public string tipo_de_cambio { get; set; }
        public string moneda { get; set; }
        public bool generacion_automatica { get; set; }
        public decimal subtotal { get; set; }
        public decimal impuesto_federal { get; set; }
        public decimal descuento { get; set; }
        public decimal total { get; set; }
        public List<BillConcept> conceptos { get; set; }
    }

    public class BillConcept
    {
        public string clave_producto_servicio { get; set; }
        public string clave_unidad_de_medida { get; set; }
        public decimal cantidad { get; set; }
        public string descripcion { get; set; }
        public decimal valor_unitario { get; set; }
        public decimal descuento { get; set; }
        public decimal total { get; set; }
        public bool exento_de_impuestos { get; set; }
    }

    public class BillConceptWithOrderItem
    {
        public string clave_producto_servicio { get; set; }
        public string clave_unidad_de_medida { get; set; }
        public decimal cantidad { get; set; }
        public string descripcion { get; set; }
        public decimal valor_unitario { get; set; }
        public decimal total { get; set; }
        public bool exento_de_impuestos { get; set; }
        public OrderItem OrderItem { get; set; }
    }

    public class TokenResult
    {
        public string message { get; set; }
        public FacturifyToken jwt { get; set; }
        public string code { get; set; }
        public List<FacturifyError> errors { get; set; }
    }

    public class FacturifyToken
    {
        public string token { get; set; }
        public string expires_in { get; set; }
    }

    public class FacturifyError
    {
        public string field { get; set; }
        public string message { get; set; }
        public int code { get; set; }
    }

    public class Series
    {
        public SerieElements ingreso { get; set; }
        public SerieElements egreso { get; set; }
    }

    public class SerieElements
    {
        public string serie { get; set; }
        public string folio_inicial { get; set; }
    }

    public class CompanyResult
    {
        public string message { get; set; }
        public CompanyResultData data { get; set; }
        public int code { get; set; }
        public List<FacturifyError> errors { get; set; }
    }

    public class CustomerResult
    {
        public string message { get; set; }
        public CustomerResultData data { get; set; }
        public int code { get; set; }
        public List<FacturifyError> errors { get; set; }
    }

    public class CustomerResultData
    {
        public string razon_social { get; set; }
        public string rfc { get; set; }
        public string email { get; set; }
        public string forma_de_pago { get; set; }
        public string tarjeta_ultimos_4digitos { get; set; }
        public string updated_at { get; set; }
        public string created_at { get; set; }
        public int id { get; set; }
        public string uuid { get; set; }
        public string organizacion_uuid { get; set; }
        public string empresa_uuid { get; set; }
        public int activo { get; set; }
    }

    public class CompanyResultData
    {
        public CompanyData empresa { get; set; }
        public CertificateData certificate { get; set; }
    }

    public class CompanyData
    {
        public string email { get; set; }
        public string razon_social { get; set; }
        public string rfc { get; set; }
        public string regimen { get; set; }
        public string cp { get; set; }
        public string calle { get; set; }
        public string num_ext { get; set; }
        public string num_int { get; set; }
        public string colonia { get; set; }
        public string delegacion_municipio { get; set; }
        public string ciudad { get; set; }
        public string estado { get; set; }
        public string uuid { get; set; }
        public int created_by { get; set; }
        public string curp { get; set; }
        public bool factura_por_cuenta_de_terceros { get; set; }
        public string updated_at { get; set; }
        public string created_at { get; set; }
        public string id { get; set; }
    }

    public class CertificateData
    {
        public string serial_number { get; set; }
        public string valid_from { get; set; }
        public string valid_to { get; set; }
    }

    public class CreateBillModel
    {
        public int FaturifyAddressId { get; set; }
        public string BusinessName { get; set; }
        public string Rfc { get; set; }
        public string Email { get; set; }
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public string PaymentForm { get; set; }
        public string CardDigits { get; set; }
        public string CfdiUse { get; set; }
    }

    public class BillWebHookModel
    {
        public string module { get; set; }
        public string action { get; set; }
        public string status { get; set; }
        public GetBillResultData data { get; set; }
    }

    public class PdfConfiguration
    {
        public string empresa_uuid { get; set; }
        public List<PdfConfigurationImage> configuraciones { get; set; }

    }

    public class PdfConfigurationValue
    {
        public string key { get; set; }
        public string value { get; set; }

    }

    public class PdfConfigurationImage
    {
        public string key { get; set; }
        public PdfConfigurationImageValue value { get; set; }

    }

    public class PdfConfigurationImageValue
    {
        public string body { get; set; }
        public string mimetype { get; set; }

    }
}