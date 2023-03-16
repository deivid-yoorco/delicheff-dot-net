using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.ShippingRegions;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class ShippingRegionMap : NopEntityTypeConfiguration<ShippingRegion>
    {
        public ShippingRegionMap()
        {
            ToTable("ShippingRegions");
            HasKey(x => x.Id);
        }
    }
}