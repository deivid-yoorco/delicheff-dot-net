using Nop.Web.Framework.Mvc.Models;
using System;

namespace Teed.Plugin.MessageBird.Models
{
    public class MessageBirdLogModel
    {
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public int AdminId { get; set; }
        public string Admin { get; set; }
        public int CustomerId { get; set; }
        public string Customer { get; set; }
        public string TextSent { get; set; }
        public string ToNumber { get; set; }
        public string WhatsAppChannelId { get; set; }
        public string JsonRequest { get; set; }
        public string JsonResponse { get; set; }
        public string Status { get; set; }
    }
}
