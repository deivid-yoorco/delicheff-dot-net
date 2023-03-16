using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingVehicles;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class ShippingVehicleRouteMap : NopEntityTypeConfiguration<ShippingVehicleRoute>
    {
        public ShippingVehicleRouteMap()
        {
            ToTable(nameof(ShippingVehicleRoute));
            HasKey(x => x.Id);

            this.HasRequired(o => o.Route)
                .WithMany()
                .HasForeignKey(o => o.RouteId);

            this.HasRequired(o => o.Vehicle)
                .WithMany()
                .HasForeignKey(o => o.VehicleId);
        }
    }
}
