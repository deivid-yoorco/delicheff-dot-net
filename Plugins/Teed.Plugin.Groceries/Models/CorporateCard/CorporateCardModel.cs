using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Orders;

namespace Teed.Plugin.Groceries.Models.CorporateCard
{
    public class CorporateCardModel
    {
        public CorporateCardModel()
        {
            Statuses = new List<SelectListItem>();
            Rules = new List<SelectListItem>();
        }

        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Rfc { get; set; }
        public string Curp { get; set; }
        public string Phone { get; set; }
        public string Job { get; set; }
        public string CardNumber { get; set; }
        public int StatusId { get; set; }
        public List<SelectListItem> Statuses { get; set; }

        public int RuleId { get; set; }
        public List<SelectListItem> Rules { get; set; }

        public string Log { get; set; }
    }
}