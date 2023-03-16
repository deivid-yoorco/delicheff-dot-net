using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Products;

namespace Teed.Plugin.Api.Dtos.Home
{
    public class GetHomeProductsDto
    {
        public string HeaderText { get; set; }

        public List<ProductDto> Products { get; set; }
    }
}
