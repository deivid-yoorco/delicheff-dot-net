using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Emails.Controllers
{
    public class ReorderEmailController : BasePluginController
    {
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;

        public ReorderEmailController(IOrderService orderService, IWorkContext workContext,
            IShoppingCartService shoppingCartService, IStoreContext storeContext)
        {
            _orderService = orderService;
            _workContext = workContext;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
        }

        [AllowAnonymous]
        [Route("[controller]/[action]")]
        public IActionResult Reorder(string oid)
        {
            if (string.IsNullOrWhiteSpace(oid)) return NotFound();
            List<int> orderIds = oid.Split(',').Select(x => int.Parse(x)).ToList();
            List<Order> orders = _orderService.GetAllOrdersQuery().Where(x => orderIds.Contains(x.Id)).ToList();
            List<OrderItem> orderItems = orders.Select(x => x.OrderItems).SelectMany(x => x).ToList();
            Customer customer = _workContext.CurrentCustomer;

            foreach (var item in orderItems)
            {
                //get standard warnings without attribute validations
                //first, try to find existing shopping cart item
                var cart = customer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .ToList();
                var shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart, ShoppingCartType.ShoppingCart, item.Product);
                if (shoppingCartItem != null) continue;

                //if we already have the same product in the cart, then use the total quantity to validate
                var addToCartWarnings = _shoppingCartService
                    .GetShoppingCartItemWarnings(customer, ShoppingCartType.ShoppingCart,
                    item.Product, _storeContext.CurrentStore.Id, string.Empty,
                    decimal.Zero, null, null, item.Quantity, false, true, false, false, false);

                if (addToCartWarnings.Any()) continue;

                //now let's try adding product to the cart (now including product attribute validation, etc)
                addToCartWarnings = _shoppingCartService.AddToCart(customer: customer,
                    product: item.Product,
                    shoppingCartType: ShoppingCartType.ShoppingCart,
                    storeId: _storeContext.CurrentStore.Id,
                    attributesXml: item.AttributesXml,
                    quantity: item.Quantity, 
                    buyingBySecondary: item.BuyingBySecondary, 
                    selectedPropertyOption: item.SelectedPropertyOption);
            }

            return Redirect(Url.RouteUrl("ShoppingCart"));
        }
    }
}
