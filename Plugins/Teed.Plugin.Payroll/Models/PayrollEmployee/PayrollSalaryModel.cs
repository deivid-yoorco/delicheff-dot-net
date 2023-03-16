using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Models.PayrollEmployee
{
    public class PayrollSalaryModel
    {
        public virtual int Id { get; set; }
        public virtual DateTime ApplyDate { get; set; }
        public virtual string ApplyDateString { get; set; }
        public virtual decimal NetIncome { get; set; }
        public virtual decimal WithheldISR { get; set; }
        public virtual decimal SocialSecurityContributions { get; set; }
        public virtual decimal Bonds { get; set; }
        public virtual decimal Benefits { get; set; }
        public virtual decimal IntegratedDailyWage { get; set; }
        public virtual int PayrollEmployeeId { get; set; }
    }
}
