using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.MarketingDashboard.Domain.MarketingExpenses
{
    public class MarketingDiscountExpense : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public int MarketingAutomaticExpenseId { get; set; }
        public MarketingAutomaticExpense MarketingAutomaticExpense { get; set; }

        public int EntityTypeId { get; set; }
        public int EntityId { get; set; }
        public decimal Amount { get; set; }
        public string Log { get; set; }
    }
}
