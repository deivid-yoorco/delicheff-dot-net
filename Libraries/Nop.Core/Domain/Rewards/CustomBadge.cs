using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Rewards
{
    /// <summary>
    /// Represents a customer badge
    /// </summary>
    public partial class CustomerBadge : BaseEntity
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
        /// Gets or sets the customer connection Id, to whom the badge belong
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the badge connection Id
        /// </summary>
        public int BadgeId { get; set; }

        /// <summary>
        /// Gets or sets the badge level b integer
        /// </summary>
        public int BadgeLevel { get; set; }

        /// <summary>
        /// Gets or sets the log of changes for this customer badge, saved concatenated
        /// </summary>
        public string Log { get; set; }

        #endregion

        #region Navigation properties

        /// <summary>
        /// Gets or sets the customer
        /// </summary>
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets the badge
        /// </summary>
        public virtual Badge Badge { get; set; }

        #endregion
    }
}
