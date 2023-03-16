using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.BiweeklyPayments;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Models.BiweeklyPayment
{
    public class BiweeklyDatesModel
    {
        public DateTime OriginalDate { get; set; }
        public DateTime StartOfBiweekly { get; set; }
        public DateTime EndOfBiweekly { get; set; }
    }
}
