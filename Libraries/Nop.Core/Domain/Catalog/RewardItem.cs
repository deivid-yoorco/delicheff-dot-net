using System;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a RewardItem
    /// </summary>
    public partial class RewardItem : BaseEntity
    {
        /// <summary>
        /// Gets or sets the ProductId
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the Quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the BuyingBySecondary
        /// </summary>
        public bool BuyingBySecondary { get; set; }

        /// <summary>
        /// Gets or sets the SelectedPropertyOption
        /// </summary>
        public string SelectedPropertyOption { get; set; }

        /// <summary>
        /// Gets or sets the IsActive
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the Log
        /// </summary>
        public string Log { get; set; }

        /// <summary>
        /// Gets or sets the CreateOnUtc
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the UpdatedOnUtc
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the Delete
        /// </summary>
        public bool Deleted { get; set; }
    }
}
