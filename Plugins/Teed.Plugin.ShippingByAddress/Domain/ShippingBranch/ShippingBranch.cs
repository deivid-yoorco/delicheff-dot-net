using Nop.Core;
using System;

namespace Teed.Plugin.ShippingByAddress.Domain.ShippingBranch
{
    public class ShippingBranch : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }
        
        public virtual string BranchName { get; set; }
        public virtual string BranchPhone { get; set; }
        public virtual string BranchEmail { get; set; }
        public virtual bool ShouldSendEmail { get; set; }
    }
}