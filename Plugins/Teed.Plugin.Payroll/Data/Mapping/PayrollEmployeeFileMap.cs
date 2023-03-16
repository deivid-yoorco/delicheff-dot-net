using Nop.Data.Mapping;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Data.Mapping
{
    public class PayrollEmployeeFileMap : NopEntityTypeConfiguration<PayrollEmployeeFile>
    {
        public PayrollEmployeeFileMap()
        {
            ToTable("PayrollEmployeeFiles");
            HasKey(x => x.Id);

            this.HasRequired(x => x.PayrollEmployee)
                .WithMany()
                .HasForeignKey(x => x.PayrollEmployeeId);
        }
    }
}