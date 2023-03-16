using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Teed.Nop.Plugin.Widgets.NivoSlider2.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Services.Catalog;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Models.Catalog;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;

namespace Teed.Nop.Plugin.Widgets.NivoSlider2.Controllers
{
    [Area(AreaNames.Admin)]
    public class HomePageImagesController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly ICategoryService _categoryService;
        private readonly IProductTagService _productTagService;

        public HomePageImagesController(IWorkContext workContext,
            IStoreService storeService,
            IPermissionService permissionService,
            IPictureService pictureService,
            ISettingService settingService,
            ICacheManager cacheManager,
            ILocalizationService localizationService,
            ICategoryService categoryService,
            IProductTagService productTagService)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._permissionService = permissionService;
            this._pictureService = pictureService;
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._categoryService = categoryService;
            this._productTagService = productTagService;
        }

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var homePageSettings = _settingService.LoadSetting<HomePageImagesSettings>(storeScope);
            var model = new ConfigurationModel
            {
                //Banner
                BannerPicture1Id = homePageSettings.BannerPicture1Id,
                BannerText1 = homePageSettings.BannerText1,
                BannerLink1 = homePageSettings.BannerLink1,
                BannerPicture2Id = homePageSettings.BannerPicture2Id,
                BannerText2 = homePageSettings.BannerText2,
                BannerLink2 = homePageSettings.BannerLink2,
                BannerPicture3Id = homePageSettings.BannerPicture3Id,
                BannerText3 = homePageSettings.BannerText3,
                BannerLink3 = homePageSettings.BannerLink3,
                BannerPicture4Id = homePageSettings.BannerPicture4Id,
                BannerText4 = homePageSettings.BannerText4,
                BannerLink4 = homePageSettings.BannerLink4,

                NewBannerPicture5Id = homePageSettings.NewBannerPicture5Id,
                NewBannerText5 = homePageSettings.NewBannerText5,
                NewBannerLink5 = homePageSettings.NewBannerLink5,
                NewBannerPicture6Id = homePageSettings.NewBannerPicture6Id,
                NewBannerText6 = homePageSettings.NewBannerText6,
                NewBannerLink6 = homePageSettings.NewBannerLink6,

                BannerPictureArrowId = homePageSettings.BannerPictureArrowId,

                VideoId = homePageSettings.VideoId,
                BannerPicture5Id = homePageSettings.BannerPicture5Id,
                BannerTitle5 = homePageSettings.BannerTitle5,
                BannerSubTitle5 = homePageSettings.BannerSubTitle5,
                BannerLink5 = homePageSettings.BannerLink5,
                BannerTitleColor5 = homePageSettings.BannerTitleColor5,
                BannerSubTitleColor5 = homePageSettings.BannerSubTitleColor5,
                BannerPicture6Id = homePageSettings.BannerPicture6Id,
                BannerTitle6 = homePageSettings.BannerTitle6,
                BannerSubTitle6 = homePageSettings.BannerSubTitle6,
                BannerLink6 = homePageSettings.BannerLink6,
                BannerTitleColor6 = homePageSettings.BannerTitleColor6,
                BannerSubTitleColor6 = homePageSettings.BannerSubTitleColor6,

                Picture1Id = homePageSettings.Picture1Id,
                Text1 = homePageSettings.Text1,
                Link1 = homePageSettings.Link1,
                Picture2Id = homePageSettings.Picture2Id,
                Text2 = homePageSettings.Text2,
                Link2 = homePageSettings.Link2,
                Picture3Id = homePageSettings.Picture3Id,
                Text3 = homePageSettings.Text3,
                Link3 = homePageSettings.Link3,
                Picture4Id = homePageSettings.Picture4Id,
                Text4 = homePageSettings.Text4,
                Link4 = homePageSettings.Link4,
                Picture5Id = homePageSettings.Picture5Id,
                Text5 = homePageSettings.Text5,
                Link5 = homePageSettings.Link5,
                Picture6Id = homePageSettings.Picture6Id,
                Text6 = homePageSettings.Text6,
                Link6 = homePageSettings.Link6,
                TextColor6 = homePageSettings.TextColor6,
                TextDropdown1 = homePageSettings.TextDropdown1,
                LinkDropdown1 = homePageSettings.LinkDropdown1,
                TextDropdown2 = homePageSettings.TextDropdown2,
                LinkDropdown2 = homePageSettings.LinkDropdown2,
                TextDropdown3 = homePageSettings.TextDropdown3,
                LinkDropdown3 = homePageSettings.LinkDropdown3,
                TextDropdown4 = homePageSettings.TextDropdown4,
                LinkDropdown4 = homePageSettings.LinkDropdown4,
                TextDropdown5 = homePageSettings.TextDropdown5,
                LinkDropdown5 = homePageSettings.LinkDropdown5,
                TextDropdown6 = homePageSettings.TextDropdown6,
                LinkDropdown6 = homePageSettings.LinkDropdown6,
                TextDropdown7 = homePageSettings.TextDropdown7,
                LinkDropdown7 = homePageSettings.LinkDropdown7,
                TextDropdown8 = homePageSettings.TextDropdown8,
                LinkDropdown8 = homePageSettings.LinkDropdown8,
                TextDropdown9 = homePageSettings.TextDropdown9,
                LinkDropdown9 = homePageSettings.LinkDropdown9,
                TextDropdown10 = homePageSettings.TextDropdown10,
                LinkDropdown10 = homePageSettings.LinkDropdown10,

                TextCarousel = homePageSettings.TextCarousel,
                TextColorCarousel = homePageSettings.TextColorCarousel,

                Picture7Id = homePageSettings.Picture7Id,
                Link7 = homePageSettings.Link7,
                Picture8Id = homePageSettings.Picture8Id,
                Link8 = homePageSettings.Link8,
                Picture9Id = homePageSettings.Picture9Id,
                Link9 = homePageSettings.Link9,
                Picture10Id = homePageSettings.Picture10Id,
                Link10 = homePageSettings.Link10,
                Picture11Id = homePageSettings.Picture11Id,
                Link11 = homePageSettings.Link11,
                Picture12Id = homePageSettings.Picture12Id,
                Link12 = homePageSettings.Link12,
                Picture13Id = homePageSettings.Picture13Id,
                Link13 = homePageSettings.Link13,
                Picture14Id = homePageSettings.Picture14Id,
                Link14 = homePageSettings.Link14,
                Picture15Id = homePageSettings.Picture15Id,
                Link15 = homePageSettings.Link15,
                Picture16Id = homePageSettings.Picture16Id,
                Link16 = homePageSettings.Link16,
                Picture17Id = homePageSettings.Picture17Id,
                Link17 = homePageSettings.Link17,
                Picture18Id = homePageSettings.Picture18Id,
                Link18 = homePageSettings.Link18,

                TagsQty = homePageSettings.TagsQty,
                TagsEnable = homePageSettings.TagsEnable,

                ManufacturerEnable = homePageSettings.ManufacturerEnable,
                TitleManufacturer = homePageSettings.TitleManufacturer,

                CollageEnable = homePageSettings.CollageEnable,

                Picture19Id = homePageSettings.Picture19Id,
                Link19 = homePageSettings.Link19,
                Picture20Id = homePageSettings.Picture20Id,
                Link20 = homePageSettings.Link20,
                Picture21Id = homePageSettings.Picture21Id,
                Link21 = homePageSettings.Link21,
                Picture22Id = homePageSettings.Picture22Id,
                Link22 = homePageSettings.Link22,
                Picture23Id = homePageSettings.Picture23Id,
                Link23 = homePageSettings.Link23,
                Picture24Id = homePageSettings.Picture24Id,
                Link24 = homePageSettings.Link24,
                Picture25Id = homePageSettings.Picture25Id,
                Link25 = homePageSettings.Link25,
                Picture26Id = homePageSettings.Picture26Id,
                Link26 = homePageSettings.Link26,
                Picture27Id = homePageSettings.Picture27Id,
                Link27 = homePageSettings.Link27,
                Picture28Id = homePageSettings.Picture28Id,
                Link28 = homePageSettings.Link28,
                Picture29Id = homePageSettings.Picture29Id,
                Link29 = homePageSettings.Link29,
                Picture30Id = homePageSettings.Picture30Id,
                Link30 = homePageSettings.Link30,
                Picture31Id = homePageSettings.Picture31Id,
                Link31 = homePageSettings.Link31,
                Picture32Id = homePageSettings.Picture32Id,
                Link32 = homePageSettings.Link32,
                Picture33Id = homePageSettings.Picture33Id,
                Link33 = homePageSettings.Link33,
                Picture34Id = homePageSettings.Picture34Id,
                Link34 = homePageSettings.Link34,
                Picture35Id = homePageSettings.Picture35Id,
                Link35 = homePageSettings.Link35,
                Picture36Id = homePageSettings.Picture36Id,
                Link36 = homePageSettings.Link36,
                Picture37Id = homePageSettings.Picture37Id,
                Link37 = homePageSettings.Link37,
                Picture38Id = homePageSettings.Picture38Id,
                Link38 = homePageSettings.Link38,

                PopUpEnable = homePageSettings.PopUpEnable,
                Picture39Id = homePageSettings.Picture39Id,
                Picture39ResponsiveId = homePageSettings.Picture39ResponsiveId,

                VendorEnable = homePageSettings.VendorEnable,
                TitleVendor = homePageSettings.TitleVendor,

                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.BannerPicture1Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerPicture1Id, storeScope);
                model.BannerText1_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerText1, storeScope);
                model.BannerLink1_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerLink1, storeScope);
                model.BannerPicture2Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerPicture2Id, storeScope);
                model.BannerText2_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerText2, storeScope);
                model.BannerLink2_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerLink2, storeScope);
                model.BannerPicture3Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerPicture3Id, storeScope);
                model.BannerText3_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerText3, storeScope);
                model.BannerLink3_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerLink3, storeScope);
                model.BannerPicture4Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerPicture4Id, storeScope);
                model.BannerText4_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerText4, storeScope);
                model.BannerLink4_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerLink4, storeScope);
                model.VideoId_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.VideoId, storeScope);
                model.BannerPicture5Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerPicture5Id, storeScope);
                model.BannerTitle5_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerTitle5, storeScope);
                model.BannerSubTitle5_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerSubTitle5, storeScope);
                model.BannerLink5_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerLink5, storeScope);
                model.BannerTitleColor5_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerTitleColor5, storeScope);
                model.BannerSubTitleColor5_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerSubTitleColor5, storeScope);
                model.BannerPicture6Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerPicture6Id, storeScope);
                model.BannerTitle6_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerTitle6, storeScope);
                model.BannerSubTitle6_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerSubTitle6, storeScope);
                model.BannerLink6_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerLink6, storeScope);
                model.BannerTitleColor6_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerTitleColor6, storeScope);
                model.BannerSubTitleColor6_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.BannerSubTitleColor6, storeScope);

                model.NewBannerPicture5Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.NewBannerPicture5Id, storeScope);
                model.NewBannerText5_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.NewBannerText5, storeScope);
                model.NewBannerLink5_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.NewBannerLink5, storeScope);
                model.NewBannerPicture6Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.NewBannerPicture6Id, storeScope);
                model.NewBannerText6_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.NewBannerText6, storeScope);
                model.NewBannerLink6_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.NewBannerLink6, storeScope);

                model.Picture1Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture1Id, storeScope);
                model.Text1_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Text1, storeScope);
                model.Link1_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link1, storeScope);
                model.Picture2Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture2Id, storeScope);
                model.Text2_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Text2, storeScope);
                model.Link2_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link2, storeScope);
                model.Picture3Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture3Id, storeScope);
                model.Text3_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Text3, storeScope);
                model.Link3_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link3, storeScope);
                model.Picture4Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture4Id, storeScope);
                model.Text4_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Text4, storeScope);
                model.Link4_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link4, storeScope);
                model.Picture5Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture5Id, storeScope);
                model.Text5_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Text5, storeScope);
                model.Link5_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link5, storeScope);
                model.Picture6Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture6Id, storeScope);
                model.Text6_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Text6, storeScope);
                model.Link6_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link6, storeScope);
                model.TextColor6_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.TextColor6, storeScope);

                model.TextDropdown1_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.TextDropdown1, storeScope);
                model.LinkDropdown1_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.LinkDropdown1, storeScope);
                model.TextDropdown2_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.TextDropdown2, storeScope);
                model.LinkDropdown2_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.LinkDropdown2, storeScope);
                model.TextDropdown3_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.TextDropdown3, storeScope);
                model.LinkDropdown3_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.LinkDropdown3, storeScope);
                model.TextDropdown4_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.TextDropdown4, storeScope);
                model.LinkDropdown4_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.LinkDropdown4, storeScope);
                model.TextDropdown5_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.TextDropdown5, storeScope);
                model.LinkDropdown5_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.LinkDropdown5, storeScope);
                model.TextDropdown6_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.TextDropdown6, storeScope);
                model.LinkDropdown6_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.LinkDropdown6, storeScope);
                model.TextDropdown7_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.TextDropdown7, storeScope);
                model.LinkDropdown7_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.LinkDropdown7, storeScope);
                model.TextDropdown8_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.TextDropdown8, storeScope);
                model.LinkDropdown8_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.LinkDropdown8, storeScope);
                model.TextDropdown9_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.TextDropdown9, storeScope);
                model.LinkDropdown9_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.LinkDropdown9, storeScope);
                model.TextDropdown10_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.TextDropdown10, storeScope);
                model.LinkDropdown10_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.LinkDropdown10, storeScope);

                model.TextCarousel_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.TextCarousel, storeScope);
                model.TextColorCarousel_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.TextColorCarousel, storeScope);

                model.Picture7Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture7Id, storeScope);
                model.Link7_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link7, storeScope);
                model.Picture8Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture8Id, storeScope);
                model.Link8_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link8, storeScope);
                model.Picture9Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture9Id, storeScope);
                model.Link9_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link9, storeScope);
                model.Picture10Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture10Id, storeScope);
                model.Link10_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link10, storeScope);
                model.Picture11Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture11Id, storeScope);
                model.Link11_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link11, storeScope);
                model.Picture12Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture12Id, storeScope);
                model.Link12_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link12, storeScope);
                model.Picture13Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture13Id, storeScope);
                model.Link13_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link13, storeScope);
                model.Picture14Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture14Id, storeScope);
                model.Link14_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link14, storeScope);

                model.TagsQty_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.TagsQty, storeScope);
                model.TagsEnable_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.TagsEnable, storeScope);

                model.Picture15Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture15Id, storeScope);
                model.Link15_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link15, storeScope);
                model.Picture16Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture16Id, storeScope);
                model.Link16_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link16, storeScope);
                model.Picture17Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture17Id, storeScope);
                model.Link17_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link17, storeScope);
                model.Picture18Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture18Id, storeScope);
                model.Link18_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link18, storeScope);

                model.TitleManufacturer_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.TitleManufacturer, storeScope);
                model.ManufacturerEnable_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.ManufacturerEnable, storeScope);

                model.CollageEnable_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.CollageEnable, storeScope);

                model.Picture19Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture19Id, storeScope);
                model.Link19_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link19, storeScope);
                model.Picture20Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture20Id, storeScope);
                model.Link20_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link20, storeScope);
                model.Picture21Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture21Id, storeScope);
                model.Link21_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link21, storeScope);
                model.Picture22Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture22Id, storeScope);
                model.Link22_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link22, storeScope);
                model.Picture23Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture23Id, storeScope);
                model.Link23_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link23, storeScope);
                model.Picture24Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture24Id, storeScope);
                model.Link24_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link24, storeScope);
                model.Picture25Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture25Id, storeScope);
                model.Link25_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link25, storeScope);
                model.Picture26Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture26Id, storeScope);
                model.Link26_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link26, storeScope);
                model.Picture27Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture27Id, storeScope);
                model.Link27_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link27, storeScope);
                model.Picture28Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture28Id, storeScope);
                model.Link28_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link28, storeScope);
                model.Picture29Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture29Id, storeScope);
                model.Link29_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link29, storeScope);
                model.Picture30Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture30Id, storeScope);
                model.Link30_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link30, storeScope);
                model.Picture31Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture31Id, storeScope);
                model.Link31_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link31, storeScope);
                model.Picture32Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture32Id, storeScope);
                model.Link32_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link32, storeScope);
                model.Picture33Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture33Id, storeScope);
                model.Link33_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link33, storeScope);
                model.Picture34Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture34Id, storeScope);
                model.Link34_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link34, storeScope);
                model.Picture35Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture35Id, storeScope);
                model.Link35_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link35, storeScope);
                model.Picture36Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture36Id, storeScope);
                model.Link36_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link36, storeScope);
                model.Picture37Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture37Id, storeScope);
                model.Link37_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link37, storeScope);
                model.Picture38Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture38Id, storeScope);
                model.Link38_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Link38, storeScope);

                model.PopUpEnable_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.PopUpEnable, storeScope);
                model.Picture39Id_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture39Id, storeScope);
                model.Picture39ResponsiveId_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.Picture39ResponsiveId, storeScope);

                model.TitleVendor_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.TitleVendor, storeScope);
                model.VendorEnable_OverrideForStore = _settingService.SettingExists(homePageSettings, x => x.VendorEnable, storeScope);
            }

            return View("~/Plugins/Widgets.HomePageImages/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var homePageSettings = _settingService.LoadSetting<HomePageImagesSettings>(storeScope);

            //get previous picture identifiers
            List<int> previousPictureIds = new List<int>
            {
                homePageSettings.BannerPicture1Id,
                homePageSettings.BannerPicture2Id,
                homePageSettings.BannerPicture3Id,
                homePageSettings.BannerPicture4Id,
                homePageSettings.BannerPicture5Id,
                homePageSettings.BannerPicture6Id,
                homePageSettings.NewBannerPicture5Id,
                homePageSettings.NewBannerPicture6Id,

                homePageSettings.BannerPictureArrowId,

                homePageSettings.Picture1Id,
                homePageSettings.Picture2Id,
                homePageSettings.Picture3Id,
                homePageSettings.Picture4Id,
                homePageSettings.Picture5Id,
                homePageSettings.Picture6Id,
                homePageSettings.Picture7Id,
                homePageSettings.Picture8Id,
                homePageSettings.Picture9Id,
                homePageSettings.Picture10Id,
                homePageSettings.Picture11Id,
                homePageSettings.Picture12Id,
                homePageSettings.Picture13Id,
                homePageSettings.Picture14Id,
                homePageSettings.Picture15Id,
                homePageSettings.Picture16Id,
                homePageSettings.Picture17Id,
                homePageSettings.Picture18Id,

                homePageSettings.Picture19Id,
                homePageSettings.Picture20Id,
                homePageSettings.Picture21Id,
                homePageSettings.Picture22Id,
                homePageSettings.Picture23Id,
                homePageSettings.Picture24Id,
                homePageSettings.Picture25Id,
                homePageSettings.Picture26Id,
                homePageSettings.Picture27Id,
                homePageSettings.Picture28Id,
                homePageSettings.Picture29Id,
                homePageSettings.Picture30Id,
                homePageSettings.Picture31Id,
                homePageSettings.Picture32Id,
                homePageSettings.Picture33Id,
                homePageSettings.Picture34Id,
                homePageSettings.Picture35Id,
                homePageSettings.Picture36Id,
                homePageSettings.Picture37Id,
                homePageSettings.Picture38Id,
                homePageSettings.Picture39Id,
                homePageSettings.Picture39ResponsiveId
            };

            homePageSettings.BannerPicture1Id = model.BannerPicture1Id;
            homePageSettings.BannerText1 = model.BannerText1;
            homePageSettings.BannerLink1 = model.BannerLink1;
            homePageSettings.BannerPicture2Id = model.BannerPicture2Id;
            homePageSettings.BannerText2 = model.BannerText2;
            homePageSettings.BannerLink2 = model.BannerLink2;
            homePageSettings.BannerPicture3Id = model.BannerPicture3Id;
            homePageSettings.BannerText3 = model.BannerText3;
            homePageSettings.BannerLink3 = model.BannerLink3;
            homePageSettings.BannerPicture4Id = model.BannerPicture4Id;
            homePageSettings.BannerText4 = model.BannerText4;
            homePageSettings.BannerLink4 = model.BannerLink4;

            homePageSettings.NewBannerPicture5Id = model.NewBannerPicture5Id;
            homePageSettings.NewBannerText5 = model.NewBannerText5;
            homePageSettings.NewBannerLink5 = model.NewBannerLink5;
            homePageSettings.NewBannerPicture6Id = model.NewBannerPicture6Id;
            homePageSettings.NewBannerText6 = model.NewBannerText6;
            homePageSettings.NewBannerLink6 = model.NewBannerLink6;

            homePageSettings.BannerPictureArrowId = model.BannerPictureArrowId;

            homePageSettings.VideoId = model.VideoId;
            homePageSettings.BannerPicture5Id = model.BannerPicture5Id;
            homePageSettings.BannerTitle5 = model.BannerTitle5;
            homePageSettings.BannerSubTitle5 = model.BannerSubTitle5;
            homePageSettings.BannerLink5 = model.BannerLink5;
            homePageSettings.BannerTitleColor5 = model.BannerTitleColor5;
            homePageSettings.BannerSubTitleColor5 = model.BannerSubTitleColor5;
            homePageSettings.BannerPicture6Id = model.BannerPicture6Id;
            homePageSettings.BannerTitle6 = model.BannerTitle6;
            homePageSettings.BannerSubTitle6 = model.BannerSubTitle6;
            homePageSettings.BannerLink6 = model.BannerLink6;
            homePageSettings.BannerTitleColor6 = model.BannerTitleColor6;
            homePageSettings.BannerSubTitleColor6 = model.BannerSubTitleColor6;

            homePageSettings.Picture1Id = model.Picture1Id;
            homePageSettings.Text1 = model.Text1;
            homePageSettings.Link1 = model.Link1;
            homePageSettings.Picture2Id = model.Picture2Id;
            homePageSettings.Text2 = model.Text2;
            homePageSettings.Link2 = model.Link2;
            homePageSettings.Picture3Id = model.Picture3Id;
            homePageSettings.Text3 = model.Text3;
            homePageSettings.Link3 = model.Link3;
            homePageSettings.Picture4Id = model.Picture4Id;
            homePageSettings.Text4 = model.Text4;
            homePageSettings.Link4 = model.Link4;
            homePageSettings.Picture5Id = model.Picture5Id;
            homePageSettings.Text5 = model.Text5;
            homePageSettings.Link5 = model.Link5;
            homePageSettings.Picture6Id = model.Picture6Id;
            homePageSettings.Text6 = model.Text6;
            homePageSettings.Link6 = model.Link6;
            homePageSettings.TextColor6 = model.TextColor6;

            homePageSettings.TextDropdown1 = model.TextDropdown1;
            homePageSettings.LinkDropdown1 = model.LinkDropdown1;
            homePageSettings.TextDropdown2 = model.TextDropdown2;
            homePageSettings.LinkDropdown2 = model.LinkDropdown2;
            homePageSettings.TextDropdown3 = model.TextDropdown3;
            homePageSettings.LinkDropdown3 = model.LinkDropdown3;
            homePageSettings.TextDropdown4 = model.TextDropdown4;
            homePageSettings.LinkDropdown4 = model.LinkDropdown4;
            homePageSettings.TextDropdown5 = model.TextDropdown5;
            homePageSettings.LinkDropdown5 = model.LinkDropdown5;
            homePageSettings.TextDropdown6 = model.TextDropdown6;
            homePageSettings.LinkDropdown6 = model.LinkDropdown6;
            homePageSettings.TextDropdown7 = model.TextDropdown7;
            homePageSettings.LinkDropdown7 = model.LinkDropdown7;
            homePageSettings.TextDropdown8 = model.TextDropdown8;
            homePageSettings.LinkDropdown8 = model.LinkDropdown8;
            homePageSettings.TextDropdown9 = model.TextDropdown9;
            homePageSettings.LinkDropdown9 = model.LinkDropdown9;
            homePageSettings.TextDropdown10 = model.TextDropdown10;
            homePageSettings.LinkDropdown10 = model.LinkDropdown10;

            homePageSettings.TextCarousel = model.TextCarousel;
            homePageSettings.TextColorCarousel = model.TextColorCarousel;

            homePageSettings.Picture7Id = model.Picture7Id;
            homePageSettings.Link7 = model.Link7;
            homePageSettings.Picture8Id = model.Picture8Id;
            homePageSettings.Link8 = model.Link8;
            homePageSettings.Picture9Id = model.Picture9Id;
            homePageSettings.Link9 = model.Link9;
            homePageSettings.Picture10Id = model.Picture10Id;
            homePageSettings.Link10 = model.Link10;
            homePageSettings.Picture11Id = model.Picture11Id;
            homePageSettings.Link11 = model.Link11;
            homePageSettings.Picture12Id = model.Picture12Id;
            homePageSettings.Link12 = model.Link12;
            homePageSettings.Picture13Id = model.Picture13Id;
            homePageSettings.Link13 = model.Link13;
            homePageSettings.Picture14Id = model.Picture14Id;
            homePageSettings.Link14 = model.Link14;

            homePageSettings.TagsQty = model.TagsQty;
            homePageSettings.TagsEnable = model.TagsEnable;

            homePageSettings.Picture15Id = model.Picture15Id;
            homePageSettings.Link15 = model.Link15;
            homePageSettings.Picture16Id = model.Picture16Id;
            homePageSettings.Link16 = model.Link16;
            homePageSettings.Picture17Id = model.Picture17Id;
            homePageSettings.Link17 = model.Link17;
            homePageSettings.Picture18Id = model.Picture18Id;
            homePageSettings.Link18 = model.Link18;

            homePageSettings.TitleManufacturer = model.TitleManufacturer;
            homePageSettings.ManufacturerEnable = model.ManufacturerEnable;

            homePageSettings.CollageEnable = model.CollageEnable;

            homePageSettings.Picture19Id = model.Picture19Id;
            homePageSettings.Link19 = model.Link19;
            homePageSettings.Picture20Id = model.Picture20Id;
            homePageSettings.Link20 = model.Link20;
            homePageSettings.Picture21Id = model.Picture21Id;
            homePageSettings.Link21 = model.Link21;
            homePageSettings.Picture22Id = model.Picture22Id;
            homePageSettings.Link22 = model.Link22;
            homePageSettings.Picture23Id = model.Picture23Id;
            homePageSettings.Link23 = model.Link23;
            homePageSettings.Picture24Id = model.Picture24Id;
            homePageSettings.Link24 = model.Link24;
            homePageSettings.Picture25Id = model.Picture25Id;
            homePageSettings.Link25 = model.Link25;
            homePageSettings.Picture26Id = model.Picture26Id;
            homePageSettings.Link26 = model.Link26;
            homePageSettings.Picture27Id = model.Picture27Id;
            homePageSettings.Link27 = model.Link27;
            homePageSettings.Picture28Id = model.Picture28Id;
            homePageSettings.Link28 = model.Link28;
            homePageSettings.Picture29Id = model.Picture29Id;
            homePageSettings.Link29 = model.Link29;
            homePageSettings.Picture30Id = model.Picture30Id;
            homePageSettings.Link30 = model.Link30;
            homePageSettings.Picture31Id = model.Picture31Id;
            homePageSettings.Link31 = model.Link31;
            homePageSettings.Picture32Id = model.Picture32Id;
            homePageSettings.Link32 = model.Link32;
            homePageSettings.Picture33Id = model.Picture33Id;
            homePageSettings.Link33 = model.Link33;
            homePageSettings.Picture34Id = model.Picture34Id;
            homePageSettings.Link34 = model.Link34;
            homePageSettings.Picture35Id = model.Picture35Id;
            homePageSettings.Link35 = model.Link35;
            homePageSettings.Picture36Id = model.Picture36Id;
            homePageSettings.Link36 = model.Link36;
            homePageSettings.Picture37Id = model.Picture37Id;
            homePageSettings.Link37 = model.Link37;
            homePageSettings.Picture38Id = model.Picture38Id;
            homePageSettings.Link38 = model.Link38;

            homePageSettings.PopUpEnable = model.PopUpEnable;
            homePageSettings.Picture39Id = model.Picture39Id;
            homePageSettings.Picture39ResponsiveId = model.Picture39ResponsiveId;

            homePageSettings.TitleVendor = model.TitleVendor;
            homePageSettings.VendorEnable = model.VendorEnable;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerPicture1Id, model.BannerPicture1Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerText1, model.BannerText1_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerLink1, model.BannerLink1_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerPicture2Id, model.BannerPicture2Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerText2, model.BannerText2_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerLink2, model.BannerLink2_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerPicture3Id, model.BannerPicture3Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerText3, model.BannerText3_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerLink3, model.BannerLink3_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerPicture4Id, model.BannerPicture4Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerText4, model.BannerText4_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerLink4, model.BannerLink4_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.NewBannerPicture5Id, model.NewBannerPicture5Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.NewBannerText5, model.NewBannerText5_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.NewBannerLink5, model.NewBannerLink5_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.NewBannerPicture6Id, model.NewBannerPicture6Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.NewBannerText6, model.NewBannerText6_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.NewBannerLink6, model.NewBannerLink6_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerPictureArrowId, model.BannerPictureArrowId_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.VideoId, model.VideoId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerPicture5Id, model.BannerPicture5Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerTitle5, model.BannerTitle5_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerSubTitle5, model.BannerSubTitle5_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerLink5, model.BannerLink5_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerTitleColor5, model.BannerTitleColor5_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerSubTitleColor5, model.BannerSubTitleColor5_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerPicture6Id, model.BannerPicture6Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerTitle6, model.BannerTitle6_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerSubTitle6, model.BannerSubTitle6_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerLink6, model.BannerLink6_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerTitleColor6, model.BannerTitleColor6_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.BannerSubTitleColor6, model.BannerSubTitleColor6_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture1Id, model.Picture1Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Text1, model.Text1_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link1, model.Link1_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture2Id, model.Picture2Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Text2, model.Text2_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link2, model.Link2_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture3Id, model.Picture3Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Text3, model.Text3_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link3, model.Link3_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture4Id, model.Picture4Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Text4, model.Text4_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link4, model.Link4_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture5Id, model.Picture5Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Text5, model.Text5_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link5, model.Link5_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture6Id, model.Picture6Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Text6, model.Text6_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link6, model.Link6_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.TextColor6, model.TextColor6_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.TextDropdown1, model.TextDropdown1_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.LinkDropdown1, model.LinkDropdown1_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.TextDropdown2, model.TextDropdown2_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.LinkDropdown2, model.LinkDropdown2_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.TextDropdown3, model.TextDropdown3_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.LinkDropdown3, model.LinkDropdown3_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.TextDropdown4, model.TextDropdown4_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.LinkDropdown4, model.LinkDropdown4_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.TextDropdown5, model.TextDropdown5_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.LinkDropdown5, model.LinkDropdown5_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.TextDropdown6, model.TextDropdown6_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.LinkDropdown6, model.LinkDropdown6_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.TextDropdown7, model.TextDropdown7_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.LinkDropdown7, model.LinkDropdown7_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.TextDropdown8, model.TextDropdown8_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.LinkDropdown8, model.LinkDropdown8_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.TextDropdown9, model.TextDropdown9_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.LinkDropdown9, model.LinkDropdown9_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.TextDropdown10, model.TextDropdown10_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.LinkDropdown10, model.LinkDropdown10_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.TextCarousel, model.TextCarousel_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.TextColorCarousel, model.TextColorCarousel_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture7Id, model.Picture7Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link7, model.Link7_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture8Id, model.Picture8Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link8, model.Link8_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture9Id, model.Picture9Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link9, model.Link9_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture10Id, model.Picture10Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link10, model.Link10_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture11Id, model.Picture11Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link11, model.Link11_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture12Id, model.Picture12Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link12, model.Link12_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture13Id, model.Picture13Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link13, model.Link13_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture14Id, model.Picture14Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link14, model.Link14_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.TagsQty, model.TagsQty_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.TagsEnable, model.TagsEnable_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture15Id, model.Picture15Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link15, model.Link15_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture16Id, model.Picture16Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link16, model.Link16_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture17Id, model.Picture17Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link17, model.Link17_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture18Id, model.Picture18Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link18, model.Link18_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.TitleManufacturer, model.TitleManufacturer_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.ManufacturerEnable, model.ManufacturerEnable_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.CollageEnable, model.CollageEnable_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture19Id, model.Picture19Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link19, model.Link19_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture20Id, model.Picture20Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link20, model.Link20_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture21Id, model.Picture21Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link21, model.Link21_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture22Id, model.Picture22Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link22, model.Link22_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture23Id, model.Picture23Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link23, model.Link23_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture24Id, model.Picture24Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link24, model.Link24_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture25Id, model.Picture25Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link25, model.Link25_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture26Id, model.Picture26Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link26, model.Link26_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture27Id, model.Picture27Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link27, model.Link27_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture28Id, model.Picture28Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link28, model.Link28_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture29Id, model.Picture29Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link29, model.Link29_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture30Id, model.Picture30Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link30, model.Link30_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture31Id, model.Picture31Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link31, model.Link31_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture32Id, model.Picture32Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link32, model.Link32_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture33Id, model.Picture33Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link33, model.Link33_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture34Id, model.Picture34Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link34, model.Link34_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture35Id, model.Picture35Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link35, model.Link35_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture36Id, model.Picture36Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link36, model.Link36_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture37Id, model.Picture37Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link37, model.Link37_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture38Id, model.Picture38Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Link38, model.Link38_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.PopUpEnable, model.PopUpEnable_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture39Id, model.Picture39Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.Picture39ResponsiveId, model.Picture39ResponsiveId_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.TitleVendor, model.TitleVendor_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(homePageSettings, x => x.VendorEnable, model.VendorEnable_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            //get current picture identifiers
            List<int> currentPictureIds = new List<int>
            {
                homePageSettings.BannerPicture1Id,
                homePageSettings.BannerPicture2Id,
                homePageSettings.BannerPicture3Id,
                homePageSettings.BannerPicture4Id,
                homePageSettings.BannerPicture5Id,
                homePageSettings.BannerPicture6Id,
                homePageSettings.NewBannerPicture5Id,
                homePageSettings.NewBannerPicture6Id,
                homePageSettings.BannerPictureArrowId,

                homePageSettings.Picture1Id,
                homePageSettings.Picture2Id,
                homePageSettings.Picture3Id,
                homePageSettings.Picture4Id,
                homePageSettings.Picture5Id,
                homePageSettings.Picture6Id,
                homePageSettings.Picture7Id,
                homePageSettings.Picture8Id,
                homePageSettings.Picture9Id,
                homePageSettings.Picture10Id,
                homePageSettings.Picture11Id,
                homePageSettings.Picture12Id,
                homePageSettings.Picture13Id,
                homePageSettings.Picture14Id,
                homePageSettings.Picture15Id,
                homePageSettings.Picture16Id,
                homePageSettings.Picture17Id,
                homePageSettings.Picture18Id,

                homePageSettings.Picture19Id,
                homePageSettings.Picture20Id,
                homePageSettings.Picture21Id,
                homePageSettings.Picture22Id,
                homePageSettings.Picture23Id,
                homePageSettings.Picture24Id,
                homePageSettings.Picture25Id,
                homePageSettings.Picture26Id,
                homePageSettings.Picture27Id,
                homePageSettings.Picture28Id,
                homePageSettings.Picture29Id,
                homePageSettings.Picture30Id,
                homePageSettings.Picture31Id,
                homePageSettings.Picture32Id,
                homePageSettings.Picture33Id,
                homePageSettings.Picture34Id,
                homePageSettings.Picture35Id,
                homePageSettings.Picture36Id,
                homePageSettings.Picture37Id,
                homePageSettings.Picture38Id,
                homePageSettings.Picture39Id,
                homePageSettings.Picture39ResponsiveId
            };

            //delete an old picture (if deleted or updated)
            foreach (var pictureId in previousPictureIds.Except(currentPictureIds))
            {
                var previousPicture = _pictureService.GetPictureById(pictureId);
                if (previousPicture != null)
                    _pictureService.DeletePicture(previousPicture);
            }

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return Configure();
        }

        [AllowAnonymous]
        public IActionResult AllCategories()
        {
            var cat = _categoryService.GetAllCategories();
            List<object> categories = new List<object>();

            foreach (var item in cat)
            {
                var obj = new
                {
                    name = item.Name,
                    seName = item.GetSeName()
                };
                categories.Add(obj);
            }

            return Ok(categories.ToArray());
        }

        [AllowAnonymous]
        public IActionResult AllTags()
        {
            var tags = _productTagService.GetAllProductTags()
                //order by product count
                .OrderByDescending(x => _productTagService.GetProductCount(x.Id, 0))
                .Select(x => new ProductTagModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    ProductCount = _productTagService.GetProductCount(x.Id, 0)
                })
                .ToList();

            return Ok(tags);
        }

        //[AllowAnonymous]
        //public IActionResult GetDataManufacturer()
        //{
        //    var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
        //    var homePageSettings = _settingService.LoadSetting<HomePageImagesSettings>(storeScope);

        //    var data = new
        //    {
        //        enable = homePageSettings.ManufacturerEnable,
        //        name = homePageSettings.TitleManufacturer,
        //        link = homePageSettings.LinkManufacturer
        //    };

        //    return Ok(data);
        //}

        //[AllowAnonymous]
        //public IActionResult GetDataVendor()
        //{
        //    var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
        //    var homePageSettings = _settingService.LoadSetting<HomePageImagesSettings>(storeScope);

        //    var data = new
        //    {
        //        enable = homePageSettings.VendorEnable,
        //        name = homePageSettings.TitleVendor,
        //        link = homePageSettings.LinkVendor
        //    };

        //    return Ok(data);
        //}

        /*
        [HttpPost]
        public IActionResult UploadImages()
        {
            List<IFormFile> file = Request.Form.Files.ToList();
            List<ImgCollageModel> imgs = new List<ImgCollageModel>();

            foreach (var httpPostedFile in file)
            {
                if (httpPostedFile == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No file uploaded",
                        downloadGuid = Guid.Empty,
                    });
                }

                var fileBinary = httpPostedFile.GetDownloadBits();
                var fileName = httpPostedFile.FileName;

                fileName = Path.GetFileName(fileName);

                var contentType = httpPostedFile.ContentType;

                var fileExtension = Path.GetExtension(fileName);
                if (!string.IsNullOrEmpty(fileExtension))
                    fileExtension = fileExtension.ToLowerInvariant();

                //contentType is not always available 
                //that's why we manually update it here
                //http://www.sfsu.edu/training/mimetype.htm
                if (string.IsNullOrEmpty(contentType))
                {
                    switch (fileExtension)
                    {
                        case ".bmp":
                            contentType = MimeTypes.ImageBmp;
                            break;
                        case ".gif":
                            contentType = MimeTypes.ImageGif;
                            break;
                        case ".jpeg":
                        case ".jpg":
                        case ".jpe":
                        case ".jfif":
                        case ".pjpeg":
                        case ".pjp":
                            contentType = MimeTypes.ImageJpeg;
                            break;
                        case ".png":
                            contentType = MimeTypes.ImagePng;
                            break;
                        case ".tiff":
                        case ".tif":
                            contentType = MimeTypes.ImageTiff;
                            break;
                        default:
                            break;
                    }
                }

                var picture = _pictureService.InsertPicture(fileBinary, contentType, null);

                imgs.Add(new ImgCollageModel
                {
                    PictureId = picture.Id,
                    UrlPicture = _pictureService.GetPictureUrl(picture, 100)
                });
            }

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            var jImgs = Newtonsoft.Json.JsonConvert.SerializeObject(imgs);
            return Ok(jImgs);
        }
        */
    }
}