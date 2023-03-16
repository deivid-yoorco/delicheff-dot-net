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
using Teed.Plugin.Groceries.Services;
using Nop.Web.Areas.Admin.Models.Orders;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Security;
using Teed.Plugin.Groceries.Security;

namespace Teed.Plugin.Groceries.Components
{
    [ViewComponent(Name = "LimitedCustomerRoles")]
    public class LimitedCustomerRolesViewComponent : NopViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;

        public LimitedCustomerRolesViewComponent(ICustomerService customerService,
            IWorkContext workContext, IPermissionService permissionService)
        {
            _customerService = customerService;
            _workContext = workContext;
            _permissionService = permissionService;
        }

        public IViewComponentResult Invoke(string widgetZone, int additionalData)
        {
            var customerId = additionalData;
            var model = new LimitedCustomerRolesModel();
            model.SholdDisplaySelect = _permissionService.Authorize(TeedGroceriesPermissionProvider.AssignLimitedRoles) && customerId != 0;
            if (model.SholdDisplaySelect)
            {
                var customer = _customerService.GetCustomerById(customerId);
                var allowedRoles = new List<string>()
                {
                    "buyer",
                    "delivery",
                    "employee",
                    "exemployee"
                };

                model.Roles = _customerService.GetAllCustomerRoles()
                    .Where(x => allowedRoles.Contains(x.SystemName))
                    .Select(x => new SelectListItem()
                    {
                        Text = x.Name,
                        Value = x.Id.ToString(),
                        Selected = customer.CustomerRoles.Select(cr => cr.Id).Contains(x.Id)
                    })
                    .ToList();
                model.CustomerId = customerId;
            }


            return View("~/Plugins/Teed.Plugin.Groceries/Views/Shared/Components/LimitedCustomerRoles/Default.cshtml", model);
        }
    }

    public class LimitedCustomerRolesModel
    {
        public bool SholdDisplaySelect { get; set; }
        public int CustomerId { get; set; }
        public List<SelectListItem> Roles { get; set; }
    }
}