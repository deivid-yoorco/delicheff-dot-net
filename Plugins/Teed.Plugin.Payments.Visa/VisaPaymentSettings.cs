using Nop.Core.Configuration;
using System;

namespace Teed.Plugin.Payments.Visa
{
    public class VisaPaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }

        public string MerchantId { get; set; }

        public string ApiKeySandbox { get; set; }

        public string SharedSecretKeySandbox { get; set; }

        public string ApiKeyProduction { get; set; }

        public string SharedSecretKeyProduction { get; set; }

        public string OrganizationIdProduction { get; set; }

        public string OrganizationIdSandbox { get; set; }
    }
}
