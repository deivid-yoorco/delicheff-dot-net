using Nop.Core.Configuration;

namespace Nop.Core.Domain.Rewards
{
    /// <summary>
    /// Rewards settings
    /// </summary>
    public class RewardSettings : ISettings
    {
        #region Form fields

        /// <summary>
        /// Gets or sets a value indicating if rewards system is active or not in general
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether level points are enabled or not
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the image Id for level points
        /// </summary>
        public int ImageId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the title for level points
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the description to show for level points, rich text
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the quantity of months to calculate levels
        /// </summary>
        public int LevelMonthsCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the number of points given at first login with Facebook
        /// </summary>
        public decimal FacebookPoints { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the nmber of points given for each currency value after orders
        /// </summary>
        public decimal OrderPoints { get; set; }

        #endregion
    }
}