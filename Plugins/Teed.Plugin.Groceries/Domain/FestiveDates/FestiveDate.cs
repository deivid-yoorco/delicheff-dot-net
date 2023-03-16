using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.FestiveDates
{
    public class FestiveDate : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public DateTime Date { get; set; }
        public bool DontApplyToPayroll { get; set; }
        public bool AppliesYearly { get; set; }
        public string Log { get; set; }
    }
}
