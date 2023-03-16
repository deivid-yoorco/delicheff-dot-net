using Nop.Web.Framework.Mvc.Models;

namespace Teed.Plugin.Payments.ManualTransfer.Models
{
    public class PaymentInfoModel : BaseNopModel
    {
        public string DescriptionText { get; set; }
    }
}