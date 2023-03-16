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
    /// Level service
    /// </summary>
    public partial class LevelService : ILevelService
    {
        #region Fields

        private readonly IRepository<Level> _levelRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="levelRepository">Level repository</param>
        /// <param name="eventPublisher">Event publisher service</param>
        public LevelService(IRepository<Level> levelRepository,
            IEventPublisher eventPublisher)
        {
            this._levelRepository = levelRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a level
        /// </summary>
        /// <param name="levelId">The level identifier</param>
        /// <returns>Level</returns>
        public virtual Level GetLevelById(int levelId)
        {
            if (levelId == 0)
                return null;

            return _levelRepository.GetById(levelId);
        }

        /// <summary>
        /// Get levels by customer role identifier
        /// </summary>
        /// <param name="customerRoleId">Customer role identifiers</param>
        /// <returns>Levels</returns>
        public virtual IList<Level> GetLevelsByCustomerRoleId(int customerRoleId)
        {
            if (customerRoleId == 0)
                return new List<Level>();

            var query = from o in _levelRepository.Table
                        where customerRoleId == o.CustomerRoleId && !o.Deleted
                        select o;
            var levels = query.ToList();
            return levels;
        }

        /// <summary>
        /// Get levels by customer roles identifier
        /// </summary>
        /// <param name="customerRoleIds">Customer roles identifiers</param>
        /// <returns>Levels</returns>
        public virtual IList<Level> GetLevelsByCustomerRoleIds(int[] customerRoleIds)
        {
            if (customerRoleIds.Count() > 0)
                return new List<Level>();

            var query = from o in _levelRepository.Table
                        where customerRoleIds.Contains(o.CustomerRoleId) && !o.Deleted
                        select o;
            var levels = query.ToList();
            return levels;
        }

        public virtual IList<Level> GetLevels()
        {
            return _levelRepository.Table.Where(o => !o.Deleted).ToList();
        }

        public virtual IQueryable<Level> GetLevelsQuery()
        {
            return _levelRepository.Table;
        }

        /// <summary>
        /// Deletes a level
        /// </summary>
        /// <param name="level">The level</param>
        public virtual void DeleteLevel(Level level)
        {
            if (level == null)
                throw new ArgumentNullException(nameof(level));

            level.Deleted = true;
            UpdateLevel(level);

            //event notification
            _eventPublisher.EntityDeleted(level);
        }

        /// <summary>
        /// Inserts a level
        /// </summary>
        /// <param name="level">The level</param>
        public virtual void InsertLevel(Level level)
        {
            if (level == null)
                throw new ArgumentNullException(nameof(level));
            level.CreatedOnUtc = DateTime.UtcNow;
            level.UpdatedOnUtc = DateTime.UtcNow;

            _levelRepository.Insert(level);

            //event notification
            _eventPublisher.EntityInserted(level);
        }

        /// <summary>
        /// Updates the level
        /// </summary>
        /// <param name="level">The level</param>
        public virtual void UpdateLevel(Level level)
        {
            if (level == null)
                throw new ArgumentNullException(nameof(level));
            level.UpdatedOnUtc = DateTime.UtcNow;

            _levelRepository.Update(level);

            //event notification
            _eventPublisher.EntityUpdated(level);
        }
        
        #endregion
    }
}
