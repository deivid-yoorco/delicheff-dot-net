using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Payroll.Domain.Bonuses
{
    public enum ConditionType
    {
        [Display(Name = "Manual",
            Description = "La condición del bono aplica de forma manual, es independiente por día o por quincena y se debe de aplicar de forma manual.")]
        Manual = 0,

        [Display(Name = "Automático",
            Description = "La condición del bono aplica de forma automática, si se tienen retardos o faltas dentro de la quincena no aplica el bono.")]
        Automatic = 1,
    }
}
