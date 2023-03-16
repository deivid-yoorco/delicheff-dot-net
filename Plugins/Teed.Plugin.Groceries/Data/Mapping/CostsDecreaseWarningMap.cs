using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class CostsDecreaseWarningMap : NopEntityTypeConfiguration<CostsDecreaseWarning>
    {
        public CostsDecreaseWarningMap()
        {
            ToTable("CostsDecreaseWarnings");
            HasKey(x => x.Id);
        }
    }
}