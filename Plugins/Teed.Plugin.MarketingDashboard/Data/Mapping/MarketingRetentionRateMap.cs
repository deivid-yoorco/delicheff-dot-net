using Nop.Data.Mapping;
using Teed.Plugin.MarketingDashboard.Domain.MarketingExpenses;

namespace Teed.Plugin.MarketingDashboard.Data.Mapping
{
    public class MarketingRetentionRateMap : NopEntityTypeConfiguration<MarketingRetentionRate>
    {
        public MarketingRetentionRateMap()
        {
            ToTable(nameof(MarketingRetentionRate));
            HasKey(x => x.Id);

            Property(x => x.Value).HasPrecision(18, 4);
        }
    }
}