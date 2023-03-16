using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.MessageBird
{
    public class MessageBirdSettings : ISettings
    {
        public bool IsSandbox { get; set; }

        public string SandboxApiKey { get; set; }
        public string SandboxWhatsAppChannelId { get; set; }
        public string SandboxNamespace { get; set; }

        public string ApiKey { get; set; }
        public string WhatsAppChannelId { get; set; }
        public string Namespace { get; set; }
    }
}
