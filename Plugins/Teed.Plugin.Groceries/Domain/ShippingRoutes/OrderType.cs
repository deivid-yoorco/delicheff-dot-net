using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.ShippingRoutes
{
    public class OrderType : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public string OrderTypeName { get; set; }
        public decimal MinimumProductQty { get; set; }
        public decimal MaxProductQty { get; set; }
        public decimal CargoSpace { get; set; }
    }
}