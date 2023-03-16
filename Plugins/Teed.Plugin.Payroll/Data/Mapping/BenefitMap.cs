using Nop.Data.Mapping;
using Teed.Plugin.Payroll.Domain.Benefits;

namespace Teed.Plugin.Payroll.Data.Mapping
{
    public class BenefitMap : NopEntityTypeConfiguration<Benefit>
    {
        public BenefitMap()
        {
            ToTable("Benefits");
            HasKey(x => x.Id);
        }
    }
}