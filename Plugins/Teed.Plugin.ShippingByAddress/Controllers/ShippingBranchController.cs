using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Messages;
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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.ShippingByAddress.Domain.ShippingBranch;
using Teed.Plugin.ShippingByAddress.Domain.ShippingByAddressD;
using Teed.Plugin.ShippingByAddress.Models.ShippingBranch;
using Teed.Plugin.ShippingByAddress.Services;

namespace Teed.Plugin.ShippingByAddress.Controllers
{
    [Area(AreaNames.Admin)]
    public class ShippingBranchController : BasePluginController
    {
        private const string GOOGLE_MAPS_API_KEY = "AIzaSyAUvh0c7WBIqAeMnpGnx09MKsYQOlHkJJw";

        private readonly IPermissionService _permissionService;
        private readonly ShippingByAddressService _shippingByAddressService;
        private readonly ShippingBranchService _shippingBranchService;
        private readonly ShippingBranchOrderService _shippingBranchOrderService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;

        public ShippingBranchController(IPermissionService permissionService,
            ShippingByAddressService shippingByAddressService,
            ShippingBranchService shippingBranchService,
            ShippingBranchOrderService shippingBranchOrderService,
            IWorkflowMessageService workflowMessageService,
            IOrderService orderService,
            IWorkContext workContext,
            LocalizationSettings localizationSettings)
        {
            _permissionService = permissionService;
            _shippingByAddressService = shippingByAddressService;
            _shippingBranchService = shippingBranchService;
            _shippingBranchOrderService = shippingBranchOrderService;
            _workflowMessageService = workflowMessageService;
            _orderService = orderService;
            _workContext = workContext;
            _localizationSettings = localizationSettings;
        }

        [AuthorizeAdmin]
        public IActionResult Index()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.ShippingByAddress/Views/ShippingBranch/Index.cshtml");
        }

