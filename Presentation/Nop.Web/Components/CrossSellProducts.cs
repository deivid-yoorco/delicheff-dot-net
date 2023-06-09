﻿using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components
{
    public class CrossSellProductsViewComponent : NopViewComponent
    {
        private readonly IAclService _aclService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        public CrossSellProductsViewComponent(IAclService aclService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IStoreMappingService storeMappingService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ShoppingCartSettings shoppingCartSettings)
        {
            this._aclService = aclService;
            this._productModelFactory = productModelFactory;
            this._productService = productService;
            this._storeMappingService = storeMappingService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._shoppingCartSettings = shoppingCartSettings;
        }

        public IViewComponentResult Invoke(int? productThumbPictureSize, bool isForSimilarProducts = false, int productId = 0)
        {
            if (!isForSimilarProducts)
            {
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();

                var products = _productService.GetCrosssellProductsByShoppingCart(cart, _shoppingCartSettings.CrossSellsNumber);
                //ACL and store mapping
                products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
                //availability dates
                products = products.Where(p => p.IsAvailable()).ToList();

                if (!products.Any())
                    return Content("");

                //Cross-sell products are displayed on the shopping cart page.
                //We know that the entire shopping cart page is not refresh
                //even if "ShoppingCartSettings.DisplayCartAfterAddingProduct" setting  is enabled.
                //That's why we force page refresh (redirect) in this case
                var model = _productModelFactory.PrepareProductOverviewModels(products,
                        productThumbPictureSize: productThumbPictureSize, forceRedirectionAfterAddingToCart: true)
                    .ToList();

                return View(model);
            }
            else
            {
                // For similar products instead of cross products
                if (productId < 1)
                    return Content("");

                var product = _productService.GetProductById(productId);
                if (product == null)
                    return Content("");

                var categoryId = product.ProductCategories.Where(y => y.CategoryId > 0).OrderBy(x => x.DisplayOrder)
                    .Select(y => y.CategoryId).FirstOrDefault();
                var products = _productService.GetAllProductsQuery()
                    .Where(x => x.ProductCategories.Select(y => y.CategoryId).ToList().Contains(categoryId) &&
                    x.Id != productId && x.Published == true).Take(30).ToList();

                //ACL and store mapping
                products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
                //availability dates
                products = products.Where(p => p.IsAvailable()).ToList();
                if (!products.Any())
                    return Content("");

                var model = _productModelFactory.PrepareProductOverviewModels(products,
                        productThumbPictureSize: productThumbPictureSize, forceRedirectionAfterAddingToCart: true)
                    .ToList();

                return View(model);
            }
        }
    }
}
