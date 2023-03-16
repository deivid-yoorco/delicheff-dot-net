using Nop.Services.Orders;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Logging;
using Teed.Plugin.Groceries.Utils;
using Nop.Services.Shipping;
using System.Net;
using System.Net.Sockets;

namespace Teed.Plugin.Groceries.ScheduleTasks
{
    public class CompleteOrdersScheduleTask : IScheduleTask
    {
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;

        public CompleteOrdersScheduleTask(IOrderService orderService,
            IOrderProcessingService orderProcessingService)
        {
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
        }

        public void Execute()
        {
            var controlDate = DateTime.Now.AddDays(-3).Date;
            var today = DateTime.Now.Date;
            var pendingOrders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate &&
                x.SelectedShippingDate <= today &&
                x.PaymentStatusId == 30 &&
                x.Shipments.Any() &&
                x.OrderStatusId != 30)
                .ToList();

            foreach (var order in pendingOrders)
            {
                var pendingDeliveryTimes = order.Shipments.Where(x => !x.DeliveryDateUtc.HasValue).ToList();
                foreach (var pendingDeliveryTime in pendingDeliveryTimes)
                    pendingDeliveryTime.DeliveryDateUtc = pendingDeliveryTime.ShippedDateUtc;

                order.ShippingStatus = Nop.Core.Domain.Shipping.ShippingStatus.Delivered;
                order.ShippingStatusId = (int)Nop.Core.Domain.Shipping.ShippingStatus.Delivered;

                _orderProcessingService.CheckOrderStatus(order);
            }
        }
    }
}
