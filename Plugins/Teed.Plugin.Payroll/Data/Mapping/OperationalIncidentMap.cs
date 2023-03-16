using Nop.Data.Mapping;
using Teed.Plugin.Payroll.Domain.Incidents;

namespace Teed.Plugin.Payroll.Data.Mapping
{
    public class OperationalIncidentMap : NopEntityTypeConfiguration<OperationalIncident>
    {
        public OperationalIncidentMap()
        {
            ToTable("OperationalIncidents");
            HasKey(x => x.Id);
        }
    }
}