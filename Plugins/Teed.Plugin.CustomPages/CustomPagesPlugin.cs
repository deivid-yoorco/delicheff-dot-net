using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using System.Linq;
using Teed.Plugin.CustomPages.Data;
using Teed.Plugin.CustomPages.Security;

namespace Teed.Plugin.CustomPages
{
    public class CustomPagesPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin
    {
        private readonly PluginObjectContext _context;
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;

        public CustomPagesPlugin(IPermissionService permissionService,
            IWebHelper webHelper, PluginObjectContext context
            )
        {
            _permissionService = permissionService;
            _webHelper = webHelper;
            _context = context;

            _permissionService.InstallPermissions(new CustomPagesPermissionProvider());
        }

        public override void Install()
        {
            _context.Install();
            base.Install();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            if (!_permissionService.Authorize(CustomPagesPermissionProvider.CustomPages)) return;
            var myPluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "TeedCustomPages");
            if (myPluginNode == null)
            {
                myPluginNode = new SiteMapNode()
                {
                    SystemName = "TeedCustomPages",
                    Title = "Micrositios",
                    Visible = true,
                    IconClass = "fa fa-file-text-o",
                    ControllerName = "CustomPages",
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