using Nop.Data.Mapping;
using Teed.Plugin.CustomPages.Domain.CustomPages;

namespace Teed.Plugin.CustomPages.Data.Mapping
{
    public class CustomPageMap : NopEntityTypeConfiguration<CustomPage>
    {
        public CustomPageMap()
        {
            ToTable("CustomPage");
            HasKey(x => x.Id);
        }
    }
}