using Nop.Data.Mapping;
using Teed.Plugin.MarketingDashboard.Domain.MarketingExpenses;

namespace Teed.Plugin.MarketingDashboard.Data.Mapping
{
    public class MarketingAutomaticExpenseMap : NopEntityTypeConfiguration<MarketingAutomaticExpense>
    {
        public MarketingAutomaticExpenseMap()
        {
            ToTable(nameof(MarketingAutomaticExpense));
            HasKey(x => x.Id);
        }
    }
}