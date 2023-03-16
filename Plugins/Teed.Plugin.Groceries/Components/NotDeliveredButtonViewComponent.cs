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
    [ViewComponent(Name = "NotDeliveredButton")]
    public class NotDeliveredViewComponent : NopViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;

        public NotDeliveredViewComponent(ICustomerService customerService, IWorkContext workContext)
        {
            _customerService = customerService;
            _workContext = workContext;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var orderId = (int)additionalData;
            return View("~/Plugins/Teed.Plugin.Groceries/Views/NotDeliveredButton/NotDeliveredButton.cshtml", orderId);
        }
    }
}
