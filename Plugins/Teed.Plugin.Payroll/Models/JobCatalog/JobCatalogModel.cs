using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.JobCatalogs;

namespace Teed.Plugin.Payroll.Models.JobCatalog
{
    public class JobCatalogModel
    {
        public JobCatalogModel()
        {
            JobCatalogs = new List<JobCatalogData>();
            AddJobCatalog = new JobCatalogData();
            AddBenefit = new AddBenefit();
            EmployeesWithoutJobAssigned = new List<SelectListItem>();
        }

        public virtual List<JobCatalogData> JobCatalogs { get; set; }
        public virtual JobCatalogData AddJobCatalog { get; set; }
        public virtual AddBenefit AddBenefit { get; set; }

        [NopResourceDisplayName("Empleos")]
        public virtual SelectList Jobs { get; set; }

        [NopResourceDisplayName("Empleo padre")]
        public virtual SelectList JobParents { get; set; }

        [NopResourceDisplayName("Tipo de prestación")]
        public virtual SelectList BenefitTypes { get; set; }

        public string OrganigramString { get; set; }

        public List<SelectListItem> EmployeesWithoutJobAssigned { get; set; }
    }

    public class JobCatalogData
    {
        public JobCatalogData()
        {
            HoursSelect = new List<SelectListItem>();
            MinutesSelect = new List<SelectListItem>();
            JobParents = new List<SelectListItem>();
            PayrollEmployees = new List<Domain.PayrollEmployees.PayrollEmployee>();
        }

        public virtual int Id { get; set; }

        [NopResourceDisplayName("Nombre")]
        public virtual string Name { get; set; }

        [NopResourceDisplayName("Descripción")]
        public virtual string Description { get; set; }

        [NopResourceDisplayName("Orden para mostrar")]
        public virtual int DisplayOrder { get; set; }

        [NopResourceDisplayName("Sueldo mínimo")]
        public virtual decimal SalaryMin { get; set; }

        [NopResourceDisplayName("Sueldo máximo")]
        public virtual decimal SalaryMax { get; set; }


        [NopResourceDisplayName("Horario de trabajo")]
        public virtual string WorkSchedule { get; set; }

        [NopResourceDisplayName("Lunes")]
        public virtual bool Works_Monday { get; set; }

        [NopResourceDisplayName("Martes")]
        public virtual bool Works_Tuesday { get; set; }

        [NopResourceDisplayName("Miércoles")]
        public virtual bool Works_Wednesday { get; set; }

        [NopResourceDisplayName("Jueves")]
        public virtual bool Works_Thursday { get; set; }

        [NopResourceDisplayName("Viernes")]
        public virtual bool Works_Friday { get; set; }

        [NopResourceDisplayName("Sabado")]
        public virtual bool Works_Saturday { get; set; }

        [NopResourceDisplayName("Domingo")]
        public virtual bool Works_Sunday { get; set; }
        
        public virtual int MondayInHour { get; set; }
        public virtual int MondayOutHour { get; set; }
        public virtual int MondayInMinutes { get; set; }
        public virtual int MondayOutMinutes { get; set; }

        public virtual int TuesdayInHour { get; set; }
        public virtual int TuesdayOutHour { get; set; }
        public virtual int TuesdayInMinutes { get; set; }
        public virtual int TuesdayOutMinutes { get; set; }

        public virtual int WednesdayInHour { get; set; }
        public virtual int WednesdayOutHour { get; set; }
        public virtual int WednesdayInMinutes { get; set; }
        public virtual int WednesdayOutMinutes { get; set; }

        public virtual int ThursdayInHour { get; set; }
        public virtual int ThursdayOutHour { get; set; }
        public virtual int ThursdayInMinutes { get; set; }
        public virtual int ThursdayOutMinutes { get; set; }

        public virtual int FridayInHour { get; set; }
        public virtual int FridayOutHour { get; set; }
        public virtual int FridayInMinutes { get; set; }
        public virtual int FridayOutMinutes { get; set; }

        public virtual int SaturdayInHour { get; set; }
        public virtual int SaturdayOutHour { get; set; }
        public virtual int SaturdayInMinutes { get; set; }
        public virtual int SaturdayOutMinutes { get; set; }

        public virtual int SundayInHour { get; set; }
        public virtual int SundayOutHour { get; set; }
        public virtual int SundayInMinutes { get; set; }
        public virtual int SundayOutMinutes { get; set; }

        public virtual List<SelectListItem> HoursSelect { get; set; }
        public virtual List<SelectListItem> MinutesSelect { get; set; }


        [NopResourceDisplayName("Empleo padre")]
        public virtual int JobParentId { get; set; }
        public virtual int ParentJob { get; set; }
        public virtual List<SelectListItem> JobParents { get; set; }

        public virtual List<Domain.PayrollEmployees.PayrollEmployee> PayrollEmployees { get; set; }

        [NopResourceDisplayName("Sujeto a horario de trabajo")]
        public virtual bool SubjectToWorkingHours { get; set; }
    }
    public partial class AddBenefit
    {
        public virtual int Id { get; set; }
        public virtual int LinkId { get; set; }

        [NopResourceDisplayName("Nombre")]
        public virtual string Name { get; set; }

        [NopResourceDisplayName("Descripción")]
        public virtual string Description { get; set; }

        [NopResourceDisplayName("Monto")]
        public virtual decimal Amount { get; set; }
        public virtual bool isForSalary { get; set; }
    }
}
