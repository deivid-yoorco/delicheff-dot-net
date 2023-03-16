using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.RescheduledOrderLogs;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class RescheduledOrderLogMap : NopEntityTypeConfiguration<RescheduledOrderLog>
    {
        public RescheduledOrderLogMap()
        {
            ToTable(nameof(RescheduledOrderLog));
            HasKey(x => x.Id);
        }
    }
}