using Nop.Data.Mapping;
using Teed.Plugin.Manager.Domain.PartnerLiabilities;

namespace Teed.Plugin.Manager.Data.Mapping
{
    public class PartnerLiabilityMap : NopEntityTypeConfiguration<PartnerLiability>
    {
        public PartnerLiabilityMap()
        {
            ToTable("PartnerLiability");
            HasKey(x => x.Id);
        }
    }
}