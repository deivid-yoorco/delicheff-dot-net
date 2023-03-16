using Nop.Data.Mapping;
using Teed.Plugin.ShippingByAddress.Domain.ShippingByAddressD;

namespace Teed.Plugin.ShippingByAddress.Data.Mapping
{
    public class ShippingByAddressMap : NopEntityTypeConfiguration<ShippingByAddressD>
    {
        public ShippingByAddressMap()
        {
            ToTable("ShippingByAddress");
            HasKey(x => x.Id);
        }
    }
}