using Nop.Data.Mapping;
using Teed.Plugin.Manager.Domain.PurchaseOrders;

namespace Teed.Plugin.Manager.Data.Mapping
{
    public class PurchaseOrderMap : NopEntityTypeConfiguration<PurchaseOrder>
    {
        public PurchaseOrderMap()
        {
            ToTable("PurchaseOrder");
            HasKey(m => m.Id);
        }
    }
}