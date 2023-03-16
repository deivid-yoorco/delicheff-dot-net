using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.ComproPago.Component
{
    [ViewComponent(Name = "PaymentComproPago")]
    public class PaymentComproPagoViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.ComproPago/Views/PaymentInfo.cshtml");
        }
    }
}
