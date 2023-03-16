using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Teed.Plugin.MarketingDashboard.Models.MarketingExpenses;
using Teed.Plugin.MarketingDashboard.Security;
using Teed.Plugin.MarketingDashboard.Services;
using Teed.Plugin.MarketingDashboard.Utils;

namespace Teed.Plugin.MarketingDashboard.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class MarketingDashboardController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly ISettingService _settingService;
        private readonly MarketingExpenseService _marketingExpenseService;
        private readonly MarketingExpenseTypeService _marketingExpenseTypeService;
        private readonly MarketingGrossMarginService _marketingGrossMarginService;
        private readonly MarketingDashboardDataService _marketingDashboardDataService;
        private readonly MarketingRetentionRateService _marketingRetentionRateService;
        private readonly MarketingAutomaticExpenseService _marketingAutomaticExpenseService;
        private readonly MarketingDiscountExpenseService _marketingDiscountExpenseService;

        public MarketingDashboardController(IPermissionService permissionService,
            MarketingExpenseService marketingExpenseService, IWorkContext workContext,
            MarketingExpenseTypeService marketingExpenseTypeService, IOrderService orderService,
            ICustomerService customerService, MarketingGrossMarginService marketingGrossMarginService,
            MarketingDashboardDataService marketingDashboardDataService, MarketingRetentionRateService marketingRetentionRateService,
            ISettingService settingService, MarketingAutomaticExpenseService marketingAutomaticExpenseService,
            MarketingDiscountExpenseService marketingDiscountExpenseService)
        {
            _permissionService = permissionService;
            _marketingExpenseService = marketingExpenseService;
            _workContext = workContext;
            _marketingExpenseTypeService = marketingExpenseTypeService;
            _orderService = orderService;
            _customerService = customerService;
            _marketingGrossMarginService = marketingGrossMarginService;
            _marketingDashboardDataService = marketingDashboardDataService;
            _marketingRetentionRateService = marketingRetentionRateService;
            _settingService = settingService;
            _marketingAutomaticExpenseService = marketingAutomaticExpenseService;
            _marketingDiscountExpenseService = marketingDiscountExpenseService;
        }

        public IActionResult DashboardData()
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var firtOrderDate = _orderService.GetAllOrdersQuery()
                .Where(x => x.SelectedShippingDate != null)
                .OrderBy(x => x.SelectedShippingDate)
                .FirstOrDefault().SelectedShippingDate.Value.Date;
            var lastDate = DateTime.Now.Date;
            var allMondays = Enumerable.Range(0, 1 + lastDate.Subtract(firtOrderDate).Days)
              .Select(offset => firtOrderDate.AddDays(offset))
              .Where(x => x.DayOfWeek == DayOfWeek.Monday)
              .OrderByDescending(x => x)
              .ToList();

            var lastUpdateDate = _settingService.LoadSetting<MarketingDashboardSettings>().LastDashboardDataUpdateUtc;
            var dashboardData = _marketingDashboardDataService.GetAll()
                .Where(x => x.MarketingDashboardDataTypeId == 10)
                .GroupBy(x => x.GenerationDateUtc)
                .OrderByDescending(x => x.Key)
                .FirstOrDefault();

            var model = new MarketingDashboardDataModel()
            {
                ControlDate = lastUpdateDate.ToLocalTime(),
                Data = new List<MarketingDashboardDataItem>(),

                GenerationDates = allMondays.Select(x => new SelectListItem
                {
                    Text = $"De {x:dd/MM/yyyy} a {x.AddDays(6):dd/MM/yyyy}"
                        // + $"{(dashboardData.Where(y => y.InitDate.Date == x).FirstOrDefault() != null ? "Creado" : "Aún no creado")})"
                        ,
                    Value = x.ToString("dd-MM-yyyy"),
                    //Selected = dashboardData
                    //    .Where(y => y.GenerationDateUtc.ToLocalTime() == x).FirstOrDefault() != null,
                }).ToList()
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

            return View("~/Plugins/Teed.Plugin.MarketingDashboard/Views/MarketingDashboard/Index.cshtml", model);
        }

        public IActionResult ExportDataExcel()
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var getAllTodo = _marketingDashboardDataService.GetAll().ToList();
            if (getAllTodo.Count() > 0)
            {
                var allDashboardData = getAllTodo.GroupBy(x => x.GenerationDateUtc)
                .OrderByDescending(x => x.Key)
                .FirstOrDefault()
                .Select(x => x)
                .OrderByDescending(x => x.InitDate);

                //Weekly data 120 days
                var dashboardData10 = allDashboardData.Where(x => x.MarketingDashboardDataTypeId == 10).OrderBy(x => x.InitDate);
                // Weekly data 120 days (only 26)
                var dashboardData15 = allDashboardData.Where(x => x.MarketingDashboardDataTypeId == 10).Take(26).OrderBy(x => x.InitDate);
                //Monthly data 120 days
                var dashboardData20 = allDashboardData.Where(x => x.MarketingDashboardDataTypeId == 20);
                //Monthly data 30 days
                var dashboardData30 = allDashboardData.Where(x => x.MarketingDashboardDataTypeId == 30);
                //Weekly data 30 days
                var dashboardData40 = allDashboardData.Where(x => x.MarketingDashboardDataTypeId == 40);

                using (var stream = new MemoryStream())
                {
                    using (var xlPackage = new ExcelPackage(stream))
                    {
                        int column = 0;
                        var borderBottomColor = System.Drawing.ColorTranslator.FromHtml("#c5c5c5");
                        var worksheet = xlPackage.Workbook.Worksheets.Add("Indicadores históricos");

                        #region INDICADORES 1

                        int row = 1;
                        int col = 1;

                        worksheet.Cells[row, col].Value = "Indicadores semanales de MKT";
                        worksheet.Cells[row + 1, col].Value = "ID";
                        worksheet.Cells[row + 1, col + 1].Value = "Indicador semanal";

                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[1, col].Value = item.InitDate;
                            worksheet.Cells[1, col].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[2, col].Value = item.EndDate;
                            worksheet.Cells[2, col].Style.Numberformat.Format = "dd-mm-yyyy";
                            col++;
                        }

                        row = 3;

                        worksheet.Cells[row, 1].Value = "MK0001";
                        worksheet.Cells[row, 2].Value = "Costo por nuevo registro";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.NewRegisterCost;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0002";
                        worksheet.Cells[row, 2].Value = "CAC (Costo por adquisición de cliente)";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.AdquisitionCost;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0003";
                        worksheet.Cells[row, 2].Value = "Tasa de registros a compras";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.BuyinRegister;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "0.00%";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0004";
                        worksheet.Cells[row, 2].Value = "Promedio de pedidos diarios";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.DailyPedidosAverage;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0005";
                        worksheet.Cells[row, 2].Value = "Promedio de venta diaria";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.DailySalesAverage;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0006A";
                        worksheet.Cells[row, 2].Value = "Ticket promedio";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.AverageTicket;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0006B";
                        worksheet.Cells[row, 2].Value = "Ticket promedio clientes nuevos";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.AverageTicketNewCustomers;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0006C";
                        worksheet.Cells[row, 2].Value = "Ticket promedio clientes recurrentes";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.AverageTicketOldCustomers;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0007";
                        worksheet.Cells[row, 2].Value = "Recurrencia mensual";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.Recurrence;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0008A";
                        worksheet.Cells[row, 2].Value = "Recurrencia 120/1";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.Recurrence120days;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0008B";
                        worksheet.Cells[row, 2].Value = "Recurrencia 120/2-3";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.Recurrence120days2or3;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0008C";
                        worksheet.Cells[row, 2].Value = "Recurrencia 120/4-5";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.Recurrence120days4or5;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0008D";
                        worksheet.Cells[row, 2].Value = "Recurrencia 120/6-7";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.Recurrence120days6or7;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0008E";
                        worksheet.Cells[row, 2].Value = "Recurrencia 120/8-9";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.Recurrence120days8or9;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0008F";
                        worksheet.Cells[row, 2].Value = "Recurrencia 120/10+";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.Recurrence120daysMoreThan10;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0009";
                        worksheet.Cells[row, 2].Value = "Tasa de retención estabilizada";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.RetentionRate120Days;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "0.00%";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0010";
                        worksheet.Cells[row, 2].Value = "Customer Annual Value";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerAnnualValue;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0011";
                        worksheet.Cells[row, 2].Value = "Costumer Annual Contribution";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerAnnualContribution;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0012";
                        worksheet.Cells[row, 2].Value = "Costumer Annual Contribution (mensual) * Tasa de Retención";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerAnnualContributionRetention;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0013A";
                        worksheet.Cells[row, 2].Value = "Costumer Annual Contribution (120/1) * Tasa de Retención";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerAnnualContribution120Retention;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0013B";
                        worksheet.Cells[row, 2].Value = "Costumer Annual Contribution (120/2-3) * Tasa de Retención";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerAnnualContribution120Retention2or3;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0013C";
                        worksheet.Cells[row, 2].Value = "Costumer Annual Contribution (120/4-5) * Tasa de Retención";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerAnnualContribution120Retention4or5;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0013D";
                        worksheet.Cells[row, 2].Value = "Costumer Annual Contribution (120/6-7) * Tasa de Retención";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerAnnualContribution120Retention6or7;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0013E";
                        worksheet.Cells[row, 2].Value = "Costumer Annual Contribution (120/8-9) * Tasa de Retención";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerAnnualContribution120Retention8or9;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0013F";
                        worksheet.Cells[row, 2].Value = "Costumer Annual Contribution (120/10+) * Tasa de Retención";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerAnnualContribution120RetentionMoreThan10;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0014";
                        worksheet.Cells[row, 2].Value = "[Costumer Annual Contribution (mensual) * Tasa de Retención] / CAC";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomFormula2;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            col++;
                        }

                        row = 32;
                        col = 1;

                        worksheet.Cells[row, col].Value = "Variables para indicadores semanales de MKT";
                        worksheet.Cells[row + 1, col].Value = "ID";
                        worksheet.Cells[row + 1, col + 1].Value = "Variable";

                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[32, col].Value = item.InitDate;
                            worksheet.Cells[32, col].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[33, col].Value = item.EndDate;
                            worksheet.Cells[33, col].Style.Numberformat.Format = "dd-mm-yyyy";
                            col++;
                        }

                        row = 34;

                        worksheet.Cells[row, 1].Value = "MKV0001";
                        worksheet.Cells[row, 2].Value = "Gasto publicitario del periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.MarketingExpenses;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0002";
                        worksheet.Cells[row, 2].Value = "Nuevos registros en el periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.NewRegisteredUsersCount;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0003";
                        worksheet.Cells[row, 2].Value = "Nuevas cuentas activas (al menos han hecho un pedido en la vida) del periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.NewActiveCount;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0004";
                        worksheet.Cells[row, 2].Value = "Total de pedidos del periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.PedidosCount;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0005";
                        worksheet.Cells[row, 2].Value = "Total de ventas del periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.SalesTotal;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0006";
                        worksheet.Cells[row, 2].Value = "Número de días trabajados en el periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.WorkingDays;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0007";
                        worksheet.Cells[row, 2].Value = "Total de pedidos de los últimos 30 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.TotalPedidos30Days;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0008";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hayan hecho al menos un pedido en los últimos 60 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerCount60Days;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0009";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hayan hecho al menos un pedido en los últimos 30 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerCount30Days;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0010";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hayan hecho su primer pedido en los últimos 30 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.FirstOrderCount30Days;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0011";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hayan hecho al menos un pedido en los últimos 120 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerCount120Days;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0012";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hayan hecho al menos un pedido en los últimos 90 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerCount90Days;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0013";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hayan hecho su primer pedido en los últimos 90 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.FirstOrderCount90Days;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0014";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron al menos 1 pedido en los últimos 120 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomersCountAtLeastOneOrder120DaysAndMore;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0015";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron únicamente 1 pedido en los últimos 120 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomersCountOnlyOnePedido120DaysAndMore;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0016";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron 2 ó 3 pedidos en los últimos 120 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomersCount2or3Pedidos120DaysAndMore;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0017";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron 4 ó 5 pedidos en los últimos 120 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomersCount4or5Pedidos120DaysAndMore;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0018";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron 6 ó 7 pedidos en los últimos 120 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomersCount6or7Pedidos120DaysAndMore;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0019";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron 8 ó 9 pedidos en los últimos 120 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomersCount8or9Pedidos120DaysAndMore;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0020";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron más de 10 pedidos en los últimos 120 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomersCountMoreThan10Pedidos120DaysAndMore;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0021";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hicieron al menos un pedido entre 150 y 121 días atrás a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomersWithOneOrderBetween150and121days;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0022";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hicieron al menos un pedido entre 150 y 121 días atrás y que hicieron al menos un pedido en los últimos 120 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.CustomersWithOneOrderBetween150and121daysAnd120daysPedido;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0023";
                        worksheet.Cells[row, 2].Value = "Ventas generadas por pedidos dentro del periodo que fueron el primer pedido del cliente";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.SalesFirstOrders;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0024";
                        worksheet.Cells[row, 2].Value = "Número de pedidos dentro del periodo que fueron el primer pedido del cliente";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.FirstPedidosCount;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0025";
                        worksheet.Cells[row, 2].Value = "Ventas generadas por pedidos dentro del periodo que fueron hechas por clientes con más de un pedido";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.SalesNotFirstOrders;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0026";
                        worksheet.Cells[row, 2].Value = "Número de pedidos dentro del periodo que fueron hechos por clientes con más de un pedido";
                        col = 3;
                        foreach (var item in dashboardData10)
                        {
                            worksheet.Cells[row, col].Value = item.NotFirstOrdersCount;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        row++;

                        var grossMargin = _marketingGrossMarginService.GetAll()
                            .OrderByDescending(x => x.ApplyDate)
                            .Select(x => x.Value)
                            .FirstOrDefault();
                        worksheet.Cells[row, 1].Value = "Margen bruto PP";
                        worksheet.Cells[row, 2].Value = grossMargin + "%";

                        worksheet.DefaultRowHeight = 9;
                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).Style.Font.Size = 5;
                            worksheet.Cells[1, i].AutoFitColumns(3);
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }


                        worksheet.Column(2).Width = 79;
                        worksheet.View.ZoomScale = 180;

                        var start = worksheet.Dimension.Start;
                        var end = worksheet.Dimension.End;
                        var endLetter = ExcelCellAddress.GetColumnLetter(end.Column);
                        for (int iRow = start.Row; iRow <= end.Row; iRow++)
                        {
                            worksheet.Cells[$"A{iRow}:{endLetter}{iRow}"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            worksheet.Cells[$"A{iRow}:{endLetter}{iRow}"].Style.Border.Bottom.Color.SetColor(borderBottomColor);
                        }

                        #endregion

                        worksheet = xlPackage.Workbook.Worksheets.Add("Indicadores 26 semanas");

                        #region INDICADORES 1.5

                        row = 1;
                        col = 1;

                        worksheet.Cells[row, col].Value = "Indicadores semanales de MKT";
                        worksheet.Cells[row + 1, col].Value = "ID";
                        worksheet.Cells[row + 1, col + 1].Value = "Indicador semanal";

                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[1, col].Value = item.InitDate;
                            worksheet.Cells[1, col].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[2, col].Value = item.EndDate;
                            worksheet.Cells[2, col].Style.Numberformat.Format = "dd-mm-yyyy";
                            col++;
                        }

                        row = 3;

                        worksheet.Cells[row, 1].Value = "MK0001";
                        worksheet.Cells[row, 2].Value = "Costo por nuevo registro";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.NewRegisterCost;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0002";
                        worksheet.Cells[row, 2].Value = "CAC (Costo por adquisición de cliente)";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.AdquisitionCost;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0003";
                        worksheet.Cells[row, 2].Value = "Tasa de registros a compras";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.BuyinRegister;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "0.00%";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0004";
                        worksheet.Cells[row, 2].Value = "Promedio de pedidos diarios";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.DailyPedidosAverage;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0005";
                        worksheet.Cells[row, 2].Value = "Promedio de venta diaria";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.DailySalesAverage;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0006A";
                        worksheet.Cells[row, 2].Value = "Ticket promedio";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.AverageTicket;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0006B";
                        worksheet.Cells[row, 2].Value = "Ticket promedio clientes nuevos";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.AverageTicketNewCustomers;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0006C";
                        worksheet.Cells[row, 2].Value = "Ticket promedio clientes recurrentes";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.AverageTicketOldCustomers;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0007";
                        worksheet.Cells[row, 2].Value = "Recurrencia mensual";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.Recurrence;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0008A";
                        worksheet.Cells[row, 2].Value = "Recurrencia 120/1";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.Recurrence120days;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0008B";
                        worksheet.Cells[row, 2].Value = "Recurrencia 120/2-3";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.Recurrence120days2or3;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0008C";
                        worksheet.Cells[row, 2].Value = "Recurrencia 120/4-5";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.Recurrence120days4or5;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0008D";
                        worksheet.Cells[row, 2].Value = "Recurrencia 120/6-7";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.Recurrence120days6or7;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0008E";
                        worksheet.Cells[row, 2].Value = "Recurrencia 120/8-9";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.Recurrence120days8or9;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0008F";
                        worksheet.Cells[row, 2].Value = "Recurrencia 120/10+";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.Recurrence120daysMoreThan10;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0009";
                        worksheet.Cells[row, 2].Value = "Tasa de retención estabilizada";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.RetentionRate120Days;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "0.00%";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0010";
                        worksheet.Cells[row, 2].Value = "Customer Annual Value";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerAnnualValue;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0011";
                        worksheet.Cells[row, 2].Value = "Costumer Annual Contribution";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerAnnualContribution;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0012";
                        worksheet.Cells[row, 2].Value = "Costumer Annual Contribution (mensual) * Tasa de Retención";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerAnnualContributionRetention;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0013A";
                        worksheet.Cells[row, 2].Value = "Costumer Annual Contribution (120/1) * Tasa de Retención";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerAnnualContribution120Retention;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0013B";
                        worksheet.Cells[row, 2].Value = "Costumer Annual Contribution (120/2-3) * Tasa de Retención";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerAnnualContribution120Retention2or3;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0013C";
                        worksheet.Cells[row, 2].Value = "Costumer Annual Contribution (120/4-5) * Tasa de Retención";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerAnnualContribution120Retention4or5;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0013D";
                        worksheet.Cells[row, 2].Value = "Costumer Annual Contribution (120/6-7) * Tasa de Retención";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerAnnualContribution120Retention6or7;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0013E";
                        worksheet.Cells[row, 2].Value = "Costumer Annual Contribution (120/8-9) * Tasa de Retención";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerAnnualContribution120Retention8or9;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0013F";
                        worksheet.Cells[row, 2].Value = "Costumer Annual Contribution (120/10+) * Tasa de Retención";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerAnnualContribution120RetentionMoreThan10;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MK0014";
                        worksheet.Cells[row, 2].Value = "[Costumer Annual Contribution (mensual) * Tasa de Retención] / CAC";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomFormula2;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            col++;
                        }

                        row = 32;
                        col = 1;

                        worksheet.Cells[row, col].Value = "Variables para indicadores semanales de MKT";
                        worksheet.Cells[row + 1, col].Value = "ID";
                        worksheet.Cells[row + 1, col + 1].Value = "Variable";

                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[32, col].Value = item.InitDate;
                            worksheet.Cells[32, col].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[33, col].Value = item.EndDate;
                            worksheet.Cells[33, col].Style.Numberformat.Format = "dd-mm-yyyy";
                            col++;
                        }

                        row = 34;

                        worksheet.Cells[row, 1].Value = "MKV0001";
                        worksheet.Cells[row, 2].Value = "Gasto publicitario del periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.MarketingExpenses;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0002";
                        worksheet.Cells[row, 2].Value = "Nuevos registros en el periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.NewRegisteredUsersCount;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0003";
                        worksheet.Cells[row, 2].Value = "Nuevas cuentas activas (al menos han hecho un pedido en la vida) del periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.NewActiveCount;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0004";
                        worksheet.Cells[row, 2].Value = "Total de pedidos del periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.PedidosCount;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0005";
                        worksheet.Cells[row, 2].Value = "Total de ventas del periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.SalesTotal;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0006";
                        worksheet.Cells[row, 2].Value = "Número de días trabajados en el periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.WorkingDays;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0007";
                        worksheet.Cells[row, 2].Value = "Total de pedidos de los últimos 30 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.TotalPedidos30Days;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0008";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hayan hecho al menos un pedido en los últimos 60 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerCount60Days;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0009";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hayan hecho al menos un pedido en los últimos 30 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerCount30Days;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0010";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hayan hecho su primer pedido en los últimos 30 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.FirstOrderCount30Days;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0011";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hayan hecho al menos un pedido en los últimos 120 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerCount120Days;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0012";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hayan hecho al menos un pedido en los últimos 90 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomerCount90Days;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0013";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hayan hecho su primer pedido en los últimos 90 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.FirstOrderCount90Days;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0014";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron al menos 1 pedido en los últimos 120 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomersCountAtLeastOneOrder120DaysAndMore;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0015";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron únicamente 1 pedido en los últimos 120 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomersCountOnlyOnePedido120DaysAndMore;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0016";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron 2 ó 3 pedidos en los últimos 120 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomersCount2or3Pedidos120DaysAndMore;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0017";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron 4 ó 5 pedidos en los últimos 120 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomersCount4or5Pedidos120DaysAndMore;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0018";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron 6 ó 7 pedidos en los últimos 120 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomersCount6or7Pedidos120DaysAndMore;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0019";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron 8 ó 9 pedidos en los últimos 120 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomersCount8or9Pedidos120DaysAndMore;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0020";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron más de 10 pedidos en los últimos 120 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomersCountMoreThan10Pedidos120DaysAndMore;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0021";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hicieron al menos un pedido entre 150 y 121 días atrás a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomersWithOneOrderBetween150and121days;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0022";
                        worksheet.Cells[row, 2].Value = "Número de clientes que hicieron al menos un pedido entre 150 y 121 días atrás y que hicieron al menos un pedido en los últimos 120 días a partir de la fecha final del periodo";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.CustomersWithOneOrderBetween150and121daysAnd120daysPedido;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0023";
                        worksheet.Cells[row, 2].Value = "Ventas generadas por pedidos dentro del periodo que fueron el primer pedido del cliente";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.SalesFirstOrders;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0024";
                        worksheet.Cells[row, 2].Value = "Número de pedidos dentro del periodo que fueron el primer pedido del cliente";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.FirstPedidosCount;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0025";
                        worksheet.Cells[row, 2].Value = "Ventas generadas por pedidos dentro del periodo que fueron hechas por clientes con más de un pedido";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.SalesNotFirstOrders;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "MKV0026";
                        worksheet.Cells[row, 2].Value = "Número de pedidos dentro del periodo que fueron hechos por clientes con más de un pedido";
                        col = 3;
                        foreach (var item in dashboardData15)
                        {
                            worksheet.Cells[row, col].Value = item.NotFirstOrdersCount;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }

                        row++;
                        row++;

                        grossMargin = _marketingGrossMarginService.GetAll()
                            .OrderByDescending(x => x.ApplyDate)
                            .Select(x => x.Value)
                            .FirstOrDefault();
                        worksheet.Cells[row, 1].Value = "Margen bruto PP";
                        worksheet.Cells[row, 2].Value = grossMargin + "%";

                        worksheet.DefaultRowHeight = 9;
                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).Style.Font.Size = 5;
                            worksheet.Cells[1, i].AutoFitColumns(3);
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }


                        worksheet.Column(2).Width = 79;
                        worksheet.View.ZoomScale = 180;

                        start = worksheet.Dimension.Start;
                        end = worksheet.Dimension.End;
                        endLetter = ExcelCellAddress.GetColumnLetter(end.Column);
                        for (int iRow = start.Row; iRow <= end.Row; iRow++)
                        {
                            worksheet.Cells[$"A{iRow}:{endLetter}{iRow}"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            worksheet.Cells[$"A{iRow}:{endLetter}{iRow}"].Style.Border.Bottom.Color.SetColor(borderBottomColor);
                        }

                        #endregion

                        worksheet = xlPackage.Workbook.Worksheets.Add("Cosechas semanales 30 dias");

                        #region INDICADORES 2

                        row = 1;
                        worksheet.Cells[row, 3].Value = "Clientes";
                        worksheet.Cells[row, 4].Value = "Mes 1";
                        worksheet.Cells[row, 5].Value = "Mes 2";
                        worksheet.Cells[row, 6].Value = "Mes 3";
                        worksheet.Cells[row, 7].Value = "Mes 4";
                        worksheet.Cells[row, 8].Value = "Mes 5";
                        worksheet.Cells[row, 9].Value = "Mes 6";
                        worksheet.Cells[row, 10].Value = "Mes 7";
                        worksheet.Cells[row, 11].Value = "Mes 8";
                        worksheet.Cells[row, 12].Value = "Mes 9";
                        worksheet.Cells[row, 13].Value = "Mes 10";
                        worksheet.Cells[row, 14].Value = "Mes 11";
                        worksheet.Cells[row, 15].Value = "Mes 12";
                        worksheet.Cells[row, 16].Value = "Mes 13";
                        worksheet.Cells[row, 17].Value = "Mes 14";
                        worksheet.Cells[row, 18].Value = "Mes 15";
                        worksheet.Cells[row, 19].Value = "Mes 16";
                        worksheet.Cells[row, 20].Value = "Mes 17";
                        worksheet.Cells[row, 21].Value = "Mes 18";
                        worksheet.Cells[row, 22].Value = "Mes 19";
                        worksheet.Cells[row, 23].Value = "Mes 20";
                        worksheet.Cells[row, 24].Value = "Mes 21";
                        worksheet.Cells[row, 25].Value = "Mes 22";
                        worksheet.Cells[row, 26].Value = "Mes 23";
                        worksheet.Cells[row, 27].Value = "Mes 24";

                        worksheet.Cells[row, 31].Value = "Clientes";
                        worksheet.Cells[row, 32].Value = "Mes 1";
                        worksheet.Cells[row, 33].Value = "Mes 2";
                        worksheet.Cells[row, 34].Value = "Mes 3";
                        worksheet.Cells[row, 35].Value = "Mes 4";
                        worksheet.Cells[row, 36].Value = "Mes 5";
                        worksheet.Cells[row, 37].Value = "Mes 6";
                        worksheet.Cells[row, 38].Value = "Mes 7";
                        worksheet.Cells[row, 39].Value = "Mes 8";
                        worksheet.Cells[row, 40].Value = "Mes 9";
                        worksheet.Cells[row, 41].Value = "Mes 10";
                        worksheet.Cells[row, 42].Value = "Mes 11";
                        worksheet.Cells[row, 43].Value = "Mes 12";
                        worksheet.Cells[row, 44].Value = "Mes 13";
                        worksheet.Cells[row, 45].Value = "Mes 14";
                        worksheet.Cells[row, 46].Value = "Mes 15";
                        worksheet.Cells[row, 47].Value = "Mes 16";
                        worksheet.Cells[row, 48].Value = "Mes 17";
                        worksheet.Cells[row, 49].Value = "Mes 18";
                        worksheet.Cells[row, 50].Value = "Mes 19";
                        worksheet.Cells[row, 51].Value = "Mes 20";
                        worksheet.Cells[row, 52].Value = "Mes 21";
                        worksheet.Cells[row, 53].Value = "Mes 22";
                        worksheet.Cells[row, 54].Value = "Mes 23";
                        worksheet.Cells[row, 55].Value = "Mes 24";

                        worksheet.Cells[row, 59].Value = "Clientes";
                        worksheet.Cells[row, 60].Value = "Mes 1";
                        worksheet.Cells[row, 61].Value = "Mes 2";
                        worksheet.Cells[row, 62].Value = "Mes 3";
                        worksheet.Cells[row, 63].Value = "Mes 4";
                        worksheet.Cells[row, 64].Value = "Mes 5";
                        worksheet.Cells[row, 65].Value = "Mes 6";
                        worksheet.Cells[row, 66].Value = "Mes 7";
                        worksheet.Cells[row, 67].Value = "Mes 8";
                        worksheet.Cells[row, 68].Value = "Mes 9";
                        worksheet.Cells[row, 69].Value = "Mes 10";
                        worksheet.Cells[row, 70].Value = "Mes 11";
                        worksheet.Cells[row, 71].Value = "Mes 12";
                        worksheet.Cells[row, 72].Value = "Mes 13";
                        worksheet.Cells[row, 73].Value = "Mes 14";
                        worksheet.Cells[row, 74].Value = "Mes 15";
                        worksheet.Cells[row, 75].Value = "Mes 16";
                        worksheet.Cells[row, 76].Value = "Mes 17";
                        worksheet.Cells[row, 77].Value = "Mes 18";
                        worksheet.Cells[row, 78].Value = "Mes 19";
                        worksheet.Cells[row, 79].Value = "Mes 20";
                        worksheet.Cells[row, 80].Value = "Mes 21";
                        worksheet.Cells[row, 81].Value = "Mes 22";
                        worksheet.Cells[row, 82].Value = "Mes 23";
                        worksheet.Cells[row, 83].Value = "Mes 24";

                        worksheet.Cells[row, 87].Value = "Clientes";
                        worksheet.Cells[row, 88].Value = "Mes 1";
                        worksheet.Cells[row, 89].Value = "Mes 2";
                        worksheet.Cells[row, 90].Value = "Mes 3";
                        worksheet.Cells[row, 91].Value = "Mes 4";
                        worksheet.Cells[row, 92].Value = "Mes 5";
                        worksheet.Cells[row, 93].Value = "Mes 6";
                        worksheet.Cells[row, 94].Value = "Mes 7";
                        worksheet.Cells[row, 95].Value = "Mes 8";
                        worksheet.Cells[row, 96].Value = "Mes 9";
                        worksheet.Cells[row, 97].Value = "Mes 10";
                        worksheet.Cells[row, 98].Value = "Mes 11";
                        worksheet.Cells[row, 99].Value = "Mes 12";
                        worksheet.Cells[row, 100].Value = "Mes 13";
                        worksheet.Cells[row, 101].Value = "Mes 14";
                        worksheet.Cells[row, 102].Value = "Mes 15";
                        worksheet.Cells[row, 103].Value = "Mes 16";
                        worksheet.Cells[row, 104].Value = "Mes 17";
                        worksheet.Cells[row, 105].Value = "Mes 18";
                        worksheet.Cells[row, 106].Value = "Mes 19";
                        worksheet.Cells[row, 107].Value = "Mes 20";
                        worksheet.Cells[row, 108].Value = "Mes 21";
                        worksheet.Cells[row, 109].Value = "Mes 22";
                        worksheet.Cells[row, 110].Value = "Mes 23";
                        worksheet.Cells[row, 111].Value = "Mes 24";

                        foreach (var item in dashboardData40)
                        {
                            column = 1;
                            row++;
                            worksheet.Cells[row, column].Value = item.InitDate;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                            column++;
                            worksheet.Cells[row, column].Value = item.EndDate;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                            column++;
                            worksheet.Cells[row, column].Value = item.FirstPedidosCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client30DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client60DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client90DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client120DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client150DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client180DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client210DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client240DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client270DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client300DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client330DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client360DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client390DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client420DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client450DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client480DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client510DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client540DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client570DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client600DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client630DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client660DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client690DaysAfterCount;

                            column++;
                            worksheet.Cells[row, column].Value = item.Client720DaysAfterCount;

                            worksheet.Cells[row, 29].Value = item.InitDate;
                            worksheet.Cells[row, 29].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[row, 30].Value = item.EndDate;
                            worksheet.Cells[row, 30].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[row, 31].Value = item.FirstPedidosCount;
                            worksheet.Cells[row, 32].Value = item.Client30DaysAfterPercentage;
                            worksheet.Cells[row, 32].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 33].Value = item.Client60DaysAfterPercentage;
                            worksheet.Cells[row, 33].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 34].Value = item.Client90DaysAfterPercentage;
                            worksheet.Cells[row, 34].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 35].Value = item.Client120DaysAfterPercentage;
                            worksheet.Cells[row, 35].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 36].Value = item.Client150DaysAfterPercentage;
                            worksheet.Cells[row, 36].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 37].Value = item.Client180DaysAfterPercentage;
                            worksheet.Cells[row, 37].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 38].Value = item.Client210DaysAfterPercentage;
                            worksheet.Cells[row, 38].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 39].Value = item.Client240DaysAfterPercentage;
                            worksheet.Cells[row, 39].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 40].Value = item.Client270DaysAfterPercentage;
                            worksheet.Cells[row, 40].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 41].Value = item.Client300DaysAfterPercentage;
                            worksheet.Cells[row, 41].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 42].Value = item.Client330DaysAfterPercentage;
                            worksheet.Cells[row, 42].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 43].Value = item.Client360DaysAfterPercentage;
                            worksheet.Cells[row, 43].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 44].Value = item.Client390DaysAfterPercentage;
                            worksheet.Cells[row, 44].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 45].Value = item.Client420DaysAfterPercentage;
                            worksheet.Cells[row, 45].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 46].Value = item.Client450DaysAfterPercentage;
                            worksheet.Cells[row, 46].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 47].Value = item.Client480DaysAfterPercentage;
                            worksheet.Cells[row, 47].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 48].Value = item.Client510DaysAfterPercentage;
                            worksheet.Cells[row, 48].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 49].Value = item.Client540DaysAfterPercentage;
                            worksheet.Cells[row, 49].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 50].Value = item.Client570DaysAfterPercentage;
                            worksheet.Cells[row, 50].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 51].Value = item.Client600DaysAfterPercentage;
                            worksheet.Cells[row, 51].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 52].Value = item.Client630DaysAfterPercentage;
                            worksheet.Cells[row, 52].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 53].Value = item.Client660DaysAfterPercentage;
                            worksheet.Cells[row, 53].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 54].Value = item.Client690DaysAfterPercentage;
                            worksheet.Cells[row, 54].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 55].Value = item.Client720DaysAfterPercentage;
                            worksheet.Cells[row, 55].Style.Numberformat.Format = "#0.00%";

                            worksheet.Cells[row, 57].Value = item.InitDate;
                            worksheet.Cells[row, 57].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[row, 58].Value = item.EndDate;
                            worksheet.Cells[row, 58].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[row, 59].Value = item.FirstPedidosCount;
                            worksheet.Cells[row, 60].Value = item.Client30DaysAfterTicket;
                            worksheet.Cells[row, 61].Value = item.Client60DaysAfterTicket;
                            worksheet.Cells[row, 62].Value = item.Client90DaysAfterTicket;
                            worksheet.Cells[row, 63].Value = item.Client120DaysAfterTicket;
                            worksheet.Cells[row, 64].Value = item.Client150DaysAfterTicket;
                            worksheet.Cells[row, 65].Value = item.Client180DaysAfterTicket;
                            worksheet.Cells[row, 66].Value = item.Client210DaysAfterTicket;
                            worksheet.Cells[row, 67].Value = item.Client240DaysAfterTicket;
                            worksheet.Cells[row, 68].Value = item.Client270DaysAfterTicket;
                            worksheet.Cells[row, 69].Value = item.Client300DaysAfterTicket;
                            worksheet.Cells[row, 70].Value = item.Client330DaysAfterTicket;
                            worksheet.Cells[row, 71].Value = item.Client360DaysAfterTicket;
                            worksheet.Cells[row, 72].Value = item.Client390DaysAfterTicket;
                            worksheet.Cells[row, 73].Value = item.Client420DaysAfterTicket;
                            worksheet.Cells[row, 74].Value = item.Client450DaysAfterTicket;
                            worksheet.Cells[row, 75].Value = item.Client480DaysAfterTicket;
                            worksheet.Cells[row, 76].Value = item.Client510DaysAfterTicket;
                            worksheet.Cells[row, 77].Value = item.Client540DaysAfterTicket;
                            worksheet.Cells[row, 78].Value = item.Client570DaysAfterTicket;
                            worksheet.Cells[row, 79].Value = item.Client600DaysAfterTicket;
                            worksheet.Cells[row, 80].Value = item.Client630DaysAfterTicket;
                            worksheet.Cells[row, 81].Value = item.Client660DaysAfterTicket;
                            worksheet.Cells[row, 82].Value = item.Client690DaysAfterTicket;
                            worksheet.Cells[row, 83].Value = item.Client720DaysAfterTicket;

                            worksheet.Cells[row, 85].Value = item.InitDate;
                            worksheet.Cells[row, 85].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[row, 86].Value = item.EndDate;
                            worksheet.Cells[row, 86].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[row, 87].Value = item.FirstPedidosCount;
                            worksheet.Cells[row, 88].Value = item.Client30DaysAfterRecurrence;
                            worksheet.Cells[row, 89].Value = item.Client60DaysAfterRecurrence;
                            worksheet.Cells[row, 90].Value = item.Client90DaysAfterRecurrence;
                            worksheet.Cells[row, 91].Value = item.Client120DaysAfterRecurrence;
                            worksheet.Cells[row, 92].Value = item.Client150DaysAfterRecurrence;
                            worksheet.Cells[row, 93].Value = item.Client180DaysAfterRecurrence;
                            worksheet.Cells[row, 94].Value = item.Client210DaysAfterRecurrence;
                            worksheet.Cells[row, 95].Value = item.Client240DaysAfterRecurrence;
                            worksheet.Cells[row, 96].Value = item.Client270DaysAfterRecurrence;
                            worksheet.Cells[row, 97].Value = item.Client300DaysAfterRecurrence;
                            worksheet.Cells[row, 98].Value = item.Client330DaysAfterRecurrence;
                            worksheet.Cells[row, 99].Value = item.Client360DaysAfterRecurrence;
                            worksheet.Cells[row, 100].Value = item.Client390DaysAfterRecurrence;
                            worksheet.Cells[row, 101].Value = item.Client420DaysAfterRecurrence;
                            worksheet.Cells[row, 102].Value = item.Client450DaysAfterRecurrence;
                            worksheet.Cells[row, 103].Value = item.Client480DaysAfterRecurrence;
                            worksheet.Cells[row, 104].Value = item.Client510DaysAfterRecurrence;
                            worksheet.Cells[row, 105].Value = item.Client540DaysAfterRecurrence;
                            worksheet.Cells[row, 106].Value = item.Client570DaysAfterRecurrence;
                            worksheet.Cells[row, 107].Value = item.Client600DaysAfterRecurrence;
                            worksheet.Cells[row, 108].Value = item.Client630DaysAfterRecurrence;
                            worksheet.Cells[row, 109].Value = item.Client660DaysAfterRecurrence;
                            worksheet.Cells[row, 110].Value = item.Client690DaysAfterRecurrence;
                            worksheet.Cells[row, 111].Value = item.Client720DaysAfterRecurrence;
                        }

                        worksheet.DefaultRowHeight = 9;
                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).Style.Font.Size = 5;
                            worksheet.Cells[1, i].AutoFitColumns(3);
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                        worksheet.Column(1).Width = 6;
                        worksheet.Column(2).Width = 6;
                        worksheet.Column(29).Width = 6;
                        worksheet.Column(30).Width = 6;
                        worksheet.Column(57).Width = 6;
                        worksheet.Column(58).Width = 6;
                        worksheet.Column(85).Width = 6;
                        worksheet.Column(86).Width = 6;
                        worksheet.View.ZoomScale = 180;

                        start = worksheet.Dimension.Start;
                        end = worksheet.Dimension.End;
                        endLetter = ExcelCellAddress.GetColumnLetter(end.Column);
                        for (int iRow = start.Row; iRow <= end.Row; iRow++)
                        {
                            worksheet.Cells[$"A{iRow}:{endLetter}{iRow}"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            worksheet.Cells[$"A{iRow}:{endLetter}{iRow}"].Style.Border.Bottom.Color.SetColor(borderBottomColor);
                        }

                        #endregion

                        worksheet = xlPackage.Workbook.Worksheets.Add("Cosechas semanales 120 dias");

                        #region INDICADORES 3

                        row = 1;
                        worksheet.Cells[row, 3].Value = "Clientes";
                        worksheet.Cells[row, 4].Value = "Mes 1";
                        worksheet.Cells[row, 5].Value = "Mes 2";
                        worksheet.Cells[row, 6].Value = "Mes 3";
                        worksheet.Cells[row, 7].Value = "Mes 4";
                        worksheet.Cells[row, 8].Value = "Mes 5";
                        worksheet.Cells[row, 9].Value = "Mes 6";
                        worksheet.Cells[row, 10].Value = "Mes 7";
                        worksheet.Cells[row, 11].Value = "Mes 8";
                        worksheet.Cells[row, 12].Value = "Mes 9";
                        worksheet.Cells[row, 13].Value = "Mes 10";
                        worksheet.Cells[row, 14].Value = "Mes 11";
                        worksheet.Cells[row, 15].Value = "Mes 12";
                        worksheet.Cells[row, 16].Value = "Mes 13";
                        worksheet.Cells[row, 17].Value = "Mes 14";
                        worksheet.Cells[row, 18].Value = "Mes 15";
                        worksheet.Cells[row, 19].Value = "Mes 16";
                        worksheet.Cells[row, 20].Value = "Mes 17";
                        worksheet.Cells[row, 21].Value = "Mes 18";
                        worksheet.Cells[row, 22].Value = "Mes 19";
                        worksheet.Cells[row, 23].Value = "Mes 20";
                        worksheet.Cells[row, 24].Value = "Mes 21";
                        worksheet.Cells[row, 25].Value = "Mes 22";
                        worksheet.Cells[row, 26].Value = "Mes 23";
                        worksheet.Cells[row, 27].Value = "Mes 24";

                        worksheet.Cells[row, 31].Value = "Clientes";
                        worksheet.Cells[row, 32].Value = "Mes 1";
                        worksheet.Cells[row, 33].Value = "Mes 2";
                        worksheet.Cells[row, 34].Value = "Mes 3";
                        worksheet.Cells[row, 35].Value = "Mes 4";
                        worksheet.Cells[row, 36].Value = "Mes 5";
                        worksheet.Cells[row, 37].Value = "Mes 6";
                        worksheet.Cells[row, 38].Value = "Mes 7";
                        worksheet.Cells[row, 39].Value = "Mes 8";
                        worksheet.Cells[row, 40].Value = "Mes 9";
                        worksheet.Cells[row, 41].Value = "Mes 10";
                        worksheet.Cells[row, 42].Value = "Mes 11";
                        worksheet.Cells[row, 43].Value = "Mes 12";
                        worksheet.Cells[row, 44].Value = "Mes 13";
                        worksheet.Cells[row, 45].Value = "Mes 14";
                        worksheet.Cells[row, 46].Value = "Mes 15";
                        worksheet.Cells[row, 47].Value = "Mes 16";
                        worksheet.Cells[row, 48].Value = "Mes 17";
                        worksheet.Cells[row, 49].Value = "Mes 18";
                        worksheet.Cells[row, 50].Value = "Mes 19";
                        worksheet.Cells[row, 51].Value = "Mes 20";
                        worksheet.Cells[row, 52].Value = "Mes 21";
                        worksheet.Cells[row, 53].Value = "Mes 22";
                        worksheet.Cells[row, 54].Value = "Mes 23";
                        worksheet.Cells[row, 55].Value = "Mes 24";

                        worksheet.Cells[row, 59].Value = "Clientes";
                        worksheet.Cells[row, 60].Value = "Mes 1";
                        worksheet.Cells[row, 61].Value = "Mes 2";
                        worksheet.Cells[row, 62].Value = "Mes 3";
                        worksheet.Cells[row, 63].Value = "Mes 4";
                        worksheet.Cells[row, 64].Value = "Mes 5";
                        worksheet.Cells[row, 65].Value = "Mes 6";
                        worksheet.Cells[row, 66].Value = "Mes 7";
                        worksheet.Cells[row, 67].Value = "Mes 8";
                        worksheet.Cells[row, 68].Value = "Mes 9";
                        worksheet.Cells[row, 69].Value = "Mes 10";
                        worksheet.Cells[row, 70].Value = "Mes 11";
                        worksheet.Cells[row, 71].Value = "Mes 12";
                        worksheet.Cells[row, 72].Value = "Mes 13";
                        worksheet.Cells[row, 73].Value = "Mes 14";
                        worksheet.Cells[row, 74].Value = "Mes 15";
                        worksheet.Cells[row, 75].Value = "Mes 16";
                        worksheet.Cells[row, 76].Value = "Mes 17";
                        worksheet.Cells[row, 77].Value = "Mes 18";
                        worksheet.Cells[row, 78].Value = "Mes 19";
                        worksheet.Cells[row, 79].Value = "Mes 20";
                        worksheet.Cells[row, 80].Value = "Mes 21";
                        worksheet.Cells[row, 81].Value = "Mes 22";
                        worksheet.Cells[row, 82].Value = "Mes 23";
                        worksheet.Cells[row, 83].Value = "Mes 24";

                        worksheet.Cells[row, 87].Value = "Clientes";
                        worksheet.Cells[row, 88].Value = "Mes 1";
                        worksheet.Cells[row, 89].Value = "Mes 2";
                        worksheet.Cells[row, 90].Value = "Mes 3";
                        worksheet.Cells[row, 91].Value = "Mes 4";
                        worksheet.Cells[row, 92].Value = "Mes 5";
                        worksheet.Cells[row, 93].Value = "Mes 6";
                        worksheet.Cells[row, 94].Value = "Mes 7";
                        worksheet.Cells[row, 95].Value = "Mes 8";
                        worksheet.Cells[row, 96].Value = "Mes 9";
                        worksheet.Cells[row, 97].Value = "Mes 10";
                        worksheet.Cells[row, 98].Value = "Mes 11";
                        worksheet.Cells[row, 99].Value = "Mes 12";
                        worksheet.Cells[row, 100].Value = "Mes 13";
                        worksheet.Cells[row, 101].Value = "Mes 14";
                        worksheet.Cells[row, 102].Value = "Mes 15";
                        worksheet.Cells[row, 103].Value = "Mes 16";
                        worksheet.Cells[row, 104].Value = "Mes 17";
                        worksheet.Cells[row, 105].Value = "Mes 18";
                        worksheet.Cells[row, 106].Value = "Mes 19";
                        worksheet.Cells[row, 107].Value = "Mes 20";
                        worksheet.Cells[row, 108].Value = "Mes 21";
                        worksheet.Cells[row, 109].Value = "Mes 22";
                        worksheet.Cells[row, 110].Value = "Mes 23";
                        worksheet.Cells[row, 111].Value = "Mes 24";

                        foreach (var item in dashboardData10)
                        {
                            row++;
                            worksheet.Cells[row, 1].Value = item.InitDate;
                            worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[row, 2].Value = item.EndDate;
                            worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[row, 3].Value = item.FirstPedidosCount;
                            worksheet.Cells[row, 4].Value = item.Client30DaysAfterCount;
                            worksheet.Cells[row, 5].Value = item.Client60DaysAfterCount;
                            worksheet.Cells[row, 6].Value = item.Client90DaysAfterCount;
                            worksheet.Cells[row, 7].Value = item.Client120DaysAfterCount;
                            worksheet.Cells[row, 8].Value = item.Client150DaysAfterCount;
                            worksheet.Cells[row, 9].Value = item.Client180DaysAfterCount;
                            worksheet.Cells[row, 10].Value = item.Client210DaysAfterCount;
                            worksheet.Cells[row, 11].Value = item.Client240DaysAfterCount;
                            worksheet.Cells[row, 12].Value = item.Client270DaysAfterCount;
                            worksheet.Cells[row, 13].Value = item.Client300DaysAfterCount;
                            worksheet.Cells[row, 14].Value = item.Client330DaysAfterCount;
                            worksheet.Cells[row, 15].Value = item.Client360DaysAfterCount;
                            worksheet.Cells[row, 16].Value = item.Client390DaysAfterCount;
                            worksheet.Cells[row, 17].Value = item.Client420DaysAfterCount;
                            worksheet.Cells[row, 18].Value = item.Client450DaysAfterCount;
                            worksheet.Cells[row, 19].Value = item.Client480DaysAfterCount;
                            worksheet.Cells[row, 20].Value = item.Client510DaysAfterCount;
                            worksheet.Cells[row, 21].Value = item.Client540DaysAfterCount;
                            worksheet.Cells[row, 22].Value = item.Client570DaysAfterCount;
                            worksheet.Cells[row, 23].Value = item.Client600DaysAfterCount;
                            worksheet.Cells[row, 24].Value = item.Client630DaysAfterCount;
                            worksheet.Cells[row, 25].Value = item.Client660DaysAfterCount;
                            worksheet.Cells[row, 26].Value = item.Client690DaysAfterCount;
                            worksheet.Cells[row, 27].Value = item.Client720DaysAfterCount;

                            worksheet.Cells[row, 29].Value = item.InitDate;
                            worksheet.Cells[row, 29].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[row, 30].Value = item.EndDate;
                            worksheet.Cells[row, 30].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[row, 31].Value = item.FirstPedidosCount;
                            worksheet.Cells[row, 32].Value = item.Client30DaysAfterPercentage;
                            worksheet.Cells[row, 32].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 33].Value = item.Client60DaysAfterPercentage;
                            worksheet.Cells[row, 33].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 34].Value = item.Client90DaysAfterPercentage;
                            worksheet.Cells[row, 34].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 35].Value = item.Client120DaysAfterPercentage;
                            worksheet.Cells[row, 35].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 36].Value = item.Client150DaysAfterPercentage;
                            worksheet.Cells[row, 36].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 37].Value = item.Client180DaysAfterPercentage;
                            worksheet.Cells[row, 37].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 38].Value = item.Client210DaysAfterPercentage;
                            worksheet.Cells[row, 38].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 39].Value = item.Client240DaysAfterPercentage;
                            worksheet.Cells[row, 39].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 40].Value = item.Client270DaysAfterPercentage;
                            worksheet.Cells[row, 40].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 41].Value = item.Client300DaysAfterPercentage;
                            worksheet.Cells[row, 41].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 42].Value = item.Client330DaysAfterPercentage;
                            worksheet.Cells[row, 42].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 43].Value = item.Client360DaysAfterPercentage;
                            worksheet.Cells[row, 43].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 44].Value = item.Client390DaysAfterPercentage;
                            worksheet.Cells[row, 44].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 45].Value = item.Client420DaysAfterPercentage;
                            worksheet.Cells[row, 45].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 46].Value = item.Client450DaysAfterPercentage;
                            worksheet.Cells[row, 46].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 47].Value = item.Client480DaysAfterPercentage;
                            worksheet.Cells[row, 47].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 48].Value = item.Client510DaysAfterPercentage;
                            worksheet.Cells[row, 48].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 49].Value = item.Client540DaysAfterPercentage;
                            worksheet.Cells[row, 49].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 50].Value = item.Client570DaysAfterPercentage;
                            worksheet.Cells[row, 50].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 51].Value = item.Client600DaysAfterPercentage;
                            worksheet.Cells[row, 51].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 52].Value = item.Client630DaysAfterPercentage;
                            worksheet.Cells[row, 52].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 53].Value = item.Client660DaysAfterPercentage;
                            worksheet.Cells[row, 53].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 54].Value = item.Client690DaysAfterPercentage;
                            worksheet.Cells[row, 54].Style.Numberformat.Format = "#0.00%";
                            worksheet.Cells[row, 55].Value = item.Client720DaysAfterPercentage;
                            worksheet.Cells[row, 55].Style.Numberformat.Format = "#0.00%";

                            worksheet.Cells[row, 57].Value = item.InitDate;
                            worksheet.Cells[row, 57].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[row, 58].Value = item.EndDate;
                            worksheet.Cells[row, 58].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[row, 59].Value = item.FirstPedidosCount;
                            worksheet.Cells[row, 60].Value = item.Client30DaysAfterTicket;
                            worksheet.Cells[row, 61].Value = item.Client60DaysAfterTicket;
                            worksheet.Cells[row, 62].Value = item.Client90DaysAfterTicket;
                            worksheet.Cells[row, 63].Value = item.Client120DaysAfterTicket;
                            worksheet.Cells[row, 64].Value = item.Client150DaysAfterTicket;
                            worksheet.Cells[row, 65].Value = item.Client180DaysAfterTicket;
                            worksheet.Cells[row, 66].Value = item.Client210DaysAfterTicket;
                            worksheet.Cells[row, 67].Value = item.Client240DaysAfterTicket;
                            worksheet.Cells[row, 68].Value = item.Client270DaysAfterTicket;
                            worksheet.Cells[row, 69].Value = item.Client300DaysAfterTicket;
                            worksheet.Cells[row, 70].Value = item.Client330DaysAfterTicket;
                            worksheet.Cells[row, 71].Value = item.Client360DaysAfterTicket;
                            worksheet.Cells[row, 72].Value = item.Client390DaysAfterTicket;
                            worksheet.Cells[row, 73].Value = item.Client420DaysAfterTicket;
                            worksheet.Cells[row, 74].Value = item.Client450DaysAfterTicket;
                            worksheet.Cells[row, 75].Value = item.Client480DaysAfterTicket;
                            worksheet.Cells[row, 76].Value = item.Client510DaysAfterTicket;
                            worksheet.Cells[row, 77].Value = item.Client540DaysAfterTicket;
                            worksheet.Cells[row, 78].Value = item.Client570DaysAfterTicket;
                            worksheet.Cells[row, 79].Value = item.Client600DaysAfterTicket;
                            worksheet.Cells[row, 80].Value = item.Client630DaysAfterTicket;
                            worksheet.Cells[row, 81].Value = item.Client660DaysAfterTicket;
                            worksheet.Cells[row, 82].Value = item.Client690DaysAfterTicket;
                            worksheet.Cells[row, 83].Value = item.Client720DaysAfterTicket;

                            worksheet.Cells[row, 85].Value = item.InitDate;
                            worksheet.Cells[row, 85].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[row, 86].Value = item.EndDate;
                            worksheet.Cells[row, 86].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[row, 87].Value = item.FirstPedidosCount;
                            worksheet.Cells[row, 88].Value = item.Client30DaysAfterRecurrence;
                            worksheet.Cells[row, 89].Value = item.Client60DaysAfterRecurrence;
                            worksheet.Cells[row, 90].Value = item.Client90DaysAfterRecurrence;
                            worksheet.Cells[row, 91].Value = item.Client120DaysAfterRecurrence;
                            worksheet.Cells[row, 92].Value = item.Client150DaysAfterRecurrence;
                            worksheet.Cells[row, 93].Value = item.Client180DaysAfterRecurrence;
                            worksheet.Cells[row, 94].Value = item.Client210DaysAfterRecurrence;
                            worksheet.Cells[row, 95].Value = item.Client240DaysAfterRecurrence;
                            worksheet.Cells[row, 96].Value = item.Client270DaysAfterRecurrence;
                            worksheet.Cells[row, 97].Value = item.Client300DaysAfterRecurrence;
                            worksheet.Cells[row, 98].Value = item.Client330DaysAfterRecurrence;
                            worksheet.Cells[row, 99].Value = item.Client360DaysAfterRecurrence;
                            worksheet.Cells[row, 100].Value = item.Client390DaysAfterRecurrence;
                            worksheet.Cells[row, 101].Value = item.Client420DaysAfterRecurrence;
                            worksheet.Cells[row, 102].Value = item.Client450DaysAfterRecurrence;
                            worksheet.Cells[row, 103].Value = item.Client480DaysAfterRecurrence;
                            worksheet.Cells[row, 104].Value = item.Client510DaysAfterRecurrence;
                            worksheet.Cells[row, 105].Value = item.Client540DaysAfterRecurrence;
                            worksheet.Cells[row, 106].Value = item.Client570DaysAfterRecurrence;
                            worksheet.Cells[row, 107].Value = item.Client600DaysAfterRecurrence;
                            worksheet.Cells[row, 108].Value = item.Client630DaysAfterRecurrence;
                            worksheet.Cells[row, 109].Value = item.Client660DaysAfterRecurrence;
                            worksheet.Cells[row, 110].Value = item.Client690DaysAfterRecurrence;
                            worksheet.Cells[row, 111].Value = item.Client720DaysAfterRecurrence;
                        }

                        worksheet.DefaultRowHeight = 9;
                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).Style.Font.Size = 5;
                            worksheet.Cells[1, i].AutoFitColumns(3);
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                        worksheet.Column(1).Width = 6;
                        worksheet.Column(2).Width = 6;
                        worksheet.Column(29).Width = 6;
                        worksheet.Column(30).Width = 6;
                        worksheet.Column(57).Width = 6;
                        worksheet.Column(58).Width = 6;
                        worksheet.Column(85).Width = 6;
                        worksheet.Column(86).Width = 6;
                        worksheet.View.ZoomScale = 180;

                        start = worksheet.Dimension.Start;
                        end = worksheet.Dimension.End;
                        endLetter = ExcelCellAddress.GetColumnLetter(end.Column);
                        for (int iRow = start.Row; iRow <= end.Row; iRow++)
                        {
                            worksheet.Cells[$"A{iRow}:{endLetter}{iRow}"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            worksheet.Cells[$"A{iRow}:{endLetter}{iRow}"].Style.Border.Bottom.Color.SetColor(borderBottomColor);
                        }

                        #endregion

                        worksheet = xlPackage.Workbook.Worksheets.Add("Cosechas mensuales 30 dias");

                        #region INDICADORES 4

                        row = 1;
                        worksheet.Cells[row, 3].Value = "Clientes";
                        worksheet.Cells[row, 4].Value = "Mes 1";
                        worksheet.Cells[row, 5].Value = "Mes 2";
                        worksheet.Cells[row, 6].Value = "Mes 3";
                        worksheet.Cells[row, 7].Value = "Mes 4";
                        worksheet.Cells[row, 8].Value = "Mes 5";
                        worksheet.Cells[row, 9].Value = "Mes 6";
                        worksheet.Cells[row, 10].Value = "Mes 7";
                        worksheet.Cells[row, 11].Value = "Mes 8";
                        worksheet.Cells[row, 12].Value = "Mes 9";
                        worksheet.Cells[row, 13].Value = "Mes 10";
                        worksheet.Cells[row, 14].Value = "Mes 11";
                        worksheet.Cells[row, 15].Value = "Mes 12";
                        worksheet.Cells[row, 16].Value = "Mes 13";
                        worksheet.Cells[row, 17].Value = "Mes 14";
                        worksheet.Cells[row, 18].Value = "Mes 15";
                        worksheet.Cells[row, 19].Value = "Mes 16";
                        worksheet.Cells[row, 20].Value = "Mes 17";
                        worksheet.Cells[row, 21].Value = "Mes 18";
                        worksheet.Cells[row, 22].Value = "Mes 19";
                        worksheet.Cells[row, 23].Value = "Mes 20";
                        worksheet.Cells[row, 24].Value = "Mes 21";
                        worksheet.Cells[row, 25].Value = "Mes 22";
                        worksheet.Cells[row, 26].Value = "Mes 23";
                        worksheet.Cells[row, 27].Value = "Mes 24";

                        worksheet.Cells[row, 31].Value = "Clientes";
                        worksheet.Cells[row, 32].Value = "Mes 1";
                        worksheet.Cells[row, 33].Value = "Mes 2";
                        worksheet.Cells[row, 34].Value = "Mes 3";
                        worksheet.Cells[row, 35].Value = "Mes 4";
                        worksheet.Cells[row, 36].Value = "Mes 5";
                        worksheet.Cells[row, 37].Value = "Mes 6";
                        worksheet.Cells[row, 38].Value = "Mes 7";
                        worksheet.Cells[row, 39].Value = "Mes 8";
                        worksheet.Cells[row, 40].Value = "Mes 9";
                        worksheet.Cells[row, 41].Value = "Mes 10";
                        worksheet.Cells[row, 42].Value = "Mes 11";
                        worksheet.Cells[row, 43].Value = "Mes 12";
                        worksheet.Cells[row, 44].Value = "Mes 13";
                        worksheet.Cells[row, 45].Value = "Mes 14";
                        worksheet.Cells[row, 46].Value = "Mes 15";
                        worksheet.Cells[row, 47].Value = "Mes 16";
                        worksheet.Cells[row, 48].Value = "Mes 17";
                        worksheet.Cells[row, 49].Value = "Mes 18";
                        worksheet.Cells[row, 50].Value = "Mes 19";
                        worksheet.Cells[row, 51].Value = "Mes 20";
                        worksheet.Cells[row, 52].Value = "Mes 21";
                        worksheet.Cells[row, 53].Value = "Mes 22";
                        worksheet.Cells[row, 54].Value = "Mes 23";
                        worksheet.Cells[row, 55].Value = "Mes 24";

                        worksheet.Cells[row, 59].Value = "Clientes";
                        worksheet.Cells[row, 60].Value = "Mes 1";
                        worksheet.Cells[row, 61].Value = "Mes 2";
                        worksheet.Cells[row, 62].Value = "Mes 3";
                        worksheet.Cells[row, 63].Value = "Mes 4";
                        worksheet.Cells[row, 64].Value = "Mes 5";
                        worksheet.Cells[row, 65].Value = "Mes 6";
                        worksheet.Cells[row, 66].Value = "Mes 7";
                        worksheet.Cells[row, 67].Value = "Mes 8";
                        worksheet.Cells[row, 68].Value = "Mes 9";
                        worksheet.Cells[row, 69].Value = "Mes 10";
                        worksheet.Cells[row, 70].Value = "Mes 11";
                        worksheet.Cells[row, 71].Value = "Mes 12";
                        worksheet.Cells[row, 72].Value = "Mes 13";
                        worksheet.Cells[row, 73].Value = "Mes 14";
                        worksheet.Cells[row, 74].Value = "Mes 15";
                        worksheet.Cells[row, 75].Value = "Mes 16";
                        worksheet.Cells[row, 76].Value = "Mes 17";
                        worksheet.Cells[row, 77].Value = "Mes 18";
                        worksheet.Cells[row, 78].Value = "Mes 19";
                        worksheet.Cells[row, 79].Value = "Mes 20";
                        worksheet.Cells[row, 80].Value = "Mes 21";
                        worksheet.Cells[row, 81].Value = "Mes 22";
                        worksheet.Cells[row, 82].Value = "Mes 23";
                        worksheet.Cells[row, 83].Value = "Mes 24";

                        worksheet.Cells[row, 87].Value = "Clientes";
                        worksheet.Cells[row, 88].Value = "Mes 1";
                        worksheet.Cells[row, 89].Value = "Mes 2";
                        worksheet.Cells[row, 90].Value = "Mes 3";
                        worksheet.Cells[row, 91].Value = "Mes 4";
                        worksheet.Cells[row, 92].Value = "Mes 5";
                        worksheet.Cells[row, 93].Value = "Mes 6";
                        worksheet.Cells[row, 94].Value = "Mes 7";
                        worksheet.Cells[row, 95].Value = "Mes 8";
                        worksheet.Cells[row, 96].Value = "Mes 9";
                        worksheet.Cells[row, 97].Value = "Mes 10";
                        worksheet.Cells[row, 98].Value = "Mes 11";
                        worksheet.Cells[row, 99].Value = "Mes 12";
                        worksheet.Cells[row, 100].Value = "Mes 13";
                        worksheet.Cells[row, 101].Value = "Mes 14";
                        worksheet.Cells[row, 102].Value = "Mes 15";
                        worksheet.Cells[row, 103].Value = "Mes 16";
                        worksheet.Cells[row, 104].Value = "Mes 17";
                        worksheet.Cells[row, 105].Value = "Mes 18";
                        worksheet.Cells[row, 106].Value = "Mes 19";
                        worksheet.Cells[row, 107].Value = "Mes 20";
                        worksheet.Cells[row, 108].Value = "Mes 21";
                        worksheet.Cells[row, 109].Value = "Mes 22";
                        worksheet.Cells[row, 110].Value = "Mes 23";
                        worksheet.Cells[row, 111].Value = "Mes 24";

                        int countTable1 = 4;
                        int countTable2 = 32;
                        int countTable3 = 60;
                        int countTable4 = 88;
                        var controlCells = new List<ExcelRange>();
                        foreach (var item in dashboardData30)
                        {
                            row++;
                            column = 1;
                            worksheet.Cells[row, column].Value = item.InitDate;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                            column++;
                            worksheet.Cells[row, column].Value = item.EndDate;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                            column++;
                            worksheet.Cells[row, column].Value = item.FirstPedidosCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";

                            column++;
                            worksheet.Cells[row, column].Value = item.Client30DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client60DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client90DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client120DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client150DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client180DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client210DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client240DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client270DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client300DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client330DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client360DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client390DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client420DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client450DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client480DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client510DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client540DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client570DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client600DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client630DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client660DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client690DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client720DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            countTable1++;

                            // ************************ 

                            column = 29;
                            worksheet.Cells[row, column].Value = item.InitDate;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                            column++;
                            worksheet.Cells[row, column].Value = item.EndDate;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                            column++;
                            worksheet.Cells[row, column].Value = item.FirstPedidosCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";

                            column++;
                            worksheet.Cells[row, column].Value = item.Client30DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client60DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client90DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client120DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client150DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client180DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client210DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client240DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client270DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client300DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client330DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client360DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client390DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client420DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client450DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client480DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client510DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client540DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client570DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client600DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client630DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client660DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client690DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client720DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            countTable2++;

                            // ************************ 
                            column = 57;
                            worksheet.Cells[row, column].Value = item.InitDate;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                            column++;
                            worksheet.Cells[row, column].Value = item.EndDate;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                            column++;
                            worksheet.Cells[row, column].Value = item.FirstPedidosCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";

                            column++;
                            worksheet.Cells[row, column].Value = item.Client30DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client60DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client90DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client120DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client150DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client180DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client210DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client240DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client270DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client300DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client330DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client360DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client390DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client420DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client450DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client480DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client510DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client540DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client570DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client600DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client630DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client660DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client690DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client720DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            countTable3++;

                            // ************************ 

                            column = 85;
                            worksheet.Cells[row, column].Value = item.InitDate;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                            column++;
                            worksheet.Cells[row, column].Value = item.EndDate;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                            column++;
                            worksheet.Cells[row, column].Value = item.FirstPedidosCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";

                            column++;
                            worksheet.Cells[row, column].Value = item.Client30DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client60DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client90DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client120DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client150DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client180DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client210DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client240DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client270DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client300DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client330DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client360DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client390DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client420DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client450DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client480DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client510DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client540DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client570DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client600DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client630DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client660DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client690DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client720DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            countTable4++;

                            // ************************ 
                        }

                        column = 4;
                        int formulaControlColumn = 4;
                        string controlColumnLetter = string.Empty;

                        controlCells = controlCells.OrderBy(x => x.Start.Column).ToList();
                        int dashboardDataCount = dashboardData30.Count();
                        foreach (var item in controlCells)
                        {
                            if (dashboardData20.Count() + 2 != item.Start.Row)
                            {
                                string columnLetter = ExcelCellAddress.GetColumnLetter(item.Start.Column);
                                if (column >= 60)
                                {
                                    string controlLetter = ExcelCellAddress.GetColumnLetter(formulaControlColumn);
                                    string formula = $"=SUMPRODUCT({columnLetter}{item.Start.Row}:{columnLetter}{dashboardDataCount + 1},${controlLetter}{item.Start.Row}:${controlLetter}{dashboardDataCount + 1})/SUM(${controlLetter}{item.Start.Row}:${controlLetter}{dashboardDataCount + 1})";
                                    worksheet.Cells[dashboardDataCount + 2, column].Formula = formula;
                                    formulaControlColumn++;
                                }
                                else if (column >= 32)
                                {
                                    string formula = $"=SUMPRODUCT({columnLetter}{item.Start.Row}:{columnLetter}{dashboardDataCount + 1},${controlColumnLetter}{item.Start.Row}:${controlColumnLetter}{dashboardDataCount + 1})/SUM(${controlColumnLetter}{item.Start.Row}:${controlColumnLetter}{dashboardDataCount + 1})";
                                    worksheet.Cells[dashboardDataCount + 2, column].Formula = formula;
                                    worksheet.Cells[dashboardDataCount + 2, column].Style.Numberformat.Format = "0.00%";
                                }
                                else
                                    worksheet.Cells[dashboardDataCount + 2, column].Formula = $"SUM({columnLetter}{item.Start.Row}:{columnLetter}{dashboardDataCount + 1})";
                            }

                            if (column == 27 || (column < 27 && dashboardDataCount + 1 == item.Start.Row - 1))
                            {
                                controlColumnLetter = ExcelCellAddress.GetColumnLetter(31);
                                column = 32;
                            }
                            else if (column == 55 || (column < 55 && dashboardDataCount + 1 == item.Start.Row - 1))
                                column = 60;
                            else if (column == 83 || (column < 83 && dashboardDataCount + 1 == item.Start.Row - 1))
                            {
                                formulaControlColumn = 4;
                                column = 88;
                            }
                            else
                                column++;
                        }

                        worksheet.Cells[dashboardDataCount + 2, 3].Formula = $"=SUM(C2:C{dashboardDataCount + 1})";
                        worksheet.Cells[dashboardDataCount + 2, 31].Formula = $"=SUM(AE2:AE{dashboardDataCount + 1})";
                        worksheet.Cells[dashboardDataCount + 2, 59].Formula = $"=SUM(BG2:BG{dashboardDataCount + 1})";
                        worksheet.Cells[dashboardDataCount + 2, 87].Formula = $"=SUM(CI2:CI{dashboardDataCount + 1})";

                        worksheet.Cells[$"C{dashboardDataCount + 2}:AA{dashboardDataCount + 2}"].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        worksheet.Cells[$"AE{dashboardDataCount + 2}"].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        worksheet.Cells[$"AF{dashboardDataCount + 2}:BC{dashboardDataCount + 2}"].Style.Numberformat.Format = "0.00%";
                        worksheet.Cells[$"BG{dashboardDataCount + 2}"].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        worksheet.Cells[$"BH{dashboardDataCount + 2}:CE{dashboardDataCount + 2}"].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        worksheet.Cells[$"CI{dashboardDataCount + 2}"].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        worksheet.Cells[$"CJ{dashboardDataCount + 2}:DG{dashboardDataCount + 2}"].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";

                        worksheet.Cells["A1"].Value = "Clientes totales";
                        worksheet.Cells["A1:B1"].Merge = true;
                        worksheet.Cells["AC1"].Value = "Clientes porcentuales";
                        worksheet.Cells["AC1:AD1"].Merge = true;
                        worksheet.Cells["BE1"].Value = "Ticket promedio";
                        worksheet.Cells["BE1:BF1"].Merge = true;
                        worksheet.Cells["CG1"].Value = "Recurrencia mensual";
                        worksheet.Cells["CG1:CH1"].Merge = true;

                        worksheet.DefaultRowHeight = 9;
                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).Style.Font.Size = 5;
                            worksheet.Cells[1, i].AutoFitColumns(3);
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }

                        worksheet.Column(1).Width = 6;
                        worksheet.Column(2).Width = 6;
                        worksheet.Column(29).Width = 6;
                        worksheet.Column(30).Width = 6;
                        worksheet.Column(57).Width = 6;
                        worksheet.Column(58).Width = 6;
                        worksheet.Column(85).Width = 6;
                        worksheet.Column(86).Width = 6;
                        worksheet.View.ZoomScale = 180;

                        for (int i = 0; i < 4; i++)
                        {
                            var values = new List<decimal>();
                            var data = controlCells.Skip(i * 24).Take(24);
                            foreach (var item in data)
                            {
                                var letter = ExcelCellAddress.GetColumnLetter(item.Start.Column);
                                for (int j = 0; j < dashboardData30.Count(); j++)
                                {
                                    var value = worksheet.Cells[$"{letter}{item.Start.Row + j}"].Value;
                                    if (value == null) continue;
                                    values.Add((decimal)value);
                                }

                                var min = values.Where(x => x != 0).DefaultIfEmpty().Min();
                                var max = values.Max();
                                for (int j = 0; j < dashboardData30.Count(); j++)
                                {
                                    var value = worksheet.Cells[$"{letter}{item.Start.Row + j}"].Value;
                                    if (value == null) continue;
                                    int alpha = max == 0 ? 0 : (int)(((decimal)value * 100) / max);
                                    var color = System.Drawing.ColorTranslator.FromHtml("#" + GetCellColor(alpha));
                                    worksheet.Cells[$"{letter}{item.Start.Row + j}"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    worksheet.Cells[$"{letter}{item.Start.Row + j}"].Style.Fill.BackgroundColor.SetColor(color);
                                }
                            }
                        }

                        start = worksheet.Dimension.Start;
                        end = worksheet.Dimension.End;
                        endLetter = ExcelCellAddress.GetColumnLetter(end.Column);
                        for (int iRow = start.Row; iRow <= end.Row; iRow++)
                        {
                            worksheet.Cells[$"A{iRow}:{endLetter}{iRow}"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            worksheet.Cells[$"A{iRow}:{endLetter}{iRow}"].Style.Border.Bottom.Color.SetColor(borderBottomColor);
                        }

                        //foreach (var item in controlCells)
                        //{
                        //    var cells = worksheet.Cells[$"{item.Address}:{letter}{dashboardData30.Count() + 1}"];
                        //    cells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        //    cells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 81, 81));
                        //}

                        #endregion

                        worksheet = xlPackage.Workbook.Worksheets.Add("Cosechas mensuales 120 dias");

                        #region INDICADORES 5

                        row = 1;
                        worksheet.Cells[row, 3].Value = "Clientes";
                        worksheet.Cells[row, 4].Value = "Mes 1";
                        worksheet.Cells[row, 5].Value = "Mes 2";
                        worksheet.Cells[row, 6].Value = "Mes 3";
                        worksheet.Cells[row, 7].Value = "Mes 4";
                        worksheet.Cells[row, 8].Value = "Mes 5";
                        worksheet.Cells[row, 9].Value = "Mes 6";
                        worksheet.Cells[row, 10].Value = "Mes 7";
                        worksheet.Cells[row, 11].Value = "Mes 8";
                        worksheet.Cells[row, 12].Value = "Mes 9";
                        worksheet.Cells[row, 13].Value = "Mes 10";
                        worksheet.Cells[row, 14].Value = "Mes 11";
                        worksheet.Cells[row, 15].Value = "Mes 12";
                        worksheet.Cells[row, 16].Value = "Mes 13";
                        worksheet.Cells[row, 17].Value = "Mes 14";
                        worksheet.Cells[row, 18].Value = "Mes 15";
                        worksheet.Cells[row, 19].Value = "Mes 16";
                        worksheet.Cells[row, 20].Value = "Mes 17";
                        worksheet.Cells[row, 21].Value = "Mes 18";
                        worksheet.Cells[row, 22].Value = "Mes 19";
                        worksheet.Cells[row, 23].Value = "Mes 20";
                        worksheet.Cells[row, 24].Value = "Mes 21";
                        worksheet.Cells[row, 25].Value = "Mes 22";
                        worksheet.Cells[row, 26].Value = "Mes 23";
                        worksheet.Cells[row, 27].Value = "Mes 24";

                        worksheet.Cells[row, 31].Value = "Clientes";
                        worksheet.Cells[row, 32].Value = "Mes 1";
                        worksheet.Cells[row, 33].Value = "Mes 2";
                        worksheet.Cells[row, 34].Value = "Mes 3";
                        worksheet.Cells[row, 35].Value = "Mes 4";
                        worksheet.Cells[row, 36].Value = "Mes 5";
                        worksheet.Cells[row, 37].Value = "Mes 6";
                        worksheet.Cells[row, 38].Value = "Mes 7";
                        worksheet.Cells[row, 39].Value = "Mes 8";
                        worksheet.Cells[row, 40].Value = "Mes 9";
                        worksheet.Cells[row, 41].Value = "Mes 10";
                        worksheet.Cells[row, 42].Value = "Mes 11";
                        worksheet.Cells[row, 43].Value = "Mes 12";
                        worksheet.Cells[row, 44].Value = "Mes 13";
                        worksheet.Cells[row, 45].Value = "Mes 14";
                        worksheet.Cells[row, 46].Value = "Mes 15";
                        worksheet.Cells[row, 47].Value = "Mes 16";
                        worksheet.Cells[row, 48].Value = "Mes 17";
                        worksheet.Cells[row, 49].Value = "Mes 18";
                        worksheet.Cells[row, 50].Value = "Mes 19";
                        worksheet.Cells[row, 51].Value = "Mes 20";
                        worksheet.Cells[row, 52].Value = "Mes 21";
                        worksheet.Cells[row, 53].Value = "Mes 22";
                        worksheet.Cells[row, 54].Value = "Mes 23";
                        worksheet.Cells[row, 55].Value = "Mes 24";

                        worksheet.Cells[row, 59].Value = "Clientes";
                        worksheet.Cells[row, 60].Value = "Mes 1";
                        worksheet.Cells[row, 61].Value = "Mes 2";
                        worksheet.Cells[row, 62].Value = "Mes 3";
                        worksheet.Cells[row, 63].Value = "Mes 4";
                        worksheet.Cells[row, 64].Value = "Mes 5";
                        worksheet.Cells[row, 65].Value = "Mes 6";
                        worksheet.Cells[row, 66].Value = "Mes 7";
                        worksheet.Cells[row, 67].Value = "Mes 8";
                        worksheet.Cells[row, 68].Value = "Mes 9";
                        worksheet.Cells[row, 69].Value = "Mes 10";
                        worksheet.Cells[row, 70].Value = "Mes 11";
                        worksheet.Cells[row, 71].Value = "Mes 12";
                        worksheet.Cells[row, 72].Value = "Mes 13";
                        worksheet.Cells[row, 73].Value = "Mes 14";
                        worksheet.Cells[row, 74].Value = "Mes 15";
                        worksheet.Cells[row, 75].Value = "Mes 16";
                        worksheet.Cells[row, 76].Value = "Mes 17";
                        worksheet.Cells[row, 77].Value = "Mes 18";
                        worksheet.Cells[row, 78].Value = "Mes 19";
                        worksheet.Cells[row, 79].Value = "Mes 20";
                        worksheet.Cells[row, 80].Value = "Mes 21";
                        worksheet.Cells[row, 81].Value = "Mes 22";
                        worksheet.Cells[row, 82].Value = "Mes 23";
                        worksheet.Cells[row, 83].Value = "Mes 24";

                        worksheet.Cells[row, 87].Value = "Clientes";
                        worksheet.Cells[row, 88].Value = "Mes 1";
                        worksheet.Cells[row, 89].Value = "Mes 2";
                        worksheet.Cells[row, 90].Value = "Mes 3";
                        worksheet.Cells[row, 91].Value = "Mes 4";
                        worksheet.Cells[row, 92].Value = "Mes 5";
                        worksheet.Cells[row, 93].Value = "Mes 6";
                        worksheet.Cells[row, 94].Value = "Mes 7";
                        worksheet.Cells[row, 95].Value = "Mes 8";
                        worksheet.Cells[row, 96].Value = "Mes 9";
                        worksheet.Cells[row, 97].Value = "Mes 10";
                        worksheet.Cells[row, 98].Value = "Mes 11";
                        worksheet.Cells[row, 99].Value = "Mes 12";
                        worksheet.Cells[row, 100].Value = "Mes 13";
                        worksheet.Cells[row, 101].Value = "Mes 14";
                        worksheet.Cells[row, 102].Value = "Mes 15";
                        worksheet.Cells[row, 103].Value = "Mes 16";
                        worksheet.Cells[row, 104].Value = "Mes 17";
                        worksheet.Cells[row, 105].Value = "Mes 18";
                        worksheet.Cells[row, 106].Value = "Mes 19";
                        worksheet.Cells[row, 107].Value = "Mes 20";
                        worksheet.Cells[row, 108].Value = "Mes 21";
                        worksheet.Cells[row, 109].Value = "Mes 22";
                        worksheet.Cells[row, 110].Value = "Mes 23";
                        worksheet.Cells[row, 111].Value = "Mes 24";

                        countTable1 = 4;
                        countTable2 = 32;
                        countTable3 = 60;
                        countTable4 = 88;
                        controlCells = new List<ExcelRange>();
                        foreach (var item in dashboardData20)
                        {
                            row++;
                            column = 1;
                            worksheet.Cells[row, column].Value = item.InitDate;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                            column++;
                            worksheet.Cells[row, column].Value = item.EndDate;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                            column++;
                            worksheet.Cells[row, column].Value = item.FirstPedidosCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";

                            column++;
                            worksheet.Cells[row, column].Value = item.Client30DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client60DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client90DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client120DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client150DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client180DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client210DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client240DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client270DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client300DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client330DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client360DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client390DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client420DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client450DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client480DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client510DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client540DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client570DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client600DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client630DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client660DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client690DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client720DaysAfterCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable1 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable1 - 1 <= column && countTable1 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            countTable1++;

                            // ************************ 

                            column = 29;
                            worksheet.Cells[row, column].Value = item.InitDate;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                            column++;
                            worksheet.Cells[row, column].Value = item.EndDate;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                            column++;
                            worksheet.Cells[row, column].Value = item.FirstPedidosCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";

                            column++;
                            worksheet.Cells[row, column].Value = item.Client30DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client60DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client90DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client120DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client150DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client180DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client210DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client240DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client270DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client300DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client330DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client360DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client390DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client420DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client450DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client480DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client510DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client540DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client570DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client600DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client630DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client660DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client690DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client720DaysAfterPercentage;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                            if (countTable2 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable2 - 1 <= column && countTable2 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            countTable2++;

                            // ************************ 
                            column = 57;
                            worksheet.Cells[row, column].Value = item.InitDate;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                            column++;
                            worksheet.Cells[row, column].Value = item.EndDate;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                            column++;
                            worksheet.Cells[row, column].Value = item.FirstPedidosCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";

                            column++;
                            worksheet.Cells[row, column].Value = item.Client30DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client60DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client90DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client120DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client150DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client180DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client210DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client240DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client270DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client300DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client330DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client360DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client390DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client420DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client450DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client480DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client510DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client540DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client570DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client600DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client630DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client660DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client690DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client720DaysAfterTicket;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            if (countTable3 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable3 - 1 <= column && countTable3 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            countTable3++;

                            // ************************ 

                            column = 85;
                            worksheet.Cells[row, column].Value = item.InitDate;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                            column++;
                            worksheet.Cells[row, column].Value = item.EndDate;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                            column++;
                            worksheet.Cells[row, column].Value = item.FirstPedidosCount;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";

                            column++;
                            worksheet.Cells[row, column].Value = item.Client30DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client60DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client90DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client120DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client150DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client180DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client210DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client240DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client270DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client300DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client330DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client360DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client390DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client420DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client450DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client480DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client510DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client540DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client570DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client600DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client630DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client660DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client690DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            column++;
                            worksheet.Cells[row, column].Value = item.Client720DaysAfterRecurrence;
                            worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            if (countTable4 <= column)
                                worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                            if (countTable4 - 1 <= column && countTable4 > column)
                                FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                            countTable4++;

                            // ************************ 
                        }

                        column = 4;
                        controlColumnLetter = string.Empty;
                        controlCells = controlCells.OrderBy(x => x.Start.Column).ToList();
                        dashboardDataCount = dashboardData20.Count();
                        formulaControlColumn = 4;

                        foreach (var item in controlCells)
                        {
                            if (dashboardDataCount + 2 != item.Start.Row)
                            {
                                string columnLetter = ExcelCellAddress.GetColumnLetter(item.Start.Column);
                                if (column >= 60)
                                {
                                    string controlLetter = ExcelCellAddress.GetColumnLetter(formulaControlColumn);
                                    string formula = $"=SUMPRODUCT({columnLetter}{item.Start.Row}:{columnLetter}{dashboardDataCount + 1},${controlLetter}{item.Start.Row}:${controlLetter}{dashboardDataCount + 1})/SUM(${controlLetter}{item.Start.Row}:${controlLetter}{dashboardDataCount + 1})";
                                    worksheet.Cells[dashboardDataCount + 2, column].Formula = formula;
                                    formulaControlColumn++;
                                }
                                else if (column >= 32)
                                {
                                    string formula = $"=SUMPRODUCT({columnLetter}{item.Start.Row}:{columnLetter}{dashboardDataCount + 1},${controlColumnLetter}{item.Start.Row}:${controlColumnLetter}{dashboardDataCount + 1})/SUM(${controlColumnLetter}{item.Start.Row}:${controlColumnLetter}{dashboardDataCount + 1})";
                                    worksheet.Cells[dashboardDataCount + 2, column].Formula = formula;
                                    worksheet.Cells[dashboardDataCount + 2, column].Style.Numberformat.Format = "0.00%";
                                }
                                else
                                    worksheet.Cells[dashboardDataCount + 2, column].Formula = $"SUM({columnLetter}{item.Start.Row}:{columnLetter}{dashboardDataCount + 1})";
                            }

                            if (column == 27 || (column < 27 && dashboardDataCount + 1 == item.Start.Row - 1))
                            {
                                controlColumnLetter = ExcelCellAddress.GetColumnLetter(31);
                                column = 32;
                            }
                            else if (column == 55 || (column < 55 && dashboardDataCount + 1 == item.Start.Row - 1))
                                column = 60;
                            else if (column == 83 || (column < 83 && dashboardDataCount + 1 == item.Start.Row - 1))
                            {
                                formulaControlColumn = 4;
                                column = 88;
                            }
                            else
                                column++;
                        }

                        worksheet.Cells[dashboardDataCount + 2, 3].Formula = $"=SUM(C2:C{dashboardDataCount + 1})";
                        worksheet.Cells[dashboardDataCount + 2, 31].Formula = $"=SUM(AE2:AE{dashboardDataCount + 1})";
                        worksheet.Cells[dashboardDataCount + 2, 59].Formula = $"=SUM(BG2:BG{dashboardDataCount + 1})";
                        worksheet.Cells[dashboardDataCount + 2, 87].Formula = $"=SUM(CI2:CI{dashboardDataCount + 1})";

                        worksheet.Cells[$"C{dashboardDataCount + 2}:AA{dashboardDataCount + 2}"].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        worksheet.Cells[$"AE{dashboardDataCount + 2}"].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        worksheet.Cells[$"AF{dashboardDataCount + 2}:BC{dashboardDataCount + 2}"].Style.Numberformat.Format = "0.00%";
                        worksheet.Cells[$"BG{dashboardDataCount + 2}"].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        worksheet.Cells[$"BH{dashboardDataCount + 2}:CE{dashboardDataCount + 2}"].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        worksheet.Cells[$"CI{dashboardDataCount + 2}"].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        worksheet.Cells[$"CJ{dashboardDataCount + 2}:DG{dashboardDataCount + 2}"].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";

                        worksheet.Cells["A1"].Value = "Clientes totales";
                        worksheet.Cells["A1:B1"].Merge = true;
                        worksheet.Cells["AC1"].Value = "Clientes porcentuales";
                        worksheet.Cells["AC1:AD1"].Merge = true;
                        worksheet.Cells["BE1"].Value = "Ticket promedio";
                        worksheet.Cells["BE1:BF1"].Merge = true;
                        worksheet.Cells["CG1"].Value = "Recurrencia mensual";
                        worksheet.Cells["CG1:CH1"].Merge = true;

                        worksheet.DefaultRowHeight = 9;
                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).Style.Font.Size = 5;
                            worksheet.Cells[1, i].AutoFitColumns(3);
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }

                        worksheet.Column(1).Width = 6;
                        worksheet.Column(2).Width = 6;
                        worksheet.Column(29).Width = 6;
                        worksheet.Column(30).Width = 6;
                        worksheet.Column(57).Width = 6;
                        worksheet.Column(58).Width = 6;
                        worksheet.Column(85).Width = 6;
                        worksheet.Column(86).Width = 6;
                        worksheet.View.ZoomScale = 180;

                        for (int i = 0; i < 4; i++)
                        {
                            var values = new List<decimal>();
                            var data = controlCells.Skip(i * 24).Take(24);
                            foreach (var item in data)
                            {
                                var letter = ExcelCellAddress.GetColumnLetter(item.Start.Column);
                                for (int j = 0; j < dashboardData20.Count(); j++)
                                {
                                    var value = worksheet.Cells[$"{letter}{item.Start.Row + j}"].Value;
                                    if (value == null) continue;
                                    values.Add((decimal)value);
                                }

                                var min = values.Where(x => x != 0).DefaultIfEmpty().Min();
                                var max = values.Max();
                                for (int j = 0; j < dashboardData20.Count(); j++)
                                {
                                    var value = worksheet.Cells[$"{letter}{item.Start.Row + j}"].Value;
                                    if (value == null) continue;
                                    int alpha = (decimal)value == 0 || max == 0 ? 0 : (int)(((decimal)value * 100) / max);
                                    var color = System.Drawing.ColorTranslator.FromHtml("#" + GetCellColor(alpha));
                                    worksheet.Cells[$"{letter}{item.Start.Row + j}"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    worksheet.Cells[$"{letter}{item.Start.Row + j}"].Style.Fill.BackgroundColor.SetColor(color);
                                }
                            }
                        }

                        start = worksheet.Dimension.Start;
                        end = worksheet.Dimension.End;
                        endLetter = ExcelCellAddress.GetColumnLetter(end.Column);
                        for (int iRow = start.Row; iRow <= end.Row; iRow++)
                        {
                            worksheet.Cells[$"A{iRow}:{endLetter}{iRow}"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            worksheet.Cells[$"A{iRow}:{endLetter}{iRow}"].Style.Border.Bottom.Color.SetColor(borderBottomColor);
                        }

                        #endregion

                        worksheet = xlPackage.Workbook.Worksheets.Add("Análisis 30 dias");

                        #region Analisis 30 dias

                        row = 1;
                        column = 1;
                        worksheet.Cells[row, 2].Value = "Primera compra";
                        for (int i = 1; i <= 24; i++)
                        {
                            worksheet.Cells[row, i + 2].Value = $"Mes {i}";
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "Retención";
                        worksheet.Cells[row, 2].Value = 1;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "0.00%";
                        for (int i = 1; i <= 24; i++)
                        {
                            var columnLetter = ExcelCellAddress.GetColumnLetter(i + 31);
                            worksheet.Cells[row, i + 2].Formula = $"='Cosechas mensuales 30 dias'!{columnLetter}{dashboardData30.Count() + 2}";
                            worksheet.Cells[row, i + 2].Style.Numberformat.Format = "0.00%";
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "Ticket promedio";
                        worksheet.Cells[row, 2].Formula = $"='Indicadores históricos'!C9";
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        for (int i = 1; i <= 24; i++)
                        {
                            var columnLetter = ExcelCellAddress.GetColumnLetter(i + 59);
                            worksheet.Cells[row, i + 2].Formula = $"='Cosechas mensuales 30 dias'!{columnLetter}{dashboardData30.Count() + 2}";
                            worksheet.Cells[row, i + 2].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "Recurrencia";
                        worksheet.Cells[row, 2].Value = 1;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        for (int i = 1; i <= 24; i++)
                        {
                            var columnLetter = ExcelCellAddress.GetColumnLetter(i + 87);
                            worksheet.Cells[row, i + 2].Formula = $"='Cosechas mensuales 30 dias'!{columnLetter}{dashboardData30.Count() + 2}";
                            worksheet.Cells[row, i + 2].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "Valor mensual";
                        worksheet.Cells[row, 2].Formula = $"=B2*B3*B4";
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        for (int i = 1; i <= 24; i++)
                        {
                            var columnLetter = ExcelCellAddress.GetColumnLetter(i + 2);
                            worksheet.Cells[row, i + 2].Formula = $"={columnLetter}2*{columnLetter}3*{columnLetter}4";
                            worksheet.Cells[row, i + 2].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "LTV";
                        worksheet.Cells[row, 2].Formula = $"=B5";
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        for (int i = 1; i <= 24; i++)
                        {
                            var columnLetter = ExcelCellAddress.GetColumnLetter(i + 2);
                            var prevColumnLetter = ExcelCellAddress.GetColumnLetter(i + 1);
                            worksheet.Cells[row, i + 2].Formula = $"={columnLetter}5+{prevColumnLetter}6";
                            worksheet.Cells[row, i + 2].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        }

                        row++;
                        row++;
                        worksheet.Cells[row, 1].Value = "Primera compra";
                        worksheet.Cells[row, 2].Formula = $"='Indicadores históricos'!C9"; ;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";

                        row++;
                        worksheet.Cells[row, 1].Value = "CLTV 24 meses";
                        worksheet.Cells[row, 2].Formula = $"=SUM(B5:Z5)";
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";

                        row++;
                        worksheet.Cells[row, 1].Value = "CAC";
                        var lastColumnLetter = ExcelCellAddress.GetColumnLetter(dashboardData10.Count() + 2);
                        worksheet.Cells[row, 2].Formula = $"=SUMPRODUCT('Indicadores históricos'!C4:{lastColumnLetter}4,'Indicadores históricos'!C36:{lastColumnLetter}36)/SUM('Indicadores históricos'!C36:{lastColumnLetter}36)";
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";

                        row++;
                        worksheet.Cells[row, 1].Value = "CLTV/CAC";
                        worksheet.Cells[row, 2].Formula = $"=B9/B10";
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";

                        worksheet.DefaultRowHeight = 9;
                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).Style.Font.Size = 5;
                            worksheet.Cells[1, i].AutoFitColumns(3);
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                        worksheet.Column(1).Width = 8;
                        worksheet.View.ZoomScale = 180;

                        start = worksheet.Dimension.Start;
                        end = worksheet.Dimension.End;
                        endLetter = ExcelCellAddress.GetColumnLetter(end.Column);
                        for (int iRow = start.Row; iRow <= end.Row; iRow++)
                        {
                            worksheet.Cells[$"A{iRow}:{endLetter}{iRow}"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            worksheet.Cells[$"A{iRow}:{endLetter}{iRow}"].Style.Border.Bottom.Color.SetColor(borderBottomColor);
                        }

                        #endregion

                        worksheet = xlPackage.Workbook.Worksheets.Add("Análisis 120 dias");

                        #region Analisis 120 dias

                        row = 1;
                        column = 1;
                        worksheet.Cells[row, 2].Value = "Primera compra";
                        for (int i = 1; i <= 24; i++)
                        {
                            worksheet.Cells[row, i + 2].Value = $"Mes {i}";
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "Retención";
                        worksheet.Cells[row, 2].Value = 1;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "0.00%";
                        for (int i = 1; i <= 24; i++)
                        {
                            var columnLetter = ExcelCellAddress.GetColumnLetter(i + 31);
                            worksheet.Cells[row, i + 2].Formula = $"='Cosechas mensuales 120 dias'!{columnLetter}{dashboardData20.Count() + 2}";
                            worksheet.Cells[row, i + 2].Style.Numberformat.Format = "0.00%";
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "Ticket promedio";
                        worksheet.Cells[row, 2].Formula = $"='Indicadores históricos'!C9";
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        for (int i = 1; i <= 24; i++)
                        {
                            var columnLetter = ExcelCellAddress.GetColumnLetter(i + 59);
                            worksheet.Cells[row, i + 2].Formula = $"='Cosechas mensuales 120 dias'!{columnLetter}{dashboardData20.Count() + 2}";
                            worksheet.Cells[row, i + 2].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "Recurrencia";
                        worksheet.Cells[row, 2].Value = 1;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        for (int i = 1; i <= 24; i++)
                        {
                            var columnLetter = ExcelCellAddress.GetColumnLetter(i + 87);
                            worksheet.Cells[row, i + 2].Formula = $"='Cosechas mensuales 120 dias'!{columnLetter}{dashboardData20.Count() + 2}";
                            worksheet.Cells[row, i + 2].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "Valor mensual";
                        worksheet.Cells[row, 2].Formula = $"=B2*B3*B4";
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        for (int i = 1; i <= 24; i++)
                        {
                            var columnLetter = ExcelCellAddress.GetColumnLetter(i + 2);
                            worksheet.Cells[row, i + 2].Formula = $"={columnLetter}2*{columnLetter}3*{columnLetter}4";
                            worksheet.Cells[row, i + 2].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "LTV";
                        worksheet.Cells[row, 2].Formula = $"=B5";
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        for (int i = 1; i <= 24; i++)
                        {
                            var columnLetter = ExcelCellAddress.GetColumnLetter(i + 2);
                            var prevColumnLetter = ExcelCellAddress.GetColumnLetter(i + 1);
                            worksheet.Cells[row, i + 2].Formula = $"={columnLetter}5+{prevColumnLetter}6";
                            worksheet.Cells[row, i + 2].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        }

                        row++;
                        row++;
                        worksheet.Cells[row, 1].Value = "Primera compra";
                        worksheet.Cells[row, 2].Formula = $"='Indicadores históricos'!C9"; ;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";

                        row++;
                        worksheet.Cells[row, 1].Value = "CLTV 24 meses";
                        worksheet.Cells[row, 2].Formula = $"=SUM(B5:Z5)";
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";

                        row++;
                        worksheet.Cells[row, 1].Value = "CAC";
                        lastColumnLetter = ExcelCellAddress.GetColumnLetter(dashboardData10.Count() + 2);
                        worksheet.Cells[row, 2].Formula = $"=SUMPRODUCT('Indicadores históricos'!C4:{lastColumnLetter}4,'Indicadores históricos'!C36:{lastColumnLetter}36)/SUM('Indicadores históricos'!C36:{lastColumnLetter}36)";
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";

                        row++;
                        worksheet.Cells[row, 1].Value = "CLTV/CAC";
                        worksheet.Cells[row, 2].Formula = $"=B9/B10";
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";

                        worksheet.DefaultRowHeight = 9;
                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).Style.Font.Size = 5;
                            worksheet.Cells[1, i].AutoFitColumns(3);
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                        worksheet.Column(1).Width = 8;
                        worksheet.View.ZoomScale = 180;

                        start = worksheet.Dimension.Start;
                        end = worksheet.Dimension.End;
                        endLetter = ExcelCellAddress.GetColumnLetter(end.Column);
                        for (int iRow = start.Row; iRow <= end.Row; iRow++)
                        {
                            worksheet.Cells[$"A{iRow}:{endLetter}{iRow}"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            worksheet.Cells[$"A{iRow}:{endLetter}{iRow}"].Style.Border.Bottom.Color.SetColor(borderBottomColor);
                        }

                        #endregion

                        worksheet = xlPackage.Workbook.Worksheets.Add("Gráficas");

                        #region Gráficas

                        lastColumnLetter = ExcelCellAddress.GetColumnLetter(dashboardData15.Count() + 2);
                        int baseChartWidth = 1395;
                        int baseChartHeight = 288;
                        int lineWidth = 3;

                        // CHART 1: Gasto publicitario vs. Número de pedidos
                        var chart = worksheet.Drawings.AddChart("Gasto publicitario vs. Número de pedidos", OfficeOpenXml.Drawing.Chart.eChartType.Line);
                        var serie1 = chart.Series.Add($"'Indicadores 26 semanas'!$C$37:${lastColumnLetter}$37", $"'Indicadores 26 semanas'!$C$2:${lastColumnLetter}$2");
                        var chartType2 = chart.PlotArea.ChartTypes.Add(OfficeOpenXml.Drawing.Chart.eChartType.Line);
                        var serie2 = chartType2.Series.Add($"'Indicadores 26 semanas'!$C$34:${lastColumnLetter}$34", $"'Indicadores 26 semanas'!$C$2:${lastColumnLetter}$2");

                        serie1.Border.Width = lineWidth;
                        serie2.Border.Width = lineWidth;

                        serie1.Header = "Total de pedidos del periodo";
                        serie2.Header = "Gasto publicitario del periodo";
                        chartType2.Legend.Position = OfficeOpenXml.Drawing.Chart.eLegendPosition.Top;
                        chart.Legend.Position = OfficeOpenXml.Drawing.Chart.eLegendPosition.Top;

                        chart.UseSecondaryAxis = true;
                        chart.SetPosition(0, 20, 0, 20);
                        chart.SetSize(baseChartWidth, baseChartHeight);
                        chart.Title.Text = "Gasto publicitario vs. Número de pedidos";

                        // CHART 2: Gasto publicitario vs. Nuevos clientes
                        chart = worksheet.Drawings.AddChart("Gasto publicitario vs. Nuevos clientes", OfficeOpenXml.Drawing.Chart.eChartType.Line);
                        serie1 = chart.Series.Add($"'Indicadores 26 semanas'!$C$36:${lastColumnLetter}$36", $"'Indicadores 26 semanas'!$C$2:${lastColumnLetter}$2");
                        chartType2 = chart.PlotArea.ChartTypes.Add(OfficeOpenXml.Drawing.Chart.eChartType.Line);
                        serie2 = chartType2.Series.Add($"'Indicadores 26 semanas'!$C$34:${lastColumnLetter}$34", $"'Indicadores 26 semanas'!$C$2:${lastColumnLetter}$2");

                        serie1.Border.Width = lineWidth;
                        serie2.Border.Width = lineWidth;

                        serie1.Header = "Nuevas cuentas activas del periodo";
                        serie2.Header = "Gasto publicitario del periodo";
                        chartType2.Legend.Position = OfficeOpenXml.Drawing.Chart.eLegendPosition.Top;
                        chart.Legend.Position = OfficeOpenXml.Drawing.Chart.eLegendPosition.Top;

                        chart.UseSecondaryAxis = true;
                        chart.SetPosition(16, 20, 0, 20);
                        chart.SetSize(baseChartWidth, baseChartHeight);
                        chart.Title.Text = "Gasto publicitario vs. Nuevos clientes";

                        // CHART 3: Costo por registro vs. costo por adquisición de cliente
                        chart = worksheet.Drawings.AddChart("Costo por registro vs. costo por adquisición de cliente", OfficeOpenXml.Drawing.Chart.eChartType.Line);
                        serie1 = chart.Series.Add($"'Indicadores 26 semanas'!$C$3:${lastColumnLetter}$3", $"'Indicadores 26 semanas'!$C$2:${lastColumnLetter}$2");
                        chartType2 = chart.PlotArea.ChartTypes.Add(OfficeOpenXml.Drawing.Chart.eChartType.Line);
                        serie2 = chartType2.Series.Add($"'Indicadores 26 semanas'!$C$4:${lastColumnLetter}$4", $"'Indicadores 26 semanas'!$C$2:${lastColumnLetter}$2");
                        var chartType3 = chart.PlotArea.ChartTypes.Add(OfficeOpenXml.Drawing.Chart.eChartType.Line);
                        var serie3 = chartType3.Series.Add($"'Indicadores 26 semanas'!$C$5:${lastColumnLetter}$5", $"'Indicadores 26 semanas'!$C$2:${lastColumnLetter}$2");

                        serie1.Border.Width = lineWidth;
                        serie2.Border.Width = lineWidth;
                        serie3.Border.Width = lineWidth;

                        serie1.Header = "Costo por nuevo registro";
                        serie2.Header = "CAC (Costo por adquisición de cliente)";
                        serie3.Header = "Tasa de registros a compras";
                        chartType3.Legend.Position = OfficeOpenXml.Drawing.Chart.eLegendPosition.Top;
                        chartType2.Legend.Position = OfficeOpenXml.Drawing.Chart.eLegendPosition.Top;
                        chart.Legend.Position = OfficeOpenXml.Drawing.Chart.eLegendPosition.Top;

                        chartType3.UseSecondaryAxis = true;
                        chart.SetPosition(32, 20, 0, 20);
                        chart.SetSize(baseChartWidth, baseChartHeight);
                        chart.Title.Text = "Costo por registro vs. costo por adquisición de cliente";

                        // CHART 4: Total de pedidos vs. Nuevos clientes
                        chart = worksheet.Drawings.AddChart("Total de pedidos vs. Nuevos clientes", OfficeOpenXml.Drawing.Chart.eChartType.Line);
                        serie1 = chart.Series.Add($"'Indicadores 26 semanas'!$C$36:${lastColumnLetter}$36", $"'Indicadores 26 semanas'!$C$2:${lastColumnLetter}$2");
                        chartType2 = chart.PlotArea.ChartTypes.Add(OfficeOpenXml.Drawing.Chart.eChartType.Line);
                        serie2 = chartType2.Series.Add($"'Indicadores 26 semanas'!$C$37:${lastColumnLetter}$37", $"'Indicadores 26 semanas'!$C$2:${lastColumnLetter}$2");

                        serie1.Border.Width = lineWidth;
                        serie2.Border.Width = lineWidth;

                        serie1.Header = "Nuevas cuentas activas del periodo";
                        serie2.Header = "Total de pedidos del periodo";
                        chartType2.Legend.Position = OfficeOpenXml.Drawing.Chart.eLegendPosition.Top;
                        chart.Legend.Position = OfficeOpenXml.Drawing.Chart.eLegendPosition.Top;

                        chart.UseSecondaryAxis = true;
                        chart.SetPosition(48, 20, 0, 20);
                        chart.SetSize(baseChartWidth, baseChartHeight);
                        chart.Title.Text = "Total de pedidos vs. Nuevos clientes";

                        // CHART 5: Número de pedidos diarios vs. Venta diaria
                        chart = worksheet.Drawings.AddChart("Número de pedidos diarios vs. Venta diaria", OfficeOpenXml.Drawing.Chart.eChartType.Line);
                        serie1 = chart.Series.Add($"'Indicadores 26 semanas'!$C$37:${lastColumnLetter}$37", $"'Indicadores 26 semanas'!$C$2:${lastColumnLetter}$2");
                        chartType2 = chart.PlotArea.ChartTypes.Add(OfficeOpenXml.Drawing.Chart.eChartType.Line);
                        serie2 = chartType2.Series.Add($"'Indicadores 26 semanas'!$C$38:${lastColumnLetter}$38", $"'Indicadores 26 semanas'!$C$2:${lastColumnLetter}$2");

                        serie1.Border.Width = lineWidth;
                        serie2.Border.Width = lineWidth;

                        serie1.Header = "Total de pedidos del periodo";
                        serie2.Header = "Total de ventas del periodo";
                        chartType2.Legend.Position = OfficeOpenXml.Drawing.Chart.eLegendPosition.Top;
                        chart.Legend.Position = OfficeOpenXml.Drawing.Chart.eLegendPosition.Top;

                        chart.UseSecondaryAxis = true;
                        chart.SetPosition(64, 20, 0, 20);
                        chart.SetSize(baseChartWidth, baseChartHeight);
                        chart.Title.Text = "Número de pedidos diarios vs. Venta diaria";

                        // CHART 5: Ticket promedio vs. recurrencia
                        chart = worksheet.Drawings.AddChart("Ticket promedio vs. recurrencia", OfficeOpenXml.Drawing.Chart.eChartType.Line);
                        serie1 = chart.Series.Add($"'Indicadores 26 semanas'!$C$11:${lastColumnLetter}$11", $"'Indicadores 26 semanas'!$C$2:${lastColumnLetter}$2");
                        chartType2 = chart.PlotArea.ChartTypes.Add(OfficeOpenXml.Drawing.Chart.eChartType.Line);
                        serie2 = chartType2.Series.Add($"'Indicadores 26 semanas'!$C$8:${lastColumnLetter}$8", $"'Indicadores 26 semanas'!$C$2:${lastColumnLetter}$2");

                        serie1.Border.Width = lineWidth;
                        serie2.Border.Width = lineWidth;

                        serie1.Header = "Recurrencia mensual";
                        serie2.Header = "Ticket promedio";
                        chartType2.Legend.Position = OfficeOpenXml.Drawing.Chart.eLegendPosition.Top;
                        chart.Legend.Position = OfficeOpenXml.Drawing.Chart.eLegendPosition.Top;

                        chart.UseSecondaryAxis = true;
                        chart.SetPosition(80, 20, 0, 20);
                        chart.SetSize(baseChartWidth, baseChartHeight);
                        chart.Title.Text = "Ticket promedio vs. recurrencia";

                        // CHART 6: MK0014 [Costumer Annual Contribution (mensual) * Tasa de Retención] / CAC
                        chart = worksheet.Drawings.AddChart("MK0014 [Costumer Annual Contribution (mensual) * Tasa de Retención] / CAC", OfficeOpenXml.Drawing.Chart.eChartType.Line);
                        serie1 = chart.Series.Add($"'Indicadores 26 semanas'!$C$28:${lastColumnLetter}$28", $"'Indicadores 26 semanas'!$C$2:${lastColumnLetter}$2");

                        serie1.Border.Width = lineWidth;

                        serie1.Header = "[Costumer Annual Contribution (mensual) * Tasa de Retención] / CAC";
                        chart.Legend.Position = OfficeOpenXml.Drawing.Chart.eLegendPosition.Top;

                        chart.SetPosition(96, 20, 0, 20);
                        chart.SetSize(baseChartWidth, baseChartHeight);
                        chart.Title.Text = "MK0014 [Costumer Annual Contribution (mensual) * Tasa de Retención] / CAC";

                        #endregion

                        xlPackage.Save();
                    }

                    return File(stream.ToArray(), MimeTypes.TextXlsx, $"reporte_mkt.xlsx");
                }
            }
            ErrorNotification("No hay datos registrados por el momento");
            return RedirectToAction("DashboardData");
        }

        [HttpGet]
        public IActionResult GenerateDashboardData(string specificDate)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var query = OrderUtils.GetFilteredOrders(_orderService);
            if (!string.IsNullOrEmpty(specificDate))
            {
                var parseDate = DateTime.ParseExact(specificDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;
                var untilDate = parseDate.AddDays(6);
                query = query.Where(x => parseDate <= x.SelectedShippingDate && x.SelectedShippingDate <= untilDate);
            }
            var orders = query.ToList();
            var controlDateUtc = DateTime.UtcNow;
            var versionCode = Nop.Services.TeedCommerceStores.CurrentStore == Nop.Services.TeedStores.CentralEnLinea ? 0 : 1;

            var count = 0;
            try
            {
                _marketingDashboardDataService.GenerateDashboardData10(orders, controlDateUtc, versionCode);
                count++;
                _marketingDashboardDataService.GenerateDashboardData20(orders, controlDateUtc, versionCode);
                count++;
                _marketingDashboardDataService.GenerateDashboardData30(orders, controlDateUtc, versionCode, true);
                count++;
                _marketingDashboardDataService.GenerateDashboardData40(orders, controlDateUtc, versionCode);
                count++;

                var dashboardSettings = _settingService.LoadSetting<MarketingDashboardSettings>();
                dashboardSettings.LastDashboardDataUpdateUtc = DateTime.UtcNow;
                _settingService.SaveSetting(dashboardSettings);
                count++;
            }
            catch (Exception e)
            {
                _ = e;
            }

            return NoContent();
        }

        // Generate 30 days controlled data
        public IActionResult GenerateControlledDashboardDataExcel()
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var customeIds = new List<int>()
            {
                1
            };

            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => customeIds.Contains(x.CustomerId)).ToList();
            var controlDateUtc = DateTime.UtcNow;
            var versionCode = Nop.Services.TeedCommerceStores.CurrentStore == Nop.Services.TeedStores.CentralEnLinea ? 0 : 1;

            var dashboardData30 = _marketingDashboardDataService.GenerateDashboardData30(orders, controlDateUtc, versionCode, false);
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    int column = 0;
                    var borderBottomColor = System.Drawing.ColorTranslator.FromHtml("#c5c5c5");
                    var worksheet = xlPackage.Workbook.Worksheets.Add("Cosechas mensuales 30 dias");

                    #region INDICADORES 4

                    int row = 1;
                    worksheet.Cells[row, 3].Value = "Clientes";
                    worksheet.Cells[row, 4].Value = "Mes 1";
                    worksheet.Cells[row, 5].Value = "Mes 2";
                    worksheet.Cells[row, 6].Value = "Mes 3";
                    worksheet.Cells[row, 7].Value = "Mes 4";
                    worksheet.Cells[row, 8].Value = "Mes 5";
                    worksheet.Cells[row, 9].Value = "Mes 6";
                    worksheet.Cells[row, 10].Value = "Mes 7";
                    worksheet.Cells[row, 11].Value = "Mes 8";
                    worksheet.Cells[row, 12].Value = "Mes 9";
                    worksheet.Cells[row, 13].Value = "Mes 10";
                    worksheet.Cells[row, 14].Value = "Mes 11";
                    worksheet.Cells[row, 15].Value = "Mes 12";
                    worksheet.Cells[row, 16].Value = "Mes 13";
                    worksheet.Cells[row, 17].Value = "Mes 14";
                    worksheet.Cells[row, 18].Value = "Mes 15";
                    worksheet.Cells[row, 19].Value = "Mes 16";
                    worksheet.Cells[row, 20].Value = "Mes 17";
                    worksheet.Cells[row, 21].Value = "Mes 18";
                    worksheet.Cells[row, 22].Value = "Mes 19";
                    worksheet.Cells[row, 23].Value = "Mes 20";
                    worksheet.Cells[row, 24].Value = "Mes 21";
                    worksheet.Cells[row, 25].Value = "Mes 22";
                    worksheet.Cells[row, 26].Value = "Mes 23";
                    worksheet.Cells[row, 27].Value = "Mes 24";

                    worksheet.Cells[row, 31].Value = "Clientes";
                    worksheet.Cells[row, 32].Value = "Mes 1";
                    worksheet.Cells[row, 33].Value = "Mes 2";
                    worksheet.Cells[row, 34].Value = "Mes 3";
                    worksheet.Cells[row, 35].Value = "Mes 4";
                    worksheet.Cells[row, 36].Value = "Mes 5";
                    worksheet.Cells[row, 37].Value = "Mes 6";
                    worksheet.Cells[row, 38].Value = "Mes 7";
                    worksheet.Cells[row, 39].Value = "Mes 8";
                    worksheet.Cells[row, 40].Value = "Mes 9";
                    worksheet.Cells[row, 41].Value = "Mes 10";
                    worksheet.Cells[row, 42].Value = "Mes 11";
                    worksheet.Cells[row, 43].Value = "Mes 12";
                    worksheet.Cells[row, 44].Value = "Mes 13";
                    worksheet.Cells[row, 45].Value = "Mes 14";
                    worksheet.Cells[row, 46].Value = "Mes 15";
                    worksheet.Cells[row, 47].Value = "Mes 16";
                    worksheet.Cells[row, 48].Value = "Mes 17";
                    worksheet.Cells[row, 49].Value = "Mes 18";
                    worksheet.Cells[row, 50].Value = "Mes 19";
                    worksheet.Cells[row, 51].Value = "Mes 20";
                    worksheet.Cells[row, 52].Value = "Mes 21";
                    worksheet.Cells[row, 53].Value = "Mes 22";
                    worksheet.Cells[row, 54].Value = "Mes 23";
                    worksheet.Cells[row, 55].Value = "Mes 24";

                    worksheet.Cells[row, 59].Value = "Clientes";
                    worksheet.Cells[row, 60].Value = "Mes 1";
                    worksheet.Cells[row, 61].Value = "Mes 2";
                    worksheet.Cells[row, 62].Value = "Mes 3";
                    worksheet.Cells[row, 63].Value = "Mes 4";
                    worksheet.Cells[row, 64].Value = "Mes 5";
                    worksheet.Cells[row, 65].Value = "Mes 6";
                    worksheet.Cells[row, 66].Value = "Mes 7";
                    worksheet.Cells[row, 67].Value = "Mes 8";
                    worksheet.Cells[row, 68].Value = "Mes 9";
                    worksheet.Cells[row, 69].Value = "Mes 10";
                    worksheet.Cells[row, 70].Value = "Mes 11";
                    worksheet.Cells[row, 71].Value = "Mes 12";
                    worksheet.Cells[row, 72].Value = "Mes 13";
                    worksheet.Cells[row, 73].Value = "Mes 14";
                    worksheet.Cells[row, 74].Value = "Mes 15";
                    worksheet.Cells[row, 75].Value = "Mes 16";
                    worksheet.Cells[row, 76].Value = "Mes 17";
                    worksheet.Cells[row, 77].Value = "Mes 18";
                    worksheet.Cells[row, 78].Value = "Mes 19";
                    worksheet.Cells[row, 79].Value = "Mes 20";
                    worksheet.Cells[row, 80].Value = "Mes 21";
                    worksheet.Cells[row, 81].Value = "Mes 22";
                    worksheet.Cells[row, 82].Value = "Mes 23";
                    worksheet.Cells[row, 83].Value = "Mes 24";

                    worksheet.Cells[row, 87].Value = "Clientes";
                    worksheet.Cells[row, 88].Value = "Mes 1";
                    worksheet.Cells[row, 89].Value = "Mes 2";
                    worksheet.Cells[row, 90].Value = "Mes 3";
                    worksheet.Cells[row, 91].Value = "Mes 4";
                    worksheet.Cells[row, 92].Value = "Mes 5";
                    worksheet.Cells[row, 93].Value = "Mes 6";
                    worksheet.Cells[row, 94].Value = "Mes 7";
                    worksheet.Cells[row, 95].Value = "Mes 8";
                    worksheet.Cells[row, 96].Value = "Mes 9";
                    worksheet.Cells[row, 97].Value = "Mes 10";
                    worksheet.Cells[row, 98].Value = "Mes 11";
                    worksheet.Cells[row, 99].Value = "Mes 12";
                    worksheet.Cells[row, 100].Value = "Mes 13";
                    worksheet.Cells[row, 101].Value = "Mes 14";
                    worksheet.Cells[row, 102].Value = "Mes 15";
                    worksheet.Cells[row, 103].Value = "Mes 16";
                    worksheet.Cells[row, 104].Value = "Mes 17";
                    worksheet.Cells[row, 105].Value = "Mes 18";
                    worksheet.Cells[row, 106].Value = "Mes 19";
                    worksheet.Cells[row, 107].Value = "Mes 20";
                    worksheet.Cells[row, 108].Value = "Mes 21";
                    worksheet.Cells[row, 109].Value = "Mes 22";
                    worksheet.Cells[row, 110].Value = "Mes 23";
                    worksheet.Cells[row, 111].Value = "Mes 24";

                    int countTable1 = 4;
                    int countTable2 = 32;
                    int countTable3 = 60;
                    int countTable4 = 88;
                    var controlCells = new List<ExcelRange>();
                    foreach (var item in dashboardData30)
                    {
                        row++;
                        column = 1;
                        worksheet.Cells[row, column].Value = item.InitDate;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                        column++;
                        worksheet.Cells[row, column].Value = item.EndDate;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                        column++;
                        worksheet.Cells[row, column].Value = item.FirstPedidosCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";

                        column++;
                        worksheet.Cells[row, column].Value = item.Client30DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client60DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client90DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client120DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client150DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client180DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client210DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client240DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client270DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client300DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client330DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client360DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client390DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client420DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client450DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client480DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client510DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client540DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client570DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client600DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client630DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client660DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client690DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client720DaysAfterCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable1 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable1 - 1 <= column && countTable1 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        countTable1++;

                        // ************************ 

                        column = 29;
                        worksheet.Cells[row, column].Value = item.InitDate;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                        column++;
                        worksheet.Cells[row, column].Value = item.EndDate;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                        column++;
                        worksheet.Cells[row, column].Value = item.FirstPedidosCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";

                        column++;
                        worksheet.Cells[row, column].Value = item.Client30DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client60DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client90DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client120DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client150DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client180DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client210DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client240DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client270DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client300DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client330DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client360DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client390DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client420DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client450DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client480DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client510DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client540DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client570DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client600DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client630DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client660DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client690DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client720DaysAfterPercentage;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "#0.00%";
                        if (countTable2 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable2 - 1 <= column && countTable2 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        countTable2++;

                        // ************************ 
                        column = 57;
                        worksheet.Cells[row, column].Value = item.InitDate;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                        column++;
                        worksheet.Cells[row, column].Value = item.EndDate;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                        column++;
                        worksheet.Cells[row, column].Value = item.FirstPedidosCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";

                        column++;
                        worksheet.Cells[row, column].Value = item.Client30DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client60DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client90DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client120DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client150DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client180DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client210DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client240DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client270DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client300DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client330DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client360DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client390DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client420DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client450DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client480DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client510DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client540DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client570DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client600DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client630DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client660DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client690DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client720DaysAfterTicket;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                        if (countTable3 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable3 - 1 <= column && countTable3 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        countTable3++;

                        // ************************ 

                        column = 85;
                        worksheet.Cells[row, column].Value = item.InitDate;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                        column++;
                        worksheet.Cells[row, column].Value = item.EndDate;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";

                        column++;
                        worksheet.Cells[row, column].Value = item.FirstPedidosCount;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";

                        column++;
                        worksheet.Cells[row, column].Value = item.Client30DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client60DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client90DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client120DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client150DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client180DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client210DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client240DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client270DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client300DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client330DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client360DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client390DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client420DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client450DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client480DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client510DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client540DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client570DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client600DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client630DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client660DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client690DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        column++;
                        worksheet.Cells[row, column].Value = item.Client720DaysAfterRecurrence;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        if (countTable4 <= column)
                            worksheet.Cells[row, column].Style.Font.Color.SetColor(System.Drawing.Color.LightGray);
                        if (countTable4 - 1 <= column && countTable4 > column)
                            FormatBackground(ref worksheet, ref row, ref column, ref controlCells);

                        countTable4++;

                        // ************************ 
                    }

                    column = 4;
                    int formulaControlColumn = 4;
                    string controlColumnLetter = string.Empty;

                    controlCells = controlCells.OrderBy(x => x.Start.Column).ToList();
                    int dashboardDataCount = dashboardData30.Count();
                    foreach (var item in controlCells)
                    {
                        if (dashboardData30.Count() + 2 != item.Start.Row)
                        {
                            string columnLetter = ExcelCellAddress.GetColumnLetter(item.Start.Column);
                            if (column >= 60)
                            {
                                string controlLetter = ExcelCellAddress.GetColumnLetter(formulaControlColumn);
                                string formula = $"=SUMPRODUCT({columnLetter}{item.Start.Row}:{columnLetter}{dashboardDataCount + 1},${controlLetter}{item.Start.Row}:${controlLetter}{dashboardDataCount + 1})/SUM(${controlLetter}{item.Start.Row}:${controlLetter}{dashboardDataCount + 1})";
                                worksheet.Cells[dashboardDataCount + 2, column].Formula = formula;
                                formulaControlColumn++;
                            }
                            else if (column >= 32)
                            {
                                string formula = $"=SUMPRODUCT({columnLetter}{item.Start.Row}:{columnLetter}{dashboardDataCount + 1},${controlColumnLetter}{item.Start.Row}:${controlColumnLetter}{dashboardDataCount + 1})/SUM(${controlColumnLetter}{item.Start.Row}:${controlColumnLetter}{dashboardDataCount + 1})";
                                worksheet.Cells[dashboardDataCount + 2, column].Formula = formula;
                                worksheet.Cells[dashboardDataCount + 2, column].Style.Numberformat.Format = "0.00%";
                            }
                            else
                                worksheet.Cells[dashboardDataCount + 2, column].Formula = $"SUM({columnLetter}{item.Start.Row}:{columnLetter}{dashboardDataCount + 1})";
                        }

                        if (column == 27 || (column < 27 && dashboardDataCount + 1 == item.Start.Row - 1))
                        {
                            controlColumnLetter = ExcelCellAddress.GetColumnLetter(31);
                            column = 32;
                        }
                        else if (column == 55 || (column < 55 && dashboardDataCount + 1 == item.Start.Row - 1))
                            column = 60;
                        else if (column == 83 || (column < 83 && dashboardDataCount + 1 == item.Start.Row - 1))
                        {
                            formulaControlColumn = 4;
                            column = 88;
                        }
                        else
                            column++;
                    }

                    worksheet.Cells[dashboardDataCount + 2, 3].Formula = $"=SUM(C2:C{dashboardDataCount + 1})";
                    worksheet.Cells[dashboardDataCount + 2, 31].Formula = $"=SUM(AE2:AE{dashboardDataCount + 1})";
                    worksheet.Cells[dashboardDataCount + 2, 59].Formula = $"=SUM(BG2:BG{dashboardDataCount + 1})";
                    worksheet.Cells[dashboardDataCount + 2, 87].Formula = $"=SUM(CI2:CI{dashboardDataCount + 1})";

                    worksheet.Cells[$"C{dashboardDataCount + 2}:AA{dashboardDataCount + 2}"].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                    worksheet.Cells[$"AE{dashboardDataCount + 2}"].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                    worksheet.Cells[$"AF{dashboardDataCount + 2}:BC{dashboardDataCount + 2}"].Style.Numberformat.Format = "0.00%";
                    worksheet.Cells[$"BG{dashboardDataCount + 2}"].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                    worksheet.Cells[$"BH{dashboardDataCount + 2}:CE{dashboardDataCount + 2}"].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                    worksheet.Cells[$"CI{dashboardDataCount + 2}"].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                    worksheet.Cells[$"CJ{dashboardDataCount + 2}:DG{dashboardDataCount + 2}"].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";

                    worksheet.Cells["A1"].Value = "Clientes totales";
                    worksheet.Cells["A1:B1"].Merge = true;
                    worksheet.Cells["AC1"].Value = "Clientes porcentuales";
                    worksheet.Cells["AC1:AD1"].Merge = true;
                    worksheet.Cells["BE1"].Value = "Ticket promedio";
                    worksheet.Cells["BE1:BF1"].Merge = true;
                    worksheet.Cells["CG1"].Value = "Recurrencia mensual";
                    worksheet.Cells["CG1:CH1"].Merge = true;

                    worksheet.DefaultRowHeight = 9;
                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).Style.Font.Size = 5;
                        worksheet.Cells[1, i].AutoFitColumns(3);
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    worksheet.Column(1).Width = 6;
                    worksheet.Column(2).Width = 6;
                    worksheet.Column(29).Width = 6;
                    worksheet.Column(30).Width = 6;
                    worksheet.Column(57).Width = 6;
                    worksheet.Column(58).Width = 6;
                    worksheet.Column(85).Width = 6;
                    worksheet.Column(86).Width = 6;
                    worksheet.View.ZoomScale = 180;

                    for (int i = 0; i < 4; i++)
                    {
                        var values = new List<decimal>();
                        var data = controlCells.Skip(i * 24).Take(24);
                        foreach (var item in data)
                        {
                            var letter = ExcelCellAddress.GetColumnLetter(item.Start.Column);
                            for (int j = 0; j < dashboardData30.Count(); j++)
                            {
                                var value = worksheet.Cells[$"{letter}{item.Start.Row + j}"].Value;
                                if (value == null) continue;
                                values.Add((decimal)value);
                            }

                            var min = values.Where(x => x != 0).DefaultIfEmpty().Min();
                            var max = values.Max();
                            for (int j = 0; j < dashboardData30.Count(); j++)
                            {
                                var value = worksheet.Cells[$"{letter}{item.Start.Row + j}"].Value;
                                if (value == null) continue;
                                int alpha = max == 0 ? 0 : (int)(((decimal)value * 100) / max);
                                var color = System.Drawing.ColorTranslator.FromHtml("#" + GetCellColor(alpha));
                                worksheet.Cells[$"{letter}{item.Start.Row + j}"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                worksheet.Cells[$"{letter}{item.Start.Row + j}"].Style.Fill.BackgroundColor.SetColor(color);
                            }
                        }
                    }

                    var start = worksheet.Dimension.Start;
                    var end = worksheet.Dimension.End;
                    string endLetter = ExcelCellAddress.GetColumnLetter(end.Column);
                    for (int iRow = start.Row; iRow <= end.Row; iRow++)
                    {
                        worksheet.Cells[$"A{iRow}:{endLetter}{iRow}"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        worksheet.Cells[$"A{iRow}:{endLetter}{iRow}"].Style.Border.Bottom.Color.SetColor(borderBottomColor);
                    }

                    //foreach (var item in controlCells)
                    //{
                    //    var cells = worksheet.Cells[$"{item.Address}:{letter}{dashboardData30.Count() + 1}"];
                    //    cells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    //    cells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 81, 81));
                    //}

                    #endregion

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"reporte_mkt.xlsx");
            }
        }

        public IActionResult AutomaticDashboardData()
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var lastUpdateDate = _settingService.LoadSetting<MarketingDashboardSettings>().LastAutomaticDashboardDataUpdateUtc;
            var dashboardData = _marketingAutomaticExpenseService.GetAll()
                .Where(x => !x.Deleted)
                .OrderByDescending(x => x.InitDate).ToList();

            var model = dashboardData.Select(x => new AutomaticExpensesModel
            {
                InitDate = x.InitDate,
                EndDate = x.InitDate.AddDays(6),
                DiscountsByCoupons = x.DiscountByCoupons,
                DiscountsByShipping = x.DiscountByShipping,
                DiscountsByProducts = x.DiscountByProducts,
                Balances = x.Balances
            }).ToList();

            return View("~/Plugins/Teed.Plugin.MarketingDashboard/Views/MarketingDashboard/AutomaticDashboardData.cshtml", model);
        }

        public IActionResult ExportAutomaticDataExcel()
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var getAllTodo = _marketingAutomaticExpenseService.GetAll()
                .Where(x => !x.Deleted)
                .OrderByDescending(x => x.InitDate).ToList();
            if (getAllTodo.Count() > 0)
            {
                var discountsByProducts = getAllTodo.Select(x => new { x.InitDate, x.DiscountByProducts }).ToList();
                var discountsByCoupons = getAllTodo.Select(x => new { x.InitDate, x.DiscountByCoupons }).ToList();
                var discountsByShipping = getAllTodo.Select(x => new { x.InitDate, x.DiscountByShipping }).ToList();
                var balances = getAllTodo.Select(x => new { x.InitDate, x.Balances }).ToList();

                using (var stream = new MemoryStream())
                {
                    using (var xlPackage = new ExcelPackage(stream))
                    {
                        int column = 0;
                        var borderBottomColor = System.Drawing.ColorTranslator.FromHtml("#c5c5c5");
                        var worksheet = xlPackage.Workbook.Worksheets.Add("CONDIFENCIAL");

                        int row = 1;
                        int col = 1;

                        worksheet.Cells[row, col].Value = "Indicadores semanales de MKT";
                        worksheet.Cells[row + 1, col].Value = "Indicador semanal";

                        col = 2;
                        foreach (var item in discountsByProducts)
                        {
                            worksheet.Cells[1, col].Value = item.InitDate;
                            worksheet.Cells[1, col].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[2, col].Value = item.InitDate.AddDays(6);
                            worksheet.Cells[2, col].Style.Numberformat.Format = "dd-mm-yyyy";
                            col++;
                        }
                        row = 3;

                        worksheet.Cells[row, 1].Value = "Uso de cupones";
                        col = 2;
                        foreach (var item in discountsByCoupons)
                        {
                            worksheet.Cells[row, col].Value = item.DiscountByCoupons;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }
                        row++;

                        worksheet.Cells[row, 1].Value = "Descuentos en producto";
                        col = 2;
                        foreach (var item in discountsByProducts)
                        {
                            worksheet.Cells[row, col].Value = item.DiscountByProducts;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }
                        row++;

                        worksheet.Cells[row, 1].Value = "Saldos";
                        col = 2;
                        foreach (var item in balances)
                        {
                            worksheet.Cells[row, col].Value = item.Balances;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }
                        row++;

                        worksheet.Cells[row, 1].Value = "Descuentos en envío";
                        col = 2;
                        foreach (var item in discountsByShipping)
                        {
                            worksheet.Cells[row, col].Value = item.DiscountByShipping;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                            col++;
                        }
                        row++;

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                            worksheet.Cells[2, i].Style.Font.Bold = true;
                        }

                        xlPackage.Save();
                    }

                    return File(stream.ToArray(), MimeTypes.TextXlsx, $"reporte_gastos_automaticos_mkt.xlsx");
                }
            }
            ErrorNotification("No hay datos registrados por el momento");
            return RedirectToAction("DashboardData");
        }

        public IActionResult GenerateAutomaticDashboardData()
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            _marketingDashboardDataService.GenerateAutomaticExpensesDashboardData(DateTime.Now.Date, false);

            var dashboardSettings = _settingService.LoadSetting<MarketingDashboardSettings>();
            dashboardSettings.LastAutomaticDashboardDataUpdateUtc = DateTime.UtcNow;
            _settingService.SaveSetting(dashboardSettings);

            return NoContent();
        }

        public IActionResult BreakdownDashboardData()
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var model = PrepareBreakdownExpensesModel();
            return View("~/Plugins/Teed.Plugin.MarketingDashboard/Views/MarketingDashboard/BreakdownDashboardData.cshtml", model);
        }

        public IActionResult ExportBreakdownDataExcel()
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var model = PrepareBreakdownExpensesModel();
            if (model.Count() > 0)
            {
                using (var stream = new MemoryStream())
                {
                    using (var xlPackage = new ExcelPackage(stream))
                    {
                        int column = 0;
                        var borderBottomColor = System.Drawing.ColorTranslator.FromHtml("#c5c5c5");
                        var worksheet = xlPackage.Workbook.Worksheets.Add("CONDIFENCIAL");

                        int row = 1;
                        int col = 1;

                        worksheet.Cells[row, col].Value = "Indicadores semanales de MKT";
                        worksheet.Cells[row + 1, col].Value = "Indicador semanal";

                        var dates = model.Where(x => x.EntityId <= 0).OrderByDescending(x => x.InitDate)
                            .Select(x => new
                            {
                                x.InitDate,
                                x.EndDate
                            }).Distinct().ToList();
                        var groupedEntities = model.OrderBy(x => x.EntityName)
                            .GroupBy(x => x.EntityId).ToList();

                        col = 2;
                        foreach (var item in dates.OrderByDescending(x => x.InitDate))
                        {
                            worksheet.Cells[1, col].Value = item.InitDate;
                            worksheet.Cells[1, col].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[2, col].Value = item.InitDate.AddDays(6);
                            worksheet.Cells[2, col].Style.Numberformat.Format = "dd-mm-yyyy";
                            col++;
                        }
                        row = 3;

                        foreach (var item in groupedEntities)
                        {
                            if (item.Key > 0)
                            {
                                worksheet.Cells[row, 1].Value = item.FirstOrDefault().EntityName;
                                col = 2;
                                foreach (var date in dates)
                                {
                                    var currentData = item.Where(x => (x.InitDate <= date.InitDate && date.InitDate <= x.EndDate) || (x.InitDate <= date.EndDate && date.EndDate <= x.EndDate)).ToList();
                                    if (currentData.Any())
                                    {
                                        var amountTotal = (decimal)0;
                                        foreach (var data in currentData)
                                        {
                                            if (item.Key == 17)
                                                _ = 0;
                                            var datesBetween = Enumerable.Range(0, 1 + data.EndDate.Subtract(data.InitDate).Days)
                                             .Select(offset => data.InitDate.AddDays(offset))
                                             .ToList();
                                            var dailyAmount = data.Amount / datesBetween.Count();
                                            var daysCount = datesBetween.Where(x => date.InitDate <= x && x <= date.EndDate).Count();

                                            amountTotal += dailyAmount * daysCount;
                                        }
                                        worksheet.Cells[row, col].Value = amountTotal;
                                    }
                                    else
                                    {
                                        worksheet.Cells[row, col].Value = ((decimal)0);
                                    }
                                    worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                                    col++;
                                }
                                row++;
                            }
                            else
                            {
                                worksheet.Cells[row, 1].Value = item.FirstOrDefault().EntityName;
                                col = 2;
                                foreach (var data in item)
                                {
                                    worksheet.Cells[row, col].Value = data.Amount;
                                    worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                                    col++;
                                }
                                row++;
                            }
                        }

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                            worksheet.Cells[2, i].Style.Font.Bold = true;
                        }

                        xlPackage.Save();
                    }

                    return File(stream.ToArray(), MimeTypes.TextXlsx, $"reporte_deslgose_de_gastos_mkt.xlsx");
                }
            }
            ErrorNotification("No hay datos registrados por el momento");
            return RedirectToAction("DashboardData");
        }

        private List<BreakdownExpensesModel> PrepareBreakdownExpensesModel()
        {
            var automaticDashboardData = _marketingAutomaticExpenseService.GetAll()
                .Where(x => !x.Deleted)
                .OrderByDescending(x => x.InitDate).ToList();

            var discountsByProducts = automaticDashboardData.Select(x => new { x.InitDate, Amount = x.DiscountByProducts }).ToList();
            var discountsByCoupons = automaticDashboardData.Select(x => new { x.InitDate, Amount = x.DiscountByCoupons }).ToList();
            var discountsByShipping = automaticDashboardData.Select(x => new { x.InitDate, Amount = x.DiscountByShipping }).ToList();
            var balances = automaticDashboardData.Select(x => new { x.InitDate, Amount = x.Balances }).ToList();

            var expensesDashboardData = _marketingExpenseService.GetAll()
                .Where(x => !x.Deleted)
                .OrderByDescending(x => x.InitDate)
                .GroupBy(x => x.ExpenseTypeId).ToList();
            var expenseTypes = _marketingExpenseTypeService.GetAll()
                .Where(x => !x.Deleted)
                .ToList();

            var model = discountsByProducts.Any() ? discountsByProducts.Select(x => new BreakdownExpensesModel
            {
                EntityId = -2,
                EntityName = "Descuentos en producto",
                InitDate = x.InitDate,
                EndDate = x.InitDate.AddDays(6),
                Amount = x.Amount,
            }).ToList() : new List<BreakdownExpensesModel>();

            if (discountsByCoupons.Any())
                model.AddRange(discountsByCoupons.Select(x => new BreakdownExpensesModel
                {
                    EntityId = -1,
                    EntityName = "‌Uso de cupones",
                    InitDate = x.InitDate,
                    EndDate = x.InitDate.AddDays(6),
                    Amount = x.Amount,
                }).ToList());

            if (balances.Any())
                model.AddRange(balances.Select(x => new BreakdownExpensesModel
                {
                    EntityId = -3,
                    EntityName = "Saldos",
                    InitDate = x.InitDate,
                    EndDate = x.InitDate.AddDays(6),
                    Amount = x.Amount,
                }).ToList());

            if (discountsByShipping.Any())
                model.AddRange(discountsByShipping.Select(x => new BreakdownExpensesModel
                {
                    EntityId = -4,
                    EntityName = "Descuentos en envío",
                    InitDate = x.InitDate,
                    EndDate = x.InitDate.AddDays(6),
                    Amount = x.Amount,
                }).ToList());

            foreach (var expenses in expensesDashboardData)
            {
                if (expenses.Any())
                {
                    model.AddRange(expenses.Select(x => new BreakdownExpensesModel
                    {
                        EntityId = x.ExpenseTypeId,
                        EntityName = expenseTypes.Where(y => y.Id == x.ExpenseTypeId).FirstOrDefault()?.Name,
                        InitDate = x.InitDate,
                        EndDate = x.EndDate,
                        Amount = x.Amount,
                    }).ToList());
                }
            }

            return model;
        }

        private void FormatBackground(ref ExcelWorksheet worksheet, ref int row, ref int column, ref List<ExcelRange> controlCells)
        {
            worksheet.Cells[row, column].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[row, column].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(204, 204, 204));
            controlCells.Add(worksheet.Cells[row + 1, column]);
        }

        private string GetCellColor(int alphaLevel)
        {
            if (alphaLevel >= 0 && alphaLevel < 13)
                return "e8f5e9";
            else if (alphaLevel >= 13 && alphaLevel < 26)
                return "c8e6c9";
            else if (alphaLevel >= 26 && alphaLevel < 39)
                return "a5d6a7";
            else if (alphaLevel >= 39 && alphaLevel < 52)
                return "81c784";
            else if (alphaLevel >= 52 && alphaLevel < 65)
                return "66bb6a";
            else if (alphaLevel >= 65 && alphaLevel < 78)
                return "4caf50";
            else if (alphaLevel >= 78 && alphaLevel < 91)
                return "43a047";
            else if (alphaLevel > 91)
                return "388e3c";
            else
                return "e8f5e9";
        }
    }
}