using Nop.Web.Framework.Mvc.Models;

namespace Teed.Plugin.Payments.Visa.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        public string MerchantId { get; set; }
        public bool MerchantId_OverrideForStore { get; set; }

        public string ApiKeySandbox { get; set; }
        public bool ApiKeySandbox_OverrideForStore { get; set; }

        public string SharedSecretKeySandbox { get; set; }
        public bool SharedSecretKeySandbox_OverrideForStore { get; set; }

        public string ApiKeyProduction { get; set; }
        public bool ApiKeyProduction_OverrideForStore { get; set; }

        public string SharedSecretKeyProduction { get; set; }
        public bool SharedSecretKeyProduction_OverrideForStore { get; set; }

        public string OrganizationIdProduction { get; set; }
        public bool OrganizationIdProduction_OverrideForStore { get; set; }

        public string OrganizationIdSandbox { get; set; }
        public bool OrganizationIdSandbox_OverrideForStore { get; set; }
    }
}
