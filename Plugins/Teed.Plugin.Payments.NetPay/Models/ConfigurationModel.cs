using Nop.Web.Framework.Mvc.Models;

namespace Teed.Plugin.Payments.NetPay.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        public string PublicKeyProduction { get; set; }
        public bool PublicKeyProduction_OverrideForStore { get; set; }

        public string SecretKeyProduction { get; set; }
        public bool SecretKeyProduction_OverrideForStore { get; set; }

        public string PublicKeySandbox { get; set; }
        public bool PublicKeySandbox_OverrideForStore { get; set; }

        public string SecretKeySandbox { get; set; }
        public bool SecretKeySandbox_OverrideForStore { get; set; }

        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        public bool Allow3Msi { get; set; }
        public bool Allow3Msi_OverrideForStore { get; set; }

        public bool Allow6Msi { get; set; }
        public bool Allow6Msi_OverrideForStore { get; set; }

        public bool Allow9Msi { get; set; }
        public bool Allow9Msi_OverrideForStore { get; set; }

        public bool Allow12Msi { get; set; }
        public bool Allow12Msi_OverrideForStore { get; set; }

        public bool Allow18Msi { get; set; }
        public bool Allow18Msi_OverrideForStore { get; set; }

        public decimal MinimumMsiAmount { get; set; }
        public bool MinimumMsiAmount_OverrideForStore { get; set; }
    }
}
