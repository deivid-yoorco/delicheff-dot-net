using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;
using Teed.Plugin.Groceries.Domain.SubstituteBuyers;
using Teed.Plugin.Groceries.Models.ShippingUserRoute;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class ShippingRouteUserController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly ShippingRouteService _shippingRouteService;
        private readonly ShippingRouteUserService _shippingRouteUserService;
        private readonly ShippingVehicleRouteService _shippingVehicleRouteService;
        private readonly IOrderService _orderService;
        private readonly SubstituteBuyerService _substituteBuyerService;

        public ShippingRouteUserController(IPermissionService permissionService, ShippingRouteService shippingRouteService,
            ShippingRouteUserService shippingRouteUserService, IWorkContext workContext, ICustomerService customerService,
            IOrderService orderService, ShippingVehicleRouteService shippingVehicleRouteService,
            SubstituteBuyerService substituteBuyerService)
        {
            _permissionService = permissionService;
            _shippingRouteService = shippingRouteService;
            _shippingRouteUserService = shippingRouteUserService;
            _workContext = workContext;
            _customerService = customerService;
            _orderService = orderService;
            _shippingVehicleRouteService = shippingVehicleRouteService;
            _substituteBuyerService = substituteBuyerService;
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteUser))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingRouteUser/List.cshtml");
        }

        public IActionResult AssignRoutes(string date)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteUser))
                return AccessDeniedView();

            var dateDelivery = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;

            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService).Where(x => DbFunctions.TruncateTime(x.SelectedShippingDate) == dateDelivery);
            if (ordersQuery.Any(x => x.RouteId == 0))
            {
                var emptyModel = new AssignRoutesModel()
                {
                    Date = dateDelivery,
                    Customers = new List<SelectListItem>(),
                    Routes = new List<RouteCustomer>()
                };
                return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingRouteUser/AssignRoutes.cshtml", emptyModel);
            }

            var dateRouteIds = ordersQuery.Select(x => x.RouteId).GroupBy(x => x).Select(x => x.FirstOrDefault()).ToList();
            var deliveryRole = _customerService.GetCustomerRoleBySystemName("delivery");
            var customers = _customerService.GetAllCustomers(customerRoleIds: new int[] { deliveryRole.Id });
            var assignedRoutes = _shippingRouteUserService.GetAll().Where(x => DbFunctions.TruncateTime(x.ResponsableDateUtc) == dateDelivery).ToList();
            var vehicleRoute = _shippingVehicleRouteService.GetAll().Where(x => x.ShippingDate == dateDelivery).OrderBy(x => x.RouteId).ToList();

            var model = new AssignRoutesModel()
            {
                Date = dateDelivery,
                Customers = customers.Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.GetFullName()
                }).OrderBy(x => x.Text).ToList(),
                Routes = vehicleRoute.Select(x => new RouteCustomer()
                {
                    RouteId = x.RouteId,
                    RouteName = x.Route.RouteName,
                    VehicleName = x.VehicleId == -1 ? "Camioneta rentada" : VehicleUtils.GetVehicleName(x.Vehicle),
                    CustomerIds = assignedRoutes.Where(y => y.ShippingRouteId == x.RouteId).Select(y => y.UserInChargeId).ToList()
                }).ToList()
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingRouteUser/AssignRoutes.cshtml", model);
        }

        private IQueryable<Order> GetFilteredOrders()
        {
            return _orderService.GetAllOrdersQuery()
                .Where(x => !x.Deleted && x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10));
        }

        [HttpPost]
        public IActionResult AssignRoutes(SubmitAssignRouteModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteUser))
                return AccessDeniedView();

            //var test = model.Data.SelectMany(x => x.SelectedCustomerIds).GroupBy(x => x).Where(x => x.Count() > 1).ToList();
            var sameUserToMultipleRoutes = model.Data.SelectMany(x => x.SelectedCustomerIds).Where(x => !string.IsNullOrWhiteSpace(x)).GroupBy(x => x).Where(x => x.Count() > 1).Any();
            if (sameUserToMultipleRoutes)
            {
                return BadRequest("El mismo repartidor no puede estar asignado a rutas diferentes.");
            }

            var dateDelivery = model.Date;
            foreach (var data in model.Data)
            {
                var assigned = _shippingRouteUserService.GetAll()
                    .Where(x => x.ShippingRouteId == data.RouteId && DbFunctions.TruncateTime(x.ResponsableDateUtc) == dateDelivery)
                    .ToList();

                foreach (var item in assigned)
                    _shippingRouteUserService.Delete(item);

                foreach (var customerId in data.SelectedCustomerIds)
                {
                    if (string.IsNullOrWhiteSpace(customerId)) continue;
                    var newRouteAssign = new ShippingRouteUser()
                    {
                        ShippingRouteId = data.RouteId,
                        ResponsableDateUtc = dateDelivery,
                        UserInChargeId = int.Parse(customerId),
                        Log = $"Repartidor asignado por {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id})"
                    };
                    _shippingRouteUserService.Insert(newRouteAssign);
                }
            }

            return NoContent();
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteUser))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingRouteUser/Create.cshtml");
        }

        [HttpPost]
        public IActionResult Create(CreateViewModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteUser))
                return AccessDeniedView();

            var parsedDateUtc = DateTime.ParseExact(model.SelectedDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToUniversalTime();

            ShippingRouteUser shippingRouteUser = new ShippingRouteUser()
            {
                ResponsableDateUtc = parsedDateUtc,
                ShippingRouteId = model.ShippingRouteId,
                UserInChargeId = model.UserInChargeId,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm tt") + $" - Asignó un responsable de ruta."
            };

            _shippingRouteUserService.Insert(shippingRouteUser);

            return RedirectToAction("List");
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteUser))
                return AccessDeniedView();

            ShippingRouteUser shippingRouteUser = _shippingRouteUserService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (shippingRouteUser == null) return NotFound();

            EditViewModel model = new EditViewModel()
            {
                Id = shippingRouteUser.Id,
                SelectedDate = shippingRouteUser.ResponsableDateUtc.ToLocalTime().ToString("dd-MM-yyyy"),
                ShippingRouteId = shippingRouteUser.ShippingRouteId,
                UserInChargeId = shippingRouteUser.UserInChargeId
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingRouteUser/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult Edit(EditViewModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteUser))
                return AccessDeniedView();

            ShippingRouteUser shippingRouteUser = _shippingRouteUserService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (shippingRouteUser == null) return NotFound();

            var log = $"/n{DateTime.Now.ToString("dd-MM-yyyy hh:mm tt")} - {_workContext.CurrentCustomer.GetFullName()} editó el registro";
            if (model.SelectedDate != shippingRouteUser.ResponsableDateUtc.ToLocalTime().ToString("dd-MM-yyyy"))
            {
                log += $". Cambió la fecha de {shippingRouteUser.ResponsableDateUtc.ToLocalTime().ToString("dd-MM-yyyy")} a {model.SelectedDate}";
            }

            if (model.ShippingRouteId != shippingRouteUser.ShippingRouteId)
            {
                var initRoute = _shippingRouteService.GetAll().Where(x => x.Id == shippingRouteUser.ShippingRouteId).FirstOrDefault();
                var newRoute = _shippingRouteService.GetAll().Where(x => x.Id == model.ShippingRouteId).FirstOrDefault();
                log += $". Cambió la ruta de {initRoute.RouteName} ({initRoute.Id}) a {newRoute.RouteName} ({newRoute.Id})";
            }

            if (model.UserInChargeId != shippingRouteUser.UserInChargeId)
            {
                var initUser = _customerService.GetCustomerById(shippingRouteUser.UserInChargeId);
                var newUser = _customerService.GetCustomerById(model.UserInChargeId);
                log += $". Cambió el responsable de {initUser.GetFullName()} ({initUser.Id}) a {newUser.GetFullName()} ({newUser.Id})";
            }

            var parsedDateUtc = DateTime.ParseExact(model.SelectedDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToUniversalTime();
            shippingRouteUser.Id = model.Id;
            shippingRouteUser.ResponsableDateUtc = parsedDateUtc;
            shippingRouteUser.ShippingRouteId = model.ShippingRouteId;
            shippingRouteUser.UserInChargeId = model.UserInChargeId;
            shippingRouteUser.Log += log;
            _shippingRouteUserService.Update(shippingRouteUser);

            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command, bool onlyUntilToday = false)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteUser) && !_permissionService.Authorize(TeedGroceriesPermissionProvider.AssignSubstituteBuyers))
                return AccessDeniedView();

            var today = DateTime.Today.Date;
            var query = _orderService.GetAllOrdersQuery()
                                    .Where(x => x.OrderStatusId != 40 &&
                                    !(x.PaymentMethodSystemName == "Payments.PayPalStandard" &&
                                    x.PaymentStatusId == 10));
            var queryList = query.GroupBy(x => x.SelectedShippingDate).Select(x => x.Key.Value);
            if (onlyUntilToday)
                queryList = queryList.Where(x => DbFunctions.TruncateTime(x) <= today);
            queryList = queryList.OrderByDescending(x => x);
            var pagedList = new PagedList<DateTime>(queryList, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    Id = x,
                    Date = x.ToString("dd-MM-yyyy"),
                    //Pending = GetStatus(orderItems.Where(y => y.Order.SelectedShippingDate == x.Key.Value).Select(y => y.Id).ToList())
                }).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        private List<IGrouping<DateTime?, Order>> GroupedPageList(IQueryable<IGrouping<DateTime?, Order>> source, int pageIndex, int pageSize)
        {
            List<IGrouping<DateTime?, Order>> filteredList = new List<IGrouping<DateTime?, Order>>();
            filteredList.AddRange(source.Skip(pageIndex * pageSize).Take(pageSize).ToList());
            return filteredList;
        }

        //private string GetStatus(List<int> orderItemIds)
        //{
        //    var filteredOrderItemBuyer = _orderItemBuyerService.GetAll().Where(x => orderItemIds.Contains(x.OrderItemId)).Select(x => x.CustomerId).ToList();
        //    return filteredOrderItemBuyer.Count() == 0 ? "Ninguno asignado" : filteredOrderItemBuyer.Where(x => x == 0).Any() ? "Pendientes por asignar" : "Todos asignados";
        //}

        [HttpPost]
        public IActionResult UserListData()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteUser))
                return AccessDeniedView();

            var users = _customerService.GetAllCustomers().Where(x => !string.IsNullOrWhiteSpace(x.Email)).Where(x => x.GetCustomerRoleIds().Count() > 1).ToList();
            var elements = users.Select(x => new
            {
                x.Id,
                User = x.GetFullName()
            }).ToList();

            return Json(elements);
        }

        [HttpPost]
        public IActionResult RouteListData()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingRouteUser) && !_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var routes = _shippingRouteService.GetAll().ToList();
            var elements = routes.Select(x => new
            {
                Id = x.Id,
                Route = x.RouteName
            }).ToList();

            return Json(elements);
        }

        public IActionResult ListDatesAssignBuyers()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.AssignSubstituteBuyers))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingRouteUser/ListDatesAssignBuyers.cshtml");
        }

        public IActionResult AssignSubstituteBuyers(string date)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.AssignSubstituteBuyers))
                return AccessDeniedView();

            var dateDelivery = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;

            var substituteBuyers = _substituteBuyerService.GetAllBySelectedShippingDate(dateDelivery).ToList();
            var buyerRole = _customerService.GetCustomerRoleBySystemName("buyer");
            var buyers = _customerService.GetAllCustomers(customerRoleIds: new int[] { buyerRole.Id });

            var model = new AssignBuyersModel()
            {
                Date = dateDelivery,
                SubstituteBuyers = buyers.Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.GetFullName()
                }).OrderBy(x => x.Text).ToList(),
                Buyers = buyers.Select(x => new Buyer()
                {
                    BuyerId = x.Id,
                    BuyerName = $"{x.GetFullName()} ({x.Email})",
                    SubstituteCustomerId = substituteBuyers.Where(y => y.CustomerId == x.Id).FirstOrDefault()?.SubstituteCustomerId ?? 0,
                }).OrderBy(x => x.BuyerName).ToList()
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingRouteUser/AssignSubstituteBuyers.cshtml", model);
        }

        [HttpPost]
        public IActionResult AssignSubstituteBuyers(SubmitAssignBuyersModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.AssignSubstituteBuyers))
                return AccessDeniedView();

            var dateDelivery = model.Date;
            var substituteBuyers = _substituteBuyerService.GetAllBySelectedShippingDate(dateDelivery).ToList();

            foreach (var substituteBuyer in substituteBuyers)
            {
                substituteBuyer.Log += $"\nComprador substituto eliminado de forma automatica (Usuario presente: {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id})).";
                _substituteBuyerService.Delete(substituteBuyer);
            }

            if (model.Data != null)
                foreach (var data in model.Data)
                {
                    if (string.IsNullOrWhiteSpace(data.SelectedCustomerId)) continue;
                    var newSubstituteBuyer = new SubstituteBuyer()
                    {
                        SelectedShippingDate = dateDelivery,
                        CustomerId = data.BuyerId,
                        SubstituteCustomerId = int.Parse(data.SelectedCustomerId),
                        Log = $"Comprador substituto asignado por {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id})."
                    };
                    _substituteBuyerService.Insert(newSubstituteBuyer);
                }

            return NoContent();
        }
    }
}
