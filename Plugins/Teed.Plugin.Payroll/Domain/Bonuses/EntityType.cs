using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Payroll.Domain.Bonuses
{
    public enum EntityType
    {
        [Display(Name = "Puesto",
            Description = "El tipo de entidad a aplicar el bono es a un puesto.")]
        Job = 0,

        [Display(Name = "Empleado",
            Description = "El tipo de entidad a aplicar el bono es a un empleado.")]
        Employee = 1,
    }
}
