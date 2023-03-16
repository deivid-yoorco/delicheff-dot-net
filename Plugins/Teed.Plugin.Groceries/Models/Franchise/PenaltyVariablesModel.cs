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
    public class PenaltyVariablesModel
    {
        public PenaltyVariablesModel()
        {
            Penalties = new List<SelectListItem>();
            PenaltiesCatalog = new List<SelectListItem>();
        }

        public int Id { get; set; }
        public string IncidentCode { get; set; }
        public string PenaltyCustomId { get; set; }
        public decimal Coefficient { get; set; }
        [UIHint("DateNullable")]
        public DateTime ApplyDate { get; set; }
        public string ApplyDateString { get; set; }
        public string Log { get; set; }

        public IList<SelectListItem> PenaltiesCatalog { get; set; }
        public IList<SelectListItem> Penalties { get; set; }
    }
}