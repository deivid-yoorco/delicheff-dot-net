using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.MarketingDashboard.Domain.MarketingExpenses
{
    public class MarketingAutomaticExpense : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public DateTime InitDate { get; set; }

        public decimal DiscountByCoupons { get; set; }
        public decimal DiscountByShipping { get; set; }
        public decimal DiscountByProducts { get; set; }
        public decimal Balances { get; set; }
        public string Log { get; set; }

        public virtual ICollection<MarketingDiscountExpense> MarketingDiscountExpenses { get; set; }
    }
}
