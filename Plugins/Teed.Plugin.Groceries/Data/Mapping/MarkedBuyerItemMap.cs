using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.MarkedBuyerItems;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    class MarkedBuyerItemMap : NopEntityTypeConfiguration<MarkedBuyerItem>
    {
        public MarkedBuyerItemMap()
        {
            ToTable("MarkedBuyerItem");
            HasKey(x => x.Id);
        }
    }
}
