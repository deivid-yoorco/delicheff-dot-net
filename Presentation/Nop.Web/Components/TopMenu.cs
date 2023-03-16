using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Media;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.HomePageImages;

namespace Nop.Web.Components
{
    public class TopMenuViewComponent : NopViewComponent
    {
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly IPictureService _pictureService;
        private readonly IStaticCacheManager _staticCacheManager;

        public TopMenuViewComponent(ICatalogModelFactory catalogModelFactory,
            ISettingService settingService,
            IStoreService storeService, 
            IWorkContext workContext,
            IPictureService pictureService,
            IStaticCacheManager staticCacheManager)
        {
            this._catalogModelFactory = catalogModelFactory;
            this._settingService = settingService;
            this._storeService = storeService;
            this._workContext = workContext;
            this._pictureService = pictureService;
            this._staticCacheManager = staticCacheManager;
        }

        public IViewComponentResult Invoke(int? productThumbPictureSize, bool categoriesOnly = false)
        {
            var model = _catalogModelFactory.PrepareTopMenuModel();
            model.DisplayCategoriesOnly = categoriesOnly;

            // Get slick arrow images
            var storeId = 0;
            if (!(_storeService.GetAllStores().Count < 2)) {
                storeId = _workContext.CurrentCustomer.GetAttribute<int>(SystemCustomerAttributeNames.AdminAreaStoreScopeConfiguration);
                var store = _storeService.GetStoreById(storeId);
                storeId = store != null ? store.Id : 0;
            }
            var homePageSettings = _settingService.LoadSetting<HomePageImagesSettings>(storeId);
            var cacheKey = string.Format("Teed.Nop.plugins.widgets.nivoslider2.pictureurl-{0}", homePageSettings.BannerPictureArrowId);
            model.SlickArrowsImgUrl = _staticCacheManager.Get(cacheKey, () =>
            {
                //little hack here. nulls aren't cacheable so set it to ""
                var url = _pictureService.GetPictureUrl(homePageSettings.BannerPictureArrowId, showDefaultPicture: false) ?? "";
                return url;
            });
            return View(model);
        }
    }
}
