using Nop.Web.Framework.Mvc.Models;

namespace Nop.Web.Models.Common
{
    public partial class HeaderLinksModel : BaseNopModel
    {
        public bool IsAuthenticated { get; set; }
        public string CustomerName { get; set; }
        public string CustomerProfilePicture { get; set; }


        public bool ShoppingCartEnabled { get; set; }
        public int ShoppingCartItems { get; set; }
        
        public bool WishlistEnabled { get; set; }
        public int WishlistItems { get; set; }

        public bool CompareProductsEnabled { get; set; }
        public int CompareProducstItems { get; set; }

        public bool AllowPrivateMessages { get; set; }
        public string UnreadPrivateMessages { get; set; }
        public string AlertMessage { get; set; }

        public bool HideExtra { get; set; }
        public bool? ParentIsLogoContainer { get; set; }

        public bool GamificationIsActive { get; set; }
        public decimal CustomerPointsBalance { get; set; }
    }
}