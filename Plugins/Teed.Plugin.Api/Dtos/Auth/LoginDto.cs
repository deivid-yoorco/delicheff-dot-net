using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Auth
{
    public class LoginDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public LoginDeviceDto Device { get; set; }
    }

    public class LoginDeviceDto
    {
        [Required]
        public string Uuid { get; set; }

        public string Model { get; set; }

        public string Platform { get; set; }

        public string Version { get; set; }

        public string Manufacturer { get; set; }

        public string Serial { get; set; }
    }
}
