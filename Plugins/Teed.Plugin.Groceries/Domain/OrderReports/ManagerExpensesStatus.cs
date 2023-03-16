using Nop.Core;
using System;

namespace Teed.Plugin.Groceries.Domain.OrderReports
{
    public class ManagerExpensesStatus : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public DateTime ShippingDate { get; set; }
        public int ClosedById { get; set; }
        public DateTime? ClosedDateUtc { get; set; }
        public string Log { get; set; }
    }
}
