using Nop.Data.Mapping;
using Teed.Plugin.CustomPages.Domain.CustomPages;

namespace Teed.Plugin.CustomPages.Data.Mapping
{
    public class PopUpMap : NopEntityTypeConfiguration<PopUp>
    {
        public PopUpMap()
        {
            ToTable("PopUp");
            HasKey(x => x.Id);
        }
    }
}