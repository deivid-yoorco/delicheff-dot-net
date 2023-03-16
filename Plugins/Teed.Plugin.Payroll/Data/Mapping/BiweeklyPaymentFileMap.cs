using Nop.Data.Mapping;
using Teed.Plugin.Payroll.Domain.BiweeklyPayments;

namespace Teed.Plugin.Payroll.Data.Mapping
{
    public class BiweeklyPaymentFileMap : NopEntityTypeConfiguration<BiweeklyPaymentFile>
    {
        public BiweeklyPaymentFileMap()
        {
            ToTable("BiweeklyPaymentFiles");
            HasKey(x => x.Id);

            this.HasRequired(x => x.BiweeklyPayment)
                .WithMany(x => x.BiweeklyPaymentFiles)
                .HasForeignKey(x => x.BiweeklyPaymentId)
                .WillCascadeOnDelete(false);
        }
    }
}