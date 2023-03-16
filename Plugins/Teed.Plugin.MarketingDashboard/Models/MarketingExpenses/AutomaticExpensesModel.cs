using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.MarketingDashboard.Models.MarketingExpenses
{
    public class AutomaticExpensesModel
    {
        public AutomaticExpensesModel()
        {
            //DiscountsByProducts = new List<DiscountsByProduct>();
        }

        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal DiscountsByCoupons { get; set; }
        //public List<DiscountsByProduct> DiscountsByProducts { get; set; }
        public decimal DiscountsByProducts { get; set; }
        public decimal DiscountsByShipping { get; set; }
        public decimal Balances { get; set; }
    }

    public class DiscountsByProduct
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal Amount { get; set; }
    }
}
