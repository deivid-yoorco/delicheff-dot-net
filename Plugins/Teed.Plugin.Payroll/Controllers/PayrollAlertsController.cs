using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Security;
using Teed.Plugin.Payroll.Services;
using Teed.Plugin.Payroll.Models.PayrollEmployee;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;
using Nop.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Teed.Plugin.Payroll.Models.Incident;
using Teed.Plugin.Payroll.Domain.Incidents;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Teed.Plugin.Payroll.Domain.MinimumWagesCatalogs;
using System.Data.Entity;
using Teed.Plugin.Payroll.Models.Assistance;
using Teed.Plugin.Payroll.Domain.Assistances;
using Nop.Services.Tasks;
using OfficeOpenXml;
using Teed.Plugin.Payroll.Models.PayrollAlerts;
using Nop.Core.Domain.Customers;
using Teed.Plugin.Payroll.Helpers;

namespace Teed.Plugin.Payroll.Controllers
{
    [Area(AreaNames.Admin)]
    public class PayrollAlertsController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly PayrollEmployeeService _payrollEmployeeService;
        private readonly IncidentService _incidentService;
        private readonly IWorkContext _workContext;
        private readonly AssistanceService _assistanceService;
        private readonly JobCatalogService _jobCatalogService;
        private readonly PayrollEmployeeFileService _payrollEmployeeFileService;
        private readonly PayrollSalaryService _payrollSalaryService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ICustomerService _customerService;
        private readonly AssistanceOverrideService _assistanceOverrideService;

        public PayrollAlertsController(IPermissionService permissionService, PayrollEmployeeService payrollEmployeeService,
            IncidentService incidentService, IWorkContext workContext, AssistanceService assistanceService,
            JobCatalogService jobCatalogService, IScheduleTaskService scheduleTaskService,
            PayrollEmployeeFileService payrollEmployeeFileService, PayrollSalaryService payrollSalaryService,
            ICustomerService customerService, AssistanceOverrideService assistanceOverrideService)
        {
            _permissionService = permissionService;
            _payrollEmployeeService = payrollEmployeeService;
            _incidentService = incidentService;
            _workContext = workContext;
            _assistanceService = assistanceService;
            _jobCatalogService = jobCatalogService;
            _scheduleTaskService = scheduleTaskService;
            _payrollEmployeeFileService = payrollEmployeeFileService;
            _payrollSalaryService = payrollSalaryService;
            _customerService = customerService;
            _assistanceOverrideService = assistanceOverrideService;
        }

