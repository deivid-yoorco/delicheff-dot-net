using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Checkout
{
    public class OrderTotalDto
    {
        public decimal SubTotal { get; set; }

        public decimal Shipping { get; set; }

        public decimal OrderTotalDiscount { get; set; }

        public decimal OrderItemsDiscount { get; set; }

        public decimal OrderTotal { get; set; }

        public List<int> ShoppingCartItemIds { get; set; }
    }
}
