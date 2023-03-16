﻿using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Careers.Components
{
    [ViewComponent(Name = "CareersFooter3")]
    public class CareersFooter3ViewComponent : NopViewComponent
    {
        private readonly CareersSettings _careersSettings;
        private readonly ISettingService _settingService;

        public CareersFooter3ViewComponent(ISettingService settingService, CareersSettings careersSettings)
        {
            _settingService = settingService;
            _careersSettings = careersSettings;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var showInFooter = _careersSettings.ShowInFooter3 && _careersSettings.Published;
           return View("~/Plugins/Careers/Views/Shared/Components/CareersFooter3.cshtml", showInFooter);
        }
    }
}
