using Nop.Data.Mapping;
using Teed.Plugin.Payroll.Domain.JobCatalogs;

namespace Teed.Plugin.Payroll.Data.Mapping
{
    public class JobCatalogMap : NopEntityTypeConfiguration<JobCatalog>
    {
        public JobCatalogMap()
        {
            ToTable("JobCatalogs");
            HasKey(x => x.Id);


            //this.HasOptional(x => x.ParentJob)
            //    .WithMany()
            //    .HasForeignKey(x => x.ParentJobId)
            //    .WillCascadeOnDelete(false);
        }
    }
}