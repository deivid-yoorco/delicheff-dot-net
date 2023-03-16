using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.TicketControl.Domain.Schedules
{
    public class Schedule : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public string Name { get; set; }
        public int Quantity { get; set; }
        public int DisplayOrder { get; set; }
        public int Hour { get; set; }
        public string Log { get; set; }
    }
}
