using Nop.Data.Mapping;
using Teed.Plugin.Medical.Domain.Appointment;

namespace Teed.Plugin.Medical.Data.Mapping
{
    public class AppointmentItemMap : NopEntityTypeConfiguration<AppointmentItem>
    {
        public AppointmentItemMap()
        {
            ToTable("AppointmentItem");
            HasKey(a => a.Id);
        }
    }
}