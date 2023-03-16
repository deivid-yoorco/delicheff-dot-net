using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Components
{
    [ViewComponent(Name = "ExportExcelOrdersForDay")]
    public class ExportExcelOrdersForDayComponent : NopViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;

        public ExportExcelOrdersForDayComponent(ICustomerService customerService, IWorkContext workContext)
        {
            _customerService = customerService;
            _workContext = workContext;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            return View("~/Plugins/Teed.Plugin.Groceries/Views/ExportExcelOrdersForDay/ExportExcelOrdersForDay.cshtml");
        }
    }
}
