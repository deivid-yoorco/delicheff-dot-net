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
using Nop.Web.Areas.Admin.Models.Orders;

namespace Teed.Plugin.Groceries.Components
{
    [ViewComponent(Name = "RouteFranchise")]
    public class RouteFranchiseComponent : NopViewComponent
    {
        private readonly ShippingVehicleRouteService _shippingVehicleRouteService;
        private readonly IOrderService _orderService;

        public RouteFranchiseComponent(ShippingVehicleRouteService shippingVehicleRouteService,
            IOrderService orderService)
        {
            _shippingVehicleRouteService = shippingVehicleRouteService;
            _orderService = orderService;
        }

        public IViewComponentResult Invoke(string widgetZone, OrderModel additionalData)
        {
            var dateShippinng = additionalData.SelectedShippingDate;
            var order = _orderService.GetOrderById(additionalData.Id);
            var idRoute = additionalData.RouteId;
            var model = new FranchiseTextModel()
            {
                FranchiseName = "Franquicia no asignada",
                RescuedyFranchiseName = null
            };

            if (idRoute > 0)
            {
                var franchise = _shippingVehicleRouteService.GetAll().Where(x => x.ShippingDate == dateShippinng && x.RouteId == idRoute).FirstOrDefault();
                if (franchise != null)
                {
                    model.FranchiseName = franchise.Vehicle?.Franchise?.Name;
                    if (string.IsNullOrEmpty(model.FranchiseName))
                    {
                        model.FranchiseName = "Franquicia no asignada";
                    }
                }
            }

            if (order.RescuedByRouteId.HasValue)
            {
                var rescuedFranchise = _shippingVehicleRouteService.GetAll().Where(x => x.ShippingDate == dateShippinng && x.RouteId == order.RescuedByRouteId.Value).FirstOrDefault();
                if (rescuedFranchise != null)
                    model.RescuedyFranchiseName = rescuedFranchise?.Vehicle?.Franchise?.Name;
            }

            return View("~/Plugins/Teed.Plugin.Groceries/Views/RouteFranchise/RouteFranchise.cshtml", model);
        }
    }

    public class FranchiseTextModel
    {
        public string FranchiseName { get; set; }
        public string RescuedyFranchiseName { get; set; }
    }
}
