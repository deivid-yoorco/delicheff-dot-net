using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Products
{
    public class ShippingResponseDto
    {
        public string Name { get; set; }

        public decimal Cost { get; set; }

        public string ComputationMethod { get; set; }
    }
}
