using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.MarkedBuyerItems;
using Teed.Plugin.Groceries.Domain.OrderReports;
using Teed.Plugin.Groceries.Dtos;
using Teed.Plugin.Groceries.Extensions;
using Teed.Plugin.Groceries.Models.OrderReport;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;
using Teed.Plugin.Manager.Models.Groceries;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class OrderReportController : BasePluginController
    {
        private readonly IOrderService _orderService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly Services.OrderReportService _orderReportService;
        private readonly OrderReportLogService _orderReportLogService;
        private readonly OrderReportFileService _orderReportFileService;
        private readonly ShippingRouteService _shippingRouteService;
        private readonly ShippingRouteUserService _shippingRouteUserService;

        private readonly OrderItemBuyerService _orderItemBuyerService;
        private readonly BuyerListDownloadService _buyerListDownloadService;
        private readonly OrderReportStatusService _orderReportStatusService;
        private readonly OrderItemLogService _orderItemLogService;
        private readonly NotDeliveredOrderItemService _notDeliveredOrderItemService;
        private readonly TipsWithCardService _tipsWithCardService;
        private readonly ShippingAreaService _shippingAreaService;
        private readonly MarkedBuyerItemService _markedBuyerItemService;
        private readonly OrderReportTransferService _orderReportTransferService;
        private readonly SupermarketBuyerService _supermarketBuyerService;
        private readonly ProductMainManufacturerService _productMainManufacturerService;
        private readonly SubstituteBuyerService _substituteBuyerService;
        private readonly BuyerPaymentService _buyerPaymentService;
        private readonly BuyerPaymentTicketFileService _buyerPaymentTicketFileService;
        private readonly BuyerPaymentByteFileService _buyerPaymentByteFileService;
        private readonly IPictureService _pictureService;

        public OrderReportController(IOrderService orderService, IPermissionService permissionService, IWorkContext workContext,
            Services.OrderReportService orderReportService, OrderReportLogService orderReportLogService, IProductService productService,
            ICustomerService customerService, OrderReportFileService orderReportFileService, ShippingRouteService shippingRouteService,
            ShippingRouteUserService shippingRouteUserService, OrderItemBuyerService orderItemBuyerService,
            BuyerListDownloadService buyerListDownloadService, OrderReportStatusService orderReportStatusService,
            OrderItemLogService orderItemLogService, NotDeliveredOrderItemService notDeliveredOrderItemService,
            TipsWithCardService tipsWithCardService, ShippingAreaService shippingAreaService,
            MarkedBuyerItemService markedBuyerItemService, OrderReportTransferService orderReportTransferService,
            SupermarketBuyerService supermarketBuyerService, ProductMainManufacturerService productMainManufacturerService,
            SubstituteBuyerService substituteBuyerService, BuyerPaymentService buyerPaymentService,
            BuyerPaymentTicketFileService buyerPaymentTicketFileService, BuyerPaymentByteFileService buyerPaymentByteFileService,
            IPictureService pictureService)
        {
            _orderService = orderService;
            _permissionService = permissionService;
            _workContext = workContext;
            _orderReportService = orderReportService;
            _orderReportLogService = orderReportLogService;
            _productService = productService;
            _customerService = customerService;
            _orderReportFileService = orderReportFileService;
            _shippingRouteService = shippingRouteService;
            _shippingRouteUserService = shippingRouteUserService;

            _orderItemBuyerService = orderItemBuyerService;
            _buyerListDownloadService = buyerListDownloadService;
            _orderReportStatusService = orderReportStatusService;
            _orderItemLogService = orderItemLogService;
            _notDeliveredOrderItemService = notDeliveredOrderItemService;
            _tipsWithCardService = tipsWithCardService;
            _shippingAreaService = shippingAreaService;
            _markedBuyerItemService = markedBuyerItemService;
            _orderReportTransferService = orderReportTransferService;
            _supermarketBuyerService = supermarketBuyerService;
            _productMainManufacturerService = productMainManufacturerService;
            _substituteBuyerService = substituteBuyerService;
            _buyerPaymentService = buyerPaymentService;
            _buyerPaymentTicketFileService = buyerPaymentTicketFileService;
            _buyerPaymentByteFileService = buyerPaymentByteFileService;
            _pictureService = pictureService;
        }

        public IActionResult CreateOrderReport(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            Order order = _orderService.GetOrderById(id);
            if (order == null) return NotFound();

            ViewData["OrderItems"] = order.OrderItems.OrderBy(x => x.Product.ProductCategories?.OrderBy(y => y.Category.ParentCategoryId).FirstOrDefault()?.DisplayOrder).ToList();
            ViewData["OrderId"] = order.Id;

            return View("~/Plugins/Teed.Plugin.Groceries/Views/OrderReport/Create.cshtml");
        }

        public IActionResult EditOrderReport(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            Order order = _orderService.GetOrderById(id);
            if (order == null) return NotFound();

            EditOrderReportModel model = new EditOrderReportModel()
            {
                OrderId = order.Id,
                OrderItemsReport = _orderReportService.GetAll()
                .Where(x => x.OrderId == order.Id)
                .ToList()
                .Select(x => new EditOrderReportData()
                {
                    Comment = x.Comments,
                    RequestedQtyCost = x.RequestedQtyCost,
                    UnitCost = x.UnitCost,
                    Store = x.ShoppingStoreId,
                    OrderItem = _orderService.GetOrderItemById(x.OrderItemId)
                }).ToList(),
                LogData = _orderReportLogService.GetAll()
                .Where(x => x.OrderId == order.Id)
                .ToList()
                .Select(x => new OrderReportLogData()
                {
                    Log = x.Log,
                    Author = _customerService.GetCustomerById(x.AuthorId),
                    Date = x.CreatedOnUtc
                }).ToList(),
                OrderReportFiles = _orderReportFileService.GetAll().Where(x => x.OrderId == order.Id).Select(x => new OrderReportFileData()
                {
                    Name = x.FileName,
                    FileUrl = x.FileUrl
                }).ToList()
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/OrderReport/Edit.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> EditOrderReport(EditOrderReportModel model)
        {
            if (!ModelState.IsValid) return BadRequest("La información enviada no es válida");

            var log = string.Empty;
            foreach (var item in model.OrderItemsReport)
            {
                var itemLog = string.Empty;
                OrderReport orderReport = _orderReportService.GetAll().Where(x => x.OrderItemId == item.OrderItemId).FirstOrDefault();
                if (orderReport == null) return NotFound();

                if (orderReport.Comments != item.Comment)
                {
                    itemLog += $" Editó el comentario del producto {_orderService.GetOrderItemById(item.OrderItemId).Product.Name} de {orderReport.Comments} a {item.Comment}.";
                }

                if (orderReport.UnitCost != item.UnitCost)
                {
                    itemLog += $" Editó el costo por kg/pza del producto {_orderService.GetOrderItemById(item.OrderItemId).Product.Name} de {orderReport.UnitCost} a {item.UnitCost}.";
                }

                if (orderReport.RequestedQtyCost != item.RequestedQtyCost)
                {
                    itemLog += $" Editó el costo del pedido del producto {_orderService.GetOrderItemById(item.OrderItemId).Product.Name} de {orderReport.RequestedQtyCost} a {item.RequestedQtyCost}.";
                }

                if (orderReport.ShoppingStoreId != item.Store)
                {
                    itemLog += $" Editó la bodega del producto {_orderService.GetOrderItemById(item.OrderItemId).Product.Name} de {orderReport.ShoppingStoreId} a {item.Store}.";
                }

                if (!string.IsNullOrWhiteSpace(itemLog))
                {
                    log += itemLog;

                    orderReport.Comments = item.Comment;
                    orderReport.RequestedQtyCost = item.RequestedQtyCost;
                    orderReport.UnitCost = item.UnitCost;
                    orderReport.ShoppingStoreId = item.Store;

                    _orderReportService.Update(orderReport);
                }
            }

            if (model.OrderReportFiles?.Count > 0)
            {
                log += $" Agregó nuevos archivos.";

                foreach (var file in model.OrderReportFiles)
                {
                    var base64Parsed = file.Base64.Remove(0, file.Base64.IndexOf(',') + 1);
                    var bytes = Convert.FromBase64String(base64Parsed);
                    var contents = new StreamContent(new MemoryStream(bytes));

                    string baseDirectoryName = "orders-report-media";
                    string directoryPath = $"./wwwroot/{baseDirectoryName}/{model.OrderId}";
                    Directory.CreateDirectory(directoryPath);
                    string newFileName = $"{DateTime.Now.ToString("ddMMyyyyhhmmss")}-{file.Name}";
                    string fullPath = $"{directoryPath}/{newFileName}";

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await contents.CopyToAsync(stream);
                    }

                    OrderReportFile orderReportFile = new OrderReportFile()
                    {
                        FileName = file.Name,
                        FileType = file.Type,
                        OrderId = model.OrderId,
                        Size = file.Size,
                        UploadedByUserId = _workContext.CurrentCustomer.Id,
                        FileUrl = $"/{baseDirectoryName}/{model.OrderId}/{newFileName}",
                        Extension = Path.GetExtension(newFileName)
                    };
                    _orderReportFileService.Insert(orderReportFile);
                }
            }

            if (!string.IsNullOrWhiteSpace(log))
            {
                OrderReportLog orderReportLog = new OrderReportLog()
                {
                    AuthorId = _workContext.CurrentCustomer.Id,
                    OrderId = model.OrderId,
                    Log = log
                };
                _orderReportLogService.Insert(orderReportLog);
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrderReport(OrderReportModel model)
        {
            if (!ModelState.IsValid) return BadRequest("La información enviada no es válida");

            foreach (var item in model.OrderItemsReport)
            {
                OrderReport orderReport = new OrderReport()
                {
                    OrderId = model.OrderId,
                    Comments = item.Comment,
                    RequestedQtyCost = item.RequestedQtyCost,
                    UnitCost = item.UnitCost,
                    OrderItemId = item.OrderItemId,
                    ShoppingStoreId = item.Store
                };
                _orderReportService.Insert(orderReport);
            }

            OrderReportLog orderReportLog = new OrderReportLog()
            {
                OrderId = model.OrderId,
                Log = "Creó un reporte.",
                AuthorId = _workContext.CurrentCustomer.Id
            };
            _orderReportLogService.Insert(orderReportLog);

            if (model.OrderReportFiles != null)
            {
                foreach (var file in model.OrderReportFiles)
                {
                    var base64Parsed = file.Base64.Remove(0, file.Base64.IndexOf(',') + 1);
                    var bytes = Convert.FromBase64String(base64Parsed);
                    var contents = new StreamContent(new MemoryStream(bytes));

                    string baseDirectoryName = "orders-report-media";
                    string directoryPath = $"./wwwroot/{baseDirectoryName}/{model.OrderId}";
                    Directory.CreateDirectory(directoryPath);
                    string newFileName = $"{DateTime.Now.ToString("ddMMyyyyhhmmss")}-{file.Name}";
                    string fullPath = $"{directoryPath}/{newFileName}";

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await contents.CopyToAsync(stream);
                    }

                    OrderReportFile orderReportFile = new OrderReportFile()
                    {
                        FileName = file.Name,
                        FileType = file.Type,
                        OrderId = model.OrderId,
                        Size = file.Size,
                        UploadedByUserId = _workContext.CurrentCustomer.Id,
                        FileUrl = $"/{baseDirectoryName}/{model.OrderId}/{newFileName}",
                        Extension = Path.GetExtension(newFileName)
                    };
                    _orderReportFileService.Insert(orderReportFile);
                }
            }

            return NoContent();
        }

        [HttpGet]
        public List<GetOrderReportsModel> GetOrderReports(int customerId = 0)
        {
            var query = _orderReportService.GetAll();
            if (customerId > 0)
            {
                var userRoutes = _shippingRouteUserService.GetAll().Where(x => x.UserInChargeId == customerId).ToList();
                var ordersId = _orderService.GetOrders()
                    .Where(x => !x.Deleted)
                    .Where(x => userRoutes.Select(y => y.ShippingRouteId).Contains(x.RouteId))
                    .Where(x => userRoutes.Select(y => y.ResponsableDateUtc.ToLocalTime().Date).Contains(x.SelectedShippingDate.Value.Date))
                    .Select(x => x.Id)
                    .ToList();

                query = query.Where(x => ordersId.Contains(x.OrderId));
            }

            return query.ToList().Select(x => new GetOrderReportsModel()
            {
                Id = x.Id,
                Comments = x.Comments,
                OrderId = x.OrderId,
                OrderItemId = x.OrderItemId,
                OrderResponsableId = customerId > 0 ? customerId : GetResponsableId(_orderService.GetOrderById(x.OrderId)),
                RequestedQtyCost = x.RequestedQtyCost,
                ShoppingStoreId = x.ShoppingStoreId,
                UnitCost = x.UnitCost,
                CreatedOnUtc = x.CreatedOnUtc,
                FilesUrl = _orderReportFileService.GetAll().Where(y => y.OrderId == x.OrderId).Select(y => y.FileUrl).ToList()
            }).ToList();
        }

        [HttpGet]
        public decimal GetExpensesByReports(DateTime date)
        {
            var orderIds = _orderService.GetOrders().Where(x => x.SelectedShippingDate.Value.Month == date.Month && x.SelectedShippingDate.Value.Year == date.Year).Select(x => x.Id);
            decimal? sum = _orderReportService.GetAll().Where(x => orderIds.Contains(x.OrderId)).Select(x => x.RequestedQtyCost).DefaultIfEmpty().Sum();
            return sum ?? 0;
        }

        public int GetResponsableId(Order order)
        {
            return _shippingRouteUserService.GetAll()
                .Where(x => x.ShippingRouteId == order.RouteId)
                .ToList()
                .Where(x => x.ResponsableDateUtc.ToLocalTime().Date == order.SelectedShippingDate.Value.Date)
                .Select(x => x.UserInChargeId)
                .FirstOrDefault();
        }

        [HttpGet]
        public bool OrderHasReport(int id)
        {
            return _orderReportService.GetAll().Where(x => x.OrderId == id).Any();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetReportProducts3Test(string date)
        {
            var parsedDate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture).Date;
            int userId = 3898282;

            var orderIetmsNotContains = _orderItemBuyerService.GetAll().Where(x => !x.Deleted && x.SelectedShippingDate == parsedDate && x.CustomerId == 0);
            bool notContains = orderIetmsNotContains.Any();
            bool buyerDownload = _buyerListDownloadService.GetAll().Where(x => DbFunctions.TruncateTime(x.OrderShippingDate) == parsedDate).Select(x => x.AllowDownload).FirstOrDefault();
            bool buyerReport = _buyerListDownloadService.GetAll().Where(x => DbFunctions.TruncateTime(x.OrderShippingDate) == parsedDate).Select(x => x.AllowReport).FirstOrDefault() ?? false;
            if (!buyerDownload)
                return BadRequest("La descarga de datos aun no está disponible. Por favor, inténtalo de nuevo más tarde.");
            else if (notContains)
            {
                var orderIds = orderIetmsNotContains.Select(x => x.OrderId).Distinct().ToList();
                bool anyOrderNotCancelled = OrderUtils.GetFilteredOrders(_orderService)
                    .Where(x => orderIds.Contains(x.Id) && x.OrderStatusId != 40).Any();
                if (anyOrderNotCancelled)
                    return BadRequest("La descarga de datos aun no está disponible. Por favor, inténtalo de nuevo más tarde.");
            }

            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate.HasValue && DbFunctions.TruncateTime(x.SelectedShippingDate.Value) == parsedDate);

            List<int> buyerCustomerIds = new List<int>();
            var substituyingUsers = _substituteBuyerService.GetAll()
                .Where(x => x.SelectedShippingDate == parsedDate && x.SubstituteCustomerId == userId)
                .ToList();
            var substituyingUserIds = substituyingUsers.Select(x => x.CustomerId).ToList();
            buyerCustomerIds.AddRange(substituyingUserIds);

            var isBeingSustituted = _substituteBuyerService.GetAll()
                .Where(x => x.SelectedShippingDate == parsedDate && x.CustomerId == userId)
                .Any();
            if (!isBeingSustituted)
            {
                buyerCustomerIds.Add(userId);
            }

            var orderItemBuyer = _orderItemBuyerService.GetAll()
                .Where(x => buyerCustomerIds.Contains(x.CustomerId) && x.SelectedShippingDate == parsedDate)
                .Select(x => new { x.OrderId, x.OrderItemId, x.CustomerId, x.SelectedShippingDate })
                .ToList();

            var substituyingUserModel = substituyingUsers.Select(x => new SubstituyingUserModel()
            {
                OriginalCustomerId = x.CustomerId,
                SubstituteCustomerId = x.SubstituteCustomerId,
                OrderItemIds = orderItemBuyer.Where(y => y.CustomerId == x.CustomerId && y.SelectedShippingDate == x.SelectedShippingDate).Select(y => y.OrderItemId).ToList()
            }).ToList();


            var buyerOrderIds = orderItemBuyer.Select(x => x.OrderId).ToList();
            var orders = ordersQuery.Where(x => buyerOrderIds.Contains(x.Id)).ToList();

            var markedProducts = _markedBuyerItemService.GetAll().Where(x => x.OrderShippingDate == parsedDate).ToList();
            List<OrderItem> orderItems = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService);
            List<int> orderItemIdsOfTheDay = orderItems.Select(x => x.Id).ToList();

            List<int> orderItemIds = orderItemBuyer.Select(x => x.OrderItemId).ToList();
            var reports = _orderReportService.GetAll().Where(x => orderItemIds.Contains(x.OrderItemId)).ToList();
            orderItems = orderItems.Where(x => orderItemIds.Contains(x.Id)).ToList();

            var mainManufacturers = _productMainManufacturerService.GetAll().ToList();
            var manufacturerGroup = orderItems
                    .GroupBy(x => ProductUtils.GetMainManufacturer(x.Product.ProductManufacturers, mainManufacturers.Where(y => y.ProductId == x.ProductId).FirstOrDefault()))
                    .OrderBy(x => x.Key?.Name)
                    .ToList();

            var sentToSupermarketCustomerIds = reports.Where(x => x.SentToSupermarketByUserId.HasValue).Select(x => x.SentToSupermarketByUserId.Value).ToList();
            substituyingUserIds.AddRange(sentToSupermarketCustomerIds);
            var supermarketAndSubstituteCustomers = _customerService.GetAllCustomersQuery().Where(x => substituyingUserIds.Contains(x.Id)).ToList();

            List<List<IGrouping<Product, OrderItem>>> productGroup = new List<List<IGrouping<Product, OrderItem>>>();
            foreach (var manufacturer in manufacturerGroup)
            {
                productGroup.Add(manufacturer
                        .Select(x => x)
                        .GroupBy(x => x.Product)
                        .OrderBy(x => x.Key?.ProductCategories.Where(y => y.Category.ParentCategoryId == 0 && !y.Category.Deleted).Select(y => y.Category).FirstOrDefault()?.DisplayOrder)
                        .ThenBy(x => x.Key?.ProductCategories.Where(y => y.Category.ParentCategoryId == 0 && !y.Category.Deleted).Select(y => y.Category).FirstOrDefault()?.Name)
                        .ThenBy(x => x.Key?.ProductCategories.Where(y => y.Category.ParentCategoryId != 0 && !y.Category.Deleted).Select(y => y.Category).FirstOrDefault()?.Name)
                        .ThenBy(x => x.Key.Name)
                        .ToList());
            }

            List<ReportProductDto> dto = productGroup.SelectMany(x => x).Select(x => ConvertToReportProductDto(x, parsedDate, reports, supermarketAndSubstituteCustomers, substituyingUserModel, markedProducts)).ToList();

            return Ok(new { CanReport = buyerReport, Products = dto });
        }


        private ReportProductDto ConvertToReportProductDto(IGrouping<Product, OrderItem> group,
            DateTime shippingDate,
            List<OrderReport> reports,
            List<Customer> supermarketAndSubstituteCustomers,
            List<SubstituyingUserModel> substituteBuyers = null,
            List<MarkedBuyerItem> markedProducts = null)
        {
            var mainManufacturer = _productMainManufacturerService.GetAll().Where(x => x.ProductId == group.Key.Id).FirstOrDefault();
            var mainManufaturer = ProductUtils.GetMainManufacturer(group.Key.ProductManufacturers, mainManufacturer);
            var originalBuyerId = substituteBuyers == null ? 0 : substituteBuyers.Where(x => x.OrderItemIds.Intersect(group.Select(y => y.Id)).Any()).Select(x => x.OriginalCustomerId).FirstOrDefault();

            return new ReportProductDto()
            {
                Date = shippingDate.ToUniversalTime(),
                ProductName = group.Key.Name,
                ProductPictureUrl = group.Key.ProductPictures.Count > 0 ? _pictureService.GetPictureUrl(group.Key.ProductPictures.FirstOrDefault().PictureId) : null,
                ProductId = group.Key.Id,
                RequestedQuantity = GetTotalQuantity(group.Key, group.Select(y => y.Quantity).DefaultIfEmpty().Sum()),
                RequestedUnit = (group.Key.EquivalenceCoefficient > 0 || group.Key.WeightInterval > 0) ? "kg" : "pz",
                RequestedQtyCost = reports.Where(y => y.ProductId == group.Key.Id).Select(z => z.RequestedQtyCost).FirstOrDefault(), //se está guardando la misma para todos
                ManufacturerId = reports.Where(y => y.ProductId == group.Key.Id).Select(y => y.ManufacturerId ?? 0).FirstOrDefault(),
                BuyedQuantity = reports.Where(y => y.ProductId == group.Key.Id).Select(z => z.Quantity).FirstOrDefault(),//se está guardando la misma para todos
                CustomManufacturer = reports.Where(y => y.ProductId == group.Key.Id).Select(z => z.ShoppingStoreId).FirstOrDefault(),
                ControlPrice = group.Select(y => y.UnitPriceInclTax).DefaultIfEmpty().Average() * (1 - (group.Key.ProductCategories.Where(z => z.Category.ParentCategoryId != 0).Select(z => z.Category.PercentageOfUtility).FirstOrDefault() / 100)),
                UnitPrice = group.Select(y => y.UnitPriceInclTax).DefaultIfEmpty().Average(),
                UnitCost = reports.Where(y => y.ProductId == group.Key.Id).Select(z => z.UnitCost).FirstOrDefault(),
                SellPrice = group.Select(y => y.PriceInclTax).DefaultIfEmpty().Max(),
                NotBuyedReason = reports.Where(y => y.ProductId == group.Key.Id).Select(y => y.NotBuyedReason).FirstOrDefault(),
                NotBuyedReasonId = reports.Where(y => y.ProductId == group.Key.Id).Select(y => y.NotBuyedReasonId).FirstOrDefault(),
                OrderItemIds = string.Join(",", group.Select(y => y.Id)),
                SentToSupermarket = reports.Where(y => y.ProductId == group.Key.Id).Select(y => y.SentToSupermarket).FirstOrDefault(),
                Invoice = reports.Where(y => y.ProductId == group.Key.Id).Select(y => y.Invoice).FirstOrDefault(),
                IsMarked = markedProducts != null && markedProducts.Where(y => y.ProductId == group.Key.Id).Select(y => y.IsMarked).FirstOrDefault(),
                MainManufacturerId = mainManufaturer == null ? 0 : mainManufaturer.Id,
                MainManufacturerName = mainManufaturer?.Name,
                OriginalBuyerName = substituteBuyers == null ? null : supermarketAndSubstituteCustomers.Where(y => y.Id == originalBuyerId).FirstOrDefault()?.GetFullName(),
                SentToSupermarketByUserId = reports.Where(y => y.ProductId == group.Key.Id).Select(y => y.SentToSupermarketByUserId).FirstOrDefault(),
                SentToSupermarketByUserFullName = reports.Where(y => y.ProductId == group.Key.Id).Select(y => y.SentToSupermarketByUserId).FirstOrDefault().HasValue &&
                reports.Where(y => y.ProductId == group.Key.Id).Select(y => y.SentToSupermarketByUserId).FirstOrDefault().Value > 0 ?
                    supermarketAndSubstituteCustomers.Where(y => y.Id == reports.Where(z => z.ProductId == group.Key.Id).Select(z => z.SentToSupermarketByUserId).FirstOrDefault().Value).FirstOrDefault()?.GetFullName()
                    : null
            };
        }

        private decimal GetTotalQuantity(Product product, decimal quantity)
        {
            decimal result = quantity;
            if (product.EquivalenceCoefficient > 0)
                result = ((quantity * 1000) / product.EquivalenceCoefficient) / 1000;
            else if (product.WeightInterval > 0)
                result = (quantity * product.WeightInterval) / 1000;
            return Math.Round(result, 2);
        }
    }
}