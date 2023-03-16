using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class OrderReportMap : NopEntityTypeConfiguration<OrderReport>
    {
        public OrderReportMap()
        {
            ToTable("OrderReport");
            HasKey(x => x.Id);
            this.Property(x => x.UpdatedQuantity).HasPrecision(18, 4);
            this.Property(x => x.UpdatedRequestedQtyCost).HasPrecision(18, 4);
            this.Property(x => x.UpdatedUnitCost).HasPrecision(18, 4);
        }
    }
}