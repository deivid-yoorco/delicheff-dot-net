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
using Teed.Plugin.TicketControl.Data;
using Teed.Plugin.TicketControl.Helpers;
using Teed.Plugin.TicketControl.Security;

namespace Teed.Plugin.TicketControl
{
    public class TicketControlPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        private readonly PluginObjectContext _context;
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IRepository<EmailAccount> _emailAccountRepository;

        private readonly string SCHEDULE_NAME = "Qr generation and email send";
        private readonly string SCHEDULE_TYPE = "Teed.Plugin.TicketControl.ScheduleTasks.QrProcessScheduleTask";

        public TicketControlPlugin(IPermissionService permissionService,
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
                case "ticket-view":
                    viewComponentName = "TicketView";
                    break;
                default:
                    viewComponentName = "";
                    break;
            }
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string> { "ticket-view" };
        }

        public override void Install()
        {
            var newTicketMessageTemplate = _messageTemplateService.GetMessageTemplateByName(EmailHelper.NEW_TICKET_TEMPLATE_NAME, 0);
            var body = $@"<p>{Environment.NewLine}
<a href=""%Store.URL%"">%Store.Name%</a>{Environment.NewLine}
<br />{Environment.NewLine}
<br />{Environment.NewLine}
Este es el QR de tu orden:{Environment.NewLine}</p>{Environment.NewLine}
<br />{Environment.NewLine}
<img src=""%Ticket.URL%"">{Environment.NewLine}
<br />{Environment.NewLine}
<br />{Environment.NewLine}
Productos de esta ordern:{Environment.NewLine}
<br />{Environment.NewLine}
%Order.Product(s)%{Environment.NewLine}
<br />{Environment.NewLine}";

            if (newTicketMessageTemplate == null)
            {
                _messageTemplateService.InsertMessageTemplate(new MessageTemplate()
                {
                    Name = EmailHelper.NEW_TICKET_TEMPLATE_NAME,
                    Subject = "%Store.Name%. Gracias por tu compra, aquí esta tu QR.",
                    Body = body,
                    IsActive = true,
                    EmailAccountId = _emailAccountRepository.Table.Select(x => x.Id).FirstOrDefault(),
                });
            }

            //install store open or close schedule task
            if (_scheduleTaskService.GetTaskByType(SCHEDULE_TYPE) == null)
            {
                _scheduleTaskService.InsertTask(new ScheduleTask
                {
                    Enabled = true,
                    Seconds = 600,
                    Name = SCHEDULE_NAME,
                    Type = SCHEDULE_TYPE,
                });
            }

            _context.Install();
            _permissionService.InstallPermissions(new TeedTicketControlPermissionProvider());
            base.Install();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins)) return;
            var myPluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "TicketControl");


            if (myPluginNode == null)
            {
                myPluginNode = new SiteMapNode()
                {
                    SystemName = "TicketControl",
                    Title = "Control de boletos",
                    Visible = true,
                    IconClass = "fa fa-ticket",
                };
                rootNode.ChildNodes.Add(myPluginNode);
            }

            if (_permissionService.Authorize(TeedTicketControlPermissionProvider.TicketControl))
            myPluginNode.ChildNodes.Add(new SiteMapNode()
            {
                SystemName = "TicketControl.Schedules",
                Title = "Horarios",
                Visible = true,
                IconClass = "fa-dot-circle-o",
                ControllerName = "Schedule",
                ActionName = "Index",
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
            });

            if (_permissionService.Authorize(TeedTicketControlPermissionProvider.TicketControl))
            myPluginNode.ChildNodes.Add(new SiteMapNode()
            {
                SystemName = "TicketControl.DatePersonalization",
                Title = "Bloqueo de fechas",
                Visible = true,
                IconClass = "fa-dot-circle-o",
                ControllerName = "DatePersonalization",
                ActionName = "Index",
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
