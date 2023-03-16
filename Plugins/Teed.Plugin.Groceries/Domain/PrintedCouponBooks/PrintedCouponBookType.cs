using Nop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.PrintedCouponBooks
{
    public enum PrintedCouponBookType
    {
        [Display(Name = "Código postale")]
        ZipCode = 0,

        [Display(Name = "Subtotal")]
        Subtotal = 1,

        [Display(Name = "Cliente")]
        Client = 2,
    }
}
