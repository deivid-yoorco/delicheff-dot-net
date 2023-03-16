using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingVehicles;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class ShippingVehicleMap : NopEntityTypeConfiguration<ShippingVehicle>
    {
        public ShippingVehicleMap()
        {
            ToTable(nameof(ShippingVehicle));
            HasKey(x => x.Id);

            this.HasRequired(o => o.Franchise)
                .WithMany()
                .HasForeignKey(o => o.FranchiseId);
        }
    }
}
