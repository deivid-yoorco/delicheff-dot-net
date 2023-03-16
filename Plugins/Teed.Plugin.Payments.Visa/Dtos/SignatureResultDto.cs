using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.Visa.Dtos
{
    public class SignatureResultDto
    {
        public string Signature { get; set; }

        public string MerchantId { get; set; }

        public string Host { get; set; }

        public string DateParsed { get; set; }
    }
}
