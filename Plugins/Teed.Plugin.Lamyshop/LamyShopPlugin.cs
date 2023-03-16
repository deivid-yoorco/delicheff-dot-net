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
using Teed.Plugin.LamyShop.Security;

namespace Teed.Plugin.LamyShop
{
    public class LamyShopPlugin : BasePlugin, IMiscPlugin, IWidgetPlugin //, IAdminMenuPlugin
    //, IWidgetPlugin
    {
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;
        private readonly IScheduleTaskService _scheduleTaskService;

        public LamyShopPlugin(IPermissionService permissionService,
            IWebHelper webHelper, IScheduleTaskService scheduleTaskService)
        {
            _permissionService = permissionService;
            _webHelper = webHelper;
            _scheduleTaskService = scheduleTaskService;

            _permissionService.InstallPermissions(new LamyShopPermissionProvider());
            //_context.Install();
        }

        public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            switch (widgetZone)
            {
                case "export_report_ventas":
                    viewComponentName = "ReportsByMonthButtons";
                    break;
                default:
                    viewComponentName = "";
                    break;
            }
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string> { "export_report_ventas" };
        }

        public override void Install()
        {
            //_context.Install();
            _permissionService.InstallPermissions(new LamyShopPermissionProvider());

            base.Install();
        }

        public override void Uninstall()
        {
            //_context.Uninstall();
            base.Uninstall();
        }
    }
}