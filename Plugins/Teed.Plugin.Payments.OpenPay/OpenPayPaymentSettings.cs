using Nop.Core.Configuration;
using System;

namespace Teed.Plugin.Payments.OpenPay
{
    public class OpenPayPaymentSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the payment method is active
        /// </summary>
        public bool IsActive { get; set; }

        public bool UseSandbox { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }


        public string Production_MerchantId { get; set; }
        public string Production_PublicKey { get; set; }
        public string Production_PrivateKey { get; set; }
        public string Production_Url { get; set; }

        public string Sandbox_MerchantId { get; set; }
        public string Sandbox_PublicKey { get; set; }
        public string Sandbox_PrivateKey { get; set; }
        public string Sandbox_Url { get; set; }
    }
}
