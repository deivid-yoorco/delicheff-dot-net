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
    /// Customer Badge service
    /// </summary>
    public partial class CustomerBadgeService : ICustomerBadgeService
    {
        #region Fields

        private readonly IRepository<CustomerBadge> _customerBadgeRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="customerBadgeRepository">Customer Badge repository</param>
        /// <param name="eventPublisher">Event publisher service</param>
        public CustomerBadgeService(IRepository<CustomerBadge> customerBadgeRepository,
            IEventPublisher eventPublisher)
        {
            this._customerBadgeRepository = customerBadgeRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a customer badge
        /// </summary>
        /// <param name="customerBadgeId">The customer badge identifier</param>
        /// <returns>CustomerBadge</returns>
        public virtual CustomerBadge GetCustomerBadgeById(int customerBadgeId)
        {
            if (customerBadgeId == 0)
                return null;

            return _customerBadgeRepository.GetById(customerBadgeId);
        }

        /// <summary>
        /// Get customer badges by identifiers
        /// </summary>
        /// <param name="customerBadgeIds">Customer badges identifiers</param>
        /// <returns>CustomerBadge</returns>
        public virtual IList<CustomerBadge> GetCustomerBadgesByIds(int[] customerBadgeIds)
        {
            if (customerBadgeIds == null || customerBadgeIds.Length == 0)
                return new List<CustomerBadge>();

            var query = from o in _customerBadgeRepository.Table
                        where customerBadgeIds.Contains(o.Id) && !o.Deleted
                        select o;
            var customerBadges = query.ToList();
            //sort by passed identifiers
            var sortedCustomerBadges = new List<CustomerBadge>();
            foreach (var id in customerBadgeIds)
            {
                var customerBadge = customerBadges.Find(x => x.Id == id);
                if (customerBadge != null)
                    sortedCustomerBadges.Add(customerBadge);
            }
            return sortedCustomerBadges;
        }

        /// <summary>
        /// Get customer badges by customer identifier
        /// </summary>
        /// <param name="customerId">Customer identifiers</param>
        /// <returns>CustomerBadge</returns>
        public virtual IList<CustomerBadge> GetCustomerBadgesByCustomerId(int customerId)
        {
            if (customerId == 0)
                return new List<CustomerBadge>();

            var query = from o in _customerBadgeRepository.Table
                        where customerId == o.CustomerId && !o.Deleted
                        select o;
            var customerBadges = query.ToList();
            return customerBadges;
        }

        /// <summary>
        /// Get customer badges by customer identifier
        /// </summary>
        /// <param name="badgeId">Badge identifiers</param>
        /// <returns>CustomerBadge</returns>
        public virtual IList<CustomerBadge> GetCustomerBadgesByBadgeId(int badgeId)
        {
            if (badgeId == 0)
                return new List<CustomerBadge>();

            var query = from o in _customerBadgeRepository.Table
                        where badgeId == o.BadgeId && !o.Deleted
                        select o;
            var customerBadges = query.ToList();
            return customerBadges;
        }

        public virtual IList<CustomerBadge> GetCustomerBadges()
        {
            return _customerBadgeRepository.Table.Where(o => !o.Deleted).ToList();
        }

        public virtual IQueryable<CustomerBadge> GetCustomerBadgesQuery()
        {
            return _customerBadgeRepository.Table;
        }

        /// <summary>
        /// Deletes a customer badge
        /// </summary>
        /// <param name="customerBadge">The customer badge</param>
        public virtual void DeleteCustomerBadge(CustomerBadge customerBadge)
        {
            if (customerBadge == null)
                throw new ArgumentNullException(nameof(customerBadge));

            customerBadge.Deleted = true;
            UpdateCustomerBadge(customerBadge);

            //event notification
            _eventPublisher.EntityDeleted(customerBadge);
        }

        /// <summary>
        /// Inserts a customer badge
        /// </summary>
        /// <param name="customerBadge">CustomerBadge</param>
        public virtual void InsertCustomerBadge(CustomerBadge customerBadge)
        {
            if (customerBadge == null)
                throw new ArgumentNullException(nameof(customerBadge));
            customerBadge.CreatedOnUtc = DateTime.UtcNow;
            customerBadge.UpdatedOnUtc = DateTime.UtcNow;

            _customerBadgeRepository.Insert(customerBadge);

            //event notification
            _eventPublisher.EntityInserted(customerBadge);
        }

        /// <summary>
        /// Updates the customer badge
        /// </summary>
        /// <param name="customerBadge">The customer badge</param>
        public virtual void UpdateCustomerBadge(CustomerBadge customerBadge)
        {
            if (customerBadge == null)
                throw new ArgumentNullException(nameof(customerBadge));
            customerBadge.UpdatedOnUtc = DateTime.UtcNow;

            _customerBadgeRepository.Update(customerBadge);

            //event notification
            _eventPublisher.EntityUpdated(customerBadge);
        }
        
        #endregion
    }
}
