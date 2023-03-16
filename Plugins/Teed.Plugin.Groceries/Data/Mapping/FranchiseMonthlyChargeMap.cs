using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.Franchise;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class FranchiseMonthlyChargeMap : NopEntityTypeConfiguration<FranchiseMonthlyCharge>
    {
        public FranchiseMonthlyChargeMap()
        {
            ToTable(nameof(FranchiseMonthlyCharge));
            HasKey(x => x.Id);
        }
    }
}
