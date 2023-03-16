using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Settings;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.ScheduleTasks
{
    public class FranchiseAutomaticBillingScheduleTask : IScheduleTask
    {
        private readonly BillingService _billingService;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;

        public FranchiseAutomaticBillingScheduleTask(BillingService billingService,
            ISettingService settingService, ILogger logger)
        {
            _billingService = billingService;
            _settingService = settingService;
            _logger = logger;
        }

        public void Execute()
        {
            //Check if its time to run
            if (TaskRunUtils.ShouldRunTask(_settingService, "FranchiseAutomaticBillingScheduleTask"))
            {
                _logger.Information($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Running FranchiseAutomaticBillingScheduleTask.");

                try
                {
                    _billingService.CreateAutomaticBills();
                }
                catch (Exception e)
                {
                    _logger.Error($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Error while running FranchiseAutomaticBillingScheduleTask:\n\"{e.Message}\".", e);
                }

                _logger.Information($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Finished running FranchiseAutomaticBillingScheduleTask.");
            }
        }
    }
}
