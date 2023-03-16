using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.OrderItemBuyers
{
    public class BuyerListDownload : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public DateTime OrderShippingDate { get; set; }
        public bool AllowDownload { get; set; }
        public bool? AllowReport { get; set; }
        public string Log { get; set; }
    }
}
