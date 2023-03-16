using Nop.Core;
using System;

namespace Nop.Plugin.Shipping.Estafeta.Domain.Shopping
{
    public class Shopping : BaseEntity
    {
        public virtual Guid GuidId { get; set; }
        public virtual string Name { get; set; }
        public virtual string GuideNumber { get; set; }
        public virtual string TrackingCode { get; set; }
        public virtual byte[] guiaPdf { get; set; }
        public virtual string numOrder { get; set; }

        public virtual DateTime Created { get; set; }
        public virtual DateTime Updated { get; set; }
        public virtual bool Deleted { get; set; }
    }
}
