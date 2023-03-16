using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.MarketingDashboard.Models.MarketingExpenses
{
    public class MarketingExpenseModel
    {
        public MarketingExpenseModel()
        {
            MarketingExpenseTypes = new List<SelectListItem>();
        }

        public int Id { get; set; }
        public int ExpenseTypeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime InitDate { get; set; }
        public string InitDateString { get; set; }
        public DateTime EndDate { get; set; }
        public string EndDateString { get; set; }
        public string Comment { get; set; }
        public string Log { get; set; }

        public IList<SelectListItem> MarketingExpenseTypes { get; set; }
    }
}
