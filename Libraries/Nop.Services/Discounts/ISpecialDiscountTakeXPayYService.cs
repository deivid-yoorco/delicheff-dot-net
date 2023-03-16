using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;

namespace Nop.Services.Discounts
{
    /// <summary>
    /// Discount service interface
    /// </summary>
    public partial interface ISpecialDiscountTakeXPayYService
    {
        #region Discounts

        /// <summary>
        /// Gets a special discount
        /// </summary>
        /// <param name="id">Special discount identifier</param>
        /// <returns>Discount</returns>
        SpecialDiscountTakeXPayY GetSpecialDiscountById(int id);

        /// <summary>
        /// Gets a special discount
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <returns>Discount</returns>
        SpecialDiscountTakeXPayY GetSpecialDiscountByDiscountId(int discountId);

        /// <summary>
        /// Gets all special discounts
        /// </summary>
        /// <returns>Discounts</returns>
        IQueryable<SpecialDiscountTakeXPayY> GetAllSpecialDiscounts();

        /// <summary>
        /// Inserts a special discount
        /// </summary>
        /// <param name="discount">Discount</param>
        void InsertDiscount(SpecialDiscountTakeXPayY discount);

        /// <summary>
        /// Updates the special discount
        /// </summary>
        /// <param name="discount">Special Discount</param>
        void UpdateDiscount(SpecialDiscountTakeXPayY discount);

        #endregion
    }
}
