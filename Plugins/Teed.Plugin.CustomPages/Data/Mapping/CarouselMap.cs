using Nop.Data.Mapping;
using Teed.Plugin.CustomPages.Domain.CustomPages;

namespace Teed.Plugin.CustomPages.Data.Mapping
{
    public class CarouselMap : NopEntityTypeConfiguration<Carousel>
    {
        public CarouselMap()
        {
            ToTable("Carousel");
            HasKey(x => x.Id);
        }
    }
}