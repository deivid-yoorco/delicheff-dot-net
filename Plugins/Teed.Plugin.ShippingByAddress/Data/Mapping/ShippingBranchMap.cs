using Nop.Data.Mapping;
using Teed.Plugin.ShippingByAddress.Domain.ShippingBranch;

namespace Teed.Plugin.ShippingByAddress.Data.Mapping
{
    public class ShippingBranchMap : NopEntityTypeConfiguration<ShippingBranch>
    {
        public ShippingBranchMap()
        {
            ToTable("ShippingBranch");
            HasKey(x => x.Id);
        }
    }
}