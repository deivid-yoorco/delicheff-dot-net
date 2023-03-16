using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace Teed.Plugin.Groceries.Models.ShippingUserRoute
{
    public class AssignRoutesModel
    {
        public DateTime Date { get; set; }
        public List<SelectListItem> Customers { get; set; }
        public List<RouteCustomer> Routes { get; set; }
    }

    public class RouteCustomer
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; }
        public string VehicleName { get; set; }
        public List<int> CustomerIds { get; set; }
    }

    public class SubmitAssignRouteModel
    {
        public DateTime Date { get; set; }
        public List<SubmitAssignRouteData> Data { get; set; }
    }

    public class SubmitAssignRouteData
    {
        public int RouteId { get; set; }
        public string[] SelectedCustomerIds { get; set; }
    }
}