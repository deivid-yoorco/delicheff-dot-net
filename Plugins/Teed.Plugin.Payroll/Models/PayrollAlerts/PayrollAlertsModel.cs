using System;
using System.Collections.Generic;
using Teed.Plugin.Payroll.Domain.Assistances;

namespace Teed.Plugin.Payroll.Models.PayrollAlerts
{
    public class PayrollAlertsModel
    {
        public PayrollAlertsModel()
        {
            IncompleteEmployees = new List<IncompleteEmployee>();
            EmployeesNoChecker = new List<EmployeeNoChecker>();
            UpcomingBirthdays = new List<UpcomingBirthday>();
            DischargedWithoutDates = new List<DischargedWithoutDate>();
            ExEmployeeAndActives = new List<ExEmployeeAndActive>();
            DischargedWithoutSatisfactoryDepartureProcesses = new List<DischargedWithoutSatisfactoryDepartureProcess>();
            JustifiablyDismissEmployee4AbcensesLast30Days = new List<JustifiablyDismissEmployee4AbcensesLast30Days>();
        }

        public List<IncompleteEmployee> IncompleteEmployees { get; set; }
        public List<EmployeeNoChecker> EmployeesNoChecker { get; set; }
        public List<JobNoSchedule> JobNoSchedules { get; set; }
        public List<UpcomingBirthday> UpcomingBirthdays { get; set; }
        public List<DischargedWithoutDate> DischargedWithoutDates { get; set; }
        public List<ExEmployeeAndActive> ExEmployeeAndActives { get; set; }
        public List<DischargedWithoutSatisfactoryDepartureProcess> DischargedWithoutSatisfactoryDepartureProcesses { get; set; }
        public List<JustifiablyDismissEmployee4AbcensesLast30Days> JustifiablyDismissEmployee4AbcensesLast30Days { get; set; }
    }

    public class IncompleteEmployee
    {
        public IncompleteEmployee()
        {
            PendingInformation = new List<string>();
        }

        public int Id { get; set; }
        public string FullName { get; set; }
        public List<string> PendingInformation { get; set; }
    }

    public class EmployeeNoChecker
    {
        public int Id { get; set; }
        public string FullName { get; set; }
    }

    public class JobNoSchedule
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class UpcomingBirthday
    {
        public DateTime Birthday { get; set; }
        public string Name { get; set; }
    }

    public class DischargedWithoutDate
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ExEmployeeAndActive
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class DischargedWithoutSatisfactoryDepartureProcess
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class JustifiablyDismissEmployee4AbcensesLast30Days
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AbscensesCount { get; set; }
    }
}
