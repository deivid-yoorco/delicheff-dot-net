using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.OpenPay.Components
{
    [ViewComponent(Name = "OpenPayJs")]
    public class OpenPayJsViewComponent : NopViewComponent
    {
        private readonly ISettingService _settingService;

        public OpenPayJsViewComponent(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
           return View("~/Plugins/Payments.OpenPay/Views/OpenPayJs.cshtml");
        }
    }
}