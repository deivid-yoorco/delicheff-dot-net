using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payments.MercadoPago.Domain;

namespace Teed.Plugin.Payments.MercadoPago.Data.Mapping
{
    public class MercadoPagoTransactionMap : NopEntityTypeConfiguration<MercadoPagoTransaction>
    {
        public MercadoPagoTransactionMap()
        {
            ToTable(nameof(MercadoPagoTransaction));
            HasKey(x => x.Id);
        }
    }
}
