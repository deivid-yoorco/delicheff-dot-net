using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.MercadoPago.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        public string PublicKey { get; set; }
        public bool PublicKey_OverrideForStore { get; set; }

        public string AccessToken { get; set; }
        public bool AccessToken_OverrideForStore { get; set; }

        public string SandboxPublicKey { get; set; }
        public bool SandboxPublicKey_OverrideForStore { get; set; }

        public string SandboxAccessToken { get; set; }
        public bool SandboxAccessToken_OverrideForStore { get; set; }

        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        public bool RequireInmediatePayment { get; set; }
        public bool RequireInmediatePayment_OverrideForStore { get; set; }

        public bool CashAllowed { get; set; }
        public bool CashAllowed_OverrideForStore { get; set; }

        public bool PrepaidCardAllowed { get; set; }
        public bool PrepaidCardAllowed_OverrideForStore { get; set; }

        public bool DebitCardAllowed { get; set; }
        public bool DebitCardAllowed_OverrideForStore { get; set; }

        public bool CreditCardAllowed { get; set; }
        public bool CreditCardAllowed_OverrideForStore { get; set; }

        public bool BankTransferAllowed { get; set; }
        public bool BankTransferAllowed_OverrideForStore { get; set; }
    }
}
