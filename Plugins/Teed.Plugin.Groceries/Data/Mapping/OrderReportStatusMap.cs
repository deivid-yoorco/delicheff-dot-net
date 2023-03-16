using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class OrderReportStatusMap : NopEntityTypeConfiguration<OrderReportStatus>
    {
        public OrderReportStatusMap()
        {
            ToTable(nameof(OrderReportStatus));
            HasKey(x => x.Id);
        }
    }
}
