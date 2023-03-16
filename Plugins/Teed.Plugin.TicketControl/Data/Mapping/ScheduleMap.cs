using Nop.Data.Mapping;
using Teed.Plugin.TicketControl.Domain.Schedules;

namespace Teed.Plugin.TicketControl.Data.Mapping
{
    public class ScheduleMap : NopEntityTypeConfiguration<Schedule>
    {
        public ScheduleMap()
        {
            ToTable("Schedules");
            HasKey(x => x.Id);
        }
    }
}