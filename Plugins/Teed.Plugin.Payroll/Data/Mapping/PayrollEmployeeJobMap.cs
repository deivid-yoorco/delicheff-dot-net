using Nop.Data.Mapping;
using Teed.Plugin.Payroll.Domain.PayrollEmployeeJobs;

namespace Teed.Plugin.Payroll.Data.Mapping
{
    public class PayrollEmployeeJobMap : NopEntityTypeConfiguration<PayrollEmployeeJob>
    {
        public PayrollEmployeeJobMap()
        {
            ToTable("PayrollEmployeeJobs");
            HasKey(x => x.Id);

            this.HasRequired(x => x.PayrollEmployee)
                .WithMany(x => x.JobCatalogs)
                .HasForeignKey(x => x.PayrollEmployeeId)
                .WillCascadeOnDelete(false);

            this.HasRequired(x => x.JobCatalog)
                .WithMany(x => x.JobConections)
                .HasForeignKey(x => x.JobCatalogId)
                .WillCascadeOnDelete(false);
        }
    }
}