using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.Franchise;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class IncidentMap : NopEntityTypeConfiguration<Incidents>
    {
        public IncidentMap()
        {
            ToTable("FranchiseIncidents");
            HasKey(x => x.Id);
        }
    }
}