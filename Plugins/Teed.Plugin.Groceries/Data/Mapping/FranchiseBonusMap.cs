using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.Franchise;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class FranchiseBonusMap : NopEntityTypeConfiguration<FranchiseBonus>
    {
        public FranchiseBonusMap()
        {
            ToTable(nameof(FranchiseBonus));
            HasKey(x => x.Id);
        }
    }
}