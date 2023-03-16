using Nop.Data.Mapping;
using Teed.Plugin.Payroll.Domain.Assistances;

namespace Teed.Plugin.Payroll.Data.Mapping
{
    public class AssistanceOverrideMap : NopEntityTypeConfiguration<AssistanceOverride>
    {
        public AssistanceOverrideMap()
        {
            ToTable("AssistanceOverrides");
            HasKey(x => x.Id);
        }
    }
}