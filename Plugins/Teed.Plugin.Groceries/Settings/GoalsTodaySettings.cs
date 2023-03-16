using Nop.Core.Configuration;
using System;

namespace Teed.Plugin.Groceries
{
    public class GoalsTodaySettings : ISettings
    {
        public int GoalsMonday { get; set; }
        public int GoalsTuesday { get; set; }
        public int GoalsWednesday { get; set; }
        public int GoalsThursday { get; set; }
        public int GoalsFriday { get; set; }
        public int GoalsSaturday { get; set; }
    }
}
