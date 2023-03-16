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
    /// Customer balance service
    /// </summary>
    public partial class CustomerBalanceService : ICustomerBalanceService
    {
        #region Fields

        private readonly IRepository<CustomerBalance> _customerBalanceRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="customerBalanceRepository">Customer balance repository</param>
        /// <param name="eventPublisher">Event publisher service</param>
        public CustomerBalanceService(IRepository<CustomerBalance> customerBalanceRepository,
            IEventPublisher eventPublisher)
        {
            this._customerBalanceRepository = customerBalanceRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a Customer balance
        /// </summary>
        /// <param name="customerBalanceId">The customer balance identifier</param>
        /// <returns>CustomerBalance</returns>
        public virtual CustomerBalance GetCustomerBalanceById(int customerBalanceId)
        {
            if (customerBalanceId == 0)
                return null;

            return _customerBalanceRepository.GetById(customerBalanceId);
        }

        /// <summary>
        /// Gets all Customer balances by customer identifier
        /// </summary>
        /// <param name="customerId">The customer identifier</param>
        /// <returns>List<CustomerBalance></returns>
        public virtual IList<CustomerBalance> GetAllCustomerBalanceByCustomerId(int customerId)
        {
            if (customerId == 0)
                return null;

            return GetAllCustomerBalancesQuery().Where(x => !x.Deleted && x.CustomerId == customerId).ToList();
        }

        public virtual IList<CustomerBalance> GetAllCustomerBalances()
        {
            return _customerBalanceRepository.Table.Where(o => !o.Deleted).ToList();
        }

        public virtual IQueryable<CustomerBalance> GetAllCustomerBalancesQuery()
        {
            return _customerBalanceRepository.Table;
        }

        /// <summary>
        /// Deletes a customer balance
        /// </summary>
        /// <param name="customerBalance">The customer balance</param>
        public virtual void DeleteCustomerBalance(CustomerBalance customerBalance)
        {
            if (customerBalance == null)
                throw new ArgumentNullException(nameof(customerBalance));

            customerBalance.Deleted = true;
            UpdateCustomerBalance(customerBalance);

            //event notification
            _eventPublisher.EntityDeleted(customerBalance);
        }

        /// <summary>
        /// Inserts a customer balance
        /// </summary>
        /// <param name="customerBalance">CustomerBalance</param>
        public virtual void InsertCustomerBalance(CustomerBalance customerBalance)
        {
            if (customerBalance == null)
                throw new ArgumentNullException(nameof(customerBalance));
            customerBalance.CreatedOnUtc = DateTime.UtcNow;
            customerBalance.UpdatedOnUtc = DateTime.UtcNow;

            _customerBalanceRepository.Insert(customerBalance);

            //event notification
            _eventPublisher.EntityInserted(customerBalance);
        }

        /// <summary>
        /// Updates the customer balance
        /// </summary>
        /// <param name="customerBalance">The customer balance</param>
        public virtual void UpdateCustomerBalance(CustomerBalance customerBalance)
        {
            if (customerBalance == null)
                throw new ArgumentNullException(nameof(customerBalance));
            customerBalance.UpdatedOnUtc = DateTime.UtcNow;

            _customerBalanceRepository.Update(customerBalance);

            //event notification
            _eventPublisher.EntityUpdated(customerBalance);
        }
        
        #endregion
    }
}
