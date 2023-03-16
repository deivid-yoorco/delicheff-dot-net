using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Rewards;

namespace Nop.Services.Rewards
{
    /// <summary>
    /// Customer badge service interface
    /// </summary>
    public partial interface ICustomerBadgeService
    {
        /// <summary>
        /// Gets a customer badge
        /// </summary>
        /// <param name="customerBadgeId">The customer badge identifier</param>
        /// <returns>CustomerBadge</returns>
        CustomerBadge GetCustomerBadgeById(int customerBadgeId);

        /// <summary>
        /// Get customer badges by identifiers
        /// </summary>
        /// <param name="customerBadgeIds">Customer badges identifiers</param>
        /// <returns>CustomerBadge</returns>
        IList<CustomerBadge> GetCustomerBadgesByIds(int[] customerBadgeIds);

        /// <summary>
        /// Get customer badges by customer identifier
        /// </summary>
        /// <param name="customerId">Customer identifiers</param>
        /// <returns>CustomerBadge</returns>
        IList<CustomerBadge> GetCustomerBadgesByCustomerId(int customerId);

        /// <summary>
        /// Get customer badges by customer identifier
        /// </summary>
        /// <param name="badgeId">Badge identifiers</param>
        /// <returns>CustomerBadge</returns>
        IList<CustomerBadge> GetCustomerBadgesByBadgeId(int badgeId);

        IList<CustomerBadge> GetCustomerBadges();

        IQueryable<CustomerBadge> GetCustomerBadgesQuery();

        /// <summary>
        /// Deletes a customer badge
        /// </summary>
        /// <param name="customerBadge">The customer badge</param>
        void DeleteCustomerBadge(CustomerBadge customerBadge);

        /// <summary>
        /// Inserts a customer badge
        /// </summary>
        /// <param name="customerBadge">CustomerBadge</param>
        void InsertCustomerBadge(CustomerBadge customerBadge);

        /// <summary>
        /// Updates the customer badge
        /// </summary>
        /// <param name="customerBadge">The customer badge</param>
        void UpdateCustomerBadge(CustomerBadge customerBadge);
    }
}
