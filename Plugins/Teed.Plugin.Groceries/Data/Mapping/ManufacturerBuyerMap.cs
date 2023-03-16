using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.OrderItemBuyers;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class ManufacturerBuyerMap : NopEntityTypeConfiguration<ManufacturerBuyer>
    {
        public ManufacturerBuyerMap()
        {
            ToTable(nameof(ManufacturerBuyer));
            HasKey(x => x.Id);
        }
    }
}