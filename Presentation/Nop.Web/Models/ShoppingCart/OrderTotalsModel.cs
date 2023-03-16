﻿using System.Collections.Generic;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Web.Models.ShoppingCart
{
    public partial class OrderTotalsModel : BaseNopModel
    {
        public OrderTotalsModel()
        {
            TaxRates = new List<TaxRate>();
            GiftCards = new List<GiftCard>();
        }
        public bool IsEditable { get; set; }

        public decimal OrderTotalBalance { get; set; }
        public bool IsPaymentWithBlanceActive { get; set; }

        public bool RewardsActive { get; set; }
        public decimal OrderTotalPoints { get; set; }

        public decimal ProductDiscountsValue { get; set; }
        public string ProductDiscounts { get; set; }

        public string SubTotal { get; set; }
        public decimal SubtotalValue { get; set; }

        public string SubTotalDiscount { get; set; }
        public decimal SubTotalDiscountValue { get; set; }

        public string RequiredRewardPoints { get; set; }
        public decimal RequiredRewardPointsValue { get; set; }

        public string Shipping { get; set; }
        public decimal ShippingValue { get; set; }

        public bool RequiresShipping { get; set; }
        public string SelectedShippingMethod { get; set; }
        public bool HideShippingTotal { get; set; }

        public string PaymentMethodAdditionalFee { get; set; }

        public string Tax { get; set; }
        public IList<TaxRate> TaxRates { get; set; }
        public bool DisplayTax { get; set; }
        public bool DisplayTaxRates { get; set; }

        public IList<GiftCard> GiftCards { get; set; }

        public string OrderTotalDiscount { get; set; }
        public decimal OrderTotalDiscountValue { get; set; }
        public int RedeemedRewardPoints { get; set; }
        public string RedeemedRewardPointsAmount { get; set; }

        public int WillEarnRewardPoints { get; set; }

        public string OrderTotal { get; set; }
        public decimal OrderTotalValue { get; set; }

        public decimal CustomerRewardPointsBalance { get; set; }

        #region Nested classes

        public partial class TaxRate: BaseNopModel
        {
            public string Rate { get; set; }
            public string Value { get; set; }
        }

        public partial class GiftCard : BaseNopEntityModel
        {
            public string CouponCode { get; set; }
            public string Amount { get; set; }
            public string Remaining { get; set; }
        }

        #endregion
    }
}