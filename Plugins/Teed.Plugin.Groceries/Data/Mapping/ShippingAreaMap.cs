using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingAreas;
using Teed.Plugin.Groceries.Domain.WebScrapingUnitProduct;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class ShippingAreaMap : NopEntityTypeConfiguration<ShippingArea>
    {
        public ShippingAreaMap()
        {
            ToTable("ShippingAreas");
            HasKey(x => x.Id);
        }
    }
}