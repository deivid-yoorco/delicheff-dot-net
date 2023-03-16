using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.BuyerPayments;
using Teed.Plugin.Groceries.Extensions;
using Teed.Plugin.Groceries.Models.ManufacturerBankAccount;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class BuyerPaymentController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly ShippingRouteService _shippingRouteService;
        private readonly ShippingRouteUserService _shippingRouteUserService;
        private readonly IOrderService _orderService;
        private readonly FranchiseService _franchiseService;
        private readonly ISettingService _settingService;
        private readonly BillingService _billingService;
        private readonly PaymentFileService _paymentFileService;
        private readonly IncidentsService _incidentsService;
        private readonly PaymentService _paymentService;
        private readonly NotDeliveredOrderItemService _notDeliveredOrderItemService;
        private readonly OrderItemBuyerService _orderItemBuyerService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ProductMainManufacturerService _productMainManufacturerService;
        private readonly BuyerPaymentService _buyerPaymentService;
        private readonly BuyerPaymentTicketFileService _buyerPaymentTicketFileService;
        private readonly IPictureService _pictureService;
        private readonly BuyerPaymentByteFileService _buyerPaymentByteFileService;
        private readonly ManufacturerBankAccountService _manufacturerBankAccountService;

        public BuyerPaymentController(IPermissionService permissionService, ShippingRouteService shippingRouteService,
            ShippingRouteUserService shippingRouteUserService, IWorkContext workContext, ICustomerService customerService,
            IOrderService orderService, FranchiseService franchiseService, ISettingService settingService,
            BillingService billingService, PaymentFileService paymentFileService, IncidentsService incidentsService,
            PaymentService paymentService, NotDeliveredOrderItemService notDeliveredOrderItemService,
            IProductService productService, OrderItemBuyerService orderItemBuyerService,
            IManufacturerService manufacturerService, ProductMainManufacturerService productMainManufacturerService,
            BuyerPaymentService buyerPaymentService, BuyerPaymentTicketFileService buyerPaymentTicketFileService,
            IPictureService pictureService, BuyerPaymentByteFileService buyerPaymentByteFileService,
            ManufacturerBankAccountService manufacturerBankAccountService)
        {
            _manufacturerBankAccountService = manufacturerBankAccountService;
            _pictureService = pictureService;
            _permissionService = permissionService;
            _shippingRouteService = shippingRouteService;
            _shippingRouteUserService = shippingRouteUserService;
            _workContext = workContext;
            _customerService = customerService;
            _orderService = orderService;
            _franchiseService = franchiseService;
            _settingService = settingService;
            _billingService = billingService;
            _paymentFileService = paymentFileService;
            _incidentsService = incidentsService;
            _paymentService = paymentService;
            _notDeliveredOrderItemService = notDeliveredOrderItemService;
            _productService = productService;
            _orderItemBuyerService = orderItemBuyerService;
            _manufacturerService = manufacturerService;
            _productMainManufacturerService = productMainManufacturerService;
            _buyerPaymentService = buyerPaymentService;
            _buyerPaymentTicketFileService = buyerPaymentTicketFileService;
            _buyerPaymentByteFileService = buyerPaymentByteFileService;
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.BuyerPayment))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/BuyerPayment/List.cshtml");
        }

        public IActionResult BuyerPaymentDashboard(string date)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.BuyerPayment))
                return AccessDeniedView();

            var controlDate = DateTime.Now.Date;
            if (!string.IsNullOrWhiteSpace(date))
                controlDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate == controlDate).ToList();
            var orderIds = orders.Select(x => x.Id).ToList();
            var notDeliveredOrderItems = _notDeliveredOrderItemService.GetAll().Where(x => orderIds.Contains(x.OrderId)).ToList();

            var productsIdsWithTransferManufacturer = _manufacturerService.GetProductManufacturers()
                .Where(x => x.Manufacturer.IsPaymentWhithTransfer)
                .Select(x => x.ProductId)
                .ToList();
            List<OrderItem> parsedOrderItems = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService, notDeliveredOrderItems, false, true)
                .Where(x => productsIdsWithTransferManufacturer.Contains(x.ProductId))
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

            var model = new List<BuyerPaymentModel>();
            foreach (var group in manufacturerGroup)
            {
                if (!manufacturers.Where(x => x.Id == group.Key).Select(x => x.IsPaymentWhithTransfer).FirstOrDefault()) continue;
                var itemIds = group.Select(x => x.Id).ToList();
                var filteredOrderItems = orderItemBuyers.Where(x => itemIds.Contains(x.OrderItemId)).ToList();
                var buyerId = filteredOrderItems.Select(x => x.CustomerId).FirstOrDefault();
                var buyerMoney = OrderUtils.GetBuyerCashAndTransferAmount(group.Select(x => x).ToList(), buyerId, orderItemBuyers, manufacturers, productMainManufacturers);
                if (buyerMoney.Transfer == 0) continue;
                model.Add(new BuyerPaymentModel()
                {
                    ProjectedAmount = buyerMoney.Transfer,
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

            // REQUESTED PAYMENTS HERE
            var paymentRequest = _buyerPaymentService.GetAll().Where(x => x.ShippingDate == controlDate).ToList();
            var buyerPaymentIds = paymentRequest.Select(x => x.Id).ToList();
            var ticketBuyerFiles = _buyerPaymentTicketFileService.GetAll()
                .Where(x => buyerPaymentIds.Contains(x.BuyerPaymentId))
                .Select(x => new { x.FileId, x.BuyerPaymentId })
                .ToList();

            var paymentFileIds = paymentRequest.Select(x => x.PaymentFileId).Where(x => x > 0).ToList();
            var paymentFiles = _buyerPaymentByteFileService.GetAll().Where(x => !x.Deleted && paymentFileIds.Contains(x.Id))
                .Select(x => new {
                    x.Id,
                    x.UpdatedOnUtc
                })
                .ToList();

            foreach (var item in paymentRequest)
            {
                var currentProjected = model
                    .Where(x => x.ManufacturerId == item.ManufacturerId)
                    .Select(x => x.ProjectedAmount)
                    .FirstOrDefault();
                model.RemoveAll(x => x.ManufacturerId == item.ManufacturerId && x.RequestedAmount == 0);
                var paymentFileUploadDate = DateTime.MinValue;
                if (item.PaymentFileId > 0)
                {
                    var file = paymentFiles.Where(x => x.Id == item.PaymentFileId).FirstOrDefault();
                    if (file != null)
                        paymentFileUploadDate = file.UpdatedOnUtc.ToLocalTime();
                }
                model.Add(new BuyerPaymentModel()
                {
                    ProjectedAmount = currentProjected,
                    CreationDate = item.CreatedOnUtc.ToLocalTime(),
                    RequestedAmount = item.RequestedAmount,
                    BuyerId = item.BuyerId,
                    BuyerPaymentId = item.Id,
                    Date = controlDate,
                    InvoiceFilePdfId = item.InvoiceFilePdfId,
                    InvoiceFileXmlId = item.InvoiceFileXmlId,
                    ManufacturerId = item.ManufacturerId,
                    ManufacturerName = manufacturers.Where(x => x.Id == item.ManufacturerId).Select(x => x.Name).FirstOrDefault(),
                    PaymentFileId = item.PaymentFileId,
                    PaymentStatus = (BuyerPaymentStatus)item.PaymentStatusId,
                    PaymentFileUploadDate = paymentFileUploadDate,
                    BuyerName = customers.Where(x => x.Id == item.BuyerId).FirstOrDefault()?.GetFullName(),
                    TicketBuyerFileIds = ticketBuyerFiles
                    .Where(x => x.BuyerPaymentId == item.Id)
                    .Select(x => x.FileId)
                    .ToList()
                });
            }
            // REQUESTED PAYMENTS HERE

            var pendingModel = new List<BuyerPaymentModel>();
            var pendingPreviousDays = _buyerPaymentService.GetAll()
                .Where(x => x.ShippingDate < controlDate && (x.PaymentStatusId == 10 || (x.PaymentStatusId == 20 && (x.InvoiceFilePdfId == 0 || x.InvoiceFileXmlId == 0 || x.PaymentFileId == 0))))
                .ToList();
            var pendingPreviousDaysIds = pendingPreviousDays.Select(x => x.Id).ToList();
            var pendingFiles = _buyerPaymentTicketFileService.GetAll().Where(x => pendingPreviousDaysIds.Contains(x.BuyerPaymentId)).ToList();
            var pendingBuyerIds = pendingPreviousDays.Select(y => y.BuyerId).ToList();
            var pendingCustomers = _customerService.GetAllCustomersQuery().Where(x => pendingBuyerIds.Contains(x.Id)).ToList();

            var pendingPaymentFileIds = pendingPreviousDays.Select(x => x.PaymentFileId).Where(x => x > 0).ToList();
            var pendingPaymentFiles = _buyerPaymentByteFileService.GetAll().Where(x => !x.Deleted && paymentFileIds.Contains(x.Id))
                .Select(x => new {
                    x.Id,
                    x.UpdatedOnUtc
                })
                .ToList();

            foreach (var item in pendingPreviousDays)
            {
                var paymentFileUploadDate = DateTime.MinValue;
                if (item.PaymentFileId > 0)
                {
                    var file = pendingPaymentFiles.Where(x => x.Id == item.PaymentFileId).FirstOrDefault();
                    if (file != null)
                        paymentFileUploadDate = file.UpdatedOnUtc.ToLocalTime();
                }
                pendingModel.Add(new BuyerPaymentModel()
                {
                    ProjectedAmount = 0,
                    BuyerPaymentId = item.Id,
                    CreationDate = item.CreatedOnUtc.ToLocalTime(),
                    RequestedAmount = item.RequestedAmount,
                    BuyerId = item.BuyerId,
                    Date = item.ShippingDate,
                    InvoiceFilePdfId = item.InvoiceFilePdfId,
                    InvoiceFileXmlId = item.InvoiceFileXmlId,
                    ManufacturerId = item.ManufacturerId,
                    ManufacturerName = manufacturers.Where(x => x.Id == item.ManufacturerId).Select(x => x.Name).FirstOrDefault(),
                    PaymentFileId = item.PaymentFileId,
                    PaymentStatus = (BuyerPaymentStatus)item.PaymentStatusId,
                    PaymentFileUploadDate = paymentFileUploadDate,
                    BuyerName = pendingCustomers.Where(x => x.Id == item.BuyerId).FirstOrDefault()?.GetFullName(),
                    TicketBuyerFileIds = pendingFiles
                    .Where(x => x.BuyerPaymentId == item.Id)
                    .Select(x => x.FileId)
                    .ToList()
                });
            }

            var pendingBilling = pendingModel
                .Where(x => x.PaymentStatus == BuyerPaymentStatus.Payed && (x.InvoiceFilePdfId == 0 || x.InvoiceFileXmlId == 0))
                .GroupBy(x => x.ManufacturerId)
                .Select(x => new PendingBillingModel()
                {
                    Manufacturer = x.Select(y => y.ManufacturerName).FirstOrDefault(),
                    Buyer = x.Select(y => y.BuyerName).FirstOrDefault(),
                    Count = x.Count()
                }).OrderBy(x => x.Manufacturer).ToList();

            var resultModel = new PaymentDashboardModel()
            {
                Date = controlDate,
                DatePayments = model.OrderBy(x => x.ManufacturerName).ToList(),
                PendingPayments = pendingModel.OrderBy(x => x.ManufacturerName).ToList(),
                PendingBillings = pendingBilling
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/BuyerPayment/BuyerPaymentDashboard.cshtml", resultModel);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.BuyerPayment))
                return AccessDeniedView();

            var buyerPayment = _buyerPaymentService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (buyerPayment == null) return NotFound();
            var files = _buyerPaymentTicketFileService.GetAll().Where(x => x.BuyerPaymentId == id).Select(x => x.FileId).ToList();
            var customer = _customerService.GetCustomerById(buyerPayment.BuyerId);
            var manufacturer = _manufacturerService.GetManufacturerById(buyerPayment.ManufacturerId);
            var manufacturerBankAccount = _manufacturerBankAccountService
                .GetAll()
                .Where(x => x.ManufacturerId == buyerPayment.ManufacturerId)
                .FirstOrDefault();

            var model = new BuyerPaymentModel()
            {
                RequestedAmount = buyerPayment.RequestedAmount,
                BuyerId = buyerPayment.BuyerId,
                BuyerPaymentId = id,
                Date = buyerPayment.ShippingDate,
                InvoiceFilePdfId = buyerPayment.InvoiceFilePdfId,
                InvoiceFileXmlId = buyerPayment.InvoiceFileXmlId,
                ManufacturerId = buyerPayment.ManufacturerId,
                PaymentFileId = buyerPayment.PaymentFileId,
                PaymentStatus = (BuyerPaymentStatus)buyerPayment.PaymentStatusId,
                PaymentStatusId = buyerPayment.PaymentStatusId,
                TicketBuyerFileIds = files,
                BuyerName = customer?.GetFullName(),
                ManufacturerName = manufacturer?.Name,
                CreationDate = buyerPayment.CreatedOnUtc.ToLocalTime(),
                Log = buyerPayment.Log,
                ManufacturerBankAccount = manufacturerBankAccount == null ? null : new ManufacturerBankAccountModel()
                {
                    AccountNumber = manufacturerBankAccount.AccountNumber,
                    BankName = manufacturerBankAccount.BankName,
                    AccountOwner = manufacturerBankAccount.AccountOwner
                },
                PaymentStatuses = Enum.GetValues(typeof(BuyerPaymentStatus)).Cast<BuyerPaymentStatus>()
                .Select(x => new SelectListItem
                {
                    Value = ((int)x).ToString(),
                    Text = x.GetDisplayName()
                }).ToList()
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/BuyerPayment/Edit.cshtml", model);
        }

        [HttpPost]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(BuyerPaymentModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.BuyerPayment))
                return AccessDeniedView();

            var buyerPayment = _buyerPaymentService.GetAll()
                .Where(x => x.Id == model.BuyerPaymentId).FirstOrDefault();
            if (buyerPayment == null)
                return RedirectToAction("BuyerPaymentDashboard");

            var changed = false;
            var fileIdsDelete = new List<int>();
            if (model.PaymentStatusId != buyerPayment.PaymentStatusId)
            {
                buyerPayment.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el estatus de pago de {((BuyerPaymentStatus)buyerPayment.PaymentStatusId).GetDisplayName()} a {((BuyerPaymentStatus)model.PaymentStatusId).GetDisplayName()}.\n";
                buyerPayment.PaymentStatusId = model.PaymentStatusId;
                changed = true;
            }
            if (model.PaymentFile != null)
            {
                try
                {
                    byte[] bytes = new byte[0];
                    using (var ms = new MemoryStream())
                    {
                        model.PaymentFile.CopyTo(ms);
                        bytes = ms.ToArray();
                    }
                    var file = new BuyerPaymentByteFile
                    {
                        FileBytes = bytes,
                        Extension = model.PaymentFile.FileName.Substring(model.PaymentFile.FileName.LastIndexOf(".") + 1),
                        MimeType = model.PaymentFile.ContentType,
                    };
                    _buyerPaymentByteFileService.Insert(file);
                    buyerPayment.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) agregó un comprobante de pago de tipo \"{file.MimeType}\" con Id \"{file.Id}\"{(buyerPayment.PaymentFileId > 0 ? " (Id anterior \"" + buyerPayment.PaymentFileId + "\")" : string.Empty)}.\n";
                    if (buyerPayment.PaymentFileId > 0)
                        fileIdsDelete.Add(buyerPayment.PaymentFileId);
                    buyerPayment.PaymentFileId = file.Id;
                    changed = true;
                }
                catch (Exception e) { var err = e; }
            }
            if (model.InvoiceFilePdf != null)
            {
                try
                {
                    byte[] bytes = new byte[0];
                    using (var ms = new MemoryStream())
                    {
                        model.InvoiceFilePdf.CopyTo(ms);
                        bytes = ms.ToArray();
                    }
                    var file = new BuyerPaymentByteFile
                    {
                        FileBytes = bytes,
                        Extension = ".pdf",
                        MimeType = "application/pdf",
                    };
                    _buyerPaymentByteFileService.Insert(file);
                    buyerPayment.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) agregó un pdf de facturación con Id \"{file.Id}\"{(buyerPayment.InvoiceFilePdfId > 0 ? " (Id anterior \"" + buyerPayment.InvoiceFilePdfId + "\")" : string.Empty)}.\n";
                    if (buyerPayment.InvoiceFilePdfId > 0)
                        fileIdsDelete.Add(buyerPayment.InvoiceFilePdfId);
                    buyerPayment.InvoiceFilePdfId = file.Id;
                    changed = true;
                }
                catch (Exception e) { var err = e; }
            }
            if (model.InvoiceFileXml != null)
            {
                try
                {
                    byte[] bytes = new byte[0];
                    using (var ms = new MemoryStream())
                    {
                        model.InvoiceFileXml.CopyTo(ms);
                        bytes = ms.ToArray();
                    }
                    var file = new BuyerPaymentByteFile
                    {
                        FileBytes = bytes,
                        Extension = ".xml",
                        MimeType = "application/xml",
                    };
                    _buyerPaymentByteFileService.Insert(file);
                    buyerPayment.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) agregó un xml de facturación con Id \"{file.Id}\"{(buyerPayment.InvoiceFileXmlId > 0 ? " (Id anterior \"" + buyerPayment.InvoiceFileXmlId + "\")" : string.Empty)}.\n";
                    if (buyerPayment.InvoiceFileXmlId > 0)
                        fileIdsDelete.Add(buyerPayment.InvoiceFileXmlId);
                    buyerPayment.InvoiceFileXmlId = file.Id;
                    changed = true;
                }
                catch (Exception e) { var err = e; }
            }
            if (changed)
                _buyerPaymentService.Update(buyerPayment);

            foreach (var fileId in fileIdsDelete)
            {
                var file = _buyerPaymentByteFileService.GetById(fileId);
                if (file != null)
                {
                    file.Deleted = true;
                    _buyerPaymentByteFileService.Update(file);
                }
            }

            if (continueEditing)
                return RedirectToAction("Edit", new { id = buyerPayment.Id });
            return RedirectToAction("BuyerPaymentDashboard");

        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.BuyerPayment))
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

        [HttpGet]
        public virtual IActionResult GetPicture(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.BuyerPayment))
                return AccessDeniedView();

            var picture = _pictureService.GetPictureById(id);
            byte[] bytes = null;
            string mimeType = null;

            if (picture == null)
            {
                using (WebClient client = new WebClient())
                {
                    bytes = client.DownloadData(_pictureService.GetDefaultPictureUrl());
                    mimeType = "image/png";
                }
            }
            else
            {
                bytes = picture.PictureBinary;
                mimeType = picture.MimeType;
            }

            return File(bytes, mimeType);
        }

        [HttpGet]
        public virtual IActionResult GetByteFile(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.BuyerPayment))
                return AccessDeniedView();

            var file = _buyerPaymentByteFileService.GetById(id);
            var buyerPayment = _buyerPaymentService.GetAll()
                .Where(x => x.PaymentFileId == id || x.InvoiceFilePdfId == id || x.InvoiceFileXmlId == id)
                .FirstOrDefault();

            if (file == null || buyerPayment == null)
                return Ok();
            else
            {
                var manufacturer = _manufacturerService.GetAllManufacturers().Where(x => x.Id == buyerPayment.ManufacturerId).FirstOrDefault();
                var buyer = _customerService.GetCustomerById(buyerPayment.BuyerId);

                return File(file.FileBytes, file.MimeType,
                    $"{(buyerPayment.PaymentFileId == id ? "Comprobante de pago" : buyerPayment.InvoiceFilePdfId == id ? "Factura PDF" : buyerPayment.InvoiceFileXmlId == id ? "Factura XML" : "")}"
                    + $" - {manufacturer?.Name ?? "NA"} - {(buyer != null ? buyer.GetFullName() : "NA")} - {buyerPayment.ShippingDate:dd-MM-yyyy}.{file.Extension}");
            }
        }
    }

    public class PaymentDashboardModel
    {
        public DateTime Date { get; set; }
        public List<BuyerPaymentModel> DatePayments { get; set; }
        public List<BuyerPaymentModel> PendingPayments { get; set; }
        public List<PendingBillingModel> PendingBillings { get; set; }
    }

    public class BuyerPaymentModel
    {
        public DateTime Date { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public int ManufacturerId { get; set; }
        public string ManufacturerName { get; set; }
        public decimal ProjectedAmount { get; set; }
        public decimal RequestedAmount { get; set; }
        public List<int> TicketBuyerFileIds { get; set; }
        public int PaymentFileId { get; set; }
        public IFormFile PaymentFile { get; set; }
        public int PaymentStatusId { get; set; }
        public BuyerPaymentStatus PaymentStatus { get; set; }
        public List<SelectListItem> PaymentStatuses { get; set; }
        public DateTime PaymentFileUploadDate { get; set; }
        public int InvoiceFileXmlId { get; set; }
        public IFormFile InvoiceFilePdf { get; set; }
        public int InvoiceFilePdfId { get; set; }
        public IFormFile InvoiceFileXml { get; set; }
        public DateTime CreationDate { get; set; }
        public int BuyerPaymentId { get; set; }
        public ManufacturerBankAccountModel ManufacturerBankAccount { get; set; }
        public string CorporateCard { get; set; }
        public string Log { get; set; }
    }

    public class PendingBillingModel
    {
        public string Manufacturer { get; set; }
        public string Buyer { get; set; }
        public int Count { get; set; }
    }
}
