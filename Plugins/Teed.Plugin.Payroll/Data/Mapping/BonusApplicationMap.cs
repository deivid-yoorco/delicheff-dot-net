using Nop.Data.Mapping;
using Teed.Plugin.Payroll.Domain.Bonuses;

namespace Teed.Plugin.Payroll.Data.Mapping
{
    public class BonusApplicationMap : NopEntityTypeConfiguration<BonusApplication>
    {
        public BonusApplicationMap()
        {
            ToTable("BonusApplications");
            HasKey(x => x.Id);

            this.HasRequired(x => x.Bonus)
                .WithMany(x => x.Applications)
                .HasForeignKey(x => x.BonusId)
                .WillCascadeOnDelete(false);
        }
    }
}