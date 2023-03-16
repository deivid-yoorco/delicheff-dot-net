using Nop.Web.Framework.Mvc.Models;

namespace Teed.Plugin.Payments.Stripe.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        public string PublishableKeySandbox { get; set; }
        public bool PublishableKeySandbox_OverrideForStore { get; set; }

        public string SecretKeySandbox { get; set; }
        public bool SecretKeySandbox_OverrideForStore { get; set; }

        public string PublishableKeyProduction { get; set; }
        public bool PublishableKeyProduction_OverrideForStore { get; set; }

        public string SecretKeyProduction { get; set; }
        public bool SecretKeyProduction_OverrideForStore { get; set; }

        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }
    }
}
