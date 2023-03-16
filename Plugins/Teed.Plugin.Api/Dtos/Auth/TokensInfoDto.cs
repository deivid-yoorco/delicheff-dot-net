using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Auth
{
    public class TokensInfoDto
    {
        public string Token { get; set; }

        public string RefreshToken { get; set; }
    }
}
