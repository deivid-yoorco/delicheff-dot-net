using Nop.Data.Mapping;
using Teed.Plugin.Payroll.Domain.Bonuses;

namespace Teed.Plugin.Payroll.Data.Mapping
{
    public class BonusMap : NopEntityTypeConfiguration<Bonus>
    {
        public BonusMap()
        {
            ToTable("Bonuses");
            HasKey(x => x.Id);
        }
    }
}