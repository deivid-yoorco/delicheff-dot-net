using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.MarketingDashboard.Domain.MarketingExpenses
{
    public class MarketingGrossMargin : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public decimal Value { get; set; }
        public DateTime ApplyDate { get; set; }
        public string Log { get; set; }
    }
}
