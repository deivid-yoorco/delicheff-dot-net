using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.Franchise;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class BillingMap : NopEntityTypeConfiguration<Billing>
    {
        public BillingMap()
        {
            ToTable("FranchiseBillings");
            HasKey(x => x.Id);

            Property(x => x.BaseFixedRateCDMX).HasPrecision(18, 6);
            Property(x => x.BaseFixedRateOtherStates).HasPrecision(18, 6);
            Property(x => x.BaseVariableRateCDMX).HasPrecision(18, 6);
            Property(x => x.BaseVariableRateOtherStates).HasPrecision(18, 6);
            Property(x => x.FixedWeeklyBonusCeroIncidents).HasPrecision(18, 6);
            Property(x => x.VariableWeeklyBonusCeroIncidents).HasPrecision(18, 6);

            HasRequired(a => a.Vehicle)
                .WithMany()
                .HasForeignKey(x => x.VehicleId);
        }
    }
}