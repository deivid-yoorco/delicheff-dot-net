using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Models.ShippingRoute;

namespace Teed.Plugin.Groceries.Models.Order
{
    public class DailyOrdersModel
    {
        public DailyOrdersModel()
        {
            OrderPins = new List<MapOrderData>();
            WeeklyCorcelCustomers = new WeeklyCorcelCustomersModel();
        }

        public int OrdersTime1 { get; set; }
        public int OrdersTime2 { get; set; }
        public int OrdersTime3 { get; set; }
        public int OrdersTime4 { get; set; }
        public int OrdersTotal { get; set; }

        public int OrdersTime1Limit { get; set; }
        public int OrdersTime2Limit { get; set; }
        public int OrdersTime3Limit { get; set; }
        public int OrdersTime4Limit { get; set; }
        public int OrdersTotalLimit { get; set; }

        public string OrdersTime1Color { get; set; }
        public string OrdersTime2Color { get; set; }
        public string OrdersTime3Color { get; set; }
        public string OrdersTime4Color { get; set; }
        public string OrdersTimeTotalColor { get; set; }

        public List<OrdersByRegion> Regions { get; set; }
        public DateTime SelectedDate { get; set; }

        public string AllRegionsGoalColor { get; set; }

        public decimal TimePercentaje { get; set; }
        public string CurrentTime { get; set; }

        public List<MapOrderData> OrderPins { get; set; }

        public WeeklyCorcelCustomersModel WeeklyCorcelCustomers { get; set; }
    }

    public class DailyAmountsModel
    {
        public DailyAmountsModel()
        {
            OrderPins = new List<MapOrderData>();
            WeeklyCorcelCustomers = new WeeklyCorcelCustomersModel();
        }

        public decimal OrdersTime1 { get; set; }
        public decimal OrdersTime2 { get; set; }
        public decimal OrdersTime3 { get; set; }
        public decimal OrdersTime4 { get; set; }
        public decimal OrdersTotal { get; set; }

        public decimal OrdersTime1Limit { get; set; }
        public decimal OrdersTime2Limit { get; set; }
        public decimal OrdersTime3Limit { get; set; }
        public decimal OrdersTime4Limit { get; set; }
        public decimal OrdersTotalLimit { get; set; }

        public string OrdersTime1Color { get; set; }
        public string OrdersTime2Color { get; set; }
        public string OrdersTime3Color { get; set; }
        public string OrdersTime4Color { get; set; }
        public string OrdersTimeTotalColor { get; set; }

        public List<AmountsByRegion> Regions { get; set; }
        public DateTime SelectedDate { get; set; }

        public string AllRegionsGoalColor { get; set; }

        public decimal TimePercentaje { get; set; }
        public string CurrentTime { get; set; }

        public List<MapOrderData> OrderPins { get; set; }

        public WeeklyCorcelCustomersModel WeeklyCorcelCustomers { get; set; }
    }

    public class OrdersByRegion
    {
        public int RegionId { get; set; }
        public string RegionName { get; set; }

        public int OrdersTime1Limit { get; set; }
        public int OrdersTime2Limit { get; set; }
        public int OrdersTime3Limit { get; set; }
        public int OrdersTime4Limit { get; set; }
        public int OrdersTotalLimit { get; set; }

        public int OrdersTime1 { get; set; }
        public int OrdersTime2 { get; set; }
        public int OrdersTime3 { get; set; }
        public int OrdersTime4 { get; set; }
        public int OrdersTotal { get; set; }

        public string RegionGoalColor { get; set; }

        public decimal RegionGoal { get; set; }
    }

    public class AmountsByRegion
    {
        public int RegionId { get; set; }
        public string RegionName { get; set; }

        public decimal OrdersTime1Limit { get; set; }
        public decimal OrdersTime2Limit { get; set; }
        public decimal OrdersTime3Limit { get; set; }
        public decimal OrdersTime4Limit { get; set; }
        public decimal OrdersTotalLimit { get; set; }

        public decimal OrdersTime1 { get; set; }
        public decimal OrdersTime2 { get; set; }
        public decimal OrdersTime3 { get; set; }
        public decimal OrdersTime4 { get; set; }
        public decimal OrdersTotal { get; set; }

        public string RegionGoalColor { get; set; }

        public decimal RegionGoal { get; set; }
    }
}
