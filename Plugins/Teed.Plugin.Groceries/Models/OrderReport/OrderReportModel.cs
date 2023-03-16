using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.OrderReport
{
    public class OrderReportModel
    {
        public int OrderId { get; set; }
        public List<OrderReportFiles> OrderReportFiles { get; set; }
        public List<OrderReportData> OrderItemsReport { get; set; }
    }

    public class OrderReportData
    {
        public int OrderItemId { get; set; }
        public decimal UnitCost { get; set; }
        public decimal RequestedQtyCost { get; set; }
        public string Store { get; set; }
        public string Comment { get; set; }
    }

    public class OrderReportFiles
    {
        public string Name { get; set; }
        public string Base64 { get; set; }
        public string Type { get; set; }
        public int Size { get; set; }
    }
}
