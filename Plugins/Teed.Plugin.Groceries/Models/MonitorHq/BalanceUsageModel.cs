using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.MonitorHq
{
    public class BalanceUsageModel
    {
        public string SpecificDate { get; set; }
        public decimal BalanceGivenToday { get; set; }
        public decimal BalanceGivenWeek { get; set; }
        public decimal BalanceUsedToday { get; set; }
        public decimal BalanceUsedWeek { get; set; }
    }
}
