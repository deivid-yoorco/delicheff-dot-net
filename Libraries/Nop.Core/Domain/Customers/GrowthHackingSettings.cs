using Nop.Core.Configuration;

namespace Nop.Core.Domain.Customers
{
    public class GrowthHackingSettings : ISettings
    {
        /// <summary>
        /// Gets or sets if Growth Hacking is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the user code coupon amount
        /// </summary>
        public decimal UserCodeCouponAmount { get; set; }

        /// <summary>
        /// Gets or sets the reward amount
        /// </summary>
        public decimal RewardAmount { get; set; }

        /// <summary>
        /// Gets or sets User Code Coupon Order Minimum Amount
        /// </summary>
        public decimal UserCodeCouponOrderMinimumAmount { get; set; }

        /// <summary>
        /// Gets or sets Reward Order Minimum Amount
        /// </summary>
        public decimal RewardOrderMinimumAmount { get; set; }

        /// <summary>
        /// Gets or sets the reward valid days
        /// </summary>
        public int RewardValidDays { get; set; }

        /// <summary>
        /// Gets or sets the Minimum Amount To Create Friend Code
        /// </summary>
        public decimal MinimumAmountToCreateFriendCode { get; set; }
    }
}