using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Manager.Domain.PurchaseOrders
{
    public class PurchaseOrderProduct : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public int ProductId { get; set; }
        public int PurchaseOrderId { get; set; }
        public int CurrentInventory { get; set; }
        public int RequestedUnits { get; set; }
        public string CustomProductName { get; set; }
    }
}