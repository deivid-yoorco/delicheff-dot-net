using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Rewards;
using Nop.Services.Events;

namespace Nop.Services.Rewards
{
    /// <summary>
    /// Level history service
    /// </summary>
    public partial class LevelHistoryService : ILevelHistoryService
    {
        #region Fields

        private readonly IRepository<LevelHistory> _levelHistoryRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="levelHistoryRepository">Level repository</param>
        /// <param name="eventPublisher">Event publisher service</param>
        public LevelHistoryService(IRepository<LevelHistory> levelHistoryRepository,
            IEventPublisher eventPublisher)
        {
            this._levelHistoryRepository = levelHistoryRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a level history
        /// </summary>
        /// <param name="levelHistoryId">The level history identifier</param>
        /// <returns>Level</returns>
        public virtual LevelHistory GetLevelHistoryById(int levelHistoryId)
        {
            if (levelHistoryId == 0)
                return null;

            return _levelHistoryRepository.GetById(levelHistoryId);
        }

        /// <summary>
        /// Get level histories by customer, action or/and level identifiers
        /// </summary>
        /// <param name="customerId">Customer identifiers</param>
        /// <returns>Levels</returns>
        public virtual IList<LevelHistory> GetLevelHistoriesByCustomerId(
            int customerId = 0, int actionId = 0, int levelId = 0)
        {
            if (customerId == 0 && actionId == 0 && levelId == 0)
                return new List<LevelHistory>();

            var levelHistories = from o in _levelHistoryRepository.Table select o;
            if (customerId > 0)
                levelHistories = levelHistories.Where(x => x.CustomerId == customerId);
                if (actionId > 0)
                levelHistories = levelHistories.Where(x => x.ActionId == actionId);
            if (levelId > 0)
                levelHistories = levelHistories.Where(x => x.LevelId == levelId);
            return levelHistories.ToList();
        }

        public virtual IList<LevelHistory> GetLevelHistories()
        {
            return _levelHistoryRepository.Table.Where(o => !o.Deleted).ToList();
        }

        public virtual IQueryable<LevelHistory> GetLevelHistoriesQuery()
        {
            return _levelHistoryRepository.Table;
        }

        /// <summary>
        /// Deletes a level history
        /// </summary>
        /// <param name="levelHistory">The level history</param>
        public virtual void DeleteLevel(LevelHistory levelHistory)
        {
            if (levelHistory == null)
                throw new ArgumentNullException(nameof(levelHistory));

            levelHistory.Deleted = true;
            UpdateLevelHistory(levelHistory);

            //event notification
            _eventPublisher.EntityDeleted(levelHistory);
        }

        /// <summary>
        /// Inserts a level history
        /// </summary>
        /// <param name="levelHistory">The level history</param>
        public virtual void InsertLevel(LevelHistory levelHistory)
        {
            if (levelHistory == null)
                throw new ArgumentNullException(nameof(levelHistory));
            levelHistory.CreatedOnUtc = DateTime.UtcNow;
            levelHistory.UpdatedOnUtc = DateTime.UtcNow;

            _levelHistoryRepository.Insert(levelHistory);

            //event notification
            _eventPublisher.EntityInserted(levelHistory);
        }

        /// <summary>
        /// Updates the level history
        /// </summary>
        /// <param name="levelHistory">The level history</param>
        public virtual void UpdateLevelHistory(LevelHistory levelHistory)
        {
            if (levelHistory == null)
                throw new ArgumentNullException(nameof(levelHistory));
            levelHistory.UpdatedOnUtc = DateTime.UtcNow;

            _levelHistoryRepository.Update(levelHistory);

            //event notification
            _eventPublisher.EntityUpdated(levelHistory);
        }
        
        #endregion
    }
}
