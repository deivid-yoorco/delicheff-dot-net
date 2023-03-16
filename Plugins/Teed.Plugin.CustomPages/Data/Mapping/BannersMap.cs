using Nop.Data.Mapping;
using Teed.Plugin.CustomPages.Domain.CustomPages;

namespace Teed.Plugin.CustomPages.Data.Mapping
{
    public class BannersMap : NopEntityTypeConfiguration<Banners>
    {
        public BannersMap()
        {
            ToTable("Banners");
            HasKey(x => x.Id);
        }
    }
}