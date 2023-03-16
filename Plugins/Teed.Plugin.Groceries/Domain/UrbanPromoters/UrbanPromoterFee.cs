using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.UrbanPromoters
{
    public class UrbanPromoterFee : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual DateTime FeeGenerationDateUtc { get; set; }
        public virtual decimal FeeAmount { get; set; }
        public virtual int RelatedOrderId { get; set; }

        public virtual UrbanPromoter UrbanPromoter { get; set; }
        public virtual int UrbanPromoterId { get; set; }
    }
}