using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Customer;
using Teed.Plugin.Api.Dtos.Products;

namespace Teed.Plugin.Api.Dtos.Order
{
    public class GetCustomerOrdersDto
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; }

        public decimal OrderTotal { get; set; }

        public string OrderStatus { get; set; }

        public string ShippingStatus { get; set; }

        public bool IsCancelled { get; set; }

        public string PaymentStatus { get; set; }

        public decimal OrderSubtotal { get; set; }

        public decimal OrderShipping { get; set; }

        public UserAddressDto ShippingAddress { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime? SelectedShippingDate { get; set; }

        public string SelectedShippingTime { get; set; }

        public List<ProductDto> OrderItems { get; set; }
    }
}
