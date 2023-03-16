using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.BuyerPayments;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class BuyerPaymentMap : NopEntityTypeConfiguration<BuyerPayment>
    {
        public BuyerPaymentMap()
        {
            ToTable(nameof(BuyerPayment));
            HasKey(x => x.Id);
        }
    }
}