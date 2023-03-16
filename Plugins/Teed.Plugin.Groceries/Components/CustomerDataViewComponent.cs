using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingVehicles;
using Teed.Plugin.Groceries.Models.OrderOptionButtons;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Components
{
    [ViewComponent(Name = "CustomerData")]
    public class CustomerDataViewComponent : NopViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ShippingVehicleRouteService _shippingVehicleRouteService;

        public CustomerDataViewComponent(ICustomerService customerService, IWorkContext workContext,
            IOrderService orderService, ShippingVehicleRouteService shippingVehicleRouteService,
            IOrderProcessingService orderProcessingService)
        {
            _customerService = customerService;
            _workContext = workContext;
            _orderService = orderService;
            _shippingVehicleRouteService = shippingVehicleRouteService;
            _orderProcessingService = orderProcessingService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var customerId = (int)additionalData;
            var customer = _customerService.GetCustomerById(customerId);
            var customerOrders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.CustomerId == customerId)
                .ToList();

            var completedOrders = customerOrders.Where(x => x.OrderStatus == OrderStatus.Complete).ToList();
            List<IGrouping<dynamic, Order>> completedPedidos = OrderUtils.GetPedidosGroupByList(completedOrders).ToList();
            var totalAmount = completedOrders.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
            var averageTicket = completedPedidos.Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();
            var averageProductCount = completedPedidos.Select(x => x.SelectMany(y => y.OrderItems).Count()).DefaultIfEmpty().Average();
            var firstPurchaseDate = completedOrders.Select(x => x.SelectedShippingDate.Value).OrderBy(x => x).FirstOrDefault();
            var lastPurchaseDate = completedOrders.Select(x => x.SelectedShippingDate.Value).OrderBy(x => x).LastOrDefault();
            var completedPedidosCount = completedPedidos.Count;
            var months = (decimal)(lastPurchaseDate - firstPurchaseDate).TotalDays / 30;
            decimal monthlyRecurrence = months == 0 ? 0 : completedPedidosCount / months;
            var deliveredInTimeCount = DeliveredInTimeCount(completedPedidos);
            var deliveredInTimeData = completedPedidosCount == 0 ? "S/I" : $"{deliveredInTimeCount}/{completedPedidosCount} ({Math.Round((decimal)deliveredInTimeCount / (decimal)completedPedidosCount, 2) * 100}%)";

            var model = new CustomerDataModel()
            {
                AverageProductCount = averageProductCount,
                AverageTicket = averageTicket,
                TotalAmount = totalAmount,
                CompletedPedidosCount = completedPedidosCount,
                DeliveredInTimeData = deliveredInTimeData,
                FirstPurchaseDate = firstPurchaseDate,
                LastPurchaseDate = lastPurchaseDate,
                MonthlyRecurrence = Math.Round(monthlyRecurrence, 2),
                Orders = customerOrders
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Shared/Components/CustomerData/Default.cshtml", model);
        }

        private int DeliveredInTimeCount(List<IGrouping<dynamic, Order>> completedPedidos)
        {
            int inTimeCount = 0;
            foreach (var pedido in completedPedidos)
            {
                var deliveredDate = pedido.FirstOrDefault()
                    .Shipments.Where(x => x.DeliveryDateUtc.HasValue)
                    .Select(x => x.DeliveryDateUtc.Value)
                    .OrderBy(x => x)
                    .FirstOrDefault()
                    .ToLocalTime();
                var selectedShippingTime = pedido.FirstOrDefault().SelectedShippingTime;
                if (OrderUtils.CheckIfDeliveredInTime(selectedShippingTime, deliveredDate))
                    inTimeCount++;
            }
            return inTimeCount;
        }
    }

    public class CustomerDataModel
    {
        public int CompletedPedidosCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageTicket { get; set; }
        public double AverageProductCount { get; set; }
        public DateTime FirstPurchaseDate { get; set; }
        public DateTime LastPurchaseDate { get; set; }
        public decimal MonthlyRecurrence { get; set; }
        public string DeliveredInTimeData { get; set; }
        public List<Order> Orders { get; set; }
    }
}
