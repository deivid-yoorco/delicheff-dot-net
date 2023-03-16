using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class OrderOptimizationRequestMap : NopEntityTypeConfiguration<OrderOptimizationRequest>
    {
        public OrderOptimizationRequestMap()
        {
            this.ToTable(nameof(OrderOptimizationRequest));
            this.HasKey(x => x.Id);
        }
    }
}
