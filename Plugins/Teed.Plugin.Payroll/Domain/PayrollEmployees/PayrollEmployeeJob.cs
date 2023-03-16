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
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Domain.PayrollEmployeeJobs
{
    public class PayrollEmployeeJob : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int JobCatalogId { get; set; }
        public virtual JobCatalog JobCatalog { get; set; }

        public virtual int PayrollEmployeeId { get; set; }
        public virtual PayrollEmployee PayrollEmployee { get; set; }
        public virtual DateTime ApplyDate { get; set; }

        public virtual string Log { get; set; }
    }
}
