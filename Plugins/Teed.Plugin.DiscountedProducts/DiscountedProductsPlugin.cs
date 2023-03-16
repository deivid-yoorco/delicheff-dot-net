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
using Teed.Plugin.DiscountedProducts.Security;

namespace Teed.Plugin.DiscountedProducts
{
    public class DiscountedProductsPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, IWidgetPlugin
        //, IWidgetPlugin
    {
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;
        private readonly IScheduleTaskService _scheduleTaskService;

        public DiscountedProductsPlugin(IPermissionService permissionService,
            IWebHelper webHelper, IScheduleTaskService scheduleTaskService)
        {
            _permissionService = permissionService;
            _webHelper = webHelper;
            _scheduleTaskService = scheduleTaskService;

            _permissionService.InstallPermissions(new DiscountedProductsPermissionProvider());
            //_context.Install();
        }

        public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            switch (widgetZone)
            {
                case "header_discounted_Products":
                    viewComponentName = "DiscountedProductsNavBar";
                    break;
                case "mob_header_discounted_Products":
                    viewComponentName = "DiscountedProductsNavBar";
                    break;
                default:
                    viewComponentName = "";
                    break;
            }
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string> { "header_discounted_Products", "mob_header_discounted_Products" };
        }

        public override void Install()
        {
            //_context.Install();
            _permissionService.InstallPermissions(new DiscountedProductsPermissionProvider());

            base.Install();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var permissions = new DiscountedProductsPermissionProvider();
            var discountedProductsNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "DiscountedProducts");

            if (_permissionService.Authorize(DiscountedProductsPermissionProvider.Product))
            {
                if (discountedProductsNode == null)
                {
                    discountedProductsNode = new SiteMapNode()
                    {
                        SystemName = "TDiscountedProducts",
                        Title = "Productos con descuento",
                        Visible = true,
                        IconClass = "fas fa-book",
                    };
                    rootNode.ChildNodes.Add(discountedProductsNode);
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

                if (_permissionService.Authorize(DiscountedProductsPermissionProvider.Product))
                    discountedProductsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "DiscountedProducts.Configure",
                        Title = "Configuración",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "DiscountedProductsConfigure",
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