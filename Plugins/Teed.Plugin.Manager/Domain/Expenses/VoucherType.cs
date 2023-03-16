using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Manager.Domain.Expenses
{
    public enum VoucherType
    {
        [Display(Name = "Nota")]
        Note = 0,

        [Display(Name = "Factura CFDI")]
        CFDI = 1
    }
}
