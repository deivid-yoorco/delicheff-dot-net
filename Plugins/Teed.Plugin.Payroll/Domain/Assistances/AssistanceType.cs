using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Payroll.Domain.Assistances
{
    public enum AssistanceType
    {
        [Display(Name = "A tiempo", 
            Description = "Se llegó a tiempo.")]
        InTime = 1,

        [Display(Name = "Retardo",
            Description = "Se llegó después de la hora de entrada.")]
        Delay = 2,

        [Display(Name = "Falta",
            Description = "No se llegó en todo el día.")]
        Absence = 3,

        [Display(Name = "Falta por acumulación de retardos",
            Description = "Falta por acumulación de retardos.")]
        AbsenceByDelays = 4,

        [Display(Name = "Incapacidad pagada",
            Description = "Incapacidad pagada.")]
        PayedDisability = 5,

        [Display(Name = "Incapacidad subsidiada por IMSS",
            Description = "Incapacidad subsidiada por IMSS.")]
        SubsidizedDisability = 6,

        [Display(Name = "Vacación",
            Description = "Vacación")]
        Vacation = 7
    }
}
