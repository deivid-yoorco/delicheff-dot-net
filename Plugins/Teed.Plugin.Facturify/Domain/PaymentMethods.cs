using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Facturify.Domain
{
    public enum PaymentMethods
    {
        [Display(Name = "Pago en una sola exhibición")]
        PUE = 0,

        [Display(Name = "Pago en parcialidades o diferido")]
        PPD = 1,
    }
}