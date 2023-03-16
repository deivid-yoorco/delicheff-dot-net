using Nop.Core;
using System;

namespace Teed.Plugin.Manager.Domain.CashExpenses
{
    public class CashExpense : BaseEntity
    {
        public virtual Guid GuidId { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public int CreatedByUserId { get; set; }
        public int ReceiverUserId { get; set; }
        public string Concept { get; set; }
        public decimal Amount { get; set; }
        public string Log { get; set; }
        public TransactionType TransactionType { get; set; }
        public string Comments { get; set; }
        public virtual DateTime ExpenseDate { get; set; }
    }
}
