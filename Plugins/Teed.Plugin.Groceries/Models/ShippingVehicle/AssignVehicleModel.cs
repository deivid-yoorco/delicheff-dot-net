using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.ShippingVehicle
{
    public class AssignVehicleModel
    {
        public DateTime Date { get; set; }
        public List<SelectListItem> Vehicles { get; set; }
        public List<RouteVehicle> Routes { get; set; }
        public List<RouteVehicle> RoutesWithoutOrders { get; set; }
    }

    public class RouteVehicle
    {
        public Domain.ShippingRoutes.ShippingRoute Route { get; set; }
        public int SelectedVehicleId { get; set; }
    }

    public class SubmitAssignVehicleModel
    {
        public DateTime Date { get; set; }
        public List<SubmitAssignVehicleData> Data { get; set; }
    }

    public class SubmitAssignVehicleData
    {
        public int RouteId { get; set; }
        public int SelectedVehicleId { get; set; }
    }
}
