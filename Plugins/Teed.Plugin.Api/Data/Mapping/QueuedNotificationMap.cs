using Nop.Data.Mapping;
using Teed.Plugin.Api.Domain.Notifications;

namespace Teed.Plugin.Api.Data.Mapping
{
    public class QueuedNotificationMap : NopEntityTypeConfiguration<QueuedNotification>
    {
        public QueuedNotificationMap()
        {
            ToTable(nameof(QueuedNotification));
            HasKey(m => m.Id);

            HasRequired(x => x.Notification)
                .WithMany(x => x.QueuedNotifications)
                .HasForeignKey(x => x.NotificationId);
        }
    }
}
