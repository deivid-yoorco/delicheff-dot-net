using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.MercadoPago
{
    public class MercadoPagoPaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }

        public string AccessToken { get; set; }

        public string PublicKey { get; set; }

        public string SandboxAccessToken { get; set; }

        public string SandboxPublicKey { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public bool RequireInmediatePayment { get; set; }

        public bool CashAllowed { get; set; }

        public bool PrepaidCardAllowed { get; set; }

        public bool DebitCardAllowed { get; set; }

        public bool CreditCardAllowed { get; set; }

        public bool BankTransferAllowed { get; set; }
    }
}