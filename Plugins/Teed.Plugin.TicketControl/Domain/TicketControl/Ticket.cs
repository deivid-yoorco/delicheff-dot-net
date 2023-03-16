using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.TicketControl.Domain.Tickets
{
    public class Ticket : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public int OrderId { get; set; }
        public Guid TicketId { get; set; }
        public int QrPictureId { get; set; }
        public DateTime? VerificationDateUtc { get; set; }
        public int? VerificationUserId { get; set; }
        public string Log { get; set; }
    }
}
