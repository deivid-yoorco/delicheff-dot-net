using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Security;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class PrintReportController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;

        public PrintReportController(IPermissionService permissionService,
            IWorkContext workContext,
            IOrderService orderService)
        {
            _permissionService = permissionService;
            _workContext = workContext;
            _orderService = orderService;
        }

        public IActionResult DeliveryReportView(string date = "")
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.DeliveryReportPrint))
                return AccessDeniedView();

            DateTime controlDate = DateTime.Now.Date;
            if (!string.IsNullOrEmpty(date))
                controlDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            return View("~/Plugins/Teed.Plugin.Groceries/Views/PrintReport/DeliveryReport.cshtml", controlDate);
        }

        public IActionResult BuyerReportView(string date = "")
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.BuyerReportPrint))
                return AccessDeniedView();

            DateTime controlDate = DateTime.Now.Date;
            if (!string.IsNullOrEmpty(date))
                controlDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            return View("~/Plugins/Teed.Plugin.Groceries/Views/PrintReport/BuyerReport.cshtml", controlDate);
        }
    }
}
