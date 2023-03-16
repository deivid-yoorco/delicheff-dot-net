using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using Teed.Plugin.Groceries.Domain.OrderReports;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Extensions
{
    public static class OrderExtensions
    {
        public static List<OrderItem> GetGroceryOrderItems(this IList<Order> orders,
            NotDeliveredOrderItemService _notDeliveredOrderItemService,
            IOrderService _orderService,
            IProductService _productService,
            List<NotDeliveredOrderItem> notDeliveredOrderItems = null,
            bool orderRequired = true,
            bool productRequired = true)
        {
            if (orders == null)
                throw new ArgumentNullException(nameof(orders));

            List<OrderItem> currentOrderItems = orders.Select(x => x.OrderItems).SelectMany(x => x).ToList();
            List<int> orderIds = orders.Select(x => x.Id).ToList();
            List<NotDeliveredOrderItem> currentNotDeliveredOrderItems = notDeliveredOrderItems == null ?
                _notDeliveredOrderItemService.GetAll().Where(x => orderIds.Contains(x.OrderId)).ToList()
                : notDeliveredOrderItems.Where(x => orderIds.Contains(x.OrderId)).ToList();

            var productIds = currentNotDeliveredOrderItems.Select(x => x.ProductId).ToList();
            var products = _productService.GetAllProductsQuery(true).Where(x => productIds.Contains(x.Id)).ToList();

            List<OrderItem> notDeliveredOrderItemsParsed = currentNotDeliveredOrderItems
                .Select(x => OrderUtils.ConvertToOrderItem(x, _orderService, _productService, products, orders.ToList(), orderRequired, productRequired))
                .ToList();
            return currentOrderItems.Union(notDeliveredOrderItemsParsed).ToList();
        }

        public static List<OrderItem> GetGroceryOrderItems(this Order order,
            NotDeliveredOrderItemService _notDeliveredOrderItemService,
            IOrderService _orderService,
            IProductService _productService,
            List<NotDeliveredOrderItem> notDeliveredOrderItems = null,
            bool orderRequired = true,
            bool productRequired = true)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            List<OrderItem> currentOrderItems = order.OrderItems.ToList();
            if (notDeliveredOrderItems == null)
            {
                notDeliveredOrderItems = _notDeliveredOrderItemService.GetAll()
                                 .Where(x => x.OrderId == order.Id)
                                 .ToList();
            }
            else
            {
                notDeliveredOrderItems = notDeliveredOrderItems
                    .Where(x => x.OrderId == order.Id)
                    .ToList();
            }
            return currentOrderItems.Union(notDeliveredOrderItems
                .Select(x => OrderUtils.ConvertToOrderItem(x, _orderService, _productService, orderRequired: orderRequired, productRequired: productRequired))
                                 .ToList()).ToList();
        }
    }
}
