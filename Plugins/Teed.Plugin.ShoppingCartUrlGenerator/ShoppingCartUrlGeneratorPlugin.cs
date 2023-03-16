using Microsoft.AspNetCore.Routing;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.ShoppingCartUrlGenerator.Data;
using Teed.Plugin.ShoppingCartUrlGenerator.Security;

namespace Teed.Plugin.ShoppingCartUrlGenerator
{
    public class ShoppingCartUrlGeneratorPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        private readonly PluginObjectContext _context;
        private readonly IPermissionService _permissionService;

        public ShoppingCartUrlGeneratorPlugin(IPermissionService permissionService, PluginObjectContext context)
        {
            _permissionService = permissionService;
            _context = context;

            _permissionService.InstallPermissions(new TeedShoppingCartUrlPermissionProvider());
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string> { "shopping-cart-url-select", "blogpost_page_bottom" };
        }

        public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            switch (widgetZone)
            {
                case "shopping-cart-url-select":
                    viewComponentName = "ShoppingCartUrlSelect";
                    break;
                case "blogpost_page_bottom":
                    viewComponentName = "ShoppingCartUrlInsert";
                    break;
                default:
                    viewComponentName = "";
                    break;
            }
        }

        public override void Install()
        {
            _context.Install();
            base.Install();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            if (!_permissionService.Authorize(TeedShoppingCartUrlPermissionProvider.ShoppingCartUrl)) return;
            var myPluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "ShoppingCartUrlGenerator");
            if (myPluginNode == null)
            {
                myPluginNode = new SiteMapNode()
                {
                    SystemName = "ShoppingCartUrlGenerator",
                    Title = "Url para carritos de compra",
                    Visible = true,
                    IconClass = "fa fa-cart-plus",
                    ControllerName = "ShoppingCartUrl",
                    ActionName = "List",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                };
                rootNode.ChildNodes.Add(myPluginNode);
            }
        }

        public override void Uninstall()
        {
            _context.Uninstall();
            base.Uninstall();
        }
    }
}
