using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Auth
{
    public class RegisterCustomerValidationDto
    {
        public string Email { get; set; }
        public string PhoneNumer { get; set; }
        public bool VerifyOnlyNumber { get; set; }
    }

    public class RegisterCustomerValidationResultDto
    {
        public string Code { get; set; }
        public bool ShouldValidatePhone { get; set; }
        public string ErrorMessage { get; set; }
        public int MinutesForCodeRequest { get; set; }
    }

    public class IRegisterCodeValidationDto
    {
        public string Code { get; set; }
        public string PhoneNumer { get; set; }
    }

    public class IRegisterCodeValidationResultDto
    {
        public bool ValidatedCorrectly { get; set; }
        public string ErrorMessage { get; set; }
    }
}
