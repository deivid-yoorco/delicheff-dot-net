﻿using System.Collections.Generic;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Web.Models.Checkout
{
    public partial class CheckoutPaymentMethodModel : BaseNopModel
    {
        public CheckoutPaymentMethodModel()
        {
            PaymentMethods = new List<PaymentMethodModel>();
        }

        public IList<PaymentMethodModel> PaymentMethods { get; set; }

        public bool DisplayRewardPoints { get; set; }
        public int RewardPointsBalance { get; set; }
        public string RewardPointsAmount { get; set; }
        public bool RewardPointsEnoughToPayForOrder { get; set; }
        public bool UseRewardPoints { get; set; }

        #region Nested classes

        public partial class PaymentMethodModel : BaseNopModel
        {
            public string PaymentMethodSystemName { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Fee { get; set; }
            public bool Selected { get; set; }
            public string LogoUrl { get; set; }
            public List<PaymentChild> PaymentChildrens { get; set; }
        }

        public partial class PaymentChild
        {
            public string PublicName { get; set; }

            public string SystemName { get; set; }

            public int PaymentLimit { get; set; }

            public string ProviderImage { get; set; }
        }
        #endregion
    }
}