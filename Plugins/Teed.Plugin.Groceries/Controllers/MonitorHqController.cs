using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Rewards;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Rewards;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Models.MonitorHq;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class MonitorHqController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IOrderService _orderService;
        private readonly ISettingService _settingService;
        private readonly ShippingRegionZoneService _shippingRegionZoneService;
        private readonly ShippingZoneService _shippingZoneService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerBalanceService _customerBalanceService;
        private readonly CorcelCustomerService _corcelCustomerService;

        public MonitorHqController(IPermissionService permissionService,
            IOrderService orderService,
            ISettingService settingService,
            ShippingRegionZoneService shippingRegionZoneService,
            ShippingZoneService shippingZoneService,
            ICustomerService customerService,
            ICustomerBalanceService customerBalanceService,
            CorcelCustomerService corcelCustomerService)
        {
            _permissionService = permissionService;
            _orderService = orderService;
            _settingService = settingService;
            _shippingRegionZoneService = shippingRegionZoneService;
            _shippingZoneService = shippingZoneService;
            _customerService = customerService;
            _customerBalanceService = customerBalanceService;
            _corcelCustomerService = corcelCustomerService;
        }


        [HttpGet]
        [Route("Admin/Order/[Action]")]
        public IActionResult GetDashboard1(bool fromAdmin)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.DashboardHq))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/MonitorHq/Dashboard1.cshtml", fromAdmin);
        }

        [HttpGet]
        [Route("Admin/Order/[Action]")]
        public IActionResult GetDashboard2(bool fromAdmin)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.DashboardHq))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/MonitorHq/Dashboard2.cshtml", fromAdmin);
        }

        [HttpGet]
        [Route("Admin/Order/[Action]")]
        public IActionResult GetDashboard1Data()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.DashboardHq))
                return AccessDeniedView();

            var today = DateTime.Now.Date;
            var selectedDate = today.AddDays(1);
            if (today.DayOfWeek == DayOfWeek.Saturday)
                selectedDate = selectedDate.AddDays(1);
            if (selectedDate.Day == 16 && selectedDate.Month == 9)
                selectedDate = selectedDate.AddDays(1);

            var data = OrderUtils.GetTimesPedidosData(selectedDate, _settingService, _orderService, _shippingRegionZoneService, _shippingZoneService, _corcelCustomerService, true);

            return Ok(data);
        }

        [HttpGet]
        [Route("Admin/Order/[Action]")]
        public IActionResult GetDashboard2Data()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.DashboardHq))
                return AccessDeniedView();

            var today = DateTime.Now.Date;
            var selectedDate = today.AddDays(1);
            if (today.DayOfWeek == DayOfWeek.Saturday)
                selectedDate = selectedDate.AddDays(1);
            if (selectedDate.Day == 16 && selectedDate.Month == 9)
                selectedDate = selectedDate.AddDays(1);

            return Ok(OrderUtils.GetTimesSellsData(selectedDate, _settingService, _orderService, _shippingRegionZoneService, _shippingZoneService, _corcelCustomerService, true));
        }

        public IActionResult BalanceUsageMonitor(string date = null)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.BalanceManager))
                return AccessDeniedKendoGridJson();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/MonitorHq/BalanceUsageMonitor.cshtml", GetBalanceUsageModel(date));
        }

        [HttpPost]
        public IActionResult BalanceGiven(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.BalanceManager))
                return AccessDeniedKendoGridJson();

            var userBalance = _customerBalanceService.GetAllCustomerBalancesQuery().OrderByDescending(x => x.CreatedOnUtc);
            var pagedList = new PagedList<CustomerBalance>(userBalance, command.Page - 1, command.PageSize);

            var customerIds = pagedList.Select(x => x.CustomerId)
                .Union(pagedList.Select(x => x.GivenByCustomerId))
                .Distinct().ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => customerIds.Contains(x.Id))
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    Date = x.CreatedOnUtc.ToLocalTime().ToString("dd/MM/yyyy hh:mm:ss tt"),
                    x.CustomerId,
                    Customer = GetFormattedCustomerInfo(customers, x.CustomerId),
                    Amount = x.Amount.ToString("C"),
                    Comment = $"{(!string.IsNullOrEmpty(x.Comment) ? x.Comment : "Sin comentario especificado")}{(x.Deleted ? " <span class=\"text-red\">(El saldo fue eliminado)</span>" : "")}",
                    x.GivenByCustomerId,
                    GivenByCustomer = GetFormattedCustomerInfo(customers, x.GivenByCustomerId),
                    Log = x.Log.Replace("\n", "<br/>")
                })
                .ToList(),
                Total = userBalance.Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult BalanceUsed(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.BalanceManager))
                return AccessDeniedKendoGridJson();

            var orders = _orderService.GetAllOrdersQuery()
                .Where(x => x.OrderStatusId != 40
                && x.CustomerBalanceUsedAmount > 0)
                .OrderByDescending(x => x.CreatedOnUtc);
            var pagedList = new PagedList<Order>(orders, command.Page - 1, command.PageSize);

            var customerIds = pagedList.Select(x => x.CustomerId).ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => customerIds.Contains(x.Id))
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    CreatedOn = x.CreatedOnUtc.ToLocalTime().ToString("dd/MM/yyyy hh:mm:ss tt"),
                    x.CustomerId,
                    Customer = GetFormattedCustomerInfo(customers, x.CustomerId),
                    Amount = (x.CustomerBalanceUsedAmount ?? 0).ToString("C"),
                    x.Id,
                    SelectedShippingDate = x.SelectedShippingDate.Value.ToString("dd/MM/yyyy")
                })
                .ToList(),
                Total = orders.Count()
            };
            return Json(gridModel);
        }

        [HttpGet]
        public IActionResult GetCurrentBalance(string date = null)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.BalanceManager))
                return AccessDeniedKendoGridJson();

            var model = GetBalanceUsageModel(date);
            return Json(new
            {
                BalanceGivenToday = model.BalanceGivenToday.ToString("C"),
                BalanceGivenWeek = model.BalanceGivenWeek.ToString("C"),
                BalanceUsedToday = model.BalanceUsedToday.ToString("C"),
                BalanceUsedWeek = model.BalanceUsedWeek.ToString("C"),
            });
        }

        public string GetFormattedCustomerInfo(List<Customer> customers, int currentId)
        {
            var currentCustomer = customers.Where(x => x.Id == currentId).FirstOrDefault();
            if (currentCustomer == null)
                return "No encontrado";
            else
                return $"{currentCustomer.GetFullName()} ({currentCustomer.Email})";
        }

        private BalanceUsageModel GetBalanceUsageModel(string date = null)
        {
            var today = DateTime.Now.Date;
            if (!string.IsNullOrEmpty(date))
            {
                var parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;
                today = parsedDate;
            }
            var oneWeekAgo = today.AddDays(-7);
            var customerBalance = _customerBalanceService.GetAllCustomerBalancesQuery()
                .Where(x => oneWeekAgo <= DbFunctions.TruncateTime(x.CreatedOnUtc));
            var orders = _orderService.GetAllOrdersQuery()
                .Where(x => x.OrderStatusId != 40
                && x.CustomerBalanceUsedAmount > 0 &&
                oneWeekAgo <= DbFunctions.TruncateTime(x.SelectedShippingDate));

            return new BalanceUsageModel
            {
                SpecificDate = date,
                BalanceGivenToday = customerBalance
                .ToList()
                .Where(x => x.CreatedOnUtc.ToLocalTime().Date == today)
                .Select(x => x.Amount).DefaultIfEmpty().Sum(),

                BalanceGivenWeek = customerBalance
                .Select(x => x.Amount).DefaultIfEmpty().Sum(),

                BalanceUsedToday = orders
                .Where(x => DbFunctions.TruncateTime(x.SelectedShippingDate) == today)
                .Select(x => x.CustomerBalanceUsedAmount ?? 0)
                .DefaultIfEmpty().Sum(),

                BalanceUsedWeek = orders
                .Select(x => x.CustomerBalanceUsedAmount ?? 0)
                .DefaultIfEmpty().Sum()
            };
        }
    }
}
