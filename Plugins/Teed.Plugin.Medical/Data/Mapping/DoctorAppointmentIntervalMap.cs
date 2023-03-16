using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Doctor;

namespace Teed.Plugin.Medical.Data.Mapping
{
    public class DoctorAppointmentIntervalMap : NopEntityTypeConfiguration<DoctorAppointmentInterval>
    {
        public DoctorAppointmentIntervalMap()
        {
            ToTable("DoctorAppointmentIntervals");
            HasKey(m => m.Id); // Primary Key
        }
    }
}