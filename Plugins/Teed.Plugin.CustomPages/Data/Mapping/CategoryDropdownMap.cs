using Nop.Data.Mapping;
using Teed.Plugin.CustomPages.Domain.CustomPages;

namespace Teed.Plugin.CustomPages.Data.Mapping
{
    public class CategoryDropdownMap : NopEntityTypeConfiguration<CategoryDropdown>
    {
        public CategoryDropdownMap()
        {
            ToTable("CategoryDropdown");
            HasKey(x => x.Id);
        }
    }
}