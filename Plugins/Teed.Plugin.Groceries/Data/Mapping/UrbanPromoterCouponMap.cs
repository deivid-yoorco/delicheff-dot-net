using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.UrbanPromoters;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class UrbanPromoterCouponMap : NopEntityTypeConfiguration<UrbanPromoterCoupon>
    {
        public UrbanPromoterCouponMap()
        {
            ToTable("UrbanPromoterCoupons");
            HasKey(x => x.Id);

            this.HasRequired(x => x.UrbanPromoter)
                .WithMany(x => x.UrbanPromoterCoupons)
                .HasForeignKey(x => x.UrbanPromoterId)
                .WillCascadeOnDelete(false);
        }
    }
}
