using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Rewards;

namespace Nop.Services.Rewards
{
    /// <summary>
    /// Customer balance service interface
    /// </summary>
    public partial interface ICustomerBalanceService
    {
        /// <summary>
        /// Gets a Customer balance
        /// </summary>
        /// <param name="customerBalanceId">The customer balance identifier</param>
        /// <returns>CustomerBalance</returns>
        CustomerBalance GetCustomerBalanceById(int customerBalanceId);

        /// <summary>
        /// Gets all Customer balances by customer identifier
        /// </summary>
        /// <param name="customerId">The customer identifier</param>
        /// <returns>List<CustomerBalance></returns>
        IList<CustomerBalance> GetAllCustomerBalanceByCustomerId(int customerId);

        IList<CustomerBalance> GetAllCustomerBalances();

        IQueryable<CustomerBalance> GetAllCustomerBalancesQuery();

        /// <summary>
        /// Deletes a customer balance
        /// </summary>
        /// <param name="customerBalance">The customer balance</param>
        void DeleteCustomerBalance(CustomerBalance customerBalance);

        /// <summary>
        /// Inserts a customer balance
        /// </summary>
        /// <param name="customerBalance">CustomerBalance</param>
        void InsertCustomerBalance(CustomerBalance customerBalance);

        /// <summary>
        /// Updates the customer balance
        /// </summary>
        /// <param name="customerBalance">The customer balance</param>
        void UpdateCustomerBalance(CustomerBalance customerBalance);

    }
}
