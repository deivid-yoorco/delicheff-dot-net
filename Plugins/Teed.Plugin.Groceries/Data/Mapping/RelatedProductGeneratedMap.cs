using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.Product;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class RelatedProductGeneratedMap : NopEntityTypeConfiguration<RelatedProductGenerated>
    {
        public RelatedProductGeneratedMap()
        {
            ToTable(nameof(RelatedProductGenerated));
            HasKey(x => x.Id);
        }
    }
}