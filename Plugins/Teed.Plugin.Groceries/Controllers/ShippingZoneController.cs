using iText.Forms.Xfdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingZones;
using Teed.Plugin.Groceries.Models.ShippingZone;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Manager.Models.Groceries;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class ShippingZoneController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly ShippingZoneService _shippingZoneService;
        private readonly ShippingAreaService _shippingAreaService;

        public ShippingZoneController(IPermissionService permissionService,
            ShippingZoneService shippingZoneService,
            ShippingAreaService shippingAreaService,
            IWorkContext workContext,
            ICustomerService customerService,
            IOrderService orderService)
        {
            _permissionService = permissionService;
            _shippingZoneService = shippingZoneService;
            _shippingAreaService = shippingAreaService;
            _workContext = workContext;
            _customerService = customerService;
            _orderService = orderService;
        }

        [AuthorizeAdmin]
        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingZone))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingZone/List.cshtml");
        }

        [AuthorizeAdmin]
        public IActionResult AssignRouteOrderList()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingZone))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingZone/AssignZoneOrderList.cshtml");
        }

        //[AuthorizeAdmin]
        //public IActionResult AssignRouteOrder(string date, int orderBy = 0)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
        //        return AccessDeniedView();

        //    var dateDelivery = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //    var orders = _orderService.GetAllOrdersQuery().Where(x => x.SelectedShippingDate == dateDelivery)
        //                        .Where(x => x.OrderStatusId != 40 &&
        //                            !(x.PaymentMethodSystemName == "Payments.PayPalStandard" &&
        //                            x.PaymentStatusId == 10));

        //    switch (orderBy)
        //    {
        //        case 1:
        //            orders = orders.OrderBy(x => x.RouteDisplayOrder);
        //            break;
        //        case 2:
        //            orders = orders.OrderBy(x => x.ShippingAddress.Address2);
        //            break;
        //        default:
        //            orders = orders.OrderBy(x => x.ShippingAddress.ZipPostalCode).ThenBy(x => x.Id);
        //            break;
        //    }

        //    var model = new ShippingRouteOrderModel()
        //    {
        //        Date = dateDelivery,
        //        Orders = orders.ToList(),
        //        Routes = _shippingRouteService.GetAll().ToList().Select(x => new SelectListItem()
        //        {
        //            Text = x.RouteName,
        //            Value = x.Id.ToString()
        //        }).ToList()
        //    };

        //    model.Routes.Insert(0, new SelectListItem() { Value = "0", Text = "No asignado" });

        //    ViewBag.OrderBy = orderBy;

        //    return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingRoute/AssignRouteOrder.cshtml", model);
        //}

        //[HttpPost]
        //[AuthorizeAdmin]
        //public IActionResult AssignRouteOrder(AssignRouteResultModel model)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
        //        return AccessDeniedView();

        //    foreach (var item in model.Result)
        //    {
        //        Order order = _orderService.GetOrderById(item.OrderId);
        //        if (order == null) continue;
        //        if (order.RouteId != item.RouteId || order.RouteDisplayOrder != item.RouteDisplayOrder)
        //        {
        //            string message = string.Empty;
        //            if (order.RouteId != item.RouteId)
        //            {
        //                message = $"El usuario {_workContext.CurrentCustomer.Email} le asignó a la orden la ruta con Id {item.RouteId}.";
        //            }
        //            if (order.RouteDisplayOrder != item.RouteDisplayOrder)
        //            {
        //                message += $"El usuario {_workContext.CurrentCustomer.Email} le asignó {item.RouteDisplayOrder} como orden para mostrar a la ruta de esta orden.";
        //            }

        //            order.OrderNotes.Add(new OrderNote()
        //            {
        //                CreatedOnUtc = DateTime.UtcNow,
        //                DisplayToCustomer = false,
        //                Note = message
        //            });
        //            order.RouteId = item.RouteId;
        //            order.RouteDisplayOrder = item.RouteDisplayOrder;
        //            _orderService.UpdateOrder(order);
        //        }
        //    }

        //    return NoContent();
        //}

        [AuthorizeAdmin]
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingZone))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingZone/Create.cshtml", new CreateViewModel { PostalCodesWarning = "" });
        }

        [AuthorizeAdmin]
        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingZone))
                return AccessDeniedView();

            ShippingZone shippingZone = _shippingZoneService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (shippingZone == null) return NotFound();

            EditViewModel model = new EditViewModel()
            {
                Id = shippingZone.Id,
                PostalCodes = !string.IsNullOrWhiteSpace(shippingZone.PostalCodes) ? shippingZone.PostalCodes.Replace(" ", "").Replace(",", ", ") : "",
                ZoneName = shippingZone.ZoneName,
                ZoneColor = "#" + shippingZone.ZoneColor.Replace("#", ""),
                AdditionalPostalCodes = !string.IsNullOrWhiteSpace(shippingZone.AdditionalPostalCodes) ? shippingZone.AdditionalPostalCodes.Replace(" ", "").Replace(",", ", ") : "",
                ShippingZones = _shippingZoneService.GetAll().Where(x => x.Id != id).ToList()
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingZone/Edit.cshtml", model);
        }

        [AuthorizeAdmin]
        public IActionResult DeleteShippingZone(int id, int newZoneId)
        {
            ShippingZone shippingZone = _shippingZoneService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (shippingZone == null) return NotFound();

            var ordersInZone = _orderService.GetAllOrdersQuery().Where(x => x.RouteId == id).ToList();
            foreach (var order in ordersInZone)
            {
                order.RouteId = newZoneId;
                order.OrderNotes.Add(new OrderNote()
                {
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    Note = $"Se cambió la zona de la orden por solicitud ya que el usuario {_workContext.CurrentCustomer.Email} eliminó la zona {shippingZone.ZoneName} ({shippingZone.Id})",
                    CustomerId = _workContext.CurrentCustomer.Id
                });
                _orderService.UpdateOrder(order);
            }
            _shippingZoneService.Delete(shippingZone);

            return RedirectToAction("List");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetShippingRouteId(string pc)
        {
            return Ok(_shippingZoneService.GetAll().Where(x => x.PostalCodes.Contains(pc)).Select(x => x.Id).FirstOrDefault());
        }

        public string CheckPostalCodes(string postalCodes)
        {
            var checkPcs = string.Join(",", _shippingAreaService.GetAll().Select(x => x.PostalCode.Replace(" ", "")));
            var PcsWarning = new List<string>();
            foreach (var postalCode in postalCodes.Replace(" ", "").Split(','))
            {
                if (!checkPcs.Contains(postalCode))
                {
                    PcsWarning.Add(postalCode);
                }
            }
            if (PcsWarning.Any())
            {
                return "Los siguientes códigos postales no están en el área de cobertura: " +
                        string.Join(", ", PcsWarning);
            }
            else
            {
                return string.Empty;
            }
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult Create(CreateViewModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingZone))
                return AccessDeniedView();

            if (!ModelState.IsValid) return BadRequest();

            var warning = CheckPostalCodes(model.PostalCodes + ", " + model.AdditionalPostalCodes);
            if (!string.IsNullOrEmpty(warning))
            {
                model.PostalCodesWarning = warning;
                return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingZone/Create.cshtml", model);
            }

            ShippingZone shippingZone = new ShippingZone()
            {
                CreatedByUserId = _workContext.CurrentCustomer.Id,
                PostalCodes = model.PostalCodes,
                ZoneName = model.ZoneName,
                ZoneColor = model.ZoneColor,
                AdditionalPostalCodes = model.AdditionalPostalCodes,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) creó la ruta."
            };
            _shippingZoneService.Insert(shippingZone);

            //ShippingZoneUser shippingZoneUser = new ShippingZoneUser()
            //{
            //    ShippingRouteId = shippingZone.Id,
            //    UserInChargeId = model.FirstResponsableUserId,
            //    ResponsableDateUtc = DateTime.UtcNow,
            //    Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + $" asignó al usuario cuando creó la ruta {shippingZone.ZoneName} ({shippingZone.Id})."
            //};
            //_shippingZoneUserService.Insert(shippingZoneUser);

            return RedirectToAction("List");
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult Edit(EditViewModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingZone))
                return AccessDeniedView();

            if (!ModelState.IsValid) return BadRequest();

            var warning = CheckPostalCodes(model.PostalCodes + ", " + model.AdditionalPostalCodes);
            if (!string.IsNullOrEmpty(warning))
            {
                model.PostalCodesWarning = warning;
                model.ShippingZones = _shippingZoneService.GetAll().Where(x => x.Id != model.Id).ToList();
                return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingZone/Edit.cshtml", model);
            }

            ShippingZone shippingZone = _shippingZoneService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (shippingZone == null) return NotFound();

            shippingZone.PostalCodes = model.PostalCodes;
            shippingZone.ZoneName = model.ZoneName;
            shippingZone.ZoneColor = model.ZoneColor;
            shippingZone.AdditionalPostalCodes = model.AdditionalPostalCodes;

            var newLog = string.Empty;
            if (model.PostalCodes != shippingZone.PostalCodes)
            {
                newLog += $". Cambió los códigos postales de {shippingZone.PostalCodes} a {model.PostalCodes}";
            }

            if (model.ZoneName != shippingZone.ZoneName)
            {
                newLog += $". Cambió el nombre de la ruta de {shippingZone.ZoneName} a {model.ZoneName}";
            }

            if (model.ZoneColor != shippingZone.ZoneColor)
            {
                newLog += $". Cambió el color de la zona de {shippingZone.ZoneColor} a {model.ZoneColor}";
            }

            if (model.AdditionalPostalCodes != shippingZone.AdditionalPostalCodes)
            {
                newLog += $". Cambió los códigos postales adicionales de {shippingZone.AdditionalPostalCodes} a {model.AdditionalPostalCodes}";
            }

            shippingZone.Log = shippingZone.Log + "\n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) editó la zona{newLog}";

            _shippingZoneService.Update(shippingZone);

            return RedirectToAction("List");
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingZone))
                return AccessDeniedView();

            var query = _shippingZoneService.GetAll();
            var queryList = query.OrderByDescending(m => m.CreatedOnUtc).ToList();
            var pagedList = new PagedList<ShippingZone>(queryList, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.ZoneName,
                    x.PostalCodes,
                    x.AdditionalPostalCodes,
                    x.Id,
                    Color = "#" + x.ZoneColor?.Replace("#", "")
                }).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        [HttpGet]
        [AuthorizeAdmin]
        public List<string> GetUnassignedPcs()
        {
            var pcs = _shippingAreaService.GetAll().OrderBy(x => x.PostalCode).Select(x => x.PostalCode).ToList();
            var stringsPcs = string.Join(",", _shippingZoneService.GetAll()
                .Where(x => !string.IsNullOrEmpty(x.PostalCodes)).Select(x => x.PostalCodes.Replace(" ", "")).ToList());

            var stringsAdditionalPcs = string.Join(",", _shippingZoneService.GetAll()
                .Where(x => !string.IsNullOrEmpty(x.AdditionalPostalCodes)).Select(x => x.AdditionalPostalCodes.Replace(" ", "")).ToList());
            
            var zonePcs = new List<string>();
            foreach (var stringPcs in (stringsPcs + "," + stringsAdditionalPcs).Split(','))
            {
                zonePcs.Add(stringPcs);
            }

            zonePcs = zonePcs.OrderBy(x => x).ToList();
            var pendingPcs = pcs.Except(zonePcs).ToList();
            return pendingPcs;
        }

        [HttpGet]
        [AuthorizeAdmin]
        public IActionResult GetTakenPostalCodes(int zoneId = 0)
        {
            return Ok(string.Join(",", _shippingZoneService.GetAll().Select(x => x.PostalCodes.Replace(" ", ""))).Split(','));
        }

        [HttpGet]
        [AuthorizeAdmin]
        public IActionResult GetAllPostalCodesOfZones(int notThisZone = 0)
        {
            List<ZonesModel> model = new List<ZonesModel>();
            var zones = _shippingZoneService.GetAll().Where(x => x.Id != notThisZone).ToList();
            foreach (var zone in zones.Where(x => !string.IsNullOrWhiteSpace(x.PostalCodes)))
            {
                model.Add(new ZonesModel
                {
                    Id = zone.Id.ToString(),
                    Color = string.IsNullOrEmpty(zone.ZoneColor) ? "null" : zone.ZoneColor,
                    Pcs = zone.PostalCodes?.Replace(" ", "") + "," + zone.AdditionalPostalCodes?.Replace(" ", ""),
                    ZoneName = zone.ZoneName
                });
            }
            return Ok(model);
        }

        class ZonesModel
        {
            public string Id { get; set; }
            public string Color { get; set; }
            public string Pcs { get; set; }
            public string ZoneName { get; set; }
        }

        [HttpPost]
        public IActionResult ListOrderData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var query = _orderService.GetAllOrdersQuery()
                                    .Where(x => x.OrderStatusId != 40 &&
                                    !(x.PaymentMethodSystemName == "Payments.PayPalStandard" &&
                                    x.PaymentStatusId == 10));
            var queryList = query.GroupBy(x => x.SelectedShippingDate).OrderByDescending(x => x.Key);
            var pagedList = GroupedPageList(queryList, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    Id = x.Key,
                    Date = x.Key.Value.ToString("dd-MM-yyyy"),
                    Pending = GetStatus(x.Select(y => y).ToList())
                }).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        private string GetStatus(List<Order> orders)
        {
            var filteredOrders = orders.Where(x => x.RouteId == 0);
            return filteredOrders.Count() == orders.Count ? "Ninguna asignada" : filteredOrders.Count() > 0 ? "Pendientes por asignar" : "Todas asignadas";
        }

        private List<IGrouping<DateTime?, Order>> GroupedPageList(IQueryable<IGrouping<DateTime?, Order>> source, int pageIndex, int pageSize)
        {
            List<IGrouping<DateTime?, Order>> filteredList = new List<IGrouping<DateTime?, Order>>();
            filteredList.AddRange(source.Skip(pageIndex * pageSize).Take(pageSize).ToList());
            return filteredList;
        }

        [HttpPost]
        public IActionResult UsersListData()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var users = _customerService.GetAllCustomersQuery().Where(x => x.Email != null).ToList().Where(x => x.GetCustomerRoleIds().Count() > 1);
            var elements = users.Select(x => new
            {
                Id = x.Id,
                User = x.GetFullName()
            }).ToList();

            return Json(elements);
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult UpdateOrderRoute(UpdateOrderZoneModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null) return NotFound();

            var route = _shippingZoneService.GetAll().Where(x => x.Id == model.RouteId).FirstOrDefault();
            if (route == null) return NotFound();

            order.RouteId = model.RouteId;
            order.OrderNotes.Add(new OrderNote
            {
                Note = $"{_workContext.CurrentCustomer.GetFullName()} cambió la ruta del envío a {route.ZoneName} ({route.Id})",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = _workContext.CurrentCustomer.Id
            });
            _orderService.UpdateOrder(order);

            return NoContent();
        }
    }

    //public class ShippingRouteOrderModel
    //{
    //    public DateTime Date { get; set; }
    //    public List<SelectListItem> Routes { get; set; }
    //    public List<Order> Orders { get; set; }
    //}

    //public class AssignRouteResultModel
    //{
    //    public DateTime Date { get; set; }
    //    public List<AssignRouteResultData> Result { get; set; }
    //}

    //public class AssignRouteResultData
    //{
    //    public int OrderId { get; set; }
    //    public int RouteId { get; set; }
    //    public int RouteDisplayOrder { get; set; }
    //}
}
