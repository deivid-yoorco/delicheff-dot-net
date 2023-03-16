using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Products;

namespace Teed.Plugin.Api.Dtos.Cart
{
    public class ShoppingCartDto
    {
        public int Id { get; set; }

        public string Sku { get; set; }

        public string PictureUrl { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string UnitPrice { get; set; }

        public string SubTotal { get; set; }

        public string Discount { get; set; }

        public int Quantity { get; set; }

        public IList<string> Warnings { get; set; }

        public string Category { get; set; }

        public decimal EquivalenceCoefficient { get; set; }

        public bool BuyingBySecondary { get; set; }

        public string SelectedPropertyOption { get; set; }

        public decimal WeightInterval { get; set; }

        public string CustomerEnteredPrice { get; set; }

        public List<string> ValidWeightQuantities { get; set; }
    }
}
