using System.Collections.Generic;

namespace Teed.Plugin.Api.Dtos.ShoppingCart
{
    public class WishlistDataDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSku { get; set; }
        public decimal ProductSubtotal { get; set; }
    }
}
