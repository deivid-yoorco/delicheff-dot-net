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

namespace Teed.Plugin.Payroll.Models.Bonus
{
    public class AmountModel
    {
        public int BonusId { get; set; }
        public virtual DateTime ApplyDate { get; set; }
        public virtual decimal Amount { get; set; }
    }
}