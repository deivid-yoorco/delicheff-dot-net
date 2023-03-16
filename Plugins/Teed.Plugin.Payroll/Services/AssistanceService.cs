using Newtonsoft.Json;
using Nop.Core;
using Nop.Services.Customers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Teed.Plugin.Payroll.Domain.Assistances;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;
using Teed.Plugin.Payroll.Models.Assistance;
using Teed.Plugin.Payroll.Models.FestiveDate;

namespace Teed.Plugin.Payroll.Services
{
    public class AssistanceService
    {
        ICustomerService _customerService;
        IStoreContext _storeContext;
        JobCatalogService _jobCatalogService;
        IncidentService _incidentService;

        public AssistanceService(ICustomerService customerService,
            IStoreContext storeContext,
            JobCatalogService jobCatalogService,
            IncidentService incidentService)
        {
            _customerService = customerService;
            _storeContext = storeContext;
            _jobCatalogService = jobCatalogService;
            _incidentService = incidentService;
        }

        /// <summary>
        /// Starts the connection to the outer server
        /// </summary>
        /// <returns></returns>
        public SqlConnection StartConectionToDb()
        {
            var connetionString = GetConnetionString();
            if (string.IsNullOrEmpty(connetionString))
                return null;
            SqlConnection cnn = new SqlConnection(connetionString);
            cnn.Open();
            return cnn;
        }

        /// <summary>
        /// Get the connection string to the sql server from local json
        /// </summary>
        /// <returns></returns>
        public string GetConnetionString()
        {
            var connection = string.Empty;
            using (StreamReader r = new StreamReader(Environment.CurrentDirectory +
                @"\App_Data\payrollDataSettings.json"))
            {
                string json = r.ReadToEnd();
                dynamic dataSettings = JsonConvert.DeserializeObject<dynamic>(json);
                connection = (string)dataSettings.DataConnectionString;
            }
            return connection;
        }

        /// <summary>
        /// Formats employee number to its necesary 9 digits conversion for outer server search
        /// </summary>
        /// <param name="id">Employee number</param>
        /// <returns>Formatted employee number</returns>
        public string FormatEmployeeNumber(int id)
        {
            var final = id.ToString();
            var badgeSize = 9 - final.Length;
            for (int i = 0; i < badgeSize; i++)
                final = 0 + final;
            return final;
        }

        /// <summary>
        /// Formats employee numbers to its necesary 9 digits conversion for outer server search
        /// </summary>
        /// <param name="ids">Employee numbers</param>
        /// <returns>Formatted employee numbers</returns>
        public List<string> FormatEmployeeNumbers(List<int> ids)
        {
            var final = new List<string>();
            foreach (var id in ids)
            {
                var temp = id.ToString();
                var badgeSize = 9 - temp.Length;
                for (int i = 0; i < badgeSize; i++)
                    temp = 0 + temp;
                final.Add(temp);
            }
            return final;
        }

        /// <summary>
        /// Converts string to DateTime
        /// </summary>
        /// <param name="dateValue">Date in string format</param>
        /// <returns>DateTime</returns>
        public DateTime ConvertToDateTime(string dateValue)
        {
            var final = DateTime.MinValue;
            if (!string.IsNullOrEmpty(dateValue))
            {
                try
                {
                    final = DateTime.ParseExact(dateValue, "M/d/yyyy h:mm:ss tt",
                        CultureInfo.InvariantCulture, DateTimeStyles.None);
                }
                catch (Exception e) { }
            }
            return final;
        }

        /// <summary>
        /// Get schedules of employee by the formatted string
        /// </summary>
        /// <param name="stringSchedule">Formatted string with specified days and hours of check in and out</param>
        /// <returns>List of Schedules</returns>
        public List<Schedule> GetScheduleOfEmployee(string stringSchedule)
        {
            var workingDaysOfWeek = new List<Schedule>();
            if (!string.IsNullOrEmpty(stringSchedule))
            {
                var workSchedule = stringSchedule.Split('|').ToList();
                foreach (var dayOfWeek in workSchedule)
                {
                    var scheduleAdd = new Schedule();
                    var schedule = dayOfWeek.Split('-');
                    switch (schedule[0])
                    {
                        case "Monday":
                            scheduleAdd.DayOfWeek = DayOfWeek.Monday;
                            break;
                        case "Tuesday":
                            scheduleAdd.DayOfWeek = DayOfWeek.Tuesday;
                            break;
                        case "Wednesday":
                            scheduleAdd.DayOfWeek = DayOfWeek.Wednesday;
                            break;
                        case "Thursday":
                            scheduleAdd.DayOfWeek = DayOfWeek.Thursday;
                            break;
                        case "Friday":
                            scheduleAdd.DayOfWeek = DayOfWeek.Friday;
                            break;
                        case "Saturday":
                            scheduleAdd.DayOfWeek = DayOfWeek.Saturday;
                            break;
                        case "Sunday":
                            scheduleAdd.DayOfWeek = DayOfWeek.Sunday;
                            break;
                        default:
                            break;
                    }
                    if (schedule[1].Contains(":"))
                    {
                        var time = schedule[1].Split(':').ToList();
                        scheduleAdd.CheckIn = int.Parse(time[0]);
                        scheduleAdd.CheckInMinutes = int.Parse(time[1]);
                    }
                    else
                        scheduleAdd.CheckIn = int.Parse(schedule[1]);
                    if (schedule[2].Contains(":"))
                    {
                        var time = schedule[2].Split(':').ToList();
                        scheduleAdd.CheckOut = int.Parse(time[0]);
                        scheduleAdd.CheckOutMinutes = int.Parse(time[1]);
                    }
                    else
                        scheduleAdd.CheckOut = int.Parse(schedule[2]);
                    workingDaysOfWeek.Add(scheduleAdd);
                }
            }
            return workingDaysOfWeek;
        }

