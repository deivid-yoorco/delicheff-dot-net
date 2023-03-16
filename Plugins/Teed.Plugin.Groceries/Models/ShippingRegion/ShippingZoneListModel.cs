using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Models.Settings;

namespace Teed.Plugin.Groceries.Models.ShippingRegion
{
    public class ShippingZoneListModel
    {
        public List<Domain.ShippingZones.ShippingZone> PendingZones { get; set; }
        public GlobalScheduleSettingsModel GlobalScheduleSettings { get; set; }
    }
}
