using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.RatesAndBonuses
{
    public class RatesAndBonusesModel
    {
        public decimal BaseFixedRateCDMX { get; set; }
        public decimal BaseVariableRateCDMX { get; set; }
        public decimal BaseFixedRateOtherStates { get; set; }
        public decimal BaseVariableRateOtherStates { get; set; }
        public decimal FixedWeeklyBonusCeroIncidents { get; set; }
        public decimal VariableWeeklyBonusCeroIncidents { get; set; }
        public string Log { get; set; }
    }
}