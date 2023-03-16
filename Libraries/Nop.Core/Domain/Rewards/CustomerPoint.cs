using System;
using System.Collections.Generic;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;

namespace Nop.Core.Domain.Rewards
{
    /// <summary>
    /// Represents a customer point
    /// </summary>
    public partial class CustomerPoint : BaseEntity
    {
        #region Properties

        /// <summary>
        /// Gets or sets created date in UTC time
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets last updated date in UTC time
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets value for deleted
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the customer connection Id, to whom the points belong
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the value of the order id if reward points where given by order amount
        /// </summary>
        public int? OrderId { get; set; }

        /// <summary>
        /// Gets or sets the points quantity/value
        /// </summary>
        public decimal Points { get; set; }

        /// <summary>
        /// Gets or sets the description of why this points are given
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets date in UTC of when the points expire
        /// </summary>
        public DateTime? ExpirationDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the system name for the reward, used to locate given rewards
        /// </summary>
        public string RewardSystemName { get; set; }

        /// <summary>
        /// Gets or sets the value to specify if points where given as a challenge reward
        /// </summary>
        public bool IsChallenge { get; set; }

        #endregion

        #region Navigation properties

        /// <summary>
        /// Gets or sets the customer
        /// </summary>
        public virtual Customer Customer { get; set; }

        #endregion
    }
}