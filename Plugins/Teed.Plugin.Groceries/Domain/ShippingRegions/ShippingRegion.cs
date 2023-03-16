using Nop.Core;
using System;

namespace Teed.Plugin.Groceries.Domain.ShippingRegions
{
    public class ShippingRegion : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual string Name { get; set; }
        public virtual int Schedule1Quantity { get; set; }
        public virtual int Schedule2Quantity { get; set; }
        public virtual int Schedule3Quantity { get; set; }
        public virtual int Schedule4Quantity { get; set; }
    }
}