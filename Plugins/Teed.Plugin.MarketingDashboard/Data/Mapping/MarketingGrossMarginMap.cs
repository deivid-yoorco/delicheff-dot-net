using Nop.Data.Mapping;
using Teed.Plugin.MarketingDashboard.Domain.MarketingExpenses;

namespace Teed.Plugin.MarketingDashboard.Data.Mapping
{
    public class MarketingGrossMarginMap : NopEntityTypeConfiguration<MarketingGrossMargin>
    {
        public MarketingGrossMarginMap()
        {
            ToTable(nameof(MarketingGrossMargin));
            HasKey(x => x.Id);
        }
    }
}