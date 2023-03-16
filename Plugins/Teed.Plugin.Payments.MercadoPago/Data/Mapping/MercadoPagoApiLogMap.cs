using Nop.Data.Mapping;
using Teed.Plugin.Payments.MercadoPago.Domain;

namespace Teed.Plugin.Payments.MercadoPago.Data.Mapping
{
    public class MercadoPagoApiLogMap : NopEntityTypeConfiguration<MercadoPagoApiLog>
    {
        public MercadoPagoApiLogMap()
        {
            ToTable(nameof(MercadoPagoApiLog));
            HasKey(x => x.Id);
        }
    }
}