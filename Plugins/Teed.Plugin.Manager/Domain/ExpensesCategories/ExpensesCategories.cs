using Nop.Core;
using System;

namespace Teed.Plugin.Manager.Domain.ExpensesCategories
{
    public class ExpensesCategories : BaseEntity
    {
        public virtual Guid GuidId { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int ExpenseCategoryId { get; set; }
        public virtual int ParentId { get; set; }
        public virtual string Value { get; set; }
        public virtual string ValueTitle { get; set; }
    }
}