using Nop.Services.Orders;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.MarketingDashboard.Services;
using Teed.Plugin.MarketingDashboard.Utils;

namespace Teed.Plugin.MarketingDashboard.ScheduleTasks
{
    public class DashboardDataScheduleTask : IScheduleTask
    {
        private readonly IOrderService _orderService;
        private readonly MarketingDashboardDataService _marketingDashboardDataService;

        public DashboardDataScheduleTask(MarketingDashboardDataService marketingDashboardDataService,
            IOrderService orderService)
        {
            _marketingDashboardDataService = marketingDashboardDataService;
            _orderService = orderService;
        }

        public void Execute()
        {
            var orders = OrderUtils.GetFilteredOrders(_orderService).ToList();
            var controlDateUtc = DateTime.UtcNow;

            if (Nop.Services.TeedCommerceStores.CurrentStore == Nop.Services.TeedStores.CentralEnLinea)
            {
                _marketingDashboardDataService.GenerateDashboardData10(orders, controlDateUtc, 0);
                _marketingDashboardDataService.GenerateDashboardData20(orders, controlDateUtc, 0);
            }
            else
            {
                _marketingDashboardDataService.GenerateDashboardData10(orders, controlDateUtc, 1);
                _marketingDashboardDataService.GenerateDashboardData20(orders, controlDateUtc, 1);
            }
        }
    }
}
