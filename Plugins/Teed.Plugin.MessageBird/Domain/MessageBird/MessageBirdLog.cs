using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.MessageBird.Domain
{
    public class MessageBirdLog : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public int AdminId { get; set; }
        public int CustomerId { get; set; }
        public string TextSent { get; set; }
        public string ToNumber { get; set; }
        public string WhatsAppChannelId { get; set; }
        public string JsonRequest { get; set; }
        public string JsonResponse { get; set; }
        public string Status { get; set; }
    }
}
