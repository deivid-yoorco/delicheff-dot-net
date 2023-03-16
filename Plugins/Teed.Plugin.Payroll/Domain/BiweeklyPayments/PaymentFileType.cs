using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Payroll.Domain.BiweeklyPayments
{
    public enum PaymentFileType
    {
        [Display(Name = "Recibos de nómina firmados por el empleado", 
            Description = "Recibos de nómina firmados por el empleado")]
        ReceiptsSignedByEmployee = 1,

        [Display(Name = "Comprobante de pago de la nómina",
            Description = "Comprobante de pago de la nómina")]
        ProofOfPayroll = 2,

        [Display(Name = "Comprobante de retención de ISR",
            Description = "Comprobante de retención de ISR")]
        TaxWithholdingReceipt = 3,

        [Display(Name = "Comprobante del IMSS",
            Description = "Comprobante del IMSS")]
        ProofOfIMSS = 4
    }
}
