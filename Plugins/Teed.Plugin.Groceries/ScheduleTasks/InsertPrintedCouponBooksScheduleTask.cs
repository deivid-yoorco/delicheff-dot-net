using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.PrintedCouponBooks;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.ScheduleTasks
{
    public class InsertPrintedCouponBooksScheduleTask : IScheduleTask
    {
        private readonly PrintedCouponBooksService _printedCouponBooksService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;

        public InsertPrintedCouponBooksScheduleTask(PrintedCouponBooksService printedCouponBooksService,
            ICustomerService customerService, IOrderService orderService,
            ILogger logger)
        {
            _printedCouponBooksService = printedCouponBooksService;
            _customerService = customerService;
            _orderService = orderService;
            _logger = logger;
        }

        public void Execute()
        {
            _logger.Information($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Running InsertPrintedCouponBooksScheduleTask.");

            var today = DateTime.Now;
            var todayStartUtc = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0).ToUniversalTime();
            var couponBooks = _printedCouponBooksService.GetAll()
                .Where(x => !x.Deleted && x.Active && !string.IsNullOrEmpty(x.BookTypeValue) && x.ConnectedProductId > 0 &&
                x.StartDate <= today && today <= x.EndDate).ToList();
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.CreatedOnUtc >= todayStartUtc &&
                (x.PaymentStatusId == (int)PaymentStatus.Paid && x.OrderStatusId == (int)OrderStatus.Processing ||
                    (x.PaymentStatusId == (int)PaymentStatus.Pending && x.OrderStatusId == (int)OrderStatus.Pending &&
                        (x.PaymentMethodSystemName == "Payments.CardOnDelivery" || x.PaymentMethodSystemName == "Payments.CashOnDelivery"))));
            var orderIdsNotCancelled = OrderUtils.GetFilteredOrders(_orderService)
                .Select(x => x.Id).ToList();
            var productIds = couponBooks.Select(x => x.ConnectedProductId).ToList();
            var couponBookItemsUsed = _orderService.GetAllOrderItemsQuery()
                .Where(x => productIds.Contains(x.ProductId))
                .Select(x => new { x.ProductId, x.OrderId })
                .ToList();
            var pedidos = OrderUtils.GetPedidosGroup(orders);

            if (orders.Any()) {
                foreach (var couponBook in couponBooks)
                {
                    var couponBookItemUsed = couponBookItemsUsed.Where(x => x.ProductId == couponBook.ConnectedProductId && orderIdsNotCancelled.Contains(x.OrderId)).Count();
                    var inventoryAvailable = couponBook.Inventory - couponBookItemUsed;
                    if (inventoryAvailable > 0)
                    {
                        if (couponBook.BookTypeId == (int)PrintedCouponBookType.ZipCode)
                        {
                            var zipCodes = couponBook.BookTypeValue.Split(',').ToList();
                            var ordersWithZipCode = orders.Where(x => zipCodes.Contains(x.ShippingAddress.ZipPostalCode) &&
                                !x.OrderItems.Select(y => y.ProductId).Contains(couponBook.ConnectedProductId))
                                .ToList();
                            foreach (var order in ordersWithZipCode)
                            {
                                InsertCouponBookOrderItem(order, couponBook, pedidos);
                            }
                        }
                        else if (couponBook.BookTypeId == (int)PrintedCouponBookType.Subtotal)
                        {
                            var subtotal = decimal.Parse(couponBook.BookTypeValue);
                            var ordersWithSubtotal = orders.Where(x => x.OrderSubtotalInclTax >= subtotal &&
                                !x.OrderItems.Select(y => y.ProductId).Contains(couponBook.ConnectedProductId))
                                .ToList();
                            foreach (var order in ordersWithSubtotal)
                            {
                                InsertCouponBookOrderItem(order, couponBook, pedidos);
                            }
                        }
                        else if (couponBook.BookTypeId == (int)PrintedCouponBookType.Client)
                        {
                            var customerIds = couponBook.BookTypeValue.Split(',').Select(x => int.Parse(x)).ToList();
                            var ordersWithSubtotal = orders.Where(x => customerIds.Contains(x.CustomerId) &&
                                !x.OrderItems.Select(y => y.ProductId).Contains(couponBook.ConnectedProductId))
                                .ToList();
                            foreach (var order in ordersWithSubtotal)
                            {
                                InsertCouponBookOrderItem(order, couponBook, pedidos);
                            }
                        }
                    }
                }
            }

            _logger.Information($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Finished running InsertPrintedCouponBooksScheduleTask.");
        }

        public void InsertCouponBookOrderItem(Order order, PrintedCouponBook couponBook,
            IQueryable<IGrouping<object, Order>> pedidos)
        {
            var shouldInsert = true;
            var pedidoOfOrder = pedidos.Where(x => x.Select(y => y.Id).Contains(order.Id)).FirstOrDefault();
            if (pedidoOfOrder.Count() > 1)
            {
                var hasOrderItemOfCouponBook = pedidoOfOrder.SelectMany(x => x.OrderItems)
                    .Where(x => x.ProductId == couponBook.ConnectedProductId).Any();
                if (hasOrderItemOfCouponBook)
                    shouldInsert = false;
            }
            if (shouldInsert)
            {
                var orderItem = new OrderItem
                {
                    OrderItemGuid = Guid.NewGuid(),
                    Order = order,
                    ProductId = couponBook.ConnectedProductId,
                    Quantity = 1,
                    DownloadCount = 0,
                    IsDownloadActivated = false,
                    LicenseDownloadId = 0,
                };
                order.OrderItems.Add(orderItem);
                _orderService.UpdateOrder(order);

                _logger.Information($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - " +
                    $"Inserted coupon book \"{couponBook.Name}\" ({couponBook.ConnectedProductId}) [{couponBook.Id}] to order {order.Id}.");
            }
        }
    }
}
