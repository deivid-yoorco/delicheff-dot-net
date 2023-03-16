using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.FavoriteProducts;
using Teed.Plugin.FavoriteProducts.Controllers;
using Teed.Plugin.FavoriteProducts.Dtos.Products;
using Teed.Plugin.FavoriteProducts.Helpers;
using Teed.Plugin.FavoriteProducts.Security;
using Nop.Services.Orders;

namespace Teed.Plugin.Api.Controllers
{
    public class FavoriteProductsApiController : ApiBaseController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly ISettingService _favoriteProductsSettingConfiguration;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public FavoriteProductsApiController(ICustomerService customerService,
            IPermissionService permissionService,
            IProductService productService,
            ISettingService favoriteProductsSettingConfiguration,
            IPriceCalculationService priceCalculationService,
            IOrderService orderService)
        {
            _customerService = customerService;
            _permissionService = permissionService;
            _productService = productService;
            _favoriteProductsSettingConfiguration = favoriteProductsSettingConfiguration;
            _priceCalculationService = priceCalculationService;
            _orderService = orderService;
        }

        #endregion

        #region Methods

        [HttpGet]
        [AllowAnonymous]
        public IActionResult FavoriteProductsForApp(int pageNumber = 0)
        {
            //prepare model
            var id = 0;
            int.TryParse(UserId, out id);
            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                customer = new Customer();

            FavoriteProductsSettings settings = _favoriteProductsSettingConfiguration.LoadSetting<FavoriteProductsSettings>();

            var model = FavoriteProductsHelper.GetCustomerFavoriteProducts(customer.Id, _orderService, _customerService);
            var products = new PagedList<Product>(model, pageNumber, settings.ProductsPerPage);

            var dto = products.Select(x => new FavoriteProductsDto()
            {
                Id = x.Id,
                Sku = x.Sku,
                Name = x.Name,
                OldPrice = x.OldPrice,
                Price = x.Price,
                Discount = _priceCalculationService.GetDiscountAmount(x, customer ?? new Customer(), x.Price, out List<DiscountForCaching> appliedDiscounts),
                PictureUrl = "/Product/ProductImage?id=" + x.Id,
                EquivalenceCoefficient = x.EquivalenceCoefficient,
                WeightInterval = FavoriteProductsHelper.GetWeightInterval(x),
                CurrentCartQuantity = customer != null ? customer.ShoppingCartItems.Where(y => y.ProductId == x.Id).Select(y => y.Quantity).FirstOrDefault() : 0,
                BuyingBySecondary = customer != null && customer.ShoppingCartItems.Where(y => y.ProductId == x.Id).Select(y => y.BuyingBySecondary).FirstOrDefault(),
                SelectedPropertyOption = customer?.ShoppingCartItems.Where(y => y.ProductId == x.Id).Select(y => y.SelectedPropertyOption).FirstOrDefault(),
                PropertyOptions = x.PropertiesOptions?.Split(',').Select(z => z.ToUpper().First() + z.ToLower().Substring(1)).ToArray(),
                Stock = x.ManageInventoryMethod == ManageInventoryMethod.DontManageStock ? 1000 : x.StockQuantity
            }).ToList();

            return Ok(dto);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetFavoriteProductsPluginInfo()
        {
            FavoriteProductsSettings settings = _favoriteProductsSettingConfiguration.LoadSetting<FavoriteProductsSettings>();
            return Ok(settings);
        }

        #endregion
    }
}
