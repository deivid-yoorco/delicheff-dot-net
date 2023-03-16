using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Utils
{
    public static class DashboardUtils
    {
        public static Dictionary<decimal, decimal> GoalData { get; } = new Dictionary<decimal, decimal>()
        {
            { 0m, 24m },
            { 1m, 25m },
            { 2m, 25m },
            { 3m, 25m },
            { 4m, 26m },
            { 5m, 26m },
            { 6m, 26m },
            { 7m, 26m },
            { 8m, 28m },
            { 9m, 30m },
            { 10m, 34m },
            { 11m, 39m },
            { 12m, 44m },
            { 13m, 50m },
            { 14m, 54m },
            { 15m, 59m },
            { 16m, 64m },
            { 17m, 69m },
            { 18m, 74m },
            { 19m, 80m },
            { 20m, 86m },
            { 21m, 91m },
            { 22m, 96m },
            { 23m, 100m },
        };

        public static DateTime GetControlTime()
        {
            DateTime now = DateTime.Now;
            if (now.Minute > 0) now = now.AddHours(1);
            return now;
        }

        public static decimal GetGoal(int totalQty)
        {
            var controlDate = GetControlTime();
            GoalData.TryGetValue(controlDate.Hour, out decimal value);
            return Math.Round(totalQty * (value / 100));
        }

        public static string GetGoalColor(decimal currentQty, decimal totalQty)
        {
            if (totalQty == 0) return "";
            var controlDate = GetControlTime();
            var percentaje = GetCurrentTimePercentaje(controlDate);
            var shouldBe = totalQty * (percentaje / 100);
            var goalPercentaje = (currentQty * 100) / shouldBe;
            return "dashboard-" + GetColorValue(goalPercentaje);
        }

        public static decimal GetCurrentTimePercentaje(DateTime controlDate)
        {
            GoalData.TryGetValue(controlDate.Hour, out decimal percentaje);
            return percentaje;
        }

        private static string GetColorValue(decimal percentaje)
        {
            if (percentaje >= 90) return "green";
            else if (percentaje >= 65 && percentaje < 90) return "yellow";
            else return "red";
        }

        
    }
}
