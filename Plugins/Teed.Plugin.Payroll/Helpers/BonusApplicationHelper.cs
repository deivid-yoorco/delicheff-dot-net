using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.Bonuses;
using Teed.Plugin.Payroll.Domain.Incidents;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;
using Teed.Plugin.Payroll.Models.Assistance;
using Teed.Plugin.Payroll.Services;

namespace Teed.Plugin.Payroll.Helpers
{
    public static class BonusApplicationHelper
    {
        public static int InsertNewBonusApplications(DateTime start, DateTime end,
            BonusService _bonusService, BonusApplicationService _bonusApplicationService,
            JobCatalogService _jobCatalogService, PayrollEmployeeService _payrollEmployeeService,
            IncidentService _incidentService, AssistanceService _assistanceService)
        {
            var finalInsertCount = 0;
            try
            {
                var festiveDates = _assistanceService.GetFestiveDatesFromGroceries()
                    .Where(x => !x.DontApplyToPayroll).ToList();
                if (start == DateTime.MinValue)
                    start = DateTime.Now;
                if (end == DateTime.MinValue)
                    end = DateTime.Now;
                var automaticBonuses = _bonusService.GetAll()
                    .Where(x => x.ConditionTypeId == (int)ConditionType.Automatic &&
                    x.Amounts.Count > 0 && x.IsActive)
                    .ToList();
                foreach (var bonus in automaticBonuses)
                {
                    var bonusApplications = _bonusApplicationService.GetAll()
                        .Where(x => x.BonusId == bonus.Id).ToList();
                    var frequencyType = (FrequencyType)bonus.FrequencyTypeId;
                    var bonusType = (BonusType)bonus.BonusTypeId;
                    var entityTpe = bonus.BonusTypeId == (int)BonusType.Individual ? EntityType.Employee : EntityType.Job;

                    var biweeks = new List<DateTime>();
                    if (frequencyType == FrequencyType.ByBiweek)
                        biweeks = GetBiweekliesInRange(start, end);
                    else
                        biweeks = GetDatesInRange(start, end);

                    var jobOfBonusIds = new List<int>();
                    var jobCatalogIdsFormat = bonus.JobCatalogIdsFormat.Split('|').ToList();
                    if (jobCatalogIdsFormat.Any())
                        jobOfBonusIds = jobCatalogIdsFormat.Select(x => int.Parse(x)).ToList();
                    var jobsOfBonus = _jobCatalogService.GetAll()
                        .Where(x => jobOfBonusIds.Contains(x.Id))
                        .ToList();
                    var employeesOfBonus = _payrollEmployeeService.GetAll()
                        .Where(x => x.EmployeeStatusId == (int)EmployeeStatus.Active)
                        .ToList()
                        .Where(x => jobOfBonusIds.Contains(x.GetCurrentJob()?.Id ?? 0))
                        .ToList();
                    var employeeOfBonusIds = employeesOfBonus.Select(y => y.Id).ToList();
                    var incidentsOfEmployees = _incidentService.GetAll()
                    .Where(x => !x.Justified && employeeOfBonusIds.Contains(x.PayrollEmployeeId))
                    .ToList();


                    var entityIds = new List<int>();
                    if (entityTpe == EntityType.Employee)
                        entityIds = employeesOfBonus.Select(x => x.Id).ToList();
                    else
                        entityIds = jobsOfBonus.Select(x => x.Id).ToList();

                    foreach (var entityId in entityIds)
                    {
                        var incidentsOfEmployee = new List<Incident>();
                        var daysOfWork = new List<DayOfWeek>();
                        if (entityTpe == EntityType.Employee)
                        {
                            incidentsOfEmployee = incidentsOfEmployees.Where(x => x.PayrollEmployeeId == entityId).ToList();
                            var jobCatalogId = employeesOfBonus.Where(x => x.Id == entityId).FirstOrDefault().GetCurrentJob()?.Id ?? 0;
                            var employeeSchedule = jobsOfBonus.Where(x => x.Id == jobCatalogId).FirstOrDefault().WorkSchedule;
                            daysOfWork = _assistanceService.GetScheduleOfEmployee(employeeSchedule).Select(x => x.DayOfWeek).ToList();
                        }
                        else
                        {
                            foreach (var employee in employeesOfBonus.Where(x => (x.GetCurrentJob()?.Id ?? 0) == entityId))
                                incidentsOfEmployee.AddRange(incidentsOfEmployees
                                    .Where(x => x.PayrollEmployeeId == employee.Id)
                                    .ToList());
                            var employeeSchedule = jobsOfBonus.Where(x => x.Id == entityId).FirstOrDefault().WorkSchedule;
                            daysOfWork = _assistanceService.GetScheduleOfEmployee(employeeSchedule).Select(x => x.DayOfWeek).ToList();
                        }
                        incidentsOfEmployee = incidentsOfEmployee.Where(x => daysOfWork.Contains(x.Date.Value.DayOfWeek)).ToList();
                        if (frequencyType == FrequencyType.ByDay)
                            biweeks = biweeks.Where(x => daysOfWork.Contains(x.DayOfWeek)).ToList();
                        var applicationsOfEmployee = bonusApplications
                        .Where(x => x.EntityId == entityId && x.EntityTypeId == (int)entityTpe)
                        .ToList();
                        foreach (var biweek in biweeks)
                        {
                            // HERE
                            var dates = BiweeklyDatesHelper.GetBiweeklyDates(biweek, festiveDates);
                            var startOfCurrentBiweekly = dates.StartOfBiweekly;
                            var endOfCurrentBiweekly = dates.EndOfBiweekly;
                            var dateOfApplication = string.Empty;

                            if ((frequencyType == FrequencyType.ByDay && !applicationsOfEmployee.Where(x => x.Date == biweek).Any()) ||
                                (frequencyType == FrequencyType.ByBiweek && !applicationsOfEmployee.Where(x => startOfCurrentBiweekly <= x.Date && x.Date <= endOfCurrentBiweekly).Any()))
                            {
                                var bonusAmount = bonus.GetAppliableAmount(biweek.Date);
                                if (bonusAmount != null || bonusAmount?.Amount > 0)
                                {
                                    var application = new BonusApplication
                                    {
                                        Amount = bonusAmount.Amount,
                                        BonusId = bonus.Id,
                                        Date = biweek,
                                        EntityId = entityId,
                                        EntityTypeId = (int)entityTpe
                                    };
                                    if (frequencyType == FrequencyType.ByDay)
                                    {
                                        if (!incidentsOfEmployee.Where(x => x.Date == biweek).Any())
                                            application.WillApply = true;
                                        dateOfApplication = "para el día " + biweek.ToString("dd/MM/yyyy");
                                    }
                                    else
                                    {
                                        if (!incidentsOfEmployee.Where(x => startOfCurrentBiweekly <= x.Date && x.Date <= endOfCurrentBiweekly).Any())
                                            application.WillApply = true;
                                        dateOfApplication = "para la quincena del " + startOfCurrentBiweekly.ToString("dd/MM/yyyy") + " al " + endOfCurrentBiweekly.ToString("dd/MM/yyyy");
                                    }
                                    application.Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - Se agregó una nueva autorización automática {dateOfApplication} para " +
                                            $"{(entityTpe == EntityType.Employee ? "el empleado \"" + employeesOfBonus.Where(x => x.Id == entityId).FirstOrDefault()?.GetFullName() : "el empleo \"" + jobsOfBonus.Where(x => x.Id == entityId).FirstOrDefault()?.Name) + "\""}" +
                                            $" del bono \"{bonus.Name}\" como \"{(application.WillApply ? "Autorizada" : "No autorizada")}\"\n";
                                    _bonusApplicationService.Insert(application);
                                    finalInsertCount++;
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
            return finalInsertCount;
        }

        public static List<DateTime> GetBiweekliesInRange(DateTime from, DateTime to)
        {
            var lastDate = to.Day <= 15 ? new DateTime(to.Year, to.Month, 1) : new DateTime(to.Year, to.Month, 16);
            var current = from.Day <= 15 ? new DateTime(from.Year, from.Month, 1) : new DateTime(from.Year, from.Month, 16);
            var result = new List<DateTime> { current };
            do
            {
                current = current.AddDays(current.Day == 1 ? 15 : (DateTime.DaysInMonth(current.Year, current.Month) - current.Day + 1));
                result.Add(current);
            } while (current != lastDate && current < lastDate);
            return result.OrderBy(x => x).ToList();
        }

        public static List<DateTime> GetDatesInRange(DateTime from, DateTime to)
        {
            return Enumerable.Range(0, to.Subtract(from).Days + 1)
                     .Select(d => from.AddDays(d)).OrderBy(x => x).ToList();
        }

        public static bool CheckBonusesAfterIncidentAdd(Incident incident,
            BonusService _bonusService, BonusApplicationService _bonusApplicationService,
            JobCatalogService _jobCatalogService, PayrollEmployeeService _payrollEmployeeService)
        {
            var finalDidChange = false;
            try
            {
                if (!incident.Justified && incident.Date != null)
                {
                    var dateOfIncident = (incident.Date ?? DateTime.Now).Date;
                    var startOfCurrentBiweeklyIncident = dateOfIncident.Day <= 15 ? new DateTime(dateOfIncident.Year, dateOfIncident.Month, 1).Date :
                        new DateTime(dateOfIncident.Year, dateOfIncident.Month, 16).Date;
                    var endOfCurrentBiweeklyIncident = dateOfIncident.Day <= 15 ? new DateTime(dateOfIncident.Year, dateOfIncident.Month, 15).Date :
                        new DateTime(dateOfIncident.Year, dateOfIncident.Month, DateTime.DaysInMonth(dateOfIncident.Year, dateOfIncident.Month)).Date;

                    var bonusApplicationsOfDate = _bonusApplicationService.GetAll()
                        .Where(x => x.Date == incident.Date || (startOfCurrentBiweeklyIncident <= x.Date && x.Date <= endOfCurrentBiweeklyIncident))
                        .ToList();
                    var bonusIdsOfDate = bonusApplicationsOfDate.Select(x => x.BonusId)
                        .ToList();
                    var bonuses = _bonusService.GetAll()
                        .Where(x => x.Amounts.Count > 0 && x.IsActive && bonusIdsOfDate.Contains(x.Id))
                        .ToList();
                    var employeeOfIncident = _payrollEmployeeService.GetById(incident.PayrollEmployeeId);
                    var jobOfEmployee = _jobCatalogService.GetById(employeeOfIncident.GetCurrentJob()?.Id ?? 0);
                    foreach (var bonus in bonuses)
                    {
                        var bonusAmount = bonus.GetAppliableAmount();
                        if (bonusAmount != null || bonusAmount?.Amount > 0)
                        {
                            var frequencyType = (FrequencyType)bonus.FrequencyTypeId;

                            var applicationsOfCurrentBonus = bonusApplicationsOfDate.Where(x => x.BonusId == bonus.Id &&
                                (x.EntityId == jobOfEmployee.Id || x.EntityId == employeeOfIncident.Id)).ToList();
                            if (frequencyType == FrequencyType.ByDay)
                                applicationsOfCurrentBonus = applicationsOfCurrentBonus
                                    .Where(x => x.Date == incident.Date).ToList();
                            else
                                applicationsOfCurrentBonus = applicationsOfCurrentBonus
                                    .Where(x => startOfCurrentBiweeklyIncident <= x.Date && x.Date <= endOfCurrentBiweeklyIncident).ToList();
                            foreach (var application in applicationsOfCurrentBonus)
                            {
                                application.WillApply = false;
                                _bonusApplicationService.Update(application);
                                finalDidChange = true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
            return finalDidChange;
        }
    }
}
