using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Branches;

namespace Teed.Plugin.Medical.Data.Mapping
{
    public class HolidayMap : NopEntityTypeConfiguration<Holiday>
    {
        public HolidayMap()
        {
            ToTable("Holiday");
            HasKey(m => m.Id);

            Property(m => m.Name).HasMaxLength(256).IsRequired();
            Property(m => m.Date).IsRequired();
        }
    }
}
