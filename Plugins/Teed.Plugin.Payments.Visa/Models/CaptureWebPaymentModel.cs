using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.Visa.Models
{
    public class CaptureWebPaymentModel
    {
        public bool CreatingNewCard { get; set; }
        public string CardType { get; set; }
        public int SelectedBillingAddressId { get; set; }
        public CapturedCardModel Card { get; set; }
        public string SessionId { get; set; }
    }

    public class CapturedCardModel
    {
        public string CardNumber { get; set; }
        public string ExpirationMonth { get; set; }
        public string ExpirationYear { get; set; }
        public string SecurityCode { get; set; }
    }
}
