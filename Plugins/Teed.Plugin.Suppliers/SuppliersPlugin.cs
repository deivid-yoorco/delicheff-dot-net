using Microsoft.AspNetCore.Routing;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using System.Linq;
using Teed.Plugin.Suppliers.Data;

namespace Teed.Plugin.Suppliers
{
    public class SuppliersPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin
    {
        private readonly PluginObjectContext _context;
        private readonly IPermissionService _permissionService;

        public SuppliersPlugin(IPermissionService permissionService, PluginObjectContext context)
        {
            _permissionService = permissionService;
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
            var myPluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "TeedSuppliers");
            if (myPluginNode == null)
            {
                myPluginNode = new SiteMapNode()
                {
                    SystemName = "TeedSuppliers",
                    Title = "Proveedores",
                    Visible = true,
                    IconClass = "fa fa-cart-plus",
                    ControllerName = "Supplier",
                    ActionName = "Index",
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
