using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Manager.Domain.CashExpenses
{
    public class CashExpenseFile : BaseEntity
    {
        public virtual Guid GuidId { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int CashExpenseId { get; set; }
        public virtual int Size { get; set; }
        public virtual int UploadedByUserId { get; set; }
        public virtual string Extension { get; set; }
        public virtual string FileType { get; set; }
        public virtual string FileUrl { get; set; }
        public virtual string FileName { get; set; }
    }
}
