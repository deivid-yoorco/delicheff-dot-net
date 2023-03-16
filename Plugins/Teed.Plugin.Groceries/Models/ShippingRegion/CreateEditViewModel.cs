using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.ShippingRegion
{
    public class CreateEditViewModel
    {
        public int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual int Schedule1Quantity { get; set; }
        public virtual int Schedule2Quantity { get; set; }
        public virtual int Schedule3Quantity { get; set; }
        public virtual int Schedule4Quantity { get; set; }
        public virtual string ZoneIds { get; set; }
        public virtual string ZoneNames { get; set; }

        public Dictionary<string, string> CurrentOrderCount { get; set; }
    }
}
