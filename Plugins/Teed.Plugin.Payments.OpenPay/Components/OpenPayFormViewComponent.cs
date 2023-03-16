using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.OpenPay.Components
{
    [ViewComponent(Name = "OpenPayForm")]
    public class OpenPayFormViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            return View("~/Plugins/Payments.OpenPay/Views/OpenPayForm.cshtml");
        }
    }
}
