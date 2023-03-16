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
using Teed.Plugin.Payroll.Domain.PayrollEmployeeJobs;

namespace Teed.Plugin.Payroll.Domain.PayrollEmployees
{
    public class PayrollEmployee : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int CustomerId { get; set; }

        public virtual string FirstNames { get; set; }
        public virtual string MiddleName { get; set; }
        public virtual string LastName { get; set; }
        public virtual DateTime? DateOfBirth { get; set; }
        public virtual DateTime? DateOfAdmission { get; set; }
        public virtual DateTime? TrialPeriodEndDate { get; set; }
        public virtual DateTime? DateOfDeparture { get; set; }
        public virtual bool? SatisfactoryDepartureProcess { get; set; }
        public virtual string Address { get; set; }
        public virtual string Latitude { get; set; }
        public virtual string Longitude { get; set; }
        public virtual string Cellphone { get; set; }
        public virtual string Landline { get; set; }
        public virtual string ReferenceOneName { get; set; }
        public virtual string ReferenceOneContact { get; set; }
        public virtual string ReferenceTwoName { get; set; }
        public virtual string ReferenceTwoContact { get; set; }
        public virtual string ReferenceThreeName { get; set; }
        public virtual string ReferenceThreeContact { get; set; }
        public virtual string IMSS { get; set; }
        public virtual string RFC { get; set; }
        public virtual string CURP { get; set; }
        public virtual int EmployeeNumber { get; set; }
        public virtual int BankTypeId { get; set; }
        public virtual string Clabe { get; set; }
        public virtual string AccountNumber { get; set; }
        public virtual int? JobCatalogId { get; set; }
        public virtual JobCatalog JobCatalog { get; set; }
        public virtual int EmployeeStatusId { get; set; }
        public virtual int? DepartureReasonTypeId { get; set; }
        public virtual string DepartureComment { get; set; }
        public virtual int? PayrollContractTypeId { get; set; }
        public virtual int? PayrollProfilePictureId { get; set; }
        public virtual int? FranchiseId { get; set; }
        public virtual string Log { get; set; }

        public virtual ICollection<Incident> Incidents { get; set; }
        public virtual ICollection<PayrollSalary> PayrollSalaries { get; set; }
        public virtual ICollection<PayrollEmployeeJob> JobCatalogs { get; set; }

        public virtual string GetFullName()
        {
            return string.Join(" ", new List<string> { FirstNames, MiddleName, LastName });
        }

        public virtual PayrollSalary GetCurrentSalary(DateTime? date = null)
        {
            if (date == DateTime.MinValue || date == null)
                return PayrollSalaries.Where(x => x.ApplyDate.Value <= DateTime.Now.Date).OrderByDescending(x => x.ApplyDate).FirstOrDefault();
            else
                return PayrollSalaries.Where(x => x.ApplyDate.Value <= date.Value.Date).OrderByDescending(x => x.ApplyDate).FirstOrDefault();
        }

        public virtual decimal GetCurrentSalaryValue(DateTime? date = null)
        {
            if (date == DateTime.MinValue || date == null)
                return PayrollSalaries.Where(x => !x.Deleted && x.ApplyDate.Value <= DateTime.Now.Date).OrderByDescending(x => x.ApplyDate).FirstOrDefault() == null ? 0 :
                Math.Round(PayrollSalaries.Where(x => !x.Deleted && x.ApplyDate.Value <= DateTime.Now.Date).OrderByDescending(x => x.ApplyDate).FirstOrDefault().NetIncome / 2, 2);
            else
                return PayrollSalaries.Where(x => !x.Deleted && x.ApplyDate.Value <= date.Value.Date).OrderByDescending(x => x.ApplyDate).FirstOrDefault() == null ? 0 :
                Math.Round(PayrollSalaries.Where(x => !x.Deleted && x.ApplyDate.Value <= date.Value.Date).OrderByDescending(x => x.ApplyDate).FirstOrDefault().NetIncome / 2, 2);
        }

        public virtual JobCatalog GetCurrentJob(DateTime? date = null)
        {
            if (date == DateTime.MinValue || date == null)
                return JobCatalogs.Where(x => !x.Deleted && x.ApplyDate <= DateTime.Now.Date).OrderByDescending(x => x.ApplyDate).FirstOrDefault()?.JobCatalog;
            else
                return JobCatalogs.Where(x => !x.Deleted && x.ApplyDate <= date.Value.Date).OrderByDescending(x => x.ApplyDate).FirstOrDefault()?.JobCatalog;
        }
    }
}
