using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.BiweeklyPayments;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Models.BiweeklyPayment
{
    public class CreateOrUpdateModel
    {
        public CreateOrUpdateModel()
        {
            PayrollEmployee = new Domain.PayrollEmployees.PayrollEmployee();
            PayrollSalary = new PayrollSalary();
            BiweeklyPaymentFiles = new List<BiweeklyPaymentFile>();
            AddBiweeklyPaymentFile = new AddBiweeklyPaymentFile();
            IncidentsBiweek = new List<string>();
            IncidentsPending = new List<string>();
            BonusesBiweek = new List<string>();
        }

        public virtual int Id { get; set; }
        public bool Paid { get; set; }
        public virtual string Date { get; set; }
        public virtual string EmployeeName { get; set; }
        public virtual decimal NetIncome { get; set; }
        public virtual decimal BonusesTotal { get; set; }
        public virtual decimal IncidentsTotal { get; set; }
        public virtual decimal Total { get; set; }

        public virtual decimal OriginalPayment { get; set; }
        public virtual DateTime Payday { get; set; }

        public virtual int CustomerThatReportedId { get; set; }

        public virtual int PayrollEmployeeId { get; set; }
        public virtual Domain.PayrollEmployees.PayrollEmployee PayrollEmployee { get; set; }

        public virtual int PayrollSalaryId { get; set; }
        public virtual PayrollSalary PayrollSalary { get; set; }

        public virtual List<BiweeklyPaymentFile> BiweeklyPaymentFiles { get; set; }

        public virtual AddBiweeklyPaymentFile AddBiweeklyPaymentFile { get; set; }
        public virtual SelectList Types { get; set; }

        [NopResourceDisplayName("Tipo de archivo")]
        public virtual int Type { get; set; }

        public virtual List<string> IncidentsBiweek { get; set; }
        public virtual List<string> IncidentsPending { get; set; }

        public virtual List<string> BonusesBiweek { get; set; }

        public virtual bool CannotBePaid { get; set; }
    }

    public partial class AddBiweeklyPaymentFile
    {
        public virtual IFormFile File { get; set; }
        public virtual PaymentFileType Type { get; set; }
        public virtual decimal OriginalPayment { get; set; }
        public virtual string Payday { get; set; }

        public virtual int CustomerThatReportedId { get; set; }

        public virtual int PayrollSalaryId { get; set; }
        public virtual int PayrollEmployeeId { get; set; }

        public virtual int BiweeklyPaymentId { get; set; }
    }

    public partial class MarkPiad
    {
        public virtual bool Paid { get; set; }
        public virtual decimal OriginalPayment { get; set; }
        public virtual string Payday { get; set; }

        public virtual int CustomerThatReportedId { get; set; }

        public virtual int PayrollSalaryId { get; set; }
        public virtual int PayrollEmployeeId { get; set; }

        public virtual int BiweeklyPaymentId { get; set; }
    }
}
