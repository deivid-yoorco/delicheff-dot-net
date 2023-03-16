using Nop.Services.Customers;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;
using Teed.Plugin.Payroll.Models.Assistance;
using Teed.Plugin.Payroll.Services;

namespace Teed.Plugin.Payroll.ScheduleTasks
{
    public class IncidentsRegisterTask : IScheduleTask
    {
        private readonly AssistanceService _assistanceService;
        private readonly PayrollEmployeeService _payrollEmployeeService;

        public IncidentsRegisterTask(AssistanceService assistanceService,
            PayrollEmployeeService payrollEmployeeService)
        {
            _assistanceService = assistanceService;
            _payrollEmployeeService = payrollEmployeeService;
        }

        public void Execute()
        {
            var employees = _payrollEmployeeService.GetAll().Where(x => x.EmployeeStatusId != (int)EmployeeStatus.Discharged).ToList();
            var inserted = _assistanceService.IncidentsRegister(employees);
        }
    }
}
