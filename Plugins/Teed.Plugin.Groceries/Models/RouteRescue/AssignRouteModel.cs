using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingVehicles;

namespace Teed.Plugin.Groceries.Models.RouteRescue
{
    public class AssignRouteModel
    {
        public List<IGrouping<int, Nop.Core.Domain.Orders.Order>> GroupedOrders { get; set; }
        public List<IGrouping<int, Nop.Core.Domain.Orders.Order>> GroupedRescuedOrders { get; set; }
        public List<ShippingVehicleRoute> VehicleRoutes { get; set; }
        public string SelectedDate { get; set; }
    }

    public class AssignRescueRoutes
    {
        public string Date { get; set; }
        public List<AssignRescueRoutesData> Data { get; set; }
        public List<AssignRescueRoutesData> DisplayOrderData { get; set; }
    }

    public class AssignRescueRoutesData
    {
        public string OrderIds { get; set; }
        public int SelectedValue { get; set; }
    }
}
