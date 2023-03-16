using Nop.Core.Configuration;
using System;

namespace Teed.Plugin.Payments.PaypalPlus
{
    public class PaypalPlusPaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }

        public string ClientId { get; set; }

        public string SecretKey { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public string Token { get; set; }

        public DateTime TokenExpirationDateUtc { get; set; }
    }
}
