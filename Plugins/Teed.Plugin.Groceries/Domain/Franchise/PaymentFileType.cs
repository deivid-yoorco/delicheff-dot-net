using Nop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.Franchise
{
    public enum PaymentFileType
    {
        [Display(Name = "PDF de factura")]
        BillPdf = 0,

        [Display(Name = "XML de factura")]
        BillXml = 1,

        [Display(Name = "Archivo comprobante de pago")]
        ProofOfPaiment = 2,
    }
}
