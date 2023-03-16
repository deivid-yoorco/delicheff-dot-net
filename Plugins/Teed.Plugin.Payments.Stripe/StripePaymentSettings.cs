using Nop.Core.Configuration;
using System;

namespace Teed.Plugin.Payments.Stripe
{
    public class StripePaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }

        public string PublishableKeySandbox { get; set; }

        public string SecretKeySandbox { get; set; }

        public string PublishableKeyProduction { get; set; }

        public string SecretKeyProduction { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }
    }
}
