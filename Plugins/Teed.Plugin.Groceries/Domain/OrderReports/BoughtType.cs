using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Groceries.Domain.OrderReports
{
    public enum BoughtType
    {
        [Display(Name = "Efectivo")]
        Cash = 1,

        [Display(Name = "Transferencia")]
        Transfer = 2,

        [Display(Name = "Tarjeta Corporativa")]
        CorporateCard = 3,
    }
}
