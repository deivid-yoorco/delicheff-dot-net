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
using Teed.Plugin.MessageBird.Data;
using Teed.Plugin.MessageBird.Security;

namespace Teed.Plugin.MessageBird
{
    public class MessageBirdPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        private readonly PluginObjectContext _context;
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IRepository<EmailAccount> _emailAccountRepository;

        public MessageBirdPlugin(IPermissionService permissionService,
            IWebHelper webHelper, PluginObjectContext context,
            IScheduleTaskService scheduleTaskService,
            IMessageTemplateService messageTemplateService,
            IRepository<EmailAccount> emailAccountRepository)
        {
            _permissionService = permissionService;
            _webHelper = webHelper;
            _context = context;
            _scheduleTaskService = scheduleTaskService;
            _messageTemplateService = messageTemplateService;
            _emailAccountRepository = emailAccountRepository;
        }

        public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            switch (widgetZone)
            {
                case "whatsapp-button":
                    viewComponentName = "WhatsAppButton";
                    break;
                case "whatsapp-tab-logs":
                    viewComponentName = "WhatsAppTabLogs";
                    break;
                case "whatsapp-kendo-script":
                    viewComponentName = "WhatsAppKendoScript";
                    break;
                default:
                    viewComponentName = "";
                    break;
            }
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string> { "whatsapp-button", "whatsapp-tab-logs", "whatsapp-kendo-script" };
        }

        public override void Install()
        {
            _context.Install();
            _permissionService.InstallPermissions(new TeedMessageBirdPermissionProvider());
            base.Install();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            if (!_permissionService.Authorize(TeedMessageBirdPermissionProvider.MessageBirdAdmin)) return;
            var myPluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "MessageBird");

            if (myPluginNode == null)
            {
                myPluginNode = new SiteMapNode()
                {
                    SystemName = "MessageBird",
                    Title = "Control de MessageBird",
                    Visible = true,
                    IconClass = "fa fa-comments",
                };
                rootNode.ChildNodes.Add(myPluginNode);
            }

            if (_permissionService.Authorize(TeedMessageBirdPermissionProvider.MessageBirdAdmin))
                myPluginNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "MessageBird.Configure",
                    Title = "Configuración",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "MessageBird",
                    ActionName = "Configure",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });

            if (_permissionService.Authorize(TeedMessageBirdPermissionProvider.MessageBirdAdmin))
                myPluginNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "MessageBird.Log",
                    Title = "Logs",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "MessageBird",
                    ActionName = "Log",
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
