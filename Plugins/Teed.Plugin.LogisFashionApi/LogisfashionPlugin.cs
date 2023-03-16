using Microsoft.AspNetCore.Routing;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Logisfashion.Data;

namespace Teed.Plugin.Logisfashion
{
    public class LogisfashionPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin
    {
        private readonly IPermissionService _permissionService;
        private readonly PluginObjectContext _context;

        public LogisfashionPlugin(IPermissionService permissionService,
            PluginObjectContext context)
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
            var myPluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "TeedHamleys");
            if (myPluginNode == null)
            {
                myPluginNode = new SiteMapNode()
                {
                    SystemName = "TeedHamleys",
                    Title = "Hamleys",
                    Visible = true,
                    IconClass = "fa fa-file-text-o",
                };
                rootNode.ChildNodes.Add(myPluginNode);
            }
            myPluginNode.ChildNodes.Add(new SiteMapNode()
            {
                SystemName = "TeedLogisfashion",
                Title = "Logisfashion",
                Visible = true,
                IconClass = "fa-dot-circle-o",
                ControllerName = "Logisfashion",
                ActionName = "Index",
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
            });
            myPluginNode.ChildNodes.Add(new SiteMapNode()
            {
                SystemName = "TeedLogisfashionUpdate",
                Title = "Enviar productos",
                Visible = true,
                IconClass = "fa-dot-circle-o",
                ControllerName = "Logisfashion",
                ActionName = "UpdateProducts",
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
            });
            myPluginNode.ChildNodes.Add(new SiteMapNode()
            {
                SystemName = "TeedLogisfashionLog",
                Title = "Logs",
                Visible = true,
                IconClass = "fa-dot-circle-o",
                ControllerName = "Logisfashion",
                ActionName = "Logs",
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
            });
            myPluginNode.ChildNodes.Add(new SiteMapNode()
            {
                SystemName = "TeedLogisfashionExcelUpload",
                Title = "Enviar aviso de inventario",
                Visible = true,
                IconClass = "fa-dot-circle-o",
                ControllerName = "Logisfashion",
                ActionName = "ExcelUpload",
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
            });
        }

        public override void Uninstall()
        {
            _context.Uninstall();
            base.Uninstall();
        }
    }
}
