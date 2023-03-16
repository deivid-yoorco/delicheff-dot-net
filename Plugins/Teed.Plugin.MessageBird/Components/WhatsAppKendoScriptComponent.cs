using Microsoft.AspNetCore.Mvc;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework.Components;
using Teed.Plugin.MessageBird.Security;

namespace Teed.Plugin.MessageBird.Components
{
    [ViewComponent(Name = "WhatsAppKendoScript")]
    public class WhatsAppKendoScriptComponent : NopViewComponent
    {
        public WhatsAppKendoScriptComponent()
        {
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            return View("~/Plugins/Teed.Plugin.MessageBird/Views/WhatsAppKendoScript.cshtml", (int)additionalData);
        }
    }
}
