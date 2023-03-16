using Nop.Core;
using System;

namespace Teed.Plugin.Groceries.Domain.OrderReports
{
    public class ManagerQuantities : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public DateTime ShippingDate { get; set; }

        public decimal InitialAmount { get; set; }
        public decimal AmountManagerReceives { get; set; }
        public string Log { get; set; }

    }
}
