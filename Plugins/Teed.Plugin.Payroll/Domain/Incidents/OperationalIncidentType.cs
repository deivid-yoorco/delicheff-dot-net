using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Payroll.Domain.Incidents
{
    public enum OperationalIncidentType
    {
        [Display(Name = "Reparto")]
        Delivery = 1,

        [Display(Name = "Compras")]
        Buyer = 2,
    }
}
