using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.BuyerPayments
{
    public enum BuyerPaymentStatus
    {
        [Display(Name = "Pendiente")]
        Pending = 10,

        [Display(Name = "Pagado")]
        Payed = 20,

        [Display(Name = "No autorizado")]
        Unauthorized = 30
    }
}
