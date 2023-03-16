using Nop.Data.Mapping;
using Teed.Plugin.Payroll.Domain.Bonuses;

namespace Teed.Plugin.Payroll.Data.Mapping
{
    public class BonusAmountMap : NopEntityTypeConfiguration<BonusAmount>
    {
        public BonusAmountMap()
        {
            ToTable("BonusAmounts");
            HasKey(x => x.Id);

            this.HasRequired(x => x.Bonus)
                .WithMany(x => x.Amounts)
                .HasForeignKey(x => x.BonusId)
                .WillCascadeOnDelete(false);
        }
    }
}