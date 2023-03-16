using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Auth
{
    public class UpdateAccountDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string OldPassword { get; set; }

        public string NewPassword { get; set; }

        public string BirthDate { get; set; }

        public string Gender { get; set; }

        public string PhoneNumber { get; set; }

        public bool NewsletterCheck { get; set; }

        public string ProfilePictureBase64 { get; set; }

        public string ProfilePictureMimeType { get; set; }
    }
}
