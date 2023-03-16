using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.OrderReport
{
    public class EditOrderReportModel
    {
        public int OrderId { get; set; }
        public List<EditOrderReportData> OrderItemsReport { get; set; }
        public List<OrderReportFileData> OrderReportFiles { get; set; }
        public List<OrderReportLogData> LogData { get; set; }
    }

    public class EditOrderReportData
    {
        public OrderItem OrderItem { get; set; }
        public int OrderItemId { get; set; }
        public decimal UnitCost { get; set; }
        public decimal RequestedQtyCost { get; set; }
        public string Store { get; set; }
        public string Comment { get; set; }
    }

    public class OrderReportLogData
    {
        public string Log { get; set; }
        public DateTime Date { get; set; }
        public Customer Author { get; set; }
    }

    public class OrderReportFileData
    {
        public string Name { get; set; }
        public string Base64 { get; set; }
        public string Type { get; set; }
        public int Size { get; set; }
        public string FileUrl { get; set; }
    }
}
