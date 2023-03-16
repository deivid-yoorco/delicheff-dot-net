using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.MarketingDashboard
{
    public class MarketingDashboardSettings : ISettings
    {
        public DateTime LastDashboardDataUpdateUtc { get; set; }
        public DateTime LastAutomaticDashboardDataUpdateUtc { get; set; }
    }
}
