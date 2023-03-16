using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.OrderReports
{
    public enum NotBuyedReason
    {
        [Display(Name = "Producto fuera de temporada")]
        FueraDeTemporada = 1,

        [Display(Name = "El único proveedor está cerrado")]
        ProveedorCerrado = 2,

        [Display(Name = "No hay disponibilidad por el momento")]
        SinDisponibilidad = 3,

        [Display(Name = "El mínimo de compra es mayor que la cantidad solicitada")]
        MinimoDeCompra = 4,

        [Display(Name = "El costo es muy superior al precio de venta")]
        CostoMuyAlto = 5,
    }
}
