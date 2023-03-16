using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;

namespace Teed.Plugin.Groceries.Models.ShippingRoute
{
    public class OrderOptimizationRequestModel
    {
        public string OrderIds { get; set; }
        public string OriginalTime { get; set; }
        public int SelectedOptimizationTypeId { get; set; }
        public string TimeOption1 { get; set; }
        public string TimeOption2 { get; set; }
        public string TimeOption3 { get; set; }
        public string ManagerComment { get; set; }
    }

    public class OptimizationRequestDataModel
    {
        public DateTime Date { get; set; }
        public List<OptimizationData> Data { get; set; }
        public List<CurrentOrderData> CurrentOrdersData { get; set; }
    }

    public class OptimizationData
    {
        public int OptimizationTypeId { get; set; }
        public string OptimizationTypeName { get; set; }
        public List<OptimizationOrderData> OptimizationRequest { get; set; }
    }

    public class OptimizationOrderData
    {
        public string OrderIds { get; set; }
        public string OrderNames { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string OriginalTime { get; set; }
        public string CurrentTime { get; set; }
        public string TimeOption1 { get; set; }
        public string TimeOption2 { get; set; }
        public string TimeOption3 { get; set; }
        public int OptimizationTypeId { get; set; }
        public string RouteName { get; set; }
        public int RouteId { get; set; }
        public int CurrentStatusId { get; set; }
        public string FinalTimeSelected { get; set; }
        public string Comments { get; set; }
        public string ManagerComment { get; set; }
        public bool EmptyRoute { get; set; }
    }

    public class CurrentOrderData
    {
        public string SelectedShippingTime { get; set; }
        public int RouteId { get; set; }
    }

    public class UpdateOptimizationModel
    {
        public List<UpdateOptimizationData> Data { get; set; }
    }

    public class UpdateOptimizationData
    {
        public string OrderIds { get; set; }
        public string SelectedTime { get; set; }
        public int SelectedStatusId { get; set; }
        public string Comments { get; set; }
    }
}
