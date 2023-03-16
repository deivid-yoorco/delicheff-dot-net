using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using System.Linq;
using Teed.Plugin.ShippingByAddress.Data;

namespace Teed.Plugin.ShippingByAddress
{
    public class ShippingByAddressPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin
    {
        private readonly PluginObjectContext _context;
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;

        public ShippingByAddressPlugin(IPermissionService permissionService,
            IWebHelper webHelper, PluginObjectContext context)
        {
            _permissionService = permissionService;
            _webHelper = webHelper;
            _context = context;
        }

        public override void Install()
        {
            _context.Install();
            base.Install();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins)) return;
            var myPluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "ShippingByAddress");
            var myPluginNode2 = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "ShippingByAddress");

            if (myPluginNode == null)
            {
                rootNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "ShippingByAddress",
                    Title = "Envíos por dirección",
                    Visible = true,
                    IconClass = "fa fa-building",
                    ControllerName = "ShippingByAddress",
                    ActionName = "Index",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });

                rootNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "ShippingBranch",
                    Title = "Sucursales",
                    Visible = true,
                    IconClass = "fa fa-building",
                    ControllerName = "ShippingBranch",
                    ActionName = "Index",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });
            }
        }

        public override void Uninstall()
        {
            _context.Uninstall();
            base.Uninstall();
        }
    }
}
