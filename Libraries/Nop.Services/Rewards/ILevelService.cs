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
    public partial interface ILevelService
    {
        /// <summary>
        /// Gets a level
        /// </summary>
        /// <param name="levelId">The level identifier</param>
        /// <returns>Level</returns>
        Level GetLevelById(int levelId);

        /// <summary>
        /// Get levels by customer role identifier
        /// </summary>
        /// <param name="customerRoleId">Customer role identifiers</param>
        /// <returns>Levels</returns>
        IList<Level> GetLevelsByCustomerRoleId(int customerRoleId);

        /// <summary>
        /// Get levels by customer roles identifier
        /// </summary>
        /// <param name="customerRoleIds">Customer roles identifiers</param>
        /// <returns>Levels</returns>
        IList<Level> GetLevelsByCustomerRoleIds(int[] customerRoleIds);

        IList<Level> GetLevels();

        IQueryable<Level> GetLevelsQuery();

        /// <summary>
        /// Deletes a level
        /// </summary>
        /// <param name="level">The level</param>
        void DeleteLevel(Level level);

        /// <summary>
        /// Inserts a level
        /// </summary>
        /// <param name="level">The level</param>
        void InsertLevel(Level level);

        /// <summary>
        /// Updates the level
        /// </summary>
        /// <param name="level">The level</param>
        void UpdateLevel(Level level);
    }
}
