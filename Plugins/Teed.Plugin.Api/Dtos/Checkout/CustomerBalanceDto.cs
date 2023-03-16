using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Checkout
{
    public class CustomerBalanceDto
    {
        public decimal CurrentBalance { get; set; }
        public decimal OrderUsableBalance { get; set; }
        public bool BalanceIsActive { get; set; }
    }
}
