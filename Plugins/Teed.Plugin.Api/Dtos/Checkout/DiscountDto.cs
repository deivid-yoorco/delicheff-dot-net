using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Checkout
{
    public class DiscountDto
    {
        public int DiscountId { get; set; }
        public string CouponCode { get; set; }
        public string Name { get; set; }
        public string ResultMessage { get; set; }
    }
}
