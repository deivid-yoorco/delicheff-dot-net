using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class OrderReportLogMap : NopEntityTypeConfiguration<OrderReportLog>
    {
        public OrderReportLogMap()
        {
            ToTable("OrderReportLog");
            HasKey(x => x.Id);
        }
    }
}