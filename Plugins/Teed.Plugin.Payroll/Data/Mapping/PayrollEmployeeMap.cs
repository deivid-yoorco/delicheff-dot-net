using Nop.Data.Mapping;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Data.Mapping
{
    public class PayrollEmployeeMap : NopEntityTypeConfiguration<PayrollEmployee>
    {
        public PayrollEmployeeMap()
        {
            ToTable("PayrollEmployees");
            HasKey(x => x.Id);
            
            this.HasRequired(x => x.JobCatalog)
                .WithMany(x => x.PayrollEmployees)
                .HasForeignKey(x => x.JobCatalogId)
                .WillCascadeOnDelete(false);
        }
    }
}