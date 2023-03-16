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
using System.Net;
using System.Net.Sockets;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.ScheduleTasks
{
    public class GrowthHackingScheduleTask : IScheduleTask
    {
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly IDiscountService _discountService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICustomerService _customerService;
        private readonly ILogger _logger;

        public GrowthHackingScheduleTask(ISettingService settingService,
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
            if (TaskRunUtils.ShouldRunTask(_settingService, "GrowthHackingScheduleTask"))
            {
                _logger.Information($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Running GrowthHackingScheduleTask.");

                try
                {
                    var today = DateTime.Now;
                    var controlDate = today.AddDays(-7);
                    var settings = _settingService.LoadSetting<GrowthHackingSettings>();
                    if (settings == null || !settings.IsActive) return;
                    var discountCustomerIds = _discountService.GetAllDiscounts()
                        .Select(x => x.CustomerOwnerId)
                        .Where(x => x > 0)
                        .GroupBy(x => x)
                        .Select(x => x.Key)
                        .ToList();

                    var orders = Utils.OrderUtils.GetFilteredOrders(_orderService)
                        .Where(x => x.OrderStatusId == 30 && x.OrderTotal >= settings.MinimumAmountToCreateFriendCode && x.SelectedShippingDate >= controlDate && x.SelectedShippingDate < today)
                        .ToList();
                    var pendingCustomerIds = orders
                        .Where(x => !discountCustomerIds.Contains(x.CustomerId))
                        .GroupBy(x => x.CustomerId)
                        .Select(x => x.Key)
                        .ToList();

                    if (pendingCustomerIds.Count > 0)
                    {
                        var customers = _customerService.GetAllCustomersQuery().Where(x => pendingCustomerIds.Contains(x.Id)).ToList();
                        var count = 0;
                        foreach (var customer in customers)
                        {
                            if (string.IsNullOrEmpty(customer.Email)) continue;
                            count++;
                            try
                            {
                                _orderProcessingService.ActivateGrowHackingForCustomer(customer, settings);

                            }
                            catch (Exception e)
                            {
                                Debugger.Break();
                            }
                            _logger.Information($"[Growth Hacking] Se creó el código de amigo de {customer.Email}", null, customer);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Error while running GrowthHackingScheduleTask:\n\"{e.Message}\".", e);
                }

                _logger.Information($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Finished running GrowthHackingScheduleTask.");
            }
        }
    }
}
