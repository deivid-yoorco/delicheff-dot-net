using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Mvc.Models;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    public partial class ProductOverviewModel : BaseNopEntityModel
    {
        public ProductOverviewModel()
        {
            ProductPrice = new ProductPriceModel();
            DefaultPictureModel = new PictureModel();
            SpecificationAttributeModels = new List<ProductSpecificationModel>();
            ReviewOverviewModel = new ProductReviewOverviewModel();
            AttributeProduct = new List<ProductAttributeValue>();
            PictureModels = new List<PictureModel>();
            VisibleDiscount = new VisibleDiscountModel();
        }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string SeName { get; set; }

        public string Sku { get; set; }

        public ProductType ProductType { get; set; }

        public bool MarkAsNew { get; set; }

        public int Quantity { get; set; }

        public bool StockAvailability { get; set; }

        public int CurrentQuantityInCart { get; set; }

        public string SelectedPropertyOption { get; set; }

        public bool BuyingBySecondary { get; set; }

        public decimal WeightInterval { get; set; }

        public string ListName { get; set; }

        public int ListPosition { get; set; }

        public bool IsInWishList { get; set; }

        public Product Product { get; set; }

        public List<string> PropertiesOptions { get; set; }

        //price
        public ProductPriceModel ProductPrice { get; set; }
        //picture
        public PictureModel DefaultPictureModel { get; set; }
        //specification attributes
        public IList<ProductSpecificationModel> SpecificationAttributeModels { get; set; }
        //price
        public ProductReviewOverviewModel ReviewOverviewModel { get; set; }

        public List<ProductAttributeValue> AttributeProduct { get; set; }

        public List<PictureModel> PictureModels { get; set; }

        public VisibleDiscountModel VisibleDiscount { get;set;}

        #region Nested Classes

        public partial class ProductPriceModel : BaseNopModel
        {
            public string OldPrice { get; set; }
            public string Price { get; set; }
            public decimal PriceValue { get; set; }
            public decimal PriceWithDiscountValue { get; set; }
            public string PriceWithDiscount { get; set; }
            /// <summary>
            /// PAngV baseprice (used in Germany)
            /// </summary>
            public string BasePricePAngV { get; set; }

            public bool DisableBuyButton { get; set; }
            public bool DisableWishlistButton { get; set; }
            public bool DisableAddToCompareListButton { get; set; }

            public bool AvailableForPreOrder { get; set; }
            public DateTime? PreOrderAvailabilityStartDateTimeUtc { get; set; }

            public bool IsRental { get; set; }

            public bool ForceRedirectionAfterAddingToCart { get; set; }

            public decimal EquivalenceCoefficient { get; set; }

            /// <summary>
            /// A value indicating whether we should display tax/shipping info (used in Germany)
            /// </summary>
            public bool DisplayTaxShippingInfo { get; set; }
        }

        public partial class VisibleDiscountModel : BaseNopModel
        {
            public int Id { get; set; }
            public decimal DiscountAmount { get; set; }
            public string CouponCode { get; set; }
        }

        #endregion
    }
}