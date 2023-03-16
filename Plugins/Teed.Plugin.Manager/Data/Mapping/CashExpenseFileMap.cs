using Nop.Data.Mapping;
using Teed.Plugin.Manager.Domain.CashExpenses;

namespace Teed.Plugin.Manager.Data.Mapping
{
    public class CashExpenseFileMap : NopEntityTypeConfiguration<CashExpenseFile>
    {
        public CashExpenseFileMap()
        {
            ToTable("CashExpenseFiles");
            HasKey(x => x.Id);
        }
    }
}