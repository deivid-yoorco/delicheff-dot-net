using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.ShoppingCart;
using static Nop.Web.Models.ShoppingCart.ShoppingCartModel;

namespace Nop.Web.Components
{
    public class ExtraCartProductsViewComponent : NopViewComponent
    {
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IProductService _productService;

        public ExtraCartProductsViewComponent(IShoppingCartModelFactory shoppingCartModelFactory,
            IStoreContext storeContext,
            IWorkContext workContext,
            IProductService productService)
        {
            this._shoppingCartModelFactory = shoppingCartModelFactory;
            this._storeContext = storeContext;
            this._workContext = workContext;
            this._productService = productService;
        }

        public IViewComponentResult Invoke()
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var products = _productService.GetAllProductsQuery()
                .Where(x => x.IsExtraCartProduct && x.Published)
                .ToList();

            var model = new ExtraShoppingCartProductModel();
            model = _shoppingCartModelFactory.PrepareExtraShoppingCartProductModel(model, cart, products, _workContext.CurrentCustomer, _storeContext.CurrentStore.Id);

            return View(model);
        }
    }
}
