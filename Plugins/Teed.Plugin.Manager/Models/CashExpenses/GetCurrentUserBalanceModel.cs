using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Manager.Models.CashExpenses
{
    public class GetCurrentUserBalanceModel
    {
        public decimal Charge { get; set; }
        public decimal Deposit { get; set; }
        public CustomerModel User { get; set; }
        public DateTime DateObject { get; set; }
    }
}
