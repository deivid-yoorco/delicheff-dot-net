using Nop.Core;
using System;

namespace Teed.Plugin.Groceries.Domain.ShippingRoutes
{
    public class ShippingRoute : BaseEntity
    {
        public virtual Guid GuidId { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int CreatedByUserId { get; set; }
        public virtual string PostalCodes { get; set; }
        public bool Active { get; set; }
        public virtual string RouteName { get; set; }
        public virtual string VehicleName { get; set; }
        public virtual decimal LoadingCapacity { get; set; }
        public virtual string Log { get; set; }

        public virtual decimal FridgeVolume { get; set; }
        public virtual decimal BunchVolume { get; set; }

        public virtual int? FranchiseId { get; set; }
    }
}