using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.OrderType
{
    public class OrderTypeModel
    {
        public int Id { get; set; }
        public string OrderTypeName { get; set; }
        public decimal MinimumProductQty { get; set; }
        public decimal MaxProductQty { get; set; }
        public decimal CargoSpace { get; set; }
    }
}
