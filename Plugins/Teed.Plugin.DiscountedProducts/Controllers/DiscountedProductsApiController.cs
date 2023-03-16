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
using Teed.Plugin.DiscountedProducts;
using Teed.Plugin.DiscountedProducts.Controllers;
using Teed.Plugin.DiscountedProducts.Dtos.Products;
using Teed.Plugin.DiscountedProducts.Helpers;
using Teed.Plugin.DiscountedProducts.Security;

namespace Teed.Plugin.Api.Controllers
{
    public class DiscountedProductsApiController : ApiBaseController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly ISettingService _discountedProductsSettingConfiguration;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IDiscountService _discountService;

        #endregion

        #region Ctor

        public DiscountedProductsApiController(ICustomerService customerService,
            IPermissionService permissionService,
            IProductService productService,
            ISettingService discountedProductsSettingConfiguration,
            IPriceCalculationService priceCalculationService,
            IDiscountService discountService)
        {
            _discountService = discountService;
            _customerService = customerService;
            _permissionService = permissionService;
            _productService = productService;
            _discountedProductsSettingConfiguration = discountedProductsSettingConfiguration;
            _priceCalculationService = priceCalculationService;
        }

        #endregion

        #region Methods

        [HttpGet]
        [AllowAnonymous]
        public IActionResult DiscountedProductsForApp(int pageNumber = 0)
        {
            //prepare model
            var id = 0;
            int.TryParse(UserId, out id);
            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                customer = new Customer();

            DiscountedProductsSettings settings = _discountedProductsSettingConfiguration.LoadSetting<DiscountedProductsSettings>();
            var productsQuery = DiscountedProductHelper.GetDiscountedProducts(_discountService, _productService);
            var products = new PagedList<Product>(productsQuery, pageNumber, settings.ProductsPerPage);

            var dto = products.Select(x => new DiscountedProductDto()
            {
                Id = x.Id,
                Sku = x.Sku,
                Name = x.Name,
                OldPrice = x.OldPrice,
                Price = x.Price,
                Discount = _priceCalculationService.GetDiscountAmount(x, customer ?? new Customer(), x.Price, out List<DiscountForCaching> appliedDiscounts),
                PictureUrl = "/Product/ProductImage?id=" + x.Id,
                EquivalenceCoefficient = x.EquivalenceCoefficient,
                WeightInterval = DiscountedProductHelper.GetWeightInterval(x),
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
        public IActionResult GetDiscountedProductsPluginInfo()
        {
            DiscountedProductsSettings settings = _discountedProductsSettingConfiguration.LoadSetting<DiscountedProductsSettings>();
            return Ok(settings);
        }

        #endregion
    }
}
