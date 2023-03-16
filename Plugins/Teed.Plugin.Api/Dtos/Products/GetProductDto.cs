using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Products
{
    public class GetProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ShortDescription { get; set; }

        public decimal OldPrice { get; set; }

        public decimal Price { get; set; }

        public string FullDescription { get; set; }

        public decimal Weight { get; set; }

        public int WarehouseId { get; set; }

        public int ProductStockQuantity { get; set; }

        public List<string> Pictures { get; set; }

        public List<ProductCategory> ProductCategories { get; set; }

        public List<ProductAttribute> ProductAttributes { get; set; }

    }

    public class ProductCategory
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class ProductAttribute
    {
        public int AttributeId { get; set; }

        public string AttributeName { get; set; }

        public List<ProductAttributeValue> ProductAttributeValues { get; set; }
    }

    public class ProductAttributeValue
    {
        public int ProductAttributeValueId { get; set; }

        public string ProductAttributeValueName { get; set; }

        public int StockQuantity { get; set; }
    }
}
