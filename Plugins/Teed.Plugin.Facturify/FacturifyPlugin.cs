using Microsoft.AspNetCore.Routing;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using System.Linq;
using Teed.Plugin.Facturify.Data;
using Teed.Plugin.Facturify.Security;

namespace Teed.Plugin.Facturify
{
    public class FacturifyPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin
    {
        private readonly PluginObjectContext _context;
        private readonly IPermissionService _permissionService;

        public FacturifyPlugin(IPermissionService permissionService, PluginObjectContext context)
        {
            _permissionService = permissionService;
            _context = context;

            _permissionService.InstallPermissions(new FacturifyPermissionProvider());
        }

        public override void Install()
        {
            _context.Install();
            base.Install();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            if (!_permissionService.Authorize(FacturifyPermissionProvider.Facturify)) return;
            var myPluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "TeedFacturify");
            if (myPluginNode == null)
            {
                myPluginNode = new SiteMapNode()
                {
                    SystemName = "TeedFacturify",
                    Title = "Facturación",
                    Visible = true,
                    IconClass = "fa fa-file-text-o",
                    ControllerName = "Facturify",
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
