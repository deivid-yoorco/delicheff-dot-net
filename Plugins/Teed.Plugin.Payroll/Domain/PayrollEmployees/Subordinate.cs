using Nop.Core;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.Incidents;
using Teed.Plugin.Payroll.Domain.JobCatalogs;

namespace Teed.Plugin.Payroll.Domain.PayrollEmployees
{
    public class Subordinate : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }
        
        public virtual int BossId { get; set; }
        public virtual PayrollEmployee Boss { get; set; }

        public virtual int PayrollSubordinateId { get; set; }
        public virtual PayrollEmployee PayrollSubordinate { get; set; }
    }
}
