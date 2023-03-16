using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Tasks;
using Nop.Core.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using System.Linq;
using Teed.Plugin.Api.Data;
using Teed.Plugin.Api.Security;

namespace Teed.Plugin.Api
{
    public class TeedApiPlugin : BasePlugin, IAdminMenuPlugin
    {
        private readonly ApiObjectContext _context;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly IRepository<ScheduleTask> _scheduleTaskRepository;

        public TeedApiPlugin(ApiObjectContext context,
            ISettingService settingService,
            IWebHelper webHelper,
            IPermissionService permissionService,
            IRepository<ScheduleTask> scheduleTaskRepository)
        {
            _context = context;
            _settingService = settingService;
            _webHelper = webHelper;
            _permissionService = permissionService;
            _scheduleTaskRepository = scheduleTaskRepository;

            _permissionService.InstallPermissions(new TeedApiPermissionProvider());
            //_context.Install();
        }

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/TeedApi/Configure";
        }

        public override void Install()
        {
            var taskExist = _scheduleTaskRepository.Table.Where(x => x.Type == "Teed.Plugin.Api.ScheduleTasks.SendNotificationsScheduleTask").Any();
            if (!taskExist)
            {
                var task = new ScheduleTask
                {
                    Name = "Send notifications",
                    Seconds = 60,
                    Type = "Teed.Plugin.Api.ScheduleTasks.SendNotificationsScheduleTask",
                    Enabled = true,
                    StopOnError = false,
                };
                _scheduleTaskRepository.Insert(task);
            }

            _context.Install();
            base.Install();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var currentElementRequiredPermissions = new TeedApiPermissionProvider();
            bool shouldCreate = currentElementRequiredPermissions.GetPermissions().Where(x => _permissionService.Authorize(x)).Any();

            if (shouldCreate)
            {
                var myPluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "TeedApi");
                if (myPluginNode == null)
                {
                    myPluginNode = new SiteMapNode()
                    {
                        SystemName = "TeedApi",
                        Title = "Teed Api",
                        Visible = true,
                        IconClass = "fa-mobile",
                    };
                    rootNode.ChildNodes.Add(myPluginNode);
                }

                if (_permissionService.Authorize(TeedApiPermissionProvider.Configure))
                    myPluginNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "TeedApi.Configuration",
                        Title = "Configuración",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "TeedApi",
                        ActionName = "Configure",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedApiPermissionProvider.Notifications))
                    myPluginNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "TeedApi.Notification",
                        Title = "Notificaciones",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "TeedApi",
                        ActionName = "NotificationList",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedApiPermissionProvider.PopupConfig))
                    myPluginNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "TeedApi.Popups",
                        Title = "Pop ups",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "PopupConfig",
                        ActionName = "List",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedApiPermissionProvider.TaggableBoxConfig))
                    myPluginNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "TeedApi.TaggableBoxes",
                        Title = "Cajas etiquetables",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "TaggableBoxConfig",
                        ActionName = "List",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedApiPermissionProvider.OnboardingConfig))
                    myPluginNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "TeedApi.Onboardings",
                        Title = "Pantallas onboarding",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "OnboardingConfig",
                        ActionName = "List",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });
            }
        }

        public override void Uninstall()
        {
            //_settingService.DeleteSetting<TeedApiPluginSettings>();
            _context.Uninstall();
            base.Uninstall();
        }
    }
}
