using Nop.Data.Mapping;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Data.Mapping
{
    public class SubordinateMap : NopEntityTypeConfiguration<Subordinate>
    {
        public SubordinateMap()
        {
            ToTable("Subordinates");
            HasKey(x => x.Id);

            this.HasRequired(x => x.Boss)
                .WithMany()
                .HasForeignKey(x => x.BossId)
                .WillCascadeOnDelete(false);
            this.HasRequired(x => x.PayrollSubordinate)
                .WithMany()
                .HasForeignKey(x => x.PayrollSubordinateId)
                .WillCascadeOnDelete(false);
        }
    }
}