        /// <summary>
        /// Get outer server information by the employee's number
        /// </summary>
        /// <param name="id">Employee Number</param>
        /// <returns>List of Assistances</returns>
        public List<Assistance> GetByEmployeeNumber(int id)
        {
            var final = new List<Assistance>();
            if (id < 1)
                return final;
            var cnn = StartConectionToDb();
            var formatedEmployeeNumber = FormatEmployeeNumber(id);
            var command = new SqlCommand($@"SELECT ch.checktime, ch.area_name
                                        FROM dbo.checkinout ch
                                        LEFT JOIN dbo.userinfo us
                                        ON ch.pin = us.badgenumber
                                        WHERE us.badgenumber = '{formatedEmployeeNumber}'
                                        ORDER BY ch.checktime DESC", cnn);
            var dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                final.Add(new Assistance
                {
                    BadgeNumber = formatedEmployeeNumber,
                    Area = dataReader.GetValue(1).ToString(),
                    CheckIn = ConvertToDateTime(dataReader.GetValue(0).ToString())
                });
            }
            dataReader.Close();
            cnn.Close();
            return final;
        }

        /// <summary>
        /// Get asistances between two dates
        /// </summary>
        /// <param name="after">Start point for search</param>
        /// <param name="before">End point for search</param>
        /// <returns>List of Assistances</returns>
        public List<Assistance> GetBetweenDates(DateTime after, DateTime before)
        {
            if (after == DateTime.MinValue || before == DateTime.MinValue)
                return null;
            var final = new List<Assistance>();
            var cnn = StartConectionToDb();

            var command = new SqlCommand($@"SELECT ch.checktime, ch.area_name, us.badgenumber
                                        FROM dbo.checkinout ch
                                        LEFT JOIN dbo.userinfo us
                                        ON ch.pin = us.badgenumber
                                        WHERE CAST(ch.checktime AS Datetime) <= '{after:yyyy-MM-dd HH:mm:sss.000}'
                                        AND CAST(ch.checktime AS Datetime) >= '{before:yyyy-MM-dd 00:00:000.000}'
                                        ORDER BY ch.checktime DESC", cnn);
            var dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                final.Add(new Assistance
                {
                    BadgeNumber = dataReader.GetValue(2).ToString(),
                    Area = dataReader.GetValue(1).ToString(),
                    CheckIn = ConvertToDateTime(dataReader.GetValue(0).ToString())
                });
            }
            dataReader.Close();
            cnn.Close();
            return final;
        }

