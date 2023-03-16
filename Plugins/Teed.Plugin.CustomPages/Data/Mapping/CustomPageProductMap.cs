using Nop.Data.Mapping;
using Teed.Plugin.CustomPages.Domain.CustomPages;

namespace Teed.Plugin.CustomPages.Data.Mapping
{
    public class CustomPageProductMap : NopEntityTypeConfiguration<CustomPageProduct>
    {
        public CustomPageProductMap()
        {
            ToTable("CustomPageProduct");
            HasKey(x => x.Id);
        }
    }
}