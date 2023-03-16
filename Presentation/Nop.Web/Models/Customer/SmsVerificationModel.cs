using System.ComponentModel.DataAnnotations;
using FluentValidation.Attributes;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;
using Nop.Web.Validators.Customer;

namespace Nop.Web.Models.Customer
{
    public partial class SmsVerificationModel : BaseNopModel
    {
        public string ElementSelector { get; set; }
        public string ElementsToCheckSelector { get; set; }
        public string FormSelector { get; set; }
        public int MinutesForCodeRequest { get; set; }
        public string OriginalPhoneNumber { get; set; }
    }
}