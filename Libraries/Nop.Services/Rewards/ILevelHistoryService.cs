using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Rewards;

namespace Nop.Services.Rewards
{
    /// <summary>
    /// Level service interface
    /// </summary>
    public partial interface ILevelHistoryService
    {
        /// <summary>
        /// Gets a level history
        /// </summary>
        /// <param name="levelHistoryId">The level history identifier</param>
        /// <returns>Level</returns>
        LevelHistory GetLevelHistoryById(int levelHistoryId);

        /// <summary>
        /// Get level histories by customer, action or/and level identifiers
        /// </summary>
        /// <param name="customerId">Customer identifiers</param>
        /// <returns>Levels</returns>
        IList<LevelHistory> GetLevelHistoriesByCustomerId(
            int customerId = 0, int actionId = 0, int levelId = 0);

        IList<LevelHistory> GetLevelHistories();

        IQueryable<LevelHistory> GetLevelHistoriesQuery();

        /// <summary>
        /// Deletes a level history
        /// </summary>
        /// <param name="level">The level history</param>
        void DeleteLevel(LevelHistory levelHistory);

        /// <summary>
        /// Inserts a level history
        /// </summary>
        /// <param name="levelHistory">The level history</param>
        void InsertLevel(LevelHistory levelHistory);

        /// <summary>
        /// Updates the level history
        /// </summary>
        /// <param name="levelHistory">The level history</param>
        void UpdateLevelHistory(LevelHistory levelHistory);
    }
}
