using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class OrderReportFileMap : NopEntityTypeConfiguration<OrderReportFile>
    {
        public OrderReportFileMap()
        {
            ToTable("OrderReportFile");
            HasKey(x => x.Id);
        }
    }
}