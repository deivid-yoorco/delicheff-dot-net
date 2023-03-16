using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Utils
{
    public static class ProductUtils
    {
        public static int GetProductStock(Product product, 
            string attributes, 
            IProductAttributeParser _productAttributeParser, 
            IProductAttributeService _productAttributeService)
        {
            if (!string.IsNullOrWhiteSpace(attributes))
            {
                int[] attributeIds = Array.ConvertAll(attributes.Split(','), int.Parse);
                string attributesXml = ConvertToXml(attributeIds, product.Id, _productAttributeParser, _productAttributeService);
                ProductAttributeCombination combination = _productAttributeParser.FindProductAttributeCombination(product, attributesXml);

                var value = combination != null ? combination.StockQuantity : product.StockQuantity;
                return value;
            }

            var value2 = product.StockQuantity;
            return value2;
        }

        private static string ConvertToXml(int[] attributeDtos, int productId, IProductAttributeParser _productAttributeParser,
            IProductAttributeService _productAttributeService)
        {
            string attributesXml = "";

            if (attributeDtos == null)
                return attributesXml;

            var productAttributes = _productAttributeService.GetProductAttributeMappingsByProductId(productId);
            foreach (var attribute in productAttributes)
            {
                // there should be only one selected value for this attribute
                var selectedAttribute = attributeDtos.Where(x => x == attribute.ProductAttributeValues.Where(y => x == y.Id).Select(y => y.Id).FirstOrDefault()).FirstOrDefault();
                if (selectedAttribute > 0)
                {
                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                            attribute, selectedAttribute.ToString());
                }
            }

            return attributesXml;
        }
    }
}
