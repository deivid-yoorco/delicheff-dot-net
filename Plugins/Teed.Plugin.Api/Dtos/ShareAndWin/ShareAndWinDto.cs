using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.ShareAndWin
{
    public class ShareAndWinDto
    {
        public decimal CouponValue { get; set; }
        public decimal RewardAmount { get; set; }
        public string CustomerCode { get; set; }
        public decimal MinimumAmountToCreateFriendCode { get; set; }
        public decimal MinimumAmountToUseFriendCode { get; set; }
    }
}
