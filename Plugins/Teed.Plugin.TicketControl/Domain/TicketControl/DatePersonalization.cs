using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.TicketControl.Domain.Schedules;

namespace Teed.Plugin.TicketControl.Domain.DatePersonalizations
{
    public class DatePersonalization : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public DateTime Date { get; set; }
        public int Quantity { get; set; }
        public int ScheduleId { get; set; }
        public Schedule Schedule { get; set; }
        public string Log { get; set; }
    }
}
