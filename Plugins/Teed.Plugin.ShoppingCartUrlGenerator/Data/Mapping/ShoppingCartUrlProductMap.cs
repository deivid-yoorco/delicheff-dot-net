using Nop.Data.Mapping;
using Teed.Plugin.ShoppingCartUrlGenerator.Domain;

namespace Teed.Plugin.ShoppingCartUrlGenerator.Data.Mapping
{
    public class ShoppingCartUrlProductMap : NopEntityTypeConfiguration<ShoppingCartUrlProduct>
    {
        public ShoppingCartUrlProductMap()
        {
            ToTable(nameof(ShoppingCartUrlProduct));
            HasKey(x => x.Id);

            HasRequired(a => a.ShoppingCartUrl)
                .WithMany(x => x.ShoppingCartUrlProducts)
                .HasForeignKey(x => x.ShoppingCartUrlId);
        }
    }
}