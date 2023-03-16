using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;

namespace Teed.Plugin.Groceries.Domain.ShippingVehicles
{
    public class ShippingVehicleRoute : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int RouteId { get; set; }
        public virtual int VehicleId { get; set; }
        public virtual DateTime ShippingDate { get; set; }
        public virtual string Log { get; set; }

        public virtual ShippingRoute Route { get; set; }
        public virtual ShippingVehicle Vehicle { get; set; }
    }
}