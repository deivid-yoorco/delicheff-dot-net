using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.MarketingDashboard.Domain.MarketingExpenses
{
    public class MarketingExpense : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public int ExpenseTypeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Comment { get; set; }
        public string Log { get; set; }

        public MarketingExpenseType ExpenseType { get; set; }
    }
}
