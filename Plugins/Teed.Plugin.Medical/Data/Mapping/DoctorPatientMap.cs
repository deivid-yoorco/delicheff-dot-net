using Nop.Data.Mapping;
using Teed.Plugin.Medical.Domain;

namespace Teed.Plugin.Medical.Data.Mapping
{
    public class DoctorPatientMap : NopEntityTypeConfiguration<DoctorPatient>
    {
        public DoctorPatientMap()
        {
            ToTable("DoctorPatients");
            HasKey(m => m.Id); // Primary Key
        }
    }
}