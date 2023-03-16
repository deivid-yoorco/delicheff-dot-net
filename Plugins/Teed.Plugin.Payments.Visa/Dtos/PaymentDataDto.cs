using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.Visa.Dtos
{
    public class PaymentDataDto
    {
        public string Url { get; set; }

        public string MerchantId { get; set; }

        public string ApiKey { get; set; }

        public string SecretSharedKey { get; set; }
    }
}
