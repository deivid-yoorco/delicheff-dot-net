using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            SelectedProductsIds = new List<int>();
            SelectedCategoriesIds = new List<int>();
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        public List<SelectListItem> ActionTypes { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int BannerPicture1Id { get; set; }
        public bool BannerPicture1Id_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Action")]
        public int BannerPicture1TypeId { get; set; }
        public bool BannerPicture1TypeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.AdditionalData")]
        public string BannerPicture1AdditionalData { get; set; }
        public bool BannerPicture1AdditionalData_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int BannerPicture2Id { get; set; }
        public bool BannerPicture2Id_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Action")]
        public int BannerPicture2TypeId { get; set; }
        public bool BannerPicture2TypeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.AdditionalData")]
        public string BannerPicture2AdditionalData { get; set; }
        public bool BannerPicture2AdditionalData_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int BannerPicture3Id { get; set; }
        public bool BannerPicture3Id_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Action")]
        public int BannerPicture3TypeId { get; set; }
        public bool BannerPicture3TypeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.AdditionalData")]
        public string BannerPicture3AdditionalData { get; set; }
        public bool BannerPicture3AdditionalData_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int BannerPicture4Id { get; set; }
        public bool BannerPicture4Id_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Action")]
        public int BannerPicture4TypeId { get; set; }
        public bool BannerPicture4TypeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.AdditionalData")]
        public string BannerPicture4AdditionalData { get; set; }
        public bool BannerPicture4AdditionalData_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int BannerPicture5Id { get; set; }
        public bool BannerPicture5Id_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Action")]
        public int BannerPicture5TypeId { get; set; }
        public bool BannerPicture5TypeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.AdditionalData")]
        public string BannerPicture5AdditionalData { get; set; }
        public bool BannerPicture5AdditionalData_OverrideForStore { get; set; }

        public string FacebookAppSecret { get; set; }

        public string FacebookAppId { get; set; }

        public string ProductsHeader { get; set; }

        public IList<int> SelectedProductsIds { get; set; }

        public int CategoryId { get; set; }

        // Notifications
        public string AbandonedShoppingCartTitle { get; set; }
        public string AbandonedShoppingCartBody { get; set; }

        public string OrderPlacedTitle { get; set; }
        public string OrderPlacedBody { get; set; }

        public string OrderPaidTitle { get; set; }
        public string OrderPaidBody { get; set; }

        public string OrderCompletedTitle { get; set; }
        public string OrderCompletedBody { get; set; }

        public IFormFile WelcomeImage { get; set; }
        public string WelcomeImageB64 { get; set; }


        public IList<int> SelectedCategoriesIds { get; set; }
        public string CategoriesHeader { get; set; }

        public bool IsActiveCategories { get; set; }

        // Smartlook
        public string SmartlookProjectKey { get; set; }
        public bool SmartlookIsActive { get; set; }

    }
}
