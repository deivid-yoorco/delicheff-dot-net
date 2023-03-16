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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Teed.Plugin.Payroll.Models;
using Teed.Plugin.Payroll.Models.BiweeklyPayment;
using Teed.Plugin.Payroll.Helpers;
using System.Drawing;
using OfficeOpenXml.Style;

namespace Teed.Plugin.Payroll.Controllers
{
    [Area(AreaNames.Admin)]
    public class AssistanceController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly PayrollEmployeeService _payrollEmployeeService;
        private readonly IncidentService _incidentService;
        private readonly IWorkContext _workContext;
        private readonly AssistanceService _assistanceService;
        private readonly JobCatalogService _jobCatalogService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ICustomerService _customerService;
        private readonly PayrollEmployeeJobService _payrollEmployeeJobService;
        private readonly AssistanceOverrideService _assistanceOverrideService;

        public AssistanceController(IPermissionService permissionService, PayrollEmployeeService payrollEmployeeService,
            IncidentService incidentService, IWorkContext workContext, AssistanceService assistanceService,
            JobCatalogService jobCatalogService, IScheduleTaskService scheduleTaskService,
            ICustomerService customerService, PayrollEmployeeJobService payrollEmployeeJobService,
            AssistanceOverrideService assistanceOverrideService)
        {
            _permissionService = permissionService;
            _payrollEmployeeService = payrollEmployeeService;
            _incidentService = incidentService;
            _workContext = workContext;
            _assistanceService = assistanceService;
            _jobCatalogService = jobCatalogService;
            _scheduleTaskService = scheduleTaskService;
            _customerService = customerService;
            _payrollEmployeeJobService = payrollEmployeeJobService;
            _assistanceOverrideService = assistanceOverrideService;
        }

