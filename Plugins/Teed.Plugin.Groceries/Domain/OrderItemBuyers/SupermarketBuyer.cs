using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.OrderItemBuyers
{
    public class SupermarketBuyer : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public DateTime ShippingDate { get; set; }
        public int BuyerId { get; set; }
        public string Log { get; set; }
    }
}
