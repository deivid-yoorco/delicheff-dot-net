using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Components
{
    [ViewComponent(Name = "OrderSuspiciousCheck")]
    public class OrderSuspiciousCheckViewComponent : NopViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;

        public OrderSuspiciousCheckViewComponent(ICustomerService customerService,
            IOrderService orderService)
        {
            _customerService = customerService;
            _orderService = orderService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            int orderId = (int)additionalData;
            var order = _orderService.GetOrderById(orderId);
            var model = new List<string>();
            if (order != null)
            {
                var customerOrders = OrderUtils.GetFilteredOrders(_orderService)
                    .Where(x => x.CustomerId == order.CustomerId)
                    .ToList();
                if (customerOrders.Count < 4)
                {
                    model = OrderUtils.GetOrderSuspiciousElements(order, customerOrders, _customerService);
                }
            }

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Shared/Components/OrderSuspiciousCheck/Default.cshtml", model);
        }
    }
}
