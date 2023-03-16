using Nop.Data.Mapping;
using Teed.Plugin.MarketingDashboard.Domain.MarketingExpenses;

namespace Teed.Plugin.MarketingDashboard.Data.Mapping
{
    public class MarketingExpenseMap : NopEntityTypeConfiguration<MarketingExpense>
    {
        public MarketingExpenseMap()
        {
            ToTable(nameof(MarketingExpense));
            HasKey(x => x.Id);
            HasRequired(x => x.ExpenseType)
               .WithMany()
               .HasForeignKey(x => x.ExpenseTypeId);
        }
    }
}