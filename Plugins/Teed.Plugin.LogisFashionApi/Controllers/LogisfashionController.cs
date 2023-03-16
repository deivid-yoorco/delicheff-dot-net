using Nop.Services.Configuration;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using Nop.Services.Logging;
using Nop.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Teed.Plugin.Logisfashion.Models;
using Nop.Web.Framework;
using System.Net.Http;
using Teed.Plugin.Logisfashion.Filters;
using Teed.Plugin.Logisfashion.Services;
using System.Linq;
using Teed.Plugin.Logisfashion.Domain;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Catalog;
using Nop.Core.Domain.Catalog;
using System.Diagnostics;
using Nop.Web.Framework.Kendoui;
using Nop.Core;
using System.Data.Entity;
using Nop.Services.Helpers;
using Nop.Core.Domain.Orders;
using Microsoft.AspNetCore.Authorization;
using Nop.Services.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Shipping;
using System.Data.Entity.SqlServer;
using OfficeOpenXml;
using System.IO;
using Newtonsoft.Json;

namespace Teed.Plugin.Logisfashion.Controllers
{
    [Area(AreaNames.Admin)]
    public class LogisfashionController : BasePluginController
    {
        private const string API_URL = "https://logiseconnect-usa-pro.logisfashion.com";
        private const string API_URL_TEST = "https://logiseconnect-usa-pre.logisfashion.com";
        private const string MASTER_DATA_URL = "/api/ImportMasterData";
        private const string GET_PRODUCT_URL = "/api/sku";
        private const string INBOUNDS_URL = "/api/importinbounds";
        private const string OUTBOUNDS_URL = "/api/ImportOutbounds";
        private const string STOCK_URL = "/api/articles/sku/";
        private const string UPDATE_DIMENSIONS_URL = "/api/articles/updateDimensions";

        private readonly LogisfashionRequestLogService _logisfashionRequestLogService;
        private readonly ISettingService _settingService;
        private readonly IProductService _productService;
        private readonly ILogger _logger;
        private readonly IPermissionService _permissionService;
        private readonly IOrderService _orderService;
        private readonly IShipmentService _shipmentService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IWorkContext _workContext;

        private List<string> processedOrders = new List<string>();

        public LogisfashionController(ISettingService settingService, ILogger logger,
            IPermissionService permissionService, IProductService productService, IOrderService orderService,
            LogisfashionRequestLogService logisfashionRequestLogService, IShipmentService shipmentService,
            IOrderProcessingService orderProcessingService, IWorkContext workContext)
        {
            _settingService = settingService;
            _logger = logger;
            _permissionService = permissionService;
            _logisfashionRequestLogService = logisfashionRequestLogService;
            _productService = productService;
            _orderService = orderService;
            _shipmentService = shipmentService;
            _orderProcessingService = orderProcessingService;
            _workContext = workContext;
        }

        public IActionResult Index()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            LogisfashionSettings logisfashionSettings = _settingService.LoadSetting<LogisfashionSettings>();

            ConfigureModel model = new ConfigureModel()
            {
                ApiKey = logisfashionSettings.ApiKey,
                ClientCode = logisfashionSettings.ClientCode,
                Sandbox = logisfashionSettings.Sandbox
            };

            return View("~/Plugins/Teed.Plugin.Logisfashion/Views/Logisfashion/Index.cshtml", model);
        }

