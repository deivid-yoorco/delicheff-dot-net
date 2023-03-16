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

namespace Teed.Plugin.Payroll.Domain.Benefits
{
    public class Benefit : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int SalaryOrEmployeeId { get; set; }
        public virtual bool IsForSalary { get; set; }

        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual decimal Amount { get; set; }
    }
}
