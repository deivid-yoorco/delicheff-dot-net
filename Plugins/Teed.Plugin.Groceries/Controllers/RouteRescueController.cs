using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Models.RouteRescue;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class RouteRescueController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly ShippingVehicleService _shippingVehicleService;
        private readonly ShippingVehicleRouteService _shippingVehicleRouteService;
        private readonly FranchiseService _franchiseService;
        private readonly ShippingRouteService _shippingRouteService;

        public RouteRescueController(IPermissionService permissionService,
            IWorkContext workContext,
            IOrderService orderService,
            ShippingVehicleService shippingVehicleService,
            ShippingVehicleRouteService shippingVehicleRouteService,
            FranchiseService franchiseService,
            ShippingRouteService shippingRouteService)
        {
            _permissionService = permissionService;
            _workContext = workContext;
            _shippingVehicleService = shippingVehicleService;
            _shippingVehicleRouteService = shippingVehicleRouteService;
            _franchiseService = franchiseService;
            _orderService = orderService;
            _shippingRouteService = shippingRouteService;
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.RouteRescue))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/RouteRescue/List.cshtml");
        }

        public IActionResult AssignRescue(string date)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.RouteRescue))
                return AccessDeniedView();

            var parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate == parsedDate).ToList();

            List<IGrouping<int, Nop.Core.Domain.Orders.Order>> groupedOrders = orders.GroupBy(x => x.RouteId).OrderBy(x => x.Key).ToList();
            List<IGrouping<int, Nop.Core.Domain.Orders.Order>> groupedRescuedOrders = orders
                .Where(x => x.RescuedByRouteId.HasValue && x.RescuedByRouteId > 0)
                .GroupBy(x => x.RescuedByRouteId.Value)
                .OrderBy(x => x.Key)
                .ToList();
            List<Domain.ShippingVehicles.ShippingVehicleRoute> vechicleRoute = _shippingVehicleRouteService.GetAll().Where(x => x.ShippingDate == parsedDate).ToList();

            var model = new AssignRouteModel()
            {
                GroupedOrders = groupedOrders,
                VehicleRoutes = vechicleRoute,
                GroupedRescuedOrders = groupedRescuedOrders,
                SelectedDate = date
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/RouteRescue/AssignRescue.cshtml", model);
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.RouteRescue))
                return AccessDeniedView();

            var today = DateTime.Now.Date;
            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService);
            var query = ordersQuery
                .Where(x => x.SelectedShippingDate <= today)
                .GroupBy(x => x.SelectedShippingDate.Value)
                .Select(x => x.Key);
            var pagedList = new PagedList<DateTime>(query.OrderByDescending(m => m), command.Page - 1, command.PageSize);

            var data = new List<object>();
            foreach (var date in pagedList)
            {
                var dateQuery = ordersQuery.Where(x => x.SelectedShippingDate == date)
                    .Where(x => x.RescuedByRouteId.HasValue && x.RescuedByRouteId.Value > 0);

                var pedidosCount = OrderUtils.GetPedidosOnly(dateQuery).Count();
                var rescuedRoutes = dateQuery
                    .GroupBy(x => x.RouteId)
                    .Count();

                data.Add(new
                {
                    Date = date.ToString("dddd, dd 'de' MMMM 'de' yyyy", new CultureInfo("es-MX")),
                    DateShort = date.ToString("dd-MM-yyyy"),
                    RescuedRoutes = rescuedRoutes,
                    PedidosCount = pedidosCount
                });
            }

            var gridModel = new DataSourceResult
            {
                Data = data,
                Total = query.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult AssignRescueRoutes(AssignRescueRoutes model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.RouteRescue))
                return AccessDeniedView();

            var parsedDate = DateTime.ParseExact(model.Date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate == parsedDate).ToList();

            foreach (var data in model.Data)
            {
                var orderIds = data.OrderIds.Split(',').Select(x => int.Parse(x)).ToList();
                var currentOrders = orders.Where(x => orderIds.Contains(x.Id));
                foreach (var order in currentOrders)
                {
                    if (order.RescuedByRouteId == data.SelectedValue) continue;
                    order.RescuedByRouteId = data.SelectedValue == -1 ? null : (int?)data.SelectedValue;
                    order.RescueRouteDisplayOrder = data.SelectedValue == -1 ? null : (int?)order.RouteDisplayOrder;
                    order.OrderNotes.Add(new Nop.Core.Domain.Orders.OrderNote()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        DisplayToCustomer = false,
                        Note = $"El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) le asignó a la orden la ruta de rescate con Id {data.SelectedValue}",
                        CustomerId = _workContext.CurrentCustomer.Id
                    });
                    _orderService.UpdateOrder(order);
                }
            }

            if (model.DisplayOrderData == null) return NoContent();
            foreach (var data in model.DisplayOrderData)
            {
                var orderIds = data.OrderIds.Split(',').Select(x => int.Parse(x)).ToList();
                var currentOrders = orders.Where(x => orderIds.Contains(x.Id));

                foreach (var order in currentOrders)
                {
                    if (order.RescueRouteDisplayOrder == data.SelectedValue || !order.RescuedByRouteId.HasValue) continue;
                    order.RescueRouteDisplayOrder = data.SelectedValue;
                    order.OrderNotes.Add(new Nop.Core.Domain.Orders.OrderNote()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        DisplayToCustomer = false,
                        Note = $"El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) le asignó a la orden el número {data.SelectedValue} como orden para mostrar de la ruta de rescate",
                        CustomerId = _workContext.CurrentCustomer.Id
                    });
                    _orderService.UpdateOrder(order);
                }
            }
            return NoContent();
        }
    }
}
