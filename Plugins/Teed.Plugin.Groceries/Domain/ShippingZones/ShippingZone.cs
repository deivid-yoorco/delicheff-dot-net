using Nop.Core;
using System;

namespace Teed.Plugin.Groceries.Domain.ShippingZones
{
    public class ShippingZone : BaseEntity
    {
        public virtual Guid GuidId { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int CreatedByUserId { get; set; }
        public virtual string PostalCodes { get; set; }
        public virtual string ZoneName { get; set; }
        public virtual string Log { get; set; }

        public virtual string ZoneColor { get; set; }
        public virtual string AdditionalPostalCodes { get; set; }
    }
}