        /// <summary>
        /// Inserts new incidents by checking outer db for specified employees and between dates
        /// </summary>
        /// <param name="employees">List of employees to apply to</param>
        /// <param name="after">"From" date, from when to start searching the incidents, from this date</param>
        /// <param name="before">"To" date, from when to end searching the incidents, until this date</param>
        /// <returns>Number of incidents inserted</returns>
        public int IncidentsRegister(List<PayrollEmployee> employees, DateTime? after = null, DateTime? before = null)
        {
            var past = before ?? DateTime.Now.AddDays(-7);
            var future = after ?? DateTime.Now;
            var final = 0;
            var exactDates = new List<DateTime>();
            var yearlyDates = new List<DateTime>();
            var OnTimeDates = new List<DateTime>();
            var assistances = GetBetweenDates(future, past).ToList();
            var festiveDates = GetFestiveDatesFromGroceries().Where(x => !x.DontApplyToPayroll).ToList();
            // Get rid of asistances in festive dates
            if (festiveDates.Any())
            {
                exactDates = festiveDates.Where(x => !x.AppliesYearly).Select(x => x.Date).ToList();
                yearlyDates = festiveDates.Where(x => x.AppliesYearly).Select(x => x.Date).ToList();
            }
            if (exactDates.Any())
                assistances = assistances.Where(x => !exactDates.Contains(x.CheckIn.Date)).ToList();
            if (yearlyDates.Any())
            {
                var datesAsDayMonth = yearlyDates.Select(x => x.ToString("dd/MM")).ToList();
                assistances = assistances.Where(x => !datesAsDayMonth.Contains(x.CheckIn.ToString("dd/MM"))).ToList();
            }
            //

            // Filter users so partners aren't added
            var customerIds = employees.Select(x => x.CustomerId).ToList();
            var filteredCustomerIds = _customerService.GetAllCustomersQuery()
                .Where(x => customerIds.Contains(x.Id) &&
                !x.CustomerRoles.Select(y => y.SystemName.ToLower()).Contains("partner"))
                ?.Select(x => x.Id).ToList();
            if (filteredCustomerIds.Any())
            {
                employees = employees.Where(x => filteredCustomerIds.Contains(x.CustomerId)).ToList();
                foreach (var employee in employees)
                {
                    var incidentsOfEmployee = _incidentService.GetAllByPayrollEmployeeId(employee.Id);
                    var jobOfEmployee = _jobCatalogService.GetById(employee.GetCurrentJob()?.Id ?? 0);
                    if (jobOfEmployee != null)
                    {
                        var workingDaysOfWeek = GetScheduleOfEmployee(jobOfEmployee.WorkSchedule);
                        var daysOfWeek = workingDaysOfWeek.Select(x => x.DayOfWeek).ToList();

                        var assistancesOfEmployee =
                            assistances.Where(x => daysOfWeek.Contains(x.CheckIn.DayOfWeek) &&
                            FormatEmployeeNumber(employee.EmployeeNumber) == x.BadgeNumber)
                            .GroupBy(x => x.CheckIn.Date).ToList();
                        foreach (var assist in assistancesOfEmployee)
                        {
                            var date = assist.Key.Date;
                            var currentSchedule = workingDaysOfWeek.Where(x => x.DayOfWeek == date.DayOfWeek).FirstOrDefault();
                            if (currentSchedule != null)
                            {
                                var checkInOfCurrentDay = new DateTime(date.Year, date.Month, date.Day, currentSchedule.CheckIn, 0, 0).AddMinutes(15);
                                var checkOutOfCurrentDay = new DateTime(date.Year, date.Month, date.Day, currentSchedule.CheckOut, 0, 0);

                                if (!assist.Where(x => x.CheckIn <= checkInOfCurrentDay).Any() &&
                                    assist.Where(x => x.CheckIn > checkInOfCurrentDay).Any())
                                {
                                    var incidentRegistered =
                                        incidentsOfEmployee
                                        .Where(x => x.Date == date &&
                                        (x.Type == Domain.Incidents.IncidentType.Absence ||
                                        x.Type == Domain.Incidents.IncidentType.Delay))
                                        .FirstOrDefault();
                                    if (incidentRegistered == null)
                                    {
                                        incidentRegistered = new Domain.Incidents.Incident
                                        {
                                            Date = date,
                                            PayrollEmployeeId = employee.Id,
                                            Type = Domain.Incidents.IncidentType.Delay,
                                            Reason = "Incidencia automática por retardo según Reloj Checador.",
                                            Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - Incidencia automática por retardo según Reloj Checador.\n"
                                        };
                                        _incidentService.Insert(incidentRegistered);
                                        final++;
                                    }
                                }
                                else if (assist.Where(x => x.CheckIn <= checkInOfCurrentDay).Any())
                                    OnTimeDates.Add(date);

                            }
                        }

                        var dateCheckerStartedWorking = new DateTime(2020, 9, 12).Date;
                        var today = DateTime.Now;
                        var daysOfWork = workingDaysOfWeek.Select(x => x.DayOfWeek).ToList();
                        var dates = assistancesOfEmployee.Any() ? assistancesOfEmployee.OrderByDescending(x => x.Key).Select(x => x.Key.Date).ToList() : new List<DateTime>();
                        var firstAssistance =
                            ((employee.DateOfAdmission < dateCheckerStartedWorking || employee.DateOfAdmission == null ? dateCheckerStartedWorking : employee.DateOfAdmission) ?? DateTime.Now).Date;
                        var lastAssitance = today;

                        var datesBetween = Enumerable.Range(0, 1 + lastAssitance.Subtract(firstAssistance).Days)
                          .Select(offset => firstAssistance.AddDays(offset))
                          .Where(x => !dates.Contains(x.Date) && daysOfWork.Contains(x.DayOfWeek))
                          .Where(x => past <= x && x <= future)
                          .ToList();

                        if (datesBetween.Where(x => x == today.Date).Any() &&
                            workingDaysOfWeek.Where(x => x.DayOfWeek == today.DayOfWeek).Any())
                        {
                            var nowSchedule = workingDaysOfWeek.Where(x => x.DayOfWeek == today.DayOfWeek).FirstOrDefault();
                            var nowCheckIn = new DateTime(today.Year, today.Month, today.Day, nowSchedule.CheckIn, 15, 0);
                            if (today < nowCheckIn)
                                datesBetween.RemoveAt(datesBetween.IndexOf(today.Date));
                        }
                        if (incidentsOfEmployee.Any())
                        {
                            var incidentsDates = incidentsOfEmployee.Select(x => x.Date).ToList();
                            datesBetween = datesBetween.Where(x => !incidentsDates.Contains(x.Date)).ToList();
                        }
                        // Get rid of dates in between in festive dates
                        if (exactDates.Any())
                            datesBetween = datesBetween.Where(x => !exactDates.Contains(x)).ToList();
                        if (yearlyDates.Any())
                        {
                            var datesAsDayMonth = yearlyDates.Select(x => x.ToString("dd/MM")).ToList();
                            datesBetween = datesBetween.Where(x => !datesAsDayMonth.Contains(x.ToString("dd/MM"))).ToList();
                        }
                        //
                        // Get rid of dates that arrived on time
                        datesBetween = datesBetween.Where(x => !OnTimeDates.Contains(x)).ToList();
                        //
                        foreach (var date in datesBetween)
                        {
                            var incidentRegistered =
                                        incidentsOfEmployee
                                        .Where(x => x.Date == date.Date &&
                                        (x.Type == Domain.Incidents.IncidentType.Absence ||
                                        x.Type == Domain.Incidents.IncidentType.Delay))
                                        .FirstOrDefault();
                            if (incidentRegistered == null)
                            {
                                incidentRegistered = new Domain.Incidents.Incident
                                {
                                    Date = date.Date,
                                    PayrollEmployeeId = employee.Id,
                                    Type = Domain.Incidents.IncidentType.Absence,
                                    Reason = "Incidencia automática por inasistencia según Reloj Checador.",
                                    Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - Incidencia automática por inasistencia según Reloj Checador.\n"
                                };
                                _incidentService.Insert(incidentRegistered);
                                final++;
                            }
                        }
                    }
                }
            }
            return final;
        }

