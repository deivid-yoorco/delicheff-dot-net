using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
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
using Teed.Plugin.RecentProducts;
using Teed.Plugin.RecentProducts.Controllers;
using Teed.Plugin.RecentProducts.Dtos.Products;
using Teed.Plugin.RecentProducts.Helpers;
using Teed.Plugin.RecentProducts.Security;

namespace Teed.Plugin.Api.Controllers
{
    public class RecentProductsApiController : ApiBaseController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly ISettingService _recentProductsSettingConfiguration;
        private readonly IPriceCalculationService _priceCalculationService;

        #endregion

        #region Ctor

        public RecentProductsApiController(ICustomerService customerService,
            IPermissionService permissionService,
            IProductService productService,
            ISettingService recentProductsSettingConfiguration,
            IPriceCalculationService priceCalculationService)
        {
            _customerService = customerService;
            _permissionService = permissionService;
            _productService = productService;
            _recentProductsSettingConfiguration = recentProductsSettingConfiguration;
            _priceCalculationService = priceCalculationService;
        }

        #endregion

        #region Methods

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RecentProductsForApp(int pageNumber = 0)
        {
            //prepare model
            var id = 0;
            int.TryParse(UserId, out id);
            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                customer = new Customer();

            RecentProductsSettings settings = _recentProductsSettingConfiguration.LoadSetting<RecentProductsSettings>();
            var days = DateTime.Now.AddDays(-settings.ProductsBeforeDays);

            var model = _productService.GetAllProductsQuery().Where(x => x.VisibleIndividually && !x.GiftProductEnable && x.Published && !x.Deleted && x.CreatedOnUtc >= days)
                .OrderByDescending(x => x.CreatedOnUtc);
            var products = new PagedList<Product>(model, pageNumber, settings.ProductsPerPage);

            var dto = products.Select(x => new ProductDto()
            {
                Id = x.Id,
                Sku = x.Sku,
                Name = x.Name,
                OldPrice = x.OldPrice,
                Price = x.Price,
                Discount = _priceCalculationService.GetDiscountAmount(x, customer ?? new Customer(), x.Price, out List<DiscountForCaching> appliedDiscounts),
                PictureUrl = "/Product/ProductImage?id=" + x.Id,
                EquivalenceCoefficient = x.EquivalenceCoefficient,
                WeightInterval = ProductHelper.GetWeightInterval(x),
                CurrentCartQuantity = customer != null ? customer.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.Quantity).FirstOrDefault() : 0,
                BuyingBySecondary = customer != null && customer.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.BuyingBySecondary).FirstOrDefault(),
                SelectedPropertyOption = customer?.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.SelectedPropertyOption).FirstOrDefault(),
                PropertyOptions = x.PropertiesOptions?.Split(',').Select(z => z.ToUpper().First() + z.ToLower().Substring(1)).ToArray(),
                IsInWishlist = customer.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.Wishlist).Any(),
                Stock = x.ManageInventoryMethod == ManageInventoryMethod.DontManageStock ? 1000 : x.StockQuantity
            }).ToList();

            return Ok(dto);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetRecentProductsPluginInfo()
        {
            RecentProductsSettings settings = _recentProductsSettingConfiguration.LoadSetting<RecentProductsSettings>();
            return Ok(settings);
        }

        #endregion
    }
}
