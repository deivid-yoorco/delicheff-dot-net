using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.ShippingByAddress.Models.ShippingBranch
{
    public class CreateViewModel
    {
        public virtual string BranchName { get; set; }
        public virtual string BranchPhone { get; set; }
        public virtual string BranchEmail { get; set; }
        public virtual bool ShouldSendEmail { get; set; }
    }
}
