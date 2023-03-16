using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Manager.Domain.Expenses
{
    public enum PaymentType
    {
        [Display(Name = "Efectivo")]
        Cash = 0,

        [Display(Name = "Tarjeta de crédito propia (para reembolso)")]
        SelfCard = 1,

        [Display(Name = "Tarjeta corporativa")]
        CompanyCard = 2,

        [Display(Name = "Transferencia electrónica")]
        Transfer = 3,

        [Display(Name = "Paypal")]
        Paypal = 4,

        [Display(Name = "Cheque")]
        Check = 5,
    }
}