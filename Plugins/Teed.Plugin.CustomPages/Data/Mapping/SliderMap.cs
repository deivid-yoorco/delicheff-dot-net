using Nop.Data.Mapping;
using Teed.Plugin.CustomPages.Domain.CustomPages;

namespace Teed.Plugin.CustomPages.Data.Mapping
{
    public class SliderMap : NopEntityTypeConfiguration<Slider>
    {
        public SliderMap()
        {
            ToTable("Slider");
            HasKey(x => x.Id);
        }
    }
}