using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.MarketingDashboard.Models.MarketingExpenses;
using Teed.Plugin.MarketingDashboard.Services;
using Teed.Plugin.MarketingDashboard.Utils;

namespace Teed.Plugin.MarketingDashboard.Controllers
{
    [Area(AreaNames.Admin)]
    public class MonitorHqController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IOrderService _orderService;
        private readonly ISettingService _settingService;
        private readonly ICustomerService _customerService;
        private readonly MarketingExpenseService _marketingExpenseService;
        private readonly MarketingDashboardDataService _marketingDashboardDataService;

        public MonitorHqController(IPermissionService permissionService,
            IOrderService orderService,
            ISettingService settingService,
            ICustomerService customerService,
            MarketingExpenseService marketingExpenseService,
            MarketingDashboardDataService marketingDashboardDataService)
        {
            _permissionService = permissionService;
            _orderService = orderService;
            _settingService = settingService;
            _marketingExpenseService = marketingExpenseService;
            _customerService = customerService;
            _marketingDashboardDataService = marketingDashboardDataService;
        }

        [HttpGet]
        public IActionResult GetDashboard3(bool fromAdmin)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var model = new MarketingDashboardDataModel();
            PrepareModel(ref model, 52);
            model.Data = model.Data.OrderBy(x => x.InitDate).ToList();
            model.FromAdmin = fromAdmin;

            return View("~/Plugins/Teed.Plugin.MarketingDashboard/Views/MonitorHq/Dashboard3.cshtml", model);
        }

        [HttpGet]
        public IActionResult GetDashboard4(bool fromAdmin)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var model = new MarketingDashboardDataModel();
            PrepareModel(ref model, 52);
            model.Data = model.Data.OrderBy(x => x.InitDate).ToList();
            model.FromAdmin = fromAdmin;

            return View("~/Plugins/Teed.Plugin.MarketingDashboard/Views/MonitorHq/Dashboard4.cshtml", model);
        }

        private void PrepareModel(ref MarketingDashboardDataModel model, int weeksCount)
        {
            var dashboardData = _marketingDashboardDataService.GetAll()
                .Where(x => x.MarketingDashboardDataTypeId == 10)
                .GroupBy(x => x.GenerationDateUtc)
                .OrderByDescending(x => x.Key)
                .FirstOrDefault()
                .OrderByDescending(x => x.InitDate)
                .Take(weeksCount)
                .OrderBy(x => x.InitDate);

            model = new MarketingDashboardDataModel()
            {
                Data = new List<MarketingDashboardDataItem>()
            };

            if (dashboardData != null)
            {
                foreach (var data in dashboardData)
                {
                    model.Data.Add(new MarketingDashboardDataItem()
                    {
                        AdquisitionCost = data.AdquisitionCost,
                        AverageTicket = data.AverageTicket,
                        CustomerAnnualValue = data.CustomerAnnualValue,
                        DailyPedidosAverage = data.DailyPedidosAverage,
                        DailySalesAverage = data.DailySalesAverage,
                        NewActiveCount = data.NewActiveCount,
                        BuyinRegister = data.BuyinRegister,
                        CustomerCount120Days = data.CustomerCount120Days,
                        CustomerCount30Days = data.CustomerCount30Days,
                        CustomerCount60Days = data.CustomerCount60Days,
                        CustomerCount90Days = data.CustomerCount90Days,
                        CustomFormula1 = data.CustomFormula1,
                        EndDate = data.EndDate,
                        FirstOrderCount30Days = data.FirstOrderCount30Days,
                        FirstOrderCount90Days = data.FirstOrderCount90Days,
                        InitDate = data.InitDate,
                        MarketingExpenses = data.MarketingExpenses,
                        MonthlyChurnRate = data.MonthlyChurnRate,
                        NewRegisterCost = data.NewRegisterCost,
                        NewRegisteredUsersCount = data.NewRegisteredUsersCount,
                        PedidosCount = data.PedidosCount,
                        QuarterlyChurnRate = data.QuarterlyChurnRate,
                        Recurrence = data.Recurrence,
                        SalesTotal = data.SalesTotal,
                        TotalPedidos30Days = data.TotalPedidos30Days,
                        WorkingDays = data.WorkingDays,
                        AverageTicketNewCustomers = data.AverageTicketNewCustomers,
                        AverageTicketOldCustomers = data.AverageTicketOldCustomers,
                        CustomersWithOneOrderBetween150and121days = data.CustomersWithOneOrderBetween150and121days,
                        CustomersWithOneOrderBetween150and121daysAnd120daysPedido = data.CustomersWithOneOrderBetween150and121daysAnd120daysPedido,
                        CustomerAnnualContribution = data.CustomerAnnualContribution,
                        CustomerAnnualContribution120Retention = data.CustomerAnnualContribution120Retention,
                        CustomerAnnualContribution120Retention2or3 = data.CustomerAnnualContribution120Retention2or3,
                        CustomerAnnualContribution120Retention4or5 = data.CustomerAnnualContribution120Retention4or5,
                        CustomerAnnualContribution120Retention6or7 = data.CustomerAnnualContribution120Retention6or7,
                        CustomerAnnualContribution120Retention8or9 = data.CustomerAnnualContribution120Retention8or9,
                        CustomerAnnualContribution120RetentionMoreThan10 = data.CustomerAnnualContribution120RetentionMoreThan10,
                        CustomerAnnualContributionRetention = data.CustomerAnnualContributionRetention,
                        CustomersCount2or3Pedidos120DaysAndMore = data.CustomersCount2or3Pedidos120DaysAndMore,
                        CustomersCount4or5Pedidos120DaysAndMore = data.CustomersCount4or5Pedidos120DaysAndMore,
                        CustomersCount6or7Pedidos120DaysAndMore = data.CustomersCount6or7Pedidos120DaysAndMore,
                        CustomersCount8or9Pedidos120DaysAndMore = data.CustomersCount8or9Pedidos120DaysAndMore,
                        CustomersCountAtLeastOneOrder120DaysAndMore = data.CustomersCountAtLeastOneOrder120DaysAndMore,
                        CustomersCountMoreThan10Pedidos120DaysAndMore = data.CustomersCountMoreThan10Pedidos120DaysAndMore,
                        CustomersCountOnlyOnePedido120DaysAndMore = data.CustomersCountOnlyOnePedido120DaysAndMore,
                        CustomFormula2 = data.CustomFormula2,
                        FirstPedidosCount = data.FirstPedidosCount,
                        NotFirstOrdersCount = data.NotFirstOrdersCount,
                        Recurrence120days = data.Recurrence120days,
                        Recurrence120days2or3 = data.Recurrence120days2or3,
                        Recurrence120days4or5 = data.Recurrence120days4or5,
                        Recurrence120days6or7 = data.Recurrence120days6or7,
                        Recurrence120days8or9 = data.Recurrence120days8or9,
                        Recurrence120daysMoreThan10 = data.Recurrence120daysMoreThan10,
                        RetentionRate120Days = data.RetentionRate120Days,
                        SalesFirstOrders = data.SalesFirstOrders,
                        SalesNotFirstOrders = data.SalesNotFirstOrders
                    });
                }

                model.Data = model.Data.OrderByDescending(x => x.InitDate).ToList();
            }
        }
    }
}
