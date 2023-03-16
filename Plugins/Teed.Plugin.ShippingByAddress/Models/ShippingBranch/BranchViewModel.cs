using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.ShippingByAddress.Models.ShippingBranch
{
    public class BranchViewModel
    {
        public int Id { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual string PostalCode { get; set; }
        public virtual string DaysToShip { get; set; }
    }
}
