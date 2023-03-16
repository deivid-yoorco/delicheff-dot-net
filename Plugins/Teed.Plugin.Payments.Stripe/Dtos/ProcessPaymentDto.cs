using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.Stripe.Dtos
{
    public class ProcessPaymentDto
    {
        public int UserId { get; set; }

        public string StripeCustomerId { get; set; }
    }
}
