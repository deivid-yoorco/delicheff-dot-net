using Nop.Data.Mapping;
using Teed.Plugin.Manager.Domain.CashExpenses;
using Teed.Plugin.Manager.Domain.Expenses;
using Teed.Plugin.Manager.Domain.ExpensesCategories;

namespace Teed.Plugin.Manager.Data.Mapping
{
    public class CashExpenseMap : NopEntityTypeConfiguration<CashExpense>
    {
        public CashExpenseMap()
        {
            ToTable("CashExpenses");
            HasKey(x => x.Id);
        }
    }
}