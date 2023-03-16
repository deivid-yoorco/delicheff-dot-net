using Nop.Data.Mapping;
using Teed.Plugin.Payroll.Domain.BiweeklyPayments;

namespace Teed.Plugin.Payroll.Data.Mapping
{
    public class BiweeklyPaymentMap : NopEntityTypeConfiguration<BiweeklyPayment>
    {
        public BiweeklyPaymentMap()
        {
            ToTable("BiweeklyPayments");
            HasKey(x => x.Id);

            this.HasRequired(x => x.PayrollEmployee)
                .WithMany()
                .HasForeignKey(x => x.PayrollEmployeeId);

            this.HasRequired(x => x.PayrollSalary)
                .WithMany()
                .HasForeignKey(x => x.PayrollSalaryId);
        }
    }
}