using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Checkout
{
    public class PaymentMethodDto
    {
        public string PaymentMethodSystemName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Fee { get; set; }
        public bool Selected { get; set; }
        public string LogoUrl { get; set; }
        public SavedCardDataDto SavedCard { get; set; }
    }

    public class SavedCardDataDto
    {
        public string CardOwnerName { get; set; }
        public string Brand { get; set; }
        public string LastFourDigits { get; set; }
        public string CardLogoUrl { get; set; }
        public string ServiceCustomerId { get; set; }
        public string BillFirstName { get; set; }
        public string BillLastName { get; set; }
        public string BillAddress1 { get; set; }
        public string BillLocality { get; set; }
        public string BillAdministrativeArea { get; set; }
        public string BillPostalCode { get; set; }
        public string BillCountry { get; set; }
        public string BillEmail { get; set; }
        public string BillPhoneNumber { get; set; }
    }
}