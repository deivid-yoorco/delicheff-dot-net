using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Teed.Plugin.Api.Utils
{
    public static class OrderUtils
    {
        public static IQueryable<Order> GetFilteredOrders(IOrderService orderService)
        {
            return orderService.GetAllOrdersQuery().Where(x => x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10));
        }

        public static List<Order> GetOrdersInPedidoByOrder(Order order, IOrderService orderService)
        {
            return GetFilteredOrders(orderService).Where(x => x.CustomerId == order.CustomerId &&
            x.SelectedShippingDate == order.SelectedShippingDate &&
            x.ShippingAddress.Address1 == order.ShippingAddress.Address1)
                .ToList();
        }

    }
}
