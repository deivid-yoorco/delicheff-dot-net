using Nop.Core;
using System;
using Teed.Plugin.Groceries.Domain.ShippingZones;

namespace Teed.Plugin.Groceries.Domain.ShippingRegions
{
    public class ShippingRegionZone : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int RegionId { get; set; }
        public virtual int ZoneId { get; set; }

        public virtual ShippingRegion Region { get; set; }
        public virtual ShippingZone Zone { get; set; }
    }
}