using Nop.Core;
using System;

namespace Teed.Plugin.Api.Domain.Notifications
{
    public class QueuedNotification : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public int NotificationId { get; set; }
        public Notification Notification { get; set; }
        public string ErrorMessage { get; set; }
        public int CustomerId { get; set; }
        public DateTime? SentAtUtc { get; set; }
        public string MessageId { get; set; }
    }
}