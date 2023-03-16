using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class ShippingRouteUserMap : NopEntityTypeConfiguration<ShippingRouteUser>
    {
        public ShippingRouteUserMap()
        {
            ToTable("ShippingRoutesUser");
            HasKey(x => x.Id);
        }
    }
}