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
using Teed.Plugin.Payroll.Helpers;
using OfficeOpenXml;
using Teed.Plugin.Payroll.Models.BiweeklyPayment;
using Teed.Plugin.Payroll.Models;

namespace Teed.Plugin.Payroll.Controllers
{
    [Area(AreaNames.Admin)]
    public class IncidentController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly PayrollEmployeeService _payrollEmployeeService;
        private readonly PayrollEmployeeFileService _payrollEmployeeFileService;
        private readonly PayrollSalaryService _payrollSalaryService;
        private readonly JobCatalogService _jobCatalogService;
        private readonly IncidentService _incidentService;
        private readonly IWorkContext _workContext;
        private readonly MinimumWagesCatalogService _minimumWagesCatalogService;
        private readonly BonusService _bonusService;
        private readonly BonusApplicationService _bonusApplicationService;
        private readonly AssistanceService _assistanceService;

        public IncidentController(IPermissionService permissionService, PayrollEmployeeService payrollEmployeeService,
            PayrollEmployeeFileService payrollEmployeeFileService, PayrollSalaryService payrollSalaryService,
            JobCatalogService jobCatalogService, IncidentService incidentService,
            IWorkContext workContext, MinimumWagesCatalogService minimumWagesCatalogService,
            BonusService bonusService, BonusApplicationService bonusApplicationService,
            AssistanceService assistanceService)
        {
            _permissionService = permissionService;
            _payrollEmployeeService = payrollEmployeeService;
            _payrollEmployeeFileService = payrollEmployeeFileService;
            _payrollSalaryService = payrollSalaryService;
            _jobCatalogService = jobCatalogService;
            _incidentService = incidentService;
            _workContext = workContext;
            _minimumWagesCatalogService = minimumWagesCatalogService;
            _bonusService = bonusService;
            _bonusApplicationService = bonusApplicationService;
            _assistanceService = assistanceService;
        }

        public AddOrCheckModel PrepareModel(AddOrCheckModel model)
        {
            var types = Enum.GetValues(typeof(IncidentType)).Cast<IncidentType>().ToList();
            var list = types.Select(x => new
            {
                Id = (int)x,
                Name = x.GetDisplayName()
            });
            model.Types = new SelectList(list, "Id", "Name");
            return model;
        }

