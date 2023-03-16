using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System;
using System.Globalization;
using System.Linq;
using Teed.Plugin.Groceries.Models.Discount;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class DiscountController : BasePluginController
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IPermissionService _permissionService;

        public DiscountController(IOrderService orderService,
            ICustomerService customerService,
            IDiscountService discountService,
            IPermissionService permissionService)
        {
            _orderService = orderService;
            _customerService = customerService;
            _discountService = discountService;
            _permissionService = permissionService;
        }

        public IActionResult ListDates()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.DiscountsBreakdown))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Discount/List.cshtml");
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.DiscountsBreakdown))
                return AccessDeniedView();

            var orders = _orderService.GetAllOrdersQuery()
                .Where(x => x.DiscountUsageHistory.Any());
            var datesGrouping = orders.Where(x => x.SelectedShippingDate != null)
                .GroupBy(x => x.SelectedShippingDate).OrderByDescending(x => x.Key);
            var dates = datesGrouping.Select(x => x.Key);
            var pagedList = new PagedList<DateTime?>(dates, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    Date = (x ?? DateTime.MinValue).ToString("dd-MM-yyyy"),
                    Discounts = datesGrouping.Where(y => y.Key.Value == x.Value)
                        .FirstOrDefault().ToList().Count()
                }).ToList(),
                Total = dates.Count()
            };

            return Ok(gridModel);
        }

        public IActionResult DiscountsBreakdown(string date)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.DiscountsBreakdown))
                return AccessDeniedView();

            var parseDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.DiscountUsageHistory.Any()
                && x.SelectedShippingDate.Value == parseDate);
            var allDiscounts = orders.SelectMany(x => x.DiscountUsageHistory)
                .Distinct().ToList();
            var discounts = orders.SelectMany(x => x.DiscountUsageHistory)
                .Select(x => x.Discount)
                .Distinct().ToList();
            var model = new DiscountsBreakdownModel
            {
                DateString = date,
                Discounts = discounts.Select(x => new Discount
                {
                    Id = x.Id,
                    Name = x.Name,
                    TimesUsed = orders.Where(y => y.DiscountUsageHistory.Select(z => z.DiscountId)
                    .Contains(x.Id)).Count(),
                    Orders = orders.Where(y => y.DiscountUsageHistory.Select(z => z.DiscountId)
                    .Contains(x.Id)).Select(w => new Order
                    {
                        Id = w.Id,
                        CustomerId = w.CustomerId,
                        Customer = w.Customer.Email,
                    }).ToList()
                }).OrderByDescending(x => x.TimesUsed).ThenBy(x => x.Name).ToList()
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Discount/DiscountsBreakdown.cshtml", model);
        }
    }
}
