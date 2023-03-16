using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.ShippingUserRoute
{
    public class CreateViewModel
    {
        public int ShippingRouteId { get; set; }
        public int UserInChargeId { get; set; }
        public string SelectedDate { get; set; }
    }
}