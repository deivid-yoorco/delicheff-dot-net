using System.ComponentModel.DataAnnotations;

namespace Nop.Core.Domain.Discounts
{
    /// <summary>
    /// Represents a discount limitation type
    /// </summary>
    public enum DiscountLimitationType
    {
        /// <summary>
        /// None
        /// </summary>
        Unlimited = 0,
        /// <summary>
        /// N Times Only
        /// </summary>
        NTimesOnly = 15,
        /// <summary>
        /// N Times Per Customer
        /// </summary>
        NTimesPerCustomer = 25,

        /// <summary>
        /// Valid only for first order
        /// </summary>
        SoloPrimeraOrden = 35
    }
}
