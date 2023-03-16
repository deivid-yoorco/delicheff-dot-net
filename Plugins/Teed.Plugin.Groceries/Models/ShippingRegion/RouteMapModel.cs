using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain;

namespace Teed.Plugin.Groceries.Models.ShippingRegion
{
    public class RouteMapModel
    {
        public List<Domain.ShippingRoutes.ShippingRoute> Routes { get; set; }
        public List<MapDataModel> Data { get; set; }
        public int ProductsTotal { get; set; }
    }

    public class OrderTotals
    {
        public int CountByHour { get; set; }
        public decimal CountByHourDevidedByRoute { get; set; }
    }

    public class MapDataModel
    {
        public Domain.ShippingRoutes.ShippingRoute Route { get; set; }
        public int ProductsCount { get; set; }
        public List<MapOrderData> Orders { get; set; }
        public decimal OrderTotals { get; set; }
    }

    public class MapOrderData
    {
        public string OrderIds { get; set; }
        public string OrderNumber { get; set; }
        public string ShippingFullName { get; set; }
        public string ShippingAddress { get; set; }
        public string ZoneName { get; set; }
        public string PostalCode { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string SelectedShippingTime { get; set; }
    }
}
