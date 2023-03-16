using Nop.Data.Mapping;
using Teed.Plugin.Payroll.Domain.Incidents;

namespace Teed.Plugin.Payroll.Data.Mapping
{
    public class IncidentMap : NopEntityTypeConfiguration<Incident>
    {
        public IncidentMap()
        {
            ToTable("Incidents");
            HasKey(x => x.Id);

            this.HasRequired(x => x.PayrollEmployee)
                .WithMany(x => x.Incidents)
                .HasForeignKey(x => x.PayrollEmployeeId)
                .WillCascadeOnDelete(false);
        }
    }
}