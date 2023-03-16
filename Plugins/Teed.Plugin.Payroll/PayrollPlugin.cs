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
using Teed.Plugin.Payroll.Data;
using Teed.Plugin.Payroll.Security;

namespace Teed.Plugin.Payroll
{
    public class PayrollPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        private readonly PluginObjectContext _context;
        private readonly IPermissionService _permissionService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IWorkContext _workContext;

        public PayrollPlugin(IPermissionService permissionService, PluginObjectContext context,
            IScheduleTaskService scheduleTaskService, IWorkContext workContext)
        {
            _permissionService = permissionService;
            _context = context;
            _scheduleTaskService = scheduleTaskService;
            _workContext = workContext;

            _permissionService.InstallPermissions(new TeedPayrollPermissionProvider());
        }
        public IList<string> GetWidgetZones()
        {
            return new List<string> { 
                "employee_number_editor",
            };
        }

        public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            switch (widgetZone)
            {
                case "employee_number_editor":
                    viewComponentName = "EmployeeNumberEditor";
                    break;
                default:
                    viewComponentName = "";
                    break;
            }
        }

        public override void Install()
        {
            _context.Install();
            //_permissionService.InstallPermissions(new TeedPayrollPermissionProvider());

            var scheduleTask1 = _scheduleTaskService.GetTaskByType("Teed.Plugin.Payroll.ScheduleTasks.IncidentsRegisterTask");
            if (scheduleTask1 == null)
            {
                _scheduleTaskService.InsertTask(new Nop.Core.Domain.Tasks.ScheduleTask()
                {
                    Enabled = false,
                    Name = "Register Incidents from outer Database Task",
                    Seconds = 86400,
                    StopOnError = false,
                    Type = "Teed.Plugin.Payroll.ScheduleTasks.IncidentsRegisterTask"
                });
            }
            var scheduleTask2 = _scheduleTaskService.GetTaskByType("Teed.Plugin.Payroll.ScheduleTasks.BonusApplicationTask");
            if (scheduleTask2 == null)
            {
                _scheduleTaskService.InsertTask(new Nop.Core.Domain.Tasks.ScheduleTask()
                {
                    Enabled = false,
                    Name = "Register automatic bonus applications Task",
                    Seconds = 3600,
                    StopOnError = false,
                    Type = "Teed.Plugin.Payroll.ScheduleTasks.BonusApplicationTask"
                });
            }

            base.Install();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var permissions = new TeedPayrollPermissionProvider();
            bool shouldContinue = false;
            foreach (var item in permissions.GetPermissions())
            {
                if (_permissionService.Authorize(item))
                {
                    shouldContinue = true;
                    break;
                }
            }

            var hasFranchiseRole = _workContext.CurrentCustomer.CustomerRoles.Where(x => x.SystemName?.ToLower() == "franchise").Any();

            if (!shouldContinue || (shouldContinue && hasFranchiseRole)) return;

            var myPluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "TeedPayroll");
            if (myPluginNode == null)
            {
                myPluginNode = new SiteMapNode()
                {
                    SystemName = "TeedPayroll",
                    Title = "Nómina",
                    Visible = true,
                    IconClass = "fa fa-users"
                };
                rootNode.ChildNodes.Add(myPluginNode);
            }

            if (_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollAlerts))
                myPluginNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Payroll.PayrollAlerts",
                    Title = "Alertas de nómina",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "PayrollAlerts",
                    ActionName = "Index",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });

            if (_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                myPluginNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Payroll.PayrollEmployee",
                    Title = "Expediente de empleados",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "PayrollEmployee",
                    ActionName = "List",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });

            if (_permissionService.Authorize(TeedPayrollPermissionProvider.JobCatalog))
                myPluginNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Payroll.JobCatalog",
                    Title = "Catálogo de puestos",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "JobCatalog",
                    ActionName = "Index",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });

            if (_permissionService.Authorize(TeedPayrollPermissionProvider.Assistance))
                myPluginNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Payroll.Assistance",
                    Title = "Bitácora de asistencia",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "Assistance",
                    ActionName = "List",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });

            if (_permissionService.Authorize(TeedPayrollPermissionProvider.Incident))
                myPluginNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Payroll.Incident",
                    Title = "Reporte de incidencias",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "Incident",
                    ActionName = "Index",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });

            if (_permissionService.Authorize(TeedPayrollPermissionProvider.BiweeklyPayment))
                myPluginNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Payroll.BiweeklyPayment",
                    Title = "Nómina quincenal",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "BiweeklyPayment",
                    ActionName = "Index",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });

            if (_permissionService.Authorize(TeedPayrollPermissionProvider.Bonuses))
                myPluginNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Payroll.Bonus",
                    Title = "Bonos",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "Bonus",
                    ActionName = "Index",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });

            if (_permissionService.Authorize(TeedPayrollPermissionProvider.MinimumWagesCatalog))
                myPluginNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Payroll.MinimumWagesCatalog",
                    Title = "Catálogo de salarios mínimos",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "MinimumWagesCatalog",
                    ActionName = "List",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });

            if (_permissionService.Authorize(TeedPayrollPermissionProvider.OperationalIncident))
                myPluginNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Payroll.OperationalIncidentsDelivery",
                    Title = "Incidencias operativas - Reparto",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "OperationalIncident",
                    ActionName = "Deliveries",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });

            if (_permissionService.Authorize(TeedPayrollPermissionProvider.OperationalIncident))
                myPluginNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Payroll.OperationalIncidentsBuyer",
                    Title = "Incidencias operativas - Compras",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "OperationalIncident",
                    ActionName = "Buyers",
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
