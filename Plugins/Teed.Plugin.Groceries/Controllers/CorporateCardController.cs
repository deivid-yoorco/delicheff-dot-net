using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
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
using Teed.Plugin.Groceries.Domain.BuyerPayments;
using Teed.Plugin.Groceries.Extensions;
using Teed.Plugin.Groceries.Helpers;
using Teed.Plugin.Groceries.Models.CategoryPrice;
using Teed.Plugin.Groceries.Models.CorporateCard;
using Teed.Plugin.Groceries.Models.External;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class CorporateCardController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly ICategoryService _categoryService;
        private readonly CorporateCardService _corporateCardService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly NotDeliveredOrderItemService _notDeliveredOrderItemService;
        private readonly OrderItemBuyerService _orderItemBuyerService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICustomerService _customerService;
        private readonly ProductMainManufacturerService _productMainManufacturerService;
        private readonly IProductService _productService;

        public CorporateCardController(IPermissionService permissionService, ICategoryService categoryService,
            CorporateCardService corporateCardService, IStoreContext storeContext,
            IWorkContext workContext, IOrderService orderService,
            NotDeliveredOrderItemService notDeliveredOrderItemService, OrderItemBuyerService orderItemBuyerService,
            IManufacturerService manufacturerService, ICustomerService customerService,
            ProductMainManufacturerService productMainManufacturerService, IProductService productService)
        {
            _permissionService = permissionService;
            _categoryService = categoryService;
            _corporateCardService = corporateCardService;
            _storeContext = storeContext;
            _workContext = workContext;
            _orderService = orderService;
            _notDeliveredOrderItemService = notDeliveredOrderItemService;
            _orderItemBuyerService = orderItemBuyerService;
            _manufacturerService = manufacturerService;
            _customerService = customerService;
            _productMainManufacturerService = productMainManufacturerService;
            _productService = productService;
        }

        public void PrepareModel(CorporateCardModel model)
        {
            model.Statuses = Enum.GetValues(typeof(CorporateCardStatus)).Cast<CorporateCardStatus>().Select(v => new SelectListItem
            {
                Text = v.GetDisplayName(),
                Value = ((int)v).ToString()
            }).OrderBy(x => x.Value).ToList();

            model.Rules = Enum.GetValues(typeof(RuleType)).Cast<RuleType>().Select(v => new SelectListItem
            {
                Text = v.GetDisplayName(),
                Value = ((int)v).ToString()
            }).OrderBy(x => x.Value).ToList();
        }

        [HttpPost]
        public IActionResult GetExternalEmployees()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.CorporateCard))
                return AccessDeniedView();

            return Json(ExternalHelper.GetExternalPayrollEmployeesByIds(new List<int>(), _storeContext)
                .Select(x => new { x.Id, Name = x.FullName }).ToList());
        }

        public string SplitInParts(string s, int partLength)
        {
            var final = new List<string>();
            for (var i = 0; i < s.Length; i += partLength)
                 final.Add(s.Substring(i, Math.Min(partLength, s.Length - i)));
            return string.Join(" ", final);
        }

        public IActionResult Index()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.CorporateCard))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/CorporateCard/Index.cshtml");
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.CorporateCard))
                return AccessDeniedView();

            try
            {
                var cards = _corporateCardService.GetAll().OrderByDescending(x => x.UpdatedOnUtc);
                var employeeIds = cards.Select(x => x.EmployeeId).ToList();
                var employees = ExternalHelper.GetExternalPayrollEmployeesByIds(employeeIds, _storeContext);

                var pagedList = new PagedList<CorporateCard>(cards, command.Page - 1, command.PageSize);

                var gridModel = new DataSourceResult
                {
                    Data = pagedList.Select(x => new
                    {
                        x.Id,
                        x.EmployeeId,
                        EmployeeName = employees.Where(y => y.Id == x.EmployeeId).FirstOrDefault()?.FullName ?? "Sin especificar",
                        CardNumber = SplitInParts(x.CardNumber, 4),
                        Status = ((CorporateCardStatus)x.StatusId).GetDisplayName(),
                        Rule = ((RuleType)x.RuleId).GetDisplayName(),
                    }).ToList(),
                    Total = cards.Count()
                };

                return Ok(gridModel);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.CorporateCard))
                return AccessDeniedView();

            var model = new CorporateCardModel();
            PrepareModel(model);
            return View("~/Plugins/Teed.Plugin.Groceries/Views/CorporateCard/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(CorporateCardModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.CorporateCard))
                return AccessDeniedView();

            var card = new CorporateCard
            {
                CardNumber = model.CardNumber,
                EmployeeId = model.EmployeeId,
                RuleId = model.RuleId,
                StatusId = model.StatusId,
                Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) creó la nueva tarjeta corporativa con número \"{model.CardNumber}\".\n"
            };

            _corporateCardService.Insert(card);

            if (continueEditing)
            {
                return RedirectToAction("Edit", new { id = card.Id });
            }
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.CorporateCard))
                return AccessDeniedView();

            var card = _corporateCardService.GetAll()
                .Where(x => x.Id == id).FirstOrDefault();
            if (card == null)
                return RedirectToAction("Index");

            var model = new CorporateCardModel();
            PrepareModel(model);
            model.Id = card.Id;
            model.EmployeeId = card.EmployeeId;
            model.CardNumber = card.CardNumber;
            model.RuleId = card.RuleId;
            model.StatusId = card.StatusId;
            model.Log = card.Log;

            var employee = ExternalHelper.GetExternalPayrollEmployeesByIds(new List<int> { card.EmployeeId }, _storeContext).FirstOrDefault();
            if (employee != null)
            {
                model.FullName = employee.FullName;
                model.Rfc = employee.Rfc;
                model.Curp = employee.Curp;
                model.Phone = employee.Phone;
                model.Job = employee.Job;
            }

            return View("~/Plugins/Teed.Plugin.Groceries/Views/CorporateCard/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(CorporateCardModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.CorporateCard))
                return AccessDeniedView();

            var card = _corporateCardService.GetAll()
                .Where(x => x.Id == model.Id).FirstOrDefault();
            if (card == null)
                return RedirectToAction("Index");

            var employees = ExternalHelper.GetExternalPayrollEmployeesByIds(new List<int> { card.EmployeeId, model.EmployeeId }.Distinct().ToList(), _storeContext);

            if (model.CardNumber != card.CardNumber)
            {
                card.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el número de tarjeta de \"{card.CardNumber}\" a \"{model.CardNumber}\".\n";
                card.CardNumber = model.CardNumber;
            }

            if (model.EmployeeId != card.EmployeeId)
            {
                var oldEmployee = employees.Where(x => x.Id == card.EmployeeId).FirstOrDefault();
                var newEmployee = employees.Where(x => x.Id == model.EmployeeId).FirstOrDefault();
                card.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el empleado vinculado \"{(oldEmployee != null ? oldEmployee.FullName + $" ({oldEmployee.Id})" : "Sin especificar")}\" a \"{(newEmployee != null ? newEmployee.FullName + $" ({newEmployee.Id})" : "Sin especificar")}\".\n";
                card.EmployeeId = model.EmployeeId;
            }

            if (model.StatusId != card.StatusId)
            {
                card.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el estatus de \"{((CorporateCardStatus)card.StatusId).GetDisplayName()}\" a \"{((CorporateCardStatus)model.StatusId).GetDisplayName()}\".\n";
                card.EmployeeId = model.EmployeeId;
            }

            if (model.RuleId != card.RuleId)
            {
                card.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el estatus de \"{((RuleType)card.RuleId).GetDisplayName()}\" a \"{((RuleType)model.RuleId).GetDisplayName()}\".\n";
                card.RuleId = model.RuleId;
            }

            _corporateCardService.Update(card);

            if (continueEditing)
            {
                return RedirectToAction("Edit", new { id = card.Id });
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.CorporateCard))
                return AccessDeniedView();

            var card = _corporateCardService.GetAll()
                .Where(x => x.Id == id).FirstOrDefault();
            if (card == null)
                return RedirectToAction("Index");

            card.Deleted = true;
            card.UpdatedOnUtc = DateTime.UtcNow;
            _corporateCardService.Update(card);

            return RedirectToAction("Index");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.CorporateCard))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/CorporateCard/List.cshtml");
        }

        [HttpPost]
        public IActionResult DateListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.CorporateCard))
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

        public IActionResult CorporateCardDashboard(string date)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.CorporateCard))
                return AccessDeniedView();

            var controlDate = DateTime.Now.Date;
            if (!string.IsNullOrWhiteSpace(date))
                controlDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate == controlDate).ToList();
            var orderIds = orders.Select(x => x.Id).ToList();
            var notDeliveredOrderItems = _notDeliveredOrderItemService.GetAll().Where(x => orderIds.Contains(x.OrderId)).ToList();

            var productsIdsWithCorporateCardManufacturer = _manufacturerService.GetProductManufacturers()
                .Where(x => x.Manufacturer.IsPaymentWhithCorporateCard)
                .Select(x => x.ProductId)
                .ToList();
            List<OrderItem> parsedOrderItems = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService, notDeliveredOrderItems, false, true)
                .Where(x => productsIdsWithCorporateCardManufacturer.Contains(x.ProductId))
                .ToList();
            List<int> orderItemIdsOfTheDay = parsedOrderItems.Select(x => x.Id).ToList();
            var orderItemBuyers = _orderItemBuyerService.GetAll().Where(x => orderItemIdsOfTheDay.Contains(x.OrderItemId)).ToList();
            var buyerRole = _customerService.GetCustomerRoleBySystemName("buyer");
            var buyerIds = _customerService.GetAllCustomersQuery()
                .Where(x => x.CustomerRoles.Select(y => y.Id).Contains(buyerRole.Id))
                .Select(x => x.Id)
                .ToList();
            var customers = _customerService.GetAllCustomersQuery().Where(x => buyerIds.Contains(x.Id)).ToList();
            var mainManufacturers = _productMainManufacturerService.GetAll().ToList();
            var manufacturerGroup = parsedOrderItems
                .GroupBy(x => ProductUtils.GetMainManufacturerId(x.Product.ProductManufacturers, mainManufacturers.Where(y => y.ProductId == x.ProductId).FirstOrDefault()))
                .ToList();
            var manufacturers = _manufacturerService.GetAllManufacturers().ToList();
            var productMainManufacturers = _productMainManufacturerService.GetAll().ToList();

            var productsIdsWithCardManufacturer = _manufacturerService.GetProductManufacturers()
                .Where(x => x.Manufacturer.IsPaymentWhithCorporateCard)
                .Select(x => x.ProductId)
                .ToList();

            var corporateCards = _corporateCardService.GetAll().ToList();
            var employeeIds = corporateCards.Select(x => x.EmployeeId).ToList();
            var employees = ExternalHelper.GetExternalPayrollEmployeesByIds(employeeIds, _storeContext);

            var model = new List<BuyerPaymentModel>();
            foreach (var group in manufacturerGroup)
            {
                if (!manufacturers.Where(x => x.Id == group.Key).Select(x => x.IsPaymentWhithCorporateCard).FirstOrDefault()) continue;
                var itemIds = group.Select(x => x.Id).ToList();
                var filteredOrderItems = orderItemBuyers.Where(x => itemIds.Contains(x.OrderItemId)).ToList();
                var buyerId = filteredOrderItems.Select(x => x.CustomerId).FirstOrDefault();
                var buyerMoney = OrderUtils.GetBuyerCashAndTransferAmount(group.Select(x => x).ToList(), buyerId, orderItemBuyers, manufacturers, productMainManufacturers);
                if (buyerMoney.Card == 0) continue;

                var cardNumber = string.Empty;
                var payrollOfUser = employees.Where(x => x.CustomerId == buyerId).FirstOrDefault();
                if (payrollOfUser != null)
                    cardNumber = SplitInParts(corporateCards.Where(x => x.EmployeeId == payrollOfUser.Id).FirstOrDefault()?.CardNumber ?? string.Empty, 4);
                model.Add(new BuyerPaymentModel()
                {
                    CorporateCard = cardNumber,
                    ProjectedAmount = buyerMoney.Card,
                    RequestedAmount = 0,
                    BuyerPaymentId = 0,
                    BuyerId = buyerMoney.BuyerId,
                    Date = controlDate,
                    InvoiceFileXmlId = 0,
                    InvoiceFilePdfId = 0,
                    ManufacturerId = group.Key,
                    ManufacturerName = manufacturers.Where(x => x.Id == group.Key).Select(x => x.Name).FirstOrDefault(),
                    PaymentFileId = 0,
                    PaymentStatus = BuyerPaymentStatus.Pending,
                    TicketBuyerFileIds = new List<int>(),
                    BuyerName = customers.Where(x => x.Id == buyerId).FirstOrDefault()?.GetFullName()
                });
            }

            //// REQUESTED PAYMENTS HERE
            //var paymentRequest = _buyerPaymentService.GetAll().Where(x => x.ShippingDate == controlDate).ToList();
            //var buyerPaymentIds = paymentRequest.Select(x => x.Id).ToList();
            //var ticketBuyerFiles = _buyerPaymentTicketFileService.GetAll()
            //    .Where(x => buyerPaymentIds.Contains(x.BuyerPaymentId))
            //    .Select(x => new { x.FileId, x.BuyerPaymentId })
            //    .ToList();
            //foreach (var item in paymentRequest)
            //{
            //    var currentProjected = model
            //        .Where(x => x.ManufacturerId == item.ManufacturerId)
            //        .Select(x => x.ProjectedAmount)
            //        .FirstOrDefault();
            //    model.RemoveAll(x => x.ManufacturerId == item.ManufacturerId && x.RequestedAmount == 0);
            //    model.Add(new BuyerPaymentModel()
            //    {
            //        ProjectedAmount = currentProjected,
            //        CreationDate = item.CreatedOnUtc.ToLocalTime(),
            //        RequestedAmount = item.RequestedAmount,
            //        BuyerId = item.BuyerId,
            //        BuyerPaymentId = item.Id,
            //        Date = controlDate,
            //        InvoiceFilePdfId = item.InvoiceFilePdfId,
            //        InvoiceFileXmlId = item.InvoiceFileXmlId,
            //        ManufacturerId = item.ManufacturerId,
            //        ManufacturerName = manufacturers.Where(x => x.Id == item.ManufacturerId).Select(x => x.Name).FirstOrDefault(),
            //        PaymentFileId = item.PaymentFileId,
            //        PaymentStatus = (BuyerPaymentStatus)item.PaymentStatusId,
            //        BuyerName = customers.Where(x => x.Id == item.BuyerId).FirstOrDefault()?.GetFullName(),
            //        TicketBuyerFileIds = ticketBuyerFiles
            //        .Where(x => x.BuyerPaymentId == item.Id)
            //        .Select(x => x.FileId)
            //        .ToList()
            //    });
            //}
            //// REQUESTED PAYMENTS HERE

            //var pendingModel = new List<BuyerPaymentModel>();
            //var pendingPreviousDays = _buyerPaymentService.GetAll()
            //    .Where(x => x.ShippingDate < controlDate && (x.PaymentStatusId == 10 || (x.PaymentStatusId == 20 && (x.InvoiceFilePdfId == 0 || x.InvoiceFileXmlId == 0 || x.PaymentFileId == 0))))
            //    .ToList();
            //var pendingPreviousDaysIds = pendingPreviousDays.Select(x => x.Id).ToList();
            //var pendingFiles = _buyerPaymentTicketFileService.GetAll().Where(x => pendingPreviousDaysIds.Contains(x.BuyerPaymentId)).ToList();
            //var pendingBuyerIds = pendingPreviousDays.Select(y => y.BuyerId).ToList();
            //var pendingCustomers = _customerService.GetAllCustomersQuery().Where(x => pendingBuyerIds.Contains(x.Id)).ToList();
            //foreach (var item in pendingPreviousDays)
            //{
            //    pendingModel.Add(new BuyerPaymentModel()
            //    {
            //        ProjectedAmount = 0,
            //        BuyerPaymentId = item.Id,
            //        CreationDate = item.CreatedOnUtc.ToLocalTime(),
            //        RequestedAmount = item.RequestedAmount,
            //        BuyerId = item.BuyerId,
            //        Date = item.ShippingDate,
            //        InvoiceFilePdfId = item.InvoiceFilePdfId,
            //        InvoiceFileXmlId = item.InvoiceFileXmlId,
            //        ManufacturerId = item.ManufacturerId,
            //        ManufacturerName = manufacturers.Where(x => x.Id == item.ManufacturerId).Select(x => x.Name).FirstOrDefault(),
            //        PaymentFileId = item.PaymentFileId,
            //        PaymentStatus = (BuyerPaymentStatus)item.PaymentStatusId,
            //        BuyerName = pendingCustomers.Where(x => x.Id == item.BuyerId).FirstOrDefault()?.GetFullName(),
            //        TicketBuyerFileIds = pendingFiles
            //        .Where(x => x.BuyerPaymentId == item.Id)
            //        .Select(x => x.FileId)
            //        .ToList()
            //    });
            //}

            //var pendingBilling = pendingModel
            //    .Where(x => x.PaymentStatus == BuyerPaymentStatus.Payed && (x.InvoiceFilePdfId == 0 || x.InvoiceFileXmlId == 0))
            //    .GroupBy(x => x.ManufacturerId)
            //    .Select(x => new PendingBillingModel()
            //    {
            //        Manufacturer = x.Select(y => y.ManufacturerName).FirstOrDefault(),
            //        Buyer = x.Select(y => y.BuyerName).FirstOrDefault(),
            //        Count = x.Count()
            //    }).OrderBy(x => x.Manufacturer).ToList();

            var resultModel = new PaymentDashboardModel()
            {
                Date = controlDate,
                DatePayments = model.OrderBy(x => x.ManufacturerName).ToList(),
                //PendingPayments = pendingModel.OrderBy(x => x.ManufacturerName).ToList(),
                //PendingBillings = pendingBilling
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/CorporateCard/CorporateCardDashboard.cshtml", resultModel);
        }
    }
}