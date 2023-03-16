using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.OrderReport
{
    public class GetOrderReportsModel
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int OrderItemId { get; set; }
        public int OrderResponsableId { get; set; }
        public decimal UnitCost { get; set; }
        public decimal RequestedQtyCost { get; set; }
        public string ShoppingStoreId { get; set; }
        public string Comments { get; set; }
        public List<string> FilesUrl { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
    }
}
