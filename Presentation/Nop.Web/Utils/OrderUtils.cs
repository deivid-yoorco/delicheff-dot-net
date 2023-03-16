using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Utils
{
    public static class OrderUtils
    {
        public static IQueryable<Order> GetFilteredOrders(IOrderService orderService)
        {
            return orderService.GetAllOrdersQuery().Where(x => x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10));
        }

        public static bool CashPaymentOrderIsValid(IOrderService orderService, int currentCustomerId, DateTime selectedShippingDate, decimal currentOrderTotal)
        {
            DateTime today = DateTime.Now.Date;
            var customerActiveCashOrdersTotalSum = GetFilteredOrders(orderService)
                .Where(x => x.CustomerId == currentCustomerId &&
                x.SelectedShippingDate == selectedShippingDate)
                .Where(x => x.PaymentMethodSystemName == "Payments.CashOnDelivery")
                .Select(x => x.OrderTotal)
                .DefaultIfEmpty()
                .Sum();

            decimal cashOrderSum = customerActiveCashOrdersTotalSum + currentOrderTotal;
            return cashOrderSum <= 6000;
        }

        public static bool OrderAmountsAreValid(IOrderService orderService, int currentCustomerId, DateTime selectedShippingDate, decimal currentOrderTotal)
        {
            var customerHasHighValueOrder = GetFilteredOrders(orderService)
                .Where(x => x.CustomerId == currentCustomerId &&
                x.SelectedShippingDate == selectedShippingDate &&
                x.OrderTotal >= 1500)
                .Any();

            return !(customerHasHighValueOrder && currentOrderTotal > 1500);
        }

        public static IEnumerable<IGrouping<dynamic, Order>> GetPedidosGroupByList(List<Order> orders)
        {
            return orders.GroupBy(x => new
            {
                x.CustomerId,
                CreatedOnUtc = x.CreatedOnUtc,
                ShippingAddress = x.ShippingAddress.Address1
            });
        }

        //public static IEnumerable<IGrouping<dynamic, Order>> GetOrdersByUserDate(List<Order> orders)
        //{
        //    return orders.GroupBy(x => new
        //    {
        //        x.CustomerId,
        //        CreatedOnUtc = x.CreatedOnUtc,
        //        ShippingAddress = x.ShippingAddress.Address1,
        //        Name = x.Customer.Username,
        //        NumCliente = x.Customer.ShippingAddress.FirstName,
        //        LastName = x.Customer.ShippingAddress.LastName,
        //        OrderId = x.Id,
        //        OrderTotal = x.OrderTotal,
        //        PartsSold = x.OrderItems.Count(),
        //        street = x.ShippingAddress.Address1,
        //        Colony = x.ShippingAddress.Address2,
        //        City = x.ShippingAddress.City,


        //    });
        //}
    }
}
