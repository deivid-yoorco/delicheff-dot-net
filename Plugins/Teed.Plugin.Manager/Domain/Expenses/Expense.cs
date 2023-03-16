using Nop.Core;
using System;

namespace Teed.Plugin.Manager.Domain.Expenses
{
    public class Expense : BaseEntity
    {
        public virtual Guid GuidId { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual DateTime ExpenseDate { get; set; }
        public virtual int CreatedByUserId { get; set; }
        public virtual string Concept { get; set; }
        public virtual string Comments { get; set; }
        public virtual int CategoryId { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual decimal Tax { get; set; }
        public virtual decimal Total { get; set; }
        public virtual VoucherType VoucherType { get; set; }
        public virtual PaymentType PaymentType { get; set; }
        public virtual string Log { get; set; }
    }
}