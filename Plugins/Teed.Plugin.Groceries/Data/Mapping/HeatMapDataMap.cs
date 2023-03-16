using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.HeatMaps;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class HeatMapDataMap : NopEntityTypeConfiguration<HeatMapData>
    {
        public HeatMapDataMap()
        {
            ToTable(nameof(HeatMapData));
            HasKey(x => x.Id);
        }
    }
}
