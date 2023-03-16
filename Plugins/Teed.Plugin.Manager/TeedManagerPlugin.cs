using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using System.Linq;
using Teed.Plugin.Manager.Data;
using Teed.Plugin.Medical.Security;

namespace Teed.Plugin.Manager
{
    public class TeedManagerPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin
    {
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly IWebHelper _webHelper;
        private readonly PluginObjectContext _context;

        public TeedManagerPlugin(IPermissionService permissionService,
            IWebHelper webHelper, ICustomerService customerService, PluginObjectContext context)
        {
            _permissionService = permissionService;
            _webHelper = webHelper;
            _context = context;
            _customerService = customerService;
        }

        public override void Install()
        {
            CreatePartnerRole();
            _context.Install();
            _permissionService.InstallPermissions(new TeedManagerPermissionProvider());
            base.Install();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var myPluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "TeedManager");
            if (_permissionService.Authorize(TeedManagerPermissionProvider.TeedManager))
            {
                if (myPluginNode == null)
                {
                    myPluginNode = CreateManagerBaseNode();
                    rootNode.ChildNodes.Add(myPluginNode);
                }
                myPluginNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Manager.Expenses",
                    Title = "Registro de gastos",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "Expenses",
                    ActionName = "List",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });
                myPluginNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Manager.ExpensesCategories",
                    Title = "Categorías de gastos",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "ExpensesCategories",
                    ActionName = "Index",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });
                myPluginNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Manager.CashExpenses",
                    Title = "Gastos en efectivo",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "CashExpenses",
                    ActionName = "List",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });
                myPluginNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Manager.StatementOfIncome",
                    Title = "P&L - Estado de resultados",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "StatementOfIncome",
                    ActionName = "Index",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });
            }

            if (_permissionService.Authorize(TeedManagerPermissionProvider.TeedPurchaseOrders))
            {
                if (myPluginNode == null)
                {
                    myPluginNode = CreateManagerBaseNode();
                    rootNode.ChildNodes.Add(myPluginNode);
                }
                myPluginNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Manager.PurchaseOrder",
                    Title = "Orden de compra",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "PurchaseOrder",
                    ActionName = "List",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });
            }


            if (_permissionService.Authorize(TeedManagerPermissionProvider.TeedPartnerLiabilities))
            {
                if (myPluginNode == null)
                {
                    myPluginNode = CreateManagerBaseNode();
                    rootNode.ChildNodes.Add(myPluginNode);
                }
                myPluginNode.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Manager.PartnerLiabilities",
                    Title = "Pasivos con socios",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "PartnerLiabilities",
                    ActionName = "List",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });
            }
        }

        private void CreatePartnerRole()
        {
            var role = _customerService.GetCustomerRoleBySystemName("Partner");
            if (role != null) return;
            role = new Nop.Core.Domain.Customers.CustomerRole()
            {
                Active = true,
                SystemName = "Partner",
                Name = "Socio"
            };
            _customerService.InsertCustomerRole(role);
        }

        private SiteMapNode CreateManagerBaseNode()
        {
            return new SiteMapNode()
            {
                SystemName = "TeedManager",
                Title = "Administración",
                Visible = true,
                IconClass = "fa fa-book",
            };
        }

        public override void Uninstall()
        {
            _context.Uninstall();
            base.Uninstall();
        }
    }
}