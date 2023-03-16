using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Branches;

namespace Teed.Plugin.Medical.Data.Mapping
{
    public class BranchMap : NopEntityTypeConfiguration<Branch>
    {
        public BranchMap()
        {
            ToTable("Branch");
            HasKey(m => m.Id);

            Property(m => m.Name).HasMaxLength(256).IsRequired();
            Property(m => m.Phone).HasMaxLength(256).IsRequired();
            Property(m => m.Phone2).HasMaxLength(256);
            Property(m => m.StreetAddress).HasMaxLength(500).IsRequired();
            Property(m => m.StreetAddress2).HasMaxLength(500);
            Property(m => m.City).HasMaxLength(256).IsRequired();
            Property(m => m.ZipPostalCode).HasMaxLength(256).IsRequired();
            Property(m => m.WeekOpenTime).IsRequired();
            Property(m => m.WeekCloseTime).IsRequired();

        }
    }
}
