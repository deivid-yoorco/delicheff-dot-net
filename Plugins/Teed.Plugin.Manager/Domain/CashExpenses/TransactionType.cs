using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Manager.Domain.CashExpenses
{
    public enum TransactionType
    {
        [Display(Name = "Gasto")]
        Expense = 0,

        [Display(Name = "Transferencia")]
        Transfer = 1,

        [Display(Name = "Abono")]
        Deposit = 2,
    }
}
