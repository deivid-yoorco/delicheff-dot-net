using Nop.Data.Mapping;
using Teed.Plugin.Medical.Domain;
using Teed.Plugin.Medical.Domain.Prescription;

namespace Teed.Plugin.Medical.Data.Mapping
{
    public class PrescriptionMap : NopEntityTypeConfiguration<Prescription>
    {
        public PrescriptionMap()
        {
            ToTable("Prescriptions");
            HasKey(m => m.Id); // Primary Key

            Property(m => m.Comment).HasMaxLength(1000);
        }
    }
}