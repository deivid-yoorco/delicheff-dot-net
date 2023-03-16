using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.Franchise;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class PenaltiesVariablesMap : NopEntityTypeConfiguration<PenaltiesVariables>
    {
        public PenaltiesVariablesMap()
        {
            ToTable(nameof(PenaltiesVariables));
            HasKey(x => x.Id);
            Property(x => x.Coefficient).HasPrecision(18, 4);
        }
    }
}