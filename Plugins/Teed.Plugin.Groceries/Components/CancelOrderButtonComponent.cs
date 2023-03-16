using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Orders;
using Teed.Plugin.Groceries.Models.CancelOrderButtonModel;

namespace Teed.Plugin.Groceries.Components
{
    [ViewComponent(Name = "CancelOrderButton")]
    public class CancelOrderButtonComponent : NopViewComponent
    {
        private readonly IOrderService _orderService;

        public CancelOrderButtonComponent(IOrderService orderService)
            {
            this._orderService = orderService;
        }

        public IViewComponentResult Invoke(string widgetZone, int additionalData)
        {
            var orderId = additionalData;
            var order = _orderService.GetOrderById(orderId);
            var dateToday = DateTime.Now;
            var filteredDateShipping = order.SelectedShippingDate ?? DateTime.Now;
            var shipindMenor = new DateTime(filteredDateShipping.Year, filteredDateShipping.Month, filteredDateShipping.Day, 21, 0, 0).AddDays(-1);
            var model = new CancelOrderButtonModel()
            {
                OrderId = order == null ? 0 : order.Id,
                CancelButtonEnable = order != null && dateToday < shipindMenor,
                StatusOrder = order == null ? 0 : order.OrderStatusId
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/CancelOrderButton/CancelOrderButton.cshtml", model);
        }
    }
}
