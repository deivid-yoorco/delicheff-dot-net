using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts.Cache;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Orders;

namespace Nop.Services.Discounts
{
    /// <summary>
    /// Discount service
    /// </summary>
    public partial class SpecialDiscountTakeXPayYService : ISpecialDiscountTakeXPayYService
    {
        #region Fields

        private readonly IRepository<SpecialDiscountTakeXPayY> _specialDiscountTakeXPayYRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Static cache manager</param>
        /// <param name="specialDiscountTakeXPayYRepository">Special discount repository</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="eventPublisher">Event published</param>
        /// <param name="orderService">Order service</param>
        public SpecialDiscountTakeXPayYService(IRepository<SpecialDiscountTakeXPayY> specialDiscountTakeXPayYRepository,
            IEventPublisher eventPublisher)
        {
            this._specialDiscountTakeXPayYRepository = specialDiscountTakeXPayYRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        #region Discounts

        /// <summary>
        /// Gets a special discount
        /// </summary>
        /// <param name="id">Special discount identifier</param>
        /// <returns>Discount</returns>
        public virtual SpecialDiscountTakeXPayY GetSpecialDiscountById(int id)
        {
            if (id == 0)
                return null;

            return _specialDiscountTakeXPayYRepository.GetById(id);
        }


        /// <summary>
        /// Gets a special discount
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <returns>Discount</returns>
        public virtual SpecialDiscountTakeXPayY GetSpecialDiscountByDiscountId(int discountId)
        {
            if (discountId == 0)
                return null;

            return _specialDiscountTakeXPayYRepository.Table.Where(x => x.DiscountId == discountId).FirstOrDefault();
        }

        /// <summary>
        /// Gets all special discounts
        /// </summary>
        /// <returns>Discounts</returns>
        public virtual IQueryable<SpecialDiscountTakeXPayY> GetAllSpecialDiscounts()
        {
            return _specialDiscountTakeXPayYRepository.Table;
        }

        /// <summary>
        /// Inserts a special discount
        /// </summary>
        /// <param name="discount">Discount</param>
        public virtual void InsertDiscount(SpecialDiscountTakeXPayY discount)
        {
            if (discount == null)
                throw new ArgumentNullException(nameof(discount));

            var utcNow = DateTime.UtcNow;
            discount.CreatedOnUtc = utcNow;
            discount.UpdatedOnUtc = utcNow;
            _specialDiscountTakeXPayYRepository.Insert(discount);

            //event notification
            _eventPublisher.EntityInserted(discount);
        }

        /// <summary>
        /// Updates the special discount
        /// </summary>
        /// <param name="discount">Special Discount</param>
        public virtual void UpdateDiscount(SpecialDiscountTakeXPayY discount)
        {
            if (discount == null)
                throw new ArgumentNullException(nameof(discount));

            discount.UpdatedOnUtc = DateTime.UtcNow;
            _specialDiscountTakeXPayYRepository.Update(discount);

            //event notification
            _eventPublisher.EntityUpdated(discount);
        }

        #endregion

        #endregion
    }
}
