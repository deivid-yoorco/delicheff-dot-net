using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payments.PaypalPlus.Domain;

namespace Teed.Plugin.Payments.PaypalPlus.Data.Mapping
{
    public class RememberedCardsMap : NopEntityTypeConfiguration<RememberedCards>
    {
        public RememberedCardsMap()
        {
            ToTable("PaypalPlusRememberedCards");
            HasKey(x => x.Id);
        }
    }
}
