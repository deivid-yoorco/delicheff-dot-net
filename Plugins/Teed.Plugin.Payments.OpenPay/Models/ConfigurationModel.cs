using Nop.Web.Framework.Mvc.Models;

namespace Teed.Plugin.Payments.OpenPay.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        public string Production_MerchantId { get; set; }
        public bool Production_MerchantId_OverrideForStore { get; set; }
        public string Production_PublicKey { get; set; }
        public bool Production_PublicKey_OverrideForStore { get; set; }
        public string Production_PrivateKey { get; set; }
        public bool Production_PrivateKey_OverrideForStore { get; set; }
        public string Production_Url { get; set; }
        public bool Production_Url_OverrideForStore { get; set; }

        public string Sandbox_MerchantId { get; set; }
        public bool Sandbox_MerchantId_OverrideForStore { get; set; }
        public string Sandbox_PublicKey { get; set; }
        public bool Sandbox_PublicKey_OverrideForStore { get; set; }
        public string Sandbox_PrivateKey { get; set; }
        public bool Sandbox_PrivateKey_OverrideForStore { get; set; }
        public string Sandbox_Url { get; set; }
        public bool Sandbox_Url_OverrideForStore { get; set; }

        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }
    }
}
