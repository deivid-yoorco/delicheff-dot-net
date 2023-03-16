using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.FestiveDates;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class FestiveDateMap : NopEntityTypeConfiguration<FestiveDate>
    {
        public FestiveDateMap()
        {
            ToTable("FestiveDates");
            HasKey(x => x.Id);
        }
    }
}