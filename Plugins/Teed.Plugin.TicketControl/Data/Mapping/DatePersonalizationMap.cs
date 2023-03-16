using Nop.Data.Mapping;
using Teed.Plugin.TicketControl.Domain.DatePersonalizations;

namespace Teed.Plugin.TicketControl.Data.Mapping
{
    public class DatePersonalizationMap : NopEntityTypeConfiguration<DatePersonalization>
    {
        public DatePersonalizationMap()
        {
            ToTable("DatePersonalizations");
            HasKey(x => x.Id);

            HasRequired(x => x.Schedule)
                .WithMany()
                .HasForeignKey(x => x.ScheduleId);
        }
    }
}