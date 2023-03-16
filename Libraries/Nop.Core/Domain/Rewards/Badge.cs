using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Rewards
{
    /// <summary>
    /// Represents a badge
    /// </summary>
    public partial class Badge : BaseEntity
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
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description for the badge
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the image Id for Bronze level
        /// </summary>
        public int BronzeImageId { get; set; }

        /// <summary>
        /// Gets or sets the amount needed for Bronze level
        /// </summary>
        public decimal BronzeAmount { get; set; }

        /// <summary>
        /// Gets or sets the image Id for silver level
        /// </summary>
        public int SilverImageId { get; set; }

        /// <summary>
        /// Gets or sets the amount needed for silver level
        /// </summary>
        public decimal SilverAmount { get; set; }

        /// <summary>
        /// Gets or sets the image Id for gold level
        /// </summary>
        public int GoldImageId { get; set; }

        /// <summary>
        /// Gets or sets the amount needed for gold level
        /// </summary>
        public decimal GoldAmount { get; set; }

        /// <summary>
        /// Gets or sets the type Id of elements that are used for this badge
        /// </summary>
        public int ElementTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Ids of elements that apply for this badge, saved as string separated by comas
        /// </summary>
        public string ElementIds { get; set; }

        /// <summary>
        /// Gets or sets the log of changes for this badge, saved concatenated
        /// </summary>
        public string Log { get; set; }

        #endregion
    }
}
