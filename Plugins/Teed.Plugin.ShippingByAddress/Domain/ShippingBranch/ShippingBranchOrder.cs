using Nop.Core;
using System;

namespace Teed.Plugin.ShippingByAddress.Domain.ShippingBranch
{
    public class ShippingBranchOrder : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }
        
        public virtual int ShippingBranchId { get; set; }
        public virtual int OrderId { get; set; }
    }
}