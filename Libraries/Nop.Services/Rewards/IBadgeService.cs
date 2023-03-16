using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Rewards;

namespace Nop.Services.Rewards
{
    /// <summary>
    /// Badge service interface
    /// </summary>
    public partial interface IBadgeService
    {
        /// <summary>
        /// Gets a badge
        /// </summary>
        /// <param name="badgeId">The badge identifier</param>
        /// <returns>Badge</returns>
        Badge GetBadgeById(int badgeId);

        /// <summary>
        /// Get badges by identifiers
        /// </summary>
        /// <param name="badgeIds">badges identifiers</param>
        /// <returns>Badge</returns>
        IList<Badge> GetBadgesByIds(int[] badgeIds);

        /// <summary>
        /// Get badges by element identifier
        /// </summary>
        /// <param name="elementId">Element identifier</param>
        /// <returns>Badge</returns>
        IList<Badge> GetBadgesByElementId(int elementId);

        IList<Badge> GetBadges();

        IQueryable<Badge> GetBadgesQuery();

        /// <summary>
        /// Deletes a badge
        /// </summary>
        /// <param name="badge">The badge</param>
        void DeleteBadge(Badge badge);

        /// <summary>
        /// Inserts a badge
        /// </summary>
        /// <param name="badge">Badge</param>
        void InsertBadge(Badge badge);

        /// <summary>
        /// Updates the badge
        /// </summary>
        /// <param name="badge">The badge</param>
        void UpdateBadge(Badge badge);
    }
}
