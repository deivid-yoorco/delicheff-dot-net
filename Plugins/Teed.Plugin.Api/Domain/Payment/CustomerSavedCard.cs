using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Domain.Payment
{
    public class CustomerSavedCard : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public string CardOwnerName { get; set; }
        public int CustomerId { get; set; }
        public string Brand { get; set; }
        public string LastFourDigits { get; set; }
        public string ServiceCustomerId { get; set; }
        public string PaymentMethodSystemName { get; set; }
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
