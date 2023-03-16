using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Orders;
using Nop.Web.Framework.Mvc.Models;
using Nop.Web.Models.Common;
using Nop.Web.Models.ShoppingCart;
using System.Collections.Generic;

namespace Nop.Web.Models.Checkout
{
    public partial class OnePageCheckoutModel : BaseNopModel
    {
        public OnePageCheckoutModel()
        {
            this.Address = new AddressModel();
        }

        public bool ShippingRequired { get; set; }
        public bool DisableBillingAddressCheckoutStep { get; set; }

        public CheckoutShippingAddressModel ShippingAddress { get; set; }

        public CheckoutBillingAddressModel BillingAddress { get; set; }

        public ShoppingCartModel CartModel { get; set; }

        public OrderTotalsModel TotalModel { get; set; }

        //MVC is suppressing further validation if the IFormCollection is passed to a controller method. That's why we add to the model
        public IFormCollection Form { get; set; }

        public AddressModel Address { get; set; }

        public string ValidPostalCodes { get; set; }

        public string MercadoPreferenceId { get; set; }

        public bool AllProductsNoShipping { get; set; }
        public bool AnyProductHasTicketSpecification { get; set; }
    }
}