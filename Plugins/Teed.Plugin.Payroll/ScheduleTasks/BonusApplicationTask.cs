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
using Teed.Plugin.Payroll.Helpers;
using Teed.Plugin.Payroll.Models.Assistance;
using Teed.Plugin.Payroll.Services;

namespace Teed.Plugin.Payroll.ScheduleTasks
{
    public class BonusApplicationTask : IScheduleTask
    {
        private readonly BonusService _bonusService;
        private readonly BonusApplicationService _bonusApplicationService;
        private readonly JobCatalogService _jobCatalogService;
        private readonly PayrollEmployeeService _payrollEmployeeService;
        private readonly IncidentService _incidentService;
        private readonly AssistanceService _assistanceService;

        public BonusApplicationTask(BonusService bonusService, BonusApplicationService bonusApplicationService,
            JobCatalogService jobCatalogService, PayrollEmployeeService payrollEmployeeService,
            IncidentService incidentService, AssistanceService assistanceService)
        {
            _bonusService = bonusService;
            _bonusApplicationService = bonusApplicationService;
            _jobCatalogService = jobCatalogService;
            _payrollEmployeeService = payrollEmployeeService;
            _incidentService = incidentService;
            _assistanceService = assistanceService;
        }

        public void Execute()
        {
            var doneCount = BonusApplicationHelper.InsertNewBonusApplications(DateTime.Now.AddMonths(-3).Date, DateTime.Now.Date,
                _bonusService, _bonusApplicationService, _jobCatalogService, _payrollEmployeeService, _incidentService, _assistanceService);
        }
    }
}
