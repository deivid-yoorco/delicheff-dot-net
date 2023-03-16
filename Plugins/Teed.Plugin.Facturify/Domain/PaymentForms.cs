using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Facturify.Domain
{
    public enum PaymentForms
    {
        [Display(Name = "Efectivo")]
        Efectivo = 1,

        [Display(Name = "Cheque nominativo")]
        Chequenominativo = 2,

        [Display(Name = "Transferencia electrónica de fondos")]
        Transferenciaelectronicadefondos = 3,

        [Display(Name = "Tarjeta de crédito")]
        Tarjetadecredito = 4,

        [Display(Name = "Monedero electrónico")]
        Monederoelectronico = 5,

        [Display(Name = "Dinero electrónico")]
        Dineroelectronico = 6,

        [Display(Name = "Vales de despensa")]
        Valesdedespensa = 8,

        [Display(Name = "Dación en pago")]
        Dacionenpago = 12,

        [Display(Name = "Pago por subrogación")]
        Pagoporsubrogacion = 13,

        [Display(Name = "Pago por consignación")]
        Pagoporconsignacion = 14,

        [Display(Name = "Condonación")]
        Condonacion = 15,

        [Display(Name = "Compensación")]
        Compensacion = 17,

        [Display(Name = "Novación")]
        Novacion = 23,

        [Display(Name = "Confusión")]
        Confusion = 24,

        [Display(Name = "Remisión de deuda")]
        Remisiondedeuda = 25,

        [Display(Name = "Prescripción o caducidad")]
        Prescripcionocaducidad = 26,

        [Display(Name = "A satisfacción del acreedor")]
        Asatisfacciondelacreedor = 27,

        [Display(Name = "Tarjeta de débito")]
        Tarjetadedebito = 28,

        [Display(Name = "Tarjeta de servicios")]
        Tarjetadeservicios = 29,

        [Display(Name = "Aplicación de anticipos")]
        Aplicaciondeanticipos = 30,

        [Display(Name = "Por definir")]
        Pordefinir = 99
    }
}
