using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Dtos
{
    public class ReportProductDto
    {
        public DateTime? Date { get; set; }
        public string ProductName { get; set; }
        public string ProductPictureUrl { get; set; }
        public int ProductId { get; set; }
        public decimal? RequestedQuantity { get; set; }
        public string RequestedUnit { get; set; }
        public decimal? RequestedQtyCost { get; set; }
        public decimal? UnitCost { get; set; }
        public int ManufacturerId { get; set; }
        public decimal? BuyedQuantity { get; set; }
        public string CustomManufacturer { get; set; }
        public decimal ControlPrice { get; set; }
        public decimal SellPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public int? NotBuyedReasonId { get; set; }
        public string NotBuyedReason { get; set; }
        public string OrderItemIds { get; set; }
        public string Invoice { get; set; }
        public bool? SentToSupermarket { get; set; }
        public string SentToSupermarketByUserFullName { get; set; }
        public int? SentToSupermarketByUserId { get; set; }
        public bool IsMarked { get; set; }
        public int MainManufacturerId { get; set; }
        public string MainManufacturerName { get; set; }
        public string OriginalBuyerName { get; set; }
    }

    public class OrderItemData
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public decimal RequestedQuantity { get; set; }
        public string RequestedUnit { get; set; }
        public string PriceUnit { get; set; }
        public string SelectedPropertyOption { get; set; }
        public int ManufacturerId { get; set; }
        public decimal? BuyedQuantity { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? RequestedQtyCost { get; set; }
        public string CustomManufacturer { get; set; }
        public decimal ControlPrice { get; set; }
        public decimal SellPrice { get; set; }
        public int? NotBuyedReasonId { get; set; }
        public string NotBuyedReason { get; set; }
    }

    public class UpdateInvoiceDto
    {
        public int ProductId { get; set; }
        public string Date { get; set; }
        public string Invoice { get; set; }
    }

    public class SaveOrderReportDto
    {
        public List<SaveOrderBodyDto> Body { get; set; }
    }

    public class SaveOrderBodyDto
    {
        public string Date { get; set; }
        public decimal? BuyedQuantity { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? RequestedQtyCost { get; set; }
        public int ManufacturerId { get; set; }
        public string CustomManufacturer { get; set; }
        public int? NotBuyedReasonId { get; set; }
        public string NotBuyedReason { get; set; }
        public int ProductId { get; set; }
        public string Invoice { get; set; }
        public virtual bool? SentToSupermarket { get; set; }
    }

    public class ReportFileDto
    {
        public DateTime Date { get; set; }
        public string Path { get; set; }
        public string Uri { get; set; }
        public string ServerUrl { get; set; }
        public string FileName { get; set; }
        public string ProductIds { get; set; }
        public string FileType { get; set; }
    }

    public class ReportDateDto
    {
        public DateTime? Date { get; set; }
        public int StatusId { get; set; }
        public string StatusValue { get; set; }
    }

    public class DeliveryDateDto
    {
        public DateTime? Date { get; set; }
        public string RouteName { get; set; }
        public int RouteId { get; set; }
    }

    public class SaveMarkedProductDto
    {
        public string OrderShippingDate { get; set; }
        public int BuyerId { get; set; }
        public bool IsMarked { get; set; }
        public int ProductId { get; set; }
        public string action { get; set; }
    }
}
