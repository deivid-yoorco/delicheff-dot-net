using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Models
{
    public class FacebookTokenModel
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
    }

    public class FacebookTokenValidationModel
    {
        public FacebookDataValidationModel data { get; set; }
    }

    public class FacebookDataValidationModel
    {
        public string app_id { get; set; }
        public string type { get; set; }
        public string application { get; set; }
        public string[] scopes { get; set; }
        public string user_id { get; set; }
        public int expires_at { get; set; }
        public bool is_valid { get; set; }
        public FacebookErrorValidationModel error { get; set; }
    }

    public class FacebookErrorValidationModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public int subcode { get; set; }
    }
}
