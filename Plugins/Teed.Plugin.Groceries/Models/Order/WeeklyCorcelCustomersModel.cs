using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Models.ShippingRoute;

namespace Teed.Plugin.Groceries.Models.Order
{
    public class WeeklyCorcelCustomersModel
    {
        public virtual int AmountOfCustomers { get; set; }
        public virtual List<WeeklyCorcelCustomersInfoModel> WeeklyCorcelCustomersInfos { get; set; }
    }

    public class WeeklyCorcelCustomersInfoModel
    {
        public virtual string StartOfWeek { get; set; }
        public virtual string EndOfWeek { get; set; }
        public virtual int AmountOfNewCustomers { get; set; }
    }
}
