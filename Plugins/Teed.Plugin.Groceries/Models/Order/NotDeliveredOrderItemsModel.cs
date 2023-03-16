using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.Order
{
    public class NotDeliveredOrderItemsModel
    {
        public int OrderId { get; set; }
        public List<Item> Items { get; set; }
    }

    public class Item
    {
        public string PictureThumbnailUrl { get; set; }
        public string Name { get; set; }
        public decimal EquivalenceCoefficient { get; set; }
        public bool BuyingBySecondary { get; set; }
        public decimal WeightInterval { get; set; }
        public string Sku { get; set; }
        public string Price { get; set; }
        public int Quantity { get; set; }
        public string Discount { get; set; }
        public string Total { get; set; }
        public string NotDeliveredReason { get; set; }
        public string SelectedPropertyOption { get; set; }
    }
}
