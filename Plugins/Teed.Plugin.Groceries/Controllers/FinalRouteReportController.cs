using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;
using Teed.Plugin.Groceries.Domain.ShippingZones;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class FinalRouteReportController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly ShippingRouteService _shippingRouteService;
        private readonly ShippingZoneService _shippingZoneService;
        private readonly ShippingRouteUserService _shippingRouteUserService;
        private readonly ShippingVehicleRouteService _shippingVehicleRouteService;

        public FinalRouteReportController(IPermissionService permissionService,
            IOrderService orderService,
            ICustomerService customerService,
            ShippingRouteService shippingRouteService,
            ShippingZoneService shippingZoneService,
            ShippingRouteUserService shippingRouteUserService,
            ShippingVehicleRouteService shippingVehicleRouteService)
        {
            _permissionService = permissionService;
            _orderService = orderService;
            _shippingRouteService = shippingRouteService;
            _shippingZoneService = shippingZoneService;
            _shippingRouteUserService = shippingRouteUserService;
            _shippingVehicleRouteService = shippingVehicleRouteService;
            _customerService = customerService;
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.FinalRouteReport))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/FinalRouteReport/List.cshtml");
        }

        [HttpGet]
        public IActionResult RouteReport(string date)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.FinalRouteReport))
                return AccessDeniedView();

            var controlDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate == controlDate)
                .ToList();
            var assignedRoutes = _shippingRouteUserService.GetAll()
                .Where(x => DbFunctions.TruncateTime(x.ResponsableDateUtc) == controlDate)
                .ToList();
            var assignedVehicles = _shippingVehicleRouteService.GetAll()
                .Where(x => x.ShippingDate == controlDate)
                .ToList();
            var routes = _shippingRouteService.GetAll().ToList();
            var zones = _shippingZoneService.GetAll().ToList();
            var deliveryRole = _customerService.GetCustomerRoleBySystemName("delivery");
            var customers = _customerService.GetAllCustomers(customerRoleIds: new int[] { deliveryRole.Id });
            var model = new FinalRouteModel()
            {
                Orders = orders,
                Zones = zones,
                ShippingDate = controlDate,
                Customers = customers.Select(x => new AssignCustomer()
                {
                    Text = x.GetFullName(),
                    Value = x.Id
                }).ToList(),
                Routes = _shippingRouteService.GetAll()
                .Select(x => new { x.RouteName, x.Id })
                .ToList()
                .Select(x =>
                {
                    var assignedVehicle = assignedVehicles.Where(y => y.RouteId == x.Id).FirstOrDefault();
                    return new AssignedRoute()
                    {
                        Text = x.RouteName,
                        Value = x.Id.ToString(),
                        CustomerIds = assignedRoutes.Where(y => y.ShippingRouteId == x.Id).Select(y => y.UserInChargeId).ToList(),
                        AssignedVehicle = assignedVehicle?.VehicleId == -1 ? "Camioneta rentada" : assignedVehicles.Where(y => y.RouteId == x.Id).Count() > 0 ? VehicleUtils.GetVehicleName(assignedVehicle.Vehicle) : "Sin camioneta asignada",
                        Franchise = assignedVehicle == null || assignedVehicle.Vehicle == null || assignedVehicle.Vehicle.Franchise == null ? "Sin franquicia asignada" : assignedVehicle.Vehicle?.Franchise?.Name
                    };
                }
                ).ToList(),
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/FinalRouteReport/RouteReport.cshtml", model);
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.FinalRouteReport))
                return AccessDeniedView();

            var today = DateTime.Now.Date;
            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService);
            var query = ordersQuery
                .Where(x => x.SelectedShippingDate <= today)
                .Select(x => x.SelectedShippingDate.Value)
                .Distinct();
            var pagedList = new PagedList<DateTime>(query.OrderByDescending(m => m), command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(date => new
                {
                    Date = date.ToString("dddd, dd 'de' MMMM 'de' yyyy", new CultureInfo("es-MX")),
                    DateShort = date.ToString("dd-MM-yyyy"),
                }).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }
    }

    public class FinalRouteModel
    {
        public List<Order> Orders { get; set; }
        public List<AssignedRoute> Routes { get; set; }
        public List<ShippingZone> Zones { get; set; }
        public DateTime ShippingDate { get; set; }
        public List<AssignCustomer> Customers { get; set; }
    }
}
