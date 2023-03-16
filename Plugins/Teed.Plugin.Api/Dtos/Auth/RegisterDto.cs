using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Auth
{
    public class RegisterDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string BirthDate { get; set; }

        public string Gender { get; set; }

        public string PhoneNumber { get; set; }

        public bool NewsletterCheck { get; set; }

        public LoginDeviceDto Device { get; set; }
    }

    public class RegistrationResultDto
    {
        public string BirthDate { get; set; }

        public string Gender { get; set; }

        public string PhoneNumber { get; set; }

        public bool NewsletterCheck { get; set; }

        public int ProfilePictureId { get; set; }

        public string SuccessNote { get; set; }

        public TokensInfoDto Tokens { get; set; }
    }
}