        [AuthorizeAdmin]
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.ShippingByAddress/Views/ShippingBranch/Create.cshtml");
        }

        [AuthorizeAdmin]
        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var branch = _shippingBranchService.GetById(id).FirstOrDefault();
            EditViewModel model = new EditViewModel
            {
                Id = branch.Id,
                BranchEmail = branch.BranchEmail,
                BranchName = branch.BranchName,
                BranchPhone = branch.BranchPhone,
                ShouldSendEmail = branch.ShouldSendEmail
            };

            return View("~/Plugins/Teed.Plugin.ShippingByAddress/Views/ShippingBranch/Edit.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        public async Task<IActionResult> Create(CreateViewModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            try
            {
                ShippingBranch shippingBranch = new ShippingBranch
                {
                    BranchEmail = model.BranchEmail,
                    BranchName = model.BranchName,
                    BranchPhone = model.BranchPhone,
                    ShouldSendEmail = model.ShouldSendEmail
                };
                _shippingBranchService.Insert(shippingBranch);
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.Message);
            }

            // If we get here something went wrong
            return View("~/Plugins/Teed.Plugin.ShippingByAddress/Views/ShippingBranch/Create.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        public async Task<IActionResult> Edit(EditViewModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var shippingBranch = _shippingBranchService.GetById(model.Id).FirstOrDefault();
            if (shippingBranch != null)
            {
                try
                {
                    shippingBranch.BranchEmail = model.BranchEmail;
                    shippingBranch.BranchName = model.BranchName;
                    shippingBranch.BranchPhone = model.BranchPhone;
                    shippingBranch.ShouldSendEmail = model.ShouldSendEmail;
                    _shippingBranchService.Update(shippingBranch);
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.Message);
                }
            }

            // If we get here something went wrong
            return View("~/Plugins/Teed.Plugin.ShippingByAddress/Views/ShippingBranch/Edit.cshtml", model);
        }

        [HttpGet]
        [AuthorizeAdmin]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var shippingBranch = _shippingBranchService.GetById(id).FirstOrDefault();
            if (shippingBranch == null) return NotFound();
            _shippingBranchService.Delete(shippingBranch);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetBranches()
        {
            var branches = _shippingBranchService.GetAll().ToList();
            return Json(branches);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetBranchesSelect()
        {
            var branchesDb = _shippingBranchService.GetAll().ToList();
            var branches = branchesDb.Select(x => new
            {
                Id = x.Id.ToString(),
                Name = x.BranchName
            });
            var final = new SelectList(branches, "Id", "Name");
            return Json(final);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult UpdateShippingBranchOrder(int orderId, int branchId)
        {
            if (orderId < 1 || branchId < 1)
                return BadRequest();
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return BadRequest();
            var branch = _shippingBranchService.GetById(branchId).FirstOrDefault();
            if (branch == null)
                return BadRequest();
            var branchOrder = _shippingBranchOrderService.GetAll()
                .Where(x => x.OrderId == orderId).FirstOrDefault();
            if (branchOrder != null)
            {
                branchOrder.ShippingBranchId = branchId;
                _shippingBranchOrderService.Update(branchOrder);
                return Ok();
            }
            return BadRequest();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetShippingBranchOrder(int orderId)
        {
            if (orderId < 1)
                return BadRequest();
            var branchOrder = _shippingBranchOrderService.GetAll()
                .Where(x => x.OrderId == orderId).FirstOrDefault();
            if (branchOrder != null)
                return Ok(branchOrder.ShippingBranchId);
            return Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetOrderIdsByBranch(int branchId)
        {
            if (branchId < 1)
                return Ok();
            var branchOrder = _shippingBranchOrderService.GetAll()
                .Where(x => x.ShippingBranchId == branchId).ToList();
            if (branchOrder != null)
                return Json(branchOrder.Select(x => x.OrderId).ToList());
            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult SetShippingBranchOrder([FromBody] ShippingBranchModel model)
        {
            if (model.orderId < 1)
                return Ok();
            var shipping = _shippingByAddressService.GetByPostalCode(model.postalCode).FirstOrDefault();
            if (shipping == null)
                return Ok();
            var branch = _shippingBranchService.GetAll()
                .Where(x => x.Id == shipping.ShippingBranchId).FirstOrDefault();
            if (branch == null)
                return Ok();
            var branchOrder = _shippingBranchOrderService.GetAll()
                .Where(x => x.OrderId == model.orderId).FirstOrDefault();
            if (branchOrder == null)
                _shippingBranchOrderService.Insert(new ShippingBranchOrder
                {
                    OrderId = model.orderId,
                    ShippingBranchId = branch.Id
                });
            if (branch.ShouldSendEmail)
            {
                var order = _orderService.GetOrderById(model.orderId);
                if (order != null)
                {
                    //send email notifications
                    var orderPlacedStoreOwnerNotificationQueuedEmailId = _workflowMessageService.SendOrderPlacedStoreOwnerNotification(order, _localizationSettings.DefaultAdminLanguageId, branch.BranchEmail);
                    if (orderPlacedStoreOwnerNotificationQueuedEmailId > 0)
                    {
                        order.OrderNotes.Add(new OrderNote
                        {
                            Note = $"\"Order placed\" email (to store owner) has been queued. Queued email identifier: {orderPlacedStoreOwnerNotificationQueuedEmailId}.",
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                        _orderService.UpdateOrder(order);
                    }
                }
            }

            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult GetBranchNamesByOrderIds([FromBody] ShippingBranchNameModel model)
        {
            var names = new List<string>();
            if (model.orderIds.Count() > 0)
            {
                var branches = _shippingBranchService.GetAll().ToList();
                var branchOrders = _shippingBranchOrderService.GetAll()
                    .Where(x => model.orderIds.Contains(x.OrderId)).ToList();
                branchOrders = branchOrders.OrderBy(x => model.orderIds.IndexOf(x.OrderId)).ToList();
                foreach (var orderId in model.orderIds)
                {
                    var branchOrder = branchOrders.Where(x => x.OrderId == orderId).FirstOrDefault();
                    var name = branches.Where(x => x.Id == branchOrder?.ShippingBranchId)
                        .FirstOrDefault() == null ? " " :
                        branches.Where(x => x.Id == branchOrder?.ShippingBranchId)
                        .FirstOrDefault().BranchName;
                    names.Add(name);
                }
                return Json(names);
            }
            return Ok();
        }

        [AuthorizeAdmin]
        public IActionResult ListData()
        {
            var branches = _shippingBranchService.GetAll().ToList();
            var gridModel = new DataSourceResult
            {
                Data = branches.Select(x => new
                {
                    x.Id,
                    x.BranchName,
                    x.BranchEmail,
                    x.BranchPhone,
                    x.ShouldSendEmail
                }),
                Total = branches.Count()
            };
            return Json(gridModel);
        }
    }

    public class ShippingBranchModel
    {
        public int orderId { get; set; }
        public string postalCode { get; set; }
    }

    public class ShippingBranchNameModel
    {
        public List<int> orderIds { get; set; }
    }
}