        /// <summary>
        /// Checks if employee numbers exist in outer service
        /// </summary>
        /// <param name="ids"></param>
        /// <returns>List of Assistances with outer existing employee numbers</returns>
        public List<Assistance> CheckEmployeeNumbersExistence(List<int> ids)
        {
            var final = new List<Assistance>();
            if (ids.Count < 1)
                return final;
            var cnn = StartConectionToDb();
            var formatedEmployeeNumbers = FormatEmployeeNumbers(ids);
            var command = new SqlCommand($@"SELECT us.badgenumber, us.name
                                        FROM dbo.userinfo us
                                        WHERE {CreateEmployeeNumbersConditional(formatedEmployeeNumbers)}
                                        ORDER BY us.badgenumber DESC", cnn);
            var dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                final.Add(new Assistance
                {
                    BadgeNumber = dataReader.GetValue(0).ToString(),
                    Name = dataReader.GetValue(1).ToString()
                });
            }
            dataReader.Close();
            cnn.Close();
            return final;
        }

        /// <summary>
        /// Creates a string for sql command to be able to search by employee numbers
        /// </summary>
        /// <param name="numbers">Employee numbers</param>
        /// <returns>String sql conditional</returns>
        public string CreateEmployeeNumbersConditional(List<string> numbers)
        {
            var final = string.Empty;
            if (numbers.Count > 0)
            {
                final = string.Join("\nOR ", numbers.Select(x => $"us.badgenumber = '{x}' "));
            }
            return final;
        }

        public List<FestiveDateModel> GetFestiveDatesFromGroceries()
        {
            var dates = new List<FestiveDateModel>();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var result = client.GetAsync(
                        //"https://localhost:44387/" +
                        _storeContext.CurrentStore.SecureUrl +
                        "/Admin/FestiveDate/GetFestiveDates").Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var resultJson = result.Content.ReadAsStringAsync().Result;
                        if (!string.IsNullOrEmpty(resultJson))
                        {
                            dates = JsonConvert.DeserializeObject<List<FestiveDateModel>>(resultJson);
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
            return dates;
        }
    }
}