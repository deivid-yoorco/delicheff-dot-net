using Nop.Data.Mapping;
using Teed.Plugin.ShippingByAddress.Domain.ShippingBranch;

namespace Teed.Plugin.ShippingByAddress.Data.Mapping
{
    public class ShippingBranchOrderMap : NopEntityTypeConfiguration<ShippingBranchOrder>
    {
        public ShippingBranchOrderMap()
        {
            ToTable("ShippingBranchOrder");
            HasKey(x => x.Id);
        }
    }
}