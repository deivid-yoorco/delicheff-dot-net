using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Facturify.Domain;

namespace Teed.Plugin.Facturify.Data.Mapping
{
    public class CustomerBillingAddressMap : NopEntityTypeConfiguration<CustomerBillingAddress>
    {
        public CustomerBillingAddressMap()
        {
            ToTable("CustomerBillingAddress");
            HasKey(x => x.Id);
        }
    }
}
