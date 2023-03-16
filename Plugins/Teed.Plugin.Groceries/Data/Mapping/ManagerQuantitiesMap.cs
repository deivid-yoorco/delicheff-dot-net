using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class ManagerQuantitiesMap : NopEntityTypeConfiguration<ManagerQuantities>
    {
        public ManagerQuantitiesMap()
        {
            ToTable("ManagerQuantities");
            HasKey(x => x.Id);
        }
    }
}