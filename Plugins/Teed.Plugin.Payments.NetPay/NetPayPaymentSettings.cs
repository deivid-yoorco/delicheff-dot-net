using Nop.Core.Configuration;
using System;

namespace Teed.Plugin.Payments.NetPay
{
    public class NetPayPaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }

        public string PublicKeyProduction { get; set; }

        public string SecretKeyProduction { get; set; }

        public string PublicKeySandbox { get; set; }

        public string SecretKeySandbox { get; set; }

        public bool Allow3Msi { get; set; }

        public bool Allow6Msi { get; set; }

        public bool Allow9Msi { get; set; }

        public bool Allow12Msi { get; set; }

        public bool Allow18Msi { get; set; }

        public decimal MinimumMsiAmount { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }
    }
}
