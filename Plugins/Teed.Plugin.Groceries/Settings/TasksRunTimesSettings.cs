using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Settings
{
    public class TasksRunTimesSettings : ISettings
    {
        public DateTime TimeOfRun
        {
            get
            {
                return new DateTime(1, 1, 1, 0, 30, 00);
            }
            set
            { 
                // No set for this entity
            }
        }

        public DateTime RunDate_FranchiseMonthlyChargeScheduleTask { get; set; }
        public DateTime RunDate_FranchiseBonusScheduleTask { get; set; }
        public DateTime RunDate_IncidentsCheckScheduleTask { get; set; }
        public DateTime RunDate_GrowthHackingRewardScheduleTask { get; set; }
        public DateTime RunDate_GrowthHackingScheduleTask { get; set; }
        public DateTime RunDate_UrbanPromoterFeesInsertScheduleTask { get; set; }
        public DateTime RunDate_FranchiseAutomaticBillingScheduleTask { get; set; }
        public DateTime RunDate_RecurrenceReminderTask { get; set; }
        public DateTime RunDate_CorcelConvertScheduleTask { get; set; }

        //public DateTime RunDate_ReorderEmailScheduleTask { get; set; } IN EMAIL PLUGIN
    }
}
