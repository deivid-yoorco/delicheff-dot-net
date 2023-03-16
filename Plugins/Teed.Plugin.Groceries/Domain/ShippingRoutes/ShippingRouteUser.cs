using Nop.Core;
using System;

namespace Teed.Plugin.Groceries.Domain.ShippingRoutes
{
    public class ShippingRouteUser : BaseEntity
    {
        public virtual Guid GuidId { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int ShippingRouteId { get; set; }
        public virtual int UserInChargeId { get; set; }
        public virtual DateTime ResponsableDateUtc { get; set; }
        public virtual string Log { get; set; }
    }
}