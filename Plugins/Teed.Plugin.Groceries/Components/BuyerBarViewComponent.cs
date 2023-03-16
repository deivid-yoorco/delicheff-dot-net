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
    [ViewComponent(Name = "BuyerBar")]
    public class BuyerBarViewComponent : NopViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;

        public BuyerBarViewComponent(ICustomerService customerService, IWorkContext workContext)
        {
            _customerService = customerService;
            _workContext = workContext;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var role = _customerService.GetCustomerRoleBySystemName("buyer");
            var model = new BuyerBarModel()
            {
                BuyerId = _workContext.CurrentCustomer.Id,
                IsBuyer = _workContext.CurrentCustomer.CustomerRoles.Select(x => x.Id).Contains(role.Id)
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/OrderItemBuyer/BuyerBar.cshtml", model);
        }
    }

    public class BuyerBarModel
    {
        public bool IsBuyer { get; set; }
        public int BuyerId { get; set; }
    }
}
