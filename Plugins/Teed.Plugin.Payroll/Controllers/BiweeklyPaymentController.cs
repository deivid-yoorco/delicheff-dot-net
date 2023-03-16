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
using Teed.Plugin.Payroll.Models.BiweeklyPayment;
using Teed.Plugin.Payroll.Domain.BiweeklyPayments;
using Nop.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Teed.Plugin.Payroll.Domain.Incidents;
using Teed.Plugin.Payroll.Models.PayrollEmployee;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;
using Teed.Plugin.Payroll.Domain.MinimumWagesCatalogs;
using Nop.Core.Domain.Discounts;
using OfficeOpenXml;
using Teed.Plugin.Payroll.Domain.Bonuses;
using Teed.Plugin.Payroll.Helpers;
using Teed.Plugin.Payroll.Models.FestiveDate;
using Teed.Plugin.Payroll.Models;

namespace Teed.Plugin.Payroll.Controllers
{
    [Area(AreaNames.Admin)]
    public class BiweeklyPaymentController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly PayrollEmployeeService _payrollEmployeeService;
        private readonly PayrollEmployeeFileService _payrollEmployeeFileService;
        private readonly PayrollSalaryService _payrollSalaryService;
        private readonly JobCatalogService _jobCatalogService;
        private readonly SubordinateService _subordinateService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly BiweeklyPaymentService _biweeklyPaymentService;
        private readonly BiweeklyPaymentFileService _biweeklyPaymentFileService;
        private readonly IncidentService _incidentService;
        private readonly MinimumWagesCatalogService _minimumWagesCatalogService;
        private readonly BonusService _bonusService;
        private readonly BonusApplicationService _bonusApplicationService;
        private readonly AssistanceService _assistanceService;
        private readonly PayrollEmployeeJobService _payrollEmployeeJobService;

        public BiweeklyPaymentController(IPermissionService permissionService, PayrollEmployeeService payrollEmployeeService,
            PayrollEmployeeFileService payrollEmployeeFileService, PayrollSalaryService payrollSalaryService,
            JobCatalogService jobCatalogService, SubordinateService subordinateService, ICustomerService customerService,
            IWorkContext workContext, BiweeklyPaymentService biweeklyPaymentService, BiweeklyPaymentFileService biweeklyPaymentFileService,
            IncidentService incidentService, MinimumWagesCatalogService minimumWagesCatalogService, BonusService bonusService,
            BonusApplicationService bonusApplicationService, AssistanceService assistanceService, PayrollEmployeeJobService payrollEmployeeJobService)
        {
            _permissionService = permissionService;
            _payrollEmployeeService = payrollEmployeeService;
            _payrollEmployeeFileService = payrollEmployeeFileService;
            _payrollSalaryService = payrollSalaryService;
            _jobCatalogService = jobCatalogService;
            _subordinateService = subordinateService;
            _customerService = customerService;
            _workContext = workContext;
            _biweeklyPaymentService = biweeklyPaymentService;
            _biweeklyPaymentFileService = biweeklyPaymentFileService;
            _incidentService = incidentService;
            _minimumWagesCatalogService = minimumWagesCatalogService;
            _bonusService = bonusService;
            _bonusApplicationService = bonusApplicationService;
            _assistanceService = assistanceService;
            _payrollEmployeeJobService = payrollEmployeeJobService;
        }

        [HttpGet]
        public IActionResult GetBiweeklyDates(string date, bool testing = false)
        {
            var parsedDate = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var festiveDates = _assistanceService.GetFestiveDatesFromGroceries()
                .Where(x => !x.DontApplyToPayroll).ToList();
            var dates = new List<BiweeklyDatesModel>();

            var beforeDate = parsedDate.AddMonths(-1);
            var beforeBiweekDate = new DateTime(beforeDate.Year, beforeDate.Month, DateTime.DaysInMonth(beforeDate.Year, beforeDate.Month));
            var before = BiweeklyDatesHelper.GetBiweeklyDates(beforeBiweekDate, festiveDates);

            var firstBiweekDate = new DateTime(parsedDate.Year, parsedDate.Month, 15);
            var first = BiweeklyDatesHelper.GetBiweeklyDates(firstBiweekDate, festiveDates);

            var lastBiweekDate = new DateTime(parsedDate.Year, parsedDate.Month, DateTime.DaysInMonth(parsedDate.Year, parsedDate.Month));
            var last = BiweeklyDatesHelper.GetBiweeklyDates(lastBiweekDate, festiveDates);

            var afterDate = parsedDate.AddMonths(1);
            var afterBiweekDate = new DateTime(afterDate.Year, afterDate.Month, 15);
            var after = BiweeklyDatesHelper.GetBiweeklyDates(afterBiweekDate, festiveDates);

            if (!testing)
                return Json(new
                {
                    beforeBiweekDate = ConvertDateToIntList(beforeBiweekDate),
                    before1stBiweek = ConvertDateToIntList(before.StartOfBiweekly),
                    before2dBiweek = ConvertDateToIntList(before.EndOfBiweekly),

                    firstBiweekDate = ConvertDateToIntList(firstBiweekDate),
                    start1stBiweek = ConvertDateToIntList(first.StartOfBiweekly),
                    end1stBiweek = ConvertDateToIntList(first.EndOfBiweekly),

                    lastBiweekDate = ConvertDateToIntList(lastBiweekDate),
                    start2dBiweek = ConvertDateToIntList(last.StartOfBiweekly),
                    end2dBiweek = ConvertDateToIntList(last.EndOfBiweekly),

                    afterBiweekDate = ConvertDateToIntList(afterBiweekDate),
                    after1stBiweek = ConvertDateToIntList(after.StartOfBiweekly),
                    after2dBiweek = ConvertDateToIntList(after.EndOfBiweekly),
                });
            else
                return Json(new
                {
                    start1stBiweek = first.StartOfBiweekly.ToString("dd/MM/yyyy"),
                    end1stBiweek = first.EndOfBiweekly.ToString("dd/MM/yyyy"),
                    start2dBiweek = last.StartOfBiweekly.ToString("dd/MM/yyyy"),
                    end2dBiweek = last.EndOfBiweekly.ToString("dd/MM/yyyy")
                });
        }

