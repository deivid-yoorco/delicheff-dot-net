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
    public class BonusModel
    {
        public BonusModel()
        {
            AuthorizationEmployees = new List<SelectListItem>();
            Jobs = new List<SelectListItem>();
            JobIds = new List<int>();
        }

        public int Id { get; set; }

        public virtual string Name { get; set; }
        public IList<int> JobIds { get; set; }
        public virtual int ConditionTypeId { get; set; }
        public virtual int BonusTypeId { get; set; }
        public virtual int FrequencyTypeId { get; set; }
        public virtual int AuthorizationEmployeeId { get; set; }
        public virtual bool IsActive { get; set; }

        public IList<SelectListItem> Jobs { get; set; }
        public SelectList ConditionTypes { get; set; }
        public SelectList BonusTypes { get; set; }
        public SelectList FrequencyTypes { get; set; }
        public IList<SelectListItem> AuthorizationEmployees { get; set; }

        public string Log { get; set; }
    }
}