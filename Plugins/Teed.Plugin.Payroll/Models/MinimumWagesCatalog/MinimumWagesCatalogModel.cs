using Microsoft.AspNetCore.Http;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Models.MinimumWagesCatalog
{
    public class MinimumWagesCatalogModel
    {
        public virtual int Id { get; set; }

        [NopResourceDisplayName("Monto")]
        public virtual decimal Amount { get; set; }

        [NopResourceDisplayName("UMA")]
        public virtual decimal Uma { get; set; }

        [NopResourceDisplayName("Año")]
        public virtual int Year { get; set; }
    }
}
