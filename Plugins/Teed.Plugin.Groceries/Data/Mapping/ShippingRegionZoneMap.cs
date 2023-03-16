using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.ShippingRegions;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class ShippingRegionZoneMap : NopEntityTypeConfiguration<ShippingRegionZone>
    {
        public ShippingRegionZoneMap()
        {
            ToTable("ShippingRegionZones");
            HasKey(x => x.Id);

            this.HasRequired(o => o.Region)
                .WithMany()
                .HasForeignKey(o => o.RegionId);

            this.HasRequired(o => o.Zone)
                .WithMany()
                .HasForeignKey(o => o.ZoneId);
        }
    }
}