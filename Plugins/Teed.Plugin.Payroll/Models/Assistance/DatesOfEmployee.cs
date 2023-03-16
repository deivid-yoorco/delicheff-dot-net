using System;
using System.Collections.Generic;
using Teed.Plugin.Payroll.Domain.Assistances;

namespace Teed.Plugin.Payroll.Models.Assistance
{
    public class DateOfEmployeeMain
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string NameInChecker { get; set; }
    }

    public class DateOfEmployee
    {
        public DateOfEmployee()
        {
            TimesRegistred = new List<DateTime>();
        }

        public int CustomerId { get; set; }
        public int EmployeeNumber { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public List<DateTime> TimesRegistred { get; set; }
        public int Assistance { get; set; }
        public string ExtraInfo { get; set; }
        public bool Overriden { get; set; }
        public string OverrideLog { get; set; }
        public string OverrideComment { get; set; }
    }

    public class Schedule
    {
        public DayOfWeek DayOfWeek { get; set; }
        public int CheckIn { get; set; }
        public int CheckInMinutes { get; set; }
        public int CheckOut { get; set; }
        public int CheckOutMinutes { get; set; }
    }
}
