using Nop.Data.Mapping;
using Teed.Plugin.CustomPages.Domain.CustomPages;

namespace Teed.Plugin.CustomPages.Data.Mapping
{
    public class ParallaxMap : NopEntityTypeConfiguration<Parallax>
    {
        public ParallaxMap()
        {
            ToTable("Parallax");
            HasKey(x => x.Id);
        }
    }
}