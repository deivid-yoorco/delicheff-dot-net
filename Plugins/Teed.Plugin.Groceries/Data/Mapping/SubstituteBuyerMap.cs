using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.SubstituteBuyers;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class SubstituteBuyerMap : NopEntityTypeConfiguration<SubstituteBuyer>
    {
        public SubstituteBuyerMap()
        {
            ToTable("SubstituteBuyers");
            HasKey(x => x.Id);
        }
    }
}