using Nop.Data.Mapping;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Data.Mapping
{
    public class TakenVacationDayMap : NopEntityTypeConfiguration<TakenVacationDay>
    {
        public TakenVacationDayMap()
        {
            ToTable("TakenVacationDays");
            HasKey(x => x.Id);
        }
    }
}