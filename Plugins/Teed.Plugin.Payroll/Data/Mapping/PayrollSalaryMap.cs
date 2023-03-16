using Nop.Data.Mapping;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Data.Mapping
{
    public class PayrollSalaryMap : NopEntityTypeConfiguration<PayrollSalary>
    {
        public PayrollSalaryMap()
        {
            ToTable("PayrollSalarys");
            HasKey(x => x.Id);

            this.HasRequired(x => x.PayrollEmployee)
                .WithMany(x => x.PayrollSalaries)
                .HasForeignKey(x => x.PayrollEmployeeId)
                .WillCascadeOnDelete(false);
        }
    }
}