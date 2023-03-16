using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Branches;

namespace Teed.Plugin.Medical.Data.Mapping
{
    public class OfficeMap : NopEntityTypeConfiguration<Office>
    {
        public OfficeMap()
        {
            ToTable("Office");
            HasKey(o => o.Id);

            Property(m => m.Name).HasMaxLength(256).IsRequired();
            Property(m => m.BranchId).IsRequired();
        }
    }
}
