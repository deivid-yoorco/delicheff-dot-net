using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Models.Customer
{
    public class ShareAndWinModel : BaseNopModel
    {
        public string CustomerCode { get; set; }

        public string RewardAmount { get; set; }

        public string CouponValue { get; set; }

        public decimal UserCodeCouponOrderMinimumAmount { get; set; }

        public decimal MinimumAmountToCreateFriendCode { get; set; }

        public bool CouponDesactivated { get; set; }
    }
}
