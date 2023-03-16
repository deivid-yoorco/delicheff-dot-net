using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.Franchise;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class FranchiseMap : NopEntityTypeConfiguration<Franchise>
    {
        public FranchiseMap()
        {
            ToTable("Franchises");
            HasKey(x => x.Id);
        }
    }
}