using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.BuyerPayments;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class BuyerPaymentTicketFileMap : NopEntityTypeConfiguration<BuyerPaymentTicketFile>
    {
        public BuyerPaymentTicketFileMap()
        {
            ToTable(nameof(BuyerPaymentTicketFile));
            HasKey(x => x.Id);
        }
    }
}