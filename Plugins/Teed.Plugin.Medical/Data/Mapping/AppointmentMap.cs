using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Appointment;

namespace Teed.Plugin.Medical.Data.Mapping
{
    public class AppointmentMap : NopEntityTypeConfiguration<Appointment>
    {
        public AppointmentMap()
        {
            ToTable("Appointments");
            HasKey(a => a.Id);
            Property(a => a.Comments).HasMaxLength(500);
            Property(a => a.CheckInDate).IsOptional();
            Property(a => a.CheckOutDate).IsOptional();
        }
    }
}