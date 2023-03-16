using Nop.Data.Mapping;
using Teed.Plugin.Manager.Domain.Expenses;
using Teed.Plugin.Manager.Domain.ExpensesCategories;

namespace Teed.Plugin.Manager.Data.Mapping
{
    public class ExpenseFileMap : NopEntityTypeConfiguration<ExpenseFile>
    {
        public ExpenseFileMap()
        {
            ToTable("ExpenseFiles");
            HasKey(x => x.Id);
        }
    }
}