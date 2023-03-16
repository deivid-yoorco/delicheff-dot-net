using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using System;

namespace Teed.Plugin.Groceries.Domain.OrderReports
{
    public class OrderItemLog : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public int CustomerId { get; set; }
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public int OrderId { get; set; }
        public decimal OriginalQuantity { get; set; }
        public decimal NewQuantity { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal NewPrice { get; set; }
    }
}