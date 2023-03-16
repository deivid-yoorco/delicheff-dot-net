using Nop.Core;
using Nop.Core.Domain.Customers;
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
    public class TakenVacationDay : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int PayrollId { get; set; }
        public virtual DateTime VacationDate { get; set; }
        public virtual string Comment { get; set; }
        public virtual string Log { get; set; }
    }
}
