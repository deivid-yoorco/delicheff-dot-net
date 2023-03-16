using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.ComproPago
{
    public class ComproPagoPaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }

        public string PrivateKey { get; set; }

        public string PublicKey { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }
    }
}