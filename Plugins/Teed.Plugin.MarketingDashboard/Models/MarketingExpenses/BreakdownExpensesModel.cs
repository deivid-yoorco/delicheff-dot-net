using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.MarketingDashboard.Models.MarketingExpenses
{
    public class BreakdownExpensesModel
    {
        public BreakdownExpensesModel()
        {
        }

        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal Amount { get; set; }
    }
}
