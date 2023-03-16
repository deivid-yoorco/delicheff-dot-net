using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Payroll.Domain.Incidents
{
    public enum ResponsibleAreaType
    {
        [Display(Name = "No asignado")]
        None = 0,

        [Display(Name = "Compras")]
        Buyers = 1,

        [Display(Name = "Reparto")]
        Delivery = 2,

        [Display(Name = "Proveedor")]
        Provider = 3,

        [Display(Name = "Cliente")]
        Client = 4,
    }
}
