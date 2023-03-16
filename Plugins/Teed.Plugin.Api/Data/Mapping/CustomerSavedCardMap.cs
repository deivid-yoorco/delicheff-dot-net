using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Domain.Payment;

namespace Teed.Plugin.Api.Data.Mapping
{
    public class CustomerSavedCardMap : NopEntityTypeConfiguration<CustomerSavedCard>
    {
        public CustomerSavedCardMap()
        {
            ToTable(nameof(CustomerSavedCard));
            HasKey(m => m.Id);
        }
    }
}