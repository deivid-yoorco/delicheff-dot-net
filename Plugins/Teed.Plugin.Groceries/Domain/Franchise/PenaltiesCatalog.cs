using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.Franchise;

namespace Teed.Plugin.Groceries.Domain.PenaltiesCatalog
{
    public class PenaltiesCatalog : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public string Concepto { get; set; }
        public string PenaltyCustomId { get; set; }

        public string Unit { get; set; }
        public decimal Amount { get; set; }
        public string Log { get; set; }
        public DateTime ApplyDate { get; set; }
    }
}
