using Microsoft.AspNetCore.Http;
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
    public class BillingModel
    {
        public int Id { get; set; }

        [UIHint("DateNullable")]
        public DateTime InitDate { get; set; }

        [UIHint("DateNullable")]
        public DateTime EndDate { get; set; }
        public decimal Billed { get; set; }
        public string Comment { get; set; }
        public decimal AmountAjustment { get; set; }
        public string Log { get; set; }
        public List<SelectListItem> FileTypes { get; set; }
        public int FranchiseId { get; set; }
        public List<SelectListItem> Franchises { get; set; }
    }
}