        public List<int> ConvertDateToIntList(DateTime date)
        {
            return date.ToString("dd/MM/yyyy").Split('/').Select(x => int.Parse(x)).ToList();
        }

        public Models.BiweeklyPayment.CreateOrUpdateModel PrepareModel(Models.BiweeklyPayment.CreateOrUpdateModel model)
        {
            var types = Enum.GetValues(typeof(PaymentFileType)).Cast<PaymentFileType>().ToList();
            var list = types.Select(x => new
            {
                Id = (int)x,
                Name = x.GetDisplayName()
            });
            model.Types = new SelectList(list, "Id", "Name");
            return model;
        }

        public List<PayrollEmployee> GetFilteredEmployees()
        {
            var payrollEmployees = _payrollEmployeeService.GetAll()
                .Where(x => x.PayrollSalaries.Any())
                .ToList().Where(x => (x.GetCurrentJob()?.Id ?? 0) > 0 &&
                !string.IsNullOrEmpty(x.GetCurrentJob()?.WorkSchedule))
                .ToList();
            var partnerRole = _customerService.GetCustomerRoleBySystemName("Partner");
            var employees = new List<PayrollEmployee>();
            foreach (var payrollEmployee in payrollEmployees)
            {
                var customer = _customerService.GetCustomerById(payrollEmployee.CustomerId);
                if (customer != null)
                    if (!customer.GetCustomerRoleIds().Contains(partnerRole.Id))
                        employees.Add(payrollEmployee);
            }
            return employees;
        }

