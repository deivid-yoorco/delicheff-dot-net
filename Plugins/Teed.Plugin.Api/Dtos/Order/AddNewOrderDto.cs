using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Customer;
using Teed.Plugin.Api.Dtos.Products;

namespace Teed.Plugin.Api.Dtos.Order
{
    public class AddNewOrderDto
    {
        public string CustomerId { get; set; }

        public decimal OrderSubtotal { get; set; }

        public decimal OrderTotal { get; set; }

        public decimal OrderShipping { get; set; }

        public List<LegacyProductDto> OrderProducts { get; set; }

        public string ShippingRateComputationMethodSystemName { get; set; }

        public CustomerShippingAddressDto ShippingAddress { get; set; }
    }
}
