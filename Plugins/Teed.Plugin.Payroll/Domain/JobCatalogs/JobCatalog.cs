using Nop.Core;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.PayrollEmployeeJobs;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Domain.JobCatalogs
{
    public class JobCatalog : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }
        
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual int DisplayOrder { get; set; }
        public virtual decimal? SalaryMax { get; set; }
        public virtual decimal? SalaryMin { get; set; }

        public virtual string WorkSchedule { get; set; }
        public virtual int? ParentJobId { get; set; }

        public virtual bool? SubjectToWorkingHours { get; set; }

        public virtual ICollection<PayrollEmployee> PayrollEmployees { get; set; }
        public virtual ICollection<PayrollEmployeeJob> JobConections { get; set; }
    }
}
