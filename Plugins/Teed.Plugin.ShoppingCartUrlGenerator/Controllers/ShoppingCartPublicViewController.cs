using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.ShoppingCartUrlGenerator.Models;
using Teed.Plugin.ShoppingCartUrlGenerator.Services;
using static Nop.Web.Models.ShoppingCart.ShoppingCartModel;

namespace Teed.Plugin.ShoppingCartUrlGenerator.Controllers
{
    public class ShoppingCartPublicViewController : BasePublicController
    {
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IPaymentService _paymentService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ShoppingCartUrlService _shoppingCartUrlService;
        private readonly ShoppingCartUrlProductService _shoppingCartUrlProductService;
        private readonly MediaSettings _mediaSettings;

        public ShoppingCartPublicViewController(IPermissionService permissionService, ShoppingCartUrlService shoppingCartUrlService,
            IProductService productService, IShoppingCartService shoppingCartService, IWorkContext workContext,
            ShoppingCartUrlProductService shoppingCartUrlProductService, IPaymentService paymentService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            MediaSettings mediaSettings, IStoreContext storeContext, ICustomerActivityService customerActivityService,
            ILocalizationService localizationService)
        {
            _permissionService = permissionService;
            _shoppingCartUrlService = shoppingCartUrlService;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _shoppingCartUrlProductService = shoppingCartUrlProductService;
            _paymentService = paymentService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _mediaSettings = mediaSettings;
            _storeContext = storeContext;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("sc/{urlCode}")]
        public IActionResult AddProductsToCart(string urlCode)
        {
            var shoppingCartUrl = _shoppingCartUrlService.GetAll()
                .Where(x => x.Code.ToLower() == urlCode.Trim().ToLower())
                .Include(x => x.ShoppingCartUrlProducts)
                .FirstOrDefault();

            if (shoppingCartUrl == null) return NotFound();
            if (!shoppingCartUrl.IsActive) return NotFound();

            var productIds = shoppingCartUrl.ShoppingCartUrlProducts.Select(x => x.ProductId).ToList();
            var products = _productService.GetAllProductsQuery().Where(x => productIds.Contains(x.Id)).ToList();
            var items = new List<ShoppingCartItemModel>();

            var currentShoppingCart = _workContext.CurrentCustomer.ShoppingCartItems.ToList();

            foreach (var urlProduct in shoppingCartUrl.ShoppingCartUrlProducts.Where(x => !x.Deleted))
            {
                var product = products.Where(x => x.Id == urlProduct.ProductId).FirstOrDefault();
                if (product == null) continue;
                var productInCart = currentShoppingCart.Where(x => x.ProductId == urlProduct.ProductId).FirstOrDefault();
                var sci = new ShoppingCartItemModel()
                {
                    Id = product.Id,
                    ProductId = urlProduct.ProductId,
                    Quantity = urlProduct.Quantity,
                    BuyingBySecondary = urlProduct.BuyingBySecondary,
                    SelectedPropertyOption = urlProduct.SelectedPropertyOption,
                    EquivalenceCoefficient = product.EquivalenceCoefficient,
                    ProductName = product.Name,
                    WeightInterval = product.WeightInterval,
                    Warnings = productInCart != null ? new List<string>() { $"Ya tienes {GetParsedQuantity(productInCart.Quantity, urlProduct.BuyingBySecondary, product.EquivalenceCoefficient, product.WeightInterval)} de este producto en tu carrito." } : null,
                    Picture = _shoppingCartModelFactory.PrepareCartItemPictureModel(new ShoppingCartItem() { Id = urlProduct.Id ,Product = product }, _mediaSettings.MiniCartThumbPictureSize, true, product.Name)
                };
                //sci.SubTotal = _paymentService.CalculateGroceryPrice(product, null, urlProduct.Quantity, false).ToString("C");
                items.Add(sci);

            }

            var model = new ShoppingCartUrlPublicViewModel()
            {
                Products = items,
                Body = shoppingCartUrl.Body,
                ShoppingCartUrlId = shoppingCartUrl.Id
            };

            return View("~/Plugins/Teed.Plugin.ShoppingCartUrlGenerator/Views/ShoppingCartUrl/AddToCartUrlProducts.cshtml", model);
        }

        [HttpPost]
        public IActionResult AddProductsToCart(ShoppingCartUrlPublicViewModel model)
        {
            var productsToAdd = model.SelectedProducts.Where(x => x.Selected && x.SelectedQuantity > 0).ToList();
            var productIds = productsToAdd.Select(x => x.ProductId).ToList();
            var shoppingCartUrlProductsToAdd = _shoppingCartUrlService.GetAll()
                .Include(x => x.ShoppingCartUrlProducts)
                .Where(x => x.Id == model.ShoppingCartUrlId)
                .SelectMany(x => x.ShoppingCartUrlProducts)
                .Where(x => productIds.Contains(x.ProductId) && !x.Deleted)
                .ToList();

            var customer = _workContext.CurrentCustomer;
            var cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var products = _productService.GetAllProductsQuery().Where(x => productIds.Contains(x.Id)).ToList();

            foreach (var item in shoppingCartUrlProductsToAdd)
            {
                var product = products.Where(x => x.Id == item.ProductId).FirstOrDefault();
                if (product == null) continue;

                var shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart, ShoppingCartType.ShoppingCart, product);
                //if we already have the same product in the cart, then use the total quantity to validate
                var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + item.Quantity : item.Quantity;
                var addToCartWarnings = _shoppingCartService
                    .GetShoppingCartItemWarnings(customer, ShoppingCartType.ShoppingCart,
                    product, _storeContext.CurrentStore.Id, string.Empty,
                    decimal.Zero, null, null, quantityToValidate, false, true, false, false, false);

                if (addToCartWarnings.Any()) continue;

                decimal customerEnteredPrice = _paymentService.CalculateGroceryPrice(product, shoppingCartItem, item.Quantity, false); //calculate price
                                                                                                                           //now let's try adding product to the cart (now including product attribute validation, etc)
                addToCartWarnings = _shoppingCartService.AddToCart(customer: customer,
                    product: product,
                    shoppingCartType: ShoppingCartType.ShoppingCart,
                    storeId: _storeContext.CurrentStore.Id,
                    attributesXml: "",
                    quantity: item.Quantity, buyingBySecondary: item.BuyingBySecondary, selectedPropertyOption: item.SelectedPropertyOption, customerEnteredPrice: customerEnteredPrice);

                _customerActivityService.InsertActivity("PublicStore.AddToShoppingCart", _localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart") + " Desde la app.", product.Name);
            }

            return RedirectToRoute("ShoppingCart");
        }

        private string GetParsedQuantity(int quantity, bool buyingBySecondary, decimal equivalenceCoefficient, decimal weightInterval)
        {
            string unit = "pz";
            decimal weight = quantity;
            if (equivalenceCoefficient > 0 && buyingBySecondary)
            {
                weight = (quantity * 1000) / equivalenceCoefficient;
                unit = "gr";
            }
            else if (weightInterval > 0)
            {
                weight = quantity * weightInterval;
                unit = "gr";
            }

            if (weight >= 1000)
            {
                weight = (weight / 1000);
                unit = "kg";
            }

            return Math.Round(weight, 2).ToString() + " " + unit;
        }
    }
}
