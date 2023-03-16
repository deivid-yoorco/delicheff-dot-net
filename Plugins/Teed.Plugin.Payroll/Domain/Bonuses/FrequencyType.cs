using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Payroll.Domain.Bonuses
{
    public enum FrequencyType
    {
        [Display(Name = "Por día",
            Description = "El bono se dará de forma diaria cuando aplique.")]
        ByDay = 0,

        [Display(Name = "Por quincena",
            Description = "El bono se dará de forma quincenal cuando aplique.")]
        ByBiweek = 1,
    }
}
