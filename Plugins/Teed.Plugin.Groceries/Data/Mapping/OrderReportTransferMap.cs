using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class OrderReportTransferMap : NopEntityTypeConfiguration<OrderReportTransfer>
    {
        public OrderReportTransferMap()
        {
            ToTable(nameof(OrderReportTransfer));
            HasKey(x => x.Id);
        }
    }
}
