using Microsoft.AspNetCore.Mvc;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.CustomerComments.Security;
using Teed.Plugin.CustomerComments.Services;

namespace Teed.Plugin.CustomerComments.Components
{
    [ViewComponent(Name = "CustomerCommentsView")]
    public class CustomerCommentsViewComponent : NopViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IDownloadService _downloadService;
        private readonly CustomerCommentService _customerCommentService;
        private readonly IPermissionService _permissionService;

        public CustomerCommentsViewComponent(ICustomerService customerService,
            IDownloadService downloadService,
            CustomerCommentService customerCommentService,
            IPermissionService permissionService)
        {
            _customerService = customerService;
            _downloadService = downloadService;
            _customerCommentService = customerCommentService;
            _permissionService = permissionService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var customerId = (int)additionalData;
            if (!_permissionService.Authorize(TeedCustomerCommentsPermissionProvider.CustomerComments))
                customerId = 0;

            return View("~/Plugins/Teed.Plugin.CustomerComments/Views/CustomerCommentsView.cshtml", customerId);
        }
    }
}
