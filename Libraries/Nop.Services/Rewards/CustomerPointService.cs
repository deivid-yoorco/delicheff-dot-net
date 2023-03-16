using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Rewards;
using Nop.Services.Discounts;
using Nop.Services.Events;

namespace Nop.Services.Rewards
{
    /// <summary>
    /// Customer Point service
    /// </summary>
    public partial class CustomerPointService : ICustomerPointService
    {
        #region Fields

        private readonly IRepository<CustomerPoint> _customerPointRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IDiscountService _discountService;
        //private readonly ICustomerPowerUpService _customerPowerUpService;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="customerPointRepository">Customer Point repository</param>
        /// <param name="eventPublisher">Event publisher service</param>
        public CustomerPointService(IRepository<CustomerPoint> customerPointRepository,
            IEventPublisher eventPublisher, IDiscountService discountService
            //,ICustomerPowerUpService customerPowerUpService
            )
        {
            this._customerPointRepository = customerPointRepository;
            this._eventPublisher = eventPublisher;
            this._discountService = discountService;
            //this._customerPowerUpService = customerPowerUpService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a customer point
        /// </summary>
        /// <param name="customerPointId">The customer point identifier</param>
        /// <returns>CustomerPoint</returns>
        public virtual CustomerPoint GetCustomerPointById(int customerPointId)
        {
            if (customerPointId == 0)
                return null;

            return _customerPointRepository.GetById(customerPointId);
        }

        /// <summary>
        /// Get customer points by identifiers
        /// </summary>
        /// <param name="customerPointIds">Customer points identifiers</param>
        /// <returns>CustomerPoint</returns>
        public virtual IList<CustomerPoint> GetCustomerPointsByIds(int[] customerPointIds)
        {
            if (customerPointIds == null || customerPointIds.Length == 0)
                return new List<CustomerPoint>();

            var query = from o in _customerPointRepository.Table
                        where customerPointIds.Contains(o.Id) && !o.Deleted
                        select o;
            var customerPoints = query.ToList();
            //sort by passed identifiers
            var sortedCustomerPoints = new List<CustomerPoint>();
            foreach (var id in customerPointIds)
            {
                var customerPoint = customerPoints.Find(x => x.Id == id);
                if (customerPoint != null)
                    sortedCustomerPoints.Add(customerPoint);
            }
            return sortedCustomerPoints;
        }

        /// <summary>
        /// Get customer points by customer identifier
        /// </summary>
        /// <param name="customerId">Customer identifiers</param>
        /// <returns>CustomerPoint</returns>
        public virtual IList<CustomerPoint> GetCustomerPointsByCustomerId(int customerId)
        {
            if (customerId == 0)
                return new List<CustomerPoint>();

            var query = from o in _customerPointRepository.Table
                        where customerId == o.CustomerId && !o.Deleted
                        select o;
            var customerPoints = query.ToList();
            return customerPoints;
        }

        /// <summary>
        /// Get customer points balance
        /// </summary>
        /// <returns></returns>
        public virtual decimal GetCustomerPointsBalance(int customerId)
        {
            var now = DateTime.UtcNow;
            return _customerPointRepository.TableNoTracking
                .Where(x => !x.Deleted && x.CustomerId == customerId && (!x.ExpirationDateUtc.HasValue || (x.ExpirationDateUtc.HasValue && x.ExpirationDateUtc.Value < now)))
                .Select(x => x.Points)
                .DefaultIfEmpty()
                .Sum();
        }

        public virtual IList<CustomerPoint> GetCustomerPoints()
        {
            return _customerPointRepository.Table.Where(o => !o.Deleted).ToList();
        }

        public virtual IQueryable<CustomerPoint> GetCustomerPointsQuery()
        {
            return _customerPointRepository.Table;
        }

        /// <summary>
        /// Deletes a customer point
        /// </summary>
        /// <param name="customerPoint">The customer point</param>
        public virtual void DeleteCustomerPoint(CustomerPoint customerPoint)
        {
            if (customerPoint == null)
                throw new ArgumentNullException(nameof(customerPoint));

            customerPoint.Deleted = true;
            UpdateCustomerPoint(customerPoint);

            //event notification
            _eventPublisher.EntityDeleted(customerPoint);
        }

        /// <summary>
        /// Inserts a customer point
        /// </summary>
        /// <param name="customerPoint">CustomerPoint</param>
        public virtual void InsertCustomerPoint(CustomerPoint customerPoint)
        {
            if (customerPoint == null)
                throw new ArgumentNullException(nameof(customerPoint));
            customerPoint.CreatedOnUtc = DateTime.UtcNow;
            customerPoint.UpdatedOnUtc = DateTime.UtcNow;

            _customerPointRepository.Insert(customerPoint);

            //AssignPowerUp(customerPoint.CustomerId);

            //event notification
            _eventPublisher.EntityInserted(customerPoint);
        }

        /// <summary>
        /// Updates the customer point
        /// </summary>
        /// <param name="customerPoint">The customer point</param>
        public virtual void UpdateCustomerPoint(CustomerPoint customerPoint)
        {
            if (customerPoint == null)
                throw new ArgumentNullException(nameof(customerPoint));
            customerPoint.UpdatedOnUtc = DateTime.UtcNow;

            _customerPointRepository.Update(customerPoint);

            //event notification
            _eventPublisher.EntityUpdated(customerPoint);
        }

        #endregion

        #region Private methods

        //private void AssignPowerUp(int customerId)
        //{
        //    decimal totalCustomerPoints = GetCustomerPointsByCustomerId(customerId)
        //        .Where(x => x.Points > 0)
        //        .Select(x => x.Points)
        //        .DefaultIfEmpty()
        //        .Sum();

        //    if (totalCustomerPoints == 0) return;
        //    var powerUps = _discountService.GetAllDiscountsQuery().Where(x => x.PowerUpPointsRequired <= totalCustomerPoints);
        //    if (powerUps.Count() > 0)
        //    {
        //        var alreadyAssignedPowerUpCodes = _customerPowerUpService.GetCustomerPowerUpsQuery()
        //            .Where(x => x.CustomerId == customerId)
        //            .Select(x => x.DiscountId)
        //            .ToList();

        //        var powerUpsToAssign = powerUps.Where(x => !alreadyAssignedPowerUpCodes.Contains(x.Id)).ToList();
        //        foreach (var item in powerUpsToAssign)
        //        {
        //            _customerPowerUpService.InsertCustomerPowerUp(new CustomerPowerUp()
        //            {
        //                CustomerId = customerId,
        //                DiscountId = item.Id
        //            });
        //        }
        //    }

        //}

        #endregion
    }
}