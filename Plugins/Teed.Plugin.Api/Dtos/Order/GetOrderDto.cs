using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Products;

namespace Teed.Plugin.Api.Dtos.Order
{
    public class GetOrderDto
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; }

        public decimal OrderTotal { get; set; }

        public string OrderStatus { get; set; }

        public string ShippingStatus { get; set; }

        public string PaymentStatus { get; set; }

        //public ShippingAddress ShippingAddress { get; set; }

        public DateTime OrderTime { get; set; }

        public List<ProductDto> OrderItems { get; set; }
    }
}
