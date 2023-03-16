using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Customer;

namespace Teed.Plugin.Api.Dtos.Products
{
    public class CalculateShippingDto
    {
        public string CustomerId { get; set; }

        public List<LegacyProductDto> Items { get; set; }

        public CustomerShippingAddressDto ShippingAddress { get; set; }
    }
}