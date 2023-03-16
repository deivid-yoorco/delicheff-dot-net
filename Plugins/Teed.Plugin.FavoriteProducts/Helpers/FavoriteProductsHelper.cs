using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.FavoriteProducts.Helpers
{
    public static class FavoriteProductsHelper
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

        public static List<Product> GetCustomerFavoriteProducts(int UserId, IOrderService _orderService, ICustomerService _customerService)
        {
            if (UserId < 1) return null;

            var customerOrdersQuery = _orderService.GetAllOrdersQuery()
                .Where(x => x.CustomerId == UserId);

            if (customerOrdersQuery.Count() == 0) return null;

            Customer customer = _customerService.GetCustomerById(UserId);

            var products = customerOrdersQuery
                .SelectMany(x => x.OrderItems)
                .GroupBy(x => x.Product)
                .Where(x => x.Key.Published && !x.Key.GiftProductEnable)
                .OrderByDescending(x => x.Count())
                .Select(x => x.Key);

            return products.ToList();
        }
    }

    
}
