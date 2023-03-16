using Nop.Core;
using System;

namespace Teed.Plugin.Groceries.Domain.ShippingAreas
{
    public class PostalCodeSearch : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual string PostalCode { get; set; }
        public virtual int UserId { get; set; }
    }
}