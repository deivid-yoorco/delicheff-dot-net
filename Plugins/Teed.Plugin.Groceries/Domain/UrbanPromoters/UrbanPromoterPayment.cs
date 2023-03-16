using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.UrbanPromoters
{
    public class UrbanPromoterPayment : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual DateTime PaymentDateUtc { get; set; }
        public virtual decimal PaymentAmount { get; set; }
        public virtual string Comment { get; set; }
        public virtual string VoucherExtension { get; set; }
        public virtual byte[] VoucherFile { get; set; }

        public virtual UrbanPromoter UrbanPromoter { get; set; }
        public virtual int UrbanPromoterId { get; set; }
    }
}