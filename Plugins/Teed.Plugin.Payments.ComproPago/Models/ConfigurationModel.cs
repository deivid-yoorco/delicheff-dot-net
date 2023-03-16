using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.ComproPago.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        public string PublicKey { get; set; }
        public bool PublicKey_OverrideForStore { get; set; }

        public string PrivateKey { get; set; }
        public bool PrivateKey_OverrideForStore { get; set; }

        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }
    }
}
