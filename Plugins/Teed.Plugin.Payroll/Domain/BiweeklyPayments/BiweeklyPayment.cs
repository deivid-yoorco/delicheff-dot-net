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

namespace Teed.Plugin.Payroll.Domain.BiweeklyPayments
{
    public class BiweeklyPayment : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual decimal OriginalPayment { get; set; }
        public virtual DateTime Payday { get; set; }
        public virtual bool Paid { get; set; }

        public virtual int CustomerThatReportedId { get; set; }

        public virtual int PayrollEmployeeId { get; set; }
        public virtual PayrollEmployee PayrollEmployee { get; set; }

        public virtual int PayrollSalaryId { get; set; }
        public virtual PayrollSalary PayrollSalary { get; set; }

        public virtual ICollection<BiweeklyPaymentFile> BiweeklyPaymentFiles { get; set; }
    }
}
