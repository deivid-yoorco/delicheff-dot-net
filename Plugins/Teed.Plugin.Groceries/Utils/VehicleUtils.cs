using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingVehicles;

namespace Teed.Plugin.Groceries.Utils
{
    public static class VehicleUtils
    {
        public static string GetVehicleName(ShippingVehicle vehicle)
        {
            if (vehicle == null) return string.Empty;
            return vehicle.Brand + " " + vehicle.Year + " " + vehicle.Plates;
        }
    }
}
