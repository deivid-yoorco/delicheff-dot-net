using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Security;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Security;
using Nop.Services.Tasks;
using Nop.Web.Framework.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.SaleRecord.Data;
using Teed.Plugin.SaleRecord.Security;

namespace Teed.Plugin.SaleRecord
{
    public class SaleRecordPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin
    {
        private readonly PluginObjectContext _context;
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;
        private readonly IScheduleTaskService _scheduleTaskService;

        public SaleRecordPlugin(IPermissionService permissionService,
            IWebHelper webHelper, PluginObjectContext context,
            IScheduleTaskService scheduleTaskService)
        {
            _permissionService = permissionService;
            _webHelper = webHelper;
            _context = context;
            _scheduleTaskService = scheduleTaskService;

            _permissionService.InstallPermissions(new SaleRecordPermissionProvider());
            //_context.Install();
        }

        public override void Install()
        {
            //_context.Install();
            _permissionService.InstallPermissions(new SaleRecordPermissionProvider());

            base.Install();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var permissions = new SaleRecordPermissionProvider();
            var saleRecordNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "SaleRecord");

            #region Sale Record node

            if (_permissionService.Authorize(SaleRecordPermissionProvider.SaleRecords))
            {
                if (saleRecordNode == null)
                {
                    saleRecordNode = new SiteMapNode()
                    {
                        SystemName = "SaleRecords",
                        Title = "Registro de ventas",
                        Visible = true,
                        IconClass = "fa fa-credit-card",
                        ControllerName = "SaleRecord",
                        ActionName = "List",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    };
                    rootNode.ChildNodes.Add(saleRecordNode);
                }
            }

            #endregion
        }

        public override void Uninstall()
        {
            _context.Uninstall();
            base.Uninstall();
        }
    }
}
