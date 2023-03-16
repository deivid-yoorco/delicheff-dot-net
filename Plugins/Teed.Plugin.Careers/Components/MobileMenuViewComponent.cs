using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;

namespace Teed.Plugin.Careers.Components
{
    [ViewComponent(Name = "MobileMenu")]
    public class MobileMenuViewComponent : NopViewComponent
    {
        private readonly CareersSettings _careersSettings;
        private readonly ISettingService _settingService;

        public MobileMenuViewComponent(ISettingService settingService, CareersSettings careersSettings)
        {
            _settingService = settingService;
            _careersSettings = careersSettings;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var showInFooter = _careersSettings.ShowInMobileMenu && _careersSettings.Published;
           return View("~/Plugins/Careers/Views/Shared/Components/MobileMenu.cshtml", showInFooter);
        }
    }
}
