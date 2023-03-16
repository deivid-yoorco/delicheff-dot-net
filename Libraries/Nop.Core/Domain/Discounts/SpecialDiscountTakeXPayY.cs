using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;

namespace Nop.Core.Domain.Discounts
{
    /// <summary>
    /// Represents a special discount type "Take X, pay Y"
    /// </summary>
    public partial class SpecialDiscountTakeXPayY : BaseEntity
    {
        /// <summary>
        /// Gets or sets the date and time of order creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of order update
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the id of the discount
        /// </summary>
        public int DiscountId { get; set; }

        /// <summary>
        /// Gets or sets the type of entity to check for this discount
        /// </summary>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity's id
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets amount the user takes in the promotion
        /// </summary>
        public int TakeAmount { get; set; }

        /// <summary>
        /// Gets or sets amount the user pays in the promotion
        /// </summary>
        public int PayAmount { get; set; }

        /// <summary>
        /// Gets or sets if promotion is active or not
        /// </summary>
        public bool IsAcitve { get; set; }

        /// <summary>
        /// Gets or sets the log of changes
        /// </summary>
        public string Log { get; set; }
    }
}
