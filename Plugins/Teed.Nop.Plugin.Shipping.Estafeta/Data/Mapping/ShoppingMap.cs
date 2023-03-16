using Nop.Data.Mapping;
using Nop.Plugin.Shipping.Estafeta.Domain.Shopping;

namespace Nop.Plugin.Shipping.Estafeta.Data.Mapping
{
    public class ShoppingMap : NopEntityTypeConfiguration<Shopping>
    {
        public ShoppingMap()
        {
            ToTable("Estafeta");

            //Map the primary key
            HasKey(m => m.Id);
            Property(m => m.GuidId);
            Property(m => m.Name).HasMaxLength(256).IsRequired();
            Property(m => m.GuideNumber).HasMaxLength(256);
            Property(m => m.TrackingCode).HasMaxLength(15);
            Property(m => m.Deleted);
            Property(m => m.Updated);
            Property(m => m.Created);
            Property(m => m.guiaPdf);
            Property(m => m.numOrder).HasMaxLength(15);
        }
    }
}
