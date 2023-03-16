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

namespace Teed.Plugin.Payroll.Models.FestiveDate
{
    public class FestiveDateModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public bool DontApplyToPayroll { get; set; }
        public bool AppliesYearly { get; set; }
        public string Log { get; set; }
    }
}