using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.Visa.Models
{
    public class DeviceFingerprintModel
    {
        public string SessionId { get; set; }
        public string OrganizationId { get; set; }
        public string MerchantId { get; set; }
    }
}
