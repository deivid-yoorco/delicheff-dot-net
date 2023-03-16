using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.Franchise;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class PaymentFileMap : NopEntityTypeConfiguration<PaymentFile>
    {
        public PaymentFileMap()
        {
            ToTable("FranchisePaymentFiles");
            HasKey(x => x.Id);
            
            HasRequired(a => a.Payment)
                .WithMany(x => x.PaymentFiles)
                .HasForeignKey(x => x.PaymentId);
        }
    }
}