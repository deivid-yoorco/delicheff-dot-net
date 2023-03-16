using Nop.Services.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingVehicles;
using Teed.Plugin.Groceries.Settings;

namespace Teed.Plugin.Groceries.Utils
{
    public static class TaskRunUtils
    {
        public static bool ShouldRunTask(ISettingService _settingService, string taskName, bool testOverride = false)
        {
            if (testOverride)
                return true;

            var shouldRun = false;
            var taskSettings = _settingService.LoadSetting<TasksRunTimesSettings>(0);
            var timeToRun = taskSettings.TimeOfRun;
            var today = DateTime.Now.AddDays(1);
            var todayWithRunTime = new DateTime(today.Year, today.Month, today.Day, timeToRun.Hour, timeToRun.Minute, timeToRun.Second);
            DateTime currentTaskRunDate;
            switch (taskName)
            {
                case "FranchiseAutomaticBillingScheduleTask":
                    currentTaskRunDate = taskSettings.RunDate_FranchiseAutomaticBillingScheduleTask;
                    taskSettings.RunDate_FranchiseAutomaticBillingScheduleTask = today;
                    break;
                case "FranchiseBonusScheduleTask":
                    currentTaskRunDate = taskSettings.RunDate_FranchiseBonusScheduleTask;
                    taskSettings.RunDate_FranchiseBonusScheduleTask = today;
                    break;
                case "IncidentsCheckScheduleTask":
                    currentTaskRunDate = taskSettings.RunDate_IncidentsCheckScheduleTask;
                    taskSettings.RunDate_IncidentsCheckScheduleTask = today;
                    break;
                case "GrowthHackingRewardScheduleTask":
                    currentTaskRunDate = taskSettings.RunDate_GrowthHackingRewardScheduleTask;
                    taskSettings.RunDate_GrowthHackingRewardScheduleTask = today;
                    break;
                case "GrowthHackingScheduleTask":
                    currentTaskRunDate = taskSettings.RunDate_GrowthHackingScheduleTask;
                    taskSettings.RunDate_GrowthHackingScheduleTask = today;
                    break;
                case "UrbanPromoterFeesInsertScheduleTask":
                    currentTaskRunDate = taskSettings.RunDate_UrbanPromoterFeesInsertScheduleTask;
                    taskSettings.RunDate_UrbanPromoterFeesInsertScheduleTask = today;
                    break;
                case "FranchiseMonthlyChargeScheduleTask":
                    currentTaskRunDate = taskSettings.RunDate_FranchiseMonthlyChargeScheduleTask;
                    taskSettings.RunDate_FranchiseMonthlyChargeScheduleTask = today;
                    break;
                case "RecurrenceReminderTask":
                    currentTaskRunDate = taskSettings.RunDate_RecurrenceReminderTask;
                    taskSettings.RunDate_RecurrenceReminderTask = today;
                    break;
                case "CorcelConvertScheduleTask":
                    currentTaskRunDate = taskSettings.RunDate_CorcelConvertScheduleTask;
                    taskSettings.RunDate_CorcelConvertScheduleTask = today;
                    break;
                default:
                    return false;
            }

            if (currentTaskRunDate == DateTime.MinValue ||
               (todayWithRunTime <= today &&
               currentTaskRunDate.Date != today.Date))
            {
                _settingService.SaveSetting(taskSettings);
                shouldRun = true;
            }
            return shouldRun;
        }
    }
}
