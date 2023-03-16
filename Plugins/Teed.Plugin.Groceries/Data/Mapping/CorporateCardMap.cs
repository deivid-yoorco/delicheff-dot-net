using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.BuyerPayments;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class CorporateCardMap : NopEntityTypeConfiguration<CorporateCard>
    {
        public CorporateCardMap()
        {
            ToTable(nameof(CorporateCard));
            HasKey(x => x.Id);
        }
    }
}