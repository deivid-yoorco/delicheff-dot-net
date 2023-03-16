using Nop.Data.Mapping;
using Teed.Plugin.Manager.Domain.ExpensesCategories;

namespace Teed.Plugin.Manager.Data.Mapping
{
    public class ExpensesCategoriesMap : NopEntityTypeConfiguration<ExpensesCategories>
    {
        public ExpensesCategoriesMap()
        {
            ToTable("ExpensesCategories");
            HasKey(x => x.Id);
        }
    }
}