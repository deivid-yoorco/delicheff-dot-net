using Nop.Core.Configuration;
using Nop.Core.Domain.Customers;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Orders;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Logging;
using System.Diagnostics;
using Teed.Plugin.Groceries.Utils;
using System.Net;
using System.Net.Sockets;

namespace Teed.Plugin.Groceries.ScheduleTasks
{
    public class GrowthHackingRewardScheduleTask : IScheduleTask
    {
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly IDiscountService _discountService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICustomerService _customerService;
        private readonly ILogger _logger;

        public GrowthHackingRewardScheduleTask(ISettingService settingService,
            IOrderService orderService,
            IDiscountService discountService,
            IOrderProcessingService orderProcessingService,
            ICustomerService customerService,
            ILogger logger)
        {
            _settingService = settingService;
            _orderService = orderService;
            _discountService = discountService;
            _orderProcessingService = orderProcessingService;
            _customerService = customerService;
            _logger = logger;
        }

        public void Execute()
        {
            //Check if its time to run
            if (TaskRunUtils.ShouldRunTask(_settingService, "GrowthHackingRewardScheduleTask"))
            {
                _logger.Information($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Running GrowthHackingRewardScheduleTask.");

                try
                {
                    var settings = _settingService.LoadSetting<GrowthHackingSettings>();
                    if (settings == null || !settings.IsActive) return;

                    var controlDate = DateTime.Now.AddDays(-14);

                    var orderIdsWithDiscount = _discountService.GetAllDiscounts(showHidden: true)
                        .Where(x => x.RelatedOrderId.HasValue && x.RelatedOrderId > 0)
                        .Select(x => x.RelatedOrderId.Value)
                        .ToList();

                    var pendingOrderIdsForCoupon = OrderUtils.GetFilteredOrders(_orderService)
                        .Where(x => x.OrderStatusId == 30 && x.DiscountUsageHistory.Where(y => y.Discount.CustomerOwnerId > 0).Any() && !orderIdsWithDiscount.Contains(x.Id) && x.SelectedShippingDate >= controlDate)
                       .Select(x => x.Id)
                       .ToList();

                    if (pendingOrderIdsForCoupon.Count > 0)
                    {
                        var orders = _orderService.GetAllOrdersQuery().Where(x => pendingOrderIdsForCoupon.Contains(x.Id)).ToList();
                        foreach (var order in orders)
                        {
                            try
                            {
                                _orderProcessingService.GenerateRewardCouponCode(order, settings);
                            }
                            catch (Exception e)
                            {
                                Debugger.Break();
                            }
                            _logger.Information($"[Growth Hacking] Se envió el cupón de recompensa de la orden #{order.CustomOrderNumber}");
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Error while running GrowthHackingRewardScheduleTask:\n\"{e.Message}\".", e);
                }

                _logger.Information($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Finished running GrowthHackingRewardScheduleTask.");
            }
        }
    }
}
