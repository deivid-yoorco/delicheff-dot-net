using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Payroll.Domain.Bonuses
{
    public enum BonusType
    {
        [Display(Name = "Individual",
            Description = "El bono aplica de forma individual, depende de cada empleado dentro de los empleados seleccionados.")]
        Individual = 0,

        [Display(Name = "Grupal",
            Description = "El bono aplica de forma grupal, o todos lo ganan o ninguno, dependiendo del tipo de condición.")]
        Collective = 1,
    }
}
