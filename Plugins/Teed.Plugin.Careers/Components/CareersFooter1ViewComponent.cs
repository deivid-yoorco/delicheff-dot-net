using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Careers.Components
{
    [ViewComponent(Name = "CareersFooter1")]
    public class CareersFooter1ViewComponent : NopViewComponent
    {
        private readonly CareersSettings _careersSettings;
        private readonly ISettingService _settingService;

        public CareersFooter1ViewComponent(ISettingService settingService, CareersSettings careersSettings)
        {
            _settingService = settingService;
            _careersSettings = careersSettings;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var showInFooter = _careersSettings.ShowInFooter1 && _careersSettings.Published;
           return View("~/Plugins/Careers/Views/Shared/Components/CareersFooter1.cshtml", showInFooter);
        }
    }
}
