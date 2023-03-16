using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Facturify.Domain;

namespace Teed.Plugin.Facturify.Data.Mapping
{
    public class ProductSatCodeMap : NopEntityTypeConfiguration<ProductSatCode>
    {
        public ProductSatCodeMap()
        {
            ToTable("ProductsSatCodes");
            HasKey(x => x.Id);
        }
    }
}
