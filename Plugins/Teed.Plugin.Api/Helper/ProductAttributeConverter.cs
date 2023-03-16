using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Helper
{
    public class ProductAttributeConverter
    {
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;

        public ProductAttributeConverter(IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser)
        {
            _productAttributeService = productAttributeService;
            _productAttributeParser = productAttributeParser;
        }

        public string ConvertToXml(int[] attributeDtos, int productId)
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
