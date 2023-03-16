using Nop.Data.Mapping;
using Teed.Plugin.Manager.Domain.Expenses;
using Teed.Plugin.Manager.Domain.ExpensesCategories;

namespace Teed.Plugin.Manager.Data.Mapping
{
    public class ExpenseMap : NopEntityTypeConfiguration<Expense>
    {
        public ExpenseMap()
        {
            ToTable("Expenses");
            HasKey(x => x.Id);
        }
    }
}