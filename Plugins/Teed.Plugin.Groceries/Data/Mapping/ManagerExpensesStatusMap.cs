using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class ManagerExpensesStatusMap : NopEntityTypeConfiguration<ManagerExpensesStatus>
    {
        public ManagerExpensesStatusMap()
        {
            ToTable("ManagerExpensesStatus");
            HasKey(x => x.Id);
        }
    }
}