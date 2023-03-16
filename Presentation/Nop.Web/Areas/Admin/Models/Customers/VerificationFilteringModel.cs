using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Web.Areas.Admin.Models.Customers
{
    public partial class VerificationFilteringModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.Customers.SmsVerifications.List.Verified")]
        public int Verified { get; set; }

        [NopResourceDisplayName("Admin.Customers.SmsVerifications.List.PhoneNumber")]
        public string PhoneNumber { get; set; }
    }
}