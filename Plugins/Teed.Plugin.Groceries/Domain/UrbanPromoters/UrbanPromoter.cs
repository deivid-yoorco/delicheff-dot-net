using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.UrbanPromoters
{
    public class UrbanPromoter : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int CustomerId { get; set; }
        public virtual string AccountBankName { get; set; }
        public virtual string AccountOwnerName { get; set; }
        public virtual string AccountAddress { get; set; }
        public virtual string AccountNumber { get; set; }
        public virtual string AccountCLABE { get; set; }
        public virtual bool CashPayment { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual string Log { get; set; }

        public virtual ICollection<UrbanPromoterCoupon> UrbanPromoterCoupons { get; set; }
        public virtual ICollection<UrbanPromoterFee> UrbanPromoterFees { get; set; }
        public virtual ICollection<UrbanPromoterPayment> UrbanPromoterPayments { get; set; }

        public virtual List<UrbanPromoterCoupon> GetUrbanPromoterCoupons()
        {
            return UrbanPromoterCoupons.Where(x => !x.Deleted).ToList();
        }

        public virtual List<UrbanPromoterFee> GetUrbanPromoterFees()
        {
            return UrbanPromoterFees.Where(x => !x.Deleted).ToList();
        }

        public virtual List<UrbanPromoterPayment> GetUrbanPromoterPayment()
        {
            return UrbanPromoterPayments.Where(x => !x.Deleted).ToList();
        }
    }
}