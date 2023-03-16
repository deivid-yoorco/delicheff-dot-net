using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.ShippingRoutes
{
    public enum NotDeliveredReason
    {
        [Display(Name = "El artículo no se compró")]
        NoSeCompro = 1,

        [Display(Name = "El artículo no era el solicitado por el cliente")]
        NoEraElSolicitado = 2,

        [Display(Name = "El artículo pertenecía a otra ruta")]
        PerteneciaOtraRuta = 3,
    }
}
