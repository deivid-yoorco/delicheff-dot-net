using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Payroll.Domain.Incidents
{
    public enum SolutionType
    {
        [Display(Name = "No asignado")]
        None = 0,

        [Display(Name = "Cupón")]
        Coupon = 1,

        [Display(Name = "Reposición")]
        Reposition = 2,

        [Display(Name = "Cambio de producto")]
        ProductChange = 3,

        [Display(Name = "Reembolso")]
        Refund = 4,

        [Display(Name = "No aplica solución")]
        DoesntApply = 5,
    }
}
