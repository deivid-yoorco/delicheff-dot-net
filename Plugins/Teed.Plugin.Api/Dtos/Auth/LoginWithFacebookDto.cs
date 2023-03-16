using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Auth
{
    public class LoginWithFacebookDto
    {
        public string Email { get; set; }

        public string Name { get; set; }

        public string AccessToken { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public LoginDeviceDto Device { get; set; }
    }
}