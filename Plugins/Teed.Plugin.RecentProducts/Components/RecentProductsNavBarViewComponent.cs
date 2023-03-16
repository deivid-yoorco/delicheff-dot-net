using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.RecentProducts.Models;

namespace Teed.Plugin.RecentProducts.Components
{
    [ViewComponent(Name = "RecentProductsNavBar")]
    public class RecentProductsNavBarViewComponent : NopViewComponent
    {
        private readonly RecentProductsSettings _recentProductsSettings;
        private readonly ISettingService _settingService;
        private RecentProductsSettings settings;

        public RecentProductsNavBarViewComponent(ISettingService settingService, RecentProductsSettings recentProductsSettings)
        {
            _settingService = settingService;
            _recentProductsSettings = recentProductsSettings;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var divElementClassString = string.Empty;
            try
            {
                divElementClassString = (string)additionalData;
            }
            catch (Exception e) { _ = e; }
            var model = new ConfigurationModel
            {
                TextMenu = _recentProductsSettings.TextMenu,
                Active = _recentProductsSettings.Active,
                DivElementClassString = divElementClassString
            };
            return View("~/Plugins/Teed.Plugin.RecentProducts/Views/Shared/Components/RecentProductsNavBar.cshtml", model);
        }
    }
}
