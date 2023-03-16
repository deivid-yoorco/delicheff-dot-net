using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Components
{
    [ViewComponent(Name = "OrderMarkUnpaidButton")]
    public class MarkNotPaidButtonViewController : NopViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;

        public MarkNotPaidButtonViewController(ICustomerService customerService, IWorkContext workContext)
        {
            _customerService = customerService;
            _workContext = workContext;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var model = (OrderModel)additionalData;
            return View("~/Plugins/Teed.Plugin.Groceries/Views/Shared/Components/MarkAsUnpaidButton/Default.cshtml", model);
        }
    }
}