        public IActionResult Index()
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Incident))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Payroll/Views/Incident/Index.cshtml");
        }

        public IActionResult CheckOrUpdate(int Id)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Incident))
                return AccessDeniedView();
            var model = new AddOrCheckModel
            {
                BossId = _workContext.CurrentCustomer.Id,
                Id = Id
            };
            PrepareModel(model);

            return View("~/Plugins/Teed.Plugin.Payroll/Views/Incident/CheckOrUpdate.cshtml", model);
        }

        [HttpPost]
        public IActionResult IncidentList(int payrollId)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Incident))
                return AccessDeniedView();
            var incidents = _incidentService.GetAllByPayrollEmployeeId(payrollId)
                .OrderByDescending(x => x.Date)
                .ToList();
            var minimuSalary = _minimumWagesCatalogService.GetYearWage(DateTime.Now.Year);
            var gridModel = new DataSourceResult
            {
                Data = incidents.Select(x => new
                {
                    x.Id,
                    Date = x.Date?.ToString("dd/MM/yyyy"),
                    x.Amount,
                    x.Reason,
                    MinimumTerm = GetIncidentTerm(x, minimuSalary),
                    Type = x.Type.GetDisplayName(),
                    TypeId = (int)x.Type,
                    Created = x.CreatedOnUtc.ToLocalTime().ToString("dd/MM/yyyy"),
                    HasFile = x.File != null,
                    x.Justified
                }).ToList(),
                Total = incidents.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult IncidentAdd(AddIncident model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Incident))
                return AccessDeniedView();
            try
            {
                var incident = new Incident
                {
                    Type = model.IncidentType,
                    PayrollEmployeeId = model.PayrollEmployeeId,
                    Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) agregó una nueva incidencia para el día {(model.Date == null ? "" : (model.Date ?? DateTime.Now).ToString("dd/MM/yyyy"))}\n"
                };

                if (model.IncidentType == IncidentType.Absence || model.IncidentType == IncidentType.Delay)
                {
                    incident.Justified = model.Justified;
                    incident.Date = model.Date;
                    if (model.File != null)
                    {
                        try
                        {
                            byte[] bytes = new byte[0];
                            using (var ms = new MemoryStream())
                            {
                                model.File.CopyTo(ms);
                                bytes = ms.ToArray();
                            }
                            incident.File = bytes;
                            incident.FileExtension = model.File.FileName.Substring(model.File.FileName.LastIndexOf(".") + 1);
                            incident.FileMimeType = model.File.ContentType;
                            incident.FileSize = (int)model.File.Length;
                        }
                        catch (Exception e)
                        {
                            return BadRequest(e);
                        }
                    }
                }
                else if (model.IncidentType == IncidentType.Discount)
                {
                    incident.Amount = model.Amount;
                    incident.Reason = model.Reason;
                }
                else
                {
                    return Ok();
                }
                _incidentService.Insert(incident);
                if (!incident.Justified)
                    _ = BonusApplicationHelper.CheckBonusesAfterIncidentAdd(incident,
                        _bonusService, _bonusApplicationService, _jobCatalogService,
                        _payrollEmployeeService);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        public IActionResult Edit(int Id)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Incident))
                return AccessDeniedView();

            var incident = _incidentService.GetById(Id);
            if (incident == null)
                return RedirectToAction("Index");

            var model = new AddIncident
            {
                Type = (int)incident.Type,
                Amount = incident.Amount,
                Date = incident.Date,
                Id = incident.Id,
                IncidentType = incident.Type,
                Justified = incident.Justified,
                PayrollEmployeeId = incident.PayrollEmployeeId,
                Reason = incident.Reason,
                Log = incident.Log
            };
            var types = Enum.GetValues(typeof(IncidentType)).Cast<IncidentType>().ToList();
            var list = types.Select(x => new
            {
                Id = (int)x,
                Name = x.GetDisplayName()
            });
            model.Types = new SelectList(list, "Id", "Name");

            return View("~/Plugins/Teed.Plugin.Payroll/Views/Incident/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult Edit(AddIncident model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Incident))
                return AccessDeniedView();
            try
            {
                var incident = _incidentService.GetById(model.Id);
                if (incident == null)
                    return RedirectToAction("CheckOrUpdate", new { Id = model.PayrollEmployeeId });

                if (incident.Justified != model.Justified)
                {
                    incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió la justificación de {(incident.Justified ? "\"Si\"" : "\"No\"")} a {(model.Justified ? "\"Si\"" : "\"No\"")}\n";
                }
                if (incident.Reason != model.Reason)
                {
                    incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió la razón de \"{incident.Reason}\" a \"{model.Reason}\"\n";
                }
                if (incident.Type != (IncidentType)model.Type)
                {
                    incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el tipo de incidencia de {incident.Type.GetDisplayName()} a {((IncidentType)model.Type).GetDisplayName()}\n";
                    incident.Type = (IncidentType)model.Type;
                }
                if (incident.Amount != model.Amount)
                {
                    incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió la cantidad a descontar de {incident.Amount} a {model.Amount}\n";
                }

                if ((IncidentType)model.Type == IncidentType.Absence || (IncidentType)model.Type == IncidentType.Delay)
                {
                    incident.Justified = model.Justified;
                    if (model.File != null)
                    {
                        try
                        {
                            byte[] bytes = new byte[0];
                            using (var ms = new MemoryStream())
                            {
                                model.File.CopyTo(ms);
                                bytes = ms.ToArray();
                            }
                            incident.File = bytes;
                            incident.FileExtension = model.File.FileName.Substring(model.File.FileName.LastIndexOf(".") + 1);
                            incident.FileMimeType = model.File.ContentType;
                            incident.FileSize = (int)model.File.Length;
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }
                else if ((IncidentType)model.Type == IncidentType.Discount)
                {
                    incident.Amount = model.Amount;
                    incident.Reason = model.Reason;
                }
                _incidentService.Update(incident);
            }
            catch (Exception e)
            {
            }
            return RedirectToAction("CheckOrUpdate", new { Id = model.PayrollEmployeeId });
        }

        public int GetIncidentTerm(Incident incident, MinimumWagesCatalog minimuSalary)
        {
            var terms = 0;
            // Get term with 30% calculus
            var minimum = minimuSalary.Amount;
            var salary = incident.PayrollEmployee
                .PayrollSalaries.OrderByDescending(x => x.ApplyDate)
                .FirstOrDefault();
            if (salary != null && incident.Type == IncidentType.Discount)
            {
                var termValue = (salary.NetIncome - minimum) * (decimal).30;
                terms = (int)Math.Ceiling((incident.Amount ?? 0) / termValue);
            }
            return terms;
        }

        [HttpPost]
        public IActionResult IncidentUpdate(AddIncident model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Incident))
                return AccessDeniedView();
            var incident = _incidentService.GetById(model.Id);
            if (incident == null)
                return Ok();

            incident.Amount = model.Amount;
            incident.Reason = model.Reason;
            _incidentService.Update(incident);
            return Ok();
        }

        [HttpPost]
        public IActionResult IncidentDelete(AddIncident model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Incident))
                return AccessDeniedView();
            var incident = _incidentService.GetById(model.Id);
            if (incident == null)
                return Ok();

            incident.Deleted = true;
            _incidentService.Update(incident);
            return Ok();
        }

        [HttpGet]
        public IActionResult IncidentFileDownload(int id)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            var incident = _incidentService.GetById(id);
            if (incident == null)
                return Ok();
            var model = new PayrollEmployeeFileModel
            {
                FileArray = incident.File,
                Title = incident.Type.GetDisplayName(),
                Extension = incident.FileExtension
            };
            return Json(model);
        }

        [HttpGet]
        public IActionResult GetForCalendar(int id)
        {
            var incidents = _incidentService.GetAllByPayrollEmployeeId(id)
                .Where(x => x.Type != IncidentType.Discount).ToList();
            var calendar = incidents.Select(x => new
            {
                x.Id,
                Date = x.Date?.ToString("dd/MM/yyyy"),
                Description = GetTextForIncidents(x)
            }).ToList();
            return Json(calendar);
        }

        [HttpPost]
        public IActionResult GetIncidentsXls(FiltersXlsModel model)
        {
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
            .Where(x => !x.Deleted)
            .ToList()
            .Where(x => jobIdsFormat.Contains(x.GetCurrentJob()?.Id ?? 0))
            .ToList();
            var employeeIds = employees.Select(x => x.Id).ToList();

            var incidents = _incidentService.GetAll()
                .Where(x => !x.Deleted && employeeIds.Contains(x.PayrollEmployeeId) &&
                x.Date != null && firstBiweekStart <= x.Date.Value && x.Date.Value <= lastBiweekEnd)
                .OrderBy(x => x.Date)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Fecha";
                    worksheet.Cells[row, 2].Value = "Quincena a la que pertenece";
                    worksheet.Cells[row, 3].Value = "Empleado de incidencia";
                    worksheet.Cells[row, 4].Value = "Tipo";
                    worksheet.Cells[row, 5].Value = "Razón o comentario";
                    worksheet.Cells[row, 6].Value = "Justificada";
                    worksheet.Cells[row, 7].Value = "Tiene archivo de justificación";
                    //worksheet.Cells[row, 8].Value = "Monto";
                    row++;

                    foreach (var incident in incidents)
                    {
                        worksheet.Cells[row, 1].Value = incident.Date.Value;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mm-yyyy";

                        var biweek = biweeks
                            .Where(x => x.StartOfBiweekly <= incident.Date.Value && incident.Date.Value <= x.EndOfBiweekly)
                            .FirstOrDefault();
                        worksheet.Cells[row, 2].Value = $"{biweek.StartOfBiweekly:dd/MM/yyyy} - {biweek.EndOfBiweekly:dd/MM/yyyy}";

                        worksheet.Cells[row, 3].Value = $"{incident.PayrollEmployee.GetFullName()}";
                        worksheet.Cells[row, 4].Value = $"{incident.Type.GetDisplayName()}";
                        worksheet.Cells[row, 5].Value = $"{(string.IsNullOrEmpty(incident.Reason) ? "-" : incident.Reason)}";
                        worksheet.Cells[row, 6].Value = $"{(incident.Justified ? "Justificada" : "No justificada")}";
                        worksheet.Cells[row, 7].Value = $"{(incident.Justified ? (string.IsNullOrEmpty(incident.FileExtension) ? "No" : "Si") : "-")}";
                        //worksheet.Cells[row, 8].Value = $"{(incident.Amount == null ? 0 : incident.Amount)}";

                        row++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx,
                    $"CEL - Incidencias en el rango de fechas de " + startDate.ToString("dd-MM-yyyy") + " a " + endDate.ToString("dd-MM-yyyy") + ".xlsx");
            }
        }

        public string GetTextForIncidents(Incident incident)
        {
            var type = string.Empty;
            if (incident.Type == IncidentType.Absence)
                type = "Falta";
            else if (incident.Type == IncidentType.Delay)
                type = "Retardo";
            var justified = string.Empty;
            if (incident.Type == IncidentType.Absence || incident.Type == IncidentType.Delay)
            {
                if (incident.Justified)
                    justified = "Justificada";
                else
                    justified = "No justificada";
            }
            var final = string.Join(" - ", new List<string> { type, incident.Reason, justified }
                .Where(s => !string.IsNullOrEmpty(s)));
            return final ?? "";
        }
    }

    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>()
                            .GetName();
        }
    }
}