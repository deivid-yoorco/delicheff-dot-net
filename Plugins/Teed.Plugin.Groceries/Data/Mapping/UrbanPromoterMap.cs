using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.UrbanPromoters;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class UrbanPromoterMap : NopEntityTypeConfiguration<UrbanPromoter>
    {
        public UrbanPromoterMap()
        {
            ToTable("UrbanPromoters");
            HasKey(x => x.Id);
        }
    }
}
