using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Tasks;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Tasks;
using Nop.Web.Framework.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.CustomerComments.Data;
using Teed.Plugin.CustomerComments.Security;

namespace Teed.Plugin.CustomerComments
{
    public class CustomerCommentsPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        private readonly PluginObjectContext _context;
        private readonly IPermissionService _permissionService;

        public CustomerCommentsPlugin(IPermissionService permissionService,
            PluginObjectContext context)
        {
            _permissionService = permissionService;
            _context = context;
        }

        public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            switch (widgetZone)
            {
                case "customer-comments":
                    viewComponentName = "CustomerCommentsView";
                    break;
                default:
                    viewComponentName = "";
                    break;
            }
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string> { "customer-comments" };
        }

        public override void Install()
        {
            _context.Install();
            _permissionService.InstallPermissions(new TeedCustomerCommentsPermissionProvider());
            base.Install();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
        }

        public override void Uninstall()
        {
            _context.Uninstall();
            base.Uninstall();
        }
    }
}
