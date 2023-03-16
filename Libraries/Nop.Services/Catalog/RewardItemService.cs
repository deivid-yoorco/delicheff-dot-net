using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Events;
using System;
using System.Linq;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Category service
    /// </summary>
    public partial class RewardItemService : IRewardItemService
    {
        #region Fields

        private readonly IRepository<RewardItem> _rewardItemRepository;
        private readonly IDbContext _dbContext;
        private readonly IDataProvider _dataProvider;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly CatalogSettings _catalogSettings;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="rewardItemRepository">Repository RewardItem</param>
        /// <param name="dbContext">DB context</param>
        /// <param name="dataProvider">Data provider</param>
        /// <param name="workContext">Work context</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="catalogSettings">Catalog settings</param>
        public RewardItemService(IRepository<RewardItem> rewardItemRepository,
            IDbContext dbContext,
            IDataProvider dataProvider,
            IWorkContext workContext,
            IStoreContext storeContext,
            CatalogSettings catalogSettings,
            IEventPublisher eventPublisher)
        {
            this._rewardItemRepository = rewardItemRepository;
            this._dbContext = dbContext;
            this._dataProvider = dataProvider;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._catalogSettings = catalogSettings;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        /// <summary>
        /// Delete RewardItem
        /// </summary>
        /// <param name="rewardItem">RewardItem</param>
        public virtual void DeleteRewardItem(RewardItem rewardItem)
        {
            if (rewardItem == null)
                throw new ArgumentNullException(nameof(rewardItem));

            rewardItem.Deleted = true;
            UpdateRewardItem(rewardItem);
        }

        /// <summary>
        /// Get all products by RewardItem query
        /// </summary>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        public virtual IQueryable<RewardItem> GetAllRewardItemQuery(bool includeDeleted = false)
        {
            var query = _rewardItemRepository.Table;
            if (!includeDeleted)
                query = query.Where(x => !x.Deleted);
            return query;
        }

        /// <summary>
        /// Gets product by RewardItem
        /// </summary>
        /// <param name="id">RewardItem identifier</param>
        /// <returns>RewardItem</returns>
        public virtual RewardItem GetRewardItemById(int id)
        {
            if (id == 0)
                return null;

            return _rewardItemRepository.GetById(id);
        }

        // <summary>
        /// Gets a RewardItem
        /// </summary>
        /// <param name="id">Product identifier</param>
        /// <returns>RewardItem</returns>
        public virtual RewardItem GetRewardItemByProductId(int id)
        {
            if (id == 0)
                return null;

            return _rewardItemRepository.Table.Where(x => x.ProductId == id && !x.Deleted).FirstOrDefault();
        }

        /// <summary>
        /// Inserts RewardItem
        /// </summary>
        /// <param name="rewardItem">RewardItem</param>
        public virtual void InsertRewardItem(RewardItem rewardItem)
        {
            if (rewardItem == null)
                throw new ArgumentNullException(nameof(rewardItem));

            DateTime now = DateTime.UtcNow;
            rewardItem.CreatedOnUtc = now;
            rewardItem.UpdatedOnUtc = now;

            _rewardItemRepository.Insert(rewardItem);
            _eventPublisher.EntityInserted(rewardItem);
        }

        /// <summary>
        /// Updates the RewardItem
        /// </summary>
        /// <param name="rewardItem">RewardItem</param>
        public virtual void UpdateRewardItem(RewardItem rewardItem)
        {
            if (rewardItem == null)
                throw new ArgumentNullException(nameof(rewardItem));

            DateTime now = DateTime.UtcNow;
            rewardItem.UpdatedOnUtc = now;
            //update
            _rewardItemRepository.Update(rewardItem);
            _eventPublisher.EntityInserted(rewardItem);
        }
    }
}