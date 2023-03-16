using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;
using Teed.Plugin.Groceries.Models.ShippingRegion;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Manager.Models.Groceries;
using Teed.Plugin.Groceries.Domain.ShippingZones;
using Nop.Services.Catalog;
using Teed.Plugin.Groceries.Extensions;
using Teed.Plugin.Groceries.Domain.OrderItemBuyers;
using Nop.Core.Domain.Customers;
using System.Data.Entity;
using Teed.Plugin.Groceries.Domain.RescheduledOrderLogs;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Domain.ShippingRegions;
using Nop.Services.Configuration;
using Teed.Plugin.Groceries.Settings;
using Nop.Services.Stores;
using Teed.Plugin.Groceries.Models.Settings;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class ShippingRegionController : BasePluginController
    {
        public const string GOOGLE_API_KEY = "AIzaSyAUvh0c7WBIqAeMnpGnx09MKsYQOlHkJJw";

        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IAddressService _addressService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly OrderItemBuyerService _orderItemBuyerService;
        private readonly NotDeliveredOrderItemService _notDeliveredOrderItemService;
        private readonly ShippingRouteService _shippingRouteService;
        private readonly ShippingRouteUserService _shippingRouteUserService;
        private readonly ShippingZoneService _shippingZoneService;
        private readonly OrderTypeService _orderTypeService;
        private readonly RescheduledOrderLogService _orderLogService;
        private readonly ShippingRegionService _shippingRegionService;
        private readonly ShippingRegionZoneService _shippingRegionZoneService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;

        public ShippingRegionController(IPermissionService permissionService, ShippingRouteService shippingRouteService,
            ShippingRouteUserService shippingRouteUserService, IWorkContext workContext, ICustomerService customerService,
            IOrderService orderService, IAddressService addressService, ShippingZoneService shippingZoneService, OrderTypeService orderTypeService,
            IManufacturerService manufacturerService, NotDeliveredOrderItemService notDeliveredOrderItemService, IProductService productService,
            OrderItemBuyerService orderItemBuyerService, RescheduledOrderLogService orderLogService,
            ShippingRegionService shippingRegionService, ShippingRegionZoneService shippingRegionZoneService,
            ISettingService settingService, IStoreService storeService)
        {
            _permissionService = permissionService;
            _shippingRouteService = shippingRouteService;
            _shippingRouteUserService = shippingRouteUserService;
            _workContext = workContext;
            _customerService = customerService;
            _orderService = orderService;
            _addressService = addressService;
            _shippingZoneService = shippingZoneService;
            _orderTypeService = orderTypeService;
            _manufacturerService = manufacturerService;
            _notDeliveredOrderItemService = notDeliveredOrderItemService;
            _productService = productService;
            _orderItemBuyerService = orderItemBuyerService;
            _orderLogService = orderLogService;
            _shippingRegionService = shippingRegionService;
            _shippingRegionZoneService = shippingRegionZoneService;
            _settingService = settingService;
            _storeService = storeService;
        }

        [AuthorizeAdmin]
        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRegion))
                return AccessDeniedView();

            var zones = _shippingZoneService.GetAll();
            var assignedZoneIds = _shippingRegionZoneService.GetAll().Select(x => x.ZoneId).ToList();

            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<ScheduleSettings>(storeScope);

            var model = new ShippingZoneListModel()
            {
                PendingZones = zones.Where(x => !assignedZoneIds.Contains(x.Id)).ToList(),
                GlobalScheduleSettings = new GlobalScheduleSettingsModel()
                {
                    Schedule1Quantity = settings.Schedule1Quantity,
                    Schedule2Quantity = settings.Schedule2Quantity,
                    Schedule3Quantity = settings.Schedule3Quantity,
                    Schedule4Quantity = settings.Schedule4Quantity
                }
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingRegion/List.cshtml", model);
        }

        [AuthorizeAdmin]
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRegion))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingRegion/Create.cshtml", new CreateEditViewModel());
        }

        [AuthorizeAdmin]
        public IActionResult EditGlobal()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRegion))
                return AccessDeniedView();

            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<ScheduleSettings>(storeScope);

            var model = new GlobalScheduleSettingsModel()
            {
                Schedule1Quantity = settings.Schedule1Quantity,
                Schedule2Quantity = settings.Schedule2Quantity,
                Schedule3Quantity = settings.Schedule3Quantity,
                Schedule4Quantity = settings.Schedule4Quantity
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingRegion/EditGlobal.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult EditGlobal(GlobalScheduleSettingsModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRegion))
                return AccessDeniedView();

            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<ScheduleSettings>(storeScope);

            //save settings
            settings.Schedule1Quantity = model.Schedule1Quantity;
            settings.Schedule2Quantity = model.Schedule2Quantity;
            settings.Schedule3Quantity = model.Schedule3Quantity;
            settings.Schedule4Quantity = model.Schedule4Quantity;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(settings, x => x.Schedule1Quantity, true, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.Schedule2Quantity, true, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.Schedule3Quantity, true, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.Schedule4Quantity, true, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            return RedirectToAction("EditGlobal");
        }

        [AuthorizeAdmin]
        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRegion))
                return AccessDeniedView();

            var shippingRegion = _shippingRegionService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (shippingRegion == null) return NotFound();

            CreateEditViewModel model = new CreateEditViewModel()
            {
                Id = shippingRegion.Id,
                Name = shippingRegion.Name,
                Schedule1Quantity = shippingRegion.Schedule1Quantity,
                Schedule2Quantity = shippingRegion.Schedule2Quantity,
                Schedule3Quantity = shippingRegion.Schedule3Quantity,
                Schedule4Quantity = shippingRegion.Schedule4Quantity,
            };

            var shippingRegionZoneQuery = _shippingRegionZoneService.GetAll()
                .Where(x => x.RegionId == model.Id);

            var zoneIds = shippingRegionZoneQuery.Select(x => x.ZoneId)
                .ToList();
            var zones = _shippingZoneService.GetAll()
                .Where(x => zoneIds.Contains(x.Id))
                .ToList();
            model.ZoneNames = string.Join(", ", zones.Select(x => x.ZoneName)) + ", ";
            model.ZoneIds = string.Join(",", zones.Select(x => x.Id)) + ",";

            var postalCodesList = shippingRegionZoneQuery.Select(x => x.Zone.PostalCodes + "," + x.Zone.AdditionalPostalCodes).ToList();
            var postalCodes = string.Join(",", postalCodesList).Split(',').Select(x => x.Trim()).ToList();

            DateTime today = DateTime.Now.Date.Date;
            DateTime controlDate = today.AddDays(-30);

            model.CurrentOrderCount = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today &&
                postalCodes.Contains(x.ShippingAddress.ZipPostalCode))
                .GroupBy(x => x.SelectedShippingTime)
                .ToDictionary(x => x.Key, x => x.GroupBy(y => y.SelectedShippingDate).Select(y => y.Count()).Max().ToString());

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingRegion/Edit.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult Create(CreateEditViewModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRegion))
                return AccessDeniedView();

            if (!ModelState.IsValid) return BadRequest();

            ShippingRegion shippingRegion = new ShippingRegion()
            {
                Name = model.Name,
                Schedule1Quantity = model.Schedule1Quantity,
                Schedule2Quantity = model.Schedule2Quantity,
                Schedule3Quantity = model.Schedule3Quantity,
                Schedule4Quantity = model.Schedule4Quantity,
            };
            _shippingRegionService.Insert(shippingRegion);
            model.Id = shippingRegion.Id;

            UpdateShippingRegionZones(model);

            return RedirectToAction("Edit", new { id = model.Id });
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult Edit(CreateEditViewModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRegion))
                return AccessDeniedView();

            if (!ModelState.IsValid) return BadRequest();

            var shippingRegion = _shippingRegionService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (shippingRegion == null) return NotFound();

            shippingRegion.Name = model.Name;
            shippingRegion.Schedule1Quantity = model.Schedule1Quantity;
            shippingRegion.Schedule2Quantity = model.Schedule2Quantity;
            shippingRegion.Schedule3Quantity = model.Schedule3Quantity;
            shippingRegion.Schedule4Quantity = model.Schedule4Quantity;
            _shippingRegionService.Update(shippingRegion);

            UpdateShippingRegionZones(model);

            return RedirectToAction("Edit", new { id = model.Id });
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var query = _shippingRegionService.GetAll();
            var queryList = query.OrderByDescending(m => m.CreatedOnUtc).ToList();
            var pagedList = new PagedList<ShippingRegion>(queryList, command.Page - 1, command.PageSize);
            var zones = _shippingZoneService.GetAll().ToList();
            var shippingRegionZones = _shippingRegionZoneService.GetAll().ToList();

            var data = pagedList.Select(x => new
            {
                x.Name,
                ZoneNames = GetRegionZoneNames(zones, shippingRegionZones.Where(y => y.RegionId == x.Id).ToList()),
                x.Id,
                x.Schedule1Quantity,
                x.Schedule2Quantity,
                x.Schedule3Quantity,
                x.Schedule4Quantity,
                Total = x.Schedule1Quantity + x.Schedule2Quantity + x.Schedule3Quantity + x.Schedule4Quantity
            }).ToList();

            data.Add(new
            {
                Name = "",
                ZoneNames = "TOTAL",
                Id = 0,
                Schedule1Quantity = data.Select(x => x.Schedule1Quantity).DefaultIfEmpty().Sum(),
                Schedule2Quantity = data.Select(x => x.Schedule2Quantity).DefaultIfEmpty().Sum(),
                Schedule3Quantity = data.Select(x => x.Schedule3Quantity).DefaultIfEmpty().Sum(),
                Schedule4Quantity = data.Select(x => x.Schedule4Quantity).DefaultIfEmpty().Sum(),
                Total = data.Select(x => x.Schedule1Quantity).DefaultIfEmpty().Sum() +
                data.Select(x => x.Schedule2Quantity).DefaultIfEmpty().Sum() +
                data.Select(x => x.Schedule3Quantity).DefaultIfEmpty().Sum() +
                data.Select(x => x.Schedule4Quantity).DefaultIfEmpty().Sum()
            });

            var gridModel = new DataSourceResult
            {
                Data = data,
                Total = query.Count()
            };

            return Json(gridModel);
        }

        public virtual string GetRegionZoneNames(List<ShippingZone> zones, List<ShippingRegionZone> shippingRegionZones)
        {
            var final = string.Empty;
            if (shippingRegionZones.Any())
            {
                var zoneIds = shippingRegionZones.Select(x => x.ZoneId);
                zones = zones.Where(x => zoneIds.Contains(x.Id)).ToList();
                final = string.Join(", ", zones.Select(x => x.ZoneName));
            }
            else
                final = "Sin zonas";
            return final;
        }

        [HttpGet]
        [AuthorizeAdmin]
        public IActionResult GetAllPostalCodesOfRegions(int notThisRegion = 0)
        {
            List<ZonesModel> model = new List<ZonesModel>();
            var regions = _shippingRegionService.GetAll().Where(x => x.Id != notThisRegion).ToList();
            var zones = _shippingZoneService.GetAll().ToList();
            var regionOfZones = _shippingRegionZoneService.GetAll().Where(x => x.RegionId != notThisRegion).ToList();
            foreach (var zone in zones.Where(x => !string.IsNullOrWhiteSpace(x.PostalCodes)))
            {
                model.Add(new ZonesModel
                {
                    Id = zone.Id.ToString(),
                    Pcs = zone.PostalCodes?.Replace(" ", "") + "," + zone.AdditionalPostalCodes?.Replace(" ", ""),
                    ZoneName = zone.ZoneName,
                    RegionId = regionOfZones.Where(x => x.ZoneId == zone.Id).FirstOrDefault()?.RegionId.ToString(),
                    Color = zone.ZoneColor
                });
            }
            return Ok(model);
        }

        [HttpGet]
        [AuthorizeAdmin]
        public IActionResult GetDashboardRegions()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.DashboardHq))
                return AccessDeniedView();

            var regions = _shippingRegionZoneService.GetAll()
                .Include(x => x.Zone)
                .Include(x => x.Region)
                .GroupBy(x => x.RegionId)
                .ToList();
            var model = new List<RegionsModel>();

            foreach (var regionGroup in regions)
            {
                model.Add(new RegionsModel
                {
                    RegionId = regionGroup.Key,
                    Pcs = regionGroup.SelectMany(x => (x.Zone?.PostalCodes + "," + x.Zone?.AdditionalPostalCodes).Split(',').Select(y => y?.Trim())).ToArray()
                });
            }
            return Ok(model);
        }

        [HttpGet]
        [AuthorizeAdmin]
        public IActionResult GetTakenZones(int regionId = 0)
        {
            var regionsByZonesIds = _shippingRegionZoneService.GetAll().ToList().Select(x => x.ZoneId);
            var zones = _shippingZoneService.GetAll().Where(x => regionsByZonesIds.Contains(x.Id)).ToList();
            return Ok(string.Join(",", zones.Select(x => x.Id)).Split(','));
        }

        public void UpdateShippingRegionZones(CreateEditViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ZoneIds)) return;
            var ZoneIds = model.ZoneIds.Split(',')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => int.Parse(x)).ToList();
            var existingRegionZones = _shippingRegionZoneService.GetAll()
                .Where(x => x.RegionId == model.Id).ToList();
            if (existingRegionZones.Any())
            {
                var existingZoneIds = existingRegionZones.Select(x => x.ZoneId).ToList();
                var toDelete = existingZoneIds.Except(ZoneIds).ToList();
                foreach (var zoneId in toDelete)
                {
                    var current = existingRegionZones.Where(x => x.ZoneId == zoneId).FirstOrDefault();
                    _shippingRegionZoneService.Delete(current);
                }
                var toInsert = ZoneIds.Except(existingZoneIds).Where(x => x > 0).ToList();
                foreach (var zoneId in toInsert)
                {
                    _shippingRegionZoneService.Insert(new ShippingRegionZone
                    {
                        RegionId = model.Id,
                        ZoneId = zoneId
                    });
                }
            }
            else
            {
                foreach (var zoneId in ZoneIds)
                {
                    _shippingRegionZoneService.Insert(new ShippingRegionZone
                    {
                        RegionId = model.Id,
                        ZoneId = zoneId
                    });
                }
            }
        }


        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var region = _shippingRegionService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (region == null)
                //No region found with the specified id
                return RedirectToAction("List");

            try
            {
                var regionZones = _shippingRegionZoneService.GetAll()
                    .Where(x => x.RegionId == region.Id).ToList();
                foreach (var regionZone in regionZones)
                {
                    _shippingRegionZoneService.Delete(regionZone);
                }
                _shippingRegionService.Delete(region);
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = region.Id });
            }
        }
    }

    class ZonesModel
    {
        public string Id { get; set; }
        public string RegionId { get; set; }
        public string Color { get; set; }
        public string Pcs { get; set; }
        public string ZoneName { get; set; }
    }

    class RegionsModel
    {
        public int RegionId { get; set; }
        public string[] Pcs { get; set; }
    }
}
