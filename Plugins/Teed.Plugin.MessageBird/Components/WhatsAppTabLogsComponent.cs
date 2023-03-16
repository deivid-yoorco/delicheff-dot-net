using Microsoft.AspNetCore.Mvc;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework.Components;
using Teed.Plugin.MessageBird.Security;

namespace Teed.Plugin.MessageBird.Components
{
    [ViewComponent(Name = "WhatsAppTabLogs")]
    public class WhatsAppTabLogsComponent : NopViewComponent
    {
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;

        public WhatsAppTabLogsComponent(IPermissionService permissionService,
            ICustomerService customerService)
        {
            _permissionService = permissionService;
            _customerService = customerService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            int customerId = 0;
            var customer = _customerService.GetCustomerById((int)additionalData);
            if (customer != null && 
                (_permissionService.Authorize(TeedMessageBirdPermissionProvider.MessageBirdAdmin) ||
                _permissionService.Authorize(TeedMessageBirdPermissionProvider.MessageBird)))
                customerId = customer.Id;

            return View("~/Plugins/Teed.Plugin.MessageBird/Views/WhatsAppTabLogs.cshtml", customerId);
        }
    }
}
