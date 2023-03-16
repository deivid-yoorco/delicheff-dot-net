using Nop.Data.Mapping;
using Teed.Plugin.Payroll.Domain.MinimumWagesCatalogs;

namespace Teed.Plugin.Payroll.Data.Mapping
{
    public class MinimumWagesCatalogMap : NopEntityTypeConfiguration<MinimumWagesCatalog>
    {
        public MinimumWagesCatalogMap()
        {
            ToTable("MinimumWagesCatalogs");
            HasKey(x => x.Id);
        }
    }
}