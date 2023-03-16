using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.ShoppingCartUrlGenerator.Domain;

namespace Teed.Plugin.ShoppingCartUrlGenerator.Data.Mapping
{
    public class ShoppingCartUrlUserMap : NopEntityTypeConfiguration<ShoppingCartUrlUser>
    {
        public ShoppingCartUrlUserMap()
        {
            ToTable(nameof(ShoppingCartUrlUser));
            HasKey(x => x.Id);
        }
    }
}
