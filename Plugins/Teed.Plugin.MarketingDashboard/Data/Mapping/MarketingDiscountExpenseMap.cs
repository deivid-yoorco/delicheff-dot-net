using Nop.Data.Mapping;
using Teed.Plugin.MarketingDashboard.Domain.MarketingExpenses;

namespace Teed.Plugin.MarketingDashboard.Data.Mapping
{
    public class MarketingDiscountExpenseMap : NopEntityTypeConfiguration<MarketingDiscountExpense>
    {
        public MarketingDiscountExpenseMap()
        {
            ToTable(nameof(MarketingDiscountExpense));
            HasKey(x => x.Id);

            this.HasRequired(x => x.MarketingAutomaticExpense)
                .WithMany(x => x.MarketingDiscountExpenses)
                .HasForeignKey(x => x.MarketingAutomaticExpenseId)
                .WillCascadeOnDelete(false);
        }
    }
}