using Nop.Core;
using System;

namespace Teed.Plugin.Manager.Domain.PurchaseOrders
{
    public class PurchaseOrder : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public int BranchId { get; set; }
        public int RequestedById { get; set; }
        public int ProviderId { get; set; }
        public DateTime RequestedDateUtc { get; set; }
        public PurchaseOrderPaymentStatus PaymentStatus { get; set; }
        public PurchaseOrderStatus PurchaseOrderStatus { get; set; }
        public string Comment { get; set; }
        public string Log { get; set; }
        public decimal PartialPaymentValue { get; set; }
    }
}
