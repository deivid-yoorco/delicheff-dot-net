using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.Bonuses;
using Teed.Plugin.Payroll.Domain.Incidents;
using Teed.Plugin.Payroll.Domain.JobCatalogs;
using Teed.Plugin.Payroll.Domain.PayrollEmployeeJobs;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;
using Teed.Plugin.Payroll.Models.Assistance;
using Teed.Plugin.Payroll.Models.BiweeklyPayment;
using Teed.Plugin.Payroll.Models.FestiveDate;
using Teed.Plugin.Payroll.Services;

namespace Teed.Plugin.Payroll.Helpers
{
    public static class PayrollEmployeeJobHelper
    {
        public static PayrollEmployeeJob GetCurrentJobConectionOfEmployee(PayrollEmployee payrollEmployee, DateTime dateTime,
            PayrollEmployeeJobService _payrollEmployeeJobService)
        {
            var jobConection = _payrollEmployeeJobService.GetByPayrollEmployeeId(payrollEmployee.Id)
                .Where(x => x.ApplyDate <= dateTime).OrderByDescending(x => x.ApplyDate).FirstOrDefault();
            return jobConection;
        }
    }
}
