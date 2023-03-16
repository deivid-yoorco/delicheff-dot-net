using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;
using Teed.Plugin.Groceries.Domain.OrderReports;
using Teed.Plugin.Groceries.Models.OrderDeliveryReports;
using Teed.Plugin.Groceries.Services;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using iText.Forms.Xfdf;
using System.Linq.Expressions;
using Nop.Services.Catalog;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Nop.Core.Domain.Catalog;
using Teed.Plugin.Groceries.Domain.OrderItemBuyers;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Teed.Plugin.Groceries.Extensions;
using Nop.Services.Helpers;
using Teed.Plugin.Groceries.Utils;
using Teed.Plugin.Groceries.Security;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq.Dynamic;
using Teed.Plugin.Groceries.Domain.Product;
using Nop.Core.Domain.Customers;
using System.Data.Entity;
using Teed.Plugin.Groceries.Domain.BuyerPayments;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class OrderDeliveryReportsController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly ShippingRouteService _shippingRouteService;
        private readonly ShippingRouteUserService _shippingRouteUserService;
        private readonly IOrderService _orderService;
        private readonly Services.OrderReportService _orderReportService;
        private readonly IOrderService _orderItem;
        private readonly Services.AditionalCostService _aditionalCostService;
        private readonly OrderReportStatusService _orderReportStatusService;
        private readonly IProductService _productService;
        private readonly IManufacturerService _manufacturerService;
        private readonly OrderReportLogService _orderReportLogService;
        private readonly OrderReportFileService _orderReportFileService;
        private readonly OrderItemBuyerService _orderItemBuyerService;
        private readonly NotDeliveredOrderItemService _notDeliveredOrderItemService;
        private readonly OrderReportTransferService _orderReportTransferService;
        private readonly ManagerExpensesService _managerExpensesService;
        private readonly ManagerExpensesStatusService _managerExpensesStatusService;
        private readonly ManagerQuantitiesService _managerQuantitiesService;
        private readonly IProductLogService _productLogService;
        private readonly ProductPricePendingUpdateService _productPricePendingUpdateService;
        private readonly SubstituteBuyerService _substituteBuyerService;
        private readonly CostsIncreaseWarningService _costsIncreaseWarningService;
        private readonly BuyerPaymentService _buyerPaymentService;
        private readonly BuyerPaymentTicketFileService _buyerPaymentTicketFileService;
        private readonly ProductMainManufacturerService _productMainManufacturerService;
        private readonly CostsDecreaseWarningService _costsDecreaseWarningService;

        public OrderDeliveryReportsController(BuyerPaymentService buyerPaymentService,
                                              BuyerPaymentTicketFileService buyerPaymentTicketFileService,
                                              IPermissionService permissionService, ShippingRouteService shippingRouteService,
                                              ShippingRouteUserService shippingRouteUserService, IWorkContext workContext,
                                              ICustomerService customerService, IOrderService orderService,
                                              Services.OrderReportService orderReportService,
                                              IOrderService orderItem,
                                              Services.AditionalCostService aditionalCostService,
                                              OrderReportStatusService orderReportStatusService,
                                              IProductService productService,
                                               OrderReportLogService orderReportLogService,
                                               IManufacturerService manufacturerService,
                                               OrderReportFileService orderReportFileService,
                                               OrderItemBuyerService orderItemBuyerService,
                                               NotDeliveredOrderItemService notDeliveredOrderItemService,
                                               OrderReportTransferService orderReportTransferService,
                                               ManagerExpensesService managerExpensesService,
                                               ManagerExpensesStatusService managerExpensesStatusService,
                                               ManagerQuantitiesService managerQuantitiesService,
                                               IProductLogService productLogService,
                                               ProductPricePendingUpdateService productPricePendingUpdateService,
                                               SubstituteBuyerService substituteBuyerService,
                                               CostsIncreaseWarningService costsIncreaseWarningService,
                                               ProductMainManufacturerService productMainManufacturerService,
                                               CostsDecreaseWarningService costsDecreaseWarningService)
        {
            _substituteBuyerService = substituteBuyerService;
            _orderService = orderService;
            _permissionService = permissionService;
            _shippingRouteService = shippingRouteService;
            _shippingRouteUserService = shippingRouteUserService;
            _workContext = workContext;
            _customerService = customerService;
            _orderReportService = orderReportService;
            _orderItem = orderItem;
            _aditionalCostService = aditionalCostService;
            _orderReportStatusService = orderReportStatusService;
            _productService = productService;
            _orderReportLogService = orderReportLogService;
            _manufacturerService = manufacturerService;
            _orderReportFileService = orderReportFileService;
            _orderItemBuyerService = orderItemBuyerService;
            _orderReportTransferService = orderReportTransferService;
            _notDeliveredOrderItemService = notDeliveredOrderItemService;
            _managerExpensesService = managerExpensesService;
            _managerExpensesStatusService = managerExpensesStatusService;
            _managerQuantitiesService = managerQuantitiesService;
            _productLogService = productLogService;
            _productPricePendingUpdateService = productPricePendingUpdateService;
            _costsIncreaseWarningService = costsIncreaseWarningService;
            _buyerPaymentService = buyerPaymentService;
            _buyerPaymentTicketFileService = buyerPaymentTicketFileService;
            _productMainManufacturerService = productMainManufacturerService;
            _costsDecreaseWarningService = costsDecreaseWarningService;
        }

        public IActionResult DeliveryDateList()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderDeliveryReports))
                return AccessDeniedView();

            var controlDate = DateTime.Now.AddDays(-90).Date;
            var orderItemBuyerQuery = _orderItemBuyerService.GetAll().Where(x => x.SelectedShippingDate >= controlDate && !x.Deleted);
            var orderIds = orderItemBuyerQuery.Select(x => x.OrderId)
                .Distinct()
                .ToList();
            var orderItemBuyer = _orderItemBuyerService.GetAll().Where(x => x.SelectedShippingDate >= controlDate && !x.Deleted)
                .GroupBy(x => new PendingReportModel() { ShippingDate = x.SelectedShippingDate.Value, BuyerId = x.CustomerId })
                .Select(x => x.Key)
                .ToList();

            var buyerIds = orderItemBuyer.Select(x => x.BuyerId).Distinct().ToList();
            var completedReports = _orderReportStatusService.GetAll().Where(x => x.StatusTypeId == 2 && x.ShippingDate >= controlDate).ToList();
            var notCompletedReports = _orderReportStatusService.GetAll().Where(x => x.StatusTypeId == 1 && x.ShippingDate >= controlDate).ToList();
            var customers = _customerService.GetAllCustomersQuery().Where(x => buyerIds.Contains(x.Id)).ToList();
            var pendingData = new List<PendingReportModel>();
            foreach (var item in orderItemBuyer)
            {
                if (!completedReports.Where(x => x.ShippingDate == item.ShippingDate && x.BuyerId == item.BuyerId).Any())
                {
                    var buyer = customers.Where(x => x.Id == item.BuyerId).FirstOrDefault();
                    pendingData.Add(new PendingReportModel()
                    {
                        BuyerId = item.BuyerId,
                        ShippingDate = item.ShippingDate,
                        BuyerName = buyer != null ? buyer.GetFullName() : $"Comprador no encontrado (ID: {item.BuyerId})"
                    });
                }
            }
            foreach (var item in notCompletedReports)
            {
                if (!pendingData.Where(x => x.ShippingDate == item.ShippingDate && x.BuyerId == item.BuyerId).Any())
                {
                    var buyer = customers.Where(x => x.Id == item.BuyerId).FirstOrDefault();
                    pendingData.Add(new PendingReportModel()
                    {
                        BuyerId = item.BuyerId,
                        ShippingDate = item.ShippingDate,
                        BuyerName = buyer != null ? buyer.GetFullName() : $"Comprador no encontrado (ID: {item.BuyerId})"
                    });
                }
            }

            return View("~/Plugins/Teed.Plugin.Groceries/Views/OrderDeliveryReports/DeliveryDateList.cshtml", pendingData);
        }

        public IActionResult DeleteDeliveredReport(string date, int buyerId)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderDeliveryReports))
                return AccessDeniedView();

            DateTime parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var substituyingBuyerIds = _substituteBuyerService.GetAll()
                .Where(x => x.SelectedShippingDate == parsedDate && x.SubstituteCustomerId == buyerId)
                .Select(x => x.CustomerId)
                .ToList();
            substituyingBuyerIds.Add(buyerId);

            var reportStatus = _orderReportStatusService.GetAll()
                .Where(x => substituyingBuyerIds.Contains(x.BuyerId) && DbFunctions.TruncateTime(x.ShippingDate) == parsedDate)
                .ToList();

            foreach (var item in reportStatus)
            {
                _orderReportStatusService.Delete(item);
            }

            return RedirectToAction("OrderReportStatus", new { id = date });
        }

        public IActionResult OrderReportStatus(string id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderDeliveryReports))
                return AccessDeniedView();

            if (id == null) { return AccessDeniedView(); }

            var dateSelected = DateTime.ParseExact(id, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate == dateSelected).ToList();
            var orderItemsIds = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService).Select(y => y.Id).ToList();

            var buyersData = _orderReportStatusService.GetAll().Where(x => x.ShippingDate == dateSelected).ToList();
            var buyerIds = buyersData.Select(x => x.BuyerId).ToList();
            var allBuyersIdsOfOrderItems = _orderItemBuyerService.GetAll()
                .Where(x => orderItemsIds.Contains(x.OrderItemId))
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => buyerIds.Count > 0 && !buyerIds.Contains(x))
                .ToList();

            var customers = _customerService.GetAllCustomersQuery().Where(x => x.CustomerRoles.Count > 1).ToList();

            OrderReportStatusViewModel model = new OrderReportStatusViewModel();
            model.Date = dateSelected;
            model.DayOfWeek = dateSelected.ToString("dddd", new CultureInfo("es-MX"));

            var buyersWhitReport = buyersData.Select(x => new BuyerData()
            {
                BuyerId = x.BuyerId,
                BuyerName = customers.Where(y => y.Id == x.BuyerId).FirstOrDefault()?.GetFullName(),
                Status = x.StatusTypeId,
                HasReport = true,
                NameConfirmReport = x.StatusTypeId == 2 ? customers.Where(y => y.Id == x.ClosedById).FirstOrDefault()?.GetFullName() : null,
            }).ToList();

            var buyersWhitOutReport = allBuyersIdsOfOrderItems.Select(x => new BuyerData()
            {
                BuyerId = x,
                BuyerName = customers.Where(y => y.Id == x).FirstOrDefault()?.GetFullName(),
                Status = 0,
                HasReport = false
            }).ToList();

            List<BuyerData> buyerData = new List<BuyerData>();
            buyerData.AddRange(buyersWhitReport.OrderBy(x => x.BuyerName));
            buyerData.AddRange(buyersWhitOutReport.OrderBy(x => x.BuyerName));

            model.Buyers = buyerData;

            var lastNoBuyedReport = _orderReportService.GetAll()
                .Where(x => (!string.IsNullOrEmpty(x.NotBuyedReason) || x.NotBuyedReasonId.HasValue) && x.SentToSupermarket.Value != true && x.OrderShippingDate == dateSelected)
                .FirstOrDefault();

            int? lastUserIdUpdatedNoBuyedReport = lastNoBuyedReport?.UpdatedByUserId;

            if (lastUserIdUpdatedNoBuyedReport.HasValue)
                model.UserNameLastUpdateOnReportNoBuyed = customers.Where(x => x.Id == lastUserIdUpdatedNoBuyedReport.Value).FirstOrDefault()?.GetFullName();

            var managerExpensesStatus = _managerExpensesStatusService.GetAll().Where(x => x.ShippingDate == dateSelected && !x.Deleted).FirstOrDefault();
            if (managerExpensesStatus != null)
                model.ManagerNameConfirmExpensesReport = customers.Where(x => x.Id == managerExpensesStatus.ClosedById).FirstOrDefault()?.GetFullName();

            var lastSupermarketReport = _orderReportService.GetAll()
                .Where(x => x.SentToSupermarket.HasValue && x.SentToSupermarket.Value == true && x.OrderShippingDate == dateSelected)
                .FirstOrDefault();

            int? lastUserIdUpdatedSupermarketReport = lastSupermarketReport?.UpdatedByUserId;
            if (lastUserIdUpdatedSupermarketReport.HasValue)
                model.LastUpdateSupermarketReportByUser = customers.Where(x => x.Id == lastUserIdUpdatedSupermarketReport.Value).FirstOrDefault()?.GetFullName();

            model.SubstitutedBuyerIds = _substituteBuyerService.GetAll().Where(x => x.SelectedShippingDate == dateSelected).Select(x => x.CustomerId).ToList();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/OrderDeliveryReports/OrderReportStatus.cshtml", model);
        }

        private void ConfirmOrderReport(int buyerId, DateTime orderShippingDate, int testingWithProductId = 0)
        {
            OrderReportStatus orderReportStatus = _orderReportStatusService
                .GetAll()
                .Where(x => x.ShippingDate == orderShippingDate && x.BuyerId == buyerId)
                .FirstOrDefault();

            if (testingWithProductId < 1)
            {
                orderReportStatus.StatusTypeId = 2;
                orderReportStatus.ClosedById = _workContext.CurrentCustomer.Id;
                orderReportStatus.ClosedDateUtc = DateTime.UtcNow;

                _orderReportStatusService.Update(orderReportStatus);
            }

            var reports = _orderReportService.GetAll()
                .Where(x => x.OrderShippingDate == orderShippingDate && x.OriginalBuyerId == buyerId)
                .Select(x => new { x.ProductId, x.UpdatedUnitCost, x.OrderShippingDate })
                .GroupBy(x => x.ProductId)
                .Select(x => x.FirstOrDefault())
                .ToList();
            var productIds = reports.Select(x => x.ProductId).ToList();
            var productPreviousReports = _orderReportService.GetAll()
                .Where(x => productIds.Contains(x.ProductId) && x.OrderShippingDate < orderShippingDate)
                .Select(x => new { x.ProductId, x.UpdatedUnitCost, x.OrderShippingDate })
                .ToList();
            var products = _productService.GetAllProductsQuery().Where(x => productIds.Contains(x.Id)).ToList();

            if (testingWithProductId > 0)
                reports = reports.Where(x => x.ProductId == testingWithProductId).ToList();

            foreach (var report in reports)
            {
                var product = products.Where(x => x.Id == report.ProductId).FirstOrDefault();
                if (product == null) continue;
                decimal reportCost = report.UpdatedUnitCost;
                if (reportCost == 0) continue;
                string log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - Se actualizó el costo del producto debido a que el usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) liquidó un reporte de compras. El costo del producto pasó de {product.ProductCost} a {reportCost}";
                if (reportCost == product.ProductCost)
                {
                    log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) liquidó cuentas pero no se actualizó el costo ya que se mantuvo igual.";
                }
                else
                {
                    var firstReport = false;
                    var newestReport = productPreviousReports.Where(x => x.ProductId == product.Id).OrderByDescending(x => x.OrderShippingDate).FirstOrDefault();
                    if (newestReport == null)
                    {
                        try
                        {
                            firstReport = true;
                            newestReport = new { ProductId = product.Id, UpdatedUnitCost = product.ProductCost, report.OrderShippingDate };
                        }
                        catch (Exception e)
                        {
                            _ = e;
                        }
                    }
                    if (reportCost > newestReport.UpdatedUnitCost && newestReport.UpdatedUnitCost > 1)
                    {
                        var costWarning = new CostsIncreaseWarning
                        {
                            ProductId = product.Id,
                            NewCost = reportCost,
                            OldCost = newestReport.UpdatedUnitCost,
                            OldReportedCostDate = newestReport.OrderShippingDate,
                            OriginalOrderShippingDate = orderShippingDate.Date,
                            Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + $" - Creado automáticamente por costo de reporte mayor para el producto “{product.Name}” con costo anterior reportado de {newestReport.UpdatedUnitCost:C} al nuevo costo de {reportCost:C} para la fecha seleccionada {orderShippingDate:dd/MM/yyyy} (Usuario presente en la actualización: { _workContext.CurrentCustomer.GetFullName()} ({_workContext.CurrentCustomer.Id})){(firstReport ? " (Calculado con costo original del producto, ya que es primera vez de costo reportado de este producto)" : "")}.\n"
                        };
                        _costsIncreaseWarningService.Insert(costWarning);
                    }
                    else if (reportCost < newestReport.UpdatedUnitCost && newestReport.UpdatedUnitCost > 1)
                    {
                        // check if lower price is equal or above 10% decrease, if so insert
                        var tenPorcentAmount = newestReport.UpdatedUnitCost - decimal.Round(newestReport.UpdatedUnitCost * (decimal)0.1, 4);
                        if (reportCost < tenPorcentAmount)
                        {
                            var costWarning = new CostsDecreaseWarning
                            {
                                ProductId = product.Id,
                                NewCost = reportCost,
                                OldCost = newestReport.UpdatedUnitCost,
                                OldReportedCostDate = newestReport.OrderShippingDate,
                                OriginalOrderShippingDate = orderShippingDate.Date,
                                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + $" - Creado automáticamente por costo de reporte menor para el producto “{product.Name}” con costo anterior reportado de {newestReport.UpdatedUnitCost:C} al nuevo costo de {reportCost:C} para la fecha seleccionada {orderShippingDate:dd/MM/yyyy} (Usuario presente en la actualización: { _workContext.CurrentCustomer.GetFullName()} ({_workContext.CurrentCustomer.Id})){(firstReport ? " (Calculado con costo original del producto, ya que es primera vez de costo reportado de este producto)" : "")}.\n"
                            };
                            _costsDecreaseWarningService.Insert(costWarning);
                        }
                    }
                    product.ProductCost = reportCost;
                    _productService.UpdateProduct(product);

                    var productPricePendingUpdate = _productPricePendingUpdateService.GetAll().Where(x => x.ProductId == product.Id).Any();
                    if (!productPricePendingUpdate)
                        _productPricePendingUpdateService.Insert(new ProductPricePendingUpdate() { ProductId = product.Id });
                }

                _productLogService.InsertProductLog(new ProductLog()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    ProductId = product.Id,
                    UserId = _workContext.CurrentCustomer.Id,
                    Message = log
                });
            }
        }

        public IActionResult OrderReportDetails(int buyerId, string date, bool btnEneableOrderFolio = false)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            if (date == null) return AccessDeniedView();
            var dateSelected = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            OrderReportDetailsViewModel model = new OrderReportDetailsViewModel();

            var orderStatus = _orderReportStatusService.GetAll()
                .Where(x => x.BuyerId == buyerId && x.ShippingDate == dateSelected)
                .OrderBy(x => x.CreatedOnUtc)
                .FirstOrDefault();
            if (orderStatus == null) return NotFound();
            var orderReports = _orderReportService.GetAll()
                .Where(x => x.OriginalBuyerId == buyerId && x.OrderShippingDate == dateSelected && !x.Deleted)
                .GroupBy(x => x.ProductId)
                .ToList();

            var productMainManufacturers = _productMainManufacturerService.GetAll().ToList();
            var manufacturers = _manufacturerService.GetAllManufacturers().OrderBy(x => x.Name).ToList();
            model.Manufacturers = new SelectList(manufacturers, "Id", "Name");

            var orderItemIds = orderReports.Select(x => x.Select(y => y.OrderItemId.ToString())).SelectMany(x => x).ToList();
            var orderIds = orderReports.Select(x => x.Select(y => y.OrderId)).SelectMany(x => x).ToList();

            var orders = OrderUtils.GetFilteredOrders(_orderService)
             .Where(x => x.SelectedShippingDate.HasValue && x.SelectedShippingDate.Value == dateSelected)
             .ToList();
            List<OrderItem> parsedOrderItems = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService);

            List<int> orderItemIdsOfTheDay = parsedOrderItems.Select(x => x.Id).ToList();
            var orderItemBuyerQuery = _orderItemBuyerService.GetAll().Where(x => orderItemIdsOfTheDay.Contains(x.OrderItemId));
            orderItemBuyerQuery = orderItemBuyerQuery.Where(x => x.CustomerId == buyerId);
            var orderItemBuyer = orderItemBuyerQuery.ToList();
            var orderItemIdsFilter = orderItemBuyer.Where(x => x.CustomerId == buyerId).Select(x => x.OrderItemId);
            var filteredOrderItems = parsedOrderItems.Where(x => orderItemIdsFilter.Contains(x.Id)).ToList();

            var groupedByProduct = filteredOrderItems
                    .GroupBy(x => x.Product?.ProductCategories.Where(y => y.Category.ParentCategoryId == 0 && !y.Category.Deleted).Select(y => y.Category).FirstOrDefault())
                    .OrderBy(x => x.Key?.DisplayOrder)
                    .ToList();

            var productIds = filteredOrderItems.Select(x => x.ProductId).Distinct().ToList();
            var reportStatus = _orderReportStatusService.GetAll().Where(x => x.StatusTypeId == 2 && x.ShippingDate < dateSelected)
                .GroupBy(x => new { x.ShippingDate, x.BuyerId })
                .Select(x => x.Key)
                .ToList();
            var reports = _orderReportService.GetAll()
                .Select(x => new SimpleReportModel()
                {
                    OrderShippingDate = x.OrderShippingDate,
                    OriginalBuyerId = x.OriginalBuyerId,
                    ProductId = x.ProductId,
                    Invoice = x.Invoice,
                    SentToSupermarket = x.SentToSupermarket,
                    UpdatedUnitCost = x.UpdatedUnitCost
                })
                .Where(x => productIds.Contains(x.ProductId) && x.Invoice != null && x.OrderShippingDate < dateSelected)
                .ToList()
                .Where(x => reportStatus.Contains(new { ShippingDate = x.OrderShippingDate, BuyerId = x.OriginalBuyerId }))
                .ToList();
            var buyerIds = reports.Select(x => x.OriginalBuyerId).Distinct().ToList();
            var reportBuyers = _customerService.GetAllCustomersQuery().Where(x => buyerIds.Contains(x.Id)).ToList();

            model.BuyerId = buyerId;
            model.CurrentStatus = (ReportStatusType)orderStatus.StatusTypeId;
            model.CurrentUserName = _workContext.CurrentCustomer.GetFullName();
            model.BuyerName = _customerService.GetCustomerById(buyerId).GetFullName();
            model.OrderShippigDate = dateSelected;
            model.DayOfWeek = dateSelected.ToString("dddd", new CultureInfo("es-MX"));

            model.Products = new List<ProductData>();

            foreach (var item in groupedByProduct)
            {
                var orderItemGroup = item
                       .Select(x => x)
                       .GroupBy(x => x.Product)
                       .OrderBy(x => x.Key?.ProductManufacturers.Where(y => !y.Manufacturer.Deleted).OrderBy(y => y.Manufacturer.DisplayOrder).FirstOrDefault()?.Manufacturer.Name)
                       .ThenBy(x => x.Key?.Name)
                       .ThenBy(x => x.Key?.ProductManufacturers.Where(y => !y.Manufacturer.Deleted).OrderBy(y => y.Manufacturer.DisplayOrder).FirstOrDefault()?.DisplayOrder)
                       .ToList();

                var productsInGroup = new List<ProductData>();
                foreach (var group in orderItemGroup)
                {
                    var productData = new ProductData();
                    productData.ProductId = group.Key.Id;
                    productData.ProductName = group.Key.Name;

                    var currentReports = orderReports.Where(y => y.Key == group.Key.Id);

                    var productCostKgPz = currentReports.Select(y => y.Select(z => z.UpdatedUnitCost).FirstOrDefault()).FirstOrDefault();
                    var productCostKgPzOriginal = currentReports.Select(y => y.Select(z => z.UnitCost).FirstOrDefault()).FirstOrDefault();
                    productData.ProductCostKgPz = Math.Round(productCostKgPz, 2);

                    productData.ProductQuantity = currentReports.Select(y => y.Select(z => z.Quantity.Value).FirstOrDefault()).FirstOrDefault();

                    productData.ProductCostKgPzOriginal = productCostKgPzOriginal;

                    var productQuantity = currentReports.Select(y => y.Select(z => z.UpdatedQuantity).FirstOrDefault()).FirstOrDefault();
                    productData.ProductQuantity = productQuantity.HasValue ? Math.Round(productQuantity.Value, 2) : orderReports.Select(y => y.Select(z => z.Quantity.Value).FirstOrDefault()).FirstOrDefault();

                    productData.ProductQuantityOriginal = currentReports.Select(y => y.Select(z => z.Quantity.Value).FirstOrDefault()).FirstOrDefault();

                    var productAmountTotal = currentReports.Select(y => y.Select(z => z.UpdatedRequestedQtyCost).FirstOrDefault()).FirstOrDefault();
                    productData.ProductAmountTotal = productAmountTotal.HasValue ? Math.Round(productAmountTotal.Value, 2) : orderReports.Select(y => y.Select(z => z.RequestedQtyCost).FirstOrDefault()).FirstOrDefault();

                    productData.ProductAmountTotalOriginal = currentReports.Select(y => y.Select(z => z.RequestedQtyCost).FirstOrDefault()).FirstOrDefault();

                    productData.NoBuyedReazon = currentReports.Select(y => y.Select(z => z.NotBuyedReason).FirstOrDefault()).FirstOrDefault();

                    var manufacturerId = currentReports.Select(y => y.Select(z => z.ManufacturerId).FirstOrDefault()).FirstOrDefault();
                    var manufacturer = manufacturers.Where(x => x.Id == manufacturerId).FirstOrDefault();
                    productData.ManufacturerId = manufacturer != null ? manufacturer.Id : -1;

                    productData.Manufacturer = manufacturer != null ? manufacturer.Name : currentReports.Select(y => y.Select(z => z.ShoppingStoreId).FirstOrDefault()).FirstOrDefault();

                    productData.RequestedQuantity = GetTotalQuantity(group.Key, group.Where(y => y.ProductId == group.Key.Id).Select(y => y).ToList());

                    productData.RequestedUnit = GetUnitProduct(group.Key);

                    productData.NumberOrders = group.Select(x => x.OrderId).Count();

                    productData.ProductManufactures = GetProductManufacturers(group.Key, productData.Manufacturer);

                    productData.IsProductUpdated = Convert.ToDecimal(productCostKgPz) != productCostKgPzOriginal ||
                        (productQuantity.HasValue && Convert.ToDecimal(productQuantity) != currentReports.Select(y => y.Select(z => z.Quantity).FirstOrDefault()).FirstOrDefault()) ||
                        (productAmountTotal.HasValue && Convert.ToDecimal(productAmountTotal.Value) != productData.ProductAmountTotalOriginal);

                    productData.Invoice = currentReports.SelectMany(x => x.Select(y => y.Invoice)).Where(x => x != null).FirstOrDefault();

                    productData.SentToSuperMarket = currentReports.SelectMany(y => y.Select(z => z.SentToSupermarketByUserId)).Any(y => y.HasValue && y > 0);

                    productData.PreviousCostList = GetOrderReportCostsForProducts(reports, reportBuyers, group.Key.Id);

                    productData.BoughtTypeId = OrderUtils.GetBoughtTypeId(manufacturer, currentReports.SelectMany(x => x.Select(y => y.BoughtTypeId)).Where(x => x != null).FirstOrDefault());

                    productsInGroup.Add(productData);
                }

                model.Products.AddRange(productsInGroup);
            }


            model.TotalProductsUpdated = model.Products.Where(x => x.IsProductUpdated).Count();

            var buyerMoney = OrderUtils.GetBuyerCashAndTransferAmountWithGivenBoughtTypes(parsedOrderItems,
                buyerId,
                orderItemBuyer,
                manufacturers,
                productMainManufacturers,
                model.Products.Select(x => new OrderItemBoughtType { ProductId = x.ProductId, BoughtType = (BoughtType)x.BoughtTypeId }).ToList()
                );

            //var cashTypeAmount = model.Products.Where(x => !x.SentToSuperMarket && x.BoughtTypeId == (int)BoughtType.Cash).DefaultIfEmpty().Sum(x => x.ProductAmountTotal);
            //var cardTypeAmount = model.Products.Where(x => !x.SentToSuperMarket && x.BoughtTypeId == (int)BoughtType.CorporateCard).DefaultIfEmpty().Sum(x => x.ProductAmountTotal);
            //var transferTypeAmount = model.Products.Where(x => !x.SentToSuperMarket && x.BoughtTypeId == (int)BoughtType.Transfer).DefaultIfEmpty().Sum(x => x.ProductAmountTotal);

            var moneyToBuyer = RoundBuyerCashAmount(buyerMoney.Cash);
            model.BuyerCashAmountString = moneyToBuyer.AmountString;
            model.BuyerCashAmount = moneyToBuyer.Amount;

            model.BuyerCardAmount = buyerMoney.Card;
            model.BuyerCardAmountString = buyerMoney.Card.ToString("C");

            var orderReportTransfer = _orderReportTransferService.GetAll().Where(x => x.BuyerId == buyerId && x.OrderShippingDate == dateSelected).FirstOrDefault();
            if (orderReportTransfer == null)
            {
                orderReportTransfer = new OrderReportTransfer();
            }
            model.OrderReportTransferId = orderReportTransfer.Id;
            model.AmountTotalTransfer = orderReportTransfer.TransferAmount;
            model.FileTransfer = orderReportTransfer.File;
            var b64 = orderReportTransfer.File == null ? null : "data:image/png;base64," + Convert.ToBase64String(orderReportTransfer.File);

            model.FileTransferB64 = b64;

            model.AmountTotalSpent = model.Products.Where(x => !x.SentToSuperMarket).Sum(x => x.ProductAmountTotal);

            model.AmountTotalInCash = model.AmountTotalSpent - model.AmountTotalTransfer - model.BuyerCardAmount;

            model.AmountTotalReturned = model.BuyerCashAmount - model.AmountTotalInCash;

            if (btnEneableOrderFolio)
            {
                model.Products = model.Products.OrderBy(x => x.Invoice).ToList();
            }

            var buyerPaymentModel = new List<BuyerPaymentModel>();
            var paymentRequest = _buyerPaymentService.GetAll()
                .Where(x => x.ShippingDate == dateSelected && x.BuyerId == buyerId).ToList();
            var buyerPaymentIds = paymentRequest.Select(x => x.Id).ToList();
            var ticketBuyerFiles = _buyerPaymentTicketFileService.GetAll()
                .Where(x => buyerPaymentIds.Contains(x.BuyerPaymentId))
                .Select(x => new { x.FileId, x.BuyerPaymentId })
                .ToList();
            foreach (var item in paymentRequest)
            {
                buyerPaymentModel.Add(new BuyerPaymentModel()
                {
                    ProjectedAmount = 0,
                    CreationDate = item.CreatedOnUtc.ToLocalTime(),
                    RequestedAmount = item.RequestedAmount,
                    BuyerId = item.BuyerId,
                    BuyerPaymentId = item.Id,
                    Date = dateSelected,
                    InvoiceFilePdfId = item.InvoiceFilePdfId,
                    InvoiceFileXmlId = item.InvoiceFileXmlId,
                    ManufacturerId = item.ManufacturerId,
                    ManufacturerName = manufacturers.Where(x => x.Id == item.ManufacturerId).Select(x => x.Name).FirstOrDefault(),
                    PaymentFileId = item.PaymentFileId,
                    PaymentStatus = (BuyerPaymentStatus)item.PaymentStatusId,
                    BuyerName = model.BuyerName,
                    TicketBuyerFileIds = ticketBuyerFiles
                    .Where(x => x.BuyerPaymentId == item.Id)
                    .Select(x => x.FileId)
                    .ToList()
                });
            }

            model.BuyerPayments = buyerPaymentModel;

            return View("~/Plugins/Teed.Plugin.Groceries/Views/OrderDeliveryReports/OrderReportDetails.cshtml", model);
        }

        public IActionResult OrderNoBuyedReportDetails(string date, bool btnEneableOrderFolio = false)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderDeliveryReports))
                return AccessDeniedView();

            if (date == null) return AccessDeniedView();
            var dateSelected = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            OrderReportDetailsViewModel model = new OrderReportDetailsViewModel();

            var orderReports = _orderReportService.GetAll()
                .Where(x => (!string.IsNullOrEmpty(x.NotBuyedReason) || x.NotBuyedReasonId.HasValue) && x.SentToSupermarket.Value != true && x.OrderShippingDate == dateSelected)
                .GroupBy(x => x.ProductId)
                .ToList();
            var productIds = orderReports.Select(x => x.Key).ToList();
            var manufacturers = _manufacturerService.GetAllManufacturers().OrderBy(x => x.Name);
            var products = _productService.GetAllProductsQuery().Where(x => productIds.Contains(x.Id)).ToList();

            var orderItemIds = orderReports.Select(x => x.Select(y => y.OrderItemId.ToString())).SelectMany(x => x).ToList();
            var orderIds = orderReports.Select(x => x.Select(y => y.OrderId)).SelectMany(x => x).ToList();

            var orders = OrderUtils.GetFilteredOrders(_orderService)
             .Where(x => orderIds.Contains(x.Id))
             .ToList();
            List<OrderItem> parsedOrderItems = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService);

            var currentProductIds = parsedOrderItems.Select(x => x.ProductId).Distinct().ToList();
            var reportStatus = _orderReportStatusService.GetAll().Where(x => x.StatusTypeId == 2 && x.ShippingDate < dateSelected)
                .GroupBy(x => new { x.ShippingDate, x.BuyerId })
                .Select(x => x.Key)
                .ToList();
            var reports = _orderReportService.GetAll()
                .Select(x => new SimpleReportModel()
                {
                    OrderShippingDate = x.OrderShippingDate,
                    OriginalBuyerId = x.OriginalBuyerId,
                    ProductId = x.ProductId,
                    Invoice = x.Invoice,
                    SentToSupermarket = x.SentToSupermarket,
                    UpdatedUnitCost = x.UpdatedUnitCost
                })
                .Where(x => productIds.Contains(x.ProductId) && x.Invoice != null && x.OrderShippingDate < dateSelected)
                .ToList()
                .Where(x => reportStatus.Contains(new { ShippingDate = x.OrderShippingDate, BuyerId = x.OriginalBuyerId }))
                .ToList();
            var buyerIds = reports.Select(x => x.OriginalBuyerId).Distinct().ToList();
            var reportBuyers = _customerService.GetAllCustomersQuery().Where(x => buyerIds.Contains(x.Id)).ToList();

            model.OrderShippigDate = dateSelected;
            model.DayOfWeek = dateSelected.ToString("dddd", new CultureInfo("es-MX"));

            model.Manufacturers = new SelectList(manufacturers, "Id", "Name");
            model.Products = orderReports.Select(x => new ProductData()
            {
                ProductId = x.Key,
                ProductName = _productService.GetProductById(x.Key).Name,
                ProductCostKgPz = x.Select(y => y.UnitCost).FirstOrDefault(),
                ProductQuantity = x.Select(y => y.Quantity.Value).FirstOrDefault(),
                ProductAmountTotal = x.Select(y => y.RequestedQtyCost).FirstOrDefault(),
                NoBuyedReazon = x.Select(y => y.NotBuyedReason).FirstOrDefault(),
                Manufacturer = x.Select(y => y.ManufacturerId).FirstOrDefault() > 0 ?
                               manufacturers.Where(y => y.Id == x.Select(z => z.ManufacturerId).FirstOrDefault()).FirstOrDefault()?.Name : x.Select(y => y.ShoppingStoreId).FirstOrDefault(),
                ManufacturerId = x.Select(y => y.ManufacturerId).FirstOrDefault() > 0 ?
                               manufacturers.Where(y => y.Id == x.Select(z => z.ManufacturerId).FirstOrDefault()).FirstOrDefault().Id : -1,
                RequestedQuantity = GetTotalQuantity(products.Where(y => y.Id == x.Key).FirstOrDefault(), parsedOrderItems.Where(y => y.ProductId == x.Key).ToList()),
                RequestedUnit = GetUnitProduct(products.Where(y => y.Id == x.Key).FirstOrDefault()),
                NumberOrders = x.Select(y => y.OrderId).Count(),
                ProductManufactures = GetProductManufacturers(products.Where(y => y.Id == x.Key).FirstOrDefault(),
                                                              x.Select(y => y.ManufacturerId).FirstOrDefault() > 0 ?
                                                                manufacturers.Where(y => y.Id == x.Select(z => z.ManufacturerId).FirstOrDefault()).FirstOrDefault()?.Name :
                                                                x.Select(y => y.ShoppingStoreId).FirstOrDefault()),
                BuyerId = x.Select(y => y.OriginalBuyerId).FirstOrDefault(),
                Invoice = x.Where(y => y.Invoice != null).Select(y => y.Invoice).FirstOrDefault(),
                PreviousCostList = GetOrderReportCostsForProducts(reports, reportBuyers, x.Key)
            }).OrderBy(x => x.ProductName).ToList();

            if (btnEneableOrderFolio)
            {
                model.Products = model.Products.OrderBy(x => x.Invoice).ToList();
            }

            return View("~/Plugins/Teed.Plugin.Groceries/Views/OrderDeliveryReports/OrderNoBuyedReportDetails.cshtml", model);
        }

        public List<SelectedList> GetProductManufacturers(Nop.Core.Domain.Catalog.Product product, string shoppingStoreId)
        {
            return product.ProductManufacturers.Select(x => new SelectedList()
            {
                Id = x.ManufacturerId,
                Name = x.Manufacturer.Name,
                IsSelected = x.Manufacturer.Name == shoppingStoreId
            }).ToList();
        }

        private RoundedAmount RoundBuyerCashAmount(decimal costSum)
        {
            if (costSum == 0) return new RoundedAmount() { Amount = 0, AmountString = 0.ToString("C") };
            costSum += 50;
            costSum = Math.Ceiling(costSum / 100) * 100;
            return new RoundedAmount() { Amount = costSum, AmountString = costSum.ToString("C") };
        }

        private OrderReportFile GetReportFileByProduct(int productId, List<OrderReportFile> orderReportFiles)
        {
            List<string> productsIds = new List<string>();
            var productsIdsInOrderReportFiles = orderReportFiles.Where(x => x.ProductIds.Contains(productId.ToString())).Select(x => x.ProductIds).ToList();
            foreach (var item in productsIdsInOrderReportFiles)
            {
                var arrayInItem = item.Split(',');
                foreach (var wordInArray in arrayInItem)
                {
                    if (productId == int.Parse(wordInArray))
                    {
                        productsIds.Add(item);
                    }
                }
            }
            var findOrderReportFilesWhitProduct = orderReportFiles.Where(x => productsIds.Contains(x.ProductIds));
            var file = findOrderReportFilesWhitProduct.FirstOrDefault();
            return file;
        }

        private decimal GetTotalQuantity(Nop.Core.Domain.Catalog.Product product, List<OrderItem> orderItems)
        {
            decimal quantity = orderItems.Select(x => x.Quantity).DefaultIfEmpty().Sum();
            decimal result = quantity;
            if (product.EquivalenceCoefficient > 0)
                result = ((quantity * 1000) / product.EquivalenceCoefficient) / 1000;
            else if (product.WeightInterval > 0)
                result = (quantity * product.WeightInterval) / 1000;
            return Math.Round(result, 2);
        }

        private string GetUnitProduct(Nop.Core.Domain.Catalog.Product product)
        {
            return (product.EquivalenceCoefficient > 0 || product.WeightInterval > 0) ? "kg" : "pz";
        }

        [HttpPost]
        public IActionResult UpdateReportsOrder(OrderReportDetailsViewModel body)
        {
            if (body.OrderShippigDate == null) { return AccessDeniedView(); }

            List<ProductsJson> jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductsJson>>(body.JsonData);
            var logText = string.Empty;

            foreach (var json in jsonData)
            {
                var reportSameProducts = _orderReportService.GetAll().Where(x => x.OriginalBuyerId == body.BuyerId && x.OrderShippingDate == body.OrderShippigDate && x.ProductId == json.ProductId && x.Deleted == false).ToList();
                var getByProduct = reportSameProducts.Where(x => x.ProductId == 9694);

                if (reportSameProducts == null || reportSameProducts.Count() == 0)
                {
                    var orders = _orderService.GetOrders()
                                              .Where(x => x.SelectedShippingDate.HasValue && x.SelectedShippingDate.Value == body.OrderShippigDate)
                                              .Where(x => x.OrderStatus != OrderStatus.Cancelled &&
                                              !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatus == Nop.Core.Domain.Payments.PaymentStatus.Pending))
                                              .ToList();

                    List<OrderItem> parsedOrderItems = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService);

                    List<int> orderItemIdsOfTheDay = parsedOrderItems.Select(x => x.Id).ToList();
                    var orderItemBuyerQuery = _orderItemBuyerService.GetAll().Where(x => orderItemIdsOfTheDay.Contains(x.OrderItemId));
                    orderItemBuyerQuery = orderItemBuyerQuery.Where(x => x.CustomerId == body.BuyerId);
                    var orderItemBuyer = orderItemBuyerQuery.ToList();
                    var orderItemIdsFilter = orderItemBuyer.Where(x => x.CustomerId == body.BuyerId).Select(x => x.OrderItemId);
                    var filteredOrderItemsByProductId = parsedOrderItems.Where(x => orderItemIdsFilter.Contains(x.Id) && x.ProductId == json.ProductId).ToList();

                    foreach (var item in filteredOrderItemsByProductId)
                    {
                        OrderReport newOrderReport = new OrderReport
                        {
                            ManufacturerId = json.ManufacturerId,
                            NotBuyedReason = json.NoBuyedRazon,
                            OrderId = item.OrderId,
                            OrderItemId = item.Id,
                            OrderShippingDate = body.OrderShippigDate,
                            ProductId = json.ProductId,
                            ShoppingStoreId = json.ShoppingStoreId,
                            OriginalBuyerId = body.BuyerId,
                            NotBuyedReasonId = null,
                            ReportedDateUtc = DateTime.UtcNow,
                            UpdatedByUserId = _workContext.CurrentCustomer.Id,
                            RequestedQtyCost = json.ProductRequestCost,
                            UpdatedRequestedQtyCost = json.ProductRequestCost,
                            UnitCost = json.ProductUnitCost,
                            UpdatedUnitCost = json.ProductUnitCost,
                            Quantity = json.ProductQuantity,
                            UpdatedQuantity = json.ProductQuantity,
                            CreatedOnUtc = DateTime.UtcNow,
                            UpdatedOnUtc = DateTime.UtcNow,
                            Comments = null,
                            BoughtTypeId = json.BoughtTypeId,
                        };

                        _orderReportService.Insert(newOrderReport);

                        logText = "Agregó un reporte.";

                        OrderReportLog orderReportLog = new OrderReportLog()
                        {
                            OrderId = newOrderReport.OrderId,
                            Log = logText,
                            AuthorId = _workContext.CurrentCustomer.Id
                        };
                        _orderReportLogService.Insert(orderReportLog);
                    }

                }
                else
                {
                    foreach (var reportProduct in reportSameProducts)
                    {
                        var orderReport = _orderReportService.GetAll().Where(x => x.Id == reportProduct.Id).FirstOrDefault();

                        orderReport.UpdatedRequestedQtyCost = json.ProductRequestCost;

                        orderReport.UpdatedUnitCost = json.ProductUnitCost;

                        orderReport.UpdatedQuantity = json.ProductQuantity;

                        orderReport.ManufacturerId = json.ManufacturerId;
                        orderReport.ShoppingStoreId = string.IsNullOrWhiteSpace(json.ShoppingStoreId) ? null : json.ShoppingStoreId;
                        orderReport.NotBuyedReason = string.IsNullOrWhiteSpace(json.NoBuyedRazon) ? null : json.NoBuyedRazon;

                        orderReport.UpdatedByUserId = _workContext.CurrentCustomer.Id;
                        orderReport.BoughtTypeId = json.BoughtTypeId;


                        logText = "Actualizó un reporte.";
                        _orderReportService.Update(orderReport);


                        OrderReportLog orderReportLog = new OrderReportLog()
                        {
                            OrderId = orderReport.OrderId,
                            Log = logText,
                            AuthorId = _workContext.CurrentCustomer.Id
                        };
                        _orderReportLogService.Insert(orderReportLog);
                    }
                }

            }

            FileData fileData = new FileData();
            if (body.InputFileTransfer != null)
            {
                fileData = ImageToByteArray(body.InputFileTransfer);
            }

            var orderReporTransfer = _orderReportTransferService.GetAll().Where(x => x.BuyerId == body.BuyerId && x.OrderShippingDate == body.OrderShippigDate && x.Id == body.OrderReportTransferId).FirstOrDefault();
            if (orderReporTransfer == null)
            {
                orderReporTransfer = new OrderReportTransfer()
                {
                    BuyerId = body.BuyerId,
                    TransferAmount = body.AmountTotalTransfer,
                    OrderShippingDate = body.OrderShippigDate,
                    File = fileData.BytesFile,
                    Extension = fileData.Extension,
                    FileUploadedByUserId = _workContext.CurrentCustomer.Id,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow
                };

                _orderReportTransferService.Insert(orderReporTransfer);
            }
            else
            {
                orderReporTransfer.TransferAmount = body.AmountTotalTransfer;
                orderReporTransfer.File = body.InputFileTransfer == null ? orderReporTransfer.File : fileData.BytesFile;
                orderReporTransfer.Extension = body.InputFileTransfer == null ? orderReporTransfer.Extension : fileData.Extension;
                orderReporTransfer.FileUploadedByUserId = _workContext.CurrentCustomer.Id;
                orderReporTransfer.UpdatedOnUtc = DateTime.UtcNow;

                _orderReportTransferService.Update(orderReporTransfer);
            }

            if (body.AuthorizeData)
            {
                ConfirmOrderReport(body.BuyerId, body.OrderShippigDate);
            }

            return RedirectToAction("OrderReportDetails", new { buyerId = body.BuyerId, date = body.OrderShippigDate.ToString("dd-MM-yyyy") });
        }

        //[HttpGet]
        //public IActionResult ManulaTestOrderReport(int buyerId, string date, int productId)
        //{
        //    var parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //    ConfirmOrderReport(buyerId, parsedDate, productId);
        //    return Ok();
        //}

        [HttpPost]
        public IActionResult UpdateReportsNoBuyedOrder(OrderReportDetailsViewModel body)
        {
            if (body.OrderShippigDate == null ||
                !_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderDeliveryReports))
            { return AccessDeniedView(); }

            List<ProductsJson> jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductsJson>>(body.JsonData);

            var logText = string.Empty;

            foreach (var json in jsonData)
            {
                var reportSameProducts = _orderReportService.GetAll().Where(x => x.OriginalBuyerId == json.BuyerId && x.OrderShippingDate == body.OrderShippigDate && x.ProductId == json.ProductId).ToList();

                foreach (var reportProduct in reportSameProducts)
                {
                    var orderReport = _orderReportService.GetAll().Where(x => x.Id == reportProduct.Id).FirstOrDefault();

                    orderReport.RequestedQtyCost = json.ProductRequestCost;
                    orderReport.UnitCost = json.ProductUnitCost;
                    orderReport.Quantity = json.ProductQuantity;
                    orderReport.ManufacturerId = json.ManufacturerId;
                    orderReport.ShoppingStoreId = json.ShoppingStoreId;
                    orderReport.UpdatedByUserId = _workContext.CurrentCustomer.Id;

                    logText = "Actualizó un reporte de un producto no comprado, productId [" + reportProduct.ProductId + "].";
                    _orderReportService.Update(orderReport);


                    OrderReportLog orderReportLog = new OrderReportLog()
                    {
                        OrderId = orderReport.OrderId,
                        Log = logText,
                        AuthorId = _workContext.CurrentCustomer.Id
                    };
                    _orderReportLogService.Insert(orderReportLog);
                }

            }

            return RedirectToAction("OrderNoBuyedReportDetails", new { date = body.OrderShippigDate.ToString("dd-MM-yyyy") });
        }

        public IActionResult ValidityNewManufacturers(string date)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderDeliveryReports))
                return AccessDeniedView();

            if (date == null) return AccessDeniedView();
            var dateSelected = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            ValidityManufacturers model = new ValidityManufacturers()
            {
                Date = dateSelected,
                DateString = date
            };
            var orderReports = _orderReportService.GetAll()
                                                  .Where(x => x.OrderShippingDate == dateSelected && x.Deleted == false && (!string.IsNullOrEmpty(x.ShoppingStoreId) && x.ManufacturerId == -1))
                                                  .GroupBy(x => x.ShoppingStoreId)
                                                  .ToList();
            model.Manufactures = orderReports.Select(x => new ManufacturesData()
            {
                ShoppingStoreId = x.Key,
                NewShoppingStoreId = x.Key,
                ManufacturerId = x.Select(y => y.ManufacturerId.Value).FirstOrDefault(),
                BuyerId = x.Select(y => y.OriginalBuyerId).FirstOrDefault(),
                BuyerName = _customerService.GetCustomerById(x.Select(y => y.OriginalBuyerId).FirstOrDefault()).GetFullName()
            }).ToList();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/OrderDeliveryReports/ValidityNewManufacturers.cshtml", model);

        }

        [HttpPost]
        public IActionResult ValidityNewManufacturers(ValidityManufacturers body)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            try
            {
                if (body.DateString == null) return AccessDeniedView();
                body.Date = DateTime.ParseExact(body.DateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                List<ManufacturesData> jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ManufacturesData>>(body.ManufacturersJson);

                foreach (var data in jsonData)
                {
                    List<OrderReport> orderReports = _orderReportService.GetAll()
                                                     .Where(x => x.OrderShippingDate == body.Date &&
                                                                 x.Deleted == false &&
                                                                 (!string.IsNullOrEmpty(x.ShoppingStoreId) &&
                                                                 x.ManufacturerId == -1) &&
                                                                 x.ShoppingStoreId == data.ShoppingStoreId &&
                                                                 x.OriginalBuyerId == data.BuyerId)
                                                     .ToList();
                    foreach (var orderReport in orderReports)
                    {

                        orderReport.ShoppingStoreId = data.NewShoppingStoreId;

                        var logText = "Se actualizó el fabricante reportado. Fabricante anterior: " + data.ShoppingStoreId + ". Nuevo fabricante: " + data.NewShoppingStoreId;
                        _orderReportService.Update(orderReport);

                        OrderReportLog orderReportLog = new OrderReportLog()
                        {
                            OrderId = orderReport.OrderId,
                            Log = logText,
                            AuthorId = _workContext.CurrentCustomer.Id
                        };
                        _orderReportLogService.Insert(orderReportLog);
                    }
                }

                return RedirectToAction("ValidityNewManufacturers", new { date = body.DateString });
            }
            catch (Exception e)
            {
                return BadRequest("Ocurrió un error inesperado " + e);
                //throw;
            }
        }


        public IActionResult ManagerExpenses(string date)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            if (date == null) return AccessDeniedView();
            var dateSelected = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);


            ManagerExpensesViewModel model = new ManagerExpensesViewModel();
            model.DeliveryDate = dateSelected;
            model.DayOfWeek = dateSelected.ToString("dddd", new CultureInfo("es-MX"));

            var manufacturers = _manufacturerService.GetAllManufacturers().ToList();
            var productMainManufacturers = _productMainManufacturerService.GetAll().ToList();

            model.ManagerExpenses = _managerExpensesService.GetAll().Where(x => x.ShippingDate == dateSelected && x.Deleted == false)
                                                           .Select(x => new ManagerExpenseData()
                                                           {
                                                               Id = x.Id,
                                                               Amount = x.Amount,
                                                               Concept = x.Concept
                                                           }).OrderBy(x => x.Id).ToList();

            var orders = _orderItem.GetAllOrdersQuery().Where(x => x.SelectedShippingDate == dateSelected).ToList();

            List<OrderItem> parsedOrderItems = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService);
            List<int> orderItemIdsOfTheDay = parsedOrderItems.Select(x => x.Id).ToList();
            var allBuyersIdsOfOrderItems = _orderItemBuyerService.GetAll().Where(x => orderItemIdsOfTheDay.Contains(x.OrderItemId)).GroupBy(x => x.CustomerId).Select(x => x.Key).ToList();

            List<BuyerAmountModel> totalBuyerAmount = new List<BuyerAmountModel>();
            List<RoundedAmount> cashAmountToBuyers = new List<RoundedAmount>();
            var orderItemBuyerQuery = _orderItemBuyerService.GetAll().Where(x => orderItemIdsOfTheDay.Contains(x.OrderItemId));
            foreach (var buyerId in allBuyersIdsOfOrderItems)
            {
                var orderItemBuyer = orderItemBuyerQuery.Where(x => x.CustomerId == buyerId).ToList();
                var buyerMoney = OrderUtils.GetBuyerCashAndTransferAmount(parsedOrderItems,
                buyerId,
                orderItemBuyer,
                manufacturers,
                productMainManufacturers);
                totalBuyerAmount.Add(buyerMoney);

                var cashToBuyer = RoundBuyerCashAmount(buyerMoney.Cash);
                cashAmountToBuyers.Add(cashToBuyer);
            }

            var orderReports = _orderReportService.GetAll()
                                                  .Where(x => allBuyersIdsOfOrderItems.Contains(x.OriginalBuyerId) && x.OrderShippingDate == dateSelected && x.Deleted == false)
                                                  .GroupBy(x => x.ProductId).ToList();

            var sumUpdatedProductAmountInOrders = Math.Round(orderReports.Select(y => y.Select(z => z.UpdatedRequestedQtyCost).FirstOrDefault()).ToList().Sum(x => x.Value), 2);
            var sumOriginalProductAmountInOrders = orderReports.Where(x => !x.Select(y => y.UpdatedRequestedQtyCost).FirstOrDefault().HasValue)
                                                               .Select(y => y.Select(z => z.RequestedQtyCost).FirstOrDefault())
                                                               .ToList()
                                                               .Sum(x => x);

            var amountTotalSpent = sumUpdatedProductAmountInOrders + sumOriginalProductAmountInOrders;

            model.CardAmountTotal = totalBuyerAmount.Sum(x => x.Card);
            model.TransferAmountTotal = _orderReportTransferService.GetAll()
                                                                   .Where(x => x.OrderShippingDate == dateSelected)
                                                                   .ToList()
                                                                   .Sum(x => x.TransferAmount);

            model.CashAmountDeliveredBuyers = cashAmountToBuyers.Sum(x => x.Amount);

            model.CashAmountTotal = amountTotalSpent - model.TransferAmountTotal - model.CardAmountTotal;
            model.ReturnedAmountByBuyers = model.CashAmountDeliveredBuyers - model.CashAmountTotal;

            var managerQuantities = _managerQuantitiesService.GetAll().Where(x => x.ShippingDate == dateSelected && !x.Deleted).FirstOrDefault();

            model.InitialAmount = managerQuantities == null ? 0 : managerQuantities.InitialAmount;
            model.AmountManagerReceives = managerQuantities == null ? 0 : managerQuantities.AmountManagerReceives;
            model.ExpenseAmountManager = model.ManagerExpenses.Sum(x => x.Amount);
            model.RestAmount = model.InitialAmount - model.ExpenseAmountManager;

            var statusExpenseManager = _managerExpensesStatusService.GetAll().Where(x => x.ShippingDate == dateSelected && !x.Deleted).FirstOrDefault();
            model.IsClosedLiquidation = statusExpenseManager == null ? false : statusExpenseManager.ClosedDateUtc.HasValue;
            model.CurrentUserName = _workContext.CurrentCustomer.GetFullName();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/OrderDeliveryReports/ManagerExpenses.cshtml", model);
        }

        [HttpPost]
        public IActionResult ManagerExpenses(ManagerExpensesViewModel body)
        {
            var managerQuantities = _managerQuantitiesService.GetAll().Where(x => x.ShippingDate == body.DeliveryDate && !x.Deleted).FirstOrDefault();

            if (managerQuantities == null)
            {
                var logText = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) realizo un registro de cantidades.\n";
                managerQuantities = new ManagerQuantities()
                {
                    ShippingDate = body.DeliveryDate,
                    InitialAmount = body.InitialAmount,
                    AmountManagerReceives = body.AmountManagerReceives,
                    Log = logText
                };
                _managerQuantitiesService.Insert(managerQuantities);
            }
            else
            {
                var logText = string.Empty;
                if (managerQuantities.InitialAmount != body.InitialAmount)
                {
                    logText = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó el monto inicial {managerQuantities.InitialAmount} por {body.InitialAmount}.\n";
                    managerQuantities.InitialAmount = body.InitialAmount;
                    managerQuantities.Log += logText;
                }

                if (managerQuantities.AmountManagerReceives != body.AmountManagerReceives)
                {
                    logText = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó el monto recibido por el coordinador {managerQuantities.AmountManagerReceives} por {body.AmountManagerReceives}.\n";
                    managerQuantities.AmountManagerReceives = body.AmountManagerReceives;
                    managerQuantities.Log += logText;
                }
                _managerQuantitiesService.Update(managerQuantities);
            }

            var managerExpensesByDate = _managerExpensesService.GetAll().Where(x => x.ShippingDate == body.DeliveryDate && x.Deleted == false);
            List<ManagerExpenseData> managerExpensesData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ManagerExpenseData>>(body.ManagerExpensesJson);
            foreach (var expense in managerExpensesData)
            {
                var logText = string.Empty;
                ManagerExpense managerExpense = managerExpensesByDate.Where(x => x.Id == expense.Id).FirstOrDefault();
                if (managerExpense == null)
                {
                    logText = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) registro un gasto.\n";
                    managerExpense = new ManagerExpense()
                    {
                        ShippingDate = body.DeliveryDate,
                        Amount = expense.Amount,
                        Concept = expense.Concept,
                        UserId = _workContext.CurrentCustomer.Id,
                        Log = logText
                    };
                    _managerExpensesService.Insert(managerExpense);
                }
                else
                {
                    if (managerExpense.Amount != expense.Amount)
                    {
                        logText = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó el monto del gasto { managerExpense.Amount} por {expense.Amount}.\n";
                        managerExpense.Amount = expense.Amount;
                        managerExpense.Log += logText;
                    }
                    if (managerExpense.Concept != expense.Concept)
                    {
                        logText = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó el concepto del gasto [{ managerExpense.Concept}] por [{expense.Concept}].\n";
                        managerExpense.Concept = expense.Concept;
                        managerExpense.Log += logText;
                    }

                    if (managerExpense.UserId != _workContext.CurrentCustomer.Id)
                    {
                        logText = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) actualizó el gasto despues de {_customerService.GetCustomerById(managerExpense.UserId).Email} ({managerExpense.UserId}).\n";
                        managerExpense.UserId = _workContext.CurrentCustomer.Id;
                        managerExpense.Log += logText;
                    }
                    _managerExpensesService.Update(managerExpense);
                }
            }

            return RedirectToAction("ManagerExpenses", new { date = body.DeliveryDate.ToString("dd-MM-yyyy") });
        }

        [HttpDelete]
        public IActionResult DeleteManagerExpense(int id)
        {
            var managerExpense = _managerExpensesService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (managerExpense == null)
            {
                return BadRequest();
            }
            var logText = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) eliminó el este registro.\n";
            managerExpense.Deleted = true;
            managerExpense.Log += logText;
            _managerExpensesService.Update(managerExpense);
            return Ok();
        }

        [HttpPost]
        public IActionResult ConfirmManagerExpenses(string date)
        {
            if (date == null) return AccessDeniedView();
            var shippingDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime today = DateTime.Now;
            var managerExpensesStatus = _managerExpensesStatusService.GetAll().Where(x => x.ShippingDate == shippingDate && !x.Deleted).FirstOrDefault();
            if (managerExpensesStatus == null)
            {
                managerExpensesStatus = new ManagerExpensesStatus()
                {
                    ShippingDate = shippingDate,
                    ClosedById = _workContext.CurrentCustomer.Id,
                    ClosedDateUtc = today,
                    Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) liquidó las cuentas del coordinador del día { shippingDate.ToString("dddd", new CultureInfo("es-MX"))} - { shippingDate.ToString("dd-MM-yyyy")} el día {today.ToString("dddd", new CultureInfo("es-MX"))} - {today.ToString("dd-MM-yyyy")}.\n",
                };
                _managerExpensesStatusService.Insert(managerExpensesStatus);
            }
            else
            {
                var logText = string.Empty;
                if (!managerExpensesStatus.ClosedDateUtc.HasValue)
                {
                    logText = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambio la fecha de liquidación de las cuentas del coordinador al día {today.ToString("dddd", new CultureInfo("es-MX"))} - {today.ToString("dd-MM-yyyy")}.\n";
                    managerExpensesStatus.ClosedDateUtc = today;
                    managerExpensesStatus.ClosedById = _workContext.CurrentCustomer.Id;
                    managerExpensesStatus.Log += logText;
                }

                _managerExpensesStatusService.Update(managerExpensesStatus);
            }
            return Ok();
        }


        public IActionResult OrderSuperReportDetails(string date, bool btnEneableOrderFolio = false)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderDeliveryReports))
                return AccessDeniedView();

            if (date == null) return AccessDeniedView();
            var dateSelected = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            OrderReportDetailsViewModel model = new OrderReportDetailsViewModel();

            var orderReports = _orderReportService.GetAll().Where(x => x.SentToSupermarket.HasValue && x.SentToSupermarket.Value == true && x.OrderShippingDate == dateSelected).GroupBy(x => x.ProductId).ToList();
            var manufacturers = _manufacturerService.GetAllManufacturers().OrderBy(x => x.Name);

            var orderItemIds = orderReports.Select(x => x.Select(y => y.OrderItemId.ToString())).SelectMany(x => x).ToList();
            var orderIds = orderReports.Select(x => x.Select(y => y.OrderId)).SelectMany(x => x).ToList();
            model.OrderShippigDate = dateSelected;
            model.DayOfWeek = dateSelected.ToString("dddd", new CultureInfo("es-MX"));

            var productIds = orderReports.Select(x => x.Key).ToList();
            var products = _productService.GetAllProductsQuery().Where(x => productIds.Contains(x.Id)).ToList();

            var orders = OrderUtils.GetFilteredOrders(_orderService)
             .Where(x => orderIds.Contains(x.Id))
             .ToList();
            List<OrderItem> parsedOrderItems = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService);

            var currentProductIds = parsedOrderItems.Select(x => x.ProductId).Distinct().ToList();
            var reportStatus = _orderReportStatusService.GetAll().Where(x => x.StatusTypeId == 2 && x.ShippingDate < dateSelected)
                .GroupBy(x => new { x.ShippingDate, x.BuyerId })
                .Select(x => x.Key)
                .ToList();
            var reports = _orderReportService.GetAll()
                .Select(x => new SimpleReportModel()
                {
                    OrderShippingDate = x.OrderShippingDate,
                    OriginalBuyerId = x.OriginalBuyerId,
                    ProductId = x.ProductId,
                    Invoice = x.Invoice,
                    SentToSupermarket = x.SentToSupermarket,
                    UpdatedUnitCost = x.UpdatedUnitCost
                })
                .Where(x => productIds.Contains(x.ProductId) && x.Invoice != null && x.OrderShippingDate < dateSelected)
                .ToList()
                .Where(x => reportStatus.Contains(new { ShippingDate = x.OrderShippingDate, BuyerId = x.OriginalBuyerId }))
                .ToList();
            var buyerIds = reports.Select(x => x.OriginalBuyerId).Distinct().ToList();
            var reportBuyers = _customerService.GetAllCustomersQuery().Where(x => buyerIds.Contains(x.Id)).ToList();

            model.Manufacturers = new SelectList(manufacturers, "Id", "Name");
            model.Products = orderReports.Select(x => new ProductData()
            {
                ProductId = x.Key,
                ProductName = _productService.GetProductById(x.Key).Name,
                ProductCostKgPz = x.Select(y => y.UnitCost).FirstOrDefault(),
                ProductQuantity = x.Select(y => y.Quantity.Value).FirstOrDefault(),
                ProductAmountTotal = x.Select(y => y.RequestedQtyCost).FirstOrDefault(),
                NoBuyedReazon = x.Select(y => y.NotBuyedReason).FirstOrDefault(),
                Manufacturer = x.Select(y => y.ManufacturerId).FirstOrDefault() > 0 ?
                               manufacturers.Where(y => y.Id == x.Select(z => z.ManufacturerId).FirstOrDefault()).FirstOrDefault()?.Name : x.Select(y => y.ShoppingStoreId).FirstOrDefault(),
                ManufacturerId = x.Select(y => y.ManufacturerId).FirstOrDefault() > 0 ?
                               manufacturers.Where(y => y.Id == x.Select(z => z.ManufacturerId).FirstOrDefault()).FirstOrDefault().Id : -1,
                RequestedQuantity = GetTotalQuantity(products.Where(y => y.Id == x.Key).FirstOrDefault(), parsedOrderItems.Where(y => y.ProductId == x.Key).ToList()),
                RequestedUnit = GetUnitProduct(products.Where(y => y.Id == x.Key).FirstOrDefault()),
                NumberOrders = x.Select(y => y.OrderId).Count(),
                ProductManufactures = GetProductManufacturers(products.Where(y => y.Id == x.Key).FirstOrDefault(),
                                                              x.Select(y => y.ManufacturerId).FirstOrDefault() > 0 ?
                                                                manufacturers.Where(y => y.Id == x.Select(z => z.ManufacturerId).FirstOrDefault()).FirstOrDefault()?.Name :
                                                                x.Select(y => y.ShoppingStoreId).FirstOrDefault()),
                BuyerId = x.Select(y => y.OriginalBuyerId).FirstOrDefault(),
                Invoice = x.Where(y => y.Invoice != null).Select(y => y.Invoice).FirstOrDefault(),
                SentToSupermarketByUser = x.Select(y => y.SentToSupermarketByUserId).FirstOrDefault().HasValue ? _customerService.GetCustomerById(x.Select(y => y.SentToSupermarketByUserId).FirstOrDefault().Value)?.GetFullName() : null,
                PreviousCostList = GetOrderReportCostsForProducts(reports, reportBuyers, x.Key)
            }).OrderBy(x => x.ProductName).ToList();

            if (btnEneableOrderFolio)
            {
                model.Products = model.Products.OrderBy(x => x.Invoice).ToList();
            }

            return View("~/Plugins/Teed.Plugin.Groceries/Views/OrderDeliveryReports/OrderSuperReportDetails.cshtml", model);
        }

        public List<string> GetOrderReportCostsForProducts(List<SimpleReportModel> reports, List<Customer> customers, int productId)
        {
            return reports
                .Where(x => x.ProductId == productId)
                .GroupBy(x => x.OrderShippingDate)
                .OrderByDescending(x => x.Key)
                .Take(10)
                .Select(x => x.FirstOrDefault()).Select(x =>
                {
                    string buyerName = customers.Where(y => y.Id == x.OriginalBuyerId)?.FirstOrDefault()?.GetFullName();
                    string url = $"/Admin/OrderDeliveryReports/OrderReportDetails?buyerId={x.OriginalBuyerId}&date={x.OrderShippingDate:dd-MM-yyyy}";
                    if (x.SentToSupermarket.HasValue && x.SentToSupermarket.Value)
                    {
                        url = $"/Admin/OrderDeliveryReports/OrderSuperReportDetails?date={x.OrderShippingDate:dd-MM-yyyy}";
                    }
                    return $"{x.OrderShippingDate:dd-MM-yyyy} / {buyerName} / <a href=\"{url}\" target=\"_blank\">{x.UpdatedUnitCost:C}</a>";
                }).ToList();
        }

        [HttpPost]
        public IActionResult UpdateReportsSupermarketBuyedOrder(OrderReportDetailsViewModel body)
        {
            if (body.OrderShippigDate == null ||
                !_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderDeliveryReports))
            { return AccessDeniedView(); }

            List<ProductsJson> jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductsJson>>(body.JsonData);

            var logText = string.Empty;

            foreach (var json in jsonData)
            {
                var reportSameProducts = _orderReportService.GetAll().Where(x => x.OriginalBuyerId == json.BuyerId && x.OrderShippingDate == body.OrderShippigDate && x.ProductId == json.ProductId).ToList();

                foreach (var reportProduct in reportSameProducts)
                {
                    var orderReport = _orderReportService.GetAll().Where(x => x.Id == reportProduct.Id).FirstOrDefault();

                    orderReport.RequestedQtyCost = json.ProductRequestCost;
                    orderReport.UnitCost = json.ProductUnitCost;
                    orderReport.Quantity = json.ProductQuantity;
                    orderReport.ManufacturerId = json.ManufacturerId;
                    orderReport.ShoppingStoreId = json.ShoppingStoreId;
                    orderReport.UpdatedByUserId = _workContext.CurrentCustomer.Id;

                    logText = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) actualizó un reporte de un producto enviado al super, productId [" + reportProduct.ProductId + "]..\n";
                    _orderReportService.Update(orderReport);


                    OrderReportLog orderReportLog = new OrderReportLog()
                    {
                        OrderId = orderReport.OrderId,
                        Log = logText,
                        AuthorId = _workContext.CurrentCustomer.Id
                    };
                    _orderReportLogService.Insert(orderReportLog);
                }

            }

            return RedirectToAction("OrderNoBuyedReportDetails", new { date = body.OrderShippigDate.ToString("dd-MM-yyyy") });
        }


        public IActionResult CreateOrderReports(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            if (TempData["mensaje"] != null)
                ViewBag.mensaje = TempData["mensaje"].ToString();

            var dateDelivery = DateTime.Parse(id);
            var deliveryOrders = _orderService.GetOrders().Where(x => x.SelectedShippingDate == dateDelivery)
                                                                  .Where(x => x.OrderStatus != OrderStatus.Cancelled &&
                                                                            !(x.PaymentMethodSystemName == "Payments.PayPalStandard" &&
                                                                              x.PaymentStatus == Nop.Core.Domain.Payments.PaymentStatus.Pending))
                                                                  .ToList();



            IList<OrderItem> allProductsDelivery = new List<OrderItem>();
            foreach (var item in deliveryOrders)
            {
                foreach (var item2 in item.OrderItems)
                {
                    allProductsDelivery.Add(item2);
                }
            }

            allProductsDelivery = allProductsDelivery.OrderBy(x => x.Product.ProductCategories.Where(y => y.Category.ParentCategoryId == 0).Select(y => y.Category.DisplayOrder).FirstOrDefault())
                                                     .ThenBy(x => x.Product.ProductCategories.Where(y => y.Category.ParentCategoryId == 0).Select(y => y.Category.Name).FirstOrDefault())
                                                     .ThenBy(x => x.Product.ProductCategories.Where(y => y.Category.ParentCategoryId == 0).Select(y => y.DisplayOrder).FirstOrDefault()).ToList();

            var model = new CreateOrderReportsViewModel
            {
                DeliveryDate = dateDelivery,

                GroupsDistinc = allProductsDelivery.GroupBy(o => o.ProductId).Select(g => new Groups
                {
                    GroupProductId = g.Key,
                    GroupProductName = allProductsDelivery.Where(z => z.ProductId == g.Key).Select(y => y.Product.Name).FirstOrDefault(),
                    Products = g.Select(y => new ProductsInGroup
                    {
                        OrderId = y.OrderId,
                        OrderItemId = y.Id,
                        OrderItemName = y.Product.Name,
                        ItemCostKgPz = Math.Round(y.Product.Price, 2),
                        OrderItemCost = Math.Round(y.PriceInclTax, 2),
                        OrderItemSpecifications = y.SelectedPropertyOption,
                        OrderItemQuantity = y.Quantity,
                        OrderItemQuantitySpecific = OrderItemQuantitySpecific(y.Id),
                    }).ToList()
                }).ToList()

            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/OrderDeliveryReports/CreateOrderReports.cshtml", model);
        }

        public string OrderItemQuantitySpecific(int orderItemId)
        {
            var orderItem = _orderService.GetOrderItemById(orderItemId);
            var weight = string.Empty;
            decimal result = 0;
            if (orderItem.EquivalenceCoefficient > 0)
            {
                result = (orderItem.Quantity * 1000) / orderItem.EquivalenceCoefficient;
            }
            else if (orderItem.WeightInterval > 0)
            {
                result = orderItem.Quantity * orderItem.WeightInterval;
            }

            if (result >= 1000)
            {
                weight = (result / 1000).ToString("0.##") + " kg";
            }
            else if (result > 0)
            {
                weight = result.ToString("0.##") + " gr";
            }

            var qty = (orderItem.BuyingBySecondary && !string.IsNullOrWhiteSpace(weight)) || orderItem.WeightInterval > 0 ? $"{weight}" : $"{orderItem.Quantity.ToString()} pz";

            return qty;
        }



        [HttpPost]
        public IActionResult CreateOrderReports(CreateOrderReportsViewModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            List<ProductsInGroup> jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductsInGroup>>(model.JsonData);
            foreach (var item in jsonData)
            {
                var existOrderItemId = _orderReportService.GetAll().Where(x => x.OrderItemId == item.OrderItemId).Any();
                if (!existOrderItemId)
                {
                    OrderReport orderReport = new OrderReport()
                    {
                        OrderId = item.OrderId,
                        OrderItemId = item.OrderItemId,
                        RequestedQtyCost = item.RequestedQtyCost, /// costo del pedido
                        UnitCost = CalculateUnitCostToReportOrder(item.OrderItemId, item.RequestedQtyCost),///precio kg/pz en la bodega donde se compro
                        ShoppingStoreId = item.ShoppingStoreId,
                        Comments = item.OrderItemComments,
                    };
                    _orderReportService.Insert(orderReport);
                }
                else
                {
                    TempData["mensaje"] = "No puedes repetir reportes para productos de orden. Regresa a la lista de fechas de entregas y vuelve a entrar a los reportes de esta fecha (el botón aparecerá como 'editar').";
                    return RedirectToAction("CreateOrderReports/" + model.DeliveryDate);
                }
            }

            if (model.ExpenseJsonData != null)
            {
                List<CreateOrderReportsViewModel> expenseJsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CreateOrderReportsViewModel>>(model.ExpenseJsonData);
                foreach (var item in expenseJsonData)
                {
                    AditionalCost aditionalCost = new AditionalCost()
                    {
                        Date = model.DeliveryDate,
                        Cost = item.ExpenseAmount,
                        Description = item.ExpenseDescription
                    };
                    _aditionalCostService.Insert(aditionalCost);
                }
            }

            return RedirectToAction("DeliveryDateList");
        }

        public decimal CalculateUnitCostToReportOrder(int orderItemId, decimal requestedQtyCost)
        {
            var orderItem = _orderService.GetOrderItemById(orderItemId);
            decimal priceByKgPzInStore = 0;
            decimal quantityInKG = 0;
            decimal quantityInPz = 0;
            if (orderItem.EquivalenceCoefficient > 0)
            {
                var quantityInGrams = (orderItem.Quantity * 1000) / orderItem.EquivalenceCoefficient;
                quantityInKG = quantityInGrams / 1000;
            }
            else if (orderItem.WeightInterval > 0)
            {
                var quantityInGrams = orderItem.Quantity * orderItem.WeightInterval;
                quantityInKG = quantityInGrams / 1000;
            }
            else
            {
                quantityInPz = orderItem.Quantity;
            }


            if (quantityInKG > 0)
            {
                priceByKgPzInStore = requestedQtyCost / quantityInKG;
            }
            else if (quantityInPz > 0)
            {
                priceByKgPzInStore = requestedQtyCost / quantityInPz;
            }

            return priceByKgPzInStore;
        }

        public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            if (TempData["mensaje"] != null)
                ViewBag.mensaje = TempData["mensaje"].ToString();

            var dateDelivery = DateTime.Parse(id);
            var deliveryOrders = _orderService.GetOrders().Where(x => x.SelectedShippingDate == dateDelivery)
                                                                  .Where(x => x.OrderStatus != OrderStatus.Cancelled &&
                                                                            !(x.PaymentMethodSystemName == "Payments.PayPalStandard" &&
                                                                              x.PaymentStatus == Nop.Core.Domain.Payments.PaymentStatus.Pending))
                                                                  .ToList();


            IList<OrderItem> allProductsDelivery = new List<OrderItem>();
            foreach (var item in deliveryOrders)
            {
                foreach (var item2 in item.OrderItems)
                {
                    allProductsDelivery.Add(item2);
                }
            }
            allProductsDelivery = allProductsDelivery.OrderBy(x => x.Product.ProductCategories.Where(y => y.Category.ParentCategoryId == 0).Select(y => y.Category.DisplayOrder).FirstOrDefault())
                                                     .ThenBy(x => x.Product.ProductCategories.Where(y => y.Category.ParentCategoryId == 0).Select(y => y.Category.Name).FirstOrDefault())
                                                     .ThenBy(x => x.Product.ProductCategories.Where(y => y.Category.ParentCategoryId == 0).Select(y => y.DisplayOrder).FirstOrDefault()).ToList();

            var model = new EditOrderReportsViewModel
            {
                DeliveryDate = dateDelivery,

                GroupsDistinc = allProductsDelivery.GroupBy(o => o.ProductId).Select(g => new EditGroups
                {
                    GroupProductId = g.Key,
                    GroupProductName = allProductsDelivery.Where(z => z.ProductId == g.Key).Select(y => y.Product.Name).FirstOrDefault(),
                    Products = g.Select(y => new EditProductsInGroup
                    {
                        OrderReportId = _orderReportService.GetAll().Where(p => p.OrderId == y.OrderId && p.OrderItemId == y.Id).Select(f => f.Id).FirstOrDefault(),
                        ShoppingStoreId = _orderReportService.GetAll().Where(p => p.OrderId == y.OrderId && p.OrderItemId == y.Id).Select(f => f.ShoppingStoreId).FirstOrDefault(),
                        //ItemKgPzStoreCost = _orderReportService.GetAll().Where(p => p.OrderId == y.OrderId && p.OrderItemId == y.Id).Select(f => f.UnitCost).FirstOrDefault(),
                        OrderItemComments = _orderReportService.GetAll().Where(p => p.OrderId == y.OrderId && p.OrderItemId == y.Id).Select(f => f.Comments).FirstOrDefault(),
                        RequestedQtyCost = _orderReportService.GetAll().Where(p => p.OrderId == y.OrderId && p.OrderItemId == y.Id).Select(f => f.RequestedQtyCost).FirstOrDefault(),

                        OrderId = y.OrderId,
                        OrderItemId = y.Id,
                        OrderItemName = y.Product.Name,
                        ItemCostKgPz = Math.Round(y.Product.Price, 2),
                        OrderItemCost = Math.Round(y.PriceInclTax, 2),
                        OrderItemSpecifications = y.SelectedPropertyOption,
                        OrderItemQuantity = y.Quantity,
                        OrderItemQuantitySpecific = OrderItemQuantitySpecific(y.Id),
                    }).ToList()
                }).ToList(),

                AditionalCost = _aditionalCostService.GetAll().Where(date => date.Date == dateDelivery && date.Deleted == false).Select(cost => new AditionalCostData
                {
                    Id = cost.Id,
                    CostAmount = cost.Cost,
                    CostDescription = cost.Description
                }).ToList()
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/OrderDeliveryReports/EditOrderReports.cshtml", model);
        }

        [HttpPost]
        public IActionResult Edit(EditOrderReportsViewModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var log = string.Empty;
            List<EditProductsInGroup> jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<EditProductsInGroup>>(model.JsonData);
            foreach (var item in jsonData)
            {
                if (item.OrderReportId == 0)
                {
                    var existOrderItemId = _orderReportService.GetAll().Where(x => x.OrderItemId == item.OrderItemId).Any();
                    if (!existOrderItemId)
                    {
                        OrderReport orderReport = new OrderReport()
                        {
                            OrderId = item.OrderId,
                            OrderItemId = item.OrderItemId,
                            RequestedQtyCost = item.RequestedQtyCost, /// costo del pedido
                            UnitCost = CalculateUnitCostToReportOrder(item.OrderItemId, item.RequestedQtyCost),///precio kg/pz en la bodega donde se compro
                            ShoppingStoreId = item.ShoppingStoreId,
                            Comments = item.OrderItemComments,
                        };
                        _orderReportService.Insert(orderReport);
                    }
                    else
                    {
                        TempData["mensaje"] = "Ocurrio un error: No se puede repetir reportes para productos de orden. Regresa a la lista de fechas de entregas y trata de volver editar los reportes de esta fecha.";
                        return RedirectToAction("Edit/" + model.DeliveryDate);
                    }

                }
                else
                {
                    var itemLog = string.Empty;
                    OrderReport orderReport = _orderReportService.GetAll().Where(x => x.Id == item.OrderReportId).FirstOrDefault();
                    if (orderReport == null)
                    {
                        var existOrderItemId = _orderReportService.GetAll().Where(x => x.OrderItemId == item.OrderItemId).Any();
                        if (!existOrderItemId)
                        {
                            OrderReport newOrderReport = new OrderReport()
                            {
                                OrderId = item.OrderId,
                                OrderItemId = item.OrderItemId,
                                RequestedQtyCost = item.RequestedQtyCost, /// costo del pedido
                                UnitCost = CalculateUnitCostToReportOrder(item.OrderItemId, item.RequestedQtyCost),///precio kg/pz en la bodega donde se compro
                                ShoppingStoreId = item.ShoppingStoreId,
                                Comments = item.OrderItemComments,
                            };
                            _orderReportService.Insert(newOrderReport);
                        }
                        else
                        {
                            TempData["mensaje"] = "Ocurrio un error: No se puede repetir reportes para productos de orden. Regresa a la lista de fechas de entregas y trata de volver editar los reportes de esta fecha.";
                            return RedirectToAction("Edit/" + model.DeliveryDate);
                        }
                    }
                    else
                    {
                        var newUnitCost = CalculateUnitCostToReportOrder(orderReport.OrderItemId, item.RequestedQtyCost);
                        var newunitCostRound = Math.Truncate(newUnitCost * 100) / 100;

                        if (orderReport.Comments != item.OrderItemComments)
                        {
                            itemLog += $" Editó el comentario del producto {_orderService.GetOrderItemById(item.OrderItemId).Product.Name} de {orderReport.Comments} a {item.OrderItemComments}.";
                        }

                        if (orderReport.UnitCost != newunitCostRound)
                        {
                            itemLog += $" Editó el costo por kg/pza del producto {_orderService.GetOrderItemById(item.OrderItemId).Product.Name} de {orderReport.UnitCost} a {newUnitCost}.";
                        }

                        if (orderReport.RequestedQtyCost != item.RequestedQtyCost)
                        {
                            itemLog += $" Editó el costo del pedido del producto {_orderService.GetOrderItemById(item.OrderItemId).Product.Name} de {orderReport.RequestedQtyCost} a {item.RequestedQtyCost}.";
                        }

                        if (orderReport.ShoppingStoreId != item.ShoppingStoreId)
                        {
                            itemLog += $" Editó la bodega del producto {_orderService.GetOrderItemById(item.OrderItemId).Product.Name} de {orderReport.ShoppingStoreId} a {item.ShoppingStoreId}.";
                        }

                        if (!string.IsNullOrWhiteSpace(itemLog))
                        {
                            log += itemLog;

                            orderReport.Comments = item.OrderItemComments;
                            orderReport.RequestedQtyCost = item.RequestedQtyCost;
                            orderReport.UnitCost = newUnitCost;
                            orderReport.ShoppingStoreId = item.ShoppingStoreId;

                            _orderReportService.Update(orderReport);
                        }
                    }
                }

            }


            if (model.ExpenseJsonData != null)
            {
                List<EditOrderReportsViewModel> jsonExpenseData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<EditOrderReportsViewModel>>(model.ExpenseJsonData);
                foreach (var item in jsonExpenseData)
                {
                    if (item.ExpenseId == 0)
                    {
                        AditionalCost aditionalCost = new AditionalCost()
                        {
                            Date = model.DeliveryDate,
                            Cost = item.ExpenseAmount,
                            Description = item.ExpenseDescription
                        };
                        _aditionalCostService.Insert(aditionalCost);
                    }
                    else
                    {
                        AditionalCost aditionalCost = _aditionalCostService.GetAll().Where(x => x.Id == item.ExpenseId).FirstOrDefault();
                        aditionalCost.Cost = item.ExpenseAmount;
                        aditionalCost.Description = item.ExpenseDescription;

                        _aditionalCostService.Update(aditionalCost);
                    }

                }
            }

            return RedirectToAction("DeliveryDateList");
        }



        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderDeliveryReports))
                return AccessDeniedView();

            DateTime today = DateTime.Now.Date;
            var query = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate.Value <= today);
            var queryList = query.GroupBy(x => x.SelectedShippingDate).OrderByDescending(x => x.Key);
            var pagedList = GroupedPageList(queryList, command.Page - 1, command.PageSize);

            var shippingDates = pagedList.SelectMany(x => x.Select(y => y)).Select(x => x.SelectedShippingDate.Value).ToList();
            var orderReportStatuses = _orderReportStatusService.GetAll().Where(x => shippingDates.Contains(x.ShippingDate)).ToList();

            var orderIds = pagedList.SelectMany(x => x.Select(y => y.Id)).ToList();
            var notDeliveredItems = _notDeliveredOrderItemService.GetAll().Where(x => orderIds.Contains(x.OrderId)).ToList();

            var allOrderItemIds = notDeliveredItems.Select(x => x.OrderItemId).Union(pagedList.SelectMany(x => x.SelectMany(y => y.OrderItems.Select(z => z.Id))));
            var orderItemBuyerData = _orderItemBuyerService.GetAll().Where(x => allOrderItemIds.Contains(x.OrderItemId)).ToList();

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    Id = x.Key.Value.ToString("dd-MM-yyyy"),
                    Date = x.Key.Value.ToString("D", new CultureInfo("es-MX")),
                    IsPending = IsPendingFunction(x.Select(c => c.Id).ToList()),
                    CountConfirm = CountBuyersConfirmReport(x.Key.Value, x.Select(y => y).ToList(), orderReportStatuses, orderItemBuyerData, notDeliveredItems, true),
                    CountNotConfirm = CountBuyersConfirmReport(x.Key.Value, x.Select(y => y).ToList(), orderReportStatuses, orderItemBuyerData, notDeliveredItems, false)
                }).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        public int CountBuyersConfirmReport(DateTime date,
            List<Order> orders,
            List<OrderReportStatus> orderReportStatuses,
            List<OrderItemBuyer> orderItemBuyerData,
            List<NotDeliveredOrderItem> notDeliveredOrderItems,
            bool hasConfirmReport)
        {
            int result = 0;
            var dateSelected = date;

            var buyersData = orderReportStatuses.Where(x => x.ShippingDate == dateSelected);
            if (hasConfirmReport)
            {
                result = buyersData.Where(x => x.StatusTypeId == 2).Count();
            }
            else
            {
                var orderItemsIds = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService, notDeliveredOrderItems).Select(y => y.Id).ToList();
                var allBuyersIdsOfOrderItems = orderItemBuyerData.Where(x => orderItemsIds.Contains(x.OrderItemId)).GroupBy(x => x.CustomerId).Select(x => x.Key).ToList();
                int buyersNotConfirmReport = buyersData.Where(x => x.StatusTypeId != 2).Count();
                var buyersWhithoutReport = allBuyersIdsOfOrderItems.Where(x => !buyersData.Select(y => y.BuyerId).Contains(x)).Count();
                result = buyersNotConfirmReport + buyersWhithoutReport;
            }

            return result;
        }

        private List<IGrouping<DateTime?, Order>> GroupedPageList(IQueryable<IGrouping<DateTime?, Order>> source, int pageIndex, int pageSize)
        {
            List<IGrouping<DateTime?, Order>> filteredList = new List<IGrouping<DateTime?, Order>>();
            filteredList.AddRange(source.Skip(pageIndex * pageSize).Take(pageSize).ToList());
            return filteredList;
        }

        public bool IsPendingFunction(List<int> orderId)
        {
            var exist = _orderReportService.GetAll().Where(y => orderId.Contains(y.OrderId)).Any();
            return exist;
        }



        [HttpPost]
        public IActionResult UserListData()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
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
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var routes = _shippingRouteService.GetAll().ToList();
            var elements = routes.Select(x => new
            {
                Id = x.Id,
                Route = x.RouteName
            }).ToList();

            return Json(elements);
        }

        [HttpDelete]
        public IActionResult DeleteAditionalCost(int id)
        {
            AditionalCost aditionalCost = _aditionalCostService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            aditionalCost.Deleted = true;

            _aditionalCostService.Update(aditionalCost);
            return Ok();
        }


        public FileData ImageToByteArray(IFormFile imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.CopyToAsync(ms);

                FileData fileData = new FileData();
                fileData.BytesFile = ms.ToArray();
                fileData.Extension = imageIn.ContentType;
                return fileData;
            }
        }

        [HttpGet]
        public virtual IActionResult ShowFileTransfer(int id)
        {
            var test = _orderReportTransferService.GetAll().Where(x => x.Id == id).FirstOrDefault();

            return File(test.File, test.Extension);

        }

        public IActionResult ManualNotDeliveredProduct(int orderItemId)
        {
            if (orderItemId == 0) return BadRequest("Ocurrió un problema guardando la información en el servidor.");
            OrderItem orderItem = _orderService.GetOrderItemById(orderItemId);
            if (orderItem == null) return NotFound();
            Order order = orderItem.Order;
            decimal currentOrderTotal = order.OrderTotal;

            NotDeliveredOrderItem notDeliveredOrderItem = new NotDeliveredOrderItem()
            {
                AttributeDescription = orderItem.AttributeDescription,
                DiscountAmountExclTax = orderItem.DiscountAmountExclTax,
                AttributesXml = orderItem.AttributesXml,
                DiscountAmountInclTax = orderItem.DiscountAmountInclTax,
                BuyingBySecondary = orderItem.BuyingBySecondary,
                NotDeliveredReason = "Producto marcado como no entregado por el sistema. El repartidor olvidó marcarlo como no entregado.",
                EquivalenceCoefficient = orderItem.EquivalenceCoefficient,
                ItemWeight = orderItem.ItemWeight,
                NotDeliveredReasonId = 0,
                OrderId = orderItem.OrderId,
                OriginalProductCost = orderItem.OriginalProductCost,
                PriceExclTax = orderItem.PriceExclTax,
                PriceInclTax = orderItem.PriceInclTax,
                ProductId = orderItem.ProductId,
                Quantity = orderItem.Quantity,
                ReportedByUserId = 1,
                SelectedPropertyOption = orderItem.SelectedPropertyOption,
                UnitPriceExclTax = orderItem.UnitPriceExclTax,
                UnitPriceInclTax = orderItem.UnitPriceInclTax,
                WeightInterval = orderItem.WeightInterval,
                OrderItemId = orderItem.Id
            };
            _notDeliveredOrderItemService.Insert(notDeliveredOrderItem);

            order.OrderSubtotalInclTax -= orderItem.PriceInclTax;
            order.OrderSubtotalExclTax -= orderItem.PriceExclTax;
            order.OrderTotal -= orderItem.PriceInclTax;

            order.OrderNotes.Add(new OrderNote()
            {
                CreatedOnUtc = DateTime.UtcNow,
                DisplayToCustomer = false,
                Note = $"Se reportó manualmente el producto '{orderItem.Product.Name}' como no entregado. El total de la orden pasó de {currentOrderTotal} a {order.OrderTotal}",
                CustomerId = _workContext.CurrentCustomer.Id
            });

            _orderService.DeleteOrderItem(orderItem);
            _orderService.UpdateOrder(order);

            return Ok();
        }

        public class OrderReportCosts
        {
            public int ProductId { get; set; }
            public decimal UnitCost { get; set; }
        }

        public class FileData
        {
            public byte[] BytesFile { get; set; }

            public string Extension { get; set; }
        }

        public class PendingReportModel
        {
            public DateTime ShippingDate { get; set; }
            public int BuyerId { get; set; }
            public string BuyerName { get; set; }
        }

        public class SimpleReportModel
        {
            public DateTime OrderShippingDate { get; set; }
            public int OriginalBuyerId { get; set; }
            public int ProductId { get; set; }
            public string Invoice { get; set; }
            public bool? SentToSupermarket { get; set; }
            public decimal UpdatedUnitCost { get; set; }
        }

        public class OrderItemBoughtType
        {
            public int ProductId { get; set; }
            public BoughtType BoughtType { get; set; }
        }
    }
}
