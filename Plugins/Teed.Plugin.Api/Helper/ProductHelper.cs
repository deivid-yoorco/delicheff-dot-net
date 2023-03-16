using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Discounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Products;

namespace Teed.Plugin.Api.Helper
{
    public static class ProductHelper
    {
        public static decimal GetWeightInterval(Product product)
        {
            if (product.EquivalenceCoefficient > 0)
                return 1000 / product.EquivalenceCoefficient;
            else if (product.WeightInterval > 0)
                return product.WeightInterval;
            else
                return 0;
        }

        public static ProductDto GetProductDto(Product product, Customer customer, IPriceCalculationService priceCalculationService)
        {
            return new ProductDto()
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                PictureUrl = "/Product/ProductImage?id=" + product.Id,
                Price = product.Price,
                Discount = priceCalculationService.GetDiscountAmount(product, customer ?? new Customer(), product.Price, out List<DiscountForCaching> appliedDiscounts),
                OldPrice = product.OldPrice > product.Price ? product.OldPrice : 0,
                EquivalenceCoefficient = product.EquivalenceCoefficient,
                WeightInterval = ProductHelper.GetWeightInterval(product),
                CurrentCartQuantity = customer != null ? customer.ShoppingCartItems.Where(y => y.ProductId == product.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.Quantity).FirstOrDefault() : 0,
                BuyingBySecondary = customer != null && customer.ShoppingCartItems.Where(y => y.ProductId == product.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.BuyingBySecondary).FirstOrDefault(),
                PropertyOptions = product.PropertiesOptions?.Split(',').Select(y => y.ToUpper().First() + y.ToLower().Substring(1)).ToArray(),
                SelectedPropertyOption = customer?.ShoppingCartItems.Where(y => y.ProductId == product.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.SelectedPropertyOption).FirstOrDefault(),
                IsExtraCartProduct = product.IsExtraCartProduct,
                IsInWishlist = customer != null ? customer.ShoppingCartItems.Where(y => y.ProductId == product.Id && y.ShoppingCartType == ShoppingCartType.Wishlist).Any() : false,
                Stock = product.ManageInventoryMethod == ManageInventoryMethod.DontManageStock ? 1000 : product.StockQuantity
            };
        }
    }
}
