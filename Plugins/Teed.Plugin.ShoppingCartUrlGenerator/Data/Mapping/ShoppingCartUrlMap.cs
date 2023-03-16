using Nop.Data.Mapping;
using Teed.Plugin.ShoppingCartUrlGenerator.Domain;

namespace Teed.Plugin.ShoppingCartUrlGenerator.Data.Mapping
{
    public class ShoppingCartUrlMap : NopEntityTypeConfiguration<ShoppingCartUrl>
    {
        public ShoppingCartUrlMap()
        {
            ToTable(nameof(ShoppingCartUrl));
            HasKey(x => x.Id);
        }
    }
}