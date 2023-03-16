using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Security;
using Nop.Services.Tasks;
using Nop.Web.Framework.Menu;
using System.Collections.Generic;
using System.Linq;
using Teed.Plugin.RecentProducts.Security;

namespace Teed.Plugin.RecentProducts
{
    public class RecentProductsPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, IWidgetPlugin
        //, IWidgetPlugin
    {
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;
        private readonly IScheduleTaskService _scheduleTaskService;

        public RecentProductsPlugin(IPermissionService permissionService,
            IWebHelper webHelper, IScheduleTaskService scheduleTaskService)
        {
            _permissionService = permissionService;
            _webHelper = webHelper;
            _scheduleTaskService = scheduleTaskService;

            _permissionService.InstallPermissions(new RecentProductsPermissionProvider());
            //_context.Install();
        }

        public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            switch (widgetZone)
            {
                case "header_menu_before":
                    viewComponentName = "RecentProductsNavBar";
                    break;
                case "mob_header_menu_before":
                    viewComponentName = "RecentProductsNavBar";
                    break;
                default:
                    viewComponentName = "";
                    break;
            }
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string> { "header_menu_before", "mob_header_menu_before" };
        }

        public override void Install()
        {
            //_context.Install();
            _permissionService.InstallPermissions(new RecentProductsPermissionProvider());

            base.Install();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var permissions = new RecentProductsPermissionProvider();
            var recentProductsNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "RecentProducts");

            if (_permissionService.Authorize(RecentProductsPermissionProvider.Recent))
            {
                if (recentProductsNode == null)
                {
                    recentProductsNode = new SiteMapNode()
                    {
                        SystemName = "RecentProducts",
                        Title = "Productos recientes",
                        Visible = true,
                        IconClass = "fas fa-book",
                    };
                    rootNode.ChildNodes.Add(recentProductsNode);
                }

                //if (_permissionService.Authorize(RecentProductsPermissionProvider.Recent))
                //    RecentProductsNode.ChildNodes.Add(new SiteMapNode()
                //    {
                //        SystemName = "Careers.List",
                //        Title = "Lista de postulaciones",
                //        Visible = true,
                //        IconClass = "fa-dot-circle-o",
                //        ControllerName = "Careers",
                //        ActionName = "List",
                //        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                //    });

                if (_permissionService.Authorize(RecentProductsPermissionProvider.Recent))
                    recentProductsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "RecentProducts.Configure",
                        Title = "Configuración",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "RecentProductsConfigure",
                        ActionName = "Configure",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });
            }
        }

        public override void Uninstall()
        {
            //_context.Uninstall();
            base.Uninstall();
        }
    }
}