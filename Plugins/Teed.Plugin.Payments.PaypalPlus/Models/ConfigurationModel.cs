using Nop.Web.Framework.Mvc.Models;

namespace Teed.Plugin.Payments.PaypalPlus.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        public string ClientId { get; set; }
        public bool ClientId_OverrideForStore { get; set; }

        public string SecretKey { get; set; }
        public bool SecretKey_OverrideForStore { get; set; }

        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }
    }
}
