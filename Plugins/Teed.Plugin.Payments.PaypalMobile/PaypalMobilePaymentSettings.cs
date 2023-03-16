using Nop.Core.Configuration;

namespace Teed.Plugin.Payments.PaypalMobile
{
    public class PaypalMobilePaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }

        public string TokenizationKeySandbox { get; set; }

        public string TokenizationKeyProduction { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }
    }
}
