using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingVehicles;
using Teed.Plugin.Groceries.Models.OrderOptionButtons;
using Teed.Plugin.Groceries.Services;

namespace Teed.Plugin.Groceries.Components
{
    [ViewComponent(Name = "OrderOptionButtons")]
    public class OrderOptionButtonsViewComponent : NopViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly ShippingVehicleRouteService _shippingVehicleRouteService;

        public OrderOptionButtonsViewComponent(ICustomerService customerService, IWorkContext workContext,
            IOrderService orderService, ShippingVehicleRouteService shippingVehicleRouteService)
        {
            _customerService = customerService;
            _workContext = workContext;
            _orderService = orderService;
            _shippingVehicleRouteService = shippingVehicleRouteService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var orderId = (int)additionalData;
            Order order = _orderService.GetOrderById(orderId);

            ShippingVehicleRoute shippingVehicleRoute = null;
            if (order != null)
                shippingVehicleRoute = _shippingVehicleRouteService.GetAll()
                    .Where(x => x.RouteId == order.RouteId && x.ShippingDate == order.SelectedShippingDate)
                    .FirstOrDefault();

            var model = new OrderOptionButtonsModel()
            {
                OrderId = orderId,
                Date = order != null ? order.SelectedShippingDate.Value.ToString("dd-MM-yyyy") : null,
                FranchiseId = shippingVehicleRoute != null && shippingVehicleRoute.Vehicle != null ? shippingVehicleRoute.Vehicle.FranchiseId : 0,
                VehicleId = shippingVehicleRoute != null ? shippingVehicleRoute.VehicleId : 0
            };
            return View("~/Plugins/Teed.Plugin.Groceries/Views/OrderOptionButtons/Default.cshtml", model);
        }
    }
}
