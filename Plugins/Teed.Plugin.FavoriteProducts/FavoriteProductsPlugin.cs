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
using Teed.Plugin.FavoriteProducts.Security;

namespace Teed.Plugin.FavoriteProducts
{
    public class FavoriteProductsPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, IWidgetPlugin
        //, IWidgetPlugin
    {
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;
        private readonly IScheduleTaskService _scheduleTaskService;

        public FavoriteProductsPlugin(IPermissionService permissionService,
            IWebHelper webHelper, IScheduleTaskService scheduleTaskService)
        {
            _permissionService = permissionService;
            _webHelper = webHelper;
            _scheduleTaskService = scheduleTaskService;

            _permissionService.InstallPermissions(new FavoriteProductsPermissionProvider());
            //_context.Install();
        }

        public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            switch (widgetZone)
            {
                case "header_favorite_products":
                    viewComponentName = "FavoriteProductsNavBar";
                    break;
                case "mob_header_favorite_products":
                    viewComponentName = "FavoriteProductsNavBar";
                    break;
                default:
                    viewComponentName = "";
                    break;
            }
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string> { "header_favorite_products", "mob_header_favorite_products" };
        }

        public override void Install()
        {
            //_context.Install();
            _permissionService.InstallPermissions(new FavoriteProductsPermissionProvider());

            base.Install();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var permissions = new FavoriteProductsPermissionProvider();
            var favoriteProductsNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "FavoriteProducts");

            if (_permissionService.Authorize(FavoriteProductsPermissionProvider.Product))
            {
                if (favoriteProductsNode == null)
                {
                    favoriteProductsNode = new SiteMapNode()
                    {
                        SystemName = "FavoriteProducts",
                        Title = "Productos favoritos",
                        Visible = true,
                        IconClass = "fas fa-book",
                    };
                    rootNode.ChildNodes.Add(favoriteProductsNode);
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

                if (_permissionService.Authorize(FavoriteProductsPermissionProvider.Product))
                    favoriteProductsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "FavoriteProducts.Configure",
                        Title = "Configuración",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "FavoriteProductsConfigure",
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