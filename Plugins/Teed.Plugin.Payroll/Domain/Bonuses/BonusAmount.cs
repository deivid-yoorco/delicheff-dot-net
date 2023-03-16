using Nop.Core;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Domain.Bonuses
{
    public class BonusAmount : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int BonusId { get; set; }
        public virtual Bonus Bonus { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual DateTime ApplyDate { get; set; }

        public virtual string Log { get; set; }
    }
}
