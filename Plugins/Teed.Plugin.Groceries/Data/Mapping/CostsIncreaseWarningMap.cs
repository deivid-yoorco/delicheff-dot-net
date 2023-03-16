using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class CostsIncreaseWarningMap : NopEntityTypeConfiguration<CostsIncreaseWarning>
    {
        public CostsIncreaseWarningMap()
        {
            ToTable("CostsIncreaseWarnings");
            HasKey(x => x.Id);
        }
    }
}