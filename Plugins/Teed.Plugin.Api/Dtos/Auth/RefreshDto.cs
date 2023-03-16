using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Auth
{
    public class RefreshDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string RefreshToken { get; set; }

        [Required]
        public string DeviceUuid { get; set; }
    }
}
