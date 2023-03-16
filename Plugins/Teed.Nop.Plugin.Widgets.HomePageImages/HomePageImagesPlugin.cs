using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Media;
using System.Collections.Generic;
using System.IO;

namespace Teed.Nop.Plugin.Widgets.NivoSlider2
{
    /// <summary>
    /// PLugin
    /// </summary>
    public class HomePageImagesPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;

        public HomePageImagesPlugin(IPictureService pictureService,
            ISettingService settingService, IWebHelper webHelper)
        {
            this._pictureService = pictureService;
            this._settingService = settingService;
            this._webHelper = webHelper;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/HomePageImages/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //pictures
            var sampleImagesPath = CommonHelper.MapPath("~/Plugins/Widgets.HomePageImages/Content/sample-images/");

            //settings
            var settings = new HomePageImagesSettings
            {
                BannerPicture1Id = _pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "banner1.jpg"), MimeTypes.ImagePJpeg, "banner_1").Id,
                BannerText1 = "",
                BannerLink1 = _webHelper.GetStoreLocation(false),
                BannerPicture2Id = _pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "banner2.jpg"), MimeTypes.ImagePJpeg, "banner_2").Id,
                BannerText2 = "",
                BannerLink2 = _webHelper.GetStoreLocation(false)
            };
            _settingService.SaveSetting(settings);

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<HomePageImagesSettings>();

            base.Uninstall();
        }

        void IWidgetPlugin.GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            viewComponentName = "NonExistant";
        }

        IList<string> IWidgetPlugin.GetWidgetZones()
        {
            return new List<string> { "NonExistant" };
        }
    }
}