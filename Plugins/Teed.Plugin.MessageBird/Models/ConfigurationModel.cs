using Nop.Web.Framework.Mvc.Models;

namespace Teed.Plugin.MessageBird.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        public bool IsSandbox { get; set; }
        public bool IsSandbox_OverrideForStore { get; set; }

        public string SandboxApiKey { get; set; }
        public bool SandboxApiKey_OverrideForStore { get; set; }
        public string SandboxWhatsAppChannelId { get; set; }
        public bool SandboxWhatsAppChannelId_OverrideForStore { get; set; }
        public string SandboxNameSpace { get; set; }
        public bool SandboxNameSpace_OverrideForStore { get; set; }

        public string ApiKey { get; set; }
        public bool ApiKey_OverrideForStore { get; set; }
        public string WhatsAppChannelId { get; set; }
        public bool WhatsAppChannelId_OverrideForStore { get; set; }
        public string NameSpace { get; set; }
        public bool NameSpace_OverrideForStore { get; set; }
    }
}
