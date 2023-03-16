using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class ManagerExpensesMap : NopEntityTypeConfiguration<ManagerExpense>
    {
        public ManagerExpensesMap()
        {
            ToTable("ManagerExpenses");
            HasKey(x => x.Id);
        }
    }
}