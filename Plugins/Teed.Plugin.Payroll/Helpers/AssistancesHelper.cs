using Nop.Core.Domain.Catalog;
using Nop.Services.Customers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.Assistances;
using Teed.Plugin.Payroll.Domain.Bonuses;
using Teed.Plugin.Payroll.Domain.Incidents;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;
using Teed.Plugin.Payroll.Models.Assistance;
using Teed.Plugin.Payroll.Models.BiweeklyPayment;
using Teed.Plugin.Payroll.Models.FestiveDate;
using Teed.Plugin.Payroll.Services;

namespace Teed.Plugin.Payroll.Helpers
{
    public static class AssistanceHelper
    {
        public static Tuple<List<DateOfEmployee>, List<string>> DoAssistanceModelWork(List<DateOfEmployee> model, List<PayrollEmployee> employees, DateTime today,
            AssistanceService _assistanceService, AssistanceOverrideService _assistanceOverrideService,
            ICustomerService _customerService, JobCatalogService _jobCatalogService,
            bool useWithTolerance = false)
        {
            var festiveDates = _assistanceService.GetFestiveDatesFromGroceries()
                .Where(x => !x.DontApplyToPayroll).ToList();
            var yearlyDates = festiveDates.Where(x => x.AppliesYearly).Select(x => x.Date).ToList();
            var fixedDates = festiveDates.Where(x => !x.AppliesYearly).Select(x => x.Date).ToList();
            var err = new List<string>();
            var customerIds = employees.Select(x => x.CustomerId).ToList();
            var customers = _customerService.GetAllCustomersQuery().Where(x => customerIds.Contains(x.Id)).ToList();
            var employeeIds = employees.Select(x => x.Id).ToList();
            var overriddenAssistances = _assistanceOverrideService.GetAll()
                .Where(x => employeeIds.Contains(x.PayrollEmployeeId)).ToList();

            foreach (var employee in employees)
            {
                var customer = customers.Where(x => x.Id == employee.CustomerId).FirstOrDefault();
                var assistances = _assistanceService.GetByEmployeeNumber(employee.EmployeeNumber)
                    .Where(x => !yearlyDates.Any(y => y.Day == x.CheckIn.Day && y.Month == x.CheckIn.Month) &&
                    !fixedDates.Contains(x.CheckIn)).GroupBy(x => x.CheckIn.Date);
                var overriddensOfEmployee = overriddenAssistances.Where(x => x.PayrollEmployeeId == employee.Id).ToList();
                var jobsOfEmployee = employee.JobCatalogs.Where(x => !x.Deleted).Select(x => x.JobCatalog).ToList();

                var jobConectionWorkingDays = new List<JobConectionWorkingDays>();

                foreach (var assistancesOfDay in assistances)
                {
                    var date = assistancesOfDay.Key.Date;
                    //if (date.ToString("dd-MM-yyyy") == "28-07-2022")
                    //    _ = 0;
                    var assistance = AssistanceType.Absence;

                    var jobOfEmployee = jobsOfEmployee.Where(x => x.Id ==
                        (employee.GetCurrentJob(new DateTime(date.Year, date.Month, date.Day))?.Id ?? 0)).FirstOrDefault();
                    if (jobOfEmployee != null && !string.IsNullOrEmpty(jobOfEmployee.WorkSchedule))
                    {
                        var workingDaysOfWeek = _assistanceService.GetScheduleOfEmployee(jobOfEmployee.WorkSchedule);
                        jobConectionWorkingDays.Add(new JobConectionWorkingDays
                        {
                            ApplyDate = date.Date,
                            Schedules = workingDaysOfWeek
                        });
                        var currentSchedule = workingDaysOfWeek
                            .Where(x => x.DayOfWeek == date.DayOfWeek).FirstOrDefault();

                        if (currentSchedule != null)
                        {
                            var checkInOfCurrentDay = new DateTime(date.Year, date.Month, date.Day, currentSchedule.CheckIn, useWithTolerance ? 15 : 0, 0).AddMinutes(currentSchedule.CheckInMinutes);
                            var checkOutOfCurrentDay = new DateTime(date.Year, date.Month, date.Day, currentSchedule.CheckOut, 0, 0).AddMinutes(currentSchedule.CheckOutMinutes);

                            var overrideOfDate = overriddensOfEmployee.Where(x => x.OverriddenDate == date).FirstOrDefault();
                            if (overrideOfDate == null)
                            {
                                var festiveDate = festiveDates.Where(x => x.Date == date.Date || (x.AppliesYearly && x.Date.ToString("dd/MM") == date.ToString("dd/MM"))).FirstOrDefault();
                                if (festiveDate != null)
                                    assistance = AssistanceType.InTime;
                                else if (assistancesOfDay.Where(x => x.CheckIn <= checkInOfCurrentDay).Any())
                                    assistance = AssistanceType.InTime;
                                else if (assistancesOfDay.Where(x => x.CheckIn > checkInOfCurrentDay).Any())
                                    assistance = AssistanceType.Delay;
                                else if (assistancesOfDay.Where(x => !(x.CheckIn <= checkInOfCurrentDay || x.CheckIn > checkInOfCurrentDay)).Any())
                                    assistance = AssistanceType.Absence;
                            }
                            else
                                assistance = (AssistanceType)overrideOfDate.Type;

                            var dateOfEmployee = new DateOfEmployee
                            {
                                CustomerId = employee.CustomerId,
                                EmployeeNumber = employee.EmployeeNumber,
                                Name = !string.IsNullOrWhiteSpace(employee.GetFullName()) && !string.IsNullOrEmpty(employee.GetFullName()) ? employee.GetFullName() :
                                    customer != null ? customer.GetFullName() : "Nombre no especificado",
                                Date = date,
                                Assistance = (int)assistance,
                                CheckIn = checkInOfCurrentDay,
                                CheckOut = checkOutOfCurrentDay,
                                TimesRegistred = assistancesOfDay != null ?
                                    assistancesOfDay.Select(x => x.CheckIn).OrderBy(x => x).ToList() :
                                    new List<DateTime>(),
                                ExtraInfo = "[From assistances]",
                                Overriden = overrideOfDate != null,
                                OverrideLog = overrideOfDate?.Log,
                                OverrideComment = overrideOfDate?.Comment,
                            };
                            model.Add(dateOfEmployee);
                        }
                        else
                            err.Add("No schedule found - " + employee.GetFullName());
                    }
                    else
                        err.Add("No job or without schedule - " + employee.GetFullName());
                }

                var dateCheckerStartedWorking = new DateTime(2020, 9, 12).Date;
                //var daysOfWork = workingDaysOfWeek.Select(x => x.DayOfWeek).ToList();
                var dates = model.Any() ? model.OrderByDescending(x => x.Date).Select(x => x.Date.Date).ToList() : new List<DateTime>();
                var firstAssistance =
                    ((employee.DateOfAdmission < dateCheckerStartedWorking || employee.DateOfAdmission == null ? dateCheckerStartedWorking : employee.DateOfAdmission) ?? DateTime.Now).Date;
                var lastAssitance = today;

                var datesBetween = Enumerable.Range(0, 1 + lastAssitance.Subtract(firstAssistance).Days)
                  .Select(offset => DateTime.ParseExact(firstAssistance.AddDays(offset).ToString("dd-MM-yyyy"), "dd-MM-yyyy", CultureInfo.InvariantCulture))
                  .Where(x => !dates.Contains(x.Date))
                  .ToList();

                var jobOfToday = jobConectionWorkingDays.Where(x => x.ApplyDate == today.Date).FirstOrDefault();

                if (jobOfToday != null)
                    if (datesBetween.Where(x => x == today.Date).Any() &&
                        jobOfToday.Schedules.Where(x => x.DayOfWeek == today.DayOfWeek).Any())
                    {
                        var nowSchedule = jobOfToday.Schedules.Where(x => x.DayOfWeek == today.DayOfWeek).FirstOrDefault();
                        var nowCheckIn = new DateTime(today.Year, today.Month, today.Day, nowSchedule.CheckIn, 0, 0);
                        if (today < nowCheckIn)
                            datesBetween.RemoveAt(datesBetween.IndexOf(today.Date));
                    }

                foreach (var dateBetween in datesBetween)
                {
                    var scheduleOfDay = jobConectionWorkingDays.Where(x => x.ApplyDate == dateBetween).FirstOrDefault();
                    if (scheduleOfDay == null)
                    {
                        var workSchedule = jobsOfEmployee.Where(x => x.Id ==
                            (employee.GetCurrentJob(dateBetween.Date)?.Id ?? 0)).FirstOrDefault()?.WorkSchedule;
                        if (!string.IsNullOrEmpty(workSchedule))
                        {
                            var workingDaysOfWeek = _assistanceService.GetScheduleOfEmployee(workSchedule);
                            if (workingDaysOfWeek != null)
                                scheduleOfDay = new JobConectionWorkingDays
                                {
                                    ApplyDate = dateBetween,
                                    Schedules = workingDaysOfWeek
                                };
                        }
                    }
                    if (scheduleOfDay != null)
                    {
                        var workingDays = scheduleOfDay.Schedules.Select(x => x.DayOfWeek).ToList();
                        if (workingDays.Contains(dateBetween.DayOfWeek))
                        {
                            var applyDate = scheduleOfDay.ApplyDate;

                            var overrideOfDate = overriddensOfEmployee.Where(x => x.OverriddenDate == applyDate).FirstOrDefault();
                            var typeId = (int)AssistanceType.Absence;
                            if (overrideOfDate != null)
                                typeId = overrideOfDate.Type;
                            else
                            {
                                var festiveDate = festiveDates.Where(x => x.Date == applyDate.Date || (x.AppliesYearly && x.Date.ToString("dd/MM") == applyDate.ToString("dd/MM"))).FirstOrDefault();
                                if (festiveDate != null)
                                    typeId = (int)AssistanceType.InTime;
                            }

                            model.Add(new DateOfEmployee
                            {
                                CustomerId = employee.CustomerId,
                                EmployeeNumber = employee.EmployeeNumber,
                                Name = !string.IsNullOrWhiteSpace(employee.GetFullName()) && !string.IsNullOrEmpty(employee.GetFullName()) ? employee.GetFullName() :
                                    customer != null ? customer.GetFullName() : "Nombre no especificado",
                                Date = applyDate,
                                Assistance = typeId,
                                CheckIn = new DateTime(applyDate.Year, applyDate.Month, applyDate.Day, scheduleOfDay.Schedules
                                    .Where(y => y.DayOfWeek == applyDate.DayOfWeek).FirstOrDefault().CheckIn, 0, 0),
                                CheckOut = new DateTime(applyDate.Year, applyDate.Month, applyDate.Day, scheduleOfDay.Schedules
                                    .Where(y => y.DayOfWeek == applyDate.DayOfWeek).FirstOrDefault().CheckOut, 0, 0),
                                TimesRegistred = new List<DateTime>(),
                                ExtraInfo = $"[From betweens] - {{is festive: {yearlyDates.Any(y => y.Day == applyDate.Date.Day && y.Month == applyDate.Date.Month) || fixedDates.Contains(applyDate.Date)}}}",
                                Overriden = overrideOfDate != null,
                                OverrideLog = overrideOfDate?.Log,
                                OverrideComment = overrideOfDate?.Comment,
                            });
                        }
                    }
                }
                //model.AddRange(datesBetween.Where(x =>
                //  !yearlyDates.Any(y => y.Day == x.Date.Day && y.Month == x.Date.Month) &&
                //  !fixedDates.Contains(x.Date))
                //    .Select(x => new DateOfEmployee
                //    {
                //        CustomerId = employee.CustomerId,
                //        EmployeeNumber = employee.EmployeeNumber,
                //        Name = !string.IsNullOrWhiteSpace(employee.GetFullName()) && !string.IsNullOrEmpty(employee.GetFullName()) ? employee.GetFullName() :
                //        customer != null ? customer.GetFullName() : "Nombre no especificado",
                //        Date = x,
                //        Assistance = (int)AssistanceType.Absence,
                //        CheckIn = new DateTime(x.Year, x.Month, x.Day, workingDaysOfWeek
                //    .Where(y => y.DayOfWeek == x.DayOfWeek).FirstOrDefault().CheckIn, 0, 0),
                //        CheckOut = new DateTime(x.Year, x.Month, x.Day, workingDaysOfWeek
                //    .Where(y => y.DayOfWeek == x.DayOfWeek).FirstOrDefault().CheckOut, 0, 0),
                //        TimesRegistred = new List<DateTime>(),
                //        ExtraInfo = $"[From betweens] - {{is festive: {yearlyDates.Any(y => y.Day == x.Date.Day && y.Month == x.Date.Month) || fixedDates.Contains(x.Date)}}}"
                //    }));
            }
            return new Tuple<List<DateOfEmployee>, List<string>>(model, err);
        }
    }

    public class JobConectionWorkingDays
    {
        public JobConectionWorkingDays()
        {
            Schedules = new List<Schedule>();
        }

        public DateTime ApplyDate { get; set; }
        public List<Schedule> Schedules { get; set; }
    }
}
