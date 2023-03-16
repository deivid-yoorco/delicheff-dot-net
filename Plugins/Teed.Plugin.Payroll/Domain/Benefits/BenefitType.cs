using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Payroll.Domain.Benefits
{
    public enum BenefitType
    {
        [Display(Name = "Aguinaldo", 
            Description = "Aguinaldo")]
        Bonus = 1,

        [Display(Name = "Prima vacacional",
            Description = "Prima vacacional")]
        VacationBonus = 2,

        [Display(Name = "Vacaciones",
            Description = "Vacaciones")]
        Vacations = 3,

        [Display(Name = "Otras prestaciones",
            Description = "Otras prestaciones")]
        Other = 4
    }
}
