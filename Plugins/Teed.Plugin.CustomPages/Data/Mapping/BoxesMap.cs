using Nop.Data.Mapping;
using Teed.Plugin.CustomPages.Domain.CustomPages;

namespace Teed.Plugin.CustomPages.Data.Mapping
{
    public class BoxesMap : NopEntityTypeConfiguration<Boxes>
    {
        public BoxesMap()
        {
            ToTable("Boxes");
            HasKey(x => x.Id);
        }
    }
}