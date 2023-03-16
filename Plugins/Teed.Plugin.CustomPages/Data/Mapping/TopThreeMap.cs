using Nop.Data.Mapping;
using Teed.Plugin.CustomPages.Domain.CustomPages;

namespace Teed.Plugin.CustomPages.Data.Mapping
{
    public class TopThreeMap : NopEntityTypeConfiguration<TopThree>
    {
        public TopThreeMap()
        {
            ToTable("TopThree");
            HasKey(x => x.Id);
        }
    }
}