using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.MarketingDashboard.Domain.MarketingExpenses;

namespace Teed.Plugin.MarketingDashboard.Data.Mapping
{
    class MarketingDashboardDataMap : NopEntityTypeConfiguration<MarketingDashboardData>
    {
        public MarketingDashboardDataMap()
        {
            ToTable(nameof(MarketingDashboardData));
            HasKey(x => x.Id);

            Property(x => x.MarketingExpenses).HasPrecision(18, 4);
            Property(x => x.NewRegisteredUsersCount).HasPrecision(18, 4);
            Property(x => x.NewActiveCount).HasPrecision(18, 4);
            Property(x => x.PedidosCount).HasPrecision(18, 4);
            Property(x => x.SalesTotal).HasPrecision(18, 4);
            Property(x => x.WorkingDays).HasPrecision(18, 4);
            Property(x => x.TotalPedidos30Days).HasPrecision(18, 4);
            Property(x => x.CustomerCount60Days).HasPrecision(18, 4);
            Property(x => x.CustomerCount30Days).HasPrecision(18, 4);
            Property(x => x.FirstOrderCount30Days).HasPrecision(18, 4);
            Property(x => x.CustomerCount120Days).HasPrecision(18, 4);
            Property(x => x.CustomerCount90Days).HasPrecision(18, 4);
            Property(x => x.FirstOrderCount90Days).HasPrecision(18, 4);
            Property(x => x.NewRegisterCost).HasPrecision(18, 4);
            Property(x => x.AdquisitionCost).HasPrecision(18, 4);
            Property(x => x.BuyinRegister).HasPrecision(18, 4);
            Property(x => x.DailyPedidosAverage).HasPrecision(18, 4);
            Property(x => x.DailySalesAverage).HasPrecision(18, 4);
            Property(x => x.AverageTicket).HasPrecision(18, 4);
            Property(x => x.Recurrence).HasPrecision(18, 4);
            Property(x => x.MonthlyChurnRate).HasPrecision(18, 4);
            Property(x => x.QuarterlyChurnRate).HasPrecision(18, 4);
            Property(x => x.CustomerAnnualValue).HasPrecision(18, 4);
            Property(x => x.CustomFormula1).HasPrecision(18, 4);

            Property(x => x.CustomersCountAtLeastOneOrder120DaysAndMore).HasPrecision(18, 4);
            Property(x => x.CustomersCountOnlyOnePedido120DaysAndMore).HasPrecision(18, 4);
            Property(x => x.CustomersCount2or3Pedidos120DaysAndMore).HasPrecision(18, 4);
            Property(x => x.CustomersCount4or5Pedidos120DaysAndMore).HasPrecision(18, 4);
            Property(x => x.CustomersCount6or7Pedidos120DaysAndMore).HasPrecision(18, 4);
            Property(x => x.CustomersCount8or9Pedidos120DaysAndMore).HasPrecision(18, 4);
            Property(x => x.CustomersCountMoreThan10Pedidos120DaysAndMore).HasPrecision(18, 4);
            Property(x => x.CustomersWithOneOrderBetween150and121days).HasPrecision(18, 4);
            Property(x => x.CustomersWithOneOrderBetween150and121daysAnd120daysPedido).HasPrecision(18, 4);
            Property(x => x.SalesFirstOrders).HasPrecision(18, 4);
            Property(x => x.FirstPedidosCount).HasPrecision(18, 4);
            Property(x => x.SalesNotFirstOrders).HasPrecision(18, 4);
            Property(x => x.NotFirstOrdersCount).HasPrecision(18, 4);

            Property(x => x.AverageTicketNewCustomers).HasPrecision(18, 4);
            Property(x => x.AverageTicketOldCustomers).HasPrecision(18, 4);
            Property(x => x.Recurrence120days).HasPrecision(18, 4);
            Property(x => x.Recurrence120days2or3).HasPrecision(18, 4);
            Property(x => x.Recurrence120days4or5).HasPrecision(18, 4);
            Property(x => x.Recurrence120days6or7).HasPrecision(18, 4);
            Property(x => x.Recurrence120days8or9).HasPrecision(18, 4);
            Property(x => x.Recurrence120daysMoreThan10).HasPrecision(18, 4);
            Property(x => x.RetentionRate120Days).HasPrecision(18, 4);
            Property(x => x.CustomerAnnualContribution).HasPrecision(18, 4);
            Property(x => x.CustomerAnnualContributionRetention).HasPrecision(18, 4);
            Property(x => x.CustomerAnnualContribution120Retention).HasPrecision(18, 4);
            Property(x => x.CustomerAnnualContribution120Retention2or3).HasPrecision(18, 4);
            Property(x => x.CustomerAnnualContribution120Retention4or5).HasPrecision(18, 4);
            Property(x => x.CustomerAnnualContribution120Retention6or7).HasPrecision(18, 4);
            Property(x => x.CustomerAnnualContribution120Retention8or9).HasPrecision(18, 4);
            Property(x => x.CustomerAnnualContribution120RetentionMoreThan10).HasPrecision(18, 4);
            Property(x => x.CustomFormula2).HasPrecision(18, 4);

            Property(x => x.Client30DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client60DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client90DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client120DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client150DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client180DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client210DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client240DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client270DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client300DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client330DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client360DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client390DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client420DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client450DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client480DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client510DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client540DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client570DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client600DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client630DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client660DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client690DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client720DaysAfterCount).HasPrecision(18, 4);
            Property(x => x.Client30DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client60DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client90DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client120DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client150DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client180DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client210DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client240DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client270DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client300DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client330DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client360DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client390DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client420DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client450DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client480DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client510DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client540DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client570DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client600DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client630DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client660DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client690DaysAfterPercentage).HasPrecision(18, 4);
            Property(x => x.Client720DaysAfterPercentage).HasPrecision(18, 4);
        }
    }
}
