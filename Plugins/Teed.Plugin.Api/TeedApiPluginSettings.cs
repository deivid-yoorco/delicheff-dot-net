using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api
{
    public class TeedApiPluginSettings : ISettings
    {
        public int BannerPicture1Id { get; set; }
        public int BannerPicture1TypeId { get; set; }
        public string BannerPicture1AdditionalData { get; set; }

        public int BannerPicture2Id { get; set; }
        public int BannerPicture2TypeId { get; set; }
        public string BannerPicture2AdditionalData { get; set; }

        public int BannerPicture3Id { get; set; }
        public int BannerPicture3TypeId { get; set; }
        public string BannerPicture3AdditionalData { get; set; }

        public int BannerPicture4Id { get; set; }
        public int BannerPicture4TypeId { get; set; }
        public string BannerPicture4AdditionalData { get; set; }

        public int BannerPicture5Id { get; set; }
        public int BannerPicture5TypeId { get; set; }
        public string BannerPicture5AdditionalData { get; set; }

        public string FacebookAppSecret { get; set; }

        public string FacebookAppId { get; set; }

        public List<int> SelectedProductsIds { get; set; }

        public string ProductsHeader { get; set; }

        public int CategoryId { get; set; }

        public string AbandonedShoppingCartTitle { get; set; }
        public string AbandonedShoppingCartBody { get; set; }

        public string OrderPlacedTitle { get; set; }
        public string OrderPlacedBody { get; set; }

        public string OrderPaidTitle { get; set; }
        public string OrderPaidBody { get; set; }

        public string OrderCompletedTitle { get; set; }
        public string OrderCompletedBody { get; set; }

        public List<int> SelectedCategoriesIds { get; set; }
        public string CategoriesHeader { get; set; }

        public bool IsActiveCategories { get; set; }

        // Smartlook
        public string SmartlookProjectKey { get; set; }
        public bool SmartlookIsActive { get; set; }

    }
}
