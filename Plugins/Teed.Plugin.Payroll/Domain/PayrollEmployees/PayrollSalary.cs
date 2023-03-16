using Nop.Core;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payroll.Domain.PayrollEmployees
{
    public class PayrollSalary : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual DateTime? ApplyDate { get; set; }
        public virtual decimal NetIncome { get; set; }
        public virtual decimal WithheldISR { get; set; }
        public virtual decimal SocialSecurityContributions { get; set; }
        public virtual decimal Bonds { get; set; }
        public virtual decimal IntegratedDailyWage { get; set; }

        public virtual PayrollEmployee PayrollEmployee { get; set; }
        public virtual int PayrollEmployeeId { get; set; }
    }
}
