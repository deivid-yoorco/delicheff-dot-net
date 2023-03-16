using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.Franchise
{
    public enum PaymentStatus
    {
        [Display(Name = "Pendiente de pago")]
        Pending = 0,

        [Display(Name = "Pagado")]
        Paid = 1,
    }
}