        public IActionResult Index()
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollAlerts))
                return AccessDeniedView();

            var model = new PayrollAlertsModel();
            var employees = _payrollEmployeeService.GetAll().Where(x => x.EmployeeStatusId != (int)EmployeeStatus.Discharged).ToList().OrderBy(x => x.GetFullName()).ToList();
            var existingEmployees =
                _assistanceService.CheckEmployeeNumbersExistence(employees.Select(x => x.EmployeeNumber)
                .ToList()).ToList();
            var existingEmployeeNumbers = existingEmployees.Select(x => x.BadgeNumber).ToList();
            var employeeNumbersDontExist =
                employees.Where(x => x.EmployeeNumber < 1 ||
                !existingEmployeeNumbers.Contains(_assistanceService.FormatEmployeeNumber(x.EmployeeNumber)))
                .ToList();
            var employeesNoSalary =
                employees.Where(x => !x.PayrollSalaries.Any())
                .ToList();

            // Incomplete Employees
            model.IncompleteEmployees = GetPendingInfo(employees);
            //

            // Employees not in checker
            model.EmployeesNoChecker = employeeNumbersDontExist.Select(x => new EmployeeNoChecker
            {
                Id = x.Id,
                FullName = x.GetFullName()
            }).ToList();
            //

            // Jobs without schedule
            var jobs = _jobCatalogService.GetAll().Where(x => string.IsNullOrEmpty(x.WorkSchedule)).ToList();
            model.JobNoSchedules = jobs.Select(x => new JobNoSchedule
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
            //

            // Upcoming birthdays
            var today = DateTime.Now.Date;
            var twoMonthsFromNow = DateTime.Now.Date.AddMonths(2);
            var datesBetween = Enumerable.Range(0, 1 + twoMonthsFromNow.Subtract(today).Days)
              .Select(offset => today.AddDays(offset))
              .ToList();
            model.UpcomingBirthdays = employees
                .Where(x => x.DateOfBirth != null && datesBetween.Contains(new DateTime(today.Year, x.DateOfBirth.Value.Date.Month, x.DateOfBirth.Value.Date.Day)))
                .Select(x => new UpcomingBirthday
                {
                    Name = x.GetFullName(),
                    Birthday = x.DateOfBirth.Value
                }).ToList();
            //

            // Discharged without dates
            model.DischargedWithoutDates = _payrollEmployeeService.GetAll(includeExEmployees: true)
                .Where(x => x.EmployeeStatusId == (int)EmployeeStatus.Discharged && x.DateOfDeparture == null)
                .ToList().OrderBy(x => x.GetFullName()).ToList()
                .Select(x => new DischargedWithoutDate
                {
                    Id = x.Id,
                    Name = x.GetFullName(),
                }).ToList();
            //

            // ExEmployees still with active payroll
            var activeEmployee = _payrollEmployeeService.GetAll(includeExEmployees: true)
                .Where(x => x.EmployeeStatusId == (int)EmployeeStatus.Active)
                .ToList();
            var activeCustomerIds = activeEmployee.Select(x => x.CustomerId).Distinct().ToList();
            var ExEmployeeIds = _customerService.GetAllCustomersQuery()
                    .Where(x => activeCustomerIds.Contains(x.Id))
                    .ToList()
                    .Where(x => x.IsInCustomerRole("exemployee"))
                    .Select(x => x.Id).ToList();
            activeEmployee = activeEmployee.Where(x => ExEmployeeIds.Contains(x.CustomerId)).ToList();

            model.ExEmployeeAndActives = activeEmployee
                .Select(x => new ExEmployeeAndActive
                {
                    Id = x.Id,
                    Name = x.GetFullName(),
                }).ToList();
            //

            // Discharged without satisfactory departure process
            model.DischargedWithoutSatisfactoryDepartureProcesses = _payrollEmployeeService.GetAll(includeExEmployees: true)
                .Where(x => x.EmployeeStatusId == (int)EmployeeStatus.Discharged && !(x.SatisfactoryDepartureProcess ?? false))
                .ToList().OrderBy(x => x.GetFullName()).ToList()
                .Select(x => new DischargedWithoutSatisfactoryDepartureProcess
                {
                    Id = x.Id,
                    Name = x.GetFullName(),
                }).ToList();
            //

            // Justifiably dismiss employees when accumulating 4 absences in less than 30 days
            var today30DaysAgo = DateTime.Now.AddDays(-30);
            var dates = new List<DateOfEmployee>();
            var assistancesModel = AssistanceHelper.DoAssistanceModelWork(dates, employees, DateTime.Now,
                _assistanceService, _assistanceOverrideService, _customerService,
                _jobCatalogService, true);
            var datesOfEmployees = employees
            .Where(x => !CustomerIsParter(x)).Select(x => new
            {
                x.Id,
                Name = x.GetFullName(),
                DatesOfAbscences = assistancesModel.Item1.Where(y => y.CustomerId == x.CustomerId &&
                    today30DaysAgo <= y.Date && y.Assistance == (int)AssistanceType.Absence)
                    .OrderByDescending(y => y.Date).ToList()
            }).ToList();
            model.JustifiablyDismissEmployee4AbcensesLast30Days = datesOfEmployees.Where(x => x.DatesOfAbscences.Count() >= 4)
                .Select(x => new JustifiablyDismissEmployee4AbcensesLast30Days
                {
                    Id = x.Id,
                    Name = x.Name,
                    AbscensesCount = x.DatesOfAbscences.Count()
                }).ToList();
            //

            return View("~/Plugins/Teed.Plugin.Payroll/Views/PayrollAlerts/Index.cshtml", model);
        }

        private List<IncompleteEmployee> GetPendingInfo(List<PayrollEmployee> payrollEmployees)
        {
            var final = new List<IncompleteEmployee>();
            foreach (var payrollEmployee in payrollEmployees)
            {
                var incomplete = new List<string>();
                // Check if Partner
                var isPartner = CustomerIsParter(payrollEmployee);

                // Files
                var currentFiles = _payrollEmployeeFileService.GetAll().Where(x => x.PayrollEmployeeId == payrollEmployee.Id).Select(x => x.FileTypeId).Distinct().ToList();
                if (isPartner)
                    if (!currentFiles.Contains((int)FileType.SignedContract))
                        currentFiles.Add((int)FileType.SignedContract);

                var files = Enum.GetValues(typeof(FileType)).Cast<FileType>()
                    .Where(x => !FileTypes.OptionalFileTypes.Contains(x));
                if (payrollEmployee.PayrollContractTypeId == (int)PayrollContractType.OperatingShareholder ||
                    payrollEmployee.PayrollContractTypeId == (int)PayrollContractType.ServiceProvider ||
                    payrollEmployee.PayrollContractTypeId == (int)PayrollContractType.Sporadic)
                    files = files.Where(x => !FileTypes.ExtraOptionalFileTypes.Contains(x));
                incomplete.AddRange(files.Where(x => !currentFiles.Contains((int)x)).Select(x => x.GetDisplayName()));

                if (currentFiles.Where(x => x == (int)FileType.EmploymentTerminationDocument).FirstOrDefault() == 0 &&
                    (payrollEmployee.EmployeeStatusId == (int)EmployeeStatus.Discharged ||
                    payrollEmployee.DateOfDeparture.HasValue))
                {
                    incomplete.Add(FileType.EmploymentTerminationDocument.GetDisplayName());
                }

                // JobCatalgo
                if (payrollEmployee.GetCurrentJob() == null)
                    incomplete.Add("Sin empleo especificado");

                // Salary
                if (!isPartner)
                    if (!payrollEmployee.PayrollSalaries.Any())
                        incomplete.Add("Sin salario especificado");

                // Info
                if (string.IsNullOrEmpty(payrollEmployee.FirstNames))
                    incomplete.Add("Nombre/s");

                if (string.IsNullOrEmpty(payrollEmployee.LastName) || string.IsNullOrEmpty(payrollEmployee.MiddleName))
                    incomplete.Add("Apellido/s");

                if (!payrollEmployee.DateOfBirth.HasValue)
                    incomplete.Add("Fecha de nacimiento");

                if (!payrollEmployee.DateOfAdmission.HasValue)
                    incomplete.Add("Fecha de admisión");

                if (!payrollEmployee.DateOfDeparture.HasValue && payrollEmployee.EmployeeStatusId == (int)EmployeeStatus.Discharged)
                    incomplete.Add("Fecha de salida");

                if (!(payrollEmployee.SatisfactoryDepartureProcess ?? false) && payrollEmployee.EmployeeStatusId == (int)EmployeeStatus.Discharged)
                    incomplete.Add("Proceso de baja satisfactorio");

                if (string.IsNullOrEmpty(payrollEmployee.Address))
                    incomplete.Add("Dirección");

                if (string.IsNullOrEmpty(payrollEmployee.Cellphone))
                    incomplete.Add("Teléfono celular");

                if (string.IsNullOrEmpty(payrollEmployee.Landline))
                    incomplete.Add("Teléfono fijo");

                if (string.IsNullOrEmpty(payrollEmployee.ReferenceOneName) || string.IsNullOrEmpty(payrollEmployee.ReferenceOneContact))
                    incomplete.Add("Referencia uno");

                if (string.IsNullOrEmpty(payrollEmployee.ReferenceTwoName) || string.IsNullOrEmpty(payrollEmployee.ReferenceTwoContact))
                    incomplete.Add("Referencia dos");

                if (string.IsNullOrEmpty(payrollEmployee.ReferenceThreeName) || string.IsNullOrEmpty(payrollEmployee.ReferenceThreeContact))
                    incomplete.Add("Referencia tres");

                if (string.IsNullOrEmpty(payrollEmployee.IMSS))
                    incomplete.Add("IMSS");

                if (string.IsNullOrEmpty(payrollEmployee.RFC))
                    incomplete.Add("RFC");

                if (string.IsNullOrEmpty(payrollEmployee.CURP))
                    incomplete.Add("CURP");

                if (string.IsNullOrEmpty(payrollEmployee.Clabe))
                    incomplete.Add("CLABE");

                if (string.IsNullOrEmpty(payrollEmployee.AccountNumber))
                    incomplete.Add("Número de cuenta");

                if (payrollEmployee.EmployeeNumber == 0)
                    incomplete.Add("Número de empleado");

                if (payrollEmployee.CustomerId == 0)
                    incomplete.Add("Usuario vinculado al empleado");

                if (incomplete.Any())
                    final.Add(new IncompleteEmployee
                    {
                        Id = payrollEmployee.Id,
                        FullName = payrollEmployee.GetFullName() + $" ({payrollEmployee.GetCurrentJob()?.Name ?? "SIN EMPLEO ESPECIFICADO"})",
                        PendingInformation = incomplete
                    });
            }

            return final;
        }

        protected bool CustomerIsParter(PayrollEmployee payrollEmployee)
        {
            var partnerRole = _customerService.GetCustomerRoleBySystemName("Partner");
            var customer = _customerService.GetCustomerById(payrollEmployee.CustomerId);
            if (customer != null)
                return customer.GetCustomerRoleIds().Contains(partnerRole.Id);
            else
                return false;
        }
    }
}