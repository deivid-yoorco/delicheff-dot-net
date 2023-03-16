using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.ShippingRoutes
{
    public enum OrderOptimizationStatus
    {
        [Display(Name = "No procesado")]
        Pending = 10,
        [Display(Name = "Solicitado al cliente y pendiente de respuesta")]
        Requested = 20,
        [Display(Name = "Autorizado por el cliente y aplicado")]
        Authorized = 30,
        [Display(Name = "Rechazado por el cliente")]
        Rejected = 40
    }
}
