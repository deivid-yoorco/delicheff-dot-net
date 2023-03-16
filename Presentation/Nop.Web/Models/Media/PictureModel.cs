using Nop.Web.Framework.Mvc.Models;

namespace Nop.Web.Models.Media
{
    public partial class PictureModel : BaseNopModel
    {
        public string ImageUrl { get; set; }

        public string ThumbImageUrl { get; set; }

        public string FullSizeImageUrl { get; set; }

        public string Title { get; set; }

        public string AlternateText { get; set; }

        public bool Is360 { get; set; }

        public bool CustomEnable { get; set; }

        public string BoundingX { get; set; }

        public string BoundingY { get; set; }

        public string BoundingWidth { get; set; }

        public string BoundingHeight { get; set; }
    }
}