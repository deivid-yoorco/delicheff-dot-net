using Nop.Data.Mapping;
using Teed.Plugin.Manager.Domain.PurchaseOrders;

namespace Teed.Plugin.Manager.Data.Mapping
{
    public class PurchaseOrderProductMap : NopEntityTypeConfiguration<PurchaseOrderProduct>
    {
        public PurchaseOrderProductMap()
        {
            ToTable("PurchaseOrderProduct");
            HasKey(o => o.Id);
        }
    }
}