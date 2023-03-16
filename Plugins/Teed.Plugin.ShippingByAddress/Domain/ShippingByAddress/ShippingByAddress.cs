using Nop.Core;
using System;

namespace Teed.Plugin.ShippingByAddress.Domain.ShippingByAddressD
{
    public class ShippingByAddressD : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }
        
        public virtual string PostalCode { get; set; }
        public virtual string DaysToShip { get; set; }
        public virtual string State { get; set; }
        public virtual string City { get; set; }
        public virtual string Suburbs { get; set; }
        public virtual int? ShippingBranchId { get; set; }
    }
}