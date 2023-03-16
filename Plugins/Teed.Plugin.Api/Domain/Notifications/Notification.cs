using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Domain.Notifications
{
    public class Notification : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        private ICollection<QueuedNotification> _queuedNotifications;

        public string Title { get; set; }
        public string Body { get; set; }
        public string Log { get; set; }
        public DateTime DontSendBeforeDateUtc { get; set; }
        public bool IsCancelled { get; set; }
        public string CustomerIds { get; set; }
        public int ActionTypeId { get; set; }
        public string AdditionalData { get; set; }
        public bool IsSystemNotification { get; set; }

        public virtual ICollection<QueuedNotification> QueuedNotifications
        {
            get { return _queuedNotifications ?? (_queuedNotifications = new List<QueuedNotification>()); }
            protected set { _queuedNotifications = value; }
        }
    }
}
