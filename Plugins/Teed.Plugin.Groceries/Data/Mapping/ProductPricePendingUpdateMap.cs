using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.Product;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class ProductPricePendingUpdateMap : NopEntityTypeConfiguration<ProductPricePendingUpdate>
    {
        public ProductPricePendingUpdateMap()
        {
            ToTable(nameof(ProductPricePendingUpdate));
            HasKey(x => x.Id);
        }
    }
}