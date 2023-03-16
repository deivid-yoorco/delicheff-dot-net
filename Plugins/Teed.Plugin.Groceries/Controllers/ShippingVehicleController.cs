using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingVehicles;
using Teed.Plugin.Groceries.Models.ShippingVehicle;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class ShippingVehicleController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly ShippingVehicleService _shippingVehicleService;
        private readonly ShippingVehicleRouteService _shippingVehicleRouteService;
        private readonly FranchiseService _franchiseService;
        private readonly ShippingRouteService _shippingRouteService;

        public ShippingVehicleController(IPermissionService permissionService,
            IWorkContext workContext,
            IOrderService orderService,
            ShippingVehicleService shippingVehicleService,
            ShippingVehicleRouteService shippingVehicleRouteService,
            FranchiseService franchiseService,
            ShippingRouteService shippingRouteService)
        {
            _permissionService = permissionService;
            _workContext = workContext;
            _shippingVehicleService = shippingVehicleService;
            _shippingVehicleRouteService = shippingVehicleRouteService;
            _franchiseService = franchiseService;
            _orderService = orderService;
            _shippingRouteService = shippingRouteService;
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingVehicles))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingVehicle/List.cshtml");
        }

        public IActionResult VehicleRouteList()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingVehicles))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingVehicleRoute/List.cshtml");
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command, int franchiseId = 0)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingVehicles))
                return AccessDeniedView();

            var query = _shippingVehicleService.GetAll();
            if (franchiseId > 0) query = query.Where(x => x.FranchiseId == franchiseId);
            var pagedList = new PagedList<ShippingVehicle>(query.OrderByDescending(m => m.CreatedOnUtc), command.Page - 1, command.PageSize);
            var franchises = _franchiseService.GetAll().ToList();

            var data = pagedList.Select(x => new
            {
                x.Brand,
                x.Plates,
                x.Year,
                Franchise = franchises.Where(y => y.Id == x.FranchiseId).FirstOrDefault()?.Name,
                IsActive = x.Active ? "SI" : "NO",
                x.Id,
                GpsDate = !x.GpsInstallationDate.HasValue ? "Sin información" : x.GpsInstallationDate.Value.ToString("dd-MM-yyyy")
            }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = data,
                Total = query.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult VehicleRouteListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingVehicles))
                return AccessDeniedView();

            var now = DateTime.Now.AddDays(1).Date;
            var query = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate <= now)
                .GroupBy(x => x.SelectedShippingDate.Value)
                .Select(x => x.Key)
                .OrderByDescending(x => x);
            var pagedList = new PagedList<DateTime>(query, command.Page - 1, command.PageSize);

            var pageDates = pagedList.Select(x => x).ToList();
            var dayRoutes = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => pageDates.Contains(x.SelectedShippingDate.Value))
                .GroupBy(x => new { x.RouteId, x.SelectedShippingDate })
                .Select(x => x.Key);
            var vehicleRoute = _shippingVehicleRouteService.GetAll().Where(x => pageDates.Contains(x.ShippingDate));

            var data = pagedList.Select(x => new
            {
                Date = x.ToString("dddd, dd-MM-yyyy", new CultureInfo("es-MX")),
                SimpleDate = x.ToString("dd-MM-yyyy"),
                Pending = vehicleRoute.Where(y => y.ShippingDate == x).Count() != dayRoutes.Where(y => y.SelectedShippingDate == x).Count()
            }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = data,
                Total = query.Count()
            };

            return Json(gridModel);
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingVehicles))
                return AccessDeniedView();

            var franchises = _franchiseService.GetAll().ToList();
            var model = new CreateEditViewModel()
            {
                AvailablesFranchises = franchises.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                    Selected = false
                }).OrderBy(x => x.Value).ToList()
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingVehicle/Create.cshtml", model);
        }

        public IActionResult AssignVehicles(string date)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingVehicles))
                return AccessDeniedView();

            var parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;

            var routes = _shippingRouteService.GetAll().Where(x => x.Active).ToList();
            var vehicles = _shippingVehicleService.GetAll().Where(x => x.Active).ToList();
            var vehicleRoutes = _shippingVehicleRouteService.GetAll().Where(x => x.ShippingDate == parsedDate).ToList();

            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate == parsedDate);
            var orderRouteIds = orders.GroupBy(x => x.RouteId).Select(x => x.Key).Where(x => x > 0).ToList();

            var routesWithoutOrders = _shippingRouteService.GetAll().Where(x => !orderRouteIds.Contains(x.Id) && x.Active).ToList();

            var model = new AssignVehicleModel()
            {
                Date = parsedDate,
                Routes = orderRouteIds.Select(x => new RouteVehicle()
                {
                    Route = routes.Where(y => y.Id == x).FirstOrDefault(),
                    SelectedVehicleId = vehicleRoutes.Where(y => y.RouteId == x).Select(y => y.VehicleId).FirstOrDefault()
                }).ToList(),
                RoutesWithoutOrders = routesWithoutOrders.Select(x => new RouteVehicle()
                {
                    Route = x,
                    SelectedVehicleId = vehicleRoutes.Where(y => y.RouteId == x.Id).Select(y => y.VehicleId).FirstOrDefault()
                }).ToList(),
                Vehicles = vehicles.Select(x => new SelectListItem()
                {
                    Text = VehicleUtils.GetVehicleName(x) + $" ({x.Franchise.Name})",
                    Value = x.Id.ToString()
                }).ToList()
            };

            model.Vehicles.Insert(0, new SelectListItem() { Text = "No asignado", Value = "0" });
            model.Vehicles.Add(new SelectListItem() { Text = "Camioneta rentada", Value = "-1" });

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingVehicleRoute/AssignVehicles.cshtml", model);
        }

        [HttpPost]
        public IActionResult AssignVehicles(SubmitAssignVehicleModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingVehicles))
                return AccessDeniedView();

            var sameVehicleToMultipleRoutes = model.Data.Select(x => x.SelectedVehicleId).Where(x => x > 0).GroupBy(x => x).Where(x => x.Count() > 1).Any();
            if (sameVehicleToMultipleRoutes)
            {
                return BadRequest("El mismo vehículo no puede estar asignado a rutas diferentes.");
            }

            foreach (var data in model.Data)
            {
                var assigned = _shippingVehicleRouteService.GetAll()
                    .Where(x => x.RouteId == data.RouteId && x.ShippingDate == model.Date)
                    .ToList();

                foreach (var item in assigned)
                    _shippingVehicleRouteService.Delete(item);

                if (data.SelectedVehicleId == 0) continue;
                var newRouteAssign = new ShippingVehicleRoute()
                {
                    RouteId = data.RouteId,
                    VehicleId = data.SelectedVehicleId,
                    ShippingDate = model.Date,
                    Log = $"Vehículo asignado por {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id})"
                };
                _shippingVehicleRouteService.Insert(newRouteAssign);
            }

            return NoContent();
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingVehicles))
                return AccessDeniedView();

            var shippingVehicle = _shippingVehicleService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (shippingVehicle == null) return NotFound();

            var franchises = _franchiseService.GetAll().ToList();

            CreateEditViewModel model = new CreateEditViewModel()
            {
                Id = shippingVehicle.Id,
                Active = shippingVehicle.Active,
                BunchVolume = shippingVehicle.BunchVolume,
                FranchiseId = shippingVehicle.FranchiseId,
                FridgeVolume = shippingVehicle.FridgeVolume,
                LoadingCapacity = shippingVehicle.LoadingCapacity,
                Brand = shippingVehicle.Brand,
                Plates = shippingVehicle.Plates,
                Year = shippingVehicle.Year,
                Log = shippingVehicle.Log,
                InstallationDateString = shippingVehicle.GpsInstallationDate.HasValue ? shippingVehicle.GpsInstallationDate.Value.ToString("dd-MM-yyyy") : null,
                AvailablesFranchises = franchises.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                    Selected = x.Id == shippingVehicle.FranchiseId
                }).OrderBy(x => x.Value).ToList()
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingVehicle/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult Create(CreateEditViewModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingVehicles))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Existen valores inválidos");
                var franchises = _franchiseService.GetAll().ToList();
                model.AvailablesFranchises = franchises.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                    Selected = x.Id == model.FranchiseId
                }).OrderBy(x => x.Value).ToList();
                return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingVehicle/Create.cshtml", model);
            };

            ShippingVehicle shippingVehicle = new ShippingVehicle()
            {
                Active = model.Active,
                BunchVolume = model.BunchVolume,
                FranchiseId = model.FranchiseId,
                FridgeVolume = model.FridgeVolume,
                LoadingCapacity = model.LoadingCapacity,
                Brand = model.Brand,
                Plates = model.Plates,
                GpsInstallationDate = string.IsNullOrEmpty(model.InstallationDateString) ? null : (DateTime?)DateTime.ParseExact(model.InstallationDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                Year = model.Year,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - El usuario " + _workContext.CurrentCustomer.Email + $" ({_workContext.CurrentCustomer.Id}) creó el vehículo.\n"
            };
            _shippingVehicleService.Insert(shippingVehicle);
            model.Id = shippingVehicle.Id;

            return RedirectToAction("Edit", new { id = model.Id });
        }

        [HttpPost]
        public IActionResult Edit(CreateEditViewModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingVehicles))
                return AccessDeniedView();

            var franchises = _franchiseService.GetAll().ToList();

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Existen valores inválidos");
                model.AvailablesFranchises = franchises.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                    Selected = x.Id == model.FranchiseId
                }).OrderBy(x => x.Value).ToList();
                return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingVehicle/Edit.cshtml", model);
            };

            var shippingVehicle = _shippingVehicleService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (shippingVehicle == null) return NotFound();

            shippingVehicle.Log += PrepareLog(model, shippingVehicle);
            shippingVehicle.Active = model.Active;
            shippingVehicle.BunchVolume = model.BunchVolume;
            shippingVehicle.FranchiseId = model.FranchiseId;
            shippingVehicle.FridgeVolume = model.FridgeVolume;
            shippingVehicle.LoadingCapacity = model.LoadingCapacity;
            shippingVehicle.Year = model.Year;
            shippingVehicle.Plates = model.Plates;
            shippingVehicle.Brand = model.Brand;
            shippingVehicle.GpsInstallationDate = string.IsNullOrEmpty(model.InstallationDateString) ? null : (DateTime?)DateTime.ParseExact(model.InstallationDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            _shippingVehicleService.Update(shippingVehicle);
            model.Id = shippingVehicle.Id;

            return RedirectToAction("Edit", new { id = model.Id });
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var vehicle = _shippingVehicleService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (vehicle == null) return RedirectToAction("List");

            _shippingVehicleService.Delete(vehicle);

            return RedirectToAction("List");
        }

        private string PrepareLog(CreateEditViewModel newValue, ShippingVehicle originalValue)
        {
            string log = string.Empty;

            if (newValue.Active != originalValue.Active)
            {
                log += $" Cambió el estado de activo del vehículo de {originalValue.Active} a {newValue.Active}.";
            }

            if (newValue.BunchVolume != originalValue.BunchVolume)
            {
                log += $" Cambió el volumen de manojo de {originalValue.BunchVolume} a {newValue.BunchVolume}.";
            }

            if (newValue.LoadingCapacity != originalValue.LoadingCapacity)
            {
                log += $" Cambió la capacidad de carga de {originalValue.LoadingCapacity} a {newValue.LoadingCapacity}.";
            }

            if (newValue.Brand != originalValue.Brand)
            {
                log += $" Cambió la marca y submarca de '{originalValue.Brand}' a '{newValue.Brand}'.";
            }

            if (newValue.Year != originalValue.Year)
            {
                log += $" Cambió el año de '{originalValue.Year}' a '{newValue.Year}'.";
            }

            if (newValue.Plates != originalValue.Plates)
            {
                log += $" Cambió las placas de '{originalValue.Plates}' a '{newValue.Plates}'.";
            }

            if (newValue.FranchiseId != originalValue.FranchiseId)
            {
                log += $" Cambió la franquicia del ID {originalValue.FranchiseId} al ID {newValue.FranchiseId}.";
            }

            if (newValue.FridgeVolume != originalValue.FridgeVolume)
            {
                log += $" Cambió el volumen de nevera de {originalValue.FridgeVolume} a {newValue.FridgeVolume}.";
            }

            if (newValue.InstallationDateString != originalValue.GpsInstallationDate?.ToString("dd-MM-yyyy"))
            {
                log += $" Cambió la fecha de instalación del GPS de {originalValue.GpsInstallationDate?.ToString("dd-MM-yyyy")} a {newValue.InstallationDateString}.";
            }

            if (!string.IsNullOrWhiteSpace(log))
            {
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - El usuario " + _workContext.CurrentCustomer.Email + $" ({_workContext.CurrentCustomer.Id}) modificó el vehículo." + log + "\n";
            }

            return log;
        }
    }
}
