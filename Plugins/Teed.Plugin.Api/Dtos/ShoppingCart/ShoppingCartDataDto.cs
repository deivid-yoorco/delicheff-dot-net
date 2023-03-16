using System.Collections.Generic;

namespace Teed.Plugin.Api.Dtos.ShoppingCart
{
    public class ShoppingCartDataDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public bool BuyingBySecondary { get; set; }
        public string SelectedPropertyOption { get; set; }
        public string PictureUrl { get; set; }
    }
}
