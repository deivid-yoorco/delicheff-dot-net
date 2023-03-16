using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.RecentProducts.Dtos.Products
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public decimal Discount { get; set; }

        public decimal ItemDiscount { get; set; }

        public decimal OldPrice { get; set; }

        public string PictureUrl { get; set; }

        public decimal WeightInterval { get; set; }

        public decimal EquivalenceCoefficient { get; set; }

        public int CurrentCartQuantity { get; set; }

        public int CartItemId { get; set; }

        public bool BuyingBySecondary { get; set; }

        public string SelectedPropertyOption { get; set; }

        public string[] PropertyOptions { get; set; }

        public string Sku { get; set; }

        public decimal SubTotal { get; set; }

        public decimal UnitPrice { get; set; }

        public bool IsExtraCartProduct { get; set; }

        public bool IsInWishlist { get; set; }

        public IList<string> Warnings { get; set; }

        public int Stock { get; set; }
    }
}
