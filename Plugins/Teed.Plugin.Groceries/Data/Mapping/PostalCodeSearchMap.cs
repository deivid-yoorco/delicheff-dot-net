using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.ShippingAreas;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class PostalCodeSearchMap : NopEntityTypeConfiguration<PostalCodeSearch>
    {
        public PostalCodeSearchMap()
        {
            ToTable(nameof(PostalCodeSearch));
            HasKey(x => x.Id);
            Property(x => x.PostalCode).HasMaxLength(5).IsRequired();
        }
    }
}
