using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Discounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.DiscountedProducts.Helpers
{
    public static class DiscountedProductHelper
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

        public static IOrderedQueryable<Product> GetDiscountedProducts(IDiscountService _discountService, IProductService _productService)
        {
            var allDiscounts = _discountService.GetAllDiscounts()
                .Where(x => x.DiscountType == Nop.Core.Domain.Discounts.DiscountType.AssignedToSkus
                || x.DiscountType == Nop.Core.Domain.Discounts.DiscountType.AssignedToCategories
                || x.DiscountType == Nop.Core.Domain.Discounts.DiscountType.AssignedToManufacturers).ToList();

            var selectDiscount = allDiscounts.Where(x => !x.RequiresCouponCode && (!x.EndDateUtc.HasValue || (x.EndDateUtc.HasValue && x.EndDateUtc.Value > DateTime.UtcNow)))
                .ToList();

            var categorieIds = selectDiscount.SelectMany(x => x.AppliedToCategories).Select(x => x.Id).Distinct().ToList();

            var manufacturerIds = selectDiscount.SelectMany(x => x.AppliedToManufacturers).Select(x => x.Id).Distinct().ToList();

            var productIds = selectDiscount.SelectMany(x => x.AppliedToProducts).Select(x => x.Id).Distinct().ToList();

            var productsQuery = _productService.GetAllProductsQuery()
                .Where(x => x.Published && (productIds.Contains(x.Id) ||
                categorieIds.Intersect(x.ProductCategories.Select(y => y.CategoryId)).Any() ||
                manufacturerIds.Intersect(x.ProductManufacturers.Select(y => y.ManufacturerId)).Any() || (x.OldPrice > x.Price)))
                .OrderByDescending(x => x.PublishedDateUtc).ThenByDescending(x => x.CreatedOnUtc).ThenByDescending(x => x.OldPrice - x.Price);

            return productsQuery;
        }
    }
}
