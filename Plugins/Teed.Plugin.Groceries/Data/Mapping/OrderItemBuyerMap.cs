using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.OrderItemBuyers;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class OrderItemBuyerMap : NopEntityTypeConfiguration<OrderItemBuyer>
    {
        public OrderItemBuyerMap()
        {
            ToTable(nameof(OrderItemBuyer));
            HasKey(x => x.Id);
        }
    }
}