using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Tasks;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Security;
using Nop.Services.Tasks;
using Nop.Web.Framework.Menu;
using System.Data;
using System.Linq;
using Teed.Plugin.Medical.Data;
using Teed.Plugin.Medical.Domain;
using Teed.Plugin.Medical.Security;

namespace Teed.Plugin.Medical
{
    public class TeedMedicalPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin
    {
        private readonly MedicalObjectContext _context;
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;
        private readonly IScheduleTaskService _scheduleTaskService;

        public TeedMedicalPlugin(IPermissionService permissionService, 
            IWebHelper webHelper,
            IScheduleTaskService scheduleTaskService,
            MedicalObjectContext context)
        {
            _context = context;
            _permissionService = permissionService;
            _webHelper = webHelper;
            _scheduleTaskService = scheduleTaskService;
        }

        public override void Install()
        {
            //_scheduleTaskService.InsertTask(new ScheduleTask()
            //{
            //    Seconds = 86400,
            //    Type = "Teed.Plugin.Medical.Services.PatientScheduleService",
            //    Name = "UpdatePatientSchedule",
            //    Enabled = true,
            //    StopOnError = false

            //});
            _context.Install();
            //_permissionService.InstallPermissions(new TeedMedicalPermissionProvider());
            base.Install();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical)) return;
            var myPluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "TeedMedical");
            if (myPluginNode == null)
            {
                myPluginNode = new SiteMapNode()
                {
                    SystemName = "TeedMedical",
                    Title = "Clínica",
                    Visible = true,
                    IconClass = "fa-user-md"
                };
                rootNode.ChildNodes.Add(myPluginNode);
            }

            myPluginNode.ChildNodes.Add(new SiteMapNode()
            {
                SystemName = "TeedMedical.Patients",
                Title = "Pacientes",
                ControllerName = "Patient",
                ActionName = "List",
                Visible = true,
                IconClass = "fa-dot-circle-o",
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
            });

            //myPluginNode.ChildNodes.Add(new SiteMapNode()
            //{
            //    SystemName = "TeedMedical.Prescriptions",
            //    Title = "Recetas",
            //    ControllerName = "Prescription",
            //    ActionName = "List",
            //    Visible = true,
            //    IconClass = "fa-dot-circle-o",
            //    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
            //});

            myPluginNode.ChildNodes.Add(new SiteMapNode()
            {
                SystemName = "TeedMedical.Appointments",
                Title = "Citas",
                ControllerName = "Appointment",
                ActionName = "List",
                Visible = true,
                IconClass = "fa-dot-circle-o",
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
            });

            myPluginNode.ChildNodes.Add(new SiteMapNode()
            {
                SystemName = "TeedMedical.Settings",
                Title = "Configuración de encargados",
                ControllerName = "Doctor",
                ActionName = "List",
                Visible = true,
                IconClass = "fa-dot-circle-o",
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
            });

            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedBranches)) return;
            var branchesPluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "TeedBranches");
            if (branchesPluginNode == null)
            {
                branchesPluginNode = new SiteMapNode()
                {
                    SystemName = "TeedBranches",
                    Title = "Sucursales",
                    Visible = true,
                    IconClass = "fa-building",
                };
                rootNode.ChildNodes.Add(branchesPluginNode);
            }
            branchesPluginNode.ChildNodes.Add(new SiteMapNode()
            {
                SystemName = "TeedBranches.Branches",
                Title = "Sucursales",
                Visible = true,
                IconClass = "fa-dot-circle-o",
                ControllerName = "Branch",
                ActionName = "List",
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
            });
            branchesPluginNode.ChildNodes.Add(new SiteMapNode()
            {
                SystemName = "TeedBranches.Offices",
                Title = "Consultorios",
                Visible = true,
                IconClass = "fa-dot-circle-o",
                ControllerName = "Office",
                ActionName = "List",
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
            });
            branchesPluginNode.ChildNodes.Add(new SiteMapNode()
            {
                SystemName = "TeedBranches.Holidays",
                Title = "Días Feriados",
                Visible = true,
                IconClass = "fa-dot-circle-o",
                ControllerName = "Holiday",
                ActionName = "List",
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
            });
        }

        public override void Uninstall()
        {
            //_context.Uninstall();
            //_permissionService.UninstallPermissions(new TeedMedicalPermissionProvider());
            base.Uninstall();
        }
    }
}