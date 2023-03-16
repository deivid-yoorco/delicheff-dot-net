using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Search
{
    public class SearchDto
    {
        public int Id { get; set; }

        public string ProductName { get; set; }

        public decimal Price { get; set; }

        public string ImageUrl { get; set; }

        public DateTime CreationDate { get; set; }

        public int InitialOrder { get; set; }

        public List<ProductAttribute> ProductAttributes { get; set; }
    }

    public class ProductAttribute
    {
        public int SpecificationAttributeId { get; set; }
        public string SpecificationAttributeName { get; set; }
        public int SpecificationAttributeOptionId { get; set; }
        public string SpecificationAttributeOptionName { get; set; }
    }
}
