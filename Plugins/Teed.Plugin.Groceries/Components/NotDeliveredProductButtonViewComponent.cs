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
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Components
{
    [ViewComponent(Name = "NotDeliveredProductButton")]
    public class NotDeliveredProductButtonViewComponent : NopViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ShippingVehicleRouteService _shippingVehicleRouteService;

        public NotDeliveredProductButtonViewComponent(ICustomerService customerService, IWorkContext workContext,
            IOrderService orderService, ShippingVehicleRouteService shippingVehicleRouteService,
            IOrderProcessingService orderProcessingService)
        {
            _customerService = customerService;
            _workContext = workContext;
            _orderService = orderService;
            _shippingVehicleRouteService = shippingVehicleRouteService;
            _orderProcessingService = orderProcessingService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var orderId = (int)additionalData;
            var order = _orderService.GetOrderById(orderId);
            var afterDeliver = DateTime.Now.Date >= order.SelectedShippingDate.Value;

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Shared/Components/NotDeliveredProductButton/Default.cshtml", afterDeliver);
        }
    }
}
