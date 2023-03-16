using Nop.Core.Configuration;

namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// Customer settings
    /// </summary>
    public class SmsVerificationSettings : ISettings
    {
        public bool IsActive { get; set; }
        public bool IsSandbox { get; set; }
        public int MinutesForCodeRequest { get; set; }

        public string SandboxApiKey { get; set; }
        public string ApiKey { get; set; }
    }
}