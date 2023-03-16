using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.UrbanPromoters;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class UrbanPromoterFeeMap : NopEntityTypeConfiguration<UrbanPromoterFee>
    {
        public UrbanPromoterFeeMap()
        {
            ToTable("UrbanPromoterFees");
            HasKey(x => x.Id);

            this.HasRequired(x => x.UrbanPromoter)
                .WithMany(x => x.UrbanPromoterFees)
                .HasForeignKey(x => x.UrbanPromoterId)
                .WillCascadeOnDelete(false);
        }
    }
}
