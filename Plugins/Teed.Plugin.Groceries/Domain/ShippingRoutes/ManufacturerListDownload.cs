using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.ShippingRoutes
{
    public class ManufacturerListDownload : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public DateTime OrderShippingDate { get; set; }
        public bool AllowDownload { get; set; }
        public string Log { get; set; }
    }
}