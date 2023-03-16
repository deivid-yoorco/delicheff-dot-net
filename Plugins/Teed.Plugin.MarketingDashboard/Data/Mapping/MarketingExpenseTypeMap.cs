using Nop.Data.Mapping;
using Teed.Plugin.MarketingDashboard.Domain.MarketingExpenses;

namespace Teed.Plugin.MarketingDashboard.Data.Mapping
{
    public class MarketingExpenseTypeMap : NopEntityTypeConfiguration<MarketingExpenseType>
    {
        public MarketingExpenseTypeMap()
        {
            ToTable(nameof(MarketingExpenseType));
            HasKey(x => x.Id);
        }
    }
}