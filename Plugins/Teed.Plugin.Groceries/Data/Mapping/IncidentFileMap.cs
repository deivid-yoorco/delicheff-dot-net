using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.Franchise;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class IncidentFileMap : NopEntityTypeConfiguration<IncidentFile>
    {
        public IncidentFileMap()
        {
            ToTable("FranchiseIncidentFiles");
            HasKey(x => x.Id);
            
            HasRequired(a => a.Incident)
                .WithMany(x => x.IncidentFiles)
                .HasForeignKey(x => x.IncidentId);
        }
    }
}