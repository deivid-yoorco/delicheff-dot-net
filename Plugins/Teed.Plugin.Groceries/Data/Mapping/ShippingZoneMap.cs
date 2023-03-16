using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.ShippingZones;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class ShippingZoneMap : NopEntityTypeConfiguration<ShippingZone>
    {
        public ShippingZoneMap()
        {
            ToTable("ShippingZones");
            HasKey(x => x.Id);
        }
    }
}