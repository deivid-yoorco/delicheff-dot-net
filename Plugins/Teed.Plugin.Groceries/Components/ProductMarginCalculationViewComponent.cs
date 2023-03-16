using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingVehicles;
using Teed.Plugin.Groceries.Models.OrderOptionButtons;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Components
{
    [ViewComponent(Name = "ProductMarginCalculation")]
    public class ProductMarginCalculationViewComponent : NopViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IProductService _productService;
        private readonly ShippingVehicleRouteService _shippingVehicleRouteService;
        private readonly OrderReportStatusService _orderReportStatusService;
        private readonly Services.OrderReportService _orderReportService;

        public ProductMarginCalculationViewComponent(ICustomerService customerService, IWorkContext workContext,
            IOrderService orderService, ShippingVehicleRouteService shippingVehicleRouteService,
            IOrderProcessingService orderProcessingService,
            IProductService productService, OrderReportStatusService orderReportStatusService,
            Services.OrderReportService orderReportService)
        {
            _customerService = customerService;
            _workContext = workContext;
            _orderService = orderService;
            _shippingVehicleRouteService = shippingVehicleRouteService;
            _orderProcessingService = orderProcessingService;
            _productService = productService;
            _orderReportStatusService = orderReportStatusService;
            _orderReportService = orderReportService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            int productId = (int)additionalData;
            var reportStatus = _orderReportStatusService.GetAll().Where(x => x.StatusTypeId == 2)
                .Select(x => new ProductMarginGroup() { BuyerId = x.BuyerId, ShippingDate = x.ShippingDate })
                .Distinct()
                .ToList();

            var reports = _orderReportService.GetAll()
                .Where(x => x.ProductId == productId && x.UpdatedUnitCost > 0)
                .ToList()
                .Where(x => reportStatus.Where(y => y.BuyerId == x.OriginalBuyerId && y.ShippingDate == x.OrderShippingDate).Any())
                .GroupBy(x => x.OrderShippingDate)
                .OrderByDescending(x => x.Key)
                .Take(10)
                .Select(x => x.FirstOrDefault())
                .ToList();
            List<int> buyerIds = reports.GroupBy(x => x.OriginalBuyerId).Select(x => x.Key).ToList();
            List<Nop.Core.Domain.Customers.Customer> customers = _customerService.GetAllCustomersQuery().Where(x => buyerIds.Contains(x.Id)).ToList();

            List<string> costList = reports.Select(x => 
            {
                string buyerName = customers.Where(y => y.Id == x.OriginalBuyerId)?.FirstOrDefault()?.GetFullName() ?? "";
                string url = $"/Admin/OrderDeliveryReports/OrderReportDetails?buyerId={x.OriginalBuyerId}&date={x.OrderShippingDate:dd-MM-yyyy}";
                if (x.SentToSupermarket.HasValue && x.SentToSupermarket.Value)
                {
                    url = $"/Admin/OrderDeliveryReports/OrderSuperReportDetails?date={x.OrderShippingDate:dd-MM-yyyy}";
                }
                return $"{x.OrderShippingDate:dd-MM-yyyy} / {buyerName} / <a href=\"{url}\" target=\"_blank\">{x.UpdatedUnitCost:C}</a>";
            }).ToList();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Shared/Components/ProductMarginCalculation/Default.cshtml", costList);
        }
    }

    public class ProductMarginGroup
    {
        public DateTime ShippingDate { get; set; }
        public int BuyerId { get; set; }
    }
}
