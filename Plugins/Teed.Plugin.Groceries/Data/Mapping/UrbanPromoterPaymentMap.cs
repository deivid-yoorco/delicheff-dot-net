using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.UrbanPromoters;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class UrbanPromoterPaymentMap : NopEntityTypeConfiguration<UrbanPromoterPayment>
    {
        public UrbanPromoterPaymentMap()
        {
            ToTable("UrbanPromoterPayments");
            HasKey(x => x.Id);

            this.HasRequired(x => x.UrbanPromoter)
                .WithMany(x => x.UrbanPromoterPayments)
                .HasForeignKey(x => x.UrbanPromoterId)
                .WillCascadeOnDelete(false);
        }
    }
}
