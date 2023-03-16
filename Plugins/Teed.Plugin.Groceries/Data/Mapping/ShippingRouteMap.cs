using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class ShippingRouteMap : NopEntityTypeConfiguration<ShippingRoute>
    {
        public ShippingRouteMap()
        {
            ToTable("ShippingRoutes");
            HasKey(x => x.Id);
        }
    }
}