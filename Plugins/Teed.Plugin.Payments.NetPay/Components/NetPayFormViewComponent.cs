using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.NetPay.Components
{
    [ViewComponent(Name = "NetPayForm")]
    public class NetPayFormViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            return View("~/Plugins/Payments.NetPay/Views/NetPayForm.cshtml");
        }
    }
}