        [HttpGet]
        public IActionResult TestAssistanceDatesPrint(int Id)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Assistance))
                return AccessDeniedView();

            var employee = _payrollEmployeeService.GetByCustomerId(Id);
            if (employee == null || employee?.EmployeeStatusId == (int)EmployeeStatus.Discharged)
                return BadRequest("No employee found with given Id");

            var jobOfEmployee = _jobCatalogService.GetById(employee.GetCurrentJob()?.Id ?? 0);
            if (jobOfEmployee == null || string.IsNullOrEmpty(jobOfEmployee.WorkSchedule))
                return BadRequest("Employee has no job assigned");

            var model = new List<DateOfEmployee>();
            var tuple = new Tuple<List<DateOfEmployee>, List<string>>(model, new List<string>());
            tuple = AssistanceHelper.DoAssistanceModelWork(model, new List<PayrollEmployee> { employee }, DateTime.Now,
                _assistanceService, _assistanceOverrideService, _customerService, _jobCatalogService, true);
            model = tuple.Item1.OrderByDescending(x => x.Date).ToList();

            var festiveDates = _assistanceService.GetFestiveDatesFromGroceries()
                .Where(x => !x.DontApplyToPayroll).ToList();
            var yearlyDates = festiveDates.Where(x => x.AppliesYearly).Select(x => x.Date).OrderBy(x => x).ToList();
            var fixedDates = festiveDates.Where(x => !x.AppliesYearly).Select(x => x.Date).OrderBy(x => x).ToList();

            return Json(new
            {
                YearlyFestiveDates = yearlyDates,
                FixedFestiveDates = fixedDates,
                Data = model,
            });
        }

        //protected Tuple<List<DateOfEmployee>, List<string>> DoAssistanceModelWork(List<DateOfEmployee> model, List<PayrollEmployee> employees, DateTime today,
        //    bool useWithTolerance = false)
        //{
        //    var festiveDates = _assistanceService.GetFestiveDatesFromGroceries()
        //        .Where(x => !x.DontApplyToPayroll).ToList();
        //    var yearlyDates = festiveDates.Where(x => x.AppliesYearly).Select(x => x.Date).ToList();
        //    var fixedDates = festiveDates.Where(x => !x.AppliesYearly).Select(x => x.Date).ToList();
        //    var err = new List<string>();
        //    var customerIds = employees.Select(x => x.CustomerId).ToList();
        //    var customers = _customerService.GetAllCustomersQuery().Where(x => customerIds.Contains(x.Id)).ToList();
        //    foreach (var employee in employees)
        //    {
        //        var customer = customers.Where(x => x.Id == employee.CustomerId).FirstOrDefault();
        //        var jobOfEmployee = _jobCatalogService.GetById(employee.JobCatalogId ?? 0);
        //        if (jobOfEmployee != null && !string.IsNullOrEmpty(jobOfEmployee.WorkSchedule))
        //        {
        //            var assistances = _assistanceService.GetByEmployeeNumber(employee.EmployeeNumber)
        //                .Where(x => !yearlyDates.Any(y => y.Day == x.CheckIn.Day && y.Month == x.CheckIn.Month) &&
        //                !fixedDates.Contains(x.CheckIn)).GroupBy(x => x.CheckIn.Date);
        //            var workingDaysOfWeek = _assistanceService.GetScheduleOfEmployee(jobOfEmployee.WorkSchedule);

        //            foreach (var assistancesOfDay in assistances)
        //            {
        //                var date = assistancesOfDay.Key.Date;
        //                var currentSchedule = workingDaysOfWeek
        //                    .Where(x => x.DayOfWeek == date.DayOfWeek).FirstOrDefault();
        //                if (currentSchedule != null)
        //                {
        //                    var assistance = AssistanceType.Absence;
        //                    var checkInOfCurrentDay = new DateTime(date.Year, date.Month, date.Day, currentSchedule.CheckIn, useWithTolerance ? 15 : 0, 0);
        //                    var checkOutOfCurrentDay = new DateTime(date.Year, date.Month, date.Day, currentSchedule.CheckOut, 0, 0);

        //                    if (assistancesOfDay.Where(x => x.CheckIn <= checkInOfCurrentDay).Any())
        //                        assistance = AssistanceType.InTime;
        //                    else if (assistancesOfDay.Where(x => x.CheckIn > checkInOfCurrentDay).Any())
        //                        assistance = AssistanceType.Delay;
        //                    else if (assistancesOfDay.Where(x => !(x.CheckIn <= checkInOfCurrentDay || x.CheckIn > checkInOfCurrentDay)).Any())
        //                        assistance = AssistanceType.Absence;

        //                    var dateOfEmployee = new DateOfEmployee
        //                    {
        //                        CustomerId = employee.CustomerId,
        //                        EmployeeNumber = employee.EmployeeNumber,
        //                        Name = !string.IsNullOrWhiteSpace(employee.GetFullName()) && !string.IsNullOrEmpty(employee.GetFullName()) ? employee.GetFullName() :
        //                            customer != null ? customer.GetFullName() : "Nombre no especificado",
        //                        Date = date,
        //                        Assistance = (int)assistance,
        //                        CheckIn = checkInOfCurrentDay,
        //                        CheckOut = checkOutOfCurrentDay,
        //                        TimesRegistred = assistancesOfDay != null ?
        //                            assistancesOfDay.Select(x => x.CheckIn).OrderBy(x => x).ToList() :
        //                            new List<DateTime>(),
        //                        ExtraInfo = "[From assistances]"
        //                    };
        //                    model.Add(dateOfEmployee);
        //                }
        //                else
        //                    err.Add("No schedule found - " + employee.GetFullName());
        //            }

        //            var dateCheckerStartedWorking = new DateTime(2020, 9, 12).Date;
        //            var daysOfWork = workingDaysOfWeek.Select(x => x.DayOfWeek).ToList();
        //            var dates = model.Any() ? model.OrderByDescending(x => x.Date).Select(x => x.Date.Date).ToList() : new List<DateTime>();
        //            var firstAssistance =
        //                ((employee.DateOfAdmission < dateCheckerStartedWorking || employee.DateOfAdmission == null ? dateCheckerStartedWorking : employee.DateOfAdmission) ?? DateTime.Now).Date;
        //            var lastAssitance = today;

        //            var datesBetween = Enumerable.Range(0, 1 + lastAssitance.Subtract(firstAssistance).Days)
        //              .Select(offset => DateTime.ParseExact(firstAssistance.AddDays(offset).ToString("dd-MM-yyyy"), "dd-MM-yyyy", CultureInfo.InvariantCulture))
        //              .Where(x => !dates.Contains(x.Date) && daysOfWork.Contains(x.DayOfWeek))
        //              .ToList();
        //            if (datesBetween.Where(x => x == today.Date).Any() &&
        //                workingDaysOfWeek.Where(x => x.DayOfWeek == today.DayOfWeek).Any())
        //            {
        //                var nowSchedule = workingDaysOfWeek.Where(x => x.DayOfWeek == today.DayOfWeek).FirstOrDefault();
        //                var nowCheckIn = new DateTime(today.Year, today.Month, today.Day, nowSchedule.CheckIn, 0, 0);
        //                if (today < nowCheckIn)
        //                    datesBetween.RemoveAt(datesBetween.IndexOf(today.Date));
        //            }

        //            model.AddRange(datesBetween.Where(x =>
        //              !yearlyDates.Any(y => y.Day == x.Date.Day && y.Month == x.Date.Month) &&
        //              !fixedDates.Contains(x.Date))
        //                .Select(x => new DateOfEmployee
        //                {
        //                    CustomerId = employee.CustomerId,
        //                    EmployeeNumber = employee.EmployeeNumber,
        //                    Name = !string.IsNullOrWhiteSpace(employee.GetFullName()) && !string.IsNullOrEmpty(employee.GetFullName()) ? employee.GetFullName() :
        //                    customer != null ? customer.GetFullName() : "Nombre no especificado",
        //                    Date = x,
        //                    Assistance = (int)AssistanceType.Absence,
        //                    CheckIn = new DateTime(x.Year, x.Month, x.Day, workingDaysOfWeek
        //                .Where(y => y.DayOfWeek == x.DayOfWeek).FirstOrDefault().CheckIn, 0, 0),
        //                    CheckOut = new DateTime(x.Year, x.Month, x.Day, workingDaysOfWeek
        //                .Where(y => y.DayOfWeek == x.DayOfWeek).FirstOrDefault().CheckOut, 0, 0),
        //                    TimesRegistred = new List<DateTime>(),
        //                    ExtraInfo = $"[From betweens] - {{is festive: {yearlyDates.Any(y => y.Day == x.Date.Day && y.Month == x.Date.Month) || fixedDates.Contains(x.Date)}}}"
        //                }));
        //        }
        //        else
        //            err.Add("No job or without schedule - " + employee.GetFullName());
        //    }
        //    return new Tuple<List<DateOfEmployee>, List<string>>(model, err);
        //}

        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Assistance))
                return AccessDeniedView();

            var dontExist = new List<string>();

            var employees = _payrollEmployeeService.GetAll().Where(x => x.EmployeeStatusId != (int)EmployeeStatus.Discharged).ToList().OrderBy(x => x.GetFullName()).ToList();
            var existingEmployees =
                _assistanceService.CheckEmployeeNumbersExistence(employees.Select(x => x.EmployeeNumber)
                .ToList()).ToList();
            var existingEmployeeNumbers = existingEmployees.Select(x => x.BadgeNumber).ToList();
            var employeeNumbersDontExist =
                employees.Where(x => x.EmployeeNumber < 1 ||
                !existingEmployeeNumbers.Contains(_assistanceService.FormatEmployeeNumber(x.EmployeeNumber)))
                .ToList();

            if (employeeNumbersDontExist.Any())
                dontExist = employeeNumbersDontExist.Select(x => $"{x.GetFullName()} ({(x.EmployeeNumber > 0 ? x.EmployeeNumber.ToString() : "Sin número especificado")})").ToList();

            return View("~/Plugins/Teed.Plugin.Payroll/Views/Assistance/List.cshtml", dontExist);
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Assistance))
                return AccessDeniedView();

            var employees = _payrollEmployeeService.GetAll(includeExEmployees: true)
                .ToList().OrderBy(x => x.GetFullName()).ToList();
            var existingEmployees =
                _assistanceService.CheckEmployeeNumbersExistence(employees.Select(x => x.EmployeeNumber)
                .ToList()).ToList();
            var existingEmployeeNumbers = existingEmployees.Select(x => x.BadgeNumber).ToList();

            var gridModel = new DataSourceResult
            {
                Data = employees.Select(x => new
                {
                    x.EmployeeNumber,
                    x.Id,
                    x.CustomerId,
                    FullName = x.GetFullName() + (x.EmployeeStatusId == (int)EmployeeStatus.Discharged ? " (Baja)" : string.Empty),
                    NameInChecker =
                        existingEmployees.Where(y => y.BadgeNumber == _assistanceService.FormatEmployeeNumber(x.EmployeeNumber))
                        .FirstOrDefault()?.Name,
                    ExistsInChecker =
                        existingEmployeeNumbers
                        .Contains(_assistanceService.FormatEmployeeNumber(x.EmployeeNumber))
                }).ToList(),
                Total = employees.Count()
            };

            return Json(gridModel);
        }

        public IActionResult DatesOfEmployee(int customerId)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Assistance))
                return AccessDeniedView();

            var model = new DateOfEmployeeMain();
            var employee = _payrollEmployeeService.GetByCustomerId(customerId);
            if (employee != null)
            {
                model.Id = employee.CustomerId;
                model.FullName = employee.GetFullName() + (employee.EmployeeStatusId == (int)EmployeeStatus.Discharged ? " (Baja)" : string.Empty);
                model.NameInChecker =
                    _assistanceService.CheckEmployeeNumbersExistence(new List<int> { employee.EmployeeNumber })
                    .FirstOrDefault()?.Name;
            }
            else
                return RedirectToAction("List");

            if (employee.EmployeeStatusId != (int)EmployeeStatus.Discharged)
            {
                var inserted = _assistanceService.IncidentsRegister(new List<PayrollEmployee> { employee });
                if (inserted > 0)
                    SuccessNotification($"Se insertaron {inserted} nuevas incidencias para el empleado {model.FullName}");
            }

            return View("~/Plugins/Teed.Plugin.Payroll/Views/Assistance/DatesOfEmployee.cshtml", model);
        }

        [HttpPost]
        public IActionResult DatesOfEmployeeData(int customerId)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Assistance))
                return AccessDeniedView();

            var employee = _payrollEmployeeService.GetByCustomerId(customerId);
            if (employee == null)
                return RedirectToAction("Index");

            var jobOfEmployee = _jobCatalogService.GetById(employee.GetCurrentJob()?.Id ?? 0);
            if (jobOfEmployee == null || string.IsNullOrEmpty(jobOfEmployee.WorkSchedule))
                return RedirectToAction("Index");

            var model = new List<DateOfEmployee>();
            var tuple = new Tuple<List<DateOfEmployee>, List<string>>(model, new List<string>());
            tuple = AssistanceHelper.DoAssistanceModelWork(model, new List<PayrollEmployee> { employee }, DateTime.Now,
                _assistanceService, _assistanceOverrideService, _customerService, _jobCatalogService, true);
            model = tuple.Item1;

            if (employee.DateOfDeparture != null)
                model = model.Where(x => x.Date <= employee.DateOfDeparture.Value).ToList();

            var gridModel = new DataSourceResult
            {
                Data = model.Select(x => new
                {
                    Id = employee.Id.ToString() + x.Date.ToString("ddMMyyyy"),
                    EmployeeId = employee.Id,
                    x.Date,
                    DateString = x.Date.ToString("dd/MM/yyyy"),
                    OriginalAssistance = x.Assistance,
                    x.Assistance,
                    TimesRegistred = string.Join("<br/>", x.TimesRegistred.Select(y => y.ToString("hh:mm:ss tt")).ToArray()),
                    CheckIn = x.CheckIn.ToString("hh:mm tt"),
                    CheckOut = x.CheckOut.ToString("hh:mm tt"),
                    Overriden = x.Overriden,
                    OverrideLogs = (x.OverrideLog ?? "Esta asistencia no ha sido modificada.").Replace("\n", "<br>"),
                    OverrideComment = (x.OverrideComment ?? "Esta asistencia no tiene comentarios.").Replace("\n", "<br>"),
                }).OrderByDescending(x => x.Date).ToList(),
                Total = model.Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult AssistanceOverride(AssistanceInfoModel model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Assistance))
                return AccessDeniedView();

            if (model.Assistance > 0 && model.OriginalAssistance > 0 &&
                model.EmployeeId > 0 && !string.IsNullOrEmpty(model.DateString))
            {
                var date = DateTime.ParseExact(model.DateString, "dd/MM/yyyy", CultureInfo.InvariantCulture).Date;

                var assistanceOverride = _assistanceOverrideService.GetByDateAndEmployeeId(model.EmployeeId, date);

                if (assistanceOverride == null)
                {
                    assistanceOverride = new AssistanceOverride
                    {
                        PayrollEmployeeId = model.EmployeeId,
                        OverriddenDate = date,
                        Type = model.Assistance,
                        Comment = model.Comment,
                        Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) creó este override manual " +
                            $"de asistencia del día {model.DateString} del tipo de asistencia \"{((AssistanceType)model.OriginalAssistance).GetDisplayName()}\" a " +
                            $"\"{((AssistanceType)model.Assistance).GetDisplayName()}\".\n",
                    };
                    _assistanceOverrideService.Insert(assistanceOverride);
                }
                else
                {
                    if (assistanceOverride.Comment != model.Comment)
                    {
                        assistanceOverride.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó este override manual " +
                                $"de asistencia del día {model.DateString} del comentario \"{assistanceOverride.Comment}\" a " +
                                $"\"{model.Comment}\".\n";
                        assistanceOverride.Comment = model.Comment;
                    }
                    if (assistanceOverride.Type != model.Assistance)
                    {
                        assistanceOverride.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó este override manual " +
                                $"de asistencia del día {model.DateString} del tipo de asistencia \"{((AssistanceType)model.OriginalAssistance).GetDisplayName()}\" a " +
                                $"\"{((AssistanceType)model.Assistance).GetDisplayName()}\".\n";
                        assistanceOverride.Type = model.Assistance;
                    }
                    _assistanceOverrideService.Update(assistanceOverride);
                }

                return Ok();
            }
            else
                return BadRequest("Data incomplete");
        }

        [HttpGet]
        public IActionResult DownloadAssistances(int id)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Assistance))
                return AccessDeniedView();

            var today = DateTime.Now;
            var model = new List<DateOfEmployee>();
            var tuple = new Tuple<List<DateOfEmployee>, List<string>>(model, new List<string>());
            try
            {
                var employees = _payrollEmployeeService.GetAll(includeExEmployees: true)
                   .Where(x => x.EmployeeNumber > 0).ToList().OrderBy(x => x.GetFullName()).ToList();
                if (id > 0)
                    employees = employees.Where(x => x.CustomerId == id).ToList();
                var existingEmployees =
                    _assistanceService.CheckEmployeeNumbersExistence(employees.Select(x => x.EmployeeNumber)
                    .ToList()).ToList();
                tuple = AssistanceHelper.DoAssistanceModelWork(model, employees, today,
                _assistanceService, _assistanceOverrideService, _customerService, _jobCatalogService, true);
                model = tuple.Item1;
                if (model.Any())
                {
                    using (var stream = new MemoryStream())
                    {
                        using (var xlPackage = new ExcelPackage(stream))
                        {
                            var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                            int row = 1;

                            worksheet.Cells[row, 1].Value = "Empleado (Nombre en Reloj Checador)";
                            worksheet.Cells[row, 2].Value = "Fecha";
                            worksheet.Cells[row, 3].Value = "Tiempos registrados";
                            worksheet.Cells[row, 4].Value = "Hora de entrada y salida";
                            worksheet.Cells[row, 5].Value = "Asistencia";
                            row++;

                            foreach (var date in model.OrderByDescending(x => x.Date))
                            {
                                var employeeInChecker = existingEmployees
                                    .Where(x => x.BadgeNumber == _assistanceService.FormatEmployeeNumber(date.EmployeeNumber))
                                    .FirstOrDefault();
                                if (date.TimesRegistred.Any())
                                    foreach (var time in date.TimesRegistred)
                                    {
                                        worksheet.Cells[row, 1].Value =
                                            $"{date.Name} ({(employeeInChecker == null ? "NÚMERO DE EMPLEADO NO EXISTE EN RELOJ CHECADOR" : employeeInChecker.Name)})";
                                        worksheet.Cells[row, 2].Value = date.Date;
                                        worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                                        worksheet.Cells[row, 3].Value = time;
                                        worksheet.Cells[row, 3].Style.Numberformat.Format = "hh:mm:ss";
                                        worksheet.Cells[row, 4].Value = $"De {date.CheckIn:hh:mm tt} a {date.CheckOut:hh:mm tt}";
                                        worksheet.Cells[row, 5].Value = ((AssistanceType)date.Assistance).GetDisplayName();
                                        row++;
                                    }
                                else
                                {
                                    worksheet.Cells[row, 1].Value =
                                        $"{date.Name} ({(employeeInChecker == null ? "NÚMERO DE EMPLEADO NO EXISTE EN RELOJ CHECADOR" : employeeInChecker.Name)})";
                                    worksheet.Cells[row, 2].Value = date.Date;
                                    worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                                    worksheet.Cells[row, 3].Value = "--:--:--";
                                    worksheet.Cells[row, 4].Value = $"De {date.CheckIn:hh:mm tt} a {date.CheckOut:hh:mm tt}";
                                    worksheet.Cells[row, 5].Value = ((AssistanceType)date.Assistance).GetDisplayName();
                                    row++;
                                }
                            }

                            for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                            {
                                worksheet.Column(i).AutoFit();
                                worksheet.Cells[1, i].Style.Font.Bold = true;
                            }

                            xlPackage.Save();
                        }

                        return File(stream.ToArray(), MimeTypes.TextXlsx, $"Asistencias Reloj Checador " + DateTime.Now.ToString("dd-MM-yyyy") + ".xlsx");
                    }
                }
            }
            catch (Exception e)
            {
                tuple.Item2.Add(e.Message);
            }
            return Ok(string.Join("\n", tuple.Item2));
        }

        [HttpGet]
        public IActionResult DownloadAssistancesForFranchise(string customerIds)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Assistance))
                return AccessDeniedView();

            var today = DateTime.Now;
            var model = new List<DateOfEmployee>();
            var tuple = new Tuple<List<DateOfEmployee>, List<string>>(model, new List<string>());
            try
            {
                var ids = customerIds.Split(',').Select(x => int.Parse(x)).ToList();
                var customers = _customerService.GetAllCustomersQuery().Where(x => ids.Contains(x.Id)).ToList();
                if (customers.Any())
                {
                    var currentCustomerIds = customers.Select(x => x.Id).ToList();
                    var employees = _payrollEmployeeService.GetAll(onlyEmployees: false)
                        .Where(x => currentCustomerIds.Contains(x.CustomerId) && !x.Deleted)
                        .ToList();
                    var employeeNumbers = employees?.Select(x => x.EmployeeNumber).ToList();
                    var existingEmployees =
                        _assistanceService.CheckEmployeeNumbersExistence(employeeNumbers).ToList();
                    tuple = AssistanceHelper.DoAssistanceModelWork(model, employees, today,
                _assistanceService, _assistanceOverrideService, _customerService, _jobCatalogService);
                    model = tuple.Item1;
                    if (model.Any())
                    {
                        using (var stream = new MemoryStream())
                        {
                            using (var xlPackage = new ExcelPackage(stream))
                            {
                                var customersData = model.OrderBy(x => x.Name).GroupBy(x => x.CustomerId);
                                foreach (var customer in customersData)
                                {
                                    var worksheet = xlPackage.Workbook.Worksheets.Add(customer.FirstOrDefault().Name);
                                    int row = 1;

                                    worksheet.Cells[row, 1].Value = "Empleado (Nombre en Reloj Checador)";
                                    worksheet.Cells[row, 2].Value = "Fecha";
                                    worksheet.Cells[row, 3].Value = "Tiempos registrados";
                                    worksheet.Cells[row, 4].Value = "Hora de entrada y salida";
                                    worksheet.Cells[row, 5].Value = "Asistencia";
                                    row++;

                                    var customerDates = customer.OrderByDescending(x => x.Date);
                                    foreach (var date in customerDates)
                                    {
                                        var employeeInChecker = existingEmployees
                                            .Where(x => x.BadgeNumber == _assistanceService.FormatEmployeeNumber(date.EmployeeNumber))
                                            .FirstOrDefault();
                                        if (date.TimesRegistred.Any())
                                            foreach (var time in date.TimesRegistred)
                                            {
                                                worksheet.Cells[row, 1].Value =
                                                    $"{date.Name} ({(employeeInChecker == null ? "NÚMERO DE EMPLEADO NO EXISTE EN RELOJ CHECADOR" : employeeInChecker.Name)})";
                                                worksheet.Cells[row, 2].Value = date.Date;
                                                worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                                                worksheet.Cells[row, 3].Value = time;
                                                worksheet.Cells[row, 3].Style.Numberformat.Format = "hh:mm:ss";
                                                worksheet.Cells[row, 4].Value = $"De {date.CheckIn:hh:mm tt} a {date.CheckOut:hh:mm tt}";
                                                worksheet.Cells[row, 5].Value = ((AssistanceType)date.Assistance).GetDisplayName();
                                                row++;
                                            }
                                        else
                                        {
                                            worksheet.Cells[row, 1].Value =
                                                $"{date.Name} ({(employeeInChecker == null ? "NÚMERO DE EMPLEADO NO EXISTE EN RELOJ CHECADOR" : employeeInChecker.Name)})";
                                            worksheet.Cells[row, 2].Value = date.Date;
                                            worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                                            worksheet.Cells[row, 3].Value = "--:--:--";
                                            worksheet.Cells[row, 4].Value = $"De {date.CheckIn:hh:mm tt} a {date.CheckOut:hh:mm tt}";
                                            worksheet.Cells[row, 5].Value = ((AssistanceType)date.Assistance).GetDisplayName();
                                            row++;
                                        }
                                    }

                                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                                    {
                                        worksheet.Column(i).AutoFit();
                                        worksheet.Cells[1, i].Style.Font.Bold = true;
                                    }
                                }

                                xlPackage.Save();
                            }

                            return File(stream.ToArray(), MimeTypes.TextXlsx, $"Asistencias Reloj Checador " + DateTime.Now.ToString("dd-MM-yyyy") + ".xlsx");
                        }
                    }
                }
                else
                    tuple.Item2.Add("No customers found with given ids: " + customerIds);
            }
            catch (Exception e)
            {
                tuple.Item2.Add(e.Message);
            }
            return Ok(string.Join("\n", tuple.Item2));
        }

        [HttpGet]
        public IActionResult GetIncidentsFromAllTime()
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Assistance))
                return AccessDeniedView();

            var employees = _payrollEmployeeService.GetAll().Where(x => x.EmployeeStatusId != (int)EmployeeStatus.Discharged).ToList();
            var inserted = _assistanceService.IncidentsRegister(employees, DateTime.Now.AddDays(1).Date, new DateTime(2020, 9, 12).Date);
            return Ok();
        }

        [HttpGet]
        public IActionResult CurrentAssistanceData()
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Assistance))
                return AccessDeniedView();

            var employees = _payrollEmployeeService.GetAll()
                .Where(x => x.EmployeeStatusId != (int)EmployeeStatus.Discharged)
                .ToList().OrderBy(x => x.GetFullName()).ToList();
            var existingEmployees =
                _assistanceService.CheckEmployeeNumbersExistence(employees.Select(x => x.EmployeeNumber)
                .ToList()).ToList();
            var existingEmployeeNumbers = existingEmployees.Select(x => x.BadgeNumber).ToList();

            var data = employees.Select(x => new
            {
                x.EmployeeNumber,
                x.Id,
                x.CustomerId,
                FullName = x.GetFullName(),
                NameInChecker =
                        existingEmployees.Where(y => y.BadgeNumber == _assistanceService.FormatEmployeeNumber(x.EmployeeNumber))
                        .FirstOrDefault()?.Name,
                ExistsInChecker =
                        existingEmployeeNumbers
                        .Contains(_assistanceService.FormatEmployeeNumber(x.EmployeeNumber))
            }).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Id";
                    worksheet.Cells[row, 2].Value = "Número de empleado en CEL";
                    worksheet.Cells[row, 3].Value = "Nombre en CEL";
                    worksheet.Cells[row, 4].Value = "Nombre en Reloj checador";
                    worksheet.Cells[row, 5].Value = "Existe en el Reloj checador";
                    worksheet.Cells[row, 6].Value = "Número correcto";
                    row++;

                    foreach (var user in data)
                    {
                        worksheet.Cells[row, 1].Value = user.Id;
                        worksheet.Cells[row, 2].Value = user.EmployeeNumber;
                        worksheet.Cells[row, 3].Value = user.FullName;
                        worksheet.Cells[row, 4].Value = user.NameInChecker;
                        worksheet.Cells[row, 5].Value = user.ExistsInChecker ? "SI" : "NO";
                        row++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"CEL - Reloj checador empleados " + DateTime.Now.ToString("dd-MM-yyyy") + ".xlsx");
            }
        }

        [HttpPost]
        public IActionResult GetAssistancesXls(FiltersXlsModel model)
        {
            var today = DateTime.Now;
            var startDate = model.StartDate;
            var endDate = model.EndDate;
            var festiveDates = _assistanceService.GetFestiveDatesFromGroceries()
                .Where(x => !x.DontApplyToPayroll).ToList();
            var biweeks = new List<BiweeklyDatesModel>();
            var dates = BonusApplicationHelper.GetBiweekliesInRange(startDate, endDate);
            foreach (var date in dates)
                biweeks.Add(BiweeklyDatesHelper.GetBiweeklyDates(date, festiveDates));
            biweeks = biweeks.OrderBy(x => x.StartOfBiweekly).ToList();

            var firstBiweekStart = biweeks.FirstOrDefault().StartOfBiweekly;
            var lastBiweekEnd = biweeks.LastOrDefault().EndOfBiweekly;

            var jobIdsFormat = model.JobIds;
            var jobs = _jobCatalogService.GetAll()
                .Where(x => !x.Deleted && jobIdsFormat.Contains(x.Id))
                .ToList();
            jobIdsFormat = jobs.Select(x => x.Id).ToList();

            var employees = _payrollEmployeeService.GetAll()
            .ToList()
            .Where(x => !x.Deleted && jobIdsFormat.Contains(x.GetCurrentJob()?.Id ?? 0))
            .ToList();
            var employeeNumbers = employees.Select(x => x.EmployeeNumber).ToList();

            var existingEmployees =
                _assistanceService.CheckEmployeeNumbersExistence(employeeNumbers).ToList();

            var data = new List<DateOfEmployee>();
            var tuple = new Tuple<List<DateOfEmployee>, List<string>>(data, new List<string>());
            var delayColor = Color.FromArgb(251, 255, 103);
            var abscenseColor = Color.FromArgb(255, 103, 103);
            var onTimeColor = Color.FromArgb(142, 255, 103);

            data = AssistanceHelper.DoAssistanceModelWork(data, employees, lastBiweekEnd,
                _assistanceService, _assistanceOverrideService, _customerService, _jobCatalogService, true).Item1;

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    foreach (var biweek in biweeks)
                    {
                        var worksheet = xlPackage.Workbook.Worksheets.Add($"{biweek.StartOfBiweekly:dd-MM-yyyy} = {biweek.EndOfBiweekly:dd-MM-yyyy}");
                        int row = 1;

                        worksheet.Cells[row, 1].Value = "Número de empleado";
                        worksheet.Cells[row, 2].Value = "Nombre completo";
                        worksheet.Cells[row, 3].Value = "Puesto";
                        worksheet.Cells[row, 4].Value = "Días laborables para el empleado según su puesto";
                        worksheet.Cells[row, 5].Value = "Días laborables con llegada a tiempo";
                        worksheet.Cells[row, 6].Value = "Días laborables con retardo";
                        worksheet.Cells[row, 7].Value = "Días laborables sin asistencia";
                        worksheet.Cells[row, 8].Value = "Lunes - Hora límite de entrada según puesto";
                        worksheet.Cells[row, 9].Value = "Martes -  Hora límite de entrada según puesto";
                        worksheet.Cells[row, 10].Value = "Miércoles -  Hora límite de entrada según puesto";
                        worksheet.Cells[row, 11].Value = "Jueves -  Hora límite de entrada según puesto";
                        worksheet.Cells[row, 12].Value = "Viernes -  Hora límite de entrada según puesto";
                        worksheet.Cells[row, 13].Value = "Sabado -  Hora límite de entrada según puesto";
                        worksheet.Cells[row, 14].Value = "Domingo -  Hora límite de entrada según puesto";

                        var totalDays = (biweek.EndOfBiweekly - biweek.StartOfBiweekly).TotalDays;
                        for (int i = 0; i < totalDays; i++)
                            worksheet.Cells[row, 15 + i].Value = $"Día {i + 1} de quincena";

                        row++;

                        var employeesOfCurrentBiweeklyIds = employees
                            .Where(x => x.DateOfDeparture >= firstBiweekStart || x.DateOfDeparture == null)
                            .Select(x => x.CustomerId).ToList();
                        data = data.Where(x => employeesOfCurrentBiweeklyIds.Contains(x.CustomerId)).ToList();
                        var customersData = data.OrderBy(x => x.Name).GroupBy(x => x.CustomerId);

                        foreach (var customerData in customersData)
                        {
                            var employee = employees.Where(x => x.CustomerId == customerData.Key).FirstOrDefault();
                            if (employee != null)
                            {
                                var employeeInChecker = existingEmployees
                                    .Where(x => x.BadgeNumber == _assistanceService.FormatEmployeeNumber(employee.EmployeeNumber))
                                    .FirstOrDefault();

                                worksheet.Cells[row, 1].Value = employee.EmployeeNumber;
                                if (employeeInChecker != null)
                                {
                                    worksheet.Cells[row, 2].Value = employee.GetFullName();
                                    worksheet.Cells[row, 3].Value = employee.GetCurrentJob() == null ? "-"
                                        : employee.GetCurrentJob().Name;
                                    worksheet.Cells[row, 4].Value = employee.GetCurrentJob() == null ? 0
                                        : employee.GetCurrentJob().WorkSchedule.Split('|').Length;
                                    if (!string.IsNullOrEmpty(employee.GetCurrentJob()?.WorkSchedule))
                                    {
                                        worksheet.Cells[row, 5].Value = customerData.Where(x => biweek.StartOfBiweekly <= x.Date.Date && x.Date.Date <= biweek.EndOfBiweekly &&
                                            x.Assistance == (int)AssistanceType.InTime).Count();
                                        worksheet.Cells[row, 6].Value = customerData.Where(x => biweek.StartOfBiweekly <= x.Date.Date && x.Date.Date <= biweek.EndOfBiweekly &&
                                            x.Assistance == (int)AssistanceType.Delay).Count();
                                        worksheet.Cells[row, 7].Value = customerData.Where(x => biweek.StartOfBiweekly <= x.Date.Date && x.Date.Date <= biweek.EndOfBiweekly &&
                                            x.Assistance == (int)AssistanceType.Absence).Count();

                                        var jobSchedule = _assistanceService.GetScheduleOfEmployee(employee.GetCurrentJob().WorkSchedule);
                                        var monday = jobSchedule.Where(x => x.DayOfWeek == DayOfWeek.Monday).FirstOrDefault();
                                        if (monday != null)
                                        {
                                            worksheet.Cells[row, 8].Value = new DateTime(today.Year, today.Month, today.Day, monday.CheckIn, 0, 0);
                                            worksheet.Cells[row, 8].Style.Numberformat.Format = "hh:mm:ss";
                                        }
                                        else
                                            worksheet.Cells[row, 8].Value = "--:--:--";

                                        var tuesday = jobSchedule.Where(x => x.DayOfWeek == DayOfWeek.Tuesday).FirstOrDefault();
                                        if (tuesday != null)
                                        {
                                            worksheet.Cells[row, 9].Value = new DateTime(today.Year, today.Month, today.Day, tuesday.CheckIn, 0, 0);
                                            worksheet.Cells[row, 9].Style.Numberformat.Format = "hh:mm:ss";
                                        }
                                        else
                                            worksheet.Cells[row, 9].Value = "--:--:--";

                                        var wednesday = jobSchedule.Where(x => x.DayOfWeek == DayOfWeek.Wednesday).FirstOrDefault();
                                        if (wednesday != null)
                                        {
                                            worksheet.Cells[row, 10].Value = new DateTime(today.Year, today.Month, today.Day, wednesday.CheckIn, 0, 0);
                                            worksheet.Cells[row, 10].Style.Numberformat.Format = "hh:mm:ss";
                                        }
                                        else
                                            worksheet.Cells[row, 10].Value = "--:--:--";

                                        var thursday = jobSchedule.Where(x => x.DayOfWeek == DayOfWeek.Thursday).FirstOrDefault();
                                        if (thursday != null)
                                        {
                                            worksheet.Cells[row, 11].Value = new DateTime(today.Year, today.Month, today.Day, thursday.CheckIn, 0, 0);
                                            worksheet.Cells[row, 11].Style.Numberformat.Format = "hh:mm:ss";
                                        }
                                        else
                                            worksheet.Cells[row, 11].Value = "--:--:--";

                                        var friday = jobSchedule.Where(x => x.DayOfWeek == DayOfWeek.Friday).FirstOrDefault();
                                        if (friday != null)
                                        {
                                            worksheet.Cells[row, 12].Value = new DateTime(today.Year, today.Month, today.Day, friday.CheckIn, 0, 0);
                                            worksheet.Cells[row, 12].Style.Numberformat.Format = "hh:mm:ss";
                                        }
                                        else
                                            worksheet.Cells[row, 12].Value = "--:--:--";

                                        var saturday = jobSchedule.Where(x => x.DayOfWeek == DayOfWeek.Saturday).FirstOrDefault();
                                        if (saturday != null)
                                        {
                                            worksheet.Cells[row, 13].Value = new DateTime(today.Year, today.Month, today.Day, saturday.CheckIn, 0, 0);
                                            worksheet.Cells[row, 13].Style.Numberformat.Format = "hh:mm:ss";
                                        }
                                        else
                                            worksheet.Cells[row, 13].Value = "--:--:--";

                                        var sunday = jobSchedule.Where(x => x.DayOfWeek == DayOfWeek.Sunday).FirstOrDefault();
                                        if (sunday != null)
                                        {
                                            worksheet.Cells[row, 14].Value = new DateTime(today.Year, today.Month, today.Day, sunday.CheckIn, 0, 0);
                                            worksheet.Cells[row, 14].Style.Numberformat.Format = "hh:mm:ss";
                                        }
                                        else
                                            worksheet.Cells[row, 14].Value = "--:--:--";

                                        var dateStart = biweek.StartOfBiweekly;
                                        var datesGroup = customerData.GroupBy(x => x.Date.Date).OrderBy(x => x.Key).ToList();
                                        for (int i = 0; i < totalDays; i++)
                                        {
                                            if (i > 0)
                                                dateStart = dateStart.AddDays(1);

                                            worksheet.Cells[row, 15 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            if (jobSchedule.Select(x => x.DayOfWeek).Contains(dateStart.DayOfWeek))
                                            {
                                                var assistancesOfDay = datesGroup.Where(x => x.Key.Date == dateStart.Date.Date).FirstOrDefault();
                                                if (assistancesOfDay != null)
                                                {
                                                    var firstAssistance = assistancesOfDay.ToList().OrderBy(x => x.Date).FirstOrDefault();
                                                    var firstDate = firstAssistance.TimesRegistred.OrderBy(x => x).FirstOrDefault();
                                                    if (firstDate != null && firstDate != DateTime.MinValue)
                                                        worksheet.Cells[row, 15 + i].Value = new DateTime(firstDate.Year, firstDate.Month, firstDate.Day, firstDate.Hour, firstDate.Minute, firstDate.Second);
                                                    else
                                                        worksheet.Cells[row, 15 + i].Value = new DateTime(dateStart.Year, dateStart.Month, dateStart.Day, 0, 0, 0);

                                                    worksheet.Cells[row, 15 + i].Style.Fill.BackgroundColor.SetColor(
                                                        firstAssistance.Assistance == (int)AssistanceType.InTime ? onTimeColor :
                                                        firstAssistance.Assistance == (int)AssistanceType.Delay ? delayColor :
                                                        abscenseColor
                                                        );
                                                }
                                                else
                                                {
                                                    worksheet.Cells[row, 15 + i].Style.Fill.BackgroundColor.SetColor(abscenseColor);
                                                    worksheet.Cells[row, 15 + i].Value = new DateTime(dateStart.Year, dateStart.Month, dateStart.Day, 0, 0, 0);
                                                }
                                            }
                                            else
                                            {
                                                worksheet.Cells[row, 15 + i].Style.Fill.BackgroundColor.SetColor(Color.Black);
                                                worksheet.Cells[row, 15 + i].Style.Font.Color.SetColor(Color.White);
                                                worksheet.Cells[row, 15 + i].Value = new DateTime(dateStart.Year, dateStart.Month, dateStart.Day, 0, 0, 0);
                                            }

                                            worksheet.Cells[row, 15 + i].Style.Numberformat.Format = "dd/mm/yyyy hh:mm:ss";
                                        }
                                    }
                                }
                                else
                                    worksheet.Cells[row, 2].Value = "NÚMERO DE EMPLEADO NO EXISTE EN RELOJ CHECADOR";

                                row++;
                            }
                        }

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx,
                    $"CEL - Asistencias en el rango de fechas de " + startDate.ToString("dd-MM-yyyy") + " a " + endDate.ToString("dd-MM-yyyy") + ".xlsx");
            }
        }

        //public IActionResult Checker()
        //{
        //    if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Assistance))
        //        return AccessDeniedView();

        //    var testUser = _assistanceService.GetByEmployeeNumber(18);
        //    return Ok("");
        //}
    }

    public class AssistanceInfoModel
    {
        public string Id { get; set; }
        public int EmployeeId { get; set; }
        public string DateString { get; set; }
        public int Assistance { get; set; }
        public int OriginalAssistance { get; set; }
        public string Comment { get; set; }
    }
}