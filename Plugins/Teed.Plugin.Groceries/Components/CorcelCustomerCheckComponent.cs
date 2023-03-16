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
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;
using Teed.Plugin.Groceries.Models.Corcel;

namespace Teed.Plugin.Groceries.Components
{
    [ViewComponent(Name = "CorcelCustomerCheck")]
    public class CorcelCustomerCheckComponent : NopViewComponent
    {
        private readonly ShippingVehicleRouteService _shippingVehicleRouteService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly CorcelCustomerService _corcelCustomerService;
        private readonly CorcelProductService _corcelProductService;

        public CorcelCustomerCheckComponent(ShippingVehicleRouteService shippingVehicleRouteService,
            IOrderService orderService, ICustomerService customerService,
            CorcelCustomerService corcelCustomerService, CorcelProductService corcelProductService)
        {
            _shippingVehicleRouteService = shippingVehicleRouteService;
            _orderService = orderService;
            _customerService = customerService;
            _corcelCustomerService = corcelCustomerService;
            _corcelProductService = corcelProductService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var model = new CorcelCustomerModel();
            var customerId = (int)additionalData;
            var customer = _customerService.GetCustomerById(customerId);
            if (customer != null)
            {
                var corcelCustomer = _corcelCustomerService.GetAll()
                    .Where(x => x.CustomerId == customer.Id).FirstOrDefault();
                var order = _orderService.GetOrderById(corcelCustomer?.OrderId ?? 0);
                if (order != null)
                    model.CorcelifiedDate = order.SelectedShippingDate.Value;

                model.CorcelRulesThatApply = CorcelUtils.CorcelAppliesToCustomer(customer, _corcelProductService, _corcelCustomerService, _orderService);
            }

            return View("~/Plugins/Teed.Plugin.Groceries/Views/CorcelCustomer/ConvertedCustomer.cshtml", model);
        }
    }
}
