using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.UrbanPromoters
{
    public class UrbanPromoterCoupon : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int DiscountId { get; set; }
        public virtual decimal Fee { get; set; }

        public virtual UrbanPromoter UrbanPromoter { get; set; }
        public virtual int UrbanPromoterId { get; set; }
    }
}