using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.Franchise
{
    public class PenaltyCatalogModel
    {
        public int Id { get; set; }
        public string Concepto { get; set; }
        public string PenaltyCustomId { get; set; }
        public string Unit { get; set; }
        public decimal Amount { get; set; }

        [UIHint("DateNullable")]
        public DateTime ApplyDate { get; set; }
        public string ApplyDateString { get; set; }
        public string Log { get; set; }
    }
}