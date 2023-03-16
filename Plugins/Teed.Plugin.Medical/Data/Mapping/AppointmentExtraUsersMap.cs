using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Appointment;

namespace Teed.Plugin.Medical.Data.Mapping
{
    public class AppointmentExtraUsersMap : NopEntityTypeConfiguration<AppointmentExtraUsers>
    {
        public AppointmentExtraUsersMap()
        {
            ToTable("AppointmentExtraUsers");
            HasKey(a => a.Id);
        }
    }
}