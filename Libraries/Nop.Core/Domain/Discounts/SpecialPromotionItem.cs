using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;

namespace Nop.Core.Domain.Discounts
{
    /// <summary>
    /// Represents a discount being used in current calculation of discounted amounts
    /// </summary>
    public partial class SpecialPromotionItem
    {
        /// <summary>
        /// Gets or sets the original id of the product
        /// </summary>
        public virtual int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the original id with its index position appended
        /// </summary>
        public virtual string IdCount { get; set; }

        /// <summary>
        /// Gets or sets the original name of the product
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the original price of the product
        /// </summary>
        public virtual decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the shopping carts original XML values
        /// </summary>
        public virtual string AttributesXml { get; set; }

        /// <summary>
        /// Gets or sets the special discount it got applied by if any
        /// </summary>
        public virtual int AppliedId { get; set; }
    }
}
