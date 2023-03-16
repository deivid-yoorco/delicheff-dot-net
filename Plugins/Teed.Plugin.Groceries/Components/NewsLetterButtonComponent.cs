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
using Nop.Services.Security;
using Teed.Plugin.Groceries.Security;

namespace Teed.Plugin.Groceries.Components
{
    [ViewComponent(Name = "NewsLetterButton")]
    public class NewsLetterButtonComponent : NopViewComponent
    {
        private readonly IPermissionService _permissionService;

        public NewsLetterButtonComponent(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var permiso = false;
            if (_permissionService.Authorize(TeedGroceriesPermissionProvider.NewsLetter))
                permiso = true;

            return View("~/Plugins/Teed.Plugin.Groceries/Views/NewsLetterButton/NewsLetterButton.cshtml", permiso);
        }
    }
}
