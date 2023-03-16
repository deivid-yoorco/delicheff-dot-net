using Nop.Core.Domain.Catalog;
using System.Linq;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Product service
    /// </summary>
    public partial interface IRewardItemService
    {
        /// <summary>
        /// Delete RewardItem
        /// </summary>
        /// <param name="rewardItem">RewardItem</param>
        void DeleteRewardItem(RewardItem rewardItem);

        /// <summary>
        /// Get all products query
        /// </summary>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        IQueryable<RewardItem> GetAllRewardItemQuery(bool includeDeleted = false);

        // <summary>
        /// Gets a RewardItem
        /// </summary>
        /// <param name="id">RewardItem identifier</param>
        /// <returns>RewardItem</returns>
        RewardItem GetRewardItemById(int id);

        // <summary>
        /// Gets a RewardItem
        /// </summary>
        /// <param name="id">Product identifier</param>
        /// <returns>RewardItem</returns>
        RewardItem GetRewardItemByProductId(int id);

        /// <summary>
        /// Inserts RewardItem
        /// </summary>
        /// <param name="rewardItem">RewardItem</param>
        void InsertRewardItem(RewardItem rewardItem);

        /// <summary>
        /// Updates the RewardItem
        /// </summary>
        /// <param name="rewardItem">RewardItem</param>
        void UpdateRewardItem(RewardItem rewardItem);
    }
}
