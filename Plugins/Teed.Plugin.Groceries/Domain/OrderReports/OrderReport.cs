using Nop.Core;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.OrderReports
{
    public class OrderReport : BaseEntity
    {
        public virtual Guid GuidId { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int OrderId { get; set; }
        public virtual int OrderItemId { get; set; }
        public virtual int ProductId { get; set; }
        public virtual decimal UnitCost { get; set; }
        public virtual decimal RequestedQtyCost { get; set; }
        public virtual string ShoppingStoreId { get; set; }
        public virtual int? ManufacturerId { get; set; }
        public virtual string Comments { get; set; }
        public virtual decimal? Quantity { get; set; }
        public virtual int? NotBuyedReasonId { get; set; }
        public virtual string NotBuyedReason { get; set; }
        public virtual int ReportedByCustomerId { get; set; }
        public virtual int OriginalBuyerId { get; set; }
        public virtual DateTime ReportedDateUtc { get; set; }
        public virtual DateTime OrderShippingDate { get; set; }
        public virtual string Invoice { get; set; }
        public virtual bool? SentToSupermarket { get; set; }
        public virtual int? SentToSupermarketByUserId { get; set; }

        public virtual decimal? UpdatedRequestedQtyCost { get; set; }
        public virtual decimal? UpdatedQuantity { get; set; }
        public virtual decimal UpdatedUnitCost { get; set; }
        public virtual int? UpdatedByUserId { get; set; }
        public virtual int? BoughtTypeId { get; set; }
    }
}
