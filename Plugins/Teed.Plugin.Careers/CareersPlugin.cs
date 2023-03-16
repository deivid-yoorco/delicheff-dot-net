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
using Teed.Plugin.Careers.Data;
using Teed.Plugin.Careers.Security;

namespace Teed.Plugin.Careers
{
    public class CareersPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        private readonly PluginObjectContext _context;
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;
        private readonly IScheduleTaskService _scheduleTaskService;

        public CareersPlugin(IPermissionService permissionService,
            IWebHelper webHelper, PluginObjectContext context,
            IScheduleTaskService scheduleTaskService)
        {
            _permissionService = permissionService;
            _webHelper = webHelper;
            _context = context;
            _scheduleTaskService = scheduleTaskService;

            _permissionService.InstallPermissions(new CareersPermissionProvider());
            //_context.Install();
        }

        public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            switch (widgetZone)
            {
                case "footer-custom-1":
                    viewComponentName = "CareersFooter1";
                    break;
                case "footer-custom-2":
                    viewComponentName = "CareersFooter2";
                    break;
                case "footer-custom-3":
                    viewComponentName = "CareersFooter3";
                    break;
                case "mob_header_menu_after":
                    viewComponentName = "MobileMenu";
                    break;
                case "header_menu_after":
                    viewComponentName = "CareersHeader";
                    break;
                default:
                    viewComponentName = "";
                    break;
            }
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string> { "footer-custom-1", "footer-custom-2", "footer-custom-3", "mob_header_menu_after", "header_menu_after" };
        }

        public override void Install()
        {
            //_context.Install();
            _permissionService.InstallPermissions(new CareersPermissionProvider());

            base.Install();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var permissions = new CareersPermissionProvider();
            var careersNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Careers");

            if (_permissionService.Authorize(CareersPermissionProvider.Careers))
            {
                if (careersNode == null)
                {
                    careersNode = new SiteMapNode()
                    {
                        SystemName = "Careers",
                        Title = "Bolsa de trabajo",
                        Visible = true,
                        IconClass = "fas fa-book",
                    };
                    rootNode.ChildNodes.Add(careersNode);
                }

                if (_permissionService.Authorize(CareersPermissionProvider.Careers))
                    careersNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Careers.List",
                        Title = "Lista de postulaciones",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "Careers",
                        ActionName = "List",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(CareersPermissionProvider.Careers))
                    careersNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Careers.Configure",
                        Title = "Configuración",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "Careers",
                        ActionName = "Configure",
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