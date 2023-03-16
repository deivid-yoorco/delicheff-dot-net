using Nop.Data.Mapping;
using Teed.Plugin.CustomPages.Domain.CustomPages;

namespace Teed.Plugin.CustomPages.Data.Mapping
{
    public class CollageMap : NopEntityTypeConfiguration<Collage>
    {
        public CollageMap()
        {
            ToTable("Collage");
            HasKey(x => x.Id);
        }
    }
}