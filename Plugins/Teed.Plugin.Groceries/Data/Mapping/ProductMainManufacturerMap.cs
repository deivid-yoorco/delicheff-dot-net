using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.Product;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class ProductMainManufacturerMap : NopEntityTypeConfiguration<ProductMainManufacturer>
    {
        public ProductMainManufacturerMap()
        {
            ToTable(nameof(ProductMainManufacturer));
            HasKey(x => x.Id);
        }
    }
}