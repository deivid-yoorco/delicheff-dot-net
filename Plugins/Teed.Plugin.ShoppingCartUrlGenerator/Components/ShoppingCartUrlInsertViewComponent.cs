using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.ShoppingCartUrlGenerator.Components
{
    [ViewComponent(Name = "ShoppingCartUrlInsert")]
    public class ShoppingCartUrlInsertViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            return View("~/Plugins/Teed.Plugin.ShoppingCartUrlGenerator/Views/Shared/Components/ShoppingCartUrlInsert/Default.cshtml");
        }
    }
}
