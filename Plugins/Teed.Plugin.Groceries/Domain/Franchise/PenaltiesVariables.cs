using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.Franchise
{
    public class PenaltiesVariables : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public string IncidentCode { get; set; }
        public string PenaltyCustomId { get; set; }
        public string Log { get; set; }
        public decimal Coefficient { get; set; }
        public DateTime ApplyDate { get; set; }
    }
}
