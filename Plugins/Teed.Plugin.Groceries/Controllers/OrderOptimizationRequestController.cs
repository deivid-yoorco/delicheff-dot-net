using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;
using Teed.Plugin.Groceries.Models.ShippingRoute;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class OrderOptimizationRequestController : BasePluginController
    {
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

        public OrderOptimizationRequestController(IPermissionService permissionService, ShippingRouteService shippingRouteService,
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

        public IActionResult Index(string date = null)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OptimizationRequest) && !_permissionService.Authorize(TeedGroceriesPermissionProvider.OptimizationRequestManager) && !_permissionService.Authorize(TeedGroceriesPermissionProvider.OptimizationRequestManager))
                return AccessDeniedView();

            var modelDate = DateTime.Now.Date;
            if (!string.IsNullOrEmpty(date))
                modelDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate == modelDate).ToList();
            var orderIds = orders.Select(x => x.Id).ToList();
            var optimizations = _orderOptimizationRequestService.GetAll().Where(x => orderIds.Contains(x.OrderId)).ToList();
            var optimizationOrderIds = optimizations.Select(x => x.OrderId).ToList();
            var allPedidos = OrderUtils.GetPedidosGroupByList(orders).ToList();
            var pedidos = OrderUtils.GetPedidosGroupByList(orders.Where(x => optimizationOrderIds.Contains(x.Id)).ToList()).ToList();
            var routes = _shippingRouteService.GetAll().ToList();

            var alreadyAddedOrderIds = new List<int>();
            var optimizationOrderData = new List<OptimizationOrderData>();
            foreach (var optimization in optimizations)
            {
                if (alreadyAddedOrderIds.Where(x => x == optimization.OrderId).Any()) continue;
                var ordersInPedido = pedidos.Where(a => a.Where(b => b.Id == optimization.OrderId).Any()).FirstOrDefault();
                Address shippingAddress = ordersInPedido.FirstOrDefault().ShippingAddress;
                var route = routes.Where(x => x.Id == ordersInPedido.FirstOrDefault().RouteId).FirstOrDefault();
                optimizationOrderData.Add(new OptimizationOrderData()
                {
                    CustomerId = ordersInPedido.FirstOrDefault().CustomerId,
                    CustomerName = shippingAddress.FirstName + " " + shippingAddress.LastName,
                    Address = shippingAddress.Address1,
                    OriginalTime = optimization.OriginalSelectedTime,
                    PhoneNumber = shippingAddress.PhoneNumber,
                    TimeOption1 = optimization.TimeOption1,
                    TimeOption2 = optimization.TimeOption2,
                    TimeOption3 = optimization.TimeOption3,
                    OrderIds = string.Join("-", ordersInPedido.Select(x => x.Id).ToList()),
                    OrderNames = string.Join(", ", ordersInPedido.Select(x => $"<a href=\"/Admin/Order/Edit/{x.Id}\" target=\"_blank\">#{x.CustomOrderNumber}</a>").ToList()),
                    OptimizationTypeId = optimization.SelectedOptimizationTypeId,
                    RouteName = route == null ? "Sin ruta asignada" : route.RouteName,
                    RouteId = route == null ? 0 : route.Id,
                    CurrentStatusId = optimization.CurrentStatusId,
                    FinalTimeSelected = optimization.FinalTimeSelected,
                    CurrentTime = ordersInPedido.FirstOrDefault().SelectedShippingTime,
                    Comments = optimization.Comments,
                    ManagerComment = optimization.ManagerComment
                });
                alreadyAddedOrderIds.AddRange(ordersInPedido.Select(x => x.Id).ToList());
            }

            var model = new OptimizationRequestDataModel()
            {
                Date = modelDate,
                CurrentOrdersData = allPedidos.Select(x => new CurrentOrderData()
                {
                    SelectedShippingTime = x.Key.SelectedShippingTime,
                    RouteId = x.Select(y => y.RouteId).FirstOrDefault()
                }).ToList()
            };

            var routeIdsAlreadyCreated = optimizationOrderData.Select(x => x.RouteId).Distinct().ToList();
            var ordersByRoute = orders
                .Where(x => !routeIdsAlreadyCreated.Contains(x.RouteId))
                .OrderBy(x => x.RouteId)
                .GroupBy(x => x.RouteId).ToList();
            foreach (var grouping in ordersByRoute)
            {
                var route = routes.Where(x => x.Id == grouping.Key).FirstOrDefault();
                optimizationOrderData.Add(new OptimizationOrderData()
                {
                    CustomerId = 0,
                    CustomerName = string.Empty,
                    Address = string.Empty,
                    OriginalTime = string.Empty,
                    PhoneNumber = string.Empty,
                    TimeOption1 = string.Empty,
                    TimeOption2 = string.Empty,
                    TimeOption3 = string.Empty,
                    OrderIds = string.Empty,
                    OrderNames = string.Empty,
                    OptimizationTypeId = (int)OrderOptimizationTypes.Inmediate,
                    RouteName = route == null ? "Sin ruta asignada" : route.RouteName,
                    RouteId = route == null ? 0 : route.Id,
                    CurrentStatusId = 0,
                    FinalTimeSelected = "",
                    CurrentTime = string.Empty,
                    Comments = string.Empty,
                    EmptyRoute = true
                });

                //model.CurrentOrdersData.AddRange(grouping.Select(x => new CurrentOrderData
                //{
                //    RouteId = route == null ? 0 : route.Id,
                //    SelectedShippingTime = x.SelectedShippingTime
                //}));
            }

            model.Data = optimizationOrderData.GroupBy(x => x.OptimizationTypeId).Select(x => new OptimizationData()
            {
                OptimizationTypeId = x.Key,
                OptimizationTypeName = ((OrderOptimizationTypes)x.Key).GetDisplayName(),
                OptimizationRequest = x.ToList()
            }).ToList();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/OptimizationRequest/Index.cshtml", model);
        }

        public IActionResult UpdateOptimizationRequest(UpdateOptimizationModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OptimizationRequest) && !_permissionService.Authorize(TeedGroceriesPermissionProvider.OptimizationRequestManager))
                return Unauthorized();

            var selectedOrderIds = model.Data.Select(x => x.OrderIds).Select(x => x.Split('-')).SelectMany(x => x).Select(x => int.Parse(x)).ToList();
            var optimizations = _orderOptimizationRequestService.GetAll().Where(x => selectedOrderIds.Contains(x.OrderId)).ToList();
            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => selectedOrderIds.Contains(x.Id)).ToList();

            foreach (var data in model.Data)
            {
                var orderIds = data.OrderIds.Split('-').Select(x => int.Parse(x)).ToList();
                foreach (var orderId in orderIds)
                {
                    OrderOptimizationRequest optimization = optimizations.Where(x => x.OrderId == orderId).FirstOrDefault();
                    if (optimization == null) continue;
                    optimization.CurrentStatusId = data.SelectedStatusId;
                    optimization.FinalTimeSelected = data.SelectedTime;
                    optimization.Comments = data.Comments;
                    _orderOptimizationRequestService.Update(optimization);

                    if ((OrderOptimizationStatus)optimization.CurrentStatusId == OrderOptimizationStatus.Authorized &&
                        (optimization.SelectedOptimizationTypeId == (int)OrderOptimizationTypes.Inmediate || optimization.SelectedOptimizationTypeId == (int)OrderOptimizationTypes.RequestedByClient) &&
                        !string.IsNullOrWhiteSpace(data.SelectedTime))
                    {
                        var order = orders.Where(x => x.Id == orderId).FirstOrDefault();
                        if (order == null) continue;
                        if (order.SelectedShippingTime == data.SelectedTime) continue;
                        string log = $"El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) autorizó la solicitud de optimización de horario. El horario de la orden cambió de {order.SelectedShippingTime} a {data.SelectedTime}.";
                        order.SelectedShippingTime = data.SelectedTime;
                        order.OrderNotes.Add(new OrderNote()
                        {
                            CreatedOnUtc = DateTime.UtcNow,
                            OrderId = order.Id,
                            DisplayToCustomer = false,
                            Note = log,
                            CustomerId = _workContext.CurrentCustomer.Id
                        });
                        _orderService.UpdateOrder(order);
                    }
                }
            }

            return NoContent();
        }

        public IActionResult DeleteOptimization(string orderids, string date)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OptimizationRequest) && !_permissionService.Authorize(TeedGroceriesPermissionProvider.OptimizationRequestManager))
                return Unauthorized();

            var parsedOrderIds = orderids.Split('-').SelectMany(x => x.Split(',')).Select(x => int.Parse(x)).ToList();
            var optimizations = _orderOptimizationRequestService.GetAll().Where(x => parsedOrderIds.Contains(x.OrderId)).ToList();
            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => parsedOrderIds.Contains(x.Id)).ToList();
            foreach (var optimization in optimizations)
            {
                var order = orders.Where(x => x.Id == optimization.OrderId).FirstOrDefault();
                if (order == null) continue;
                string extraLog = string.Empty;
                if (order.SelectedShippingTime != optimization.OriginalSelectedTime)
                    extraLog = $", y el horario regresó al original: '{optimization.OriginalSelectedTime}'";
                order.OrderNotes.Add(new OrderNote()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,
                    DisplayToCustomer = false,
                    Note = $"El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) canceló la solicitud de optimización de horario{extraLog}.",
                    CustomerId = _workContext.CurrentCustomer.Id
                });
                order.SelectedShippingTime = optimization.OriginalSelectedTime;
                _orderService.UpdateOrder(order);
                _orderOptimizationRequestService.Delete(optimization);
            }

            return RedirectToAction("Index", new { date });
        }

        public IActionResult UpgradeOptimization(string orderids, string date)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OptimizationRequestManager))
                return Unauthorized();

            var parsedOrderIds = orderids.Split(',').SelectMany(x => x.Split('-')).Select(x => int.Parse(x)).ToList();
            var optimizations = _orderOptimizationRequestService.GetAll().Where(x => parsedOrderIds.Contains(x.OrderId)).ToList();
            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => parsedOrderIds.Contains(x.Id)).ToList();
            foreach (var optimization in optimizations)
            {
                var order = orders.Where(x => x.Id == optimization.OrderId).FirstOrDefault();
                if (order == null) continue;
                order.OrderNotes.Add(new OrderNote()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,
                    DisplayToCustomer = false,
                    Note = $"El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el tipo de optimización de '{((OrderOptimizationTypes)optimization.SelectedOptimizationTypeId).GetDisplayName()}' a '{OrderOptimizationTypes.Inmediate.GetDisplayName()}'.",
                    CustomerId = _workContext.CurrentCustomer.Id
                });
                _orderService.UpdateOrder(order);
                optimization.SelectedOptimizationTypeId = (int)OrderOptimizationTypes.Inmediate;
                _orderOptimizationRequestService.Update(optimization);
            }

            return RedirectToAction("Index", new { date });
        }

        public IActionResult ResetSchedules(string date)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OptimizationRequest) && !_permissionService.Authorize(TeedGroceriesPermissionProvider.OptimizationRequestManager))
                return Unauthorized();

            var parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => parsedDate == x.SelectedShippingDate).ToList();
            var orderIds = orders.Select(x => x.Id).ToList();
            // We get optimizations of day and not requested by client
            var optimizations = _orderOptimizationRequestService.GetAll().Where(x => orderIds.Contains(x.OrderId) && x.SelectedOptimizationTypeId != 4).ToList();
            foreach (var optimization in optimizations)
            {
                var order = orders.Where(x => x.Id == optimization.OrderId).FirstOrDefault();
                if (order == null) continue;
                order.SelectedShippingTime = optimization.OriginalSelectedTime;
                order.OrderNotes.Add(new OrderNote()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,
                    DisplayToCustomer = false,
                    Note = $"El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) regresó la hora de entrega de la orden a su horario original de {optimization.FinalTimeSelected} a {optimization.OriginalSelectedTime}.",
                    CustomerId = _workContext.CurrentCustomer.Id
                });
                _orderService.UpdateOrder(order);
            }

            return RedirectToAction("Index", new { date });
        }
    }
}
