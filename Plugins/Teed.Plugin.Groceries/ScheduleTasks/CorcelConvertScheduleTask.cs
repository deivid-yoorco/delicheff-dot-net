using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Tasks;
using Nop.Services.Logging;
using Nop.Services.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.Corcel;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.ScheduleTasks
{
    public class CorcelConvertScheduleTask : IScheduleTask
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly CorcelProductService _corcelProductService;
        private readonly CorcelCustomerService _corcelCustomerService;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;

        public CorcelConvertScheduleTask(IOrderService orderService,
            ICustomerService customerService, IProductService productService,
            CorcelProductService corcelProductService, CorcelCustomerService corcelCustomerService,
            ILogger logger, ISettingService settingService)
        {
            _orderService = orderService;
            _customerService = customerService;
            _productService = productService;
            _corcelProductService = corcelProductService;
            _corcelCustomerService = corcelCustomerService;
            _logger = logger;
            _settingService = settingService;
        }

        public void Execute()
        {
            //Check if its time to run
            if (TaskRunUtils.ShouldRunTask(_settingService, "CorcelConvertScheduleTask"))
            {
                _logger.Information($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Running CorcelConvertScheduleTask.");

                var datesCount = 0;
                var count = 0;
                try
                {
                    var corcelProducts = _corcelProductService.GetAll().ToList();
                    var corcelProductIds = corcelProducts.Select(x => x.ProductId).ToList();
                    var corcelCustomers = _corcelCustomerService.GetAll().ToList();
                    var corcelCustomerIds = corcelCustomers.Select(x => x.CustomerId).ToList();
                    if (corcelProducts.Any())
                    {
                        var date = DateTime.Now.Date;
                        //var firstOrderDay = OrderUtils.GetFilteredOrders(_orderService)
                        //    .Where(x => x.SelectedShippingDate != null)
                        //    .OrderBy(x => x.SelectedShippingDate)
                        //    .FirstOrDefault().SelectedShippingDate.Value;
                        //var datesBetween = Enumerable.Range(0, 1 + today.Subtract(firstOrderDay).Days)
                        //    .Select(offset => firstOrderDay.AddDays(offset))
                        //    .Where(x => x.DayOfWeek == today.DayOfWeek)
                        //    .OrderBy(x => x)
                        //    .Skip(69)
                        //    .ToList();

                        //foreach (var date in datesBetween)
                        //{

                        var lastWeekDate = date.AddDays(-7);
                        var ordersWeekQuery = OrderUtils.GetFilteredOrders(_orderService)
                            .Where(x => lastWeekDate <= x.SelectedShippingDate && x.SelectedShippingDate <= date &&
                                !corcelCustomerIds.Contains(x.CustomerId));
                        var orderIdsWeek = ordersWeekQuery.Select(x => x.Id).ToList();
                        var orderIdsWithCorcelProducts = _orderService.GetAllOrderItemsQuery()
                            .Where(x => orderIdsWeek.Contains(x.OrderId) && corcelProductIds.Contains(x.ProductId))
                            .Select(x => x.OrderId).Distinct().ToList();
                        var ordersWeek = ordersWeekQuery
                            .Where(x => orderIdsWithCorcelProducts.Contains(x.Id))
                            .ToList();

                        foreach (var order in ordersWeek)
                        {
                            count++;
                            var currentInserted = false;
                            var productsWithCorcel = order.OrderItems.Where(x => corcelProductIds.Contains(x.ProductId)).ToList();
                            corcelCustomers = _corcelCustomerService.GetAll().ToList();
                            corcelCustomerIds = corcelCustomers.Select(x => x.CustomerId).ToList();

                            if (corcelCustomerIds.Contains(order.CustomerId))
                                continue;

                            // Que compre productos de repostería y panadería de más de 1 kg (se toma de equivalencia manual).
                            foreach (var item in productsWithCorcel)
                            {
                                var productForCompare = corcelProducts.Where(x => x.ProductId == item.ProductId).FirstOrDefault();
                                if (productForCompare != null)
                                {
                                    if (productForCompare.Quantity < item.Quantity)
                                    {
                                        currentInserted = true;
                                        InsertNewCorcelCustomer(order);
                                        break;
                                    }
                                }
                            }
                            if (currentInserted)
                                continue;

                            var ordersOfCustomerByMonth = OrderUtils.GetFilteredOrders(_orderService)
                                .Where(x => x.CustomerId == order.CustomerId && orderIdsWithCorcelProducts.Contains(x.Id))
                                .Select(x => new { x.Id, x.SelectedShippingDate, x.OrderItems, x.CustomerId })
                                .ToList()
                                .GroupBy(x => x.SelectedShippingDate.Value.ToString("MM/yyyy"))
                                .ToList();

                            if (ordersOfCustomerByMonth.Count() > 1)
                            {
                                // Que en un mismo pedido compre 3 o más piezas de un mismo producto de Repostería y panadería, al menos 1 vez al mes.
                                var piecesCount = 0;
                                foreach (var orders in ordersOfCustomerByMonth)
                                {
                                    foreach (var innerOrder in orders)
                                    {
                                        productsWithCorcel = innerOrder.OrderItems.Where(x => corcelProductIds.Contains(x.ProductId)).ToList();
                                        var itemsWithMoreThan3 = productsWithCorcel.Where(x => x.Quantity >= 3).ToList();
                                        if (itemsWithMoreThan3.Any())
                                        {
                                            piecesCount++;
                                            break;
                                        }
                                    }

                                    if (ordersOfCustomerByMonth.Count() <= piecesCount)
                                    {
                                        currentInserted = true;
                                        InsertNewCorcelCustomer(order);
                                        break;
                                    }
                                }
                                if (currentInserted)
                                    continue;

                                // Que en un mismo pedido compre 5 o más productos diferentes de la categoría de Repostería y panadería, al menos 1 vez al mes.
                                var quantityCount = 0;
                                foreach (var orders in ordersOfCustomerByMonth)
                                {
                                    foreach (var innerOrder in orders)
                                    {
                                        productsWithCorcel = innerOrder.OrderItems.Where(x => corcelProductIds.Contains(x.ProductId)).ToList();
                                        if (productsWithCorcel.Count() >= 5)
                                        {
                                            quantityCount++;
                                            break;
                                        }
                                    }

                                    if (ordersOfCustomerByMonth.Count() <= quantityCount)
                                    {
                                        currentInserted = true;
                                        InsertNewCorcelCustomer(order);
                                        break;
                                    }
                                }
                                if (currentInserted)
                                    continue;
                            }

                            // Que la dirección contenga la palabra "local", "panadería", "repostería", "cafetería".
                            var address = $"{order.ShippingAddress.Address1}, {order.ShippingAddress.Address2}".ToLower().Trim();
                            if (address.Contains("local") || address.Contains("panadería") || address.Contains("repostería") || address.Contains("cafetería") ||
                                address.Contains("panaderia") || address.Contains("reposteria") || address.Contains("cafeteria"))
                                InsertNewCorcelCustomer(order);
                        }
                        datesCount++;
                        //}
                    }
                }
                catch (Exception e)
                {
                    _logger.Error($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Error while running CorcelConvertScheduleTask:\n\"{e.Message}\".", e);
                }

                _logger.Information($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Finished running CorcelConvertScheduleTask.");
            }
        }

        public void InsertNewCorcelCustomer(Order order)
        {
            if (order != null)
            {
                var corcelCustomer = new CorcelCustomer
                {
                    CustomerId = order.CustomerId,
                    OrderId = order.Id,
                    DateConvertedUtc = DateTime.UtcNow,
                    Log = $"Se insertó un nuevo cliente CORCEL de forma automática.\n"
                };
                _corcelCustomerService.Insert(corcelCustomer);
            }
        }
    }
}
