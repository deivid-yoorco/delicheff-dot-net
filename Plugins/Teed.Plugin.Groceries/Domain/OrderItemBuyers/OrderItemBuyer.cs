using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Teed.Plugin.Groceries.Domain.OrderItemBuyers
{
    public class OrderItemBuyer : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public int CustomerId { get; set; }
        public virtual int OrderItemId { get; set; }
        public virtual decimal Cost { get; set; }
        public virtual int? OrderId { get; set; }
        public virtual DateTime? SelectedShippingDate { get; set; }
    }
}