﻿using Nop.Web.Framework.Mvc.Models;

namespace Nop.Web.Models.Checkout
{
    public partial class CheckoutCompletedModel : BaseNopModel
    {
        public int OrderId { get; set; }
        public string CustomOrderNumber { get; set; }
        public bool OnePageCheckoutEnabled { get; set; }
        public decimal OrderTotal { get; set; }

        public bool RewardsActive { get; set; }
        public decimal OrderTotalPoints { get; set; }
    }
}