using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Orders;

namespace Teed.Plugin.LamyShop.Components
{
    [ViewComponent(Name = "ReportsByMonthButtons")]
    public class ReportsByMonthButtonsComponent : NopViewComponent
    {


        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            DatesIniEndModel dates = new DatesIniEndModel();
            dates.DateIni = DateTime.Now;
            dates.DateEnd = DateTime.Now;
            return View("~/Plugins/Teed.Plugin.Lamyshop/Views/ReportsByMonthButtons.cshtml", dates);
        }
    }

    public class DatesIniEndModel
    {
        public DateTime DateIni { get; set; }
        public DateTime DateEnd { get; set; }
    }
}
