using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.Franchise;
using Teed.Plugin.Groceries.Domain.OrderReports;
using Teed.Plugin.Groceries.Domain.PenaltiesCatalog;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class PenaltiesCatalogMap : NopEntityTypeConfiguration<PenaltiesCatalog>
    {
        public PenaltiesCatalogMap()
        {
            ToTable("PenaltiesCatalog");
            HasKey(x => x.Id);
        }
    }
}