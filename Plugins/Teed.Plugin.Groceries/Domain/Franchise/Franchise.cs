using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.Franchise
{
    public class Franchise : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public string Name { get; set; }
        public int UserInChargeId { get; set; }
        public bool IsActive { get; set; }
        public string BuyersIds { get; set; }
        public string Log { get; set; }
    }
}
