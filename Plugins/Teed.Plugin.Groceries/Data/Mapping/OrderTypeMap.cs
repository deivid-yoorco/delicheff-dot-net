using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class OrderTypeMap : NopEntityTypeConfiguration<OrderType>
    {
        public OrderTypeMap()
        {
            ToTable(nameof(OrderType));
            HasKey(x => x.Id);
        }
    }
}
