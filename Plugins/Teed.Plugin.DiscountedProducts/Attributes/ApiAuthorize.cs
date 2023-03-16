using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Controllers;
using Teed.Plugin.DiscountedProducts.Controllers;

namespace Teed.Plugin.DiscountedProducts.Attributes
{
    public class ApiAuthorize : Attribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            bool pluginInstalled = PluginManager.FindPlugin(typeof(ApiStartup))?.Installed ?? false;
            if (!pluginInstalled)
            {
                var controller = (ApiBaseController)context.Controller;
                context.Result =  controller.NotFound();
            }
        }
    }
}
