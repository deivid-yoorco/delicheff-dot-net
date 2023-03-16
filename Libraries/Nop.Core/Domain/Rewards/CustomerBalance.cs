using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Rewards
{
    /// <summary>
    /// Represents a customer balance
    /// </summary>
    public partial class CustomerBalance : BaseEntity
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
        /// Gets or sets the identifier of the customer balance
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the order identifier connected to the given balance, if needed
        /// </summary>
        public int? OrderId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the customer that gives the new balance
        /// </summary>
        public int GivenByCustomerId { get; set; }

        /// <summary>
        /// Gets or sets the comment for the balance
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the log of changes for this badge, saved concatenated
        /// </summary>
        public string Log { get; set; }

        #endregion
    }
}
