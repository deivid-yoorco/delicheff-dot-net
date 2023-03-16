using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.OrderDeliveryReports
{
    public class ManagerExpensesViewModel
    {
        public virtual DateTime DeliveryDate { get; set; }
        public virtual string DayOfWeek { get; set; }

        public virtual string ManagerExpensesJson { get; set; }
        public virtual IList<ManagerExpenseData> ManagerExpenses { get; set; }

        public virtual decimal CashAmountTotal { get; set; }
        public virtual decimal CardAmountTotal { get; set; }
        public virtual decimal TransferAmountTotal { get; set; }
        public virtual decimal CashAmountDeliveredBuyers { get; set; }
        public virtual decimal ReturnedAmountByBuyers { get; set; }

        public virtual decimal InitialAmount { get; set; }
        public virtual decimal AmountManagerReceives { get; set; }
        public virtual decimal ExpenseAmountManager { get; set; }
        public virtual decimal RestAmount { get; set; }

        public virtual bool IsClosedLiquidation { get; set; }
        public virtual string CurrentUserName { get; set; }
    }

    public class ManagerExpenseData
    {
        public virtual int Id { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual string Concept { get; set; }
    }
}
