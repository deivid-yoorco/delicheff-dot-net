﻿using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.OrderItemBuyers;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class SupermarketBuyerMap : NopEntityTypeConfiguration<SupermarketBuyer>
    {
        public SupermarketBuyerMap()
        {
            ToTable(nameof(SupermarketBuyer));
            HasKey(x => x.Id);
        }
    }
}
