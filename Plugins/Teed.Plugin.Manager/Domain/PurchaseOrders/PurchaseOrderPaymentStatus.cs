using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Manager.Domain.PurchaseOrders
{
    public enum PurchaseOrderPaymentStatus
    {
        [Display(Name = "Pendiente")]
        Pending = 0,

        [Display(Name = "Pagado parcial")]
        PartiallyPaid = 1,

        [Display(Name = "Pagado")]
        Paid = 2,
    }
}