using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.TipsWithCards;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class TipsWithCardMap : NopEntityTypeConfiguration<TipsWithCard>
    {
        public TipsWithCardMap()
        {
            ToTable(nameof(TipsWithCard));
            HasKey(x => x.Id);
        }
    }
}