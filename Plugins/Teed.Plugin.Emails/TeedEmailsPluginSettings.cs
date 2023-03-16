using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Emails
{
    public class TeedEmailsPluginSettings : ISettings
    {
        public string Aviso { get; set; }
        public string Address { get; set; }
        public string StoreColor { get; set; }

        public int HeaderEmailPicture1Id { get; set; }
        public int HeaderEmailPicture2Id { get; set; }
        public int HeaderEmailPicture3Id { get; set; }
        public int HeaderEmailPicture4Id { get; set; }
        public int HeaderEmailPicture5Id { get; set; }
        public int ShippingEmailPicture1Id { get; set; }
        public int ShippingEmailPicture2Id { get; set; }
        public int ShippingEmailPicture3Id { get; set; }

        public string SubTitleWelcome { get; set; }
        public string BtnTextWelcome { get; set; }

        public string SubTitlePassword { get; set; }
        public string BtnTextPassword { get; set; }

        public string SubTitleValidate { get; set; }
        public string BtnTextValidate { get; set; }

        public string SubTitleStore { get; set; }
        public string BtnTextStore { get; set; }

        public string SubTitlePayment { get; set; }
        public string BtnTextPayment { get; set; }

        public string SubTitlePaypal { get; set; }
        public string BtnTextPaypal { get; set; }

        public string SubTitleOrderConfirm { get; set; }
        public string BtnTextOrderConfirm { get; set; }

        public string SubTitleOrderSent { get; set; }
        public string BtnTextOrderSent { get; set; }

        public string SubTitleOrderDelivered { get; set; }
        public string BtnTextOrderDelivered { get; set; }

        public int HoursToSendAbandonShoppingCartEmail { get; set; }
        public bool AbandonShoppingCartEmailIsActive { get; set; }

        public int DaysToSendReorderEmail { get; set; }
        public bool ReorderEmailIsActive { get; set; }
        public bool ReorderOnlyCompletedOrders { get; set; }
    }
}
