using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Dtos
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public decimal OldPrice { get; set; }

        public string PictureUrl { get; set; }

        public decimal WeightInterval { get; set; }

        public decimal EquivalenceCoefficient { get; set; }

        public int CurrentCartQuantity { get; set; }

        public int CartItemId { get; set; }

        public bool BuyingBySecondary { get; set; }

        public string SelectedPropertyOption { get; set; }

        public string[] PropertyOptions { get; set; }

        public string Discount { get; set; }

        public string Sku { get; set; }

        public decimal SubTotal { get; set; }

        public decimal UnitPrice { get; set; }

        public IList<string> Warnings { get; set; }
    }
}
