using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Rewards;

namespace Nop.Services.Rewards
{
    /// <summary>
    /// Customer point service interface
    /// </summary>
    public partial interface ICustomerPointService
    {
        /// <summary>
        /// Gets a customer point
        /// </summary>
        /// <param name="customerPointId">The customer point identifier</param>
        /// <returns>CustomerPoint</returns>
        CustomerPoint GetCustomerPointById(int customerPointId);

        /// <summary>
        /// Get customer points by identifiers
        /// </summary>
        /// <param name="customerPointIds">Customer points identifiers</param>
        /// <returns>CustomerPoint</returns>
        IList<CustomerPoint> GetCustomerPointsByIds(int[] customerPointIds);

        /// <summary>
        /// Get customer points by customer identifier
        /// </summary>
        /// <param name="customerId">Customer identifiers</param>
        /// <returns>CustomerPoint</returns>
        IList<CustomerPoint> GetCustomerPointsByCustomerId(int customerId);

        IList<CustomerPoint> GetCustomerPoints();

        IQueryable<CustomerPoint> GetCustomerPointsQuery();

        /// <summary>
        /// Get customer points balance
        /// </summary>
        /// <returns></returns>
        decimal GetCustomerPointsBalance(int customerId);

        /// <summary>
        /// Deletes a customer point
        /// </summary>
        /// <param name="customerPoint">The customer point</param>
        void DeleteCustomerPoint(CustomerPoint customerPoint);

        /// <summary>
        /// Inserts a customer point
        /// </summary>
        /// <param name="customerPoint">CustomerPoint</param>
        void InsertCustomerPoint(CustomerPoint customerPoint);

        /// <summary>
        /// Updates the customer point
        /// </summary>
        /// <param name="customerPoint">The customer point</param>
        void UpdateCustomerPoint(CustomerPoint customerPoint);
    }
}
