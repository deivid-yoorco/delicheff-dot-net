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
    [ViewComponent(Name = "OrderAlerts")]
    public class OrderAlerViewComponent : NopViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ShippingVehicleRouteService _shippingVehicleRouteService;

        public OrderAlerViewComponent(ICustomerService customerService, IWorkContext workContext,
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
            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);
            var afterTomorrow = today.AddDays(2);
            var beforeYesterday = today.AddDays(-2);
            var ordersTodayTomorrow = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate == tomorrow || x.SelectedShippingDate == today).ToList();
            var pedidosTodayTomorrow = OrderUtils.GetPedidosGroupByList(ordersTodayTomorrow).ToList();

            var allOrdersTodayNextTwoDays = _orderService.GetAllOrdersQuery().Where(x => x.SelectedShippingDate >= today && x.SelectedShippingDate <= afterTomorrow).ToList();

            List<IGrouping<int, Order>> ordersSameCustomer = ordersTodayTomorrow.Where(x => x.SelectedShippingDate == tomorrow).GroupBy(x => x.CustomerId).Where(x => x.Count() > 1).ToList();
            List<IGrouping<dynamic, Order>> pedidosDuplicatedProduct = pedidosTodayTomorrow.Where(x => x.Count() > 1)
                .Where(x => x.SelectMany(y => y.OrderItems).GroupBy(y => y.ProductId).Where(y => y.Count() > 1).Any())
                .ToList();
            List<Order> notPayedPaypal = allOrdersTodayNextTwoDays.Where(x => x.OrderStatusId != 40 &&
                x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10)
                .ToList();

            var orderSameClientDifferentDirection = ordersTodayTomorrow.GroupBy(x => x.CustomerId).Where(x => x.GroupBy(y => y.ShippingAddress.Address1).Count() > 1).ToList();
            var ordersWithNoNumber = _orderService.GetAllOrdersQuery().Where(x => x.SelectedShippingDate == tomorrow && x.OrderItems.Count == 0).ToList();

            var ordersBigAmount = ordersTodayTomorrow.Where(x => x.OrderTotal >= 5000).ToList();
            var ordersCardNotPayed = ordersTodayTomorrow.Where(x => x.SelectedShippingDate == tomorrow && x.PaymentMethodSystemName == "Payments.Visa" && x.PaymentStatusId == (int)Nop.Core.Domain.Payments.PaymentStatus.Pending).ToList();

            var badOrders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.CustomOrderNumber == null || x.CustomOrderNumber == "").ToList();

            var tomorrowCustomersWithOrders = allOrdersTodayNextTwoDays.Where(x => x.SelectedShippingDate == tomorrow).Select(x => x.CustomerId).Distinct().ToList();
            var customerEmailsWithOrdersLastTwoDays = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= beforeYesterday && x.SelectedShippingDate <= today && tomorrowCustomersWithOrders.Contains(x.CustomerId))
                .Select(x => new CustomerSimpleDataModel() { Email = x.Customer.Email, Id = x.CustomerId })
                .Distinct()
                .ToList(); 

            var ordersWithoutPaymentMethod = ordersTodayTomorrow.Where(x => string.IsNullOrEmpty(x.PaymentMethodSystemName)).ToList();
            foreach (var order in badOrders)
            {
                _orderProcessingService.CancelOrder(order, false);
            }

            var payedAndCancelled = allOrdersTodayNextTwoDays.Where(x => x.OrderStatus == OrderStatus.Cancelled && x.PaymentStatus == Nop.Core.Domain.Payments.PaymentStatus.Paid).ToList();

            var model = new OrderAlertsModel()
            {
                NotPayedPaypal = notPayedPaypal,
                OrdersSameCustomer = ordersSameCustomer,
                OrdersBigAmount = ordersBigAmount,
                PedidosDuplicatedProduct = pedidosDuplicatedProduct,
                OrdersWithDuplicatedProducts = ordersTodayTomorrow.Where(x => x.SelectedShippingDate == tomorrow).Where(x => x.OrderItems.GroupBy(y => y.ProductId).Where(y => y.Count() > 1).Any()).ToList(),
                OrderSameClientDifferentDirection = orderSameClientDifferentDirection,
                OrdersWithNoNumber = ordersWithNoNumber,
                PayedAndCancelled = payedAndCancelled,
                OrdersCardNotPayed = ordersCardNotPayed,
                OrdersWithoutPaymentMethod = ordersWithoutPaymentMethod,
                CustomerEmailsWithOrdersLastTwoDays = customerEmailsWithOrdersLastTwoDays
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Shared/Components/OrderAlert/Default.cshtml", model);
        }
    }

    public class OrderAlertsModel
    {
        public List<IGrouping<int, Order>> OrdersSameCustomer { get; set; }
        public List<IGrouping<dynamic, Order>> PedidosDuplicatedProduct { get; set; }
        public List<Order> NotPayedPaypal { get; set; }
        public List<Order> OrdersBigAmount { get; set; }
        public List<Order> OrdersCardNotPayed { get; set; }
        public List<Order> OrdersWithDuplicatedProducts { get; set; }
        public List<Order> OrdersWithNoNumber { get; set; }
        public List<IGrouping<int, Order>> OrderSameClientDifferentDirection { get; set; }
        public List<Order> PayedAndCancelled { get; set; }
        public List<Order> OrdersWithoutPaymentMethod { get; set; }
        public List<CustomerSimpleDataModel> CustomerEmailsWithOrdersLastTwoDays { get; set; }
    }

    public class CustomerSimpleDataModel
    {
        public string Email { get; set; }
        public int Id { get; set; }
    }
}
