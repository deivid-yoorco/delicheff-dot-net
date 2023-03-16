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

namespace Teed.Plugin.Payroll.Models.Incident
{
    public class AddOrCheckModel
    {
        public AddOrCheckModel()
        {
            AddIncident = new AddIncident();
        }

        public virtual int Id { get; set; }
        public virtual int BossId { get; set; }
        public virtual SelectList Types { get; set; }

        [NopResourceDisplayName("Tipo de incidencia")]
        public virtual int Type { get; set; }
        public virtual AddIncident AddIncident { get; set; }
    }

    public partial class AddIncident
    {
        public virtual int Id { get; set; }

        [NopResourceDisplayName("Fecha de la incidencia")]
        [UIHint("DateNullable")]
        public virtual DateTime? Date { get; set; }

        [NopResourceDisplayName("Monto total a descontar")]
        public virtual decimal? Amount { get; set; }

        [NopResourceDisplayName("Razón del descuento")]
        public virtual string Reason { get; set; }

        public virtual bool Applied { get; set; }

        [NopResourceDisplayName("Justificado")]
        public virtual bool Justified { get; set; }
        public virtual IncidentType IncidentType { get; set; }

        public virtual IFormFile File { get; set; }

        public virtual int PayrollEmployeeId { get; set; }
        public virtual bool FileExists { get; set; }
        public virtual SelectList Types { get; set; }
        public virtual int Type { get; set; }

        public string Log { get; set; }
    }
}
