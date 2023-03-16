using Nop.Web.Framework.Mvc.Models;

namespace Teed.Plugin.Payments.MercadoPagoQr.Models
{
    public class PaymentInfoModel : BaseNopModel
    {
        public string DescriptionText { get; set; }
    }
}