using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.ShippingVehicles
{
    public class ShippingVehicle : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual bool Active { get; set; }
        public virtual decimal LoadingCapacity { get; set; }
        public virtual string Log { get; set; }
        public virtual decimal FridgeVolume { get; set; }
        public virtual decimal BunchVolume { get; set; }
        public virtual int FranchiseId { get; set; }
        public virtual string Brand { get; set; }
        public virtual int Year { get; set; }
        public virtual string Plates { get; set; }
        public virtual Franchise.Franchise Franchise { get; set; }
        public virtual DateTime? GpsInstallationDate { get; set; }
    }
}
