using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.Manufacturer;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class ManufacturerBankAccountMap : NopEntityTypeConfiguration<ManufacturerBankAccount>
    {
        public ManufacturerBankAccountMap()
        {
            ToTable(nameof(ManufacturerBankAccount));
            HasKey(x => x.Id);
        }
    }
}
