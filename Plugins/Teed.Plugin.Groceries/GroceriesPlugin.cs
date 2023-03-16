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
using Teed.Plugin.Groceries.Data;
using Teed.Plugin.Groceries.Security;

namespace Teed.Plugin.Groceries
{
    public class GroceriesPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        private readonly PluginObjectContext _context;
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IWorkContext _workContext;

        public GroceriesPlugin(IPermissionService permissionService,
            IWebHelper webHelper, PluginObjectContext context,
            IScheduleTaskService scheduleTaskService,
            IWorkContext workContext)
        {
            _permissionService = permissionService;
            _webHelper = webHelper;
            _context = context;
            _scheduleTaskService = scheduleTaskService;
            _workContext = workContext;

            _permissionService.InstallPermissions(new TeedGroceriesPermissionProvider());

            //_context.Install();
        }

        //public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        //{
        //    viewComponentName = "BuyerBar";
        //}

        public IList<string> GetWidgetZones()
        {
            return new List<string> { "admin_links",
                "notdelivered-button",
                "order-option-buttons",
                "main-manufacturer",
                "manufacturer-default-buyer-log",
                "order-alerts",
                "cancel-order-button",
                "admin_customer_details_orders_top",
                "export_orders_for_day_excel",
                "admin_order_details_products_top",
                "News_Letter_Button",
                "route_franchise",
                "product_margin_calculation",
                "admin_order_details_info_top",
                "limited_customer_roles",
                "buyer-payment-tab-logs",
                "manufacturer-bank-account",
                "order-mark-unpaid",
                "corcel-customer-check"
            };
        }

