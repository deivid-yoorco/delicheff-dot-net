using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Rewards
{
    /// <summary>
    /// Represents a level history
    /// </summary>
    public partial class LevelHistory : BaseEntity
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
        /// Gets or sets the Id of the customer
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the action type Id
        /// </summary>
        public int ActionId { get; set; }

        /// <summary>
        /// Gets or sets the necesary amount to achive the level
        /// </summary>
        public int LevelId { get; set; }

        #endregion

        #region Navigation properties

        /// <summary>
        /// Gets or sets the customer
        /// </summary>
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets the level
        /// </summary>
        public virtual Level Level { get; set; }

        #endregion
    }
}
