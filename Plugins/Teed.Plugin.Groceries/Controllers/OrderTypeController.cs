using Microsoft.AspNetCore.Mvc;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;
using Teed.Plugin.Groceries.Models.OrderType;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;

namespace Teed.Plugin.Groceries.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class OrderTypeController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly ShippingZoneService _shippingZoneService;
        private readonly OrderTypeService _orderTypeService;

        public OrderTypeController(IPermissionService permissionService,
            ShippingZoneService shippingZoneService,
            OrderTypeService orderTypeService,
            IWorkContext workContext,
            ICustomerService customerService,
            IOrderService orderService)
        {
            _permissionService = permissionService;
            _shippingZoneService = shippingZoneService;
            _workContext = workContext;
            _customerService = customerService;
            _orderService = orderService;
            _orderTypeService = orderTypeService;
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderType))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/OrderType/List.cshtml");
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderType))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/OrderType/Create.cshtml");
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderType))
                return AccessDeniedView();

            var orderType = _orderTypeService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (orderType == null) return NotFound();

            var model = new OrderTypeModel()
            {
                Id = orderType.Id,
                CargoSpace = orderType.CargoSpace,
                MaxProductQty = orderType.MaxProductQty,
                MinimumProductQty = orderType.MinimumProductQty,
                OrderTypeName = orderType.OrderTypeName
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/OrderType/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult Create(OrderTypeModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderType))
                return AccessDeniedView();

            if (!ModelState.IsValid) return View(model);

            var orderType = new OrderType()
            {
                CargoSpace = model.CargoSpace,
                MaxProductQty = model.MaxProductQty,
                MinimumProductQty = model.MinimumProductQty,
                OrderTypeName = model.OrderTypeName
            };
            _orderTypeService.Insert(orderType);

            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult Edit(OrderTypeModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderType))
                return AccessDeniedView();

            if (!ModelState.IsValid) return View(model);

            var orderType = _orderTypeService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (orderType == null) return NotFound();

            orderType.MaxProductQty = model.MaxProductQty;
            orderType.MinimumProductQty = model.MinimumProductQty;
            orderType.OrderTypeName = model.OrderTypeName;
            orderType.CargoSpace = model.CargoSpace;

            _orderTypeService.Update(orderType);

            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderType))
                return AccessDeniedView();

            var query = _orderTypeService.GetAll();
            var queryList = query.OrderByDescending(m => m.CreatedOnUtc).ToList();
            var pagedList = new PagedList<OrderType>(queryList, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.OrderTypeName,
                    x.CargoSpace,
                    x.MaxProductQty,
                    x.MinimumProductQty,
                    x.Id
                }).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }
    }
}
