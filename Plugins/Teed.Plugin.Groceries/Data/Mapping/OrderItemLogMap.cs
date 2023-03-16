using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class OrderItemLogMap : NopEntityTypeConfiguration<OrderItemLog>
    {
        public OrderItemLogMap()
        {
            this.ToTable(nameof(OrderItemLog));
            this.HasKey(orderItem => orderItem.Id);

            this.Property(orderItem => orderItem.NewPrice).HasPrecision(18, 4);
            this.Property(orderItem => orderItem.OriginalPrice).HasPrecision(18, 4);
        }
    }
}
