using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Manager.Domain.PurchaseOrders
{
    public enum PurchaseOrderStatus
    {
        [Display(Name = "Pendiente")]
        Pending = 0,

        [Display(Name = "En revisión")]
        Reviewing = 1,

        [Display(Name = "Aprobada")]
        Approved = 2,

        [Display(Name = "Solicitado al proveedor")]
        Requested = 3,

        [Display(Name = "Entregado")]
        Delivered = 4,
    }
}
