using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.MarketingDashboard.Domain.MarketingExpenses
{
    public class MarketingDashboardData : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

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

        public decimal Client30DaysAfterCount { get; set; }
        public decimal Client60DaysAfterCount { get; set; }
        public decimal Client90DaysAfterCount { get; set; }
        public decimal Client120DaysAfterCount { get; set; }
        public decimal Client150DaysAfterCount { get; set; }
        public decimal Client180DaysAfterCount { get; set; }
        public decimal Client210DaysAfterCount { get; set; }
        public decimal Client240DaysAfterCount { get; set; }
        public decimal Client270DaysAfterCount { get; set; }
        public decimal Client300DaysAfterCount { get; set; }
        public decimal Client330DaysAfterCount { get; set; }
        public decimal Client360DaysAfterCount { get; set; }
        public decimal Client390DaysAfterCount { get; set; }
        public decimal Client420DaysAfterCount { get; set; }
        public decimal Client450DaysAfterCount { get; set; }
        public decimal Client480DaysAfterCount { get; set; }
        public decimal Client510DaysAfterCount { get; set; }
        public decimal Client540DaysAfterCount { get; set; }
        public decimal Client570DaysAfterCount { get; set; }
        public decimal Client600DaysAfterCount { get; set; }
        public decimal Client630DaysAfterCount { get; set; }
        public decimal Client660DaysAfterCount { get; set; }
        public decimal Client690DaysAfterCount { get; set; }
        public decimal Client720DaysAfterCount { get; set; }
        public decimal Client30DaysAfterPercentage { get; set; }
        public decimal Client60DaysAfterPercentage { get; set; }
        public decimal Client90DaysAfterPercentage { get; set; }
        public decimal Client120DaysAfterPercentage { get; set; }
        public decimal Client150DaysAfterPercentage { get; set; }
        public decimal Client180DaysAfterPercentage { get; set; }
        public decimal Client210DaysAfterPercentage { get; set; }
        public decimal Client240DaysAfterPercentage { get; set; }
        public decimal Client270DaysAfterPercentage { get; set; }
        public decimal Client300DaysAfterPercentage { get; set; }
        public decimal Client330DaysAfterPercentage { get; set; }
        public decimal Client360DaysAfterPercentage { get; set; }
        public decimal Client390DaysAfterPercentage { get; set; }
        public decimal Client420DaysAfterPercentage { get; set; }
        public decimal Client450DaysAfterPercentage { get; set; }
        public decimal Client480DaysAfterPercentage { get; set; }
        public decimal Client510DaysAfterPercentage { get; set; }
        public decimal Client540DaysAfterPercentage { get; set; }
        public decimal Client570DaysAfterPercentage { get; set; }
        public decimal Client600DaysAfterPercentage { get; set; }
        public decimal Client630DaysAfterPercentage { get; set; }
        public decimal Client660DaysAfterPercentage { get; set; }
        public decimal Client690DaysAfterPercentage { get; set; }
        public decimal Client720DaysAfterPercentage { get; set; }

        public decimal Client30DaysAfterTicket { get; set; }
        public decimal Client60DaysAfterTicket { get; set; }
        public decimal Client90DaysAfterTicket { get; set; }
        public decimal Client120DaysAfterTicket { get; set; }
        public decimal Client150DaysAfterTicket { get; set; }
        public decimal Client180DaysAfterTicket { get; set; }
        public decimal Client210DaysAfterTicket { get; set; }
        public decimal Client240DaysAfterTicket { get; set; }
        public decimal Client270DaysAfterTicket { get; set; }
        public decimal Client300DaysAfterTicket { get; set; }
        public decimal Client330DaysAfterTicket { get; set; }
        public decimal Client360DaysAfterTicket { get; set; }
        public decimal Client390DaysAfterTicket { get; set; }
        public decimal Client420DaysAfterTicket { get; set; }
        public decimal Client450DaysAfterTicket { get; set; }
        public decimal Client480DaysAfterTicket { get; set; }
        public decimal Client510DaysAfterTicket { get; set; }
        public decimal Client540DaysAfterTicket { get; set; }
        public decimal Client570DaysAfterTicket { get; set; }
        public decimal Client600DaysAfterTicket { get; set; }
        public decimal Client630DaysAfterTicket { get; set; }
        public decimal Client660DaysAfterTicket { get; set; }
        public decimal Client690DaysAfterTicket { get; set; }
        public decimal Client720DaysAfterTicket { get; set; }
        public decimal Client30DaysAfterRecurrence { get; set; }
        public decimal Client60DaysAfterRecurrence { get; set; }
        public decimal Client90DaysAfterRecurrence { get; set; }
        public decimal Client120DaysAfterRecurrence { get; set; }
        public decimal Client150DaysAfterRecurrence { get; set; }
        public decimal Client180DaysAfterRecurrence { get; set; }
        public decimal Client210DaysAfterRecurrence { get; set; }
        public decimal Client240DaysAfterRecurrence { get; set; }
        public decimal Client270DaysAfterRecurrence { get; set; }
        public decimal Client300DaysAfterRecurrence { get; set; }
        public decimal Client330DaysAfterRecurrence { get; set; }
        public decimal Client360DaysAfterRecurrence { get; set; }
        public decimal Client390DaysAfterRecurrence { get; set; }
        public decimal Client420DaysAfterRecurrence { get; set; }
        public decimal Client450DaysAfterRecurrence { get; set; }
        public decimal Client480DaysAfterRecurrence { get; set; }
        public decimal Client510DaysAfterRecurrence { get; set; }
        public decimal Client540DaysAfterRecurrence { get; set; }
        public decimal Client570DaysAfterRecurrence { get; set; }
        public decimal Client600DaysAfterRecurrence { get; set; }
        public decimal Client630DaysAfterRecurrence { get; set; }
        public decimal Client660DaysAfterRecurrence { get; set; }
        public decimal Client690DaysAfterRecurrence { get; set; }
        public decimal Client720DaysAfterRecurrence { get; set; }

        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GenerationDateUtc { get; set; }
        public int MarketingDashboardDataTypeId { get; set; }
    }
}
