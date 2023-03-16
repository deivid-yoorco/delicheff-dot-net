using Nop.Core.Configuration;

namespace Nop.Core.Domain.Catalog
{
    public partial class RewardItemSettings : ISettings
    {
        public decimal MinimumAmount { get; set; }
    }
}