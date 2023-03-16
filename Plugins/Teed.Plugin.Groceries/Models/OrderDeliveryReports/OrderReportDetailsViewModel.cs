using ImageResizer.Plugins;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Controllers;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Models.OrderDeliveryReports
{
    public class OrderReportDetailsViewModel
    {
        public OrderReportDetailsViewModel()
        {

        }


        public virtual DateTime OrderShippigDate { get; set; }
        public virtual int BuyerId { get; set; }
        public virtual string BuyerName { get; set; }
        public List<ProductData> Products { get; set; }
        public virtual string JsonData { get; set; }
        public virtual bool AuthorizeData { get; set; }
        public ReportStatusType CurrentStatus { get; set; }
        public List<OrderReportFile> Files { get; set; }

        public decimal BuyerCashAmount { get; set; }
        public string BuyerCashAmountString { get; set; }

        public decimal BuyerCardAmount { get; set; }
        public string BuyerCardAmountString { get; set; }

        public decimal AmountTotalSpent { get; set; }

        public decimal AmountTotalReturned { get; set; }

        public decimal AmountTotalTransfer { get; set; }

        public SelectList Manufacturers { get; set; }

        public int TotalProductsUpdated { get; set; }

        public int OrderReportTransferId { get; set; }
        public byte[] FileTransfer { get; set; }
        public string FileTransferB64 { get; set; }
        public decimal AmountTotalInCash { get; set; }
        public IFormFile InputFileTransfer { get; set; }
        public virtual string CurrentUserName { get; set; }
        public virtual string DayOfWeek { get; set; }

        public List<BuyerPaymentModel> BuyerPayments { get; set; }

        public virtual int BoughtTypeId { get; set; }
    }

    public class ProductData
    {
        public virtual int ProductId { get; set; }
        public virtual string ProductName { get; set; }
        public virtual decimal ProductCostKgPz { get; set; }
        public virtual List<string> PreviousCostList { get; set; }
        public virtual decimal ProductAmountTotal { get; set; }

        public virtual decimal ProductQuantity { get; set; }
        public string Manufacturer { get; set; }
        public virtual string NoBuyedReazon { get; set; }

        public virtual int NumberOrders { get; set; }
        public virtual decimal RequestedQuantity { get; set; }
        public virtual string RequestedUnit { get; set; }

        public virtual OrderReportFile File { get; set; }
        public int Category { get; set; }
        public DateTime Create { get; set; }

        public List<SelectedList> ProductManufactures { get; set; }

        public int ManufacturerId { get; set; }

        public int BuyerId { get; set; }

        public virtual decimal ProductCostKgPzOriginal { get; set; }
        public virtual decimal ProductAmountTotalOriginal { get; set; }
        public virtual decimal ProductQuantityOriginal { get; set; }

        public virtual bool IsProductUpdated { get; set; }

        public virtual string Invoice { get; set; }
        public virtual bool SentToSuperMarket { get; set; }
        public virtual string SentToSupermarketByUser { get; set; }

        public int BoughtTypeId { get; set; }
    }

    public class OrderReportDetails
    {
        public virtual int BuyerId { get; set; }
        public virtual string Date { get; set; }

        public virtual string ProductsJson { get; set; }
    }

    public class ProductsJson
    {
        public virtual int ProductId { get; set; }
        public virtual decimal ProductUnitCost { get; set; }
        public virtual decimal ProductRequestCost { get; set; }
        public virtual decimal ProductQuantity { get; set; }

        public virtual int ManufacturerId { get; set; }
        public virtual string ShoppingStoreId { get; set; }
        public virtual string NoBuyedRazon { get; set; }

        public int BuyerId { get; set; }
        public int BoughtTypeId { get; set; }
    }

    public class SelectedList
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }

        public virtual bool IsSelected { get; set; } 
    }

    public class ValidityManufacturers
    {
        public virtual DateTime Date { get; set; }
        public virtual string DateString { get; set; }

        public List<ManufacturesData> Manufactures { get; set; }

        public virtual string ManufacturersJson { get; set; }
    }

    public class ManufacturesData
    {
        public virtual int ManufacturerId { get; set; }
        public virtual string ShoppingStoreId { get; set; }

        public int BuyerId { get; set; }
        public string BuyerName { get; set; }

        public virtual string NewShoppingStoreId { get; set; }

    }
}