        public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            switch (widgetZone)
            {
                case "admin_links":
                    viewComponentName = "BuyerBar";
                    break;
                case "notdelivered-button":
                    viewComponentName = "NotDeliveredButton";
                    break;
                case "order-option-buttons":
                    viewComponentName = "OrderOptionButtons";
                    break;
                case "main-manufacturer":
                    viewComponentName = "MainManufacturer";
                    break;
                case "manufacturer-default-buyer-log":
                    viewComponentName = "ManufacturerBuyerLog";
                    break;
                case "order-alerts":
                    viewComponentName = "OrderAlerts";
                    break;
                case "cancel-order-button":
                    viewComponentName = "CancelOrderButton";
                    break;
                case "admin_customer_details_orders_top":
                    viewComponentName = "CustomerData";
                    break;
                case "export_orders_for_day_excel":
                    viewComponentName = "ExportExcelOrdersForDay";
                    break;
                case "admin_order_details_products_top":
                    viewComponentName = "NotDeliveredProductButton";
                    break;
                case "News_Letter_Button":
                    viewComponentName = "NewsLetterButton";
                    break;
                case "route_franchise":
                    viewComponentName = "RouteFranchise";
                    break;
                case "product_margin_calculation":
                    viewComponentName = "ProductMarginCalculation";
                    break;
                case "admin_order_details_info_top":
                    viewComponentName = "OrderSuspiciousCheck";
                    break;
                case "limited_customer_roles":
                    viewComponentName = "LimitedCustomerRoles";
                    break;
                case "buyer-payment-tab-logs":
                    viewComponentName = "BuyerPaymentLog";
                    break;
                case "manufacturer-bank-account":
                    viewComponentName = "ManufacturerBankAccount";
                    break;
                case "order-mark-unpaid":
                    viewComponentName = "OrderMarkUnpaidButton";
                    break;
                case "corcel-customer-check":
                    viewComponentName = "CorcelCustomerCheck";
                    break;
                default:
                    viewComponentName = "";
                    break;
            }
        }

        public override void Install()
        {
            _context.Install();
            _permissionService.InstallPermissions(new TeedGroceriesPermissionProvider());

            var scheduleTask1 = _scheduleTaskService.GetTaskByType("Teed.Plugin.Groceries.ScheduleTasks.GrowthHackingScheduleTask");
            if (scheduleTask1 == null)
            {
                _scheduleTaskService.InsertTask(new Nop.Core.Domain.Tasks.ScheduleTask()
                {
                    Enabled = true,
                    Name = "Growth Hacking Verification Schedule Task",
                    Seconds = 86400,
                    StopOnError = false,
                    Type = "Teed.Plugin.Groceries.ScheduleTasks.GrowthHackingScheduleTask"
                });
            }

            var scheduleTask2 = _scheduleTaskService.GetTaskByType("Teed.Plugin.Groceries.ScheduleTasks.GrowthHackingRewardScheduleTask");
            if (scheduleTask2 == null)
            {
                _scheduleTaskService.InsertTask(new Nop.Core.Domain.Tasks.ScheduleTask()
                {
                    Enabled = true,
                    Name = "Growth Hacking Reward Verification Schedule Task",
                    Seconds = 86400,
                    StopOnError = false,
                    Type = "Teed.Plugin.Groceries.ScheduleTasks.GrowthHackingRewardScheduleTask"
                });
            }

            var scheduleTask3 = _scheduleTaskService.GetTaskByType("Teed.Plugin.Groceries.ScheduleTasks.IncidentsCheckScheduleTask");
            if (scheduleTask3 == null)
            {
                _scheduleTaskService.InsertTask(new Nop.Core.Domain.Tasks.ScheduleTask()
                {
                    Enabled = true,
                    Name = "Incidents Check Schedule Task",
                    Seconds = 86400,
                    StopOnError = false,
                    Type = "Teed.Plugin.Groceries.ScheduleTasks.IncidentsCheckScheduleTask"
                });
            }

            var scheduleTask4 = _scheduleTaskService.GetTaskByType("Teed.Plugin.Groceries.ScheduleTasks.CompleteOrdersScheduleTask");
            if (scheduleTask4 == null)
            {
                _scheduleTaskService.InsertTask(new Nop.Core.Domain.Tasks.ScheduleTask()
                {
                    Enabled = true,
                    Name = "Complete Orders Schedule Task",
                    Seconds = 1200,
                    StopOnError = false,
                    Type = "Teed.Plugin.Groceries.ScheduleTasks.CompleteOrdersScheduleTask"
                });
            }

            var scheduleTask5 = _scheduleTaskService.GetTaskByType("Teed.Plugin.Groceries.ScheduleTasks.FranchiseBonusScheduleTask");
            if (scheduleTask5 == null)
            {
                _scheduleTaskService.InsertTask(new Nop.Core.Domain.Tasks.ScheduleTask()
                {
                    Enabled = true,
                    Name = "Bonus Check Schedule Task",
                    Seconds = 86400,
                    StopOnError = false,
                    Type = "Teed.Plugin.Groceries.ScheduleTasks.FranchiseBonusScheduleTask"
                });
            }

            var scheduleTask6 = _scheduleTaskService.GetTaskByType("Teed.Plugin.Groceries.ScheduleTasks.FranchiseMonthlyChargeScheduleTask");
            if (scheduleTask6 == null)
            {
                _scheduleTaskService.InsertTask(new Nop.Core.Domain.Tasks.ScheduleTask()
                {
                    Enabled = true,
                    Name = "Franchise Monthly Charge Schedule Task",
                    Seconds = 86400,
                    StopOnError = false,
                    Type = "Teed.Plugin.Groceries.ScheduleTasks.FranchiseMonthlyChargeScheduleTask"
                });
            }

            var scheduleTask7 = _scheduleTaskService.GetTaskByType("Teed.Plugin.Groceries.ScheduleTasks.BuyerScheduleTask");
            if (scheduleTask7 == null)
            {
                _scheduleTaskService.InsertTask(new Nop.Core.Domain.Tasks.ScheduleTask()
                {
                    Enabled = true,
                    Name = "Buyers Schedule Task",
                    Seconds = 3600,
                    StopOnError = false,
                    Type = "Teed.Plugin.Groceries.ScheduleTasks.BuyerScheduleTask"
                });
            }

            var scheduleTask8 = _scheduleTaskService.GetTaskByType("Teed.Plugin.Groceries.ScheduleTasks.FranchiseAutomaticBillingScheduleTask");
            if (scheduleTask8 == null)
            {
                _scheduleTaskService.InsertTask(new Nop.Core.Domain.Tasks.ScheduleTask()
                {
                    Enabled = true,
                    Name = "Franchise Automatic Billing Schedule Task",
                    Seconds = 86400,
                    StopOnError = false,
                    Type = "Teed.Plugin.Groceries.ScheduleTasks.FranchiseAutomaticBillingScheduleTask"
                });
            }

            var scheduleTask9 = _scheduleTaskService.GetTaskByType("Teed.Plugin.Groceries.ScheduleTasks.RelatedProductGeneratedScheduleTask");
            if (scheduleTask9 == null)
            {
                _scheduleTaskService.InsertTask(new Nop.Core.Domain.Tasks.ScheduleTask()
                {
                    Enabled = true,
                    Name = "Related Products Schedule Task",
                    Seconds = 86400,
                    StopOnError = false,
                    Type = "Teed.Plugin.Groceries.ScheduleTasks.RelatedProductGeneratedScheduleTask"
                });
            }

            var scheduleTask10 = _scheduleTaskService.GetTaskByType("Teed.Plugin.Groceries.ScheduleTasks.UrbanPromoterFeesInsertScheduleTask");
            if (scheduleTask10 == null)
            {
                _scheduleTaskService.InsertTask(new Nop.Core.Domain.Tasks.ScheduleTask()
                {
                    Enabled = true,
                    Name = "Urban Promoter Fees Insertion Schedule Task",
                    Seconds = 86400,
                    StopOnError = false,
                    Type = "Teed.Plugin.Groceries.ScheduleTasks.UrbanPromoterFeesInsertScheduleTask"
                });
            }

            var scheduleTask11 = _scheduleTaskService.GetTaskByType("Teed.Plugin.Groceries.ScheduleTasks.RecurrenceReminderTask");
            if (scheduleTask11 == null)
            {
                _scheduleTaskService.InsertTask(new Nop.Core.Domain.Tasks.ScheduleTask()
                {
                    Enabled = true,
                    Name = "Recurrence Reminder Email Send Task",
                    Seconds = 86400,
                    StopOnError = false,
                    Type = "Teed.Plugin.Groceries.ScheduleTasks.RecurrenceReminderTask"
                });
            }

            var scheduleTask12 = _scheduleTaskService.GetTaskByType("Teed.Plugin.Groceries.ScheduleTasks.CorcelConvertScheduleTask");
            if (scheduleTask12 == null)
            {
                _scheduleTaskService.InsertTask(new Nop.Core.Domain.Tasks.ScheduleTask()
                {
                    Enabled = true,
                    Name = "Insert new CORCEL detected customers",
                    Seconds = 86400,
                    StopOnError = false,
                    Type = "Teed.Plugin.Groceries.ScheduleTasks.CorcelConvertScheduleTask"
                });
            }

            var scheduleTask13 = _scheduleTaskService.GetTaskByType("Teed.Plugin.Groceries.ScheduleTasks.InsertPrintedCouponBooksScheduleTask");
            if (scheduleTask13 == null)
            {
                _scheduleTaskService.InsertTask(new Nop.Core.Domain.Tasks.ScheduleTask()
                {
                    Enabled = true,
                    Name = "Insert printed coupon books into participating orders",
                    Seconds = 300,
                    StopOnError = false,
                    Type = "Teed.Plugin.Groceries.ScheduleTasks.InsertPrintedCouponBooksScheduleTask"
                });
            }

            base.Install();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var scheduleTask13 = _scheduleTaskService.GetTaskByType("Teed.Plugin.Groceries.ScheduleTasks.InsertPrintedCouponBooksScheduleTask");
            if (scheduleTask13 == null)
            {
                _scheduleTaskService.InsertTask(new Nop.Core.Domain.Tasks.ScheduleTask()
                {
                    Enabled = true,
                    Name = "Insert printed coupon books into participating orders",
                    Seconds = 300,
                    StopOnError = false,
                    Type = "Teed.Plugin.Groceries.ScheduleTasks.InsertPrintedCouponBooksScheduleTask"
                });
            }

            var permissions = new TeedGroceriesPermissionProvider();

            var hasFranchiseRole = _workContext.CurrentCustomer.CustomerRoles.Where(x => x.SystemName?.ToLower() == "franchise").Any();

            var buyersNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Groceries");
            var logisticsNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Groceries");
            var dashboardNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Groceries");
            var pricesNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Groceries");
            var printsNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Groceries");
            var corcelsNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Groceries");

            //var myPluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Groceries");
            var myPluginNode2 = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Groceries");
            var myPluginNode3 = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Groceries");
            var myPluginNode4 = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Groceries");

            #region Buyers node

            var currentElementRequiredPermissions = new List<PermissionRecord>()
            {
                TeedGroceriesPermissionProvider.OrderItemBuyer,
                TeedGroceriesPermissionProvider.OrderDeliveryReports,
                TeedGroceriesPermissionProvider.ManufacturerBuyer,
                TeedGroceriesPermissionProvider.AssignSubstituteBuyers,
                TeedGroceriesPermissionProvider.BuyerPayment,
            };

            bool shouldCreate = currentElementRequiredPermissions.Where(x => _permissionService.Authorize(x)).Any();
            if (shouldCreate)
            {
                if (buyersNode == null)
                {
                    buyersNode = new SiteMapNode()
                    {
                        SystemName = "Groceries",
                        Title = "Compras",
                        Visible = true,
                        IconClass = "fa fa-shopping-basket",
                    };
                    rootNode.ChildNodes.Add(buyersNode);
                }

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderItemBuyer))
                    buyersNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.OrderItemBuyer",
                        Title = "Asignar compradores del día",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "OrderItemBuyer",
                        ActionName = "List",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderDeliveryReports))
                    buyersNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.OrderDeliveryReports",
                        Title = "Liquidación de compradores",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "OrderDeliveryReports",
                        ActionName = "DeliveryDateList",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.ManufacturerBuyer))
                    buyersNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.ManufacturerBuyer",
                        Title = "Asignar compradores default",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "ManufacturerBuyer",
                        ActionName = "Configure"
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.AssignSubstituteBuyers))
                    buyersNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.AssignBuyers",
                        Title = "Asignar compradores suplentes",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "ShippingRouteUser",
                        ActionName = "ListDatesAssignBuyers",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.BuyerPayment))
                    buyersNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.BuyerPayment",
                        Title = "Tablero de pagos",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "BuyerPayment",
                        ActionName = "List",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.CorporateCard))
                    buyersNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.CorporateCard",
                        Title = "Catálogo de tarjetas corporativas",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "CorporateCard",
                        ActionName = "Index",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.CorporateCard))
                    buyersNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.CorporateCardList",
                        Title = "Dispersión de fondos tarjetas corportivas",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "CorporateCard",
                        ActionName = "List",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });
            }

            #endregion

            #region Logistics node

            currentElementRequiredPermissions = new List<PermissionRecord>()
            {
                TeedGroceriesPermissionProvider.ShippingRouteAssign,
                TeedGroceriesPermissionProvider.ShippingRouteUser,
                TeedGroceriesPermissionProvider.ShippingVehicles,
                TeedGroceriesPermissionProvider.ShippingRouteMonitor,
                TeedGroceriesPermissionProvider.RouteRescue,
                TeedGroceriesPermissionProvider.ShippingArea,
                TeedGroceriesPermissionProvider.ShippingZone,
                TeedGroceriesPermissionProvider.ShippingRegion,
                TeedGroceriesPermissionProvider.ShippingAreaPostalCodes,
                TeedGroceriesPermissionProvider.ShippingVehicles,
                TeedGroceriesPermissionProvider.OrderType,
                TeedGroceriesPermissionProvider.ShippingRoute,
                TeedGroceriesPermissionProvider.OptimizationRequest,
                TeedGroceriesPermissionProvider.OptimizationRequestManager,
                TeedGroceriesPermissionProvider.FinalRouteReport,
            };
            shouldCreate = currentElementRequiredPermissions.Where(x => _permissionService.Authorize(x)).Any();

            if (shouldCreate && !hasFranchiseRole)
            {
                if (logisticsNode == null)
                {
                    logisticsNode = new SiteMapNode()
                    {
                        SystemName = "Groceries",
                        Title = "Logística",
                        Visible = true,
                        IconClass = "fa fa-truck",
                    };
                    rootNode.ChildNodes.Add(logisticsNode);
                }

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteAssign))
                    logisticsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.ShippingRouteOrder",
                        Title = "Asignar rutas",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "ShippingRoute",
                        ActionName = "AssignRouteOrderList",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteUser))
                    logisticsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.ShippingRouteUser",
                        Title = "Asignar repartidores",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "ShippingRouteUser",
                        ActionName = "List",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingVehicles))
                    logisticsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.ShippingVehicleRoute",
                        Title = "Asignar vehículos",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "ShippingVehicle",
                        ActionName = "VehicleRouteList",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteMonitor))
                    logisticsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.MonitorRoutesList",
                        Title = "Monitoreo de rutas",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "ShippingRoute",
                        ActionName = "MonitorRoutesList",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.RouteRescue))
                    logisticsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.RouteRescue",
                        Title = "Rescate de rutas",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "RouteRescue",
                        ActionName = "List"
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingArea))
                    logisticsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.ShippingArea",
                        Title = "Cobertura - Códigos postales",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "ShippingArea",
                        ActionName = "Configure",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingZone))
                    logisticsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.ShippingZone",
                        Title = "Cobertura - Zonas",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "ShippingZone",
                        ActionName = "List",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRegion))
                    logisticsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.ShippingRegion",
                        Title = "Cobertura - Regiones",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "ShippingRegion",
                        ActionName = "List",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingAreaPostalCodes))
                    logisticsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.PostalCodeSearch",
                        Title = "CPs buscados por clientes",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "ShippingArea",
                        ActionName = "InvalidPostalCodes",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingVehicles))
                    logisticsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.ShippingVehicles",
                        Title = "Vehículos registrados",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "ShippingVehicle",
                        ActionName = "List",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderType))
                    logisticsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.OrderType",
                        Title = "Tipos de pedido",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "OrderType",
                        ActionName = "List",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRoute))
                    logisticsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.ShippingRoute",
                        Title = "Rutas activas",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "ShippingRoute",
                        ActionName = "List",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.OptimizationRequest) || _permissionService.Authorize(TeedGroceriesPermissionProvider.OptimizationRequestManager))
                    logisticsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.OptimizationRequest",
                        Title = "Solicitudes de optimización",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "OrderOptimizationRequest",
                        ActionName = "Index",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.FinalRouteReport))
                    logisticsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.FinalRouteReport",
                        Title = "Liquidación de reparto",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "FinalRouteReport",
                        ActionName = "List",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                // Festive dates
                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.FestiveDates))
                    logisticsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.FestiveDates",
                        Title = "Fechas festivas",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "FestiveDate",
                        ActionName = "List",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });
            }

            #endregion

            #region Dashboard node

            currentElementRequiredPermissions = new List<PermissionRecord>()
            {
                TeedGroceriesPermissionProvider.ShippingVehicles,
                TeedGroceriesPermissionProvider.Warnings,
                TeedGroceriesPermissionProvider.OperationalErrors,
                TeedGroceriesPermissionProvider.HeatMap,
                TeedGroceriesPermissionProvider.DiscountsBreakdown,
                TeedGroceriesPermissionProvider.DashboardHq,
                TeedGroceriesPermissionProvider.DashboardHqConfig,
                StandardPermissionProvider.ManageOrders
            };
            shouldCreate = currentElementRequiredPermissions.Where(x => _permissionService.Authorize(x)).Any();

            if (shouldCreate && !hasFranchiseRole)
            {
                if (dashboardNode == null)
                {
                    dashboardNode = new SiteMapNode()
                    {
                        SystemName = "Groceries",
                        Title = "Dashboards",
                        Visible = true,
                        IconClass = "fa fa-lemon-o",
                    };
                    rootNode.ChildNodes.Add(dashboardNode);
                }

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.DashboardHq))
                    dashboardNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.OrderMonitor",
                        Title = "Pedidos del día",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "Order",
                        ActionName = "MonitorDateList"
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.Warnings))
                    dashboardNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.Warnings",
                        Title = "Alertas",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "Warnings",
                        ActionName = "Index",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.OperationalErrors))
                    dashboardNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.OperationalErrors",
                        Title = "Errores operativos",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "Warnings",
                        ActionName = "OperationalErrors",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.HeatMap))
                    dashboardNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.HeatMap",
                        Title = "Generador de mapas de calor",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "HeatMap",
                        ActionName = "Index",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.DiscountsBreakdown))
                    dashboardNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.DiscountsBreakdown",
                        Title = "Desglose de descuentos",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "Discount",
                        ActionName = "ListDates",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.DashboardHqConfig))
                    dashboardNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.DashboardConfig",
                        Title = "Metas diarias",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "GoalsToday",
                        ActionName = "GoalsTodayView",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.DashboardHq))
                    dashboardNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.DashboardHq1",
                        Title = "DHQ - Pedidos por región",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "Order",
                        ActionName = "GetDashboard1",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" }, { "fromAdmin", true } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.DashboardHq))
                    dashboardNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.DashboardHq2",
                        Title = "DHQ - Ventas por región",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "Order",
                        ActionName = "GetDashboard2",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                // Marketing Expenses plugin
                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.DashboardHq))
                    dashboardNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "MarketingDashboard.DashboardHq3",
                        Title = "DHQ - Indicadores de publicidad",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "MonitorHq",
                        ActionName = "GetDashboard3",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" }, { "fromAdmin", true } }
                    });

                // Marketing Expenses plugin
                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.DashboardHq))
                    dashboardNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "MarketingDashboard.DashboardHq4",
                        Title = "DHQ - Indicadores de clientes",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "MonitorHq",
                        ActionName = "GetDashboard4",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" }, { "fromAdmin", true } }
                    });

                // Blance usage
                if (_permissionService.Authorize(StandardPermissionProvider.BalanceManager))
                    dashboardNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.BalanceUsage",
                        Title = "Monitor de saldos",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "MonitorHq",
                        ActionName = "BalanceUsageMonitor",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                //First Order List
                if (_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                    dashboardNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.FirstOrderList",
                        Title = "Primeras órdenes",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "Order",
                        ActionName = "FirstOrdersDates"
                    });
            }

            #endregion

            #region Prices node

            currentElementRequiredPermissions = new List<PermissionRecord>()
            {
                TeedGroceriesPermissionProvider.CategoryPrice,
                TeedGroceriesPermissionProvider.UpdatePrices,
                TeedGroceriesPermissionProvider.UpdateCost
            };
            shouldCreate = currentElementRequiredPermissions.Where(x => _permissionService.Authorize(x)).Any();

            if (shouldCreate)
            {
                if (pricesNode == null)
                {
                    pricesNode = new SiteMapNode()
                    {
                        SystemName = "Groceries",
                        Title = "Precios",
                        Visible = true,
                        IconClass = "fa fa-usd",
                    };
                    rootNode.ChildNodes.Add(pricesNode);
                }

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.CategoryPrice))
                    pricesNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.CategoryPrice",
                        Title = "Precios por categoría",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "CategoryPrice",
                        ActionName = "Index",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.UpdatePrices))
                    pricesNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.UpdatePrices",
                        Title = "Actualizar precios",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "Product",
                        ActionName = "UpdatePrices",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.UpdateCost))
                    pricesNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.UpdateCost",
                        Title = "Actualización de costos",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "UpdateCost",
                        ActionName = "Index",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });
            }

            #endregion

            #region Prints node

            currentElementRequiredPermissions = new List<PermissionRecord>()
            {
                TeedGroceriesPermissionProvider.BuyerReportPrint,
                TeedGroceriesPermissionProvider.DeliveryReportPrint
            };

            shouldCreate = currentElementRequiredPermissions.Where(x => _permissionService.Authorize(x)).Any();
            if (shouldCreate)
            {
                if (printsNode == null)
                {
                    printsNode = new SiteMapNode()
                    {
                        SystemName = "Groceries",
                        Title = "Impresiones del día",
                        Visible = true,
                        IconClass = "fa fa-print",
                    };
                    rootNode.ChildNodes.Add(printsNode);
                }

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.DeliveryReportPrint))
                    printsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.DeliveryReportPrint",
                        Title = "Impresiones logística",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "PrintReport",
                        ActionName = "DeliveryReportView",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.BuyerReportPrint))
                    printsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.BuyerReportPrint",
                        Title = "Impresiones compras",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "PrintReport",
                        ActionName = "BuyerReportView",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });
            }

            #endregion

            #region Corcel node

            currentElementRequiredPermissions = new List<PermissionRecord>()
            {
                TeedGroceriesPermissionProvider.CorcelProduct,
            };

            shouldCreate = currentElementRequiredPermissions.Where(x => _permissionService.Authorize(x)).Any();
            if (shouldCreate)
            {
                if (corcelsNode == null)
                {
                    corcelsNode = new SiteMapNode()
                    {
                        SystemName = "Groceries",
                        Title = "CORCEL",
                        Visible = true,
                        IconClass = "fa fa-birthday-cake",
                    };
                    rootNode.ChildNodes.Add(corcelsNode);
                }

                if (_permissionService.Authorize(TeedGroceriesPermissionProvider.CorcelProduct))
                    corcelsNode.ChildNodes.Add(new SiteMapNode()
                    {
                        SystemName = "Groceries.CorcelProduct",
                        Title = "Productos",
                        Visible = true,
                        IconClass = "fa-dot-circle-o",
                        ControllerName = "Corcel",
                        ActionName = "Products",
                        RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                    });
            }

            #endregion

            if (_permissionService.Authorize(TeedGroceriesPermissionProvider.PrintedCouponBook))
            {
                if (myPluginNode4 == null)
                {
                    myPluginNode4 = new SiteMapNode()
                    {
                        SystemName = "Groceries.PrintedCouponBooks",
                        Title = "Cuponeras impresas",
                        Visible = true,
                        IconClass = "fa fa-ticket",
                        ControllerName = "PrintedCouponBook",
                        ActionName = "List",
                    };
                    rootNode.ChildNodes.Add(myPluginNode4);
                }
            }

            if (_permissionService.Authorize(TeedGroceriesPermissionProvider.UrbanPromoters))
            {
                if (myPluginNode4 == null)
                {
                    myPluginNode4 = new SiteMapNode()
                    {
                        SystemName = "Groceries",
                        Title = "Promotores urbanos",
                        Visible = true,
                        IconClass = "fa fa-bullhorn",
                    };
                    rootNode.ChildNodes.Add(myPluginNode4);
                }
                myPluginNode4.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Groceries.UrbanPromoters",
                    Title = "Lista de promotores",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "UrbanPromoter",
                    ActionName = "Index",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });
            }

            if (_permissionService.Authorize(TeedGroceriesPermissionProvider.PANDL))
            {
                if (myPluginNode2 == null)
                {
                    myPluginNode2 = new SiteMapNode()
                    {
                        SystemName = "Groceries",
                        Title = "P&L",
                        Visible = true,
                        IconClass = "fa fa-pencil",
                    };
                    rootNode.ChildNodes.Add(myPluginNode2);
                }
                myPluginNode2.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Groceries.P&L",
                    Title = "P&L Diario",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "Order",
                    ActionName = "PyL",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });
            }

            if (_permissionService.Authorize(TeedGroceriesPermissionProvider.Franchise))
            {
                if (myPluginNode3 == null)
                {
                    myPluginNode3 = new SiteMapNode()
                    {
                        SystemName = "Groceries",
                        Title = "Franquicias",
                        Visible = true,
                        IconClass = "fa fa-building",
                    };
                    rootNode.ChildNodes.Add(myPluginNode3);
                }
                myPluginNode3.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Groceries.Franchises",
                    Title = "Franquicias",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "Franchise",
                    ActionName = "FranchiseList",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });
                myPluginNode3.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Groceries.Incidents",
                    Title = "Incidencias de logística",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "Incidents",
                    ActionName = "IncidentDatesList",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });
                myPluginNode3.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Groceries.PenaltiesCatalog",
                    Title = "Penalizaciones y cargos",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "PenaltyCatalog",
                    ActionName = "PenaltiesList",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });
                myPluginNode3.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Groceries.PenaltiesVariables",
                    Title = "Variables de incidencias",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "PenaltyVariables",
                    ActionName = "PenaltiesList",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });
                myPluginNode3.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Groceries.Billings",
                    Title = "Comisiones",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "Billing",
                    ActionName = "BillingList",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });
                myPluginNode3.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Groceries.RatesAndBonusesSettings",
                    Title = "Tarifas y bonos",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "Franchise",
                    ActionName = "RatesAndBonuses",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });
                myPluginNode3.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Groceries.Payments",
                    Title = "Pagos",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "Payment",
                    ActionName = "List",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });

                myPluginNode3.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "Groceries.AssignmentCheck",
                    Title = "Verificación de asignaciones",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    ControllerName = "Franchise",
                    ActionName = "AssignmentCheck",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                });
            }
        }

        public override void Uninstall()
        {
            _context.Uninstall();
            base.Uninstall();
        }
    }
}
