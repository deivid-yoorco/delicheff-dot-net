using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.Corcel;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class CorcelProductMap : NopEntityTypeConfiguration<CorcelProduct>
    {
        public CorcelProductMap()
        {
            ToTable("CorcelProducts");
            HasKey(x => x.Id);
        }
    }
}