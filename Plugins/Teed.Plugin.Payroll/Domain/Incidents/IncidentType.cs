using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Payroll.Domain.Incidents
{
    public enum IncidentType
    {
        [Display(Name = "Falta", 
            Description = "Falta")]
        Absence = 1,

        [Display(Name = "Retardo",
            Description = "Retardo")]
        Delay = 2,

        [Display(Name = "Descuento",
            Description = "Descuento")]
        Discount = 3
    }
}
