using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Rewards;
using Nop.Services.Events;

namespace Nop.Services.Rewards
{
    /// <summary>
    /// Badge service
    /// </summary>
    public partial class BadgeService : IBadgeService
    {
        #region Fields

        private readonly IRepository<Badge> _badgeRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="badgeRepository">Badge repository</param>
        /// <param name="eventPublisher">Event publisher service</param>
        public BadgeService(IRepository<Badge> badgeRepository,
            IEventPublisher eventPublisher)
        {
            this._badgeRepository = badgeRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a badge
        /// </summary>
        /// <param name="badgeId">The badge identifier</param>
        /// <returns>Badge</returns>
        public virtual Badge GetBadgeById(int badgeId)
        {
            if (badgeId == 0)
                return null;

            return _badgeRepository.GetById(badgeId);
        }

        /// <summary>
        /// Get badges by identifiers
        /// </summary>
        /// <param name="badgeIds">badges identifiers</param>
        /// <returns>Badge</returns>
        public virtual IList<Badge> GetBadgesByIds(int[] badgeIds)
        {
            if (badgeIds == null || badgeIds.Length == 0)
                return new List<Badge>();

            var query = from o in _badgeRepository.Table
                        where badgeIds.Contains(o.Id) && !o.Deleted
                        select o;
            var badges = query.ToList();
            //sort by passed identifiers
            var sortedBadges = new List<Badge>();
            foreach (var id in badgeIds)
            {
                var badge = badges.Find(x => x.Id == id);
                if (badge != null)
                    sortedBadges.Add(badge);
            }
            return sortedBadges;
        }

        /// <summary>
        /// Get badges by element identifier
        /// </summary>
        /// <param name="elementId">Element identifier</param>
        /// <returns>Badge</returns>
        public virtual IList<Badge> GetBadgesByElementId(int elementId)
        {
            if (elementId == 0)
                return new List<Badge>();

            var query = from o in _badgeRepository.Table
                        where o.ElementIds.Split(',').Contains(elementId.ToString())  && !o.Deleted
                        select o;
            var badges = query.ToList();
            return badges;
        }

        public virtual IList<Badge> GetBadges()
        {
            return _badgeRepository.Table.Where(o => !o.Deleted).ToList();
        }

        public virtual IQueryable<Badge> GetBadgesQuery()
        {
            return _badgeRepository.Table;
        }

        /// <summary>
        /// Deletes a badge
        /// </summary>
        /// <param name="badge">The badge</param>
        public virtual void DeleteBadge(Badge badge)
        {
            if (badge == null)
                throw new ArgumentNullException(nameof(badge));

            badge.Deleted = true;
            UpdateBadge(badge);

            //event notification
            _eventPublisher.EntityDeleted(badge);
        }

        /// <summary>
        /// Inserts a badge
        /// </summary>
        /// <param name="badge">Badge</param>
        public virtual void InsertBadge(Badge badge)
        {
            if (badge == null)
                throw new ArgumentNullException(nameof(badge));
            badge.CreatedOnUtc = DateTime.UtcNow;
            badge.UpdatedOnUtc = DateTime.UtcNow;

            _badgeRepository.Insert(badge);

            //event notification
            _eventPublisher.EntityInserted(badge);
        }

        /// <summary>
        /// Updates the badge
        /// </summary>
        /// <param name="badge">The badge</param>
        public virtual void UpdateBadge(Badge badge)
        {
            if (badge == null)
                throw new ArgumentNullException(nameof(badge));
            badge.UpdatedOnUtc = DateTime.UtcNow;

            _badgeRepository.Update(badge);

            //event notification
            _eventPublisher.EntityUpdated(badge);
        }
        
        #endregion
    }
}
