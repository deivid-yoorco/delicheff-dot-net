using System.Collections.Generic;

namespace Teed.Plugin.ShoppingCartUrlGenerator.Models
{
    public class ShoppingCartUrlModel
    {
        public ShoppingCartUrlModel()
        {
            SelectedProductsData = new List<CreateEditShoppingCartUrlData>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public string Body { get; set; }
        public IList<CreateEditShoppingCartUrlData> SelectedProductsData { get; set; }
    }

    public class CreateEditShoppingCartUrlModel
    {
        public int Id { get; set; }
        public List<CreateEditShoppingCartUrlData> Products { get; set; }
        public string UrlCode { get; set; }
        public bool IsActive { get; set; }
        public string Body { get; set; }
    }

    public class CreateEditShoppingCartUrlData
    {
        public int ProductId { get; set; }
        public int ProductUnit { get; set; }
        public int ProductQty { get; set; }
        public string ProductProperty { get; set; }
        public string ProductName { get; set; }
        public string ProductGroceryQuantity { get; set; }
    }

    public class ProductData
    {
        public int Id { get; set; }
        public string Product { get; set; }
    }
}