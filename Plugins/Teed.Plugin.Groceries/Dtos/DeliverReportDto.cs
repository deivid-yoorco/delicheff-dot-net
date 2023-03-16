using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Dtos
{
    public class DeliverReportDto
    {
        public DateTime Date { get; set; }
    }

    public class OrderDto
    {
        public List<OrderData> GroupedOrders { get; set; }
        public List<OrderData> RescuedOrders { get; set; }
        public List<SelectListItem> PaymentMethods { get; set; }
    }

    public class OrderData
    {
        public int Id { get; set; }
        public int RouteDisplayOrder { get; set; }
        public string SelectedShippingTime { get; set; }
        public DateTime? SelectedShippingDate { get; set; }
        public string CustomOrderNumber { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal OriginalOrderTotal { get; set; }
        public decimal OrderSubTotalDiscountInclTax { get; set; }
        public decimal OrderDiscount { get; set; }
        public decimal OrderShippingInclTax { get; set; }
        public int PaymentStatusId { get; set; }
        public int ShippingStatusId { get; set; }
        public string PaymentMethodSystemName { get; set; }
        public string PaymentMethodCommonName { get; set; }
        public decimal TipWithCard { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
        public OrderAddressDto ShippingAddress { get; set; }
        public List<string> OrderNotes { get; set; }
        public int? RescuedByRouteId { get; set; }
        public int? RescueRouteDisplayOrder { get; set; }
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public int ProductId { get; set; }
        public decimal WeightInterval { get; set; }
        public decimal EquivalenceCoefficient { get; set; }
        public string SelectedPropertyOption { get; set; }
        public bool BuyingBySecondary { get; set; }
        public string RequestedUnit { get; set; }
        public decimal RequestedQuantity { get; set; }
        public string ProductPictureUrl { get; set; }
        public decimal Price { get; set; }
        public decimal UnitPrice { get; set; }
        public int RawQuantity { get; set; }
        public int NotDeliveredReasonId { get; set; }
        public string NotDeliveredReason { get; set; }
        public bool NotBuyedReportedByBuyer { get; set; }
    }

    public class OrderAddressDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string ZipPostalCode { get; set; }
        public string CustomAttributes { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
    }

    public class UpdateOrderItemDto
    {
        public int OrderItemId { get; set; }
        public int RawQuantity { get; set; }
        public int NotDeliveredReasonId { get; set; }
        public string NotDeliveredReason { get; set; }
        public bool NotWanted { get; set; }
    }

    public class OrderTotalsDto
    {
        public int OrderId { get; set; }
        public decimal Total { get; set; }
    }
}
