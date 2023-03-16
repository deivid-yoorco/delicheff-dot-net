using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;
using Teed.Plugin.Groceries.Models.ShippingRoute;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Manager.Models.Groceries;
using Teed.Plugin.Groceries.Domain.ShippingZones;
using Nop.Services.Catalog;
using Teed.Plugin.Groceries.Extensions;
using Teed.Plugin.Groceries.Domain.OrderItemBuyers;
using Nop.Core.Domain.Customers;
using System.Data.Entity;
using Teed.Plugin.Groceries.Domain.RescheduledOrderLogs;
using Teed.Plugin.Groceries.Security;
using iText.Forms.Xfdf;
using Teed.Plugin.Groceries.Domain.Franchise;
using Teed.Plugin.Groceries.Utils;
using Teed.Plugin.Groceries.Domain.ShippingVehicles;
using Nop.Services.Helpers;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class ShippingRouteController : BasePluginController
    {
        public const string GOOGLE_API_KEY = "AIzaSyAUvh0c7WBIqAeMnpGnx09MKsYQOlHkJJw";
        public const string ORIGIN_COORDINATES = "19.381686,-99.086757";

        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IAddressService _addressService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly OrderItemBuyerService _orderItemBuyerService;
        private readonly NotDeliveredOrderItemService _notDeliveredOrderItemService;
        private readonly ShippingRouteService _shippingRouteService;
        private readonly ShippingRouteUserService _shippingRouteUserService;
        private readonly ShippingZoneService _shippingZoneService;
        private readonly OrderTypeService _orderTypeService;
        private readonly RescheduledOrderLogService _orderLogService;
        private readonly FranchiseService _franchiseService;
        private readonly ShippingVehicleRouteService _shippingVehicleRouteService;
        private readonly ShippingVehicleService _shippingVehicleService;
        private readonly ManufacturerListDownloadService _manufacturerListDownloadService;
        private readonly OrderOptimizationRequestService _orderOptimizationRequestService;

        public ShippingRouteController(IPermissionService permissionService, ShippingRouteService shippingRouteService,
            ShippingRouteUserService shippingRouteUserService, IWorkContext workContext, ICustomerService customerService,
            IOrderService orderService, IAddressService addressService, ShippingZoneService shippingZoneService, OrderTypeService orderTypeService,
            IManufacturerService manufacturerService, NotDeliveredOrderItemService notDeliveredOrderItemService, IProductService productService,
            OrderItemBuyerService orderItemBuyerService, RescheduledOrderLogService orderLogService,
            FranchiseService franchiseService, ShippingVehicleRouteService shippingVehicleRouteService,
            ShippingVehicleService shippingVehicleService, ManufacturerListDownloadService manufacturerListDownloadService,
            OrderOptimizationRequestService orderOptimizationRequestService)
        {
            _permissionService = permissionService;
            _shippingRouteService = shippingRouteService;
            _shippingRouteUserService = shippingRouteUserService;
            _workContext = workContext;
            _customerService = customerService;
            _orderService = orderService;
            _addressService = addressService;
            _shippingZoneService = shippingZoneService;
            _orderTypeService = orderTypeService;
            _manufacturerService = manufacturerService;
            _notDeliveredOrderItemService = notDeliveredOrderItemService;
            _productService = productService;
            _orderItemBuyerService = orderItemBuyerService;
            _orderLogService = orderLogService;
            _franchiseService = franchiseService;
            _shippingVehicleRouteService = shippingVehicleRouteService;
            _shippingVehicleService = shippingVehicleService;
            _manufacturerListDownloadService = manufacturerListDownloadService;
            _orderOptimizationRequestService = orderOptimizationRequestService;
        }

        [AuthorizeAdmin]
        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRoute))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingRoute/List.cshtml");
        }

        [AuthorizeAdmin]
        public IActionResult AssignRouteOrderList()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteAssign))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingRoute/AssignRouteOrderList.cshtml");
        }

        [AuthorizeAdmin]
        public IActionResult RouteMap(string date)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteAssign))
                return AccessDeniedView();

            var parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingRoute/RouteMap.cshtml", parsedDate);
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult UpdateOrderShippingDate(
            string oldShippingDate,
            string shippingDate,
            int orderId,
            int oldTime,
            int newTime)
        {
            if (orderId < 1)
                return BadRequest("id");

            var changedDate = false;
            var changedTime = false;
            var order = _orderService.GetOrderById(orderId);
            DateTime? date = null;
            if (!string.IsNullOrEmpty(shippingDate))
            {
                date = DateTime.ParseExact(shippingDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            if (order != null)
            {
                var stringTime = "";
                var oldStringTime = "";

                if (newTime > 0 && (newTime != oldTime))
                {
                    switch (newTime)
                    {
                        case 1:
                            stringTime = "1:00 PM - 3:00 PM";
                            break;
                        case 2:
                            stringTime = "3:00 PM - 5:00 PM";
                            break;
                        case 3:
                            stringTime = "5:00 PM - 7:00 PM";
                            break;
                        case 4:
                            stringTime = "7:00 PM - 9:00 PM";
                            break;
                        default:
                            return BadRequest("time");
                    }
                    order.SelectedShippingTime = stringTime;
                    changedTime = true;
                }

                switch (oldTime)
                {
                    case 1:
                        oldStringTime = "1:00 PM - 3:00 PM";
                        break;
                    case 2:
                        oldStringTime = "3:00 PM - 5:00 PM";
                        break;
                    case 3:
                        oldStringTime = "5:00 PM - 7:00 PM";
                        break;
                    case 4:
                        oldStringTime = "7:00 PM - 9:00 PM";
                        break;
                    default:
                        oldStringTime = "(Sin horario de envío previo)";
                        break;
                }

                if (date != null)
                {
                    if (oldShippingDate != shippingDate)
                    {
                        changedDate = true;
                        order.SelectedShippingDate = date;
                        try
                        {
                            var dateOld = DateTime.ParseExact(oldShippingDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            var finalOldDate = date ?? DateTime.Now;
                            RescheduledOrderLog orderLog = new RescheduledOrderLog
                            {
                                OrderId = order.Id,
                                NewShippingDate = finalOldDate.ToUniversalTime(),
                                OriginalShippingDate = dateOld.ToUniversalTime(),
                                RescheduledBy = _workContext.CurrentCustomer.Id,
                                OriginalShippingTime = oldStringTime,
                                ShippingTime = changedTime ? stringTime : oldStringTime,
                                OriginalRouteId = order.RouteId
                            };
                            _orderLogService.Insert(orderLog);
                        }
                        catch (Exception e)
                        {
                            order.OrderNotes.Add(new OrderNote
                            {
                                Note = "Error creating RescheduledOrderLog for this order, Error message: " + e.Message,
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                        }
                    }
                }
                else
                    return BadRequest("date");


                if (changedDate || changedTime)
                {
                    var note = $"{_workContext.CurrentCustomer.Email} cambió";
                    if (changedDate && changedTime)
                        note += $" la fecha de entrega de {oldShippingDate} a {shippingDate} y";
                    else if (changedDate && !changedTime)
                        note += $" la fecha de entrega de {oldShippingDate} a {shippingDate}.";

                    if (changedTime)
                        note += $" la hora de entrega de {oldStringTime} a {stringTime}.";
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = note,
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        CustomerId = _workContext.CurrentCustomer.Id
                    });

                    if (changedDate)
                    {
                        var currentOptimization = _orderOptimizationRequestService.GetAll().Where(x => x.OrderId == order.Id).FirstOrDefault();
                        if (currentOptimization != null)
                        {
                            _orderOptimizationRequestService.Delete(currentOptimization);
                            order.OrderNotes.Add(new OrderNote
                            {
                                Note = "La optimización fue eliminada de forma automática ya que se reprogramó la orden.",
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow,
                                CustomerId = _workContext.CurrentCustomer.Id
                            });
                        }
                    }

                    _orderService.UpdateOrder(order);
                }
                return Ok();
            }
            else
                return BadRequest("order");
        }

        [AuthorizeAdmin]
        public IActionResult RouteMapData(string date)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate == parsedDate);
            var groupedOrders = ordersQuery
                                .GroupBy(x => x.RouteId)
                                .ToList();
            var routesFromPedidos = OrderUtils.GetPedidosOnly(ordersQuery).Select(x => x.RouteId).ToList();
            var routes = _shippingRouteService.GetAll().ToList();
            var zones = _shippingZoneService.GetAll().ToList();
            var vehicleRoutes = _shippingVehicleRouteService.GetAll().Where(x => x.ShippingDate == parsedDate).ToList();
            var orderIds = ordersQuery.Select(x => x.Id).ToList();
            var optimizedOrders = _orderOptimizationRequestService.GetAll().Where(x => orderIds.Contains(x.OrderId)).ToList();

            RouteMapModel model = new RouteMapModel()
            {
                Routes = routes,
                Data = groupedOrders.Select(x => new MapDataModel()
                {
                    Route = routes.Where(y => y.Id == x.Key).FirstOrDefault(),
                    FranchiseName = vehicleRoutes.Where(y => y.RouteId == x.Key).Select(y => y.Vehicle?.Franchise?.Name).FirstOrDefault(),
                    Orders = OrderUtils.GetPedidosGroupByList(x.Select(y => y).ToList()).Select(y => new MapOrderData()
                    {
                        OrderIds = string.Join(",", y.Select(z => z.Id.ToString()).ToList()),
                        ShippingAddress = y.FirstOrDefault()?.ShippingAddress?.Address1,
                        ZoneName = y.FirstOrDefault().ZoneId.HasValue ? zones.Where(z => z.Id == y.FirstOrDefault().ZoneId).Select(z => z.ZoneName).FirstOrDefault() : null,
                        OrderNumber = string.Join(", ", y.Select(z => "Orden #" + z.CustomOrderNumber).ToList()),
                        PostalCode = y.FirstOrDefault().ShippingAddress.ZipPostalCode,
                        ShippingFullName = y.FirstOrDefault().ShippingAddress.FirstName + " " + y.FirstOrDefault().ShippingAddress.LastName,
                        SelectedShippingTime = y.FirstOrDefault().SelectedShippingTime,
                        Latitude = y.FirstOrDefault().ShippingAddress.Latitude,
                        Longitude = y.FirstOrDefault().ShippingAddress.Longitude,
                        OrderTotal = string.Join(", ", y.Select(z => z.OrderTotal.ToString("C")).ToList()),
                        ProductCount = string.Join(", ", y.Select(z => z.OrderItems.Count.ToString() + " productos").ToList()),
                        OrderStatusId = y.FirstOrDefault().OrderStatusId,
                        OptimizeTypeId = optimizedOrders.Where(a => y.Select(z => z.Id).ToList().Contains(a.OrderId)).Select(a => a.SelectedOptimizationTypeId).FirstOrDefault(),
                        PaymentMethodSystemName = string.Join(", ", y.Select(z => OrderUtils.GetPaymentOptionName(z.PaymentMethodSystemName) + " " + (z.PaymentStatus == Nop.Core.Domain.Payments.PaymentStatus.Paid ? "(pagado)" : "")).ToList()),
                        OptimizationTimes = optimizedOrders.Where(a => y.Select(z => z.Id).ToList().Contains(a.OrderId)).Select(a => new List<string>() { a.TimeOption1, a.TimeOption2, a.TimeOption3 }).FirstOrDefault(),
                        OptimizeStatusId = optimizedOrders.Where(a => y.Select(z => z.Id).ToList().Contains(a.OrderId)).Select(a => a.CurrentStatusId).FirstOrDefault()
                    }).ToList(),
                    ProductsCount = x.SelectMany(y => y.OrderItems).Count(),
                    OrderTotals = x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum(),
                    AverageTicket = routesFromPedidos.Count == 0 ? 0 : x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum() / routesFromPedidos.Where(z => z == x.Key).Count(),
                    TotalDistance = Math.Round(((decimal)x.Select(y => y.PreviousPointTransferDistance ?? 0).DefaultIfEmpty().Sum() / (decimal)1000), 2),
                    TotalTime = Math.Round(((decimal)x.Select(y => y.PreviousPointTransferTime ?? 0).DefaultIfEmpty().Sum() / (decimal)60), 2),
                }).OrderBy(x => x.Route == null).ToList(),
                ProductsTotal = groupedOrders.SelectMany(x => x).SelectMany(x => x.OrderItems).Count(),
                SalesAverage = groupedOrders.Count == 0 ? 0 : groupedOrders.SelectMany(x => x).Select(x => x.OrderTotal).Sum() / groupedOrders.Count,
                ProductsAverage = groupedOrders.SelectMany(x => x).SelectMany(x => x.OrderItems).Count() == 0 ? 0 : groupedOrders.SelectMany(x => x).SelectMany(x => x.OrderItems).Count() / groupedOrders.Count
            };

            return Ok(model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult UpdateOrdersFromMap(UpdateOrdersFromMapModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteAssign))
                return AccessDeniedView();

            if (string.IsNullOrEmpty(model.Date)) return BadRequest();
            var selectedDate = DateTime.ParseExact(model.Date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate == selectedDate).ToList();

            if (model.Data != null)
            {
                foreach (var item in model.Data)
                {
                    var orderIds = item.OrderIds.Split(',').Select(x => int.Parse(x)).ToArray();
                    foreach (var orderId in orderIds)
                    {
                        Order order = orders.Where(x => x.Id == orderId).FirstOrDefault();
                        if (order == null) return BadRequest();
                        order.RouteId = item.NewRouteId;
                        order.OrderNotes.Add(new OrderNote()
                        {
                            CreatedOnUtc = DateTime.UtcNow,
                            OrderId = order.Id,
                            DisplayToCustomer = false,
                            Note = $"El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió la ruta de la orden de {item.OriginalRouteName} a {item.NewRouteName} utilizando el mapa.",
                            CustomerId = _workContext.CurrentCustomer.Id
                        });
                        _orderService.UpdateOrder(order);
                    }
                }
            }

            if (model.OptimizationData != null)
            {
                foreach (var item in model.OptimizationData)
                {
                    var orderIds = item.OrderIds.Split(',').Select(x => int.Parse(x)).ToArray();
                    foreach (var orderId in orderIds)
                    {
                        Order order = orders.Where(x => x.Id == orderId).FirstOrDefault();
                        if (order == null) return BadRequest();
                        if (item.SelectedOptimizationTypeId == 0 || string.IsNullOrEmpty(item.TimeOption1)) continue;
                        OrderOptimizationRequest orderOptimizationRequest = new OrderOptimizationRequest()
                        {
                            OrderId = order.Id,
                            CurrentStatusId = (int)OrderOptimizationStatus.Pending,
                            OriginalSelectedTime = order.SelectedShippingTime,
                            SelectedOptimizationTypeId = item.SelectedOptimizationTypeId,
                            TimeOption1 = item.TimeOption1,
                            TimeOption2 = item.TimeOption2 != "0" ? item.TimeOption2 : null,
                            TimeOption3 = item.TimeOption3 != "0" ? item.TimeOption3 : null,
                            ManagerComment = item.ManagerComment
                        };
                        _orderOptimizationRequestService.Insert(orderOptimizationRequest);

                        string logNote = $"Con la opción 1 de horario: {item.TimeOption1}. ";
                        if (!string.IsNullOrEmpty(item.TimeOption2) && item.TimeOption2 != "0")
                            logNote += $"Con la opción 2 de horario: {item.TimeOption2}. ";
                        if (!string.IsNullOrEmpty(item.TimeOption3) && item.TimeOption3 != "0")
                            logNote += $"Con la opción 3 de horario: {item.TimeOption3}. ";
                        order.OrderNotes.Add(new OrderNote()
                        {
                            CreatedOnUtc = DateTime.UtcNow,
                            OrderId = order.Id,
                            DisplayToCustomer = false,
                            Note = $"El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) creó una solicitud de optimización de tipo '{((OrderOptimizationTypes)item.SelectedOptimizationTypeId).GetDisplayName()}'. {logNote}",
                            CustomerId = _workContext.CurrentCustomer.Id
                        });
                        _orderService.UpdateOrder(order);
                    }
                }
            }

            return NoContent();
        }

        [AuthorizeAdmin]
        public IActionResult MonitorRoutesList()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteMonitor))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingRoute/MonitorRoutesList.cshtml");
        }

        [AuthorizeAdmin]
        public IActionResult AssignRouteOrder(string date, int orderBy = 0, bool isForMonitor = false,
            int franchiseId = 0)
        {
            if ((!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteAssign) && !isForMonitor) ||
                (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteMonitor) && isForMonitor))
                return AccessDeniedView();
            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService);
            var orders = ordersQuery;
            var dateDelivery = DateTime.Now.Date;
            if (franchiseId > 0)
            {
                var franchise = _franchiseService.GetAll().Where(x => x.Id == franchiseId && x.IsActive).FirstOrDefault();
                if (franchise == null) return NotFound();
                if (franchise.UserInChargeId != _workContext.CurrentCustomer.Id) return AccessDeniedView();
                var franchiseVehicles = _shippingVehicleService.GetAll().Where(x => x.FranchiseId == franchiseId && x.Active).ToList();
                if (!string.IsNullOrEmpty(date))
                    dateDelivery = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                orders = OrderUtils.GetAllFranchiseOrders(franchiseVehicles, _shippingVehicleRouteService, _orderService).Where(x => x.SelectedShippingDate == dateDelivery);
            }
            else
            {
                dateDelivery = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                orders = orders.Where(x => x.SelectedShippingDate == dateDelivery);
            }
            var assignedVehicles = _shippingVehicleRouteService.GetAll().Where(x => x.ShippingDate == dateDelivery).ToList();
            bool allowManufacturerListDownload = _manufacturerListDownloadService.GetAll().Where(x => x.OrderShippingDate == dateDelivery).Select(x => x.AllowDownload).FirstOrDefault();

            switch (orderBy)
            {
                case 1:
                    orders = orders.OrderBy(x => x.RouteDisplayOrder);
                    break;
                case 2:
                    orders = orders.OrderBy(x => x.ShippingAddress.Address2);
                    break;
                default:
                    orders = orders.OrderBy(x => x.ShippingAddress.ZipPostalCode).ThenBy(x => x.Id);
                    break;
            }

            var orderTypes = _orderTypeService.GetAll().ToList();
            if (isForMonitor)
            {
                var deliveryRole = _customerService.GetCustomerRoleBySystemName("delivery");
                var customers = _customerService.GetAllCustomers(customerRoleIds: new int[] { deliveryRole.Id });
                var assignedRoutes = _shippingRouteUserService.GetAll().Where(x => DbFunctions.TruncateTime(x.ResponsableDateUtc) == dateDelivery).ToList();
                var orderIds = orders.Select(y => y.Id).ToList();
                var dateForCompare = dateDelivery.Date;

                var rescheduledOrderLogs = _orderLogService.GetAll()
                    .Where(x => DbFunctions.TruncateTime(x.OriginalShippingDate) == dateForCompare)
                    .ToList()
                    .Where(x => x.OriginalShippingDate.Date == x.CreatedOnUtc.ToLocalTime().Date)
                    .ToList();

                var finalOrders = orders.ToList();
                var rescheduledOrderIds = new List<int>();
                if (rescheduledOrderLogs.Any())
                {
                    rescheduledOrderIds = rescheduledOrderLogs.Select(x => x.OrderId).ToList();
                    var rescheduledOrders = ordersQuery.Where(x => rescheduledOrderIds.Contains(x.Id)).ToList();
                    finalOrders.AddRange(rescheduledOrders);
                }


                var rescuedOrders = orders.Where(x => x.RescuedByRouteId.HasValue && x.RescuedByRouteId > 0).ToList().Select(x => OrderUtils.Clone(x)).ToList();
                if (rescuedOrders.Any())
                {
                    foreach (var rescuedOrder in rescuedOrders)
                    {
                        rescuedOrder.RouteDisplayOrder = rescuedOrder.RescueRouteDisplayOrder.Value;
                        rescuedOrder.RouteId = rescuedOrder.RescuedByRouteId.Value;
                        rescuedOrder.RescuedByRouteId = null;
                        rescuedOrder.RescueRouteDisplayOrder = (int?)-99; //bandera para identificar orden reprogramada
                    }
                    finalOrders.AddRange(rescuedOrders);
                }

                switch (orderBy)
                {
                    case 1:
                        finalOrders.OrderBy(x => x.RouteDisplayOrder);
                        break;
                    case 2:
                        finalOrders.OrderBy(x => x.ShippingAddress.Address2);
                        break;
                    default:
                        finalOrders.OrderBy(x => x.ShippingAddress.ZipPostalCode).ThenBy(x => x.Id);
                        break;
                }

                var model = new ShippingRouteOrderModelForMonitor()
                {
                    FranchiseId = franchiseId,
                    Date = dateDelivery,
                    Orders = finalOrders,
                    Customers = customers.Select(x => new AssignCustomer()
                    {
                        Text = x.GetFullName(),
                        Value = x.Id
                    }).ToList(),
                    Routes = _shippingRouteService.GetAll().ToList().Select(x => new AssignedRoute()
                    {
                        Text = x.RouteName,
                        Value = x.Id.ToString(),
                        CustomerIds = assignedRoutes.Where(y => y.ShippingRouteId == x.Id).Select(y => y.UserInChargeId).ToList(),
                        AssignedVehicle = assignedVehicles.Where(y => y.RouteId == x.Id).FirstOrDefault()?.VehicleId == -1 ? "Camioneta rentada" : assignedVehicles.Where(y => y.RouteId == x.Id).Count() > 0 ? VehicleUtils.GetVehicleName(assignedVehicles.Where(y => y.RouteId == x.Id).FirstOrDefault().Vehicle) : "Sin camioneta asignada",
                        //CapacityExceeded = IsCapacityExceeded(assignedVehicles.Where(y => y.RouteId == x.Id).FirstOrDefault()?.Vehicle, finalOrders, orderTypes)
                    }).ToList(),
                    RescheduledOrderLogs = rescheduledOrderLogs
                };

                var tableListData = new List<ShippingRouteMonitorTableData>();
                var groupedByRoute = finalOrders.GroupBy(x => x.RouteId).ToList();
                foreach (var group in groupedByRoute)
                {
                    var filteredOrders = group.Where(x => (x.RescuedByRouteId == 0 || !x.RescuedByRouteId.HasValue) && !rescheduledOrderIds.Contains(x.Id));//No rescatadas, rescatadas por esta ruta, no reagendadas
                    var payedOrders = filteredOrders.Where(x => x.PaymentStatusId == 30); //no rescatadas, rescatadas por esta ruta, no reagendadas, pagadas
                    var currentRescuedOrders = group.Where(x => (x.RescuedByRouteId > 0 && x.RescuedByRouteId != group.Key) && !rescheduledOrderIds.Contains(x.Id)).ToList();//ordenes rescatadas por otra ruta, no reagendadas
                    var deliveredPedidos = OrderUtils.GetPedidosGroupByList(filteredOrders.Where(x => x.Shipments.Where(y => y.DeliveryDateUtc.HasValue).Any() && x.OrderStatusId != 50).ToList());
                    var notDeliveredPedidos = OrderUtils.GetPedidosGroupByList(filteredOrders.Where(x => x.OrderStatusId == 50).ToList());

                    var tableData = new ShippingRouteMonitorTableData()
                    {
                        RouteId = group.Key,
                        RouteName = model.Routes.Where(x => x.Value == group.Key.ToString()).FirstOrDefault()?.Text,
                        CashAmount = payedOrders.Where(x => x.PaymentMethodSystemName == "Payments.CashOnDelivery").Select(x => x.OrderTotal).DefaultIfEmpty().Sum(),
                        CardAmount = payedOrders.Where(x => x.PaymentMethodSystemName == "Payments.CardOnDelivery").Select(x => x.OrderTotal).DefaultIfEmpty().Sum(),
                        PaypalAmount = payedOrders.Where(x => x.PaymentMethodSystemName == "Payments.PayPalStandard" || x.PaymentMethodSystemName == "Payments.PaypalPlus").Select(x => x.OrderTotal).DefaultIfEmpty().Sum(),
                        StripeAmount = payedOrders.Where(x => x.PaymentMethodSystemName == "Payments.Stripe").Select(x => x.OrderTotal).DefaultIfEmpty().Sum(),
                        QrMercadopagoAmount = payedOrders.Where(x => x.PaymentMethodSystemName == "Payments.MercadoPagoQr").Select(x => x.OrderTotal).DefaultIfEmpty().Sum(),
                        ReplacementAmount = payedOrders.Where(x => x.PaymentMethodSystemName == "Payments.Replacement").Select(x => x.OrderTotal).DefaultIfEmpty().Sum(),
                        BenefitsAmount = payedOrders.Where(x => x.PaymentMethodSystemName == "Payments.Benefits").Select(x => x.OrderTotal).DefaultIfEmpty().Sum(),
                        VisaAmount = payedOrders.Where(x => x.PaymentMethodSystemName == "Payments.Visa").Select(x => x.OrderTotal).DefaultIfEmpty().Sum(),
                        TransferAmount = payedOrders.Where(x => x.PaymentMethodSystemName == "Payments.ManualTransfer").Select(x => x.OrderTotal).DefaultIfEmpty().Sum(),
                        AssignedPedidos = OrderUtils.GetPedidosGroupByList(group.Where(x => x.RescueRouteDisplayOrder != -99).Select(x => x).ToList()).Count(),
                        RescuedPedidos = OrderUtils.GetPedidosGroupByList(currentRescuedOrders).Count(),
                        RescuedByPedidos = OrderUtils.GetPedidosGroupByList(filteredOrders.Where(x => x.RescueRouteDisplayOrder == -99).ToList()).Count(),
                        DeliveredPedidos = deliveredPedidos.Count(),
                        RescheduledPedidos = group.Where(x => rescheduledOrderIds.Contains(x.Id)).Count(),
                        AssignedProducts = filteredOrders.SelectMany(x => x.OrderItems).Count(),
                        DeliveredProducts = deliveredPedidos.SelectMany(x => x).SelectMany(x => x.OrderItems).Count(),
                        NotDeliveredPedidos = notDeliveredPedidos.Count()
                    };

                    tableData.TotalPayment = tableData.TransferAmount + tableData.CashAmount + tableData.CardAmount + tableData.PaypalAmount + tableData.StripeAmount + tableData.QrMercadopagoAmount + tableData.ReplacementAmount + tableData.BenefitsAmount + tableData.VisaAmount;
                    var activePedidos = tableData.AssignedPedidos + tableData.RescuedByPedidos - tableData.RescheduledPedidos - tableData.RescuedPedidos - tableData.NotDeliveredPedidos;
                    tableData.CompletionPercentage = activePedidos == 0 ? 0 : (tableData.DeliveredPedidos * 100) / activePedidos;
                    tableData.ProductPercentaje = tableData.AssignedProducts == 0 ? 0 : (tableData.DeliveredProducts * 100) / tableData.AssignedProducts;
                    tableListData.Add(tableData);
                }

                model.TableDataList = tableListData.OrderBy(x => x.RouteId).ToList();
                model.Routes.Insert(0, new AssignedRoute() { Value = "0", Text = "No asignado", CustomerIds = new List<int>() });

                ViewBag.OrderBy = orderBy;

                return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingRoute/MonitorRoutes.cshtml", model);
            }
            else
            {
                var orderList = orders.ToList();
                var model = new ShippingRouteOrderModel()
                {
                    Date = dateDelivery,
                    Orders = OrderUtils.GetPedidosGroupByList(orderList).ToList(),
                    Zones = _shippingZoneService.GetAll().ToList(),
                    Routes = _shippingRouteService.GetAll().ToList().Select(x => new AssignedRoute()
                    {
                        Text = x.RouteName,
                        AssignedVehicle = assignedVehicles.Where(y => y.RouteId == x.Id).FirstOrDefault()?.VehicleId == -1 ? "Camioneta rentada" : VehicleUtils.GetVehicleName(assignedVehicles.Where(y => y.RouteId == x.Id).FirstOrDefault()?.Vehicle),
                        Value = x.Id.ToString(),
                        //CapacityExceeded = IsCapacityExceeded(assignedVehicles.Where(y => y.RouteId == x.Id).FirstOrDefault()?.Vehicle, orderList, orderTypes)
                    }).ToList(),
                    AllowManufacturerListDownload = allowManufacturerListDownload
                };

                model.Routes.Insert(0, new AssignedRoute() { Value = "0", Text = "No asignado", CustomerIds = new List<int>() });

                ViewBag.OrderBy = orderBy;

                return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingRoute/AssignRouteOrder.cshtml", model);
            }
        }

        [HttpPost]
        [AuthorizeAdmin]
        public async Task<IActionResult> AutoAssignRoutes(AutoRouteAssignModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteAssign))
                return AccessDeniedView();

            var dateDelivery = DateTime.ParseExact(model.Date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var orders = GetFilteredOrders().Where(x => x.SelectedShippingDate == dateDelivery).ToList();
            var zones = _shippingZoneService.GetAll().ToList();
            var orderTypes = _orderTypeService.GetAll().ToList();

            // WE CHECK IF AN ORDER DOESN'T HAVE ZONE ID AND WE ASSIGN IT
            //AssignRoutesToOrders(orders.Where(x => !x.ZoneId.HasValue || x.ZoneId.Value == 0).ToList(), zones);
            foreach (var order in orders)
            {
                var zoneId = zones
                    .Where(x => (x.PostalCodes + "," + x.AdditionalPostalCodes)
                    .Contains(order.ShippingAddress.ZipPostalCode))
                    .Select(x => x.Id)
                    .FirstOrDefault();
                if (zoneId == 0) return BadRequest("No se fue posible asignar una zona a la orden " + order.CustomOrderNumber + $"(CP: {order.ShippingAddress.ZipPostalCode})");

                if (zoneId != order.ZoneId)
                {
                    order.ZoneId = zoneId;
                    order.OrderNotes.Add(new OrderNote()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = order.Id,
                        DisplayToCustomer = false,
                        Note = "Se asignó la zona con ID " + zoneId + " cuando se estaba asignando automáticamente las rutas."
                    });
                    _orderService.UpdateOrder(order);
                }
            }

            List<ShippingRoute> routes = _shippingRouteService.GetAll().Where(x => model.RouteIds.Contains(x.Id)).OrderBy(x => x.Id).ToList();

            // THE ORDERS MUST BE DISTRIBUTED BY TIME EQUALLY THROUGH EVERY ROUTE, SO WE GROUP THE ORDERS
            var groupedByTimeRaw = orders.GroupBy(x => x.SelectedShippingTime);

            var groupedByTime = groupedByTimeRaw.Select(x => new DistributionOrdersGroup()
            {
                SelectedShippingTime = x.Key,
                Orders = x.Select(y => new OrderElement()
                {
                    Order = y,
                    Volume = GetOrderVolume(y, orderTypes)
                }).OrderBy(y => y.Order.ZoneId)
                .ThenBy(y => y.Order.ShippingAddress.ZipPostalCode)
                .ThenBy(y => y.Order.ShippingAddress.Address2)
                .ToList()
            }).OrderBy(x => x.SelectedShippingTime).ToList();

            var distributedInRoutes = new List<DistributionByRoute>();
            foreach (var route in routes)
            {
                distributedInRoutes.Add(new DistributionByRoute()
                {
                    Orders = new List<OrderElement>(),
                    Route = route
                });
            }

            var routesCount = routes.Count;

            // WE ITERATE THROUGH EVERY GROUPED BY TIME ORDERS
            foreach (var orderByTime in groupedByTime)
            {
                var pendingToAssignOrders = orderByTime.Orders
                    .OrderBy(x => zones.Where(y => y.Id == x.Order.ZoneId).FirstOrDefault()?.ZoneName)
                    .ThenBy(x => x.Order.ShippingAddress.ZipPostalCode)
                    .ThenBy(x => x.Order.ShippingAddress.Address2)
                    .ToList();

                // WE GET HOW MANY ORDERS IN THIS TIME GROUP SHOULD EVERY ROUTE HAS
                List<OrdersPerRoute> ordersByRoute = CalculateOrdersPerRoute(routes, GetUniqueOrdersCount(pendingToAssignOrders.Select(x => x.Order).ToList()));

                // WE ITERATE THROUGH EVERY ROUTE
                RoutesLoop(pendingToAssignOrders, distributedInRoutes, ordersByRoute);

                // WE DO A SECOND ITERATION TO RECHECK PENDING ORDERS IF NEEDED
                if (pendingToAssignOrders.Count > 0)
                    RoutesLoop(pendingToAssignOrders, distributedInRoutes, ordersByRoute, true);

                if (pendingToAssignOrders.Count > 0)
                    return BadRequest("No fue posible realizar la asignación, las siguientes órdenes excedían la capacidad de carga de todas las rutas: " + string.Join(", ", pendingToAssignOrders.Select(x => x.Order.Id)));
            }

            // IN THIS POINT EVERY ORDER HAS ITS ROUTE AND ZONE
            int requestCount = 0;
            foreach (var group in distributedInRoutes)
            {
                // WE USE THE GOOGLE MAPS API TO ORDER THE ORDERS
                var orderedOrders = await OrderOrdersWithGoogle(group.Orders.Select(x => x.Order).ToList(), requestCount);
                for (int i = 0; i < orderedOrders.Count(); i++)
                {
                    orderedOrders[i].RouteId = group.Route.Id;
                    orderedOrders[i].RouteDisplayOrder = i + 1;
                    _orderService.UpdateOrder(orderedOrders[i]);
                }
            }

            return NoContent();
        }

        [HttpPost]
        [AuthorizeAdmin]
        public async Task<IActionResult> NewAutoAssignRoutes(AutoRouteAssignModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteAssign))
                return AccessDeniedView();

            var dateDelivery = DateTime.ParseExact(model.Date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var orders = GetFilteredOrders().Where(x => x.SelectedShippingDate == dateDelivery).ToList();
            var zones = _shippingZoneService.GetAll().ToList();
            var orderTypes = _orderTypeService.GetAll().ToList();

            // WE CHECK IF AN ORDER DOESN'T HAVE ZONE ID AND WE ASSIGN IT
            //AssignRoutesToOrders(orders.Where(x => !x.ZoneId.HasValue || x.ZoneId.Value == 0).ToList(), zones);
            foreach (var order in orders)
            {
                var zoneId = zones
                    .Where(x => (x.PostalCodes + "," + x.AdditionalPostalCodes)
                    .Contains(order.ShippingAddress.ZipPostalCode))
                    .Select(x => x.Id)
                    .FirstOrDefault();
                if (zoneId == 0) return BadRequest("No se fue posible asignar una zona a la orden " + order.CustomOrderNumber + $"(CP: {order.ShippingAddress.ZipPostalCode})");

                if (zoneId != order.ZoneId)
                {
                    order.ZoneId = zoneId;
                    order.OrderNotes.Add(new OrderNote()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = order.Id,
                        DisplayToCustomer = false,
                        Note = "Se asignó la zona con ID " + zoneId + " cuando se estaba asignando automáticamente las rutas."
                    });
                    _orderService.UpdateOrder(order);
                }
            }

            List<ShippingRoute> routes = _shippingRouteService.GetAll().Where(x => model.RouteIds.Contains(x.Id)).OrderBy(x => x.Id).ToList();

            // WE GET THE ORDERS WITH THEIR VOLUME
            List<OrderElement> parsedOrders = orders.Select(x => new OrderElement()
            {
                Order = x,
                Volume = GetOrderVolume(x, orderTypes)
            }).OrderBy(x => zones.Where(y => y.Id == x.Order.ZoneId).FirstOrDefault()?.ZoneName)
                .ThenBy(x => x.Order.ShippingAddress.ZipPostalCode)
                .ThenBy(x => x.Order.ShippingAddress.Address2)
                .ToList();

            var distributedInRoutes = new List<DistributionByRoute>();
            foreach (var route in routes)
            {
                distributedInRoutes.Add(new DistributionByRoute()
                {
                    Orders = new List<OrderElement>(),
                    Route = route
                });
            }

            var routesCount = routes.Count;

            var pendingToAssignOrders = parsedOrders;

            // WE GET HOW MANY ORDERS IN THIS TIME GROUP SHOULD EVERY ROUTE HAS
            List<OrdersPerRoute> ordersByRoute = CalculateOrdersPerRoute(routes, GetUniqueOrdersCount(pendingToAssignOrders.Select(x => x.Order).ToList()));

            // WE ITERATE THROUGH EVERY ROUTE
            RoutesLoop(pendingToAssignOrders, distributedInRoutes, ordersByRoute);

            // WE DO A SECOND ITERATION TO RECHECK PENDING ORDERS IF NEEDED
            if (pendingToAssignOrders.Count > 0)
                RoutesLoop(pendingToAssignOrders, distributedInRoutes, ordersByRoute, true);

            if (pendingToAssignOrders.Count > 0)
                return BadRequest("No fue posible realizar la asignación, las siguientes órdenes excedían la capacidad de carga de todas las rutas: " + string.Join(", ", pendingToAssignOrders.Select(x => x.Order.Id)));

            // IN THIS POINT EVERY ORDER HAS ITS ROUTE AND ZONE
            int requestCount = 0;
            foreach (var group in distributedInRoutes)
            {
                // WE USE THE GOOGLE MAPS API TO ORDER THE ORDERS
                var orderedOrders = await OrderOrdersWithGoogle(group.Orders.Select(x => x.Order).OrderBy(x => x.SelectedShippingTime).ToList(), requestCount);
                for (int i = 0; i < orderedOrders.Count(); i++)
                {
                    orderedOrders[i].RouteId = group.Route.Id;
                    orderedOrders[i].RouteDisplayOrder = i + 1;
                    _orderService.UpdateOrder(orderedOrders[i]);
                }
            }

            return NoContent();
        }

        [HttpPost]
        [AuthorizeAdmin]
        public async Task<IActionResult> OnlyOptimizeWithGoogle(AutoRouteAssignModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteAssign))
                return AccessDeniedView();

            DateTime dateDelivery = DateTime.ParseExact(model.Date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var ordersQuery = GetFilteredOrders().Where(x => x.SelectedShippingDate == dateDelivery);
            if (model.RouteIds.Count() > 0)
                ordersQuery = ordersQuery.Where(x => model.RouteIds.Contains(x.RouteId));

            var routesGroup = ordersQuery.GroupBy(x => x.RouteId).ToList();
            int requestCount = 0;
            foreach (var routeIdGroup in routesGroup)
            {
                // WE USE THE GOOGLE MAPS API TO ORDER THE ORDERS
                var orderedOrders = await OrderOrdersWithGoogle(routeIdGroup.OrderBy(x => x.SelectedShippingTime).ToList(), requestCount);
                for (int i = 0; i < orderedOrders.Count(); i++)
                {
                    orderedOrders[i].RouteDisplayOrder = i + 1;
                    _orderService.UpdateOrder(orderedOrders[i]);
                }
            }

            return NoContent();
        }

        [HttpPost]
        [AuthorizeAdmin]
        public async Task<IActionResult> OnlyOptimizeWithGoogle2(AutoRouteAssignModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteAssign))
                return AccessDeniedView();

            DateTime dateDelivery = DateTime.ParseExact(model.Date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var ordersQuery = GetFilteredOrders().Where(x => x.SelectedShippingDate == dateDelivery);
            if (model.RouteIds.Count() > 0)
                ordersQuery = ordersQuery.Where(x => model.RouteIds.Contains(x.RouteId));

            var routesGroup = ordersQuery.GroupBy(x => x.RouteId).ToList();
            int requestCount = 0;
            foreach (var routeIdGroup in routesGroup)
            {
                // WE USE THE GOOGLE MAPS API TO ORDER THE ORDERS
                if (routeIdGroup.Key == 0) continue;
                var orderedOrders = await OrderOrdersWithGoogle2(routeIdGroup.OrderBy(x => x.SelectedShippingTime).ToList(), requestCount);
                for (int i = 0; i < orderedOrders.Count(); i++)
                {
                    orderedOrders[i].RouteDisplayOrder = i + 1;
                    _orderService.UpdateOrder(orderedOrders[i]);
                }
            }

            return NoContent();
        }

        [HttpPost]
        [AuthorizeAdmin]
        public async Task<IActionResult> CalculateTimesWithGoogle(AutoRouteAssignModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteAssign))
                return AccessDeniedView();

            DateTime dateDelivery = DateTime.ParseExact(model.Date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var ordersQuery = GetFilteredOrders().Where(x => x.SelectedShippingDate == dateDelivery);
            if (model.RouteIds.Count() > 0)
                ordersQuery = ordersQuery.Where(x => model.RouteIds.Contains(x.RouteId));

            var routesGroup = ordersQuery.GroupBy(x => x.RouteId).ToList();
            var routesWithErrors = new List<int>();
            foreach (var routeIdGroup in routesGroup)
            {
                // WE USE THE GOOGLE MAPS API TO ORDER THE ORDERS
                var ordersWithTime = await CalculateTimesWithGoogle(routeIdGroup.OrderBy(x => x.RouteDisplayOrder).ToList());
                if (ordersWithTime == null)
                {
                    routesWithErrors.Add(routeIdGroup.Key);
                    continue;
                }

                for (int i = 0; i < ordersWithTime.Count(); i++)
                {
                    _orderService.UpdateOrder(ordersWithTime[i]);
                }
            }

            if (routesWithErrors.Count > 0)
                return BadRequest("Terminó el proceso, pero no se pudo calcular el tiempo para las rutas de los siguientes IDs: " + string.Join(", ", routesWithErrors));
            return NoContent();
        }

        [HttpGet]
        public IActionResult AjaxReassignRoutes()
        {
            var orders = _orderService.GetOrders()
                .Where(x => x.SelectedShippingDate.HasValue)
                .Where(x => x.OrderStatus != OrderStatus.Cancelled &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatus == Nop.Core.Domain.Payments.PaymentStatus.Pending))
                .ToList();
            var zones = _shippingZoneService.GetAll().ToList();

            AssignRoutesToOrders(orders, zones);

            return Ok();
        }

        public IActionResult AssignRoutesToOrders(List<Order> orders, List<ShippingZone> zones)
        {
            try
            {
                var count = 0;
                foreach (var order in orders)
                {
                    var zoneId = zones
                        .Where(x => (x.PostalCodes + "," + x.AdditionalPostalCodes)
                        .Contains(order.ShippingAddress.ZipPostalCode))
                        .Select(x => x.Id)
                        .FirstOrDefault();
                    if (zoneId == 0) return BadRequest("No se fue posible asignar una zona a la orden " + order.CustomOrderNumber + $"(CP: {order.ShippingAddress.ZipPostalCode})");

                    order.ZoneId = zoneId;
                    order.OrderNotes.Add(new OrderNote()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = order.Id,
                        DisplayToCustomer = false,
                        Note = "Se asignó la zona con ID " + zoneId + " cuando se estaba asignando automáticamente las rutas."
                    });
                    _orderService.UpdateOrder(order);
                    count++;
                }
                return Ok();
            }
            catch (Exception err)
            {
                return BadRequest(err);
            }
        }

        private int GetUniqueOrdersCount(List<Order> orders)
        {
            return OrderUtils.GetPedidosGroupByList(orders).Count();
        }

        private List<OrdersPerRoute> CalculateOrdersPerRoute(List<ShippingRoute> routes, int total)
        {
            var list = routes.Select(x => new OrdersPerRoute()
            {
                RouteId = x.Id,
                RoutesCount = 0
            }).ToList();

            int assigned = 0;
            int index = 0;
            while (assigned < total)
            {
                list[index].RoutesCount++;
                index++;
                assigned++;
                if (index == list.Count) index = 0;
            }

            return list;
        }

        private void RoutesLoop(List<OrderElement> pendingToAssignOrders,
            List<DistributionByRoute> distributedInRoutes, List<OrdersPerRoute> ordersByRoute,
            bool reCalculateOrdersPerRoute = false)
        {
            if (reCalculateOrdersPerRoute)
                ordersByRoute = CalculateOrdersPerRoute(distributedInRoutes.Select(x => x.Route).ToList(), GetUniqueOrdersCount(pendingToAssignOrders.Select(x => x.Order).ToList()));

            int routeIndex = 0;
            foreach (var route in distributedInRoutes)
            {
                // IF THERE IS NO MORE PENDING ORDERS TO ASSIGN, WE BREAK THE LOOP
                if (pendingToAssignOrders.Count == 0) break;
                int routeAllowedOrdersCount = ordersByRoute.Where(x => x.RouteId == route.Route.Id).FirstOrDefault().RoutesCount;
                int assignedCount = 0;
                bool skip = false;
                while (pendingToAssignOrders.Count > 0)
                {
                    var similarOrders = pendingToAssignOrders
                        .Where(x => ShippingRouteHelper.ParseOrderAddress(x.Order.ShippingAddress.Address1) ==
                        ShippingRouteHelper.ParseOrderAddress(pendingToAssignOrders.FirstOrDefault().Order.ShippingAddress.Address1)
                        ).ToList();

                    foreach (var order in similarOrders)
                    {
                        //if (!CanAssignOrder(route, pendingToAssignOrders.FirstOrDefault()))
                        //{
                        //    if (routeIndex + 1 < distributedInRoutes.Count)
                        //    {
                        //        ordersByRoute = CalculateOrdersPerRoute(distributedInRoutes.Skip(routeIndex + 1).Select(x => x.Route).ToList(), GetUniqueOrdersCount(pendingToAssignOrders.Select(x => x.Order).ToList()));
                        //    }
                        //    //if (pendingToAssignOrders.Count > 1) index++;
                        //    skip = true;
                        //    break;
                        //}
                        route.Orders.Add(order);
                        // IF A PENDING ORDER IS ASSIGNED, WE REMOVED IT FROM THE LIST
                        pendingToAssignOrders.RemoveAt(pendingToAssignOrders.IndexOf(order));
                    }
                    if (skip) break;
                    assignedCount++;
                    if (assignedCount >= routeAllowedOrdersCount)
                    {
                        break;
                    }
                }

                routeIndex++;
            }
        }

        private void RoutesLoop2(List<IGrouping<object, Order>> pendingToAssignPedidos,
            List<NewDistributionByRoute> distributedInRoutes, List<OrdersPerRoute> ordersByRoute,
            bool reCalculateOrdersPerRoute = false)
        {
            if (reCalculateOrdersPerRoute)
                ordersByRoute = CalculateOrdersPerRoute(distributedInRoutes.Select(x => x.Route).ToList(), pendingToAssignPedidos.Count);

            foreach (var route in distributedInRoutes)
            {
                // IF THERE IS NO MORE PENDING ORDERS TO ASSIGN, WE BREAK THE LOOP
                if (pendingToAssignPedidos.Count == 0) break;
                int routeAllowedOrdersCount = ordersByRoute.Where(x => x.RouteId == route.Route.Id).FirstOrDefault().RoutesCount;
                int assignedCount = 0;

                foreach (var pedido in pendingToAssignPedidos.ToList())
                {
                    foreach (var order in pedido)
                        route.Orders.Add(order);

                    // IF A PENDING ORDER IS ASSIGNED, WE REMOVED IT FROM THE LIST
                    pendingToAssignPedidos.RemoveAt(pendingToAssignPedidos.IndexOf(pedido));

                    assignedCount++;
                    if (assignedCount >= routeAllowedOrdersCount)
                        break;
                }
            }
        }

        private bool CanAssignOrder(DistributionByRoute currentRouteDistribution, OrderElement newOrder)
        {
            decimal hielera = currentRouteDistribution.Route.FridgeVolume;
            decimal manojos = currentRouteDistribution.Route.BunchVolume;
            var assignedOrdersVolume = currentRouteDistribution.Orders.Select(x => x.Volume).DefaultIfEmpty().Sum();
            return currentRouteDistribution.Route.LoadingCapacity > (assignedOrdersVolume + hielera + manojos + newOrder.Volume);
        }

        private bool IsCapacityExceeded(ShippingVehicle vehicle, List<Order> orders, List<OrderType> orderTypes)
        {
            if (vehicle == null) return false;
            decimal hielera = vehicle.FridgeVolume;
            decimal manojos = vehicle.BunchVolume;
            var volumes = orders.Select(x => GetOrderVolume(x, orderTypes)).Sum();
            return vehicle.LoadingCapacity < (volumes + hielera + manojos);
        }

        private decimal GetOrderVolume(Order order, List<OrderType> orderTypes)
        {
            int totalItems = order.OrderItems.Count;
            return orderTypes.Where(x => totalItems >= x.MinimumProductQty && totalItems <= x.MaxProductQty).Select(x => x.CargoSpace).FirstOrDefault();
        }

        private IQueryable<Order> GetFilteredOrders()
        {
            return _orderService.GetAllOrdersQuery()
                                .Where(x => x.OrderStatusId != 40 &&
                                    !(x.PaymentMethodSystemName == "Payments.PayPalStandard" &&
                                    x.PaymentStatusId == 10));
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult AssignRouteOrder(AssignRouteResultModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteAssign))
                return AccessDeniedView();

            DateTime? orderControlDate = null;
            foreach (var item in model.Result)
            {
                int[] orderIds = item.OrderIds.Split(',').Select(x => int.Parse(x)).ToArray();
                foreach (var orderId in orderIds)
                {
                    Order order = _orderService.GetOrderById(orderId);
                    if (order == null) continue;
                    orderControlDate = order.SelectedShippingDate;
                    if (order.RouteId != item.RouteId || order.RouteDisplayOrder != item.RouteDisplayOrder)
                    {
                        string message = string.Empty;
                        if (order.RouteId != item.RouteId)
                        {
                            message = $"El usuario {_workContext.CurrentCustomer.Email} le asignó a la orden la ruta con Id {item.RouteId}.";
                        }
                        if (order.RouteDisplayOrder != item.RouteDisplayOrder)
                        {
                            message += $"El usuario {_workContext.CurrentCustomer.Email} le asignó {item.RouteDisplayOrder} como orden para mostrar a la ruta de esta orden.";
                        }

                        order.PreviousPointTransferDistance = null;
                        order.PreviousPointTransferTime = null;

                        order.OrderNotes.Add(new OrderNote()
                        {
                            CreatedOnUtc = DateTime.UtcNow,
                            DisplayToCustomer = false,
                            Note = message,
                            CustomerId = _workContext.CurrentCustomer.Id
                        });
                        order.RouteId = item.RouteId;
                        order.RouteDisplayOrder = item.RouteDisplayOrder;
                        _orderService.UpdateOrder(order);
                    }
                }
            }

            if (orderControlDate.HasValue)
            {
                var routeIds = model.Result.GroupBy(x => x.RouteId).Select(x => x.Key).ToList();
                var ordersGroup = OrderUtils.GetFilteredOrders(_orderService)
                    .Where(x => x.SelectedShippingDate == orderControlDate && routeIds.Contains(x.RouteId))
                    .GroupBy(x => x.RouteId)
                    .ToList();
                foreach (var group in ordersGroup)
                {
                    var ordersWithTime = CalculateTimesWithGoogle(group.OrderBy(x => x.RouteDisplayOrder).ToList()).Result;
                    if (ordersWithTime == null)
                        continue;

                    for (int i = 0; i < ordersWithTime.Count(); i++)
                    {
                        _orderService.UpdateOrder(ordersWithTime[i]);
                    }
                }
            }

            var manufacturerListDownload = _manufacturerListDownloadService.GetAll().Where(x => x.OrderShippingDate == model.Date).FirstOrDefault();
            string status = model.AllowManufacturerListDownload ? "activó" : "desactivó";
            string log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) {status} la descarga de la lista de fabricantes.\n";
            if (manufacturerListDownload == null)
            {
                manufacturerListDownload = new ManufacturerListDownload()
                {
                    AllowDownload = model.AllowManufacturerListDownload,
                    OrderShippingDate = model.Date,
                    Log = log
                };
                _manufacturerListDownloadService.Insert(manufacturerListDownload);
            }
            else
            {
                manufacturerListDownload.AllowDownload = model.AllowManufacturerListDownload;
                manufacturerListDownload.Log += log;
                _manufacturerListDownloadService.Update(manufacturerListDownload);
            }

            return NoContent();
        }

        [AuthorizeAdmin]
        public IActionResult Create(int franchiseId = 0)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRoute))
                return AccessDeniedView();

            CreateViewModel model = new CreateViewModel();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingRoute/Create.cshtml", model);
        }

        [AuthorizeAdmin]
        public IActionResult Edit(int id, bool canEdit = true)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRoute))
                return AccessDeniedView();

            ShippingRoute shippingRoute = _shippingRouteService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (shippingRoute == null) return NotFound();

            EditViewModel model = new EditViewModel()
            {
                Id = shippingRoute.Id,
                RouteName = shippingRoute.RouteName,
                Active = shippingRoute.Active,
                ShippingRoutes = _shippingRouteService.GetAll().Where(x => x.Id != id).ToList(),
                CanEdit = canEdit
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingRoute/Edit.cshtml", model);
        }

        [AuthorizeAdmin]
        public IActionResult DeleteShippingRoute(int id, int newRouteId)
        {
            ShippingRoute shippingRoute = _shippingRouteService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (shippingRoute == null) return NotFound();

            var ordersInRoute = _orderService.GetAllOrdersQuery().Where(x => x.RouteId == id).ToList();
            foreach (var order in ordersInRoute)
            {
                order.RouteId = newRouteId;
                order.OrderNotes.Add(new OrderNote()
                {
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    Note = $"Se cambió la ruta de la orden por solicitud ya que el usuario {_workContext.CurrentCustomer.Email} eliminó la ruta {shippingRoute.RouteName} ({shippingRoute.Id})",
                    CustomerId = _workContext.CurrentCustomer.Id
                });
                _orderService.UpdateOrder(order);
            }
            _shippingRouteService.Delete(shippingRoute);

            return RedirectToAction("List");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetShippingRouteId(string pc)
        {
            return Ok(_shippingZoneService.GetAll().Where(x => (x.PostalCodes + x.AdditionalPostalCodes).Contains(pc)).Select(x => x.Id).FirstOrDefault());
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult Create(CreateViewModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRoute))
                return AccessDeniedView();

            if (!ModelState.IsValid) return BadRequest();

            ShippingRoute shippingRoute = new ShippingRoute()
            {
                CreatedByUserId = _workContext.CurrentCustomer.Id,
                RouteName = model.RouteName,
                Active = model.Active,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) creó la ruta."
            };
            _shippingRouteService.Insert(shippingRoute);

            var userRolesIds = _workContext.CurrentCustomer.CustomerRoles.Select(x => x.Id);
            var adminRole = _customerService.GetCustomerRoleBySystemName("Administrators");
            if (!userRolesIds.Contains(adminRole.Id) && shippingRoute.FranchiseId > 0)
            {
                return RedirectToAction("Edit", "Franchise", new { id = shippingRoute.FranchiseId });
            }


            return RedirectToAction("List");
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult Edit(EditViewModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRoute))
                return AccessDeniedView();

            if (!ModelState.IsValid) return BadRequest();

            ShippingRoute shippingRoute = _shippingRouteService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (shippingRoute == null) return NotFound();

            shippingRoute.RouteName = model.RouteName;
            shippingRoute.Active = model.Active;

            var newLog = string.Empty;

            if (model.RouteName != shippingRoute.RouteName)
            {
                newLog += $". Cambió el nombre de la ruta de {shippingRoute.RouteName} a {model.RouteName}";
            }

            shippingRoute.Log = shippingRoute.Log + "\n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) editó la ruta{newLog}";

            _shippingRouteService.Update(shippingRoute);

            return RedirectToAction("List");
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult ListData(DataSourceRequest command, int franchiseId = 0)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var query = _shippingRouteService.GetAll();
            if (franchiseId > 0)
            {
                query = query.Where(x => x.FranchiseId == franchiseId);
            }
            var queryList = query.OrderByDescending(m => m.CreatedOnUtc).ToList();
            var pagedList = new PagedList<ShippingRoute>(queryList, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    Active = x.Active ? "SI" : "NO",
                    x.RouteName,
                    x.Id
                }).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        [HttpGet]
        [AuthorizeAdmin]
        public IActionResult GetTakenPostalCodes(int routeId = 0)
        {
            return Ok(string.Join(",", _shippingRouteService.GetAll().Select(x => x.PostalCodes.Replace(" ", ""))).Split(','));
        }

        [HttpGet]
        [AuthorizeAdmin]
        public IActionResult GetResponsable(int routeId, string date)
        {
            DateTime shippingDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            int userId = _shippingRouteUserService.GetAll()
                .Where(x => x.ShippingRouteId == routeId)
                .ToList()
                .Where(x => x.ResponsableDateUtc.ToLocalTime().Date == shippingDate.Date)
                .Select(x => x.UserInChargeId)
                .FirstOrDefault();

            var model = new CustomerModel();
            var customer = _customerService.GetCustomerById(userId);
            if (customer != null)
            {
                model.Email = customer.Email;
                model.FullName = customer.GetFullName();
                model.Id = customer.Id;
            }
            else
            {
                model.Email = "";
                model.FullName = "";
                model.Id = 0;
            }

            return Ok(model);
        }

        [HttpPost]
        public IActionResult ListOrderData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var query = _orderService.GetAllOrdersQuery()
                                    .Where(x => x.OrderStatusId != 40 &&
                                    !(x.PaymentMethodSystemName == "Payments.PayPalStandard" &&
                                    x.PaymentStatusId == 10));
            var queryList = query.GroupBy(x => x.SelectedShippingDate).OrderByDescending(x => x.Key);
            var pagedList = GroupedPageList(queryList, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    Id = x.Key,
                    Date = x.Key.Value.ToString("dd-MM-yyyy"),
                    Pending = GetStatus(x.Select(y => y).ToList())
                }).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        private string GetStatus(List<Order> orders)
        {
            var filteredOrders = orders.Where(x => x.RouteId == 0);
            return filteredOrders.Count() == orders.Count ? "Ninguna asignada" : filteredOrders.Count() > 0 ? "Pendientes por asignar" : "Todas asignadas";
        }

        private List<IGrouping<DateTime?, Order>> GroupedPageList(IQueryable<IGrouping<DateTime?, Order>> source, int pageIndex, int pageSize)
        {
            List<IGrouping<DateTime?, Order>> filteredList = new List<IGrouping<DateTime?, Order>>();
            filteredList.AddRange(source.Skip(pageIndex * pageSize).Take(pageSize).ToList());
            return filteredList;
        }

        [HttpPost]
        public IActionResult UsersListData()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var users = _customerService.GetAllCustomersQuery().Where(x => x.Email != null).ToList().Where(x => x.GetCustomerRoleIds().Count() > 1);
            var elements = users.Select(x => new
            {
                Id = x.Id,
                User = x.GetFullName()
            }).ToList();

            return Json(elements);
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult UpdateOrderRoute(UpdateOrderRouteModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteUser) && !_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null) return NotFound();

            var route = _shippingRouteService.GetAll().Where(x => x.Id == model.RouteId).FirstOrDefault();
            if (route == null) return NotFound();

            order.RouteId = model.RouteId;
            order.OrderNotes.Add(new OrderNote
            {
                Note = $"{_workContext.CurrentCustomer.GetFullName()} cambió la ruta del envío a {route.RouteName} ({route.Id})",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = _workContext.CurrentCustomer.Id
            });
            _orderService.UpdateOrder(order);

            return NoContent();
        }

        [HttpGet]
        public IActionResult GetDeliveryStatusColor(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null) return Json(new
            {
                deliveryTime = $"<span class='grey'>No entregada</span>",
                deliveryDate = $"No entregada"
            });
            var deliveryDate = order.Shipments.Where(x => x.DeliveryDateUtc.HasValue).Any() ? order.Shipments.Where(x => x.DeliveryDateUtc.HasValue).Select(x => x.DeliveryDateUtc.Value).FirstOrDefault().ToLocalTime() : DateTime.MinValue;
            var selectedShippingDate = (order.SelectedShippingDate ?? DateTime.MinValue);
            var shippingTime = order.SelectedShippingTime;
            string deliveryStatusStyle = _shippingRouteService.GetDeliveryTimeColor(deliveryDate, selectedShippingDate, shippingTime);
            return Json(new
            {
                deliveryTime = $"<span class='grid-report-item {deliveryStatusStyle}'>{(deliveryDate == DateTime.MinValue ? "Pendiente" : deliveryDate.ToString("hh:mmtt"))}</span>",
                deliveryDate = $"{(deliveryDate == DateTime.MinValue ? "Pendiente" : deliveryDate.ToString("dd-MM-yyyy hh:mm tt"))}"
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetFranchiseAndRouteExternal()
        {
            if (_workContext.CurrentCustomer.IsAdmin())
                return BadRequest("No external requests allowed.");
            try
            {
                var query = _shippingVehicleRouteService.GetAll()
                    .Where(x => !x.Deleted);
                var franchises = _franchiseService.GetAll()
                    .ToList();
                var routes = _shippingRouteService.GetAll()
                    .Where(x => !x.Deleted).ToList();

                var franchiseAndRouteInfos = query
                    .ToList()
                    .Select(x => new
                    {
                        x.ShippingDate,
                        FranchiseId = x.Vehicle?.FranchiseId ?? 0,
                        x.RouteId,
                    })
                    .ToList();
                var info = new
                {
                    FranchiseInfos = franchises.Select(x => new
                    {
                        x.Id,
                        x.Name
                    }).ToList(),
                    RouteInfos = routes.Select(x => new
                    {
                        x.Id,
                        Name = x.RouteName
                    }).ToList(),
                    FranchiseAndRouteDatesInfos = franchiseAndRouteInfos
                };
                return Json(info);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetFranchiseAndRouteByOrderIdExternal(int id)
        {
            if (_workContext.CurrentCustomer.IsAdmin())
                return BadRequest("No external requests allowed.");
            try
            {
                var order = _orderService.GetOrderById(id);
                if (order == null)
                    return Ok(new
                    {
                        Id = 0,
                        RouteId = 0,
                        FranchiseId = 0,
                        RescuedByRouteId = 0,
                        RescuedByFranchiseId = 0,
                        SelectedShippingDate = (string)null,
                    });
                var regularShipping = _shippingVehicleRouteService.GetAll()
                    .Where(x => !x.Deleted &&
                    x.ShippingDate == order.SelectedShippingDate &&
                    x.RouteId == order.RouteId).FirstOrDefault();
                var rescudeShipping = _shippingVehicleRouteService.GetAll()
                    .Where(x => !x.Deleted &&
                    x.ShippingDate == order.SelectedShippingDate &&
                    x.RouteId == order.RescuedByRouteId).FirstOrDefault();
                var info = new
                {
                    order.Id,
                    RouteId = regularShipping?.RouteId ?? 0,
                    FranchiseId = regularShipping?.Vehicle?.FranchiseId ?? 0,
                    RescuedByRouteId = rescudeShipping?.RouteId ?? 0,
                    RescuedByFranchiseId = rescudeShipping?.Vehicle?.FranchiseId ?? 0,
                    SelectedShippingDate = order.SelectedShippingDate.Value.ToString("dd/MM/yyyy"),
                };
                return Json(info);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        private async Task<List<Order>> OrderOrdersWithGoogle2(List<Order> ordersToOrder, int requestCount)
        {
            var pedidos = OrderUtils.GetPedidosGroupByList(ordersToOrder).ToList();
            var orderShippingTimeGroup = pedidos.GroupBy(x => x.FirstOrDefault().SelectedShippingTime).ToList();
            if (orderShippingTimeGroup.Any())
            {
                foreach (var item in ordersToOrder.Where(x => string.IsNullOrEmpty(x.ShippingAddress.Latitude) || string.IsNullOrEmpty(x.ShippingAddress.Longitude)))
                {
                    string address = item.ShippingAddress.Address1.Replace("  ", " ").Replace(" ", "+").Replace(".", "").Replace(";", "");
                    string latLongUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={address}&key={GOOGLE_API_KEY}";
                    requestCount++;
                    using (HttpClient client = new HttpClient())
                    {
                        var result = await client.GetAsync(latLongUrl);
                        if (result.IsSuccessStatusCode)
                        {
                            var json = await result.Content.ReadAsStringAsync();
                            var objectResult = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
                            try
                            {
                                string lat = objectResult.results[0].geometry.location.lat;
                                string lng = objectResult.results[0].geometry.location.lng;

                                if (!string.IsNullOrEmpty(lat) && !string.IsNullOrEmpty(lng) && item.ShippingAddressId.HasValue)
                                {
                                    var dbAddress = _addressService.GetAddressById(item.ShippingAddressId.Value);
                                    dbAddress.Latitude = lat;
                                    dbAddress.Longitude = lng;
                                    item.ShippingAddress.Latitude = lat;
                                    item.ShippingAddress.Longitude = lng;
                                    _addressService.UpdateAddress(dbAddress);
                                    _orderService.UpdateOrder(item);
                                }
                            }
                            catch (Exception e)
                            {
                                //foreach (var customerAddress in item.Customer.Addresses.ToList())
                                //{
                                //    _addressService.DeleteAddress(customerAddress);
                                //}
                            }
                        }
                    }
                }

                if (ordersToOrder.Where(x => string.IsNullOrEmpty(x.ShippingAddress.Latitude) || string.IsNullOrEmpty(x.ShippingAddress.Longitude)).Count() > 0)
                    return ordersToOrder;

                string waypoints = "&waypoints=optimize:true";
                var initialPermutationResults = new List<PermutationResult>();
                var extendedPermutationResults = new List<List<PermutationResult>>();
                foreach (var timeGroup in orderShippingTimeGroup)
                {
                    int currentScheduleId = GetScheduleId(timeGroup.Key);

                    var initPoints = initialPermutationResults.Where(x => x.EndScheduleId == (currentScheduleId - 1)).ToList();
                    if (initPoints.Count() == 0 && currentScheduleId == 1)
                    {
                        // SI ES EL PRIMER CICLO EL PRIMER PUNTO ES LA AGENCIA
                        initPoints.Add(new PermutationResult()
                        {
                            EndLat = ORIGIN_COORDINATES.Split(',')[0],
                            EndLong = ORIGIN_COORDINATES.Split(',')[1],
                            EndScheduleId = 0,
                            TotalTime = 0,
                            InitOrderIds = "",
                            EndOrderIds = "AGENCIA"
                        });
                    }
                    else if (initPoints.Count() == 0)
                    {
                        var controlScheduleId = currentScheduleId - 1;
                        while (controlScheduleId >= 0 && initPoints.Count == 0)
                        {
                            controlScheduleId--;
                            initPoints = initialPermutationResults.Where(x => x.EndScheduleId == controlScheduleId).ToList();
                        }
                    }

                    var parsedInitPoints = initPoints.GroupBy(x => new { x.EndLat, x.EndLong, x.InitScheduleId, x.EndScheduleId }).Select(x => x.FirstOrDefault()).ToList();
                    foreach (var initPoint in parsedInitPoints)
                    {
                        foreach (var order in timeGroup)
                        {
                            var otherOrders = timeGroup.Select(x => x).Where(x => x.FirstOrDefault().Id != order.FirstOrDefault().Id);
                            var additionalWaypoints = string.Join("", otherOrders.Select(x => $"|{x.FirstOrDefault().ShippingAddress.Latitude},{x.FirstOrDefault().ShippingAddress.Longitude}"));

                            // Create string url
                            var url =
                            "https://maps.googleapis.com/maps/api/directions/json?" +
                            "origin=" + $"{initPoint.EndLat},{initPoint.EndLong}" +
                            "&destination=" +
                            $"{order.FirstOrDefault().ShippingAddress.Latitude},{order.FirstOrDefault().ShippingAddress.Longitude}{(!string.IsNullOrWhiteSpace(additionalWaypoints) ? $"{waypoints}{additionalWaypoints}" : "")}" +
                            "&avoidTolls=true&key=" + GOOGLE_API_KEY;

                            requestCount++;
                            using (HttpClient client = new HttpClient())
                            {
                                var result = await client.GetAsync(url);
                                string resultJson = await result.Content.ReadAsStringAsync();
                                if (result.IsSuccessStatusCode)
                                {
                                    var resultData = Newtonsoft.Json.JsonConvert.DeserializeObject<GoogleMapsDirections>(resultJson);
                                    var times = resultData.routes.SelectMany(x => x.legs).Select(x => x.duration.value);
                                    if (times.Count() == 0)
                                    {
                                        Debugger.Break();
                                    }
                                    initialPermutationResults.Add(new PermutationResult()
                                    {
                                        InitOrderIds = initPoint.EndOrderIds,
                                        EndOrderIds = string.Join(",", order.Select(x => x.Id)),
                                        TotalTime = times.DefaultIfEmpty().Sum(),
                                        InitLat = initPoint.EndLat,
                                        InitLong = initPoint.EndLong,
                                        EndLat = order.FirstOrDefault().ShippingAddress.Latitude,
                                        EndLong = order.FirstOrDefault().ShippingAddress.Longitude,
                                        InitScheduleId = initPoint.EndScheduleId,
                                        EndScheduleId = GetScheduleId(order.FirstOrDefault().SelectedShippingTime)
                                    });
                                }
                                else
                                {
                                    Debugger.Break();
                                }
                            }
                        }
                    }
                }

                //using (var file = System.IO.File.CreateText($"C:\\Users\\Cesar\\Downloads\\permutacion_inicial_{DateTime.Now:dd-MM-yyyy_hhmmss}.csv"))
                //{
                //    file.WriteLine("Id orden punto inicial,Id orden punto final,Latitud punto inicial,Longitud punto inicial,Latitud punto final,Longitud punto final,Id horario punto inicial,Id horario punto final,Tiempo total (segundos)");
                //    foreach (var result in initialPermutationResults)
                //    {
                //        file.WriteLine($"\"{result.InitOrderIds}\",\"{result.EndOrderIds}\",{result.InitLat},{result.InitLong},{result.EndLat},{result.EndLong},{result.InitScheduleId},{result.EndScheduleId},{result.TotalTime}");
                //    }
                //}

                foreach (var result in initialPermutationResults)
                {
                    if (result.InitScheduleId != 0)
                        continue;
                    string endLat1 = result.EndLat;
                    string endLong1 = result.EndLong;
                    var connectedDataListSchedule1 = initialPermutationResults.Where(x => x.InitLat == endLat1 && x.InitLong == endLong1 && x.InitScheduleId == result.EndScheduleId).ToList();
                    if (connectedDataListSchedule1 == null || connectedDataListSchedule1.Count == 0)
                    {
                        connectedDataListSchedule1.Add(new PermutationResult()
                        {
                            EndLat = "NO APLICA",
                            EndLong = "NO APLICA",
                            EndOrderIds = string.Empty,
                            EndScheduleId = 2,
                            InitLat = endLat1,
                            InitLong = endLong1,
                            InitOrderIds = result.EndOrderIds,
                            InitScheduleId = 1,
                            TotalTime = 0
                        });
                    }

                    foreach (var connectedData1 in connectedDataListSchedule1)
                    {
                        string endLat2 = connectedData1.EndLat;
                        string endLong2 = connectedData1.EndLong;
                        var connectedDataListSchedule2 = initialPermutationResults.Where(x => x.InitLat == endLat2 && x.InitLong == endLong2 && x.InitScheduleId == connectedData1.EndScheduleId).ToList();
                        if (connectedDataListSchedule2 == null || connectedDataListSchedule2.Count == 0)
                        {
                            connectedDataListSchedule2.Add(new PermutationResult()
                            {
                                EndLat = "NO APLICA",
                                EndLong = "NO APLICA",
                                EndOrderIds = string.Empty,
                                EndScheduleId = 3,
                                InitLat = endLat2,
                                InitLong = endLong2,
                                InitOrderIds = connectedData1.EndOrderIds,
                                InitScheduleId = 2,
                                TotalTime = 0
                            });
                        }

                        foreach (var connectedData2 in connectedDataListSchedule2)
                        {
                            string endLat3 = connectedData2.EndLat;
                            string endLong3 = connectedData2.EndLong;
                            var connectedDataListSchedule3 = initialPermutationResults.Where(x => x.InitLat == endLat3 && x.InitLong == endLong3 && x.InitScheduleId == connectedData2.EndScheduleId).ToList();
                            if (connectedDataListSchedule3 == null || connectedDataListSchedule3.Count == 0)
                            {
                                connectedDataListSchedule3.Add(new PermutationResult()
                                {
                                    EndLat = "NO APLICA",
                                    EndLong = "NO APLICA",
                                    EndOrderIds = string.Empty,
                                    EndScheduleId = 4,
                                    InitLat = endLat3,
                                    InitLong = endLong3,
                                    InitOrderIds = connectedData2.EndOrderIds,
                                    InitScheduleId = 3,
                                    TotalTime = 0
                                });
                            }

                            foreach (var connectedData3 in connectedDataListSchedule3)
                            {
                                var resultObject = new List<PermutationResult>() { result, connectedData1, connectedData2, connectedData3 };
                                if (extendedPermutationResults.Any(x => x == resultObject))
                                    continue;
                                extendedPermutationResults.Add(resultObject);
                            }
                        }
                    }
                }

                //var orderedData = extendedPermutationResults.OrderBy(x => x.Select(y => y.TotalTime).DefaultIfEmpty().Sum()).ToList();
                var orderedData = extendedPermutationResults
                    .OrderByDescending(x => !x.Where(y => string.IsNullOrWhiteSpace(y.EndOrderIds)).Any())
                    .ThenBy(x => x.Select(y => y.TotalTime).DefaultIfEmpty().Sum())
                    .ToList();

                //using (var file = System.IO.File.CreateText($"C:\\Users\\Cesar\\Downloads\\permutacion_extendida_{DateTime.Now:dd-MM-yyyy_hhmmss}.csv"))
                //{
                //    file.WriteLine("Id orden punto inicial,Id orden punto final,Latitud punto inicial,Longitud punto inicial,Latitud punto final,Longitud punto final,Id horario punto inicial,Id horario punto final,Tiempo total (segundos)");
                //    foreach (var extentedResult in orderedData)
                //    {
                //        foreach (var result in extentedResult)
                //        {
                //            file.WriteLine($"\"{result.InitOrderIds}\",\"{result.EndOrderIds}\",{result.InitLat},{result.InitLong},{result.EndLat},{result.EndLong},{result.InitScheduleId},{result.EndScheduleId},{result.TotalTime}");
                //        }
                //        file.WriteLine($",,,,,,,,{extentedResult.Select(x => x.TotalTime).DefaultIfEmpty().Sum()}");
                //    }
                //}

                List<PermutationResult> fastestPermutation = orderedData.FirstOrDefault();
                List<Order> orderedOrders = new List<Order>();
                if (fastestPermutation == null) return ordersToOrder;
                foreach (var item in fastestPermutation)
                {
                    if (string.IsNullOrWhiteSpace(item.EndOrderIds)) continue;
                    int endPointOrderId = item.EndOrderIds.Split(',').Select(x => int.Parse(x)).FirstOrDefault();
                    var filteredTimeGroupOrders = ordersToOrder
                        .Where(x => GetScheduleId(x.SelectedShippingTime) == item.EndScheduleId && x.Id != endPointOrderId)
                        .ToList();
                    var additionalWaypoints = string.Join("", filteredTimeGroupOrders.Select(x => $"|{x.ShippingAddress.Latitude},{x.ShippingAddress.Longitude}"));
                    var url = "https://maps.googleapis.com/maps/api/directions/json?" +
                            "origin=" + $"{item.InitLat},{item.InitLong}" +
                            "&destination=" +
                            $"{item.EndLat},{item.EndLong}{(!string.IsNullOrWhiteSpace(additionalWaypoints) ? $"{waypoints}{additionalWaypoints}" : "")}" +
                            "&avoidTolls=true&key=" + GOOGLE_API_KEY;
                    requestCount++;
                    using (HttpClient client = new HttpClient())
                    {
                        var result = await client.GetAsync(url);
                        string resultJson = await result.Content.ReadAsStringAsync();
                        if (result.IsSuccessStatusCode)
                        {
                            var resultData = Newtonsoft.Json.JsonConvert.DeserializeObject<GoogleMapsDirections>(resultJson);
                            for (int i = 0; i < resultData.routes.FirstOrDefault().waypoint_order.Count; i++)
                            {
                                if (filteredTimeGroupOrders.Count == 0) continue;
                                int index = resultData.routes.FirstOrDefault().waypoint_order[i];
                                Order elementOrder = filteredTimeGroupOrders[index];
                                var pointData = resultData.routes.FirstOrDefault().legs[i];
                                elementOrder.PreviousPointTransferTime = pointData.duration.value;
                                elementOrder.PreviousPointTransferDistance = pointData.distance.value;

                                orderedOrders.Add(elementOrder);
                            }

                            Order lastOrder = ordersToOrder.Where(x => x.Id == endPointOrderId).FirstOrDefault();
                            lastOrder.PreviousPointTransferDistance = resultData.routes.FirstOrDefault().legs.LastOrDefault().distance.value;
                            lastOrder.PreviousPointTransferTime = resultData.routes.FirstOrDefault().legs.LastOrDefault().duration.value;
                            orderedOrders.Add(lastOrder);
                        }
                    }
                }

                return orderedOrders;
            }

            return ordersToOrder;
        }

        private async Task<List<Order>> OrderOrdersWithGoogle(List<Order> ordersToOrder, int requestCount)
        {
            var orders = ordersToOrder.GroupBy(x => x.SelectedShippingTime).ToList();
            GoogleMapsDirections newOrder = new GoogleMapsDirections();
            List<Order> waypointOrdes = new List<Order>();

            if (orders.Any())
            {
                //double totalTime = 0.00;

                foreach (var item in ordersToOrder.Where(x => string.IsNullOrEmpty(x.ShippingAddress.Latitude) || string.IsNullOrEmpty(x.ShippingAddress.Latitude)))
                {
                    string address = item.ShippingAddress.Address1.Replace("  ", " ").Replace(" ", "+").Replace(".", "").Replace(";", "");
                    string latLongUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={address}&key={GOOGLE_API_KEY}";
                    using (HttpClient client = new HttpClient())
                    {
                        requestCount++;
                        var result = await client.GetAsync(latLongUrl);
                        if (result.IsSuccessStatusCode)
                        {
                            var json = await result.Content.ReadAsStringAsync();
                            var objectResult = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
                            try
                            {
                                string lat = objectResult.results[0].geometry.location.lat;
                                string lng = objectResult.results[0].geometry.location.lng;

                                if (!string.IsNullOrEmpty(lat) && !string.IsNullOrEmpty(lng) && item.ShippingAddressId.HasValue)
                                {
                                    var dbAddress = _addressService.GetAddressById(item.ShippingAddressId.Value);
                                    dbAddress.Latitude = lat;
                                    dbAddress.Longitude = lng;
                                    item.ShippingAddress.Latitude = lat;
                                    item.ShippingAddress.Longitude = lng;
                                    _addressService.UpdateAddress(dbAddress);
                                    _orderService.UpdateOrder(item);
                                }
                            }
                            catch (Exception e)
                            {
                                //foreach (var customerAddress in item.Customer.Addresses.ToList())
                                //{
                                //    _addressService.DeleteAddress(customerAddress);
                                //}
                            }
                        }
                    }
                }

                if (ordersToOrder.Where(x => string.IsNullOrEmpty(x.ShippingAddress.Latitude) || string.IsNullOrEmpty(x.ShippingAddress.Latitude)).Count() > 0)
                    return ordersToOrder;


                foreach (var orderGrouping in orders)
                {
                    List<List<Order>> BestTimeOrders = new List<List<Order>>();
                    List<int> BestTimeInts = new List<int>();
                    var waypoints = "&waypoints=optimize:true";

                    // Get string for google maps (waypoints)
                    foreach (var order in orderGrouping)
                    {
                        waypoints += $"|{order.ShippingAddress.Latitude},{order.ShippingAddress.Longitude}";
                    }

                    foreach (var order in orderGrouping)
                    {
                        // Create string url
                        var url =
                        "https://maps.googleapis.com/maps/api/directions/json?" +
                        "origin=" +
                        (waypointOrdes.Any() ? $"{waypointOrdes.LastOrDefault().ShippingAddress.Latitude},{waypointOrdes.LastOrDefault().ShippingAddress.Longitude}" : ORIGIN_COORDINATES) + //AGENCIA
                        "&destination=" +
                        $"{order.ShippingAddress.Latitude},{order.ShippingAddress.Longitude}{waypoints}" +
                        "&avoidTolls=true&key=" + GOOGLE_API_KEY;

                        using (HttpClient client = new HttpClient())
                        {
                            requestCount++;
                            var result = await client.GetAsync(url);
                            if (result.IsSuccessStatusCode)
                            {
                                try
                                {
                                    // Create temp lists for orders and times
                                    var tempOrders = new List<Order>();
                                    var main = await result.Content.ReadAsStringAsync();
                                    newOrder = JsonConvert.DeserializeObject<GoogleMapsDirections>(main);

                                    BestTimeInts.Add(newOrder.routes.FirstOrDefault().legs.Sum(x => x.duration.value));

                                    foreach (var waypoint in newOrder.routes.FirstOrDefault().waypoint_order.ToList())
                                    {
                                        var elementOrder = orderGrouping.Select(x => x).ToList()[waypoint];
                                        var pointData = newOrder.routes.FirstOrDefault().legs[waypoint];
                                        elementOrder.PreviousPointTransferTime = pointData.duration.value;
                                        elementOrder.PreviousPointTransferDistance = pointData.distance.value;

                                        tempOrders.Add(elementOrder);
                                    }
                                    BestTimeOrders.Add(tempOrders);

                                    //var positionings = newOrder.routes.FirstOrDefault().legs.ToList();
                                }
                                catch (Exception e)
                                {
                                    var err = e;
                                    return ordersToOrder;
                                }
                            }
                        }
                    }

                    // Sum best time and save best time order list
                    //totalTime += BestTimeInts[IndexOfMin(BestTimeInts)];
                    foreach (var order in BestTimeOrders[IndexOfMin(BestTimeInts)])
                    {
                        waypointOrdes.Add(order);
                    }
                }
            }

            return waypointOrdes;
        }

        private async Task<List<Order>> CalculateTimesWithGoogle(List<Order> orders)
        {
            if (orders.Any())
            {
                for (int i = 0; i < orders.Count; i++)
                {
                    string origin = i == 0 ? ORIGIN_COORDINATES : $"{orders[i - 1].ShippingAddress.Latitude},{orders[i - 1].ShippingAddress.Longitude}";
                    string destination = $"{orders[i].ShippingAddress.Latitude},{orders[i].ShippingAddress.Longitude}";
                    string url =
                    "https://maps.googleapis.com/maps/api/directions/json?" +
                    "origin=" + origin +
                    "&destination=" + destination +
                    "&avoidTolls=true&key=" + GOOGLE_API_KEY;

                    using (HttpClient client = new HttpClient())
                    {
                        var result = await client.GetAsync(url);
                        if (result.IsSuccessStatusCode)
                        {
                            try
                            {
                                string resultJson = await result.Content.ReadAsStringAsync();
                                var resultObject = JsonConvert.DeserializeObject<GoogleMapsDirections>(resultJson);
                                orders[i].PreviousPointTransferTime = resultObject?.routes?.FirstOrDefault()?.legs?.FirstOrDefault()?.duration?.value;
                                orders[i].PreviousPointTransferDistance = resultObject?.routes?.FirstOrDefault()?.legs?.FirstOrDefault()?.distance?.value;
                            }
                            catch (Exception e)
                            {
                                var err = e;
                                return null;
                            }
                        }
                        else return null;
                    }
                }
                return orders;
            }

            return null;
        }

        private int GetScheduleId(string selectedShippingTime)
        {
            switch (selectedShippingTime)
            {
                case "1:00 PM - 3:00 PM":
                    return 1;
                case "3:00 PM - 5:00 PM":
                    return 2;
                case "5:00 PM - 7:00 PM":
                    return 3;
                case "7:00 PM - 9:00 PM":
                    return 4;
                default:
                    return 0;
            }
        }

        private int IndexOfMin(IList<int> self)
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            if (self.Count == 0)
            {
                throw new ArgumentException("List is empty.", "self");
            }

            int min = self[0];
            int minIndex = 0;

            for (int i = 1; i < self.Count; ++i)
            {
                if (self[i] < min)
                {
                    min = self[i];
                    minIndex = i;
                }
            }

            return minIndex;
        }
    }

    #region Google waypoints

    public class OrderedWaypoints
    {
        public List<Order> Orders { get; set; }
        public int TotalTimeInSeconds { get; set; }
    }

    public class GoogleMapsDirections
    {
        public List<routes> routes { get; set; }
    }

    public class routes
    {
        public List<legs> legs { get; set; }
        public List<int> waypoint_order { get; set; }
    }

    public class legs
    {
        public duration duration { get; set; }
        public distance distance { get; set; }
        public string end_address { get; set; }
        public end_location end_location { get; set; }
        public end_location start_location { get; set; }
    }

    public class end_location
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class duration
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    public class distance
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    #endregion

    public class ShippingRouteOrderModel
    {
        public DateTime Date { get; set; }
        public List<AssignedRoute> Routes { get; set; }
        public List<ShippingZone> Zones { get; set; }
        public List<IGrouping<dynamic, Order>> Orders { get; set; }
        public List<AssignCustomer> Customers { get; set; }
        public List<RescheduledOrderLog> RescheduledOrderLogs { get; set; }
        public int FranchiseId { get; set; }
        public bool AllowManufacturerListDownload { get; set; }
    }

    public class ShippingRouteOrderModelForMonitor
    {
        public DateTime Date { get; set; }
        public List<AssignedRoute> Routes { get; set; }
        public List<ShippingZone> Zones { get; set; }
        public List<Order> Orders { get; set; }
        public List<AssignCustomer> Customers { get; set; }
        public List<RescheduledOrderLog> RescheduledOrderLogs { get; set; }
        public int FranchiseId { get; set; }
        public List<ShippingRouteMonitorTableData> TableDataList { get; set; }
    }

    public class ShippingRouteMonitorTableData
    {
        public string RouteName { get; set; }
        public int RouteId { get; set; }
        public decimal CashAmount { get; set; }
        public decimal CardAmount { get; set; }
        public decimal PaypalAmount { get; set; }
        public decimal StripeAmount { get; set; }
        public decimal QrMercadopagoAmount { get; set; }
        public decimal ReplacementAmount { get; set; }
        public decimal BenefitsAmount { get; set; }
        public decimal VisaAmount { get; set; }
        public decimal TransferAmount { get; set; }
        public decimal TotalPayment { get; set; }
        public int AssignedPedidos { get; set; }
        public int RescuedPedidos { get; set; }
        public int RescuedByPedidos { get; set; }
        public int RescheduledPedidos { get; set; }
        public int DeliveredPedidos { get; set; }
        public int NotDeliveredPedidos { get; set; }
        public decimal CompletionPercentage { get; set; }
        public int AssignedProducts { get; set; }
        public int DeliveredProducts { get; set; }
        public decimal ProductPercentaje { get; set; }
    }

    public class AssignedRoute
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public string AssignedVehicle { get; set; }
        public string Franchise { get; set; }
        public bool CapacityExceeded { get; set; }
        public List<int> CustomerIds { get; set; }
    }

    public class AssignCustomer
    {
        public string Text { get; set; }
        public int Value { get; set; }
    }

    public class AssignRouteResultModel
    {
        public DateTime Date { get; set; }
        public List<AssignRouteResultData> Result { get; set; }
        public bool AllowManufacturerListDownload { get; set; }
    }

    public class AssignRouteResultData
    {
        public string OrderIds { get; set; }
        public int RouteId { get; set; }
        public int RouteDisplayOrder { get; set; }
    }

    public class DistributionByRoute
    {
        public ShippingRoute Route { get; set; }
        public List<OrderElement> Orders { get; set; }
    }

    public class NewDistributionByRoute
    {
        public ShippingRoute Route { get; set; }
        public List<Order> Orders { get; set; }
    }

    public class DistributionOrdersGroup
    {
        public string SelectedShippingTime { get; set; }
        public List<OrderElement> Orders { get; set; }
    }

    public class OrderElement
    {
        public decimal Volume { get; set; }
        public Order Order { get; set; }
    }

    public class AutoRouteAssignModel
    {
        public string Date { get; set; }
        public int[] RouteIds { get; set; }
    }

    public class OrdersPerRoute
    {
        public int RouteId { get; set; }
        public int RoutesCount { get; set; }
    }

    public class UpdateOrdersFromMapModel
    {
        public List<UpdateOrdersFromMapData> Data { get; set; }
        public List<OrderOptimizationRequestModel> OptimizationData { get; set; }
        public string Date { get; set; }
    }

    public class UpdateOrdersFromMapData
    {
        public string OrderIds { get; set; }
        public int NewRouteId { get; set; }
        public string OrderNames { get; set; }
        public string OriginalRouteName { get; set; }
        public string NewRouteName { get; set; }
    }

    public class PermutationResult
    {
        public string InitOrderIds { get; set; }
        public string EndOrderIds { get; set; }
        public string InitLat { get; set; }
        public string InitLong { get; set; }
        public string EndLat { get; set; }
        public string EndLong { get; set; }
        public int InitScheduleId { get; set; }
        public int EndScheduleId { get; set; }
        public int TotalTime { get; set; }
    }

    public static class ShippingRouteHelper
    {
        public static string ParseOrderAddress(string address)
        {
            return address.Substring(0, (
                            address.ToLower().Contains("int") ?
                            address.ToLower().LastIndexOf("int") :
                            address.ToLower().Contains("no.") ?
                            address.ToLower().LastIndexOf("no.") :
                            address.ToLower().Contains("depto") ?
                            address.ToLower().LastIndexOf("depto") :
                            address.ToLower().Contains("departamento") ?
                            address.ToLower().LastIndexOf("departamento") :
                            address.Length)).ToLower();
        }
    }

    public class FranchiseAndRouteDatesRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
