using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using System.Collections.Generic;
using System.Linq;
using Teed.Plugin.CornerForm.Data;
using Teed.Plugin.CornerForm.Security;

namespace Teed.Plugin.CornerForm
{
    public class CornerFormPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        private readonly PluginObjectContext _context;
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;

        public CornerFormPlugin(IPermissionService permissionService, PluginObjectContext context, IWebHelper webHelper)
        {
            _permissionService = permissionService;
            _context = context;
            _webHelper = webHelper;

            _permissionService.InstallPermissions(new CornerFormPermissionProvider());
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/CornerForm/Configure";
        }

        public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            viewComponentName = "TeedCornerForm";
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string> { "footer" };
        }

        public override void Install()
        {
            _context.Install();
            base.Install();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            if (!_permissionService.Authorize(CornerFormPermissionProvider.CornerForm)) return;
            var myPluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "TeedCornerForm");
            if (myPluginNode == null)
            {
                myPluginNode = new SiteMapNode()
                {
                    SystemName = "TeedCornerForm",
                    Title = "Formulario",
                    Visible = true,
                    IconClass = "fa-list-ul"
                };
                rootNode.ChildNodes.Add(myPluginNode);
            }

            myPluginNode.ChildNodes.Add(new SiteMapNode()
            {
                SystemName = "TeedCornerForm.Configure",
                Title = "Configurar",
                Visible = true,
                IconClass = "fa-dot-circle-o",
                ControllerName = "CornerForm",
                ActionName = "Configure",
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
            });

            myPluginNode.ChildNodes.Add(new SiteMapNode()
            {
                SystemName = "TeedCornerForm.Answers",
                Title = "Registro de respuestas",
                Visible = true,
                IconClass = "fa-dot-circle-o",
                ControllerName = "CornerForm",
                ActionName = "List",
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
