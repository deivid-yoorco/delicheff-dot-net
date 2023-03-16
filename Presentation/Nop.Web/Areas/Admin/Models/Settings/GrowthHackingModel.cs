using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Settings
{
    public partial class GrowthHackingModel : BaseNopModel
    {
        public bool IsActive { get; set; }

        public decimal UserCodeCouponAmount { get; set; }

        public decimal RewardAmount { get; set; }

        public decimal UserCodeCouponOrderMinimumAmount { get; set; }

        public decimal RewardOrderMinimumAmount { get; set; }

        public int RewardValidDays { get; set; }

        public decimal MinimumAmountToCreateFriendCode { get; set; }
    }
}
