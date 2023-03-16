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
    public class PayrollEmployeesAdditionalModel
    {
        public PayrollEmployeesAdditionalModel()
        {
            MissingPayrolls = new List<MissingPayrollEmployee>();
            DuplicatedCustomerPayrollEmployees = new List<DuplicatedCustomerPayrollEmployee>();
            InTrialPeriodEmployees = new List<InTrialPeriodEmployee>();
        }

        public List<MissingPayrollEmployee> MissingPayrolls { get; set; }
        public List<DuplicatedCustomerPayrollEmployee> DuplicatedCustomerPayrollEmployees { get; set; }
        public List<InTrialPeriodEmployee> InTrialPeriodEmployees { get; set; }
    }

    public class MissingPayrollEmployee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class DuplicatedCustomerPayrollEmployee
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
    }

    public class InTrialPeriodEmployee
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime TrialPeriodEndDate { get; set; }
        public int DaysLeft { get; set; }
    }
}
