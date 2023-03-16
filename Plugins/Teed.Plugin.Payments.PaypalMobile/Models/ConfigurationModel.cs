using Nop.Web.Framework.Mvc.Models;

namespace Teed.Plugin.Payments.PaypalMobile.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        public string TokenizationKeySandbox { get; set; }
        public bool TokenizationKeySandbox_OverrideForStore { get; set; }

        public string TokenizationKeyProduction { get; set; }
        public bool TokenizationKeyProduction_OverrideForStore { get; set; }

        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }
    }
}