        public IActionResult UpdateProducts()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Logisfashion/Views/Logisfashion/UpdateProducts.cshtml");
        }

        public IActionResult ExcelUpload()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            ViewData["Error"] = null;
            ViewData["Success"] = false;

            return View("~/Plugins/Teed.Plugin.Logisfashion/Views/Logisfashion/ExcelUpload.cshtml");
        }

        [HttpGet]
        public IActionResult CleanErrorLog()
        {
            if (_workContext.CurrentCustomer.Email != "cmartinez@teed.com.mx") return Unauthorized();

            List<LogisfashionRequestLog> logs = _logisfashionRequestLogService.GetAll().Where(x => x.Error).ToList();
            foreach (var log in logs)
            {
                _logisfashionRequestLogService.Delete(log);
            }
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> ExcelUpload(ExcelUploadModel model)
        {
            if (model.ExcelFile == null || model.ExcelFile.Length == 0)
                return NoContent();

            string fileExtension = Path.GetExtension(model.ExcelFile.FileName);

            if (fileExtension == ".xls" || fileExtension == ".xlsx")
            {
                using (var fileStram = model.ExcelFile.OpenReadStream())
                {
                    ExcelPackage package = new ExcelPackage(fileStram);

                    ExcelWorksheet workSheet = package.Workbook.Worksheets.First();
                    int totalRows = workSheet.Dimension.Rows;

                    if (workSheet.Cells[1, 1].Value.ToString().ToUpper() == "EAN" && workSheet.Cells[1, 2].Value.ToString().ToUpper() == "QUANTITY")
                    {
                        var dataList = new List<InboundExcelModel>();
                        for (int i = 2; i <= totalRows; i++)
                        {
                            dataList.Add(new InboundExcelModel
                            {
                                Gtin = workSheet.Cells[i, 1].Value.ToString(),
                                Quantity = workSheet.Cells[i, 2].Value.ToString(),
                            });
                        }

                        string result = await CreateInboundRequest(dataList, model.IsAssortment, model.OrderNumber);
                        if (string.IsNullOrWhiteSpace(result))
                        {
                            ViewData["Error"] = null;
                            ViewData["Success"] = true;
                        }
                        else
                        {
                            ViewData["Error"] = result;
                            ViewData["Success"] = false;

                        }
                    }
                    else
                    {
                        ViewData["Error"] = "El excel es inválido.";
                        ViewData["Success"] = false;
                    }
                }
            }
            else
            {
                ViewData["Error"] = "El archivo es inválido.";
                ViewData["Success"] = false;
            }

            return View("~/Plugins/Teed.Plugin.Logisfashion/Views/Logisfashion/ExcelUpload.cshtml");
        }

        public IActionResult Logs()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Logisfashion/Views/Logisfashion/Logs.cshtml");
        }

        [HttpPost]
        public IActionResult Index(ConfigureModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            LogisfashionSettings logisfashionSettings = _settingService.LoadSetting<LogisfashionSettings>();

            logisfashionSettings.ApiKey = model.ApiKey;
            logisfashionSettings.ClientCode = model.ClientCode;
            logisfashionSettings.Sandbox = model.Sandbox;

            _settingService.SaveSetting(logisfashionSettings);

            return View("~/Plugins/Teed.Plugin.Logisfashion/Views/Logisfashion/Index.cshtml", model);
        }

        [HttpPost]
        [ApiKeyAuthotization]
        [Route("api/[controller]/[action]")]
        public IActionResult InboundWebhook([FromBody] InboundStatusNotificationModel model)
        {
            try
            {
                LogisfashionRequestLog notification = _logisfashionRequestLogService.GetAll()
                .Where(x => x.LogInternalId == model.InboundStatusNotificacion.PONumber)
                .FirstOrDefault();

                if (notification == null)
                {
                    _logisfashionRequestLogService.Insert(CreateIncomingLog(model, "404", 0, "No encontramos ningún registro con el albarán indicado"));
                    return NotFound(new WebHookResponse()
                    {
                        Error = true,
                        ErrorMessage = "No encontramos ningún registro con el albarán indicado",
                        StatusCode = 404
                    });
                }

                StatusType? status = GetStatusEnumValue(model.InboundStatusNotificacion.Status.Name);
                if (!status.HasValue)
                {
                    _logisfashionRequestLogService.Insert(CreateIncomingLog(model, "400", notification.Id, $"No se reconoce el status \"{model.InboundStatusNotificacion.Status.Name}\"."));
                    return BadRequest(new WebHookResponse()
                    {
                        Error = true,
                        ErrorMessage = $"No se reconoce el status \"{model.InboundStatusNotificacion.Status.Name}\".",
                        StatusCode = 400
                    });
                }

                notification.StatusType = status.Value;
                notification.LastResponseJson = JsonConvert.SerializeObject(model);
                notification.StatusDescription = model.InboundStatusNotificacion.Status.Name;

                _logisfashionRequestLogService.Update(notification);

                if (notification.StatusType == StatusType.Finished)
                {
                    List<ProductToUpdateModel> productsToUpdate = new List<ProductToUpdateModel>();

                    foreach (var item in model.InboundStatusNotificacion.ItemList)
                    {
                        Product product = _productService.GetAllProductsQuery().Where(x => x.Gtin.Substring(SqlFunctions.PatIndex("%[^0]%", x.Gtin).Value - 1) == item.SKU || x.Gtin == item.SKU).FirstOrDefault();
                        if (product != null)
                        {
                            ProductWarehouseInventory productWarehouseInventory = product.ProductWarehouseInventory.FirstOrDefault();
                            int previousStockQuantityIn = 0;
                            int qty = item.ReceivedQuantity;
                            int newStock = 0;
                            if (productWarehouseInventory == null)
                            {
                                previousStockQuantityIn = product.StockQuantity;
                                newStock = previousStockQuantityIn + qty;
                                product.StockQuantity = newStock;
                            }
                            else
                            {
                                previousStockQuantityIn = productWarehouseInventory.StockQuantity;
                                newStock = previousStockQuantityIn + qty;
                                productWarehouseInventory.StockQuantity = newStock;
                            }

                            if (newStock > 0)
                                product.Published = true;

                            productsToUpdate.Add(new ProductToUpdateModel()
                            {
                                NewStock = newStock,
                                PreviousStockQuantityIn = previousStockQuantityIn,
                                Product = product,
                                ProductWarehouseInventory = productWarehouseInventory
                            });
                        }
                        else
                        {
                            _logisfashionRequestLogService.Insert(CreateIncomingLog(model, "404", notification.Id, "NO SE ENCONTRÓ EL PRODUCTO CON GTIN " + item.SKU + " AL RECIBIR UN INBOUND POR PARTE DE LOGISFASHION"));
                            return BadRequest(new WebHookResponse()
                            {
                                Error = true,
                                ErrorMessage = "NO SE ENCONTRÓ EL PRODUCTO CON EAN/SKU " + item.SKU,
                                StatusCode = 400
                            });
                        }
                    }

                    foreach (var item in productsToUpdate)
                    {
                        string sku = item.Product.Gtin;
                        try
                        {
                            _productService.UpdateProduct(item.Product);
                            _productService.AddStockQuantityHistoryEntry(item.Product, item.NewStock - item.PreviousStockQuantityIn, item.NewStock,
                                item.ProductWarehouseInventory != null ? item.ProductWarehouseInventory.WarehouseId : 0, "Se agregaron cantidades al producto desde Logisfashion.");
                        }
                        catch (Exception e)
                        {
                            Debugger.Break();
                            _logisfashionRequestLogService.Insert(CreateIncomingLog(model, "500", 0, "Ocurrió un problema al actualizar el producto con EAN " + sku + ". " + e.Message));
                            return BadRequest(new WebHookResponse()
                            {
                                Error = true,
                                ErrorMessage = "Ocurrió un problema al actualizar el producto con EAN " + sku + " por favor avisar al administrador",
                                StatusCode = 500
                            });
                        }
                    }

                    _logisfashionRequestLogService.Insert(CreateIncomingLog(model, "200", notification.Id));
                }
            }
            catch (Exception e)
            {
                _logisfashionRequestLogService.Insert(CreateIncomingLog(model, "400", 0, e.Message));
                return BadRequest(new WebHookResponse()
                {
                    Error = true,
                    ErrorMessage = e.Message,
                    StatusCode = 400
                });
            }

            return Ok(new WebHookResponse()
            {
                Error = false,
                ErrorMessage = null,
                StatusCode = 200
            });
        }

        [HttpPost]
        [ApiKeyAuthotization]
        [Route("api/[controller]/[action]")]
        public IActionResult OutBoundWebhook([FromBody] OutboundStatusNotificationModel model)
        {
            try
            {
                LogisfashionRequestLog notification = _logisfashionRequestLogService.GetAll()
                .Where(x => x.LogInternalId == model.OutboundStatusNotification.DeliveryNumber)
                .FirstOrDefault();

                if (notification == null)
                {
                    _logisfashionRequestLogService.Insert(CreateIncomingLog(model, "404", 0, "No encontramos ningún registro con el DeliveryNumber indicado."));
                    return NotFound(new WebHookResponse()
                    {
                        Error = true,
                        ErrorMessage = "No encontramos ningún registro con el DeliveryNumber indicado.",
                        StatusCode = 404
                    });
                }

                StatusType? status = GetStatusEnumValue(model.OutboundStatusNotification.Status.Name);
                if (!status.HasValue)
                {
                    _logisfashionRequestLogService.Insert(CreateIncomingLog(model, "400", notification.Id, $"No se reconoce el status \"{model.OutboundStatusNotification.Status.Name}\"."));
                    return BadRequest(new WebHookResponse()
                    {
                        Error = true,
                        ErrorMessage = $"No se reconoce el status \"{model.OutboundStatusNotification.Status.Name}\".",
                        StatusCode = 400
                    });
                }

                notification.StatusType = status.Value;
                notification.LastResponseJson = JsonConvert.SerializeObject(model);
                notification.StatusDescription = model.OutboundStatusNotification.Status.Name;

                if (notification.StatusType == StatusType.Shipped)
                {
                    Order order = _orderService.GetOrderById(notification.RelatedElementId);
                    if (order != null)
                    {
                        Shipment shipment = null;
                        decimal? totalWeight = null;
                        foreach (var orderItem in order.OrderItems)
                        {
                            //is shippable
                            if (!orderItem.Product.IsShipEnabled)
                                continue;

                            //ensure that this product can be shipped (have at least one item to ship)
                            var maxQtyToAdd = orderItem.GetTotalNumberOfItemsCanBeAddedToShipment();
                            if (maxQtyToAdd <= 0)
                                continue;

                            var qtyToAdd = orderItem.Quantity;

                            //validate quantity
                            if (qtyToAdd <= 0)
                                continue;

                            if (qtyToAdd > maxQtyToAdd)
                                qtyToAdd = maxQtyToAdd;

                            //ok. we have at least one item. let's create a shipment (if it does not exist)

                            var orderItemTotalWeight = orderItem.ItemWeight.HasValue ? orderItem.ItemWeight * qtyToAdd : null;
                            if (orderItemTotalWeight.HasValue)
                            {
                                if (!totalWeight.HasValue)
                                    totalWeight = 0;
                                totalWeight += orderItemTotalWeight.Value;
                            }
                            if (shipment == null)
                            {
                                var trackingNumber = model.OutboundStatusNotification.Status.Tracking;
                                var adminComment = "Creado por LogisFashion - ESTAFETA";
                                shipment = new Shipment
                                {
                                    OrderId = order.Id,
                                    TrackingNumber = trackingNumber,
                                    TotalWeight = null,
                                    ShippedDateUtc = null,
                                    DeliveryDateUtc = null,
                                    AdminComment = adminComment,
                                    CreatedOnUtc = DateTime.UtcNow,
                                };
                            }

                            //create a shipment item
                            var shipmentItem = new ShipmentItem
                            {
                                OrderItemId = orderItem.Id,
                                Quantity = qtyToAdd,
                                WarehouseId = orderItem.Product.WarehouseId
                            };
                            shipment.ShipmentItems.Add(shipmentItem);
                        }

                        //if we have at least one item in the shipment, then save it
                        if (shipment != null && shipment.ShipmentItems.Any())
                        {
                            shipment.TotalWeight = totalWeight;
                            _shipmentService.InsertShipment(shipment);

                            //add a note
                            order.OrderNotes.Add(new OrderNote
                            {
                                Note = "Se agregó un envío desde Logsfashion",
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                            _orderService.UpdateOrder(order);
                            _orderProcessingService.Ship(shipment, true);
                        }

                        _logisfashionRequestLogService.Insert(CreateIncomingLog(model, "200", notification.Id));
                    }
                    else
                    {
                        _logisfashionRequestLogService.Insert(CreateIncomingLog(model, "404", notification.Id, "No se encontró la órden " + notification.RelatedElementId));
                        return BadRequest(new WebHookResponse()
                        {
                            Error = true,
                            ErrorMessage = "No se encontró la órden " + notification.RelatedElementId,
                            StatusCode = 404
                        });
                    }
                }
            }
            catch (Exception e)
            {
                return BadRequest(new WebHookResponse()
                {
                    Error = true,
                    ErrorMessage = e.Message,
                    StatusCode = 400
                });
            }

            return Ok(new WebHookResponse()
            {
                Error = false,
                ErrorMessage = null,
                StatusCode = 200
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateProductDimensions()
        {
            var eanWithErrors = new List<string>();
            LogisfashionSettings settings = _settingService.LoadSetting<LogisfashionSettings>();
            var massiveData = new List<MassiveUpdateDimensionModel>();

            var model = new UpdateDimensionRequest();
            model.masterData = new UpdateDimensionRequestData();
            model.masterData.item = new List<UpdateDimensionsRequesItem>();

            #region MassiveData

            massiveData.Add(new MassiveUpdateDimensionModel() { sku = "045557817404", altura = 63.5m, anchura = 39.37m, peso = 4m, profundidad = 20.32m });
            massiveData.Add(new MassiveUpdateDimensionModel() { sku = "045557817435", altura = 20.32m, anchura = 8.89m, peso = 2.21m, profundidad = 27.94m });
            massiveData.Add(new MassiveUpdateDimensionModel() { sku = "045557819385", altura = 20.32m, anchura = 8.89m, peso = 2.21m, profundidad = 27.94m });
            massiveData.Add(new MassiveUpdateDimensionModel() { sku = "045557854065", altura = 33.02m, anchura = 11.43m, peso = 2.45m, profundidad = 45.72m });
            massiveData.Add(new MassiveUpdateDimensionModel() { sku = "5029736053321", altura = 23m, anchura = 37.5m, peso = 3.5m, profundidad = 21m });
            massiveData.Add(new MassiveUpdateDimensionModel() { sku = "045557834326", altura = 15.24m, anchura = 8.255m, peso = 2.054808m, profundidad = 30.48m });
            massiveData.Add(new MassiveUpdateDimensionModel() { sku = "045557839109", altura = 30.48m, anchura = 6.35m, peso = 3.166128m, profundidad = 45.72m });
            massiveData.Add(new MassiveUpdateDimensionModel() { sku = "045557818265", altura = 25.4m, anchura = 10.16m, peso = 0.825552m, profundidad = 33.02m });
            massiveData.Add(new MassiveUpdateDimensionModel() { sku = "045557854034", altura = 33.02m, anchura = 17.78m, peso = 1.124928m, profundidad = 71.12m });
            massiveData.Add(new MassiveUpdateDimensionModel() { sku = "045557817381", altura = 43.18m, anchura = 16.51m, peso = 4.681152m, profundidad = 41.91m });
            massiveData.Add(new MassiveUpdateDimensionModel() { sku = "045557814205", altura = 16.5m, anchura = 5m, peso = 1.6m, profundidad = 25m });
            massiveData.Add(new MassiveUpdateDimensionModel() { sku = "045557858148", altura = 34m, anchura = 18m, peso = 2.17m, profundidad = 18m });
            massiveData.Add(new MassiveUpdateDimensionModel() { sku = "045557856182", altura = 34m, anchura = 47.5m, peso = 3.7m, profundidad = 13.5m });
            massiveData.Add(new MassiveUpdateDimensionModel() { sku = "5029736060282", altura = 34m, anchura = 23m, peso = 5.2m, profundidad = 10m });
            massiveData.Add(new MassiveUpdateDimensionModel() { sku = "045557840686", altura = 34m, anchura = 23m, peso = 5.53m, profundidad = 10.2m });
            massiveData.Add(new MassiveUpdateDimensionModel() { sku = "045557817398", altura = 19m, anchura = 10.2m, peso = 2.1m, profundidad = 17.8m });
            massiveData.Add(new MassiveUpdateDimensionModel() { sku = "045557819170", altura = 61.595m, anchura = 15.24m, peso = 2.9484m, profundidad = 40.005m });
            massiveData.Add(new MassiveUpdateDimensionModel() { sku = "045557817947", altura = 25.4m, anchura = 25.4m, peso = 1.86m, profundidad = 6.35m });
            massiveData.Add(new MassiveUpdateDimensionModel() { sku = "045557817961", altura = 25.4m, anchura = 30.48m, peso = 2.99m, profundidad = 8.89m });
            massiveData.Add(new MassiveUpdateDimensionModel() { sku = "045557817978", altura = 33.02m, anchura = 38.1m, peso = 3.63m, profundidad = 13.46m });

            #endregion

            model.masterData.item = massiveData.Select(x => new UpdateDimensionsRequesItem()
            {
                sku = x.sku,
                altura = x.altura,
                anchura = x.anchura,
                peso = x.peso,
                profundidad = x.profundidad
            }).ToList();

            using (HttpClient client = new HttpClient())
            {
                PrepareRequest(client, settings);
                var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var result = await client.PostAsync((settings.Sandbox ? API_URL_TEST : API_URL) + UPDATE_DIMENSIONS_URL, content);

                LogisfashionRequestLog logisfashionRequestLog = new LogisfashionRequestLog()
                {
                    CustomerId = TempData["CustomerId"] == null ? 0 : (int)TempData["CustomerId"],
                    Error = !result.IsSuccessStatusCode,
                    StatusCode = result.StatusCode.ToString(),
                    ErrorMessage = !result.IsSuccessStatusCode ? await result.Content.ReadAsStringAsync() : null,
                    RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(model),
                    LastResponseJson = await result.Content.ReadAsStringAsync(),
                    RequestType = RequestType.UpdateDimensions,
                    StatusType = StatusType.Finished,
                    LogInternalId = null
                };
                _logisfashionRequestLogService.Insert(logisfashionRequestLog);

                if (!result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    //return BadRequest(json);
                    Debugger.Break();
                }
            }

            return Ok();
        }

        private async Task<string> CreateInboundRequest(List<InboundExcelModel> itemModel, bool isAssortment, string orderNumber)
        {
            var eanWithErrors = new List<string>();
            LogisfashionSettings settings = _settingService.LoadSetting<LogisfashionSettings>();

            InboundModel model = new InboundModel();
            model.NotificationId = Guid.NewGuid();
            model.InboundList = new List<Inbound>() {
                    new Inbound()
                    {
                        Date = DateTime.Now,
                        PONumber = (isAssortment ? "AS-" : "") + orderNumber,
                        ItemList = itemModel.Select(x => new InboundItemModel()
                        {
                            ExpectedQuantity = x.Quantity.ToString(),
                            SKU = x.Gtin
                        }).ToList(),
                        Supplier = new SupplierModel()
                        {
                            Address = "Hamleys",
                            City = "",
                            Contact = "",
                            Comments = string.Empty,
                            Country = "México",
                            Email = "",
                            Name = "",
                            PostalCode = "",
                            State = "",
                            Tel1 = "5563162661"
                        }
                    }
                };

            using (HttpClient client = new HttpClient())
            {
                PrepareRequest(client, settings);
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var result = await client.PostAsync((settings.Sandbox ? API_URL_TEST : API_URL) + INBOUNDS_URL, content);

                foreach (var inbound in model.InboundList)
                {
                    LogisfashionRequestLog logisfashionRequestLog = new LogisfashionRequestLog()
                    {
                        CustomerId = TempData["CustomerId"] == null ? 0 : (int)TempData["CustomerId"],
                        Error = !result.IsSuccessStatusCode,
                        StatusCode = result.StatusCode.ToString(),
                        ErrorMessage = !result.IsSuccessStatusCode ? await result.Content.ReadAsStringAsync() : null,
                        RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        LastResponseJson = await result.Content.ReadAsStringAsync(),
                        RequestType = RequestType.Inbound,
                        StatusType = StatusType.Pending,
                        LogInternalId = inbound.PONumber
                    };
                    _logisfashionRequestLogService.Insert(logisfashionRequestLog);
                }

                if (!result.IsSuccessStatusCode)
                {
                    return await result.Content.ReadAsStringAsync();
                }
            }

            return null;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateProductsInLGF()
        {
            List<string> model = new List<string>();
            List<string> notFoundProductsSkuInHamleys = new List<string>();
            List<string> notFoundProductsSkuInLGF = new List<string>();
            List<string> productsWithDifferentQty = new List<string>();
            int count = 0;

            #region Data

            model.Add("77SP17101901");
            model.Add("77SP17101902");
            model.Add("77SP17101903");
            model.Add("77SP17101904");
            model.Add("77SP17101905");
            model.Add("77SP17101906");
            model.Add("77SP17101907");
            model.Add("77SP17101908");
            model.Add("77SP17101909");
            model.Add("77SP17101910");
            model.Add("77SP17101911");
            model.Add("77SP17101912");
            model.Add("77SP17101913");
            model.Add("77SP17101914");
            model.Add("77SP17101915");
            model.Add("77SP17101916");
            model.Add("77SP17101917");
            model.Add("77SP17101918");
            model.Add("77SP17101919");
            model.Add("77SP17101920");
            model.Add("77SP17101921");
            model.Add("77SP17101922");
            model.Add("77SP17101923");
            model.Add("77SP17101924");
            model.Add("77SP17101925");
            model.Add("77SP17101926");
            model.Add("77SP17101927");
            model.Add("77SP17101928");
            model.Add("77SP17101929");
            model.Add("77SP17101930");
            model.Add("77SP17101931");
            model.Add("77SP17101932");
            model.Add("77SP17101933");
            model.Add("77SP17101934");
            model.Add("77SP17101935");
            model.Add("77SP17101936");
            model.Add("77SP17101937");
            model.Add("77SP17101938");
            model.Add("77SP17101939");
            model.Add("77SP17101940");
            model.Add("77SP17101941");
            model.Add("77SP17101942");
            model.Add("77SP17101943");
            model.Add("77SP17101944");
            model.Add("77SP17101945");
            model.Add("77SP17101946");
            model.Add("77SP17101947");
            model.Add("77SP17101948");
            model.Add("77SP17101949");
            model.Add("77SP17101950");
            model.Add("77SP17101951");
            model.Add("77SP17101952");
            model.Add("77SP17101953");
            model.Add("77SP17101954");
            model.Add("77SP17101955");
            model.Add("77SP17101956");
            model.Add("77SP17101957");
            model.Add("77SP17101958");
            model.Add("77SP17101959");
            model.Add("77SP17101960");
            model.Add("77SP17101961");
            model.Add("77SP17101962");
            model.Add("77SP17101963");
            model.Add("77SP17101964");
            model.Add("77SP17101965");
            model.Add("77SP17101966");
            model.Add("77SP17101967");
            model.Add("77SP17101968");
            model.Add("77SP17101969");
            model.Add("77SP17101970");
            model.Add("77SP17101971");
            model.Add("77SP17101972");
            model.Add("77SP17101973");
            model.Add("77SP17101974");
            model.Add("77SP17101975");
            model.Add("77SP17101976");
            model.Add("77SP17101977");
            model.Add("77SP17101978");
            model.Add("77SP17101979");
            model.Add("77SP17101980");
            model.Add("77SP17101981");
            model.Add("77SP17101982");
            model.Add("77SP17101983");
            model.Add("77SP17101984");
            model.Add("77SP17101985");
            model.Add("77SP17101986");
            model.Add("77SP17101987");
            model.Add("77SP17101988");
            model.Add("77SP17101989");
            model.Add("77SP17101990");
            model.Add("77SP17101991");
            model.Add("77SP17101992");
            model.Add("77SP17101993");
            model.Add("77SP17101994");
            model.Add("77SP17101995");
            model.Add("77SP17101996");
            model.Add("77SP17101997");
            model.Add("77SP17101998");
            model.Add("77SP17101999");
            model.Add("77SP17102000");
            model.Add("77SP17102001");
            model.Add("77SP17102002");
            model.Add("77SP17102003");
            model.Add("77SP17102004");
            model.Add("77SP17102005");
            model.Add("77SP17102006");
            model.Add("77SP17102007");
            model.Add("77SP17102008");
            model.Add("77SP17102009");
            model.Add("77SP17102010");


            #endregion

            try
            {
                foreach (var item in model)
                {
                    count++;
                    string parsedGtin = item.TrimStart('0');
                    Product product = _productService.GetAllProductsQuery()
                        .Where(x => x.Gtin == item || x.Gtin == parsedGtin)
                        .FirstOrDefault();

                    if (product == null)
                    {
                        notFoundProductsSkuInHamleys.Add(item);
                    }
                    else
                    {
                        LogisfashionSettings settings = _settingService.LoadSetting<LogisfashionSettings>();

                        using (HttpClient client = new HttpClient())
                        {
                            PrepareRequest(client, settings);
                            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                            var result = await client.GetAsync((settings.Sandbox ? API_URL_TEST : API_URL) + STOCK_URL + parsedGtin);

                            if (!result.IsSuccessStatusCode)
                            {
                                string json = await result.Content.ReadAsStringAsync();
                                notFoundProductsSkuInLGF.Add(parsedGtin);
                            }
                            else
                            {
                                string json = await result.Content.ReadAsStringAsync();
                                ProductStockResponseModel jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject<ProductStockResponseModel>(json);
                                if (jsonObject.StockList != null && product.StockQuantity != jsonObject.StockList.FirstOrDefault()?.Quantity)
                                {
                                    productsWithDifferentQty.Add(parsedGtin);
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debugger.Break();
            }

            Debugger.Break();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> ManualOutboundRequest(int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            Order order = _orderService.GetOrderById(orderId);
            if (order == null) return NotFound();

            string id = "0";
            string address = "Sin información";
            string city = "Sin información";
            string contact = "Sin información";
            string comments = $"CONTACTAR AL ADMINISTRADOR (ORDEN {order.Id})";
            string email = "Sin información";
            string name = "Sin información";
            string postalCode = "Sin información";
            string tel1 = "Sin información";
            string state = "Sin información";

            if (order.ShippingAddress != null && !string.IsNullOrWhiteSpace(order.ShippingAddress.Address1))
            {
                id = order.ShippingAddress.Id.ToString();
                address = order.ShippingAddress.Address1 + " " + order.ShippingAddress.Address2;
                city = order.ShippingAddress.City;
                contact = order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName;
                comments = string.Empty;
                email = order.ShippingAddress.Email;
                name = order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName;
                postalCode = order.ShippingAddress.ZipPostalCode;
                tel1 = order.ShippingAddress.PhoneNumber;
                state = order.ShippingAddress.StateProvince.Name;
            }
            else if (order.ShippingAddress != null && string.IsNullOrWhiteSpace(order.ShippingAddress.Address1))
            {
                name = order.ShippingAddress?.FirstName + " " + order.ShippingAddress?.LastName;
                email = order.ShippingAddress?.Email;
                tel1 = order.ShippingAddress?.PhoneNumber;
            }
            else if (order.PickupAddress != null)
            {
                id = order.PickupAddress.Id.ToString();
                address = order.PickupAddress.Address1 + " " + order.PickupAddress.Address2;
                city = order.PickupAddress.City;
                contact = order.PickupAddress.FirstName + " " + order.PickupAddress.LastName;
                comments = "Entregar en tienda";
                email = order.BillingAddress.Email;
                name = order.BillingAddress.FirstName + " " + order.BillingAddress.LastName;
                postalCode = order.PickupAddress.ZipPostalCode;
                tel1 = order.BillingAddress.PhoneNumber;
                state = order.PickupAddress.StateProvince.Name;
            }

            using (var client = new HttpClient())
            {
                var outbound = new Outbound()
                {
                    Date = DateTime.Now,
                    DeliveryInfo = new DeliveryModel()
                    {
                        Id = id,
                        Address = address,
                        City = city,
                        Contact = contact,
                        Comments = comments,
                        Country = "México",
                        Email = email,
                        Name = name,
                        PostalCode = postalCode,
                        Tel1 = tel1,
                        State = state
                    },
                    ShippingInfo = new DeliveryModel()
                    {
                        Id = id,
                        Address = address,
                        City = city,
                        Contact = contact,
                        Comments = comments,
                        Country = "México",
                        Email = email,
                        Name = name,
                        PostalCode = postalCode,
                        Tel1 = tel1,
                        State = state
                    },
                    DeliveryNumber = order.Id.ToString(),
                    Carrier = "ESTAFETA",
                    ShipMethod = order.ShippingMethod.ToUpper() == "EXPRESS" ? "EXPRESS" : "STANDARD",
                    ItemList = order.OrderItems.Select(x => new OutboundItemModel()
                    {
                        SKU = x.Product.Gtin,
                        RequestedQuantity = x.Quantity
                    }).ToList()
                };

                return await CreateOutboundRequest(outbound, true);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateOutboundRequest([FromBody] Outbound outbound, bool isManual = false)
        {
            if (processedOrders.Where(x => x == outbound.DeliveryNumber).Any())
            {
                return BadRequest("Ya se había procesado la orden " + outbound.DeliveryNumber);
            }

            processedOrders.Add(outbound.DeliveryNumber);

            Order order = _orderService.GetOrderById(int.Parse(outbound.DeliveryNumber));
            if (order == null) return NotFound();

            if (outbound == null) return BadRequest();
            OutboundModel model = new OutboundModel()
            {
                NotificationId = outbound.DeliveryNumber,
                OutboundList = new List<Outbound>()
                {
                    outbound
                }
            };

            LogisfashionSettings settings = _settingService.LoadSetting<LogisfashionSettings>();

            using (HttpClient client = new HttpClient())
            {
                PrepareRequest(client, settings);
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                int deliveryNumberParsed = int.Parse(outbound.DeliveryNumber);

                var logisfashionRequestLog = _logisfashionRequestLogService.GetAll()
                    .Where(x => x.RelatedElementName == "Order" && x.RelatedElementId == deliveryNumberParsed && !x.Error)
                    .FirstOrDefault();

                if (logisfashionRequestLog != null) return BadRequest("Ya existe una salida correcta reportada para la orden " + order.Id);
                logisfashionRequestLog = new LogisfashionRequestLog()
                {
                    CustomerId = TempData["CustomerId"] == null ? 0 : (int)TempData["CustomerId"],
                    RequestJson = JsonConvert.SerializeObject(model),
                    RequestType = RequestType.Outbound,
                    StatusType = StatusType.Pending,
                    LogInternalId = outbound.DeliveryNumber,
                    RelatedElementName = "Order",
                    RelatedElementId = int.Parse(outbound.DeliveryNumber)
                };
                _logisfashionRequestLogService.Insert(logisfashionRequestLog);

                var result = await client.PostAsync((settings.Sandbox ? API_URL_TEST : API_URL) + OUTBOUNDS_URL, content);

                logisfashionRequestLog = new LogisfashionRequestLog()
                {
                    CustomerId = TempData["CustomerId"] == null ? 0 : (int)TempData["CustomerId"],
                    Error = !result.IsSuccessStatusCode,
                    StatusCode = result.StatusCode.ToString(),
                    ErrorMessage = !result.IsSuccessStatusCode ? await result.Content.ReadAsStringAsync() : null,
                    RequestJson = JsonConvert.SerializeObject(model),
                    LastResponseJson = await result.Content.ReadAsStringAsync(),
                    RequestType = RequestType.Outbound,
                    StatusType = StatusType.Pending,
                    LogInternalId = outbound.DeliveryNumber,
                    RelatedElementName = "Order",
                    RelatedElementId = int.Parse(outbound.DeliveryNumber)
                };
                _logisfashionRequestLogService.Update(logisfashionRequestLog);

                if (!result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return BadRequest(json);
                }
                else if (isManual)
                {
                    order.OrderNotes.Add(new OrderNote()
                    {
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        Note = $"El usuario {_workContext.CurrentCustomer.Email} envió la notificación de salida a Logisfashion."
                    });
                    _orderService.UpdateOrder(order);
                }
                else
                {
                    order.OrderNotes.Add(new OrderNote()
                    {
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        Note = "Se envió la notificación automática de salida a Logisfashion."
                    });
                    _orderService.UpdateOrder(order);
                }
            }

            return Ok();
        }

        [HttpGet]
        public DateTime? OrderSentToProvider(int orderId)
        {
            return _logisfashionRequestLogService.GetAll()
                .Where(x => !x.Error && x.LogInternalId == orderId.ToString() && x.RequestType == RequestType.Outbound)
                .Select(x => x.CreatedOnUtc)?
                .FirstOrDefault();
        }

        [HttpGet]
        public async Task<IActionResult> MasterData(string sku)
        {
            return await ExecuteMasterData(sku);
        }

        [HttpPost]
        public IActionResult ListInboundRequest(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var query = _logisfashionRequestLogService.GetAll().Where(x => !x.Error && x.RequestType == RequestType.Inbound).OrderByDescending(x => x.CreatedOnUtc);
            var pagedList = new PagedList<LogisfashionRequestLog>(query, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.ToList().Select(x => new
                {
                    Date = x.CreatedOnUtc.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt"),
                    Request = string.Join(", ", JsonConvert.DeserializeObject<InboundModel>(x.RequestJson).InboundList.FirstOrDefault()?.ItemList.Select(y => $"{y.SKU} ({y.ExpectedQuantity})")),
                    Status = string.IsNullOrWhiteSpace(x.StatusDescription) ? "Pendiente de respuesta" : x.StatusDescription,
                    PONumber = x.LogInternalId
                }),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ListSentProducts(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var query = _logisfashionRequestLogService.GetAll().Where(x => !x.Error && x.RequestType == RequestType.MasterData).OrderByDescending(x => x.CreatedOnUtc);
            var pagedList = new PagedList<LogisfashionRequestLog>(query, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.ToList().Select(x => new
                {
                    Date = x.CreatedOnUtc.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt"),
                    ProductName = _productService.GetProductById(x.RelatedElementId)?.Name,
                    _productService.GetProductById(x.RelatedElementId)?.Sku
                }),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ListLogErrors(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var query = _logisfashionRequestLogService.GetAll().Where(x => x.Error).OrderByDescending(x => x.CreatedOnUtc);
            var pagedList = new PagedList<LogisfashionRequestLog>(query, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.ToList().Select(x => new
                {
                    Date = x.CreatedOnUtc.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt"),
                    RequestType = EnumHelper.GetDisplayName(x.RequestType),
                    x.StatusCode,
                    StatusDescription = x.ErrorMessage
                }),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ListLogSuccess(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var query = _logisfashionRequestLogService.GetAll().Where(x => !x.Error).OrderByDescending(x => x.CreatedOnUtc);
            var pagedList = new PagedList<LogisfashionRequestLog>(query, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.ToList().Select(x => new
                {
                    Date = x.CreatedOnUtc.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt"),
                    RequestType = EnumHelper.GetDisplayName(x.RequestType),
                    x.StatusCode,
                    StatusDescription = string.IsNullOrWhiteSpace(x.StatusDescription) ? $"{x.RelatedElementName} - {x.RelatedElementId}" : x.StatusDescription
                }),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateLogisfashionProducts()
        {
            var skus = _productService.GetAllProductsQuery().Where(x => !x.Deleted).Select(x => x.Sku).Distinct().ToList();
            int updatedCount = 0;
            List<string> skuWithErrors = new List<string>();

            foreach (var sku in skus)
            {
                await ExecuteMasterData(sku, skuWithErrors, updatedCount);
            }

            return Ok(new { skuWithErrors, updatedCount });
        }

        [HttpPost]
        public async Task<IActionResult> SendProductToMasterData(string sku)
        {
            await ExecuteMasterData(sku);
            return Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Test()
        {
            var test = new List<string>();
            #region data

            test.Add("8410779027603");
            test.Add("5056219003580");
            test.Add("5015353853246");
            test.Add("5056219003580");
            test.Add("5015353239880");
            test.Add("5015353239880");
            test.Add("5015353239880");
            test.Add("5015353260563");
            test.Add("5015353260563");
            test.Add("5015353239880");
            test.Add("45557815875");
            test.Add("45557358709");
            test.Add("45557397203");
            test.Add("45557397203");
            test.Add("4893808200071");
            test.Add("45557815899");
            test.Add("45557815868");
            test.Add("45557815899");
            test.Add("8410779446541");
            test.Add("8410779462701");
            test.Add("45557397203");
            test.Add("45557815868");
            test.Add("778988548264");
            test.Add("45557815165");
            test.Add("8410779446541");
            test.Add("673419279840");
            test.Add("5015353262734");
            test.Add("5015353841724");
            test.Add("4893808052014");
            test.Add("4893808200071");
            test.Add("45557397203");
            test.Add("8410779047946");
            test.Add("4893808200071");
            test.Add("45557813796");
            test.Add("4893808200071");
            test.Add("5015353841724");
            test.Add("5015353841724");
            test.Add("5015353475622");
            test.Add("5015353841892");
            test.Add("3296580849635");
            test.Add("3296580849635");
            test.Add("B00PTVYUWY");
            test.Add("8410779027603");
            test.Add("5015353260563");
            test.Add("45557358709");
            test.Add("5015353966113");
            test.Add("5015353261911");
            test.Add("5015353261911");
            test.Add("630509399123");
            test.Add("5015353262819");
            test.Add("5015353262819");
            test.Add("5015353966885");
            test.Add("5015353262819");
            test.Add("5015353262819");
            test.Add("5015353262819");
            test.Add("5015353966885");
            test.Add("5015353966885");
            test.Add("5015353262819");
            test.Add("9321268022988");
            test.Add("4893808052014");
            test.Add("45557807825");
            test.Add("45557807825");
            test.Add("45557807825");
            test.Add("45557807825");
            test.Add("45557807825");
            test.Add("5015353841892");
            test.Add("5015353256146");
            test.Add("4893808052014");
            test.Add("630509495382");
            test.Add("5015353262529");
            test.Add("5015353262529");
            test.Add("5015353262529");
            test.Add("5015353966564");
            test.Add("45557807825");
            test.Add("45557807825");
            test.Add("B07JH28MV5");
            test.Add("45557807825");
            test.Add("7506176961297");
            test.Add("5015353841724");
            test.Add("5015353260563");
            test.Add("8410779041876");
            test.Add("887961698879");
            test.Add("5015353841724");
            test.Add("887961698879");
            test.Add("5015353841724");
            test.Add("673419262309");
            test.Add("700013507");
            test.Add("887961676563");
            test.Add("887961676563");
            test.Add("778988179284");
            test.Add("778988179284");
            test.Add("700014721");
            test.Add("700014721");
            test.Add("700014721");
            test.Add("700014721");
            test.Add("700014721");
            test.Add("5015353966885");
            test.Add("5056219003559");
            test.Add("5056219003580");
            test.Add("5015353114538");
            test.Add("5015353114538");
            test.Add("5056219005423");
            test.Add("5056219003559");
            test.Add("5015353966885");
            test.Add("5015353114583");
            test.Add("5056219003580");
            test.Add("5056219003580");
            test.Add("5056219003559");
            test.Add("5015353114538");
            test.Add("45557412302");
            test.Add("5056219003559");
            test.Add("5702015595407");
            test.Add("5015353114538");
            test.Add("5056219003504");
            test.Add("45557359201");
            test.Add("5056219003504");
            test.Add("45557415303");
            test.Add("5056219003504");
            test.Add("5056219003504");
            test.Add("5702015595407");
            test.Add("5015353966885");
            test.Add("5056219003580");
            test.Add("5056219003580");
            test.Add("45557415303");
            test.Add("5015353564197");
            test.Add("45557415303");
            test.Add("5056219003559");
            test.Add("5056219003559");
            test.Add("5056219003504");
            test.Add("5056219003504");
            test.Add("730002010973");
            test.Add("5015353114538");
            test.Add("5015353335582");
            test.Add("5015353476476");
            test.Add("5015353966687");
            test.Add("5015353476476");
            test.Add("8421424092");
            test.Add("8421372034");
            test.Add("8421350315");
            test.Add("8421350278");
            test.Add("8421350353");
            test.Add("8421350117");
            test.Add("9321268022988");
            test.Add("8421366163");
            test.Add("8421350278");
            test.Add("7500247575666");
            test.Add("5015353476476");
            test.Add("5015353966687");
            test.Add("8421350193");
            test.Add("8421424108");
            test.Add("8421424009");
            test.Add("7500247988350");
            test.Add("8421952250");
            test.Add("5015353476476");
            test.Add("8421424115");
            test.Add("8421424023");
            test.Add("7500247972885");
            test.Add("5015353966687");
            test.Add("8421424023");
            test.Add("8421350391");
            test.Add("8421424023");
            test.Add("8421424085");
            test.Add("8421350117");
            test.Add("8421421343");
            test.Add("8421350285");
            test.Add("8421424061");
            test.Add("8421424023");
            test.Add("8421424115");
            test.Add("8421350261");
            test.Add("8421350322");
            test.Add("8421350193");
            test.Add("8421350261");
            test.Add("8421350278");
            test.Add("8421350353");
            test.Add("8421952250");
            test.Add("8421424023");
            test.Add("8421366163");
            test.Add("45557412302");
            test.Add("8421424092");
            test.Add("4055744012686");
            test.Add("8421424009");
            test.Add("5015353854519");
            test.Add("5015353854519");
            test.Add("5015353854519");
            test.Add("5015353854519");
            test.Add("5015353854519");
            test.Add("5015353854519");
            test.Add("5015353966120");
            test.Add("5015353260563");
            test.Add("5015353262741");
            test.Add("5015353262031");
            test.Add("5015353260563");
            test.Add("5015353262741");
            test.Add("5015353262741");
            test.Add("5015353854519");
            test.Add("5015353854519");
            test.Add("5015353852355");
            test.Add("5015353852355");
            test.Add("3222440857519");
            test.Add("5015353966892");
            test.Add("5015353966892");
            test.Add("5015353100982");
            test.Add("5015353966892");
            test.Add("5015353101262");
            test.Add("5015353101262");
            test.Add("5015353966892");
            test.Add("5015353966892");
            test.Add("5015353966892");
            test.Add("5015353100982");
            test.Add("5015353972114");
            test.Add("5015353972114");
            test.Add("5015353100982");
            test.Add("5015353966892");
            test.Add("5015353100982");
            test.Add("5015353841656");
            test.Add("5015353101316");
            test.Add("5015353101316");
            test.Add("5015353966892");
            test.Add("5015353101262");
            test.Add("5015353966878");
            test.Add("5015353854113");
            test.Add("5015353966878");
            test.Add("5015353972114");
            test.Add("5015353966878");
            test.Add("5015353966878");
            test.Add("5015353101316");
            test.Add("5015353476490");
            test.Add("5015353966878");
            test.Add("5015353854113");
            test.Add("5015353966878");
            test.Add("5015353966878");
            test.Add("5015353101316");
            test.Add("5015353972114");
            test.Add("5015353854113");
            test.Add("5015353854113");
            test.Add("5015353101316");
            test.Add("5015353101262");
            test.Add("5015353101316");
            test.Add("5015353854113");
            test.Add("5015353972114");
            test.Add("5015353966878");
            test.Add("5015353854113");
            test.Add("5056219003559");
            test.Add("5056219003559");
            test.Add("5056219003559");
            test.Add("5056219000817");
            test.Add("5056219003559");
            test.Add("5056219003559");
            test.Add("5056219005423");
            test.Add("5056219005423");
            test.Add("5056219000817");
            test.Add("5056219000817");
            test.Add("5056219000817");
            test.Add("5056219000817");
            test.Add("5056219003580");
            test.Add("5056219003580");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("5055114396612");
            test.Add("630509616824");
            test.Add("8410779066855");
            test.Add("7506176969262");
            test.Add("8421421343");
            test.Add("5015353114538");
            test.Add("5015353476506");
            test.Add("5015353476506");
            test.Add("8410779066701");
            test.Add("9781890647285");
            test.Add("5015353476506");
            test.Add("730002010973");
            test.Add("5015353246710");
            test.Add("5015353246710");
            test.Add("5015353246710");
            test.Add("5015353246710");
            test.Add("5015353246710");
            test.Add("5055261540227");
            test.Add("5015353246710");
            test.Add("5015353246710");
            test.Add("5015353246710");
            test.Add("5015353246710");
            test.Add("5015353246710");
            test.Add("5015353246710");
            test.Add("5015353246710");
            test.Add("8410779066800");
            test.Add("9321268006384");
            test.Add("8421372034");
            test.Add("6305099616824");
            test.Add("45557412302");
            test.Add("8421350391");
            test.Add("5015353143651");
            test.Add("5015353143651");
            test.Add("5015353143651");
            test.Add("45557813772");
            test.Add("730002010829");
            test.Add("887961679236");
            test.Add("4055744010002");
            test.Add("673419303699");
            test.Add("5015353143651");
            test.Add("5015353143651");
            test.Add("45557415303");
            test.Add("45557359201");
            test.Add("7500247850671");
            test.Add("45557412302");
            test.Add("887961679236");
            test.Add("45557415303");
            test.Add("630509377718");
            test.Add("5015353100982");
            test.Add("5015353966892");
            test.Add("5015353100982");
            test.Add("507436423025");
            test.Add("7502243750943");
            test.Add("5015353317090");
            test.Add("5015353317090");
            test.Add("4893808052069");
            test.Add("5056219003559");
            test.Add("5056219003580");
            test.Add("5056219005423");
            test.Add("5056219005423");
            test.Add("5056219005423");
            test.Add("5056219005423");
            test.Add("5056219005423");
            test.Add("5056219005423");
            test.Add("5056219005423");
            test.Add("5056219005423");
            test.Add("5056219003580");
            test.Add("5056219003580");
            test.Add("5056219005423");
            test.Add("4893808052069");
            test.Add("887961673180");
            test.Add("5015353262734");
            test.Add("5015353262734");
            test.Add("5015353262734");
            test.Add("XO673419281836");
            test.Add("XO673419281836");
            test.Add("X001XL4E2B");
            test.Add("5015353262031");
            test.Add("5015353262031");
            test.Add("5015353262031");
            test.Add("5015353966908");
            test.Add("5015353966908");
            test.Add("5015353966908");
            test.Add("5015353966908");
            test.Add("5015353966908");
            test.Add("5015353966908");
            test.Add("5015353966908");
            test.Add("5015353966908");
            test.Add("4893808052069");
            test.Add("7500247661680");
            test.Add("7500247518540");
            test.Add("7500247661680");
            test.Add("8410779069900");
            test.Add("8410779069900");
            test.Add("5055261540432");
            test.Add("45557398804");
            test.Add("5015353841632");
            test.Add("5015353173979");
            test.Add("5015353173979");
            test.Add("5015353101101");
            test.Add("5015353841625");
            test.Add("5015353966649");
            test.Add("5015353173979");
            test.Add("5015353100944");
            test.Add("5015353173979");
            test.Add("X001XK3N65");
            test.Add("5015353262529");
            test.Add("5015353262529");
            test.Add("45557807825");
            test.Add("5015353966564");
            test.Add("5015353966564");
            test.Add("5015353966564");
            test.Add("5015353966564");
            test.Add("5015353262659");
            test.Add("5015353262734");
            test.Add("5015353261911");
            test.Add("8421424108");
            test.Add("700014590");
            test.Add("8421424061");
            test.Add("5015353143651");
            test.Add("5015353335155");
            test.Add("9321268022988");
            test.Add("8421424085");
            test.Add("5015353143651");
            test.Add("630509786466");
            test.Add("XO673419266482");
            test.Add("8410779066817");
            test.Add("XO673419266482");
            test.Add("LPNRR417788119");
            test.Add("XO673419266482");
            test.Add("5055371510585");
            test.Add("X001XK3N65");
            test.Add("4893808052069");
            test.Add("4893808052069");
            test.Add("887961673180");

            #endregion

            var notExist = new List<string>();

            foreach (var item in test)
            {
                bool exist = _productService.GetAllProductsQuery().Where(x => x.Gtin == item).Any();
                if (!exist)
                {
                    notExist.Add(item);
                }
            }

            Debugger.Break();
            return Ok();
        }

        private async Task<IActionResult> ExecuteMasterData(string sku, List<string> ErrorMessages = null, int updatedCount = 0)
        {
            Product product = _productService.GetProductBySku(sku);
            if (product == null) { Debugger.Break(); return NotFound(); }

            LogisfashionSettings settings = _settingService.LoadSetting<LogisfashionSettings>();
            LogisfashionRequestLog logisfashionRequestLog = new LogisfashionRequestLog();

            MasterDataRequest model = new MasterDataRequest()
            {
                MasterData = new MasterData()
                {
                    Date = DateTime.Now,
                    NotificationId = Guid.NewGuid(),
                    ItemList = new List<ItemModel>()
                    {
                        new ItemModel()
                        {
                            SKU = product.Gtin,
                            ReferenceName = product.Name,
                            ReferenceID = product.Sku,
                            FamilyID = product.ProductCategories.Select(x => x.CategoryId).FirstOrDefault().ToString(),
                            FamilyName = product.ProductCategories.Select(x => x.Category).FirstOrDefault()?.Name,
                            ColorID = string.IsNullOrWhiteSpace(product.ManufacturerPartNumber) ? "" : product.ManufacturerPartNumber.Replace(" ", string.Empty).ToUpper() + "-" + product.ParentSku,
                            ColorName = string.IsNullOrWhiteSpace(product.ManufacturerPartNumber) ? "" : product.ManufacturerPartNumber,
                            AuxDataList = new List<object>() { new { AuxData = string.IsNullOrWhiteSpace(product.ParentSku) ? "" : product.ParentSku  } }
                        }
                    }
                }
            };

            using (HttpClient client = new HttpClient())
            {
                PrepareRequest(client, settings);

                var verifyItem = await client.GetAsync((settings.Sandbox ? API_URL_TEST : API_URL) + "/api/articles/sku/" + product.Gtin);
                logisfashionRequestLog = new LogisfashionRequestLog()
                {
                    CustomerId = TempData["CustomerId"] == null ? 0 : (int)TempData["CustomerId"],
                    Error = !verifyItem.IsSuccessStatusCode,
                    StatusCode = verifyItem.StatusCode.ToString(),
                    ErrorMessage = !verifyItem.IsSuccessStatusCode ? await verifyItem.Content.ReadAsStringAsync() : null,
                    RequestJson = "SKU: " + sku,
                    LastResponseJson = await verifyItem.Content.ReadAsStringAsync(),
                    RequestType = RequestType.CheckSku,
                    StatusType = StatusType.Finished,
                    RelatedElementName = "Product",
                    RelatedElementId = product.Id
                };
                _logisfashionRequestLogService.Insert(logisfashionRequestLog);

                if (verifyItem.IsSuccessStatusCode)
                {
                    string jsonResponse = await verifyItem.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<GetBySkuResponseModel>(jsonResponse);
                    if (responseObject.Item == null)
                    {
                        var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                        var result = await client.PostAsync((settings.Sandbox ? API_URL_TEST : API_URL) + MASTER_DATA_URL, content);

                        logisfashionRequestLog = new LogisfashionRequestLog()
                        {
                            CustomerId = TempData["CustomerId"] == null ? 0 : (int)TempData["CustomerId"],
                            Error = !result.IsSuccessStatusCode,
                            StatusCode = result.StatusCode.ToString(),
                            ErrorMessage = !result.IsSuccessStatusCode ? await result.Content.ReadAsStringAsync() : null,
                            RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(model),
                            LastResponseJson = await result.Content.ReadAsStringAsync(),
                            RequestType = RequestType.MasterData,
                            StatusType = StatusType.Pending,
                            RelatedElementName = "Product",
                            RelatedElementId = product.Id
                        };
                        _logisfashionRequestLogService.Insert(logisfashionRequestLog);

                        if (!result.IsSuccessStatusCode)
                        {
                            if (ErrorMessages != null)
                                ErrorMessages.Add(sku);

                            string json = await result.Content.ReadAsStringAsync();
                            Debugger.Break();
                        }
                        else
                        {
                            updatedCount++;
                        }
                    }
                    else
                    {
                        if ((product.Gtin == product.Sku) && (responseObject.Item.SKU != product.Gtin))
                        {
                            Debugger.Break();
                        }
                        else if (!string.IsNullOrWhiteSpace(product.ParentSku) && !string.IsNullOrWhiteSpace(responseObject.Item.AuxDataList.FirstOrDefault()?.ToString()) && responseObject.Item.AuxDataList.FirstOrDefault()?.ToString() != product.ParentSku)
                        {
                            Debugger.Break();
                        }
                    }
                }
                else
                {
                    string json = await verifyItem.Content.ReadAsStringAsync();
                    Debugger.Break();
                }
            }

            return Ok();
        }

        private LogisfashionRequestLog CreateIncomingLog<T>(T model, string statusCode, int parentLogId = 0, string errorMessage = null)
        {
            return new LogisfashionRequestLog()
            {
                RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(model),
                StatusCode = statusCode,
                Error = !string.IsNullOrWhiteSpace(errorMessage),
                ErrorMessage = errorMessage,
                RequestType = RequestType.Incoming,
                ParentLogId = parentLogId,
                CustomerId = TempData["CustomerId"] == null ? 0 : (int)TempData["CustomerId"]
            };
        }

        private StatusType? GetStatusEnumValue(string status)
        {
            switch (status)
            {
                case "CONFIRMED":
                    return StatusType.Confirmed;
                case "FINISHED":
                    return StatusType.Finished;
                case "IMPORTED":
                    return StatusType.Imported;
                case "PENDING":
                    return StatusType.Pending;
                case "SHIPPED":
                    return StatusType.Shipped;
                default:
                    break;
            }
            return null;
        }

        private void PrepareRequest(HttpClient client, LogisfashionSettings settings)
        {
            client.DefaultRequestHeaders.Add("ClientCode", settings.ClientCode.ToString());
            client.DefaultRequestHeaders.Add("ApiKey", settings.ApiKey);
        }
    }

    public class ProductStockResponseModel
    {
        public List<ProductStockModel> StockList { get; set; }
    }

    public class ProductStockModel
    {
        public int Quantity { get; set; }
    }

    public class InboundStatusNotificationModel
    {
        public InboundStatusNotification InboundStatusNotificacion { get; set; }
    }

    public class InboundStatusNotification
    {
        public string NotificationId { get; set; }
        public string PONumber { get; set; }
        public List<ItemModelWebHook> ItemList { get; set; }
        public StatusModel Status { get; set; }
    }

    public class ItemModelWebHook
    {
        public string SKU { get; set; }
        public int ExpectedQuantity { get; set; }
        public int ReceivedQuantity { get; set; }
    }

    public class InboundExcelModel
    {
        public string Gtin { get; set; }
        public string Quantity { get; set; }
    }

    public class OutboundStatusNotificationModel
    {
        public OutboundStatusNotification OutboundStatusNotification { get; set; }
    }

    public class OutboundStatusNotification
    {
        public string NotificationId { get; set; }
        public string DeliveryNumber { get; set; }
        public string IdDeliveryNumber { get; set; }
        public List<Carton> CartonList { get; set; }
        public List<OutboundItem> ItemList { get; set; }
        public StatusModel Status { get; set; }
    }

    public class OutboundItem
    {
        public string SKU { get; set; }
        public int Quantity { get; set; }
    }

    public class Carton
    {
        public string CartonId { get; set; }
        public List<CartonAuxDataModel> CartonAuxDataList { get; set; }
    }

    public class CartonAuxDataModel
    {
        public string CartonAuxData { get; set; }
    }

    public class StatusModel
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Tracking { get; set; }
    }

    public class UpdateDimensionRequest
    {
        public UpdateDimensionRequestData masterData { get; set; }
    }

    public class UpdateDimensionRequestData
    {
        public List<UpdateDimensionsRequesItem> item { get; set; }
    }

    public class MassiveUpdateDimensionModel
    {
        public string sku { get; set; }
        public decimal peso { get; set; }
        public decimal altura { get; set; }
        public decimal anchura { get; set; }
        public decimal profundidad { get; set; }
    }

    public class UpdateDimensionsRequesItem
    {
        public string sku { get; set; }
        public decimal peso { get; set; }
        public decimal altura { get; set; }
        public decimal anchura { get; set; }
        public decimal profundidad { get; set; }
        public string descripcion { get; set; } = null;
        public string descripcionVariante { get; set; } = null;
        public string incluye { get; set; } = null;
        public string noIncluye { get; set; } = null;
        public string codigoFamilia { get; set; } = null;
        public string nombreFamilia { get; set; } = null;
        public string codigoModelo { get; set; } = null;
        public string nombreModelo { get; set; } = null;
        public string codigoTemporada { get; set; } = null;
        public string nombreTemporada { get; set; } = null;
        public string codigoClaveAux1 { get; set; } = null;
        public string codigoClaveAux2 { get; set; } = null;
        public string imagen { get; set; } = null;
        public string ean13 { get; set; } = null;
        public string partidaArancelaria { get; set; } = null;
        public string paisOrigen { get; set; } = null;
        public string precioCompra { get; set; } = null;
        public string precioCobservaciones { get; set; } = null;
        public string descatalogado { get; set; } = null;
        public string granVolumen { get; set; } = null;
        public string abc { get; set; } = null;
        public string volumen { get; set; } = null;
    }

    public class MasterDataRequest
    {
        public MasterData MasterData { get; set; }
    }

    public class MasterData
    {
        public Guid NotificationId { get; set; }
        public DateTime Date { get; set; }
        public List<ItemModel> ItemList { get; set; }
    }

    public class ItemModel
    {
        public string SKU { get; set; }
        public string ReferenceID { get; set; }
        public string ReferenceName { get; set; }
        public string FamilyName { get; set; }
        public string FamilyID { get; set; }
        public string ColorID { get; set; }
        public string ColorName { get; set; }
        public string SizeID { get; set; } = "";
        public string SizeName { get; set; } = "";
        public string SeasonID { get; set; } = "";
        public string SeasonName { get; set; } = "";
        public string AuxKey1ID { get; set; } = "";
        public string AuxKey1Name { get; set; } = "";
        public string AuxKey2ID { get; set; } = "";
        public string AuxKey2Name { get; set; } = "";
        public string AuxKey3ID { get; set; } = "";
        public string AuxKey3Name { get; set; } = "";
        public List<object> BarcodeList { get; set; } = new List<object>() { new { Code = "" } };
        public List<object> AuxDataList { get; set; }
    }

    public class InboundModel
    {
        public Guid NotificationId { get; set; }
        public List<Inbound> InboundList { get; set; }
    }

    public class Inbound
    {
        public string PONumber { get; set; }
        public DateTime Date { get; set; }
        public SupplierModel Supplier { get; set; }
        public string InventoryLocation { get; set; } = "";
        public List<object> AuxDataList { get; set; } = new List<object>() { new { AuxData = "" } };
        public List<InboundItemModel> ItemList { get; set; }
    }

    public class GetBySkuResponseModel
    {
        public ItemModel Item { get; set; }

    }

    public class SupplierModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Tel1 { get; set; }
        public string Tel2 { get; set; } = "";
        public string Fax { get; set; } = "";
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Comments { get; set; }
    }

    public class InboundItemModel
    {
        public string SKU { get; set; }
        public string ExpectedQuantity { get; set; }
        public string ReceivedQuantity { get; set; }
        public List<object> LineAuxDataList { get; set; } = new List<object>() { new { LineAuxData = "" } };
    }

    public class OutboundModel
    {
        public string NotificationId { get; set; }
        public List<Outbound> OutboundList { get; set; }
    }

    public class Outbound
    {
        public string DeliveryNumber { get; set; }
        public DateTime Date { get; set; }
        public string OrderType { get; set; } = "ECOMMERCE";
        public DeliveryModel ShippingInfo { get; set; }
        public DeliveryModel DeliveryInfo { get; set; }
        public string PickingArea { get; set; } = string.Empty;
        public string Carrier { get; set; } = string.Empty;
        public string ShipMethod { get; set; }
        public List<object> AuxDataList { get; set; } = new List<object>() { new { AuxData = "" } };
        public List<OutboundItemModel> ItemList { get; set; }
    }

    public class DeliveryModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string PostalCode { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Tel1 { get; set; }
        public string Tel2 { get; set; } = "";
        public string Fax { get; set; } = "";
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Comments { get; set; }
        public List<object> AuxDataList { get; set; } = new List<object>() { new { AuxData = "" } };
    }

    public class OutboundItemModel
    {
        public string SKU { get; set; }
        public int RequestedQuantity { get; set; }
        public List<object> LineAuxDataList { get; set; } = new List<object>() { new { LineAuxData = "" } };
    }

    public class GenericResponse
    {
        public Status Status { get; set; }
    }

    public class Status
    {
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public bool Error { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class WebHookResponse
    {
        public int StatusCode { get; set; }
        public bool Error { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class MassiveInboudModel
    {
        public string Gtin { get; set; }
        public string Quantity { get; set; }
    }

    public class ProductToUpdateModel
    {
        public Product Product { get; set; }
        public int NewStock { get; set; }
        public int PreviousStockQuantityIn { get; set; }
        public ProductWarehouseInventory ProductWarehouseInventory { get; set; }
    }
}
