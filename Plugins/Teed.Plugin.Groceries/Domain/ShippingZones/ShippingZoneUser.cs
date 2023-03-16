using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.ShippingZones
{
    public class ShippingZoneUser : BaseEntity
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
