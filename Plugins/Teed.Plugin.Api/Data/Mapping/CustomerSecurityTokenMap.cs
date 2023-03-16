using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Domain.Identity;

namespace Teed.Plugin.Api.Data.Mapping
{
    public class CustomerSecurityTokenMap : NopEntityTypeConfiguration<CustomerSecurityToken>
    {
        public CustomerSecurityTokenMap()
        {
            ToTable("CustomerSecurityToken");
            Property(m => m.CustomerId).IsRequired();
            Property(m => m.Uuid).IsRequired();
        }
    }
}
