using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.Incidents;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Models
{
    public class FiltersXlsModel
    {
        public FiltersXlsModel()
        {
            AvailableJobs = new List<SelectListItem>();
            JobIds = new List<int>();
        }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IList<int> JobIds { get; set; }
        public IList<SelectListItem> AvailableJobs { get; set; }
    }
}
