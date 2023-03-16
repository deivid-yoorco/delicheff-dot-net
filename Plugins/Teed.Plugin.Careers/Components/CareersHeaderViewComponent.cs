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
    [ViewComponent(Name = "CareersHeader")]
    public class CareersHeaderViewComponent : NopViewComponent
    {
        private readonly CareersSettings _careersSettings;
        private readonly ISettingService _settingService;

        public CareersHeaderViewComponent(ISettingService settingService, CareersSettings careersSettings)
        {
            _settingService = settingService;
            _careersSettings = careersSettings;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var showInHeader = _careersSettings.ShowInHeader && _careersSettings.Published;
           return View("~/Plugins/Careers/Views/Shared/Components/CareersHeader.cshtml", showInHeader);
        }
    }
}
