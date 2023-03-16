using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class AditionalCostMap : NopEntityTypeConfiguration<AditionalCost>
    {
        public AditionalCostMap()
        {
            ToTable("AditionalCost");
            HasKey(x => x.Id);
        }
    }
}