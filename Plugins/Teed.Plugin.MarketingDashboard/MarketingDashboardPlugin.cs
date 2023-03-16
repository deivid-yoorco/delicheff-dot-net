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
using Teed.Plugin.MarketingDashboard.Data;
using Teed.Plugin.MarketingDashboard.Security;

namespace Teed.Plugin.MarketingDashboard
{
    public class MarketingDashboardPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin
    {
        private readonly PluginObjectContext _context;
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;
        private readonly IScheduleTaskService _scheduleTaskService;

        public MarketingDashboardPlugin(IPermissionService permissionService,
            IWebHelper webHelper, PluginObjectContext context,
            IScheduleTaskService scheduleTaskService)
        {
            _permissionService = permissionService;
            _webHelper = webHelper;
            _context = context;
            _scheduleTaskService = scheduleTaskService;

            _permissionService.InstallPermissions(new MarketingDashboardPermissionProvider());
            //_context.Install();
        }

        public override void Install()
        {
            _context.Install();
            _permissionService.InstallPermissions(new MarketingDashboardPermissionProvider());

            var scheduleTask1 = _scheduleTaskService.GetTaskByType("Teed.Plugin.MarketingDashboard.ScheduleTasks.BuyerScheduleTask");
            if (scheduleTask1 == null)
            {
                _scheduleTaskService.InsertTask(new Nop.Core.Domain.Tasks.ScheduleTask()
                {
                    Enabled = true,
                    Name = "Marketing Dashboard Data Schedule Task",
                    Seconds = 604800,
                    StopOnError = false,
                    Type = "Teed.Plugin.MarketingDashboard.ScheduleTasks.BuyerScheduleTask"
                });
            }

            base.Install();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var permissions = new MarketingDashboardPermissionProvider();
            var marketingExpensesNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "MarketingExpenses");

            #region Marketing expenses node

            if (_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
            {
                if (marketingExpensesNode == null)
                {
                    marketingExpensesNode = new SiteMapNode()
                    {
                        SystemName = "MarketingExpenses",
                        Title = "Gastos publicitarios",
                        Visible = true,
                        IconClass = "fa fa-credit-card",
                    };
                    rootNode.ChildNodes.Add(marketingExpensesNode);
                }

                marketingExpensesNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "MarketingExpenses.MarketingExpense",
                    Title = "Registro de gastos",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "MarketingExpense",
                    ActionName = "List",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });

                marketingExpensesNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "MarketingExpenses.MarketingExpenseType",
                    Title = "Catálogo de gastos",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "MarketingExpenseType",
                    ActionName = "List",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });

                marketingExpensesNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "MarketingExpenses.MarketingGrossMargin",
                    Title = "Registro de márgenes",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "MarketingGrossMargin",
                    ActionName = "List",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });

                marketingExpensesNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "MarketingExpenses.MarketingRetentionRate",
                    Title = "Registro de tasa de retención",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "MarketingRetentionRate",
                    ActionName = "List",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });

                marketingExpensesNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "MarketingExpenses.MarketingDashboard",
                    Title = "Indicadores semanales",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "MarketingDashboard",
                    ActionName = "DashboardData",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });

                marketingExpensesNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "MarketingExpenses.AutomaticDashboardData",
                    Title = "Registro de gastos automáticos",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "MarketingDashboard",
                    ActionName = "AutomaticDashboardData",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });

                marketingExpensesNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "MarketingExpenses.BreakdownDashboardData",
                    Title = "Deslgose de gastos",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "MarketingDashboard",
                    ActionName = "BreakdownDashboardData",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });
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
