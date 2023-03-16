using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Notifications
{
    public class SaveNotificationTokenDto
    {
        public string Token { get; set; }
        public string DeviceUuid { get; set; }
    }
}
