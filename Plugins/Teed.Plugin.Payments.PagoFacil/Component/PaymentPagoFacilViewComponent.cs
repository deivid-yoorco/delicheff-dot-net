using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Teed.Plugin.Payments.PagoFacil.Component
{
    [ViewComponent(Name = "PaymentPagoFacil")]
    public class PaymentPagoFacilViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.PagoFacil/Views/PaymentInfo.cshtml");
        }
    }
}
