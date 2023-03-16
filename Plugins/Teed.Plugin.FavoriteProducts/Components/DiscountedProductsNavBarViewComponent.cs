using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.FavoriteProducts.Models;

namespace Teed.Plugin.FavoriteProducts.Components
{
    [ViewComponent(Name = "FavoriteProductsNavBar")]
    public class FavoriteProductsNavBarViewComponent : NopViewComponent
    {
        private readonly FavoriteProductsSettings _favoriteProductsSettings;
        private readonly ISettingService _settingService;
        private FavoriteProductsSettings settings;

        public FavoriteProductsNavBarViewComponent(ISettingService settingService, FavoriteProductsSettings favoriteProductsSettings)
        {
            _settingService = settingService;
            _favoriteProductsSettings = favoriteProductsSettings;
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
                TextMenu = _favoriteProductsSettings.TextMenu,
                Active = _favoriteProductsSettings.Active,
                DivElementClassString = divElementClassString
            };
            return View("~/Plugins/Teed.Plugin.FavoriteProducts/Views/Shared/Components/FavoriteProductsNavBar.cshtml", model);
        }
    }
}
