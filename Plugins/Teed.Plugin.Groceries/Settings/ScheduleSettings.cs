using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Settings
{
    public class ScheduleSettings : ISettings
    {
        public int Schedule1Quantity { get; set; }
        public int Schedule2Quantity { get; set; }
        public int Schedule3Quantity { get; set; }
        public int Schedule4Quantity { get; set; }
    }
}
