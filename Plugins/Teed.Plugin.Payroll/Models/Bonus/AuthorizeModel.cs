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
    public class AuthorizeModel
    {
        public AuthorizeModel()
        {
            JobsOfBonus = new List<JobData>();
            EmployeesOfBonus = new List<EmployeeData>();
            Entities = new List<ExistingEntity>();
        }

        public virtual bool IsAuth { get; set; }
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual IList<int> JobIds { get; set; }
        public virtual int ConditionTypeId { get; set; }
        public virtual int BonusTypeId { get; set; }
        public virtual int FrequencyTypeId { get; set; }
        public virtual int AuthorizationEmployeeId { get; set; }
        public virtual bool IsActive { get; set; }

        public virtual string SelectedEntities { get; set; }
        public virtual List<JobData> JobsOfBonus { get; set; }
        public virtual List<EmployeeData> EmployeesOfBonus { get; set; }
        public virtual List<ExistingEntity> Entities { get; set; }

        public virtual string Log { get; set; }
        public virtual string ApplicationsLog { get; set; }
        public virtual bool SaveAndContinue { get; set; }
    }

    public class JobData
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class EmployeeData
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SelectedEntity
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime? OldDate { get; set; }
        public bool Apply { get; set; }
        public bool IsDelete { get; set; }
    }

    public class ExistingEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string RegularBiweekDate { get; set; }
        public string Date { get; set; }
        public string EndDate { get; set; }
        public bool Apply { get; set; }
        public bool IsDelete { get; set; }
    }
}