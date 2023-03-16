using Nop.Data.Mapping;
using Teed.Plugin.Api.Domain.Notifications;

namespace Teed.Plugin.Api.Data.Mapping
{
    public class NotificationMap : NopEntityTypeConfiguration<Notification>
    {
        public NotificationMap()
        {
            ToTable(nameof(Notification));
            HasKey(m => m.Id);

            Property(x => x.Body).HasMaxLength(512).IsRequired();
            Property(x => x.Title).HasMaxLength(30).IsRequired();
        }
    }
}