        public IActionResult Index()
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.BiweeklyPayment))
                return AccessDeniedView();

            var model = new FiltersXlsModel();

            var jobs = _jobCatalogService.GetAll()
                .Where(x => !x.Deleted)
                .Select(x => new { x.Id, x.Name })
                .ToList();

            model.AvailableJobs = jobs.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString(),
            }).ToList();

            return View("~/Plugins/Teed.Plugin.Payroll/Views/BiweeklyPayment/Index.cshtml", model);
        }

        [HttpPost]
        public IActionResult ListData()
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.BiweeklyPayment))
                return AccessDeniedView();

            var datesData = new List<DatesData>();
            var currentDate = DateTime.Now.AddMonths(1);
            var employees = GetFilteredEmployees();
            var minimuSalary = _minimumWagesCatalogService.GetYearWage(DateTime.Now.Year);
            var festiveDates = _assistanceService.GetFestiveDatesFromGroceries()
                .Where(x => !x.DontApplyToPayroll).ToList();
            for (int i = currentDate.Month; i > 0; i--)
            {
                // HERE
                var lastBiweek = BiweeklyDatesHelper.GetBiweeklyDates(new DateTime(currentDate.Year, i, DateTime.DaysInMonth(currentDate.Year, i)).Date, festiveDates).EndOfBiweekly;
                var firstBiweek = BiweeklyDatesHelper.GetBiweeklyDates(new DateTime(currentDate.Year, i, 15).Date, festiveDates).EndOfBiweekly;

                var employeesLastWeek = GetEmployeesOfBiweekly(employees, lastBiweek);
                datesData.Add(new DatesData
                {
                    Date = lastBiweek,
                    BasePayment = employeesLastWeek.Select(x => x.GetCurrentSalaryValue()).DefaultIfEmpty().Sum(),
                    //TotalPaid = GetTotalPaidByDate(lastBiweek),
                    DiscountIncidents = GetTotalOfIncidents(employeesLastWeek, lastBiweek, minimuSalary, festiveDates, 3),
                    DiscountDelays = GetTotalOfIncidents(employeesLastWeek, lastBiweek, minimuSalary, festiveDates, 4),
                    BonusTotal = GetTotalOfBonusesByBiweek(lastBiweek, employeesLastWeek, festiveDates, out _),
                    TotalBiweekly = GetTotalOfBiweekly(employeesLastWeek, minimuSalary, lastBiweek),
                    TotalEmployees = employeesLastWeek.Count()
                });

                var employeesFirstWeek = GetEmployeesOfBiweekly(employees, firstBiweek);
                datesData.Add(new DatesData
                {
                    Date = firstBiweek,
                    BasePayment = employeesFirstWeek.Select(x => x.GetCurrentSalaryValue()).DefaultIfEmpty().Sum(),
                    //TotalPaid = GetTotalPaidByDate(firstBiweek),
                    DiscountIncidents = GetTotalOfIncidents(employeesFirstWeek, firstBiweek, minimuSalary, festiveDates, 3),
                    DiscountDelays = GetTotalOfIncidents(employeesFirstWeek, firstBiweek, minimuSalary, festiveDates, 4),
                    BonusTotal = GetTotalOfBonusesByBiweek(firstBiweek, employeesFirstWeek, festiveDates, out _),
                    TotalBiweekly = GetTotalOfBiweekly(employeesFirstWeek, minimuSalary, firstBiweek),
                    TotalEmployees = employeesFirstWeek.Count()
                });
            }

            var gridModel = new DataSourceResult
            {
                Data = datesData.OrderByDescending(x => x.Date).Select(x => new
                {
                    Date = x.Date.ToString("dd/MM/yyyy"),
                    BasePayment = x.BasePayment.ToString("C"),
                    //TotalPaid = TotalPaid.ToString("C"),
                    DiscountIncidents = x.DiscountIncidents.ToString("C"),
                    DiscountDelays = x.DiscountDelays.ToString("C"),
                    BonusTotal = x.BonusTotal.ToString("C"),
                    TotalBiweekly = x.TotalBiweekly.ToString("C"),
                    x.TotalEmployees
                }).ToList(),
                Total = datesData.Count()
            };

            return Json(gridModel);
        }

        public IActionResult PaymentsOfDate(string date)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.BiweeklyPayment))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Payroll/Views/BiweeklyPayment/PaymentsOfDate.cshtml", date);
        }

        [HttpPost]
        public IActionResult ListPaymentsData(DataSourceRequest command, string date)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.BiweeklyPayment))
                return AccessDeniedView();
            if (date.Contains("-"))
                date = date.Replace("-", "/");
            var parsedDate = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var salaries = _payrollSalaryService.GetAll().Where(x => x.NetIncome > 0).ToList();
            var salaryEmployeeIds = salaries?.Select(x => x.PayrollEmployeeId).ToList();
            var biweekPayments = _biweeklyPaymentService.GetAll().Where(x => x.Payday == parsedDate.Date).ToList();

            var isPayrollManager = _permissionService.Authorize(TeedPayrollPermissionProvider.PayrollManager);
            var currentUserId = _workContext.CurrentCustomer.Id;
            var payrollOfBoss = _payrollEmployeeService.GetByCustomerId(currentUserId);
            if (payrollOfBoss == null && !isPayrollManager)
                return Ok();
            var subJobsIds = _jobCatalogService.GetAllByParentId(payrollOfBoss.GetCurrentJob()?.Id ?? 0)
                .Select(x => x.Id).ToList();
            var employees = new List<PayrollEmployee>();
            if (!isPayrollManager)
                employees = GetFilteredEmployees()
                    .Where(x => subJobsIds.Contains(x.GetCurrentJob()?.Id ?? 0)).ToList();
            else
                employees = GetFilteredEmployees();
            var minimuSalary = _minimumWagesCatalogService.GetYearWage(DateTime.Now.Year);
            var employeesOfBiweekly = GetEmployeesOfBiweekly(employees, parsedDate);
            var paged = new PagedList<PayrollEmployee>(employeesOfBiweekly, command.Page - 1, command.PageSize).ToList();

            var festiveDates = _assistanceService.GetFestiveDatesFromGroceries()
                .Where(x => !x.DontApplyToPayroll).ToList();
            var gridModel = new DataSourceResult
            {
                Data = paged.Select(x => new
                {
                    x.Id,
                    Name = x.GetFullName(),
                    BasePayment = x.GetCurrentSalaryValue().ToString("C"),
                    //TotalPaid =
                    // biweekPayments.Where(y => y.PayrollEmployee.Id == x.Id).FirstOrDefault() != null ?
                    // biweekPayments.Where(y => y.PayrollEmployee.Id == x.Id).FirstOrDefault().OriginalPayment
                    // : 0,
                    DiscountIncidents = GetTotalOfIncidents(new List<PayrollEmployee> { x }, parsedDate, minimuSalary, festiveDates, 3).ToString("C"),
                    DiscountDelays = GetTotalOfIncidents(new List<PayrollEmployee> { x }, parsedDate, minimuSalary, festiveDates, 4).ToString("C"),
                    BonusTotal = GetTotalOfBonusesByBiweek(parsedDate, new List<PayrollEmployee> { x }, festiveDates, out _).ToString("C"),
                    TotalBiweekly = PaymentAfterIncidentsAndBonuses(x, minimuSalary, parsedDate).ToString("C"),
                    IsPaid =
                        biweekPayments.Where(y => y.PayrollEmployee.Id == x.Id).FirstOrDefault() == null ? "Por pagar" : "Pagado"
                }).ToList(),
                Total = employeesOfBiweekly.Count()
            };

            return Json(gridModel);
        }

        public IActionResult PaymentOfEmployee(int Id, string date)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.BiweeklyPayment))
                return AccessDeniedView();
            var model = new Models.BiweeklyPayment.CreateOrUpdateModel();
            PrepareModel(model);
            var parseDate = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture).Date;
            var festiveDates = _assistanceService.GetFestiveDatesFromGroceries()
                .Where(x => !x.DontApplyToPayroll).ToList();
            var dates = BiweeklyDatesHelper.GetBiweeklyDates(parseDate, festiveDates);
            var biweeklyPayment = _biweeklyPaymentService.GetAllByPayrollEmployeeId(Id)
                .Where(x => x.Payday == parseDate).FirstOrDefault();
            var payrollEmployee = _payrollEmployeeService.GetById(Id);
            if (payrollEmployee == null)
                return RedirectToAction("PaymentsOfDate", new { date });
            if (payrollEmployee.EmployeeStatusId == (int)EmployeeStatus.Discharged &&
                !GetEmployeesOfBiweekly(new List<PayrollEmployee> { payrollEmployee }, parseDate).Any())
                model.CannotBePaid = true;
            model.Date = date;
            model.EmployeeName = payrollEmployee.GetFullName();
            if (biweeklyPayment != null)
            {
                model.Id = biweeklyPayment.Id;
                model.Paid = biweeklyPayment.Paid;
                model.OriginalPayment = biweeklyPayment.OriginalPayment;
                model.Payday = biweeklyPayment.Payday;
                model.CustomerThatReportedId = biweeklyPayment.CustomerThatReportedId;
                model.PayrollEmployee = biweeklyPayment.PayrollEmployee;
                model.PayrollSalary = biweeklyPayment.PayrollSalary;
                model.BiweeklyPaymentFiles =
                    biweeklyPayment.BiweeklyPaymentFiles.Where(x => !x.Deleted).ToList();
            }
            if (payrollEmployee != null && payrollEmployee.PayrollSalaries.Any())
            {
                var incidents = _incidentService.GetAllByPayrollEmployeeId(Id)
                    .ToList();
                var minimuSalary = _minimumWagesCatalogService.GetYearWage(DateTime.Now.Year);
                if (biweeklyPayment == null)
                    model.NetIncome =
                        Math.Round(payrollEmployee.PayrollSalaries.OrderByDescending(x => x.ApplyDate).FirstOrDefault().NetIncome / 2, 2);
                else
                    model.NetIncome =
                    Math.Round(payrollEmployee.PayrollSalaries
                    .Where(x => x.Id == biweeklyPayment.PayrollSalaryId).OrderByDescending(x => x.ApplyDate).FirstOrDefault().NetIncome / 2, 2);

                model.IncidentsTotal = PaymentAfterIncidentsAndBonuses(payrollEmployee, minimuSalary, parseDate, false, true);
                model.IncidentsTotal = model.IncidentsTotal < 0 ? -1 * model.IncidentsTotal : model.IncidentsTotal;
                model.BonusesTotal = PaymentAfterIncidentsAndBonuses(payrollEmployee, minimuSalary, parseDate, true);
                model.Total = PaymentAfterIncidentsAndBonuses(payrollEmployee, minimuSalary, parseDate);

                model.OriginalPayment = model.Total;
                if (biweeklyPayment == null)
                {
                    model.PayrollEmployeeId = payrollEmployee.Id;
                    model.PayrollSalaryId = payrollEmployee.PayrollSalaries.OrderByDescending(x => x.UpdatedOnUtc).FirstOrDefault().Id;
                    model.CustomerThatReportedId = _workContext.CurrentCustomer.Id;
                }
                var salary = payrollEmployee.PayrollSalaries.OrderByDescending(x => x.UpdatedOnUtc).FirstOrDefault();
                model.IncidentsBiweek = new List<string>();
                model.IncidentsPending = new List<string>();
                model.BonusesBiweek = new List<string>();
                var biweeklySalary = salary.NetIncome > 0 ? Math.Round(salary.NetIncome / 2, 2) : 0;
                foreach (var incident in incidents.Where(x => x.Type == IncidentType.Discount))
                {
                    var termValue =
                        (biweeklySalary -
                        minimuSalary.Amount) * (decimal).30;
                    var term = (int)Math.Ceiling((incident.Amount ?? 0) / termValue);
                    var incidentAmountLeft = (incident.Amount ?? 0) - incident.TermPaid;
                    var final = termValue <= incidentAmountLeft ? termValue : incidentAmountLeft;
                    model.IncidentsBiweek.Add($"Descuento: " + incident.Reason + " - " + final.ToString("C"));
                    if (term - (incident.Term ?? 0) > 0)
                        model.IncidentsPending.Add($"Descuento: " + incident.Reason + " - Término: " + (term - (incident.Term ?? 0)));
                }
                // HERE
                //var dateLast = new DateTime(parseDate.Year, parseDate.AddMonths(-1).Month,
                //    DateTime.DaysInMonth(parseDate.Year, parseDate.AddMonths(-1).Month));
                //var firstDate = parseDate.Day ==
                //    15 ?
                //    dateLast
                //    :
                //    new DateTime(parseDate.Year, parseDate.Month, 16);
                var absence = incidents
                    .Where(x => x.Type == IncidentType.Absence &&
                    dates.StartOfBiweekly.Date <= x.Date && x.Date <= dates.EndOfBiweekly.Date &&
                    !x.Justified).ToList();
                var delay = incidents
                    .Where(x => x.Type == IncidentType.Delay &&
                    dates.StartOfBiweekly.Date <= x.Date && x.Date <= dates.EndOfBiweekly.Date &&
                    !x.Justified).ToList();
                if (absence.Count() > 0)
                    model.IncidentsBiweek.Add($"Se descontarán {absence.Count()} días por faltas.");
                var absencesByDelays = (int)Math.Floor((decimal)(delay.Count() / 3));
                if (absencesByDelays > 0)
                    model.IncidentsBiweek.Add($"Se descontarán {absencesByDelays} días como faltas por acumulación de retardos ({delay.Count()} retardos de esta quincena).");

                _ = GetTotalOfBonusesByBiweek(parseDate, new List<PayrollEmployee> { payrollEmployee }, festiveDates, out List<BonusApplication> appliableBonuses);
                foreach (var appliableBonus in appliableBonuses)
                {
                    var bonus = appliableBonus.Bonus;
                    var bonusType = ((FrequencyType)bonus.FrequencyTypeId) == FrequencyType.ByBiweek ?
                        "de forma quincenal" : $"por el día {appliableBonus.Date:dd/MM/yyyy}";
                    model.BonusesBiweek.Add($"Bono \"{bonus.Name}\" aplicado {bonusType}.");
                }
                return View("~/Plugins/Teed.Plugin.Payroll/Views/BiweeklyPayment/PaymentOfEmployee.cshtml", model);
            }
            return RedirectToAction("PaymentsOfDate", new { date });
        }

        public decimal PaymentAfterIncidentsAndBonuses(PayrollEmployee employee, MinimumWagesCatalog minimuSalary,
            DateTime date, bool returnBonusOnly = false, bool returnIncidentsOnly = false)
        {
            var biweeklyTotal = (decimal)0;
            // Get term with 30% calculus
            var minimum = minimuSalary.Amount;
            var incidents = employee.Incidents.Where(x => x.AppliedDate == null).ToList();
            var salary = employee.PayrollSalaries
                .OrderByDescending(x => x.ApplyDate)
                .FirstOrDefault();
            var festiveDates = _assistanceService.GetFestiveDatesFromGroceries()
                .Where(x => !x.DontApplyToPayroll).ToList();
            var bonusesTotal = GetTotalOfBonusesByBiweek(date, new List<PayrollEmployee> { employee }, festiveDates, out _);

            if (salary != null && !returnBonusOnly)
            {
                var incidentsTotal = GetTotalOfIncidents(new List<PayrollEmployee> { employee }, date, minimuSalary, festiveDates);
                var biweeklySalary = salary.NetIncome > 0 ? Math.Round(salary.NetIncome / 2, 2) : 0;
                if (returnIncidentsOnly)
                    return incidentsTotal;
                else
                {
                    biweeklyTotal += biweeklySalary - (incidentsTotal < 0 ? -1 * incidentsTotal : incidentsTotal);
                    return biweeklyTotal + bonusesTotal;
                }
            }
            else if (salary != null && returnBonusOnly)
                return bonusesTotal;
            return biweeklyTotal;
        }

        public void IncidentsOfPayment(PayrollEmployee employee, MinimumWagesCatalog minimuSalary, DateTime date, List<FestiveDateModel> festiveDates)
        {
            var biweeklyTotal = (decimal)0;
            // Get term with 30% calculus
            var minimum = minimuSalary.Amount;
            var incidents = employee.Incidents.Where(x => x.AppliedDate == null).ToList();
            var salary = employee.PayrollSalaries
                .OrderByDescending(x => x.ApplyDate)
                .FirstOrDefault();

            if (salary != null)
            {
                var biweeklySalary = salary.NetIncome > 0 ? Math.Round(salary.NetIncome / 2, 2) : 0;
                foreach (var incident in incidents.Where(x => x.Type == IncidentType.Discount))
                {
                    var termValue = (biweeklySalary - minimum) * (decimal).30;
                    var term = (int)Math.Ceiling((incident.Amount ?? 0) / termValue);
                    var incidentAmountLeft = (incident.Amount ?? 0) - incident.TermPaid;
                    var final = termValue <= incidentAmountLeft ? termValue : incidentAmountLeft;

                    incident.TermPaid += final;
                    var termCurrent = (incident.Term ?? 0) + 1;
                    incident.Term = termCurrent;
                    if (incident.Amount <= (incident.Term ?? 0))
                        incident.AppliedDate = date;
                    _incidentService.Update(incident);
                }
                // HERE
                var dates = BiweeklyDatesHelper.GetBiweeklyDates(date, festiveDates);
                //var dateLast = new DateTime(date.Year, date.AddMonths(-1).Month, DateTime.DaysInMonth(date.Year, date.AddMonths(-1).Month));
                //var firstDate = date.Day ==
                //    15 ?
                //    dateLast
                //    :
                //    new DateTime(date.Year, date.Month, 16);
                var otherIncidents = incidents
                    .Where(x => x.Type == IncidentType.Absence &&
                    dates.StartOfBiweekly.Date <= x.Date && x.Date <= dates.EndOfBiweekly.Date &&
                    !x.Justified)
                    .Where(x => x.Type == IncidentType.Delay &&
                    dates.StartOfBiweekly.Date <= x.Date && x.Date <= dates.EndOfBiweekly.Date &&
                    !x.Justified).ToList();
                foreach (var item in otherIncidents)
                {
                    item.AppliedDate = date;
                    _incidentService.Update(item);
                }
            }
        }

        [HttpGet]
        public IActionResult DownloadBiweekly(string date)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.BiweeklyPayment))
                return AccessDeniedView();
            if (date.Contains("-"))
                date = date.Replace("-", "/");
            var parsedDate = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var salaries = _payrollSalaryService.GetAll().Where(x => x.NetIncome > 0).ToList();
            var salaryEmployeeIds = salaries?.Select(x => x.PayrollEmployeeId).ToList();
            var biweekPayments = _biweeklyPaymentService.GetAll().Where(x => x.Payday == parsedDate.Date).ToList();

            var isPayrollManager = _permissionService.Authorize(TeedPayrollPermissionProvider.PayrollManager);
            var currentUserId = _workContext.CurrentCustomer.Id;
            var payrollOfBoss = _payrollEmployeeService.GetByCustomerId(currentUserId);
            if (payrollOfBoss == null && !isPayrollManager)
                return Ok();
            var subJobsIds = _jobCatalogService.GetAllByParentId(payrollOfBoss.GetCurrentJob()?.Id ?? 0)
                .Select(x => x.Id).ToList();
            var employees = new List<PayrollEmployee>();
            if (!isPayrollManager)
                employees = GetFilteredEmployees()
                    .Where(x => subJobsIds.Contains(x.GetCurrentJob()?.Id ?? 0)).ToList();
            else
                employees = GetFilteredEmployees();
            var minimuSalary = _minimumWagesCatalogService.GetYearWage(DateTime.Now.Year);
            var employeesOfBiweekly = GetEmployeesOfBiweekly(employees, parsedDate);

            var festiveDates = _assistanceService.GetFestiveDatesFromGroceries()
                .Where(x => !x.DontApplyToPayroll).ToList();
            var info = employeesOfBiweekly.Select(x => new
            {
                x.Id,
                Name = x.GetFullName(),
                BasePayment = x.GetCurrentSalaryValue(),
                DiscountIncidents = GetTotalOfIncidents(new List<PayrollEmployee> { x }, parsedDate, minimuSalary, festiveDates, 3),
                DiscountDelays = GetTotalOfIncidents(new List<PayrollEmployee> { x }, parsedDate, minimuSalary, festiveDates, 4),
                TotalBiweekly = PaymentAfterIncidentsAndBonuses(x, minimuSalary, parsedDate),
                IsPaid =
                        biweekPayments.Where(y => y.PayrollEmployee.Id == x.Id).FirstOrDefault() == null ? "Por pagar" : "Pagado"
            }).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Nombre";
                    worksheet.Cells[row, 2].Value = "Sueldo base";
                    worksheet.Cells[row, 3].Value = "Descuentos por incidencias";
                    worksheet.Cells[row, 4].Value = "Descuentos por inasistencias o retardos";
                    worksheet.Cells[row, 5].Value = "Nómina neta";
                    worksheet.Cells[row, 6].Value = "Estatus";
                    row++;

                    foreach (var employee in info)
                    {
                        worksheet.Cells[row, 1].Value = employee.Name;
                        worksheet.Cells[row, 2].Value = employee.BasePayment;
                        worksheet.Cells[row, 3].Value = employee.DiscountIncidents;
                        worksheet.Cells[row, 4].Value = employee.DiscountDelays;
                        worksheet.Cells[row, 5].Value = employee.TotalBiweekly;
                        worksheet.Cells[row, 6].Value = employee.IsPaid;
                        row++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"Nómina quincenal " + parsedDate.ToString("dd-MM-yyyy") + ".xlsx");
            }
        }

        //public decimal GetIncidentsOfDate(DateTime date, List<Incident> incidents, int employeeId = 0)
        //{
        //    var dateString = date.ToString("dd/MM/yyyy");
        //    var currentIncidents = incidents
        //        .ToList();
        //    if (currentIncidents.Any())
        //    {
        //        var total = (decimal)0;
        //        var discounts = currentIncidents.Where(x => x.Type == IncidentType.Discount).ToList();
        //        var delays = currentIncidents.Where(x => x.Type == IncidentType.Delay).ToList();
        //        var absence = currentIncidents.Where(x => x.Type == IncidentType.Absence && x.File == null).ToList();
        //        if (employeeId > 0)
        //        {
        //            discounts = discounts.Where(x => x.PayrollEmployeeId == employeeId).ToList();
        //            delays = delays.Where(x => x.PayrollEmployeeId == employeeId).ToList();
        //            absence = absence.Where(x => x.PayrollEmployeeId == employeeId).ToList();
        //        }

        //        total += discounts.Select(x => x.Amount).DefaultIfEmpty().Sum() ?? 0;
        //        var absencesByEmployee = absence.GroupBy(x => x.PayrollEmployee).ToList();
        //        var delaysByEmployee = delays.GroupBy(x => x.PayrollEmployee).ToList();

        //        foreach (var absences in absencesByEmployee)
        //        {
        //            var currentDelays = delaysByEmployee.Where(x => x.Key.Id == absences.Key.Id).ToList();
        //            var currentSalary =
        //                absences.Key.PayrollSalaries?.OrderByDescending(x => x.UpdatedOnUtc)
        //                .FirstOrDefault().NetIncome;
        //            var salaryByDay = (decimal)0;
        //            if (date.Day == 15)
        //                salaryByDay = currentSalary / date.Day ?? 0;
        //            else if (date.Day > 15)
        //            {
        //                var days = DateTime.DaysInMonth(date.Year, date.Month) - 15;
        //                salaryByDay = currentSalary / days ?? 0;
        //            }
        //            total += salaryByDay * absences.Count();
        //            if (currentDelays.Any())
        //            {
        //                var innerDelays = currentDelays[0].ToList().Count();
        //                total += salaryByDay * Math.Floor((decimal)innerDelays / 3);
        //            }
        //        }
        //        if (!absencesByEmployee.Any())
        //        {
        //            foreach (var delaysInner in delaysByEmployee)
        //            {
        //                var currentSalary =
        //                    delaysInner.Key.PayrollSalaries?.OrderByDescending(x => x.UpdatedOnUtc)
        //                    .FirstOrDefault().NetIncome;
        //                var salaryByDay = (decimal)0;
        //                if (date.Day == 15)
        //                    salaryByDay = currentSalary / date.Day ?? 0;
        //                else if (date.Day > 15)
        //                {
        //                    var days = DateTime.DaysInMonth(date.Year, date.Month) - 15;
        //                    salaryByDay = currentSalary / days ?? 0;
        //                }
        //                if (delaysInner.Any())
        //                    total += salaryByDay * Math.Floor((decimal)delaysInner.Count() / 3);
        //            }
        //        }

        //        return Math.Round(total, 2);
        //    }
        //    else
        //        return 0;
        //}

        public List<PayrollEmployee> GetEmployeesOfBiweekly(List<PayrollEmployee> employees, DateTime date)
        {
            return employees.Where(x => x.DateOfDeparture >= date || x.DateOfDeparture == null).ToList();
        }

        public decimal GetTotalOfBiweekly(List<PayrollEmployee> employees, MinimumWagesCatalog minimuSalary, DateTime date)
        {
            var biweeklyTotal = (decimal)0;
            foreach (var employee in employees)
            {
                // Get term with 30% calculus
                var minimum = minimuSalary.Amount;
                var salary = employee.PayrollSalaries
                    .OrderByDescending(x => x.ApplyDate)
                    .FirstOrDefault();
                biweeklyTotal += PaymentAfterIncidentsAndBonuses(employee, minimuSalary, date);
            }
            return biweeklyTotal;
        }

        public decimal GetTotalOfIncidents(List<PayrollEmployee> employees, DateTime date, MinimumWagesCatalog minimumSalary,
            List<FestiveDateModel> festiveDates, int incidentType = 0)
        {
            var incidentsTotal = (decimal)0;
            var minimum = minimumSalary.Amount;
            // HERE
            var dates = BiweeklyDatesHelper.GetBiweeklyDates(date, festiveDates);
            //var dateLast = new DateTime(date.Year, date.AddMonths(-1).Month, DateTime.DaysInMonth(date.Year, date.AddMonths(-1).Month));
            //var firstDate = date.Day ==
            //    15 ?
            //    dateLast
            //    :
            //    new DateTime(date.Year, date.Month, 16);
            foreach (var employee in employees)
            {
                var salary = employee.PayrollSalaries
                    .OrderByDescending(x => x.ApplyDate)
                    .FirstOrDefault();
                if (salary != null)
                {
                    var salaryPerDay = salary.NetIncome / dates.EndOfBiweekly.Day;
                    var biweeklySalary = salary.NetIncome > 0 ? Math.Round(salary.NetIncome / 2, 2) : 0;
                    if (employee.Incidents.Count > 0)
                    {
                        var discountsTotal = (decimal)0;
                        foreach (var incident in employee.Incidents.Where(x => x.Type == IncidentType.Discount))
                        {
                            var termValue = (biweeklySalary - minimum) * (decimal).30;
                            var term = (int)Math.Ceiling((incident.Amount ?? 0) / termValue);
                            var incidentAmountLeft = (incident.Amount ?? 0) - incident.TermPaid;
                            var final = termValue <= incidentAmountLeft ? termValue : incidentAmountLeft;
                            discountsTotal += final;
                        }
                        var absence = employee.Incidents
                            .Where(x => x.Type == IncidentType.Absence &&
                            dates.StartOfBiweekly.Date <= x.Date && x.Date <= dates.EndOfBiweekly.Date &&
                            !x.Justified).ToList();
                        var delay = employee.Incidents
                            .Where(x => x.Type == IncidentType.Delay &&
                            dates.StartOfBiweekly.Date <= x.Date && x.Date <= dates.EndOfBiweekly.Date &&
                            !x.Justified).ToList();
                        var abcences = absence.Count();
                        var delays =
                            (int)Math.Floor((decimal)(delay.Count() / 3));
                        if (incidentType == 0)
                        {
                            incidentsTotal += discountsTotal - ((abcences + delays) * salaryPerDay);
                        }
                        else if (incidentType == (int)IncidentType.Absence)
                        {
                            incidentsTotal += (abcences * salaryPerDay);
                        }
                        else if (incidentType == (int)IncidentType.Delay)
                        {
                            incidentsTotal += (delays * salaryPerDay);
                        }
                        else if (incidentType == (int)IncidentType.Discount)
                        {
                            incidentsTotal += discountsTotal;
                        }
                        else if (incidentType == 4)
                        {
                            incidentsTotal += ((abcences + delays) * salaryPerDay);
                        }
                    }
                }
            }
            return incidentsTotal;
        }

        private decimal GetTotalOfBonusesByBiweek(DateTime startOfBiweek, List<PayrollEmployee> payrollEmployees,
            List<FestiveDateModel> festiveDates, out List<BonusApplication> appliableBonuses)
        {
            var final = (decimal)0;
            var firstDate = DateTime.Now;
            var lastDate = DateTime.Now;
            if (startOfBiweek.Day <= 15)
            {
                // HERE
                var dates = BiweeklyDatesHelper.GetBiweeklyDates(new DateTime(startOfBiweek.Year, startOfBiweek.Month, 1), festiveDates);
                firstDate = dates.StartOfBiweekly;
                lastDate = dates.EndOfBiweekly;
            }
            else
            {
                // HERE
                var dates = BiweeklyDatesHelper.GetBiweeklyDates(new DateTime(startOfBiweek.Year, startOfBiweek.Month, 16), festiveDates);
                firstDate = dates.StartOfBiweekly;
                lastDate = dates.EndOfBiweekly;
            }
            var employeeIds = payrollEmployees.Select(x => x.Id).ToList();
            var test = payrollEmployees.Where(x => x.Id == 83 || x.Id == 33).ToList();
            var jobIds = payrollEmployees.Where(x => x.GetCurrentJob()?.Id != null).Select(x => x.GetCurrentJob()?.Id ?? 0).Distinct().ToList();
            appliableBonuses = _bonusApplicationService.GetAll()
                .Where(x => x.WillApply && (employeeIds.Contains(x.EntityId) || jobIds.Contains(x.EntityId)))
                .ToList()
                .Where(x => BonusApplicationCorrectDateForComparison(x, festiveDates, firstDate, lastDate))
                .ToList();

            foreach (var payrollEmployee in payrollEmployees)
            {
                // Filter appliable bonuses when group, manual and incident equals any
                var bonusesToRemove = new List<BonusApplication>();
                foreach (var appliableBonus in appliableBonuses.Where(x =>
                    x.Bonus.BonusTypeId == (int)BonusType.Collective &&
                    x.Bonus.ConditionTypeId == (int)ConditionType.Manual))
                {
                    var incidentsQuery = _incidentService.GetAll()
                            .Where(x => !x.Deleted && payrollEmployee.Id == x.PayrollEmployeeId &&
                            x.Type != IncidentType.Discount && !x.Justified);
                    var anyIncidentsOfDates = false;
                    if (appliableBonus.Bonus.FrequencyTypeId == (int)FrequencyType.ByDay)
                    {
                        anyIncidentsOfDates = incidentsQuery
                            .Where(x => x.Date == appliableBonus.Date).Any();
                    }
                    else if (appliableBonus.Bonus.FrequencyTypeId == (int)FrequencyType.ByBiweek)
                    {
                        anyIncidentsOfDates = incidentsQuery
                            .Where(x => firstDate <= x.Date && x.Date <= lastDate).Any();
                    }
                    if (anyIncidentsOfDates)
                        bonusesToRemove.Add(appliableBonus);
                }
                foreach (var bonus in bonusesToRemove)
                    appliableBonuses.Remove(bonus);

                var bonusIds = appliableBonuses.Select(x => x.BonusId).Distinct().ToList();
                var bonusesOfBiweekly = _bonusService.GetAll()
                    .Where(x => bonusIds.Contains(x.Id))
                    .ToList();
                foreach (var bonus in bonusesOfBiweekly)
                {
                    var bonusApplications = appliableBonuses.Where(x => x.BonusId == bonus.Id)
                        .ToList();
                    var frequencyType = (FrequencyType)bonus.FrequencyTypeId;
                    var bonusType = (BonusType)bonus.BonusTypeId;
                    var entityTpe = bonus.BonusTypeId == (int)BonusType.Individual ? EntityType.Employee : EntityType.Job;

                    if (entityTpe == EntityType.Employee)
                        bonusApplications = bonusApplications
                            .Where(x => x.EntityId == payrollEmployee.Id && x.EntityTypeId == (int)entityTpe)
                            .ToList();
                    else
                        bonusApplications = bonusApplications
                            .Where(x => x.EntityId == (payrollEmployee.GetCurrentJob()?.Id ?? 0) && x.EntityTypeId == (int)entityTpe)
                            .ToList();
                    if (bonusApplications.Any())
                        foreach (var bonusApplication in bonusApplications)
                            final += bonusApplication.Amount;
                }
            }
            return final;
        }

        public bool BonusApplicationCorrectDateForComparison(BonusApplication bonusApplication, List<FestiveDateModel> festiveDates,
            DateTime firstDate, DateTime lastDate)
        {
            var isWithinBiweek = false;
            if (bonusApplication.Bonus.FrequencyTypeId == (int)FrequencyType.ByBiweek)
            {
                var startOfBonusBiweek = BiweeklyDatesHelper.GetBiweeklyDates(bonusApplication.Date, festiveDates).StartOfBiweekly;
                if (firstDate <= startOfBonusBiweek && startOfBonusBiweek <= lastDate)
                    isWithinBiweek = true;
            }
            else if (firstDate <= bonusApplication.Date && bonusApplication.Date <= lastDate)
                isWithinBiweek = true;
            return isWithinBiweek;
        }

        [HttpGet]
        public IActionResult BiweeklyPaymentFileDownload(int id)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.BiweeklyPayment))
                return AccessDeniedView();
            var file = _biweeklyPaymentFileService.GetById(id);
            if (file == null)
                return Ok();
            var model = new PayrollEmployeeFileModel
            {
                Title = file.PaymentFileType.GetDisplayName(),
                Extension = file.Extension,
                FileArray = file.File

            };
            return Json(model);
        }

        [HttpPost]
        public IActionResult BiweeklyPaymentFileDelete(int id)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.BiweeklyPayment))
                return AccessDeniedView();
            var file = _biweeklyPaymentFileService.GetById(id);
            if (file == null)
                return Ok();

            file.Deleted = true;
            _biweeklyPaymentFileService.Update(file);
            return Ok();
        }

        [HttpPost]
        public IActionResult BiweeklyPaymentFileAdd(AddBiweeklyPaymentFile model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            try
            {
                var minimuSalary = _minimumWagesCatalogService.GetYearWage(DateTime.Now.Year);
                var payment = new BiweeklyPayment();
                var festiveDates = _assistanceService.GetFestiveDatesFromGroceries()
                    .Where(x => !x.DontApplyToPayroll).ToList();
                if (model.BiweeklyPaymentId < 1)
                {
                    payment = new BiweeklyPayment
                    {
                        CustomerThatReportedId = model.CustomerThatReportedId,
                        OriginalPayment = model.OriginalPayment,
                        Payday = DateTime.ParseExact(model.Payday, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                        PayrollEmployeeId = model.PayrollEmployeeId,
                        PayrollSalaryId = model.PayrollSalaryId
                    };
                    var employee = _payrollEmployeeService.GetById(model.PayrollEmployeeId);
                    IncidentsOfPayment(employee, minimuSalary, DateTime.ParseExact(model.Payday, "dd/MM/yyyy", CultureInfo.InvariantCulture), festiveDates);
                    _biweeklyPaymentService.Insert(payment);
                }
                byte[] bytes = new byte[0];
                using (var ms = new MemoryStream())
                {
                    model.File.CopyTo(ms);
                    bytes = ms.ToArray();
                }
                var file = new BiweeklyPaymentFile
                {
                    File = bytes,
                    Extension = model.File.FileName.Substring(model.File.FileName.LastIndexOf(".") + 1),
                    FileMimeType = model.File.ContentType,
                    PaymentFileType = model.Type,
                    Size = (int)model.File.Length,
                    BiweeklyPaymentId = model.BiweeklyPaymentId > 0 ? model.BiweeklyPaymentId : payment.Id,
                };
                _biweeklyPaymentFileService.Insert(file);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        public IActionResult MarkPaid(MarkPiad model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            try
            {
                var minimuSalary = _minimumWagesCatalogService.GetYearWage(DateTime.Now.Year);
                var payment = new BiweeklyPayment();
                var festiveDates = _assistanceService.GetFestiveDatesFromGroceries()
                    .Where(x => !x.DontApplyToPayroll).ToList();
                if (model.BiweeklyPaymentId < 1)
                {
                    payment = new BiweeklyPayment
                    {
                        CustomerThatReportedId = model.CustomerThatReportedId,
                        OriginalPayment = model.OriginalPayment,
                        Payday = DateTime.ParseExact(model.Payday, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                        PayrollEmployeeId = model.PayrollEmployeeId,
                        PayrollSalaryId = model.PayrollSalaryId,
                        Paid = model.Paid
                    };
                    var employee = _payrollEmployeeService.GetById(model.PayrollEmployeeId);
                    IncidentsOfPayment(employee, minimuSalary, DateTime.ParseExact(model.Payday, "dd/MM/yyyy", CultureInfo.InvariantCulture), festiveDates);
                    _biweeklyPaymentService.Insert(payment);
                }
                else
                {
                    payment = _biweeklyPaymentService.GetById(model.BiweeklyPaymentId);
                    payment.Paid = model.Paid;
                    _biweeklyPaymentService.Update(payment);
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        public decimal GetTotalPaidByDate(DateTime date)
        {
            var paymentsOfDate = _biweeklyPaymentService.GetAll()
                .Where(x => x.Payday == date.Date && x.Paid).ToList();
            return paymentsOfDate.Select(x => x.OriginalPayment).DefaultIfEmpty().Sum();
        }

        public class DatesData
        {
            public DateTime Date { get; set; }
            public decimal BasePayment { get; set; }
            public decimal Bonuses { get; set; }
            public decimal DiscountIncidents { get; set; }
            public decimal DiscountDelays { get; set; }
            public decimal BonusTotal { get; set; }
            public decimal TotalBiweekly { get; set; }
            public int TotalEmployees { get; set; }
        }
    }
}