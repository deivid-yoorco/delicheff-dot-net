using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.Stripe.Components
{
    [ViewComponent(Name = "StripeJs")]
    public class StripeJsViewComponent : NopViewComponent
    {
        private readonly ISettingService _settingService;

        public StripeJsViewComponent(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
           return View("~/Plugins/Payments.Stripe/Views/StripeJs.cshtml");
        }
    }
}