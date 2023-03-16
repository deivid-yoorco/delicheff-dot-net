using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Branches;

namespace Teed.Plugin.Medical.Data.Mapping
{
    public class HolidayBranchMap : NopEntityTypeConfiguration<HolidayBranch>
    {
        public HolidayBranchMap()
        {
            ToTable("HolidayBranch");
            HasKey(m => m.Id);

            Property(m => m.BranchId).IsRequired();
            Property(m => m.HolidayId).IsRequired();
        }
    }
}
