using iText.Forms.Xfdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.Franchise;
using Teed.Plugin.Groceries.Domain.OrderReports;
using Teed.Plugin.Groceries.Domain.ShippingVehicles;
using Teed.Plugin.Groceries.Extensions;
using Teed.Plugin.Groceries.Models.Franchise;
using Teed.Plugin.Groceries.Models.ShippingUserRoute;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class IncidentsController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly ShippingRouteService _shippingRouteService;
        private readonly ShippingRouteUserService _shippingRouteUserService;
        private readonly IOrderService _orderService;
        private readonly FranchiseService _franchiseService;
        private readonly PenaltiesCatalogService _penaltiesCatalogService;
        private readonly IncidentsService _incidentsService;
        private readonly IncidentFileService _incidentFileService;
        private readonly BillingService _billingService;
        private readonly ShippingVehicleService _shippingVehicleService;
        private readonly ShippingVehicleRouteService _shippingVehicleRouteService;
        private readonly NotDeliveredOrderItemService _notDeliveredOrderItemService;
        private readonly IProductService _productService;

        public IncidentsController(IPermissionService permissionService, ShippingRouteService shippingRouteService,
            ShippingRouteUserService shippingRouteUserService, IWorkContext workContext, ICustomerService customerService,
            IOrderService orderService,
            FranchiseService franchiseService,
            PenaltiesCatalogService penaltiesCatalogService,
            IncidentsService incidentsService,
            IncidentFileService incidentFileService,
            BillingService billingService,
            ShippingVehicleService shippingVehicleService,
            ShippingVehicleRouteService shippingVehicleRouteService,
            NotDeliveredOrderItemService notDeliveredOrderItemService,
            IProductService productService)
        {
            _permissionService = permissionService;
            _shippingRouteService = shippingRouteService;
            _shippingRouteUserService = shippingRouteUserService;
            _workContext = workContext;
            _customerService = customerService;
            _orderService = orderService;
            _franchiseService = franchiseService;
            _penaltiesCatalogService = penaltiesCatalogService;
            _incidentsService = incidentsService;
            _incidentFileService = incidentFileService;
            _billingService = billingService;
            _shippingVehicleService = shippingVehicleService;
            _shippingVehicleRouteService = shippingVehicleRouteService;
            _notDeliveredOrderItemService = notDeliveredOrderItemService;
            _productService = productService;
        }

        public void PrepareModel(IncidentsModel model)
        {
            var franchiseList = _franchiseService.GetAll();

            var penalties = IncidentCodes.CodeDictionary();
            model.Penalties = new List<SelectListItem>();
            foreach (var item in penalties)
            {
                var selectItem = new SelectListItem
                {
                    Value = item.Key.ToString(),
                    Text = $"{item.Key.ToString()} - {item.Value}"
                };
                model.Penalties.Add(selectItem);
            }

            model.Status = Enum.GetValues(typeof(IncidentStatus))
                .Cast<IncidentStatus>().Select(v => new SelectListItem
                {
                    Text = v.GetDisplayName(),
                    Value = ((int)v).ToString()
                }).ToList();
        }

        public string GetFranchiseName(int franchiseId, List<Franchise> franchises)
        {
            var franchise = franchises.Where(x => x.Id == franchiseId).FirstOrDefault();
            if (franchise != null)
                return franchise.Name;
            else
                return "Sin especificar";
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult SelectIdsData(SelectData data)
        {
            if (string.IsNullOrEmpty(data.Type) || data.FranchiseId < 1) return Ok();
            var parsedDate = DateTime.ParseExact(data.Date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var franchiseVehicles = _shippingVehicleService.GetAll().Where(x => x.Id == data.VehicleId).ToList();
            var franchiseOrders = OrderUtils.GetAllFranchiseOrders(franchiseVehicles, _shippingVehicleRouteService, _orderService)
                .Where(x => x.SelectedShippingDate == parsedDate).ToList();
            var franchiseOrderIds = franchiseOrders.Select(x => x.Id).ToList();
            var notDeliveredOrderItems = _notDeliveredOrderItemService.GetAll().Where(x => franchiseOrderIds.Contains(x.OrderId)).ToList();


            if (franchiseOrders.Any())
            {
                if (data.Type == "products")
                {
                    var products = franchiseOrders
                        .Where(x => data.Ids.Contains(x.Id))
                        .SelectMany(x => x.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService, notDeliveredOrderItems, false, true))
                        .Select(x => new
                        {
                            x.Id,
                            Text = x.Product.Name + " - " + x.PriceInclTax + " - Orden #" + x.OrderId,
                        }).ToList();
                    return Json(products);
                }
                else if (data.Type == "orders")
                {
                    var orderList = franchiseOrders
                        .Select(x => new
                        {
                            x.Id,
                            Text = "Orden #" + x.Id
                        }).ToList();
                    return Json(orderList);
                }
            }
            return Ok();
        }

        [HttpGet]
        public IActionResult GetAllowedQuantitiesAndOriginalQuantity(int orderItemId)
        {
            var data = OrderUtils.GetAllowedQuantities(orderItemId, _orderService,
                out int originalQuantity, _notDeliveredOrderItemService, true);
            if (data == null) return NotFound();
            return Ok(new { originalQuantity, items = data });
        }

        public IActionResult IncidentsList(string date, int franchiseId, int vehicleId)
        {
            var model = new IncidentsListModel()
            {
                Date = date,
                FranchiseId = franchiseId,
                VehicleId = vehicleId
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Incidents/IncidentsList.cshtml", model);
        }

        public IActionResult IncidentDatesList()
        {
            return View("~/Plugins/Teed.Plugin.Groceries/Views/Incidents/IncidentDatesList.cshtml");
        }

        public IActionResult IncidentVehiclesList(string date)
        {
            return View("~/Plugins/Teed.Plugin.Groceries/Views/Incidents/IncidentVehiclesList.cshtml", date);
        }

        [HttpPost]
        public IActionResult IncidentVehiclesListData(DataSourceRequest command, string date)
        {
            var parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var franchiseVehicles = _shippingVehicleService.GetAll().ToList();
            var vehicleRoutes = _shippingVehicleRouteService.GetAll().Where(x => x.ShippingDate == parsedDate).OrderBy(x => x.Vehicle.Brand).ToList();
            var vehicles = _shippingVehicleService.GetAll().Include(x => x.Franchise).OrderBy(x => x.FranchiseId);
            var pagedList = new PagedList<ShippingVehicle>(vehicles, command.Page - 1, command.PageSize);

            var incidents = _incidentsService.GetAll();

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    FranchiseName = x.Franchise == null ? "Sin franquicia asignada" : x.Franchise.Name,
                    FranchiseId = x.FranchiseId,
                    VehicleName = VehicleUtils.GetVehicleName(x),
                    VehicleId = x.Id,
                    RouteName = vehicleRoutes.Where(y => y.VehicleId == x.Id).Select(y => y.Route.RouteName).FirstOrDefault(),
                    Date = date,
                    IncidentsCount = incidents.Where(y => y.Date == parsedDate && y.VehicleId == x.Id).Count()
                }).ToList().OrderBy(x => string.IsNullOrEmpty(x.RouteName)).ThenBy(x => x.FranchiseId),
                Total = vehicleRoutes.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult IncidentDatesListData(DataSourceRequest command)
        {
            var now = DateTime.Now.Date;
            var orderDates = _orderService.GetAllOrdersQuery()
                .Where(x => x.SelectedShippingDate <= now)
                .GroupBy(x => x.SelectedShippingDate.Value)
                .Select(x => x.Key)
                .OrderByDescending(x => x);
            var pagedList = new PagedList<DateTime>(orderDates, command.Page - 1, command.PageSize);
            var incidents = _incidentsService.GetAll();

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    Text = x.ToString("dddd, dd-MM-yyyy", new CultureInfo("es-MX")),
                    Value = x.ToString("dd-MM-yyyy"),
                    IncidentsCount = incidents.Where(y => y.Date == x).Count()
                }).ToList(),
                Total = orderDates.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult IncidentsListData(DataSourceRequest command, string date, int franchiseId, int vehicleId)
        {
            var parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var query = _incidentsService.GetAll().Where(x => x.Date == parsedDate && x.FranchiseId == franchiseId && x.VehicleId == vehicleId).OrderByDescending(x => x.Date);
            var pagedList = new PagedList<Incidents>(query, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    Comments =
                        string.IsNullOrEmpty(x.Comments) ? "Sin comentarios" : x.Comments,
                    Date = x.Date.ToString("dd/MM/yyyy"),
                    Franchise = x.Franchise?.Name,
                    Vehicle = VehicleUtils.GetVehicleName(x.Vehicle),
                    Amount = x.Amount.ToString("C"),
                    x.IncidentCode,
                    Authorized = x.AuthorizedStatusId == 0 ? "No autorizado" : "Autorizado"
                }).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        public IActionResult Create(string date, int franchiseId, int vehicleId, int orderId = 0)
        {
            IncidentsModel model = new IncidentsModel();

            PrepareModel(model);

            model.AuthorizedStatusId = 1;
            model.Date = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            model.DateString = date;
            model.FranchiseId = franchiseId;
            model.VehicleId = vehicleId;
            model.VehicleName = VehicleUtils.GetVehicleName(_shippingVehicleService.GetAll().Where(x => x.Id == vehicleId).FirstOrDefault());
            model.FranchiseName = _franchiseService.GetAll().Where(x => x.Id == franchiseId).FirstOrDefault()?.Name;
            model.OrderIds = orderId == 0 ? null : orderId.ToString();
            model.OrderIdsList = orderId == 0 ? null : new List<int>() { orderId };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Incidents/Create.cshtml", model);
        }

        [HttpPost]
        public IActionResult Create(IncidentsModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var parsedDate = DateTime.ParseExact(model.DateString, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;

            Incidents incident = new Incidents()
            {
                Comments = model.Comments,
                Date = parsedDate,
                FranchiseId = model.FranchiseId,
                IncidentCode = model.IncidentCode,
                VehicleId = model.VehicleId,
                AuthorizedStatusId = model.AuthorizedStatusId,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) creó la incidencia {model.Id}.\n"
            };
            switch (model.IncidentCode)
            {
                case "R01":
                case "R02":
                    if (string.IsNullOrWhiteSpace(model.OrderItemIds) || string.IsNullOrWhiteSpace(model.OrderIds)) return BadRequest();
                    incident.OrderItemIds = model.OrderItemIds;
                    incident.OrderIds = model.OrderIds;
                    if (model.SelectedQuantity > 0)
                    {
                        var orderItemId = int.Parse(incident.OrderItemIds);
                        incident.SelectedQuantity = model.SelectedQuantity;

                        NotDeliveredOrderItem notDeliveredOrderItem = null;
                        var orderItem = _orderService.GetOrderItemById(orderItemId);
                        if (orderItem == null)
                        {
                            notDeliveredOrderItem = _notDeliveredOrderItemService.GetAll().Where(x => x.OrderItemId == orderItemId).FirstOrDefault();
                            if (notDeliveredOrderItem != null)
                            {
                                var product = _productService.GetProductById(notDeliveredOrderItem.ProductId);
                                var order = _orderService.GetOrderById(notDeliveredOrderItem.OrderId);
                                orderItem = OrderUtils.ConvertToOrderItem(notDeliveredOrderItem, _orderService, _productService, new List<Product> { product }, new List<Order> { order });
                            }
                        }
                        var exactQuantity = ProductUtils.ParseProductExactQuantity(orderItem, model.SelectedQuantity);
                        incident.Comments = string.IsNullOrWhiteSpace(incident.Comments) ?
                            $"Incidencia de tipo {incident.IncidentCode} por el producto {orderItem.Product.Name} (Cantidad: {exactQuantity.Item1 + exactQuantity.Item2})" :
                            incident.Comments;
                        //var testAmount = _billingService.GetTotalOfIncident(incident.Id, model.VehicleId, incident.Date, incident.Date, incident);
                    }
                    break;
                case "R06":
                case "R11":
                    if (string.IsNullOrWhiteSpace(model.OrderIds)) return BadRequest();
                    incident.OrderIds = model.OrderIds;
                    break;
                case "R09":
                case "R10":
                case "D01":
                case "Z00":
                    if (model.ReportedAmount == null) return BadRequest();
                    incident.ReportedAmount = model.ReportedAmount;
                    break;
                default:
                    break;
            }
            _incidentsService.Insert(incident);

            incident.Amount = _billingService.GetTotalOfIncident(incident.Id, model.VehicleId, incident.Date, incident.Date);
            _incidentsService.Update(incident);

            return RedirectToAction("Edit", new { incident.Id });
        }

        public IActionResult Edit(int id)
        {
            var incident = _incidentsService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (incident == null) return RedirectToAction("IncidentsList");

            var model = new IncidentsModel()
            {
                Id = incident.Id,
                Comments = incident.Comments,
                Date = incident.Date,
                FranchiseId = incident.FranchiseId,
                IncidentCode = incident.IncidentCode,
                OrderIds = incident.OrderIds,
                OrderItemIds = incident.OrderItemIds,
                ReportedAmount = incident.ReportedAmount,
                AuthorizedStatusId = incident.AuthorizedStatusId,
                Log = incident.Log,
                VehicleId = incident.VehicleId,
                Amount = incident.Amount,
                Orders = new List<string>(),
                Products = new List<string>()
            };
            PrepareModel(model);

            model.DateString = model.Date.ToString("dd-MM-yyyy");
            model.VehicleName = VehicleUtils.GetVehicleName(_shippingVehicleService.GetAll().Where(x => x.Id == model.VehicleId).FirstOrDefault());
            model.FranchiseName = _franchiseService.GetAll().Where(x => x.Id == model.FranchiseId).FirstOrDefault()?.Name;

            if (!string.IsNullOrWhiteSpace(incident.OrderIds))
            {
                var orderIds = incident.OrderIds.Split(',').Select(x => int.Parse(x.Trim())).ToList();
                var orders = _orderService.GetAllOrdersQuery().Where(x => orderIds.Contains(x.Id)).OrderBy(x => x.Id).ToList();
                model.Orders = orders.Select(x => "Orden #" + x.CustomOrderNumber).ToList();

                if (!string.IsNullOrWhiteSpace(incident.OrderItemIds))
                {
                    var orderItemIds = incident.OrderItemIds?.Split(',').Select(x => int.Parse(x.Trim())).ToList();
                    var notDeliveredOrderItems = _notDeliveredOrderItemService.GetAll().Where(x => orderItemIds.Contains(x.OrderItemId)).ToList();

                    var orderItems = orders
                        .SelectMany(y => y.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService, notDeliveredOrderItems, false, true))
                        .Where(x => orderItemIds.Contains(x.Id));
                    var exactQuantity = ProductUtils.ParseProductExactQuantity(orderItems.FirstOrDefault(), incident.SelectedQuantity ?? orderItems.FirstOrDefault().Quantity);
                    model.Products = orderItems
                        .Select(x => new { x.Product.Name, x.PriceInclTax, x.OrderId })
                        .ToList()
                        .Select(x => x.Name + (incident.SelectedQuantity != null ? $" ({exactQuantity.Item1 + exactQuantity.Item2})" : string.Empty) + " - " +
                        (incident.AmountOfSelectedQuantity != null ? (incident.AmountOfSelectedQuantity ?? 0).ToString("C") : x.PriceInclTax.ToString("C"))
                        + " - Orden #" + x.OrderId).ToList();
                }
            }

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Incidents/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult Edit(IncidentsModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var incident = _incidentsService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (incident == null) return RedirectToAction("IncidentsList");
            //var franchises = _franchiseService.GetAll();
            //var franchise = new Franchise();

            string log = string.Empty;
            if (incident.Comments != model.Comments)
            {
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.Email + $" ({_workContext.CurrentCustomer.Id}) cambió el comentario de la incidencia de {incident.Comments} por {model.Comments}.\n";
                incident.Log += log;

                incident.Comments = model.Comments;
            }

            //if (incident.Date != model.Date)
            //{
            //    log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.Email + $" ({_workContext.CurrentCustomer.Id}) cambió la fecha de la incidencia de {incident.Date.ToString("dd/MM/yyyy")} por {model.Date.ToString("dd/MM/yyyy")}.\n";
            //    incident.Log += log;

            //    incident.Date = model.Date;
            //}

            //if (incident.IncidentCode != model.IncidentCode)
            //{
            //    log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.Email + $" ({_workContext.CurrentCustomer.Id}) cambió el codigo de la incidencia de {incident.IncidentCode} por {model.IncidentCode}.\n";
            //    incident.Log += log;

            //    incident.IncidentCode = model.IncidentCode;
            //}

            //if (incident.FranchiseId != model.FranchiseId)
            //{
            //    var oldFranchise = franchises.Where(x => x.Id == incident.FranchiseId).FirstOrDefault();
            //    franchise = franchises.Where(x => x.Id == model.FranchiseId).FirstOrDefault();
            //    log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.Email + $" ({_workContext.CurrentCustomer.Id}) cambió la franquicia de la incidencia de [{oldFranchise?.Name ?? "Sin especificar"}] por [{franchise?.Name ?? "Sin especificar"}].\n";
            //    incident.Log += log;

            //    incident.FranchiseId = model.FranchiseId;
            //}

            if (incident.AuthorizedStatusId != model.AuthorizedStatusId)
            {
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.Email + $" ({_workContext.CurrentCustomer.Id}) cambió el estatus de autorización de la incidencia de {EnumHelper.GetDisplayName((IncidentStatus)incident.AuthorizedStatusId)} por {EnumHelper.GetDisplayName((IncidentStatus)model.AuthorizedStatusId)}.\n";
                incident.Log += log;

                incident.AuthorizedStatusId = model.AuthorizedStatusId;
            }

            //if (incident.Amount != model.Amount)
            //{
            //    log = "(" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.Email + $" ({_workContext.CurrentCustomer.Id})) Se modificó el monto de la incidencia después de cambios manuales de {incident.Amount} por {model.Amount}.\n";
            //    incident.Log += log;

            //    incident.Amount = model.Amount;
            //}

            //switch (model.IncidentCode)
            //{
            //    case "R01":
            //    case "R02":
            //        incident.OrderItemIds = model.OrderItemIds;
            //        incident.OrderIds = model.OrderIds;
            //        incident.ReportedAmount = null;
            //        break;
            //    case "R06":
            //    case "R11":
            //        incident.OrderIds = model.OrderIds;
            //        incident.OrderItemIds = null;
            //        incident.ReportedAmount = null;
            //        break;
            //    case "R09":
            //    case "R10":
            //    case "D01":
            //        incident.ReportedAmount = model.ReportedAmount;
            //        incident.OrderIds = null;
            //        incident.OrderItemIds = null;
            //        break;
            //    default:
            //        break;
            //}

            //franchise = _franchiseService.GetAll()
            //    .Where(x => x.Id == incident.FranchiseId)
            //    .FirstOrDefault();
            //incident.Amount = _billingService.GetTotalOfIncident(franchise, incident.Id, model.VehicleId, incident.Date, incident.Date);
            _incidentsService.Update(incident);

            return RedirectToAction("Edit", new { incident.Id });
        }

        //delete
        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            var incident = _incidentsService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (incident == null)
                //No recurring payment found with the specified id
                return RedirectToAction("IncidentsList");

            _incidentsService.Delete(incident);

            return RedirectToAction("IncidentsList");
        }

        public IActionResult IncidentFileList(int incidentId, DataSourceRequest command)
        {
            var query = _incidentFileService.GetAll().Where(x => x.IncidentId == incidentId);
            var queryList = query.OrderByDescending(m => m.CreatedOnUtc).ToList();
            var pagedList = new PagedList<IncidentFile>(queryList, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    Description = string.IsNullOrEmpty(x.Description) ?
                    "Sin descripción" : x.Description,
                }).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult IncidentFileAdd(AddIncidentFile model)
        {
            try
            {
                byte[] bytes = new byte[0];
                using (var ms = new MemoryStream())
                {
                    model.File.CopyTo(ms);
                    bytes = ms.ToArray();
                }
                var file = new IncidentFile
                {
                    File = bytes,
                    Extension = model.File.FileName.Substring(model.File.FileName.LastIndexOf(".") + 1),
                    FileMimeType = model.File.ContentType,
                    Size = (int)model.File.Length,
                    Description = model.Description,
                    IncidentId = model.IncidentId,
                };
                _incidentFileService.Insert(file);
                var incident = _incidentsService.GetAll().Where(x => x.Id == model.IncidentId).FirstOrDefault();
                if (incident != null)
                {
                    incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) subío un archivo ({file.Id}) a la incidencia.\n";
                    _incidentsService.Update(incident);
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        public IActionResult IncidentFileUpdate(AddIncidentFile model)
        {
            var file = _incidentFileService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (file == null)
                return Ok();

            file.Description = model.Description;
            _incidentFileService.Update(file);
            var incident = _incidentsService.GetAll().Where(x => x.Id == model.IncidentId).FirstOrDefault();
            if (incident != null)
            {
                incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) actualizó un archivo ({file.Id}) de la incidencia.\n";
                _incidentsService.Update(incident);
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult IncidentFileDelete(AddIncidentFile model)
        {
            var file = _incidentFileService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (file == null)
                return Ok();

            file.Deleted = true;
            _incidentFileService.Update(file);
            var incident = _incidentsService.GetAll().Where(x => x.Id == model.IncidentId).FirstOrDefault();
            if (incident != null)
            {
                incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) eliminó un archivo de tipo ({file.Id}) de la incidencia.\n";
                _incidentsService.Update(incident);
            }

            return Ok();
        }

        [HttpGet]
        public IActionResult IncidentFileDownload(int id)
        {
            var file = _incidentFileService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (file == null)
                return Ok();
            var model = new AddIncidentFile
            {
                Extension = file.Extension,
                FileArray = file.File
            };
            return Json(model);
        }
    }

    public class SelectData
    {
        public string Type { get; set; }
        public string Date { get; set; }
        public int FranchiseId { get; set; }
        public int VehicleId { get; set; }
        public List<int> Ids { get; set; }
    }
}
