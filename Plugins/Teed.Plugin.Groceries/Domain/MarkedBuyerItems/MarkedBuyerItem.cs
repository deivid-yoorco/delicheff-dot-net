using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.MarkedBuyerItems
{
    public class MarkedBuyerItem : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public DateTime OrderShippingDate { get; set; }
        public int BuyerId { get; set; }
        public bool IsMarked { get; set; }
        public int ProductId { get; set; }
    }
}
