using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;

namespace Nop.Core.Domain.Discounts
{
    /// <summary>
    /// Represents a discount being used in current calculation of discounted amounts
    /// </summary>
    public partial class DiscountAppliedTemp
    {
        /// <summary>
        /// Gets or sets the original id of the discount
        /// </summary>
        public int DiscountId { get; set; }

        /// <summary>
        /// Gets or sets the current discounted amount
        /// </summary>
        public decimal AmountDiscounted { get; set; }
    }
}
