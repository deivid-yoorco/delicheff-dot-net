using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Services.Stores;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.CornerForm.Components
{
    [ViewComponent(Name = "TeedCornerForm")]
    public class TeedCornerFormViewComponent : NopViewComponent
    {
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;

        public TeedCornerFormViewComponent(IStoreService storeService, ISettingService settingService)
        {
            _settingService = settingService;
            _storeService = storeService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            CornerFormSettings pluginSettings = _settingService.LoadSetting<CornerFormSettings>();
            return View("~/Plugins/Teed.Plugin.CornerForm/Views/CornerForm/PublicView.cshtml", pluginSettings);
        }
    }
}
