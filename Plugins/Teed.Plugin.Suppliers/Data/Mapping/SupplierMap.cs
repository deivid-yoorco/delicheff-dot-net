using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Suppliers.Domain;

namespace Teed.Plugin.Suppliers.Data.Mapping
{
    public class SupplierMap : NopEntityTypeConfiguration<Supplier>
    {
        public SupplierMap()
        {
            ToTable("Supplier");
            HasKey(x => x.Id);

            //HasRequired(x => x.StateProvince)
            //    .WithMany()
            //    .HasForeignKey(x => x.StateProvinceId);

            //HasRequired(x => x.Country)
            //    .WithMany()
            //    .HasForeignKey(x => x.CountryId);
        }
    }
}
