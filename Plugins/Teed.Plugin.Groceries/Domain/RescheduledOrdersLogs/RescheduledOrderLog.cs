using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Teed.Plugin.Groceries.Domain.RescheduledOrderLogs
{
    public class RescheduledOrderLog : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public int OrderId { get; set; }
        public virtual DateTime OriginalShippingDate { get; set; }
        public virtual DateTime NewShippingDate { get; set; }
        public virtual int RescheduledBy { get; set; }

        public virtual string ShippingTime { get; set; }
        public virtual string OriginalShippingTime { get; set; }
        public virtual int OriginalRouteId { get; set; }

    }
}