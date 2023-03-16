using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.DiscountedProducts.Models;

namespace Teed.Plugin.DiscountedProducts.Components
{
    [ViewComponent(Name = "DiscountedProductsNavBar")]
    public class DiscountedProductsNavBarViewComponent : NopViewComponent
    {
        private readonly DiscountedProductsSettings _discountedProductsSettings;
        private readonly ISettingService _settingService;
        private DiscountedProductsSettings settings;

        public DiscountedProductsNavBarViewComponent(ISettingService settingService, DiscountedProductsSettings discountedProductsSettings)
        {
            _settingService = settingService;
            _discountedProductsSettings = discountedProductsSettings;
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
                TextMenu = _discountedProductsSettings.TextMenu,
                Active = _discountedProductsSettings.Active,
                DivElementClassString = divElementClassString
            };
            return View("~/Plugins/Teed.Plugin.DiscountedProducts/Views/Shared/Components/DiscountedProductsNavBar.cshtml", model);
        }
    }
}
