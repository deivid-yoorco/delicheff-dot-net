using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.BuyerPayments;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class BuyerPaymentByteFileMap : NopEntityTypeConfiguration<BuyerPaymentByteFile>
    {
        public BuyerPaymentByteFileMap()
        {
            ToTable(nameof(BuyerPaymentByteFile));
            HasKey(x => x.Id);
        }
    }
}