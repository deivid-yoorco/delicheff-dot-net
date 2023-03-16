using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Checkout
{
    public class PlaceOrderDto
    {
        public string SelectedPaymentMethodSystemName { get; set; }
        public string SelectedShippingDate { get; set; }
        public string SelectedShippingTime { get; set; }
        public int AddressId { get; set; }
        public string PaymentResult { get; set; }
        public List<int> ShoppingCartItemIds { get; set; }
    }
}
