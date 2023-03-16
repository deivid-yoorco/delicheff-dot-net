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
using Teed.Plugin.Payroll.Domain.PayrollEmployeeJobs;

namespace Teed.Plugin.Payroll.Controllers
{
    [Area(AreaNames.Admin)]
    public class PayrollEmployeeJobController : BasePluginController
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
        private readonly PayrollEmployeeJobService _payrollEmployeeJobService;

        public PayrollEmployeeJobController(IPermissionService permissionService, PayrollEmployeeService payrollEmployeeService,
            IncidentService incidentService, IWorkContext workContext, AssistanceService assistanceService,
            JobCatalogService jobCatalogService, IScheduleTaskService scheduleTaskService,
            PayrollEmployeeFileService payrollEmployeeFileService, PayrollSalaryService payrollSalaryService,
            ICustomerService customerService, PayrollEmployeeJobService payrollEmployeeJobService)
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
            _payrollEmployeeJobService = payrollEmployeeJobService;
        }

        [HttpGet]
        public IActionResult ApplyFirstInsert(string key)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.JobCatalog))
                return BadRequest();

            if (key != "teed1234")
                return BadRequest();

            var jobConections = _payrollEmployeeJobService.GetAll().ToList();
            foreach (var jobConection in jobConections)
                _payrollEmployeeJobService.Delete(jobConection);

            var payrollEmployees = _payrollEmployeeService.GetAll(includeExEmployees: true)
                .ToList()
                .Select(x => new { x.JobCatalogId, x.Id, x.DateOfAdmission })
                .Where(x => x.JobCatalogId > 0).ToList();

            var applyDate = new DateTime(2000, 1, 1).Date;
            foreach (var payrollEmployee in payrollEmployees)
            {
                _payrollEmployeeJobService.Insert(new PayrollEmployeeJob
                {
                    JobCatalogId = payrollEmployee.JobCatalogId ?? 0,
                    PayrollEmployeeId = payrollEmployee.Id,
                    ApplyDate = payrollEmployee.DateOfAdmission != null ? payrollEmployee.DateOfAdmission.Value : applyDate,
                    Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - Se insertó de forma automática la conexión empleado-empleo por el usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}).\n"
                });
            }

            return Ok(payrollEmployees.Count());
        }

        [HttpPost]
        public IActionResult EmployeeJobList(int payrollId)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            var employee = _payrollEmployeeService.GetById(payrollId);

            var jobConections = _payrollEmployeeJobService.GetByPayrollEmployeeId(payrollId)
                .OrderByDescending(x => x.ApplyDate).ToList();
            var jobCatalogIds = jobConections.Select(x => x.JobCatalogId).Distinct().ToList();
            var jobCatalogs = _jobCatalogService.GetAll()
                .Where(x => jobCatalogIds.Contains(x.Id)).ToList();

            var currentJob = employee.GetCurrentJob();
            var gridModel = new DataSourceResult
            {
                Data = jobConections.Select(x => new
                {
                    x.Id,
                    ApplyDate = x.ApplyDate.ToString("dd/MM/yyyy"),
                    JobName = jobCatalogs.Where(y => y.Id == x.JobCatalogId).FirstOrDefault().Name,
                    IsCurrent = (currentJob?.Id ?? 0) == x.JobCatalogId
                }).ToList(),
                Total = jobConections.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult EmployeeJobAdd(AddEmployeeJob model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            try
            {
                var jobCatalog = _jobCatalogService.GetById(model.JobCatalogId);
                if (jobCatalog == null)
                    return BadRequest("Job not found with given Id");
                var jobConection = new PayrollEmployeeJob
                {
                    ApplyDate = model.ApplyDate ?? DateTime.Now,
                    PayrollEmployeeId = model.PayrollEmployeeId,
                    JobCatalogId = jobCatalog.Id,
                    Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) insertó el empleo \"{jobCatalog.Name}\" ({jobCatalog.Id}) a este empleado.\n"
                };
                _payrollEmployeeJobService.Insert(jobConection);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        public IActionResult EmployeeJobDelete(AddEmployeeJob model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            var jobConection = _payrollEmployeeJobService.GetById(model.Id);
            if (jobConection == null)
                return Ok();

            var jobCatalog = _jobCatalogService.GetById(jobConection.JobCatalogId);
            if (jobCatalog == null)
                return BadRequest("Job not found with given Id");

            jobConection.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) eliminó el empleo \"{jobCatalog.Name}\" ({jobCatalog.Id}) a este empleado.\n";
            _payrollEmployeeJobService.Delete(jobConection);
            return Ok();
        }
    }
}