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
using Teed.Plugin.Payroll.Models.BiweeklyPayment;
using Teed.Plugin.Payroll.Models.FestiveDate;
using Teed.Plugin.Payroll.Services;

namespace Teed.Plugin.Payroll.Helpers
{
    public static class BiweeklyDatesHelper
    {
        public static BiweeklyDatesModel GetBiweeklyDates(DateTime date,
            List<FestiveDateModel> festiveDates)
        {
            var dates = new BiweeklyDatesModel();
            if (date == DateTime.MinValue)
                return dates;
            dates.OriginalDate = date;

            // First biweek of month
            if (date.Day <= 15)
            {
                var beforeMonthsDate = date.AddMonths(-1);
                var lastMonthsDate = new DateTime(beforeMonthsDate.Year, beforeMonthsDate.Month, DateTime.DaysInMonth(beforeMonthsDate.Year, beforeMonthsDate.Month));
                var correctLastMonthsDate = GetFreeBiweekDate(lastMonthsDate, festiveDates);

                dates.StartOfBiweekly = correctLastMonthsDate.AddDays(1);
                dates.EndOfBiweekly = GetFreeBiweekDate(new DateTime(date.Year, date.Month, 15), festiveDates);
            }
            // Second biweek of month
            else if (date.Day > 15)
            {
                var correctLastBiweeklyDate = GetFreeBiweekDate(new DateTime(date.Year, date.Month, 15), festiveDates);

                dates.StartOfBiweekly = correctLastBiweeklyDate.AddDays(1);
                dates.EndOfBiweekly = GetFreeBiweekDate(new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month)), festiveDates);
            }

            return dates;
        }

        public static DateTime GetFreeBiweekDate(DateTime date,
            List<FestiveDateModel> festiveDates)
        {
            var festiveDate = festiveDates.Where(x => x.Date.Day == date.Day && x.Date.Month == date.Month).FirstOrDefault();
            var isSunday = date.DayOfWeek == DayOfWeek.Sunday;
            do
            {
                if (isSunday)
                {
                    date = date.AddDays(-1);
                    festiveDate = festiveDates.Where(x => x.Date.Day == date.Day && x.Date.Month == date.Month).FirstOrDefault();
                }
                if (festiveDate != null)
                {
                    if (!festiveDate.AppliesYearly)
                    {
                        if (festiveDate.Date.Year == date.Year)
                            date = date.AddDays(-1);
                    }
                    else
                        date = date.AddDays(-1);
                }

                festiveDate = festiveDates.Where(x => x.Date.Day == date.Day && x.Date.Month == date.Month).FirstOrDefault();
                isSunday = date.DayOfWeek == DayOfWeek.Sunday;

            } while (festiveDate != null || isSunday);

            return date.AddDays(-2);
        }

        public static List<BiweeklyDatesModel> GetAllBiweeklyDatesSinceGivenDate(DateTime startBiweek,
            List<FestiveDateModel> festiveDates)
        {
            var biweeks = new List<BiweeklyDatesModel>();
            if (startBiweek == DateTime.MinValue)
                return biweeks;

            var year = DateTime.Now.Year;
            var date = startBiweek;
            var biweek = GetBiweeklyDates(new DateTime(date.Year, date.Month, 15).Date, festiveDates);
            var secondbiweek = GetBiweeklyDates(new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month)).Date, festiveDates);
            do
            {
                biweeks.AddRange(new List<BiweeklyDatesModel> { biweek, secondbiweek });
                date = date.AddMonths(1);
                biweek = GetBiweeklyDates(new DateTime(date.Year, date.Month, 15).Date, festiveDates);
                secondbiweek = GetBiweeklyDates(new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month)).Date, festiveDates);
            } while (year >= biweek.StartOfBiweekly.Year);

            return biweeks;
        }
    }
}
