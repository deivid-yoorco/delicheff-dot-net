using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.MarketingDashboard.Models.MarketingExpenses
{
    public class MarketingDashboardDataModel
    {
        public DateTime ControlDate { get; set; }
        public List<MarketingDashboardDataItem> Data { get; set; }
        public bool FromAdmin { get; set; }
        public List<SelectListItem> GenerationDates { get; set; }
    }

    public class MarketingDashboardDataItem
    {
        public decimal MarketingExpenses { get; set; }
        public decimal NewRegisteredUsersCount { get; set; }
        public decimal NewActiveCount { get; set; }
        public decimal PedidosCount { get; set; }
        public decimal SalesTotal { get; set; }
        public decimal WorkingDays { get; set; }
        public decimal TotalPedidos30Days { get; set; }
        public decimal CustomerCount60Days { get; set; }
        public decimal CustomerCount30Days { get; set; }
        public decimal FirstOrderCount30Days { get; set; }
        public decimal CustomerCount120Days { get; set; }
        public decimal CustomerCount90Days { get; set; }
        public decimal FirstOrderCount90Days { get; set; }
        public decimal NewRegisterCost { get; set; }
        public decimal AdquisitionCost { get; set; }
        public decimal BuyinRegister { get; set; }
        public decimal DailyPedidosAverage { get; set; }
        public decimal DailySalesAverage { get; set; }
        public decimal AverageTicket { get; set; }
        public decimal Recurrence { get; set; }
        public decimal MonthlyChurnRate { get; set; }
        public decimal QuarterlyChurnRate { get; set; }
        public decimal CustomerAnnualValue { get; set; }
        public decimal CustomFormula1 { get; set; }

        public decimal CustomersCountAtLeastOneOrder120DaysAndMore { get; set; }
        public decimal CustomersCountOnlyOnePedido120DaysAndMore { get; set; }
        public decimal CustomersCount2or3Pedidos120DaysAndMore { get; set; }
        public decimal CustomersCount4or5Pedidos120DaysAndMore { get; set; }
        public decimal CustomersCount6or7Pedidos120DaysAndMore { get; set; }
        public decimal CustomersCount8or9Pedidos120DaysAndMore { get; set; }
        public decimal CustomersCountMoreThan10Pedidos120DaysAndMore { get; set; }
        public decimal CustomersWithOneOrderBetween150and121days { get; set; }
        public decimal CustomersWithOneOrderBetween150and121daysAnd120daysPedido { get; set; }
        public decimal SalesFirstOrders { get; set; }
        public decimal FirstPedidosCount { get; set; }
        public decimal SalesNotFirstOrders { get; set; }
        public decimal NotFirstOrdersCount { get; set; }

        public decimal AverageTicketNewCustomers { get; set; }
        public decimal AverageTicketOldCustomers { get; set; }
        public decimal Recurrence120days { get; set; }
        public decimal Recurrence120days2or3 { get; set; }
        public decimal Recurrence120days4or5 { get; set; }
        public decimal Recurrence120days6or7 { get; set; }
        public decimal Recurrence120days8or9 { get; set; }
        public decimal Recurrence120daysMoreThan10 { get; set; }
        public decimal RetentionRate120Days { get; set; }
        public decimal CustomerAnnualContribution { get; set; }
        public decimal CustomerAnnualContributionRetention { get; set; }
        public decimal CustomerAnnualContribution120Retention { get; set; }
        public decimal CustomerAnnualContribution120Retention2or3 { get; set; }
        public decimal CustomerAnnualContribution120Retention4or5 { get; set; }
        public decimal CustomerAnnualContribution120Retention6or7 { get; set; }
        public decimal CustomerAnnualContribution120Retention8or9 { get; set; }
        public decimal CustomerAnnualContribution120RetentionMoreThan10 { get; set; }
        public decimal CustomFormula2 { get; set; }

        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
