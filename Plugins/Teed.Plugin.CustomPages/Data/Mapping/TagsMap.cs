using Nop.Data.Mapping;
using Teed.Plugin.CustomPages.Domain.CustomPages;

namespace Teed.Plugin.CustomPages.Data.Mapping
{
    public class TagsMap : NopEntityTypeConfiguration<Tags>
    {
        public TagsMap()
        {
            ToTable("Tags");
            HasKey(x => x.Id);
        }
    }
}