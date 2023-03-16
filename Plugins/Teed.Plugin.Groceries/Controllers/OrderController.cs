
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Data;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Controllers.IText.EventHandler;
using Teed.Plugin.Groceries.Domain.OrderItemBuyers;
using Teed.Plugin.Groceries.Domain.OrderReports;
using Teed.Plugin.Groceries.Domain.Product;
using Teed.Plugin.Groceries.Domain.ShippingRegions;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;
using Teed.Plugin.Groceries.Dtos;
using Teed.Plugin.Groceries.Extensions;
using Teed.Plugin.Groceries.Models.Order;
using Teed.Plugin.Groceries.Models.Payment;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Settings;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class OrderController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly IPaymentService _paymentService;
        private readonly Services.OrderReportService _orderReportService;
        private readonly IDbContext _dbContext;
        private readonly AditionalCostService _aditionalCostService;
        private readonly IWorkContext _workContext;
        private readonly IDiscountService _discountService;
        private readonly ShippingRouteService _shippingRouteService;
        private readonly OrderItemBuyerService _orderItemBuyerService;
        private readonly BuyerListDownloadService _buyerListDownloadService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICategoryService _categoryService;
        private readonly ShippingZoneService _shippingZoneService;
        private readonly OrderTypeService _orderTypeService;
        private readonly NotDeliveredOrderItemService _notDeliveredOrderItemService;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ITaxService _taxService;
        private readonly ShippingRegionZoneService _shippingRegionZoneService;
        private readonly ShippingRegionService _shippingRegionService;
        private readonly ShippingAreaService _shippingAreaService;
        private readonly ProductMainManufacturerService _productMainManufacturerService;
        private readonly OrderItemLogService _orderItemLogService;
        private readonly ManufacturerListDownloadService _manufacturerListDownloadService;
        private readonly CorcelCustomerService _corcelCustomerService;

        Table shoppingListHeaderTable;
        Table shoppingListDetailsTable;
        Table shoppingListNotesTable;
        Table shoppingListExtraTable;
        Table secondaryShoppingListDetailsTable;
        Table shoppingListProductTable;
        Table orderDetailsHeaderTable;
        Table orderDeliveredOrNot;
        Table orderDetailsProductTable;
        Table orderDetailsFooterTable;
        Table budgetBuyerTable;

        Table totalShippingCostTable;
        Table totalOrdenDiscountTable;
        Table ordenDiscountByCouponsTable;
        Table totalReplacementCostTable;


        byte[] shippingDataStream;
        Table shippingDataTable;
        Paragraph shoppingListDetailsTitle;
        Paragraph shoppingListTitle;
        Paragraph orderDetailsTitle;

        Paragraph discountByCouponsTitle;

        Document doc;
        PageSize ps;

        DateTime parsedDate;
        int orderCount = 0;
        int lastRouteId = 0;
        List<UserInfoForClient> usersInfo = new List<UserInfoForClient>();
        string letter = "";
        bool isSecondShippoingList = false;
        List<BuyersTotals> buyersTotals = new List<BuyersTotals>();
        int currentBuyerId = 0;

        List<AllOrderItemsModel> allOrderItems = new List<AllOrderItemsModel>();
        List<AllReportsModel> allReports = new List<AllReportsModel>();

        public OrderController(IPermissionService permissionService, IOrderService orderService, IAddressAttributeFormatter addressAttributeFormatter, ShippingRouteService shippingRouteService,
            IStoreContext storeContext, IPaymentService paymentService, Services.OrderReportService orderReportService, AditionalCostService aditionalCostService,
            IDbContext dbContext, IWorkContext workContext, OrderItemBuyerService orderItemBuyerService, ICustomerService customerService,
            IDiscountService discountService, IManufacturerService manufacturerService, ICategoryService categoryService, ShippingZoneService shippingZoneService,
            OrderTypeService orderTypeService, NotDeliveredOrderItemService notDeliveredOrderItemService, IProductService productService, BuyerListDownloadService buyerListDownloadService,
            ShippingRegionZoneService shippingRegionZoneService, ISettingService settingService, ShippingRegionService shippingRegionService,
            IOrderProcessingService orderProcessingService, ShippingAreaService shippingAreaService, ProductMainManufacturerService productMainManufacturerService,
            IShoppingCartService shoppingCartService, ITaxService taxService, OrderItemLogService orderItemLogService,
            ManufacturerListDownloadService manufacturerListDownloadService, CorcelCustomerService corcelCustomerService)
        {
            _permissionService = permissionService;
            _orderService = orderService;
            _addressAttributeFormatter = addressAttributeFormatter;
            _storeContext = storeContext;
            _paymentService = paymentService;
            _orderReportService = orderReportService;
            _dbContext = dbContext;
            _aditionalCostService = aditionalCostService;
            _workContext = workContext;
            _orderItemBuyerService = orderItemBuyerService;
            _customerService = customerService;
            _discountService = discountService;
            _shippingRouteService = shippingRouteService;
            _manufacturerService = manufacturerService;
            _categoryService = categoryService;
            _shippingZoneService = shippingZoneService;
            _orderTypeService = orderTypeService;
            _notDeliveredOrderItemService = notDeliveredOrderItemService;
            _productService = productService;
            _buyerListDownloadService = buyerListDownloadService;
            _shippingRegionZoneService = shippingRegionZoneService;
            _settingService = settingService;
            _shippingRegionService = shippingRegionService;
            _orderProcessingService = orderProcessingService;
            _shippingAreaService = shippingAreaService;
            _productMainManufacturerService = productMainManufacturerService;
            _shoppingCartService = shoppingCartService;
            _taxService = taxService;
            _orderItemLogService = orderItemLogService;
            _manufacturerListDownloadService = manufacturerListDownloadService;
            _corcelCustomerService = corcelCustomerService;
        }

        public class UserInfoForClient
        {
            public int CustomerId { get; set; }
            public int? ShippingAddressId { get; set; }
            public string Address1 { get; set; }
            public int OrderNumber { get; set; }
            public int ComplementaryCount { get; set; }
        }

        [HttpGet]
        public IActionResult SendFriendCodeReward(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null) return NotFound();
            var settings = _settingService.LoadSetting<GrowthHackingSettings>();
            _orderProcessingService.GenerateRewardCouponCode(order, settings);
            return Ok();
        }

        [HttpGet]
        public virtual IActionResult GetManufacturersSelectList()
        {
            var manufacturers = _manufacturerService.GetAllManufacturers().Where(x => x.IsIncludeInReportByManufacturer);
            return Ok(manufacturers.Select(x => new { x.Name, x.Id }).OrderBy(x => x.Name).ToList());
        }

        [HttpGet]
        public virtual IActionResult CreateRewardCode(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null) return NotFound();
            var settings = _settingService.LoadSetting<GrowthHackingSettings>();
            if (settings == null || !settings.IsActive) return NotFound();

            _orderProcessingService.GenerateRewardCouponCode(order, settings);
            return Ok();
        }

        [HttpGet]
        public virtual IActionResult MarkOrderAsNotPaid(int id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null) return NotFound();
            order.PaymentStatusId = (int)PaymentStatus.Pending;
            order.PaidDateUtc = null;
            order.OrderStatusId = (int)OrderStatus.Pending;
            order.OrderNotes.Add(new OrderNote()
            {
                CreatedOnUtc = DateTime.UtcNow,
                DisplayToCustomer = false,
                Note = $"La orden fue marcada como no pagada por {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id})",
                CustomerId = _workContext.CurrentCustomer.Id
            });
            _orderService.UpdateOrder(order);
            return RedirectToAction("Edit", "Order", new { id = id });
        }

        public virtual IActionResult MarkOrderAsNotDelivered(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List", "Order");

            if (order.OrderStatusId == 50)
                return RedirectToAction("Edit", "Order", new { id });

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                order.OrderStatusId = 50; //Not delivered
                order.OrderNotes.Add(new OrderNote()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    DisplayToCustomer = false,
                    Note = $"La orden fue marcada como no entregada por {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id})",
                    CustomerId = _workContext.CurrentCustomer.Id
                });
                _orderService.UpdateOrder(order);

                return RedirectToAction("Edit", "Order", new { id });
            }
            catch (Exception exc)
            {
                //error
                return RedirectToAction("Edit", "Order", new { id });
            }
        }

        [HttpGet]
        public virtual IActionResult SalesByRouteReportPdf(string selectedDate, bool isForDelivery = false)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var parsedDate = DateTime.Now.Date;
            if (!string.IsNullOrEmpty(selectedDate))
                parsedDate = DateTime.ParseExact(selectedDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            else
                return BadRequest("Fecha vacia");

            NewPageHandler.ParsedDate = parsedDate;
            NewPageHandler.CurrentUserEmail = _workContext.CurrentCustomer.Email;

            var orders = _orderService.GetAllOrdersQuery()
                                      .Where(x => x.SelectedShippingDate == parsedDate)
                                      .ToList();


            MemoryStream stream = new MemoryStream();
            PdfWriter pdfWriter = new PdfWriter(stream);
            PdfDocument pdfDoc = new PdfDocument(pdfWriter);
            ps = pdfDoc.GetDefaultPageSize();
            doc = new Document(pdfDoc, PageSize.LETTER, false);
            doc.SetMargins(20, 20, 50, 20);

            NewPageHandler newPageHandler = new NewPageHandler();
            pdfDoc.AddEventHandler(PdfDocumentEvent.START_PAGE, newPageHandler);

            doc.Add(new Table(1).AddHeaderCell(new Cell()
                    .Add(new Paragraph("Desglose de ventas por ruta - " + parsedDate.ToString("dd/MM/yyyy")))
                    .SetBorder(Border.NO_BORDER))
                    .SetBold().SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER)
                    .SetFontSize(20)
                    .SetMarginBottom(5));



            var routeIds = orders.Where(x => x.RouteId != 0).Select(x => x.RouteId).Distinct().ToList();

            for (int i = 0; i < routeIds.Count(); i++)
            {
                var ordersByRoute = orders.Where(x => x.RouteId == routeIds[i])
                               .Where(x => x.OrderStatus != OrderStatus.Cancelled && x.PaymentStatus != Nop.Core.Domain.Payments.PaymentStatus.Pending)
                               .ToList();

                PrepareShoppingListByRouteHeader(routeIds[i], ordersByRoute);
                doc.Add(shoppingListHeaderTable);
                PrepareTotalSaleByRouteAndPaymentMethod(ordersByRoute);
                doc.Add(shoppingListDetailsTitle);
                doc.Add(shoppingListDetailsTable);

                PrepareShippingCostByRoute(ordersByRoute);
                doc.Add(totalShippingCostTable);

                PrepareTotalDiscountsByRoute(ordersByRoute);
                doc.Add(totalOrdenDiscountTable);
                doc.Add(discountByCouponsTitle);
                doc.Add(ordenDiscountByCouponsTable);

                PrepareReplacementCostByRoute(ordersByRoute);
                doc.Add(totalReplacementCostTable);

                if (i < routeIds.Count() - 1) doc.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));
            }

            newPageHandler.WriteTotal(pdfDoc);
            doc.Flush();
            doc.Close();

            return File(stream.ToArray(), MimeTypes.ApplicationPdf);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("[controller]/[action]")]
        public virtual IActionResult DeleteShopingCart()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            //updated cart
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
               .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
               .LimitPerStore(_storeContext.CurrentStore.Id)
               .ToList();
            foreach (var item in cart)
            {
                _shoppingCartService.DeleteShoppingCartItem(item, ensureOnlyActiveCheckoutAttributes: true);
            }
            cart.Clear();


            return RedirectToAction("Cart", "ShoppingCart", new { area = "" });
        }

        private void PrepareTotalSaleByRouteAndPaymentMethod(IList<Order> orders)
        {
            shoppingListDetailsTitle = new Paragraph("Total venta efectiva");
            shoppingListDetailsTitle.SetFontSize(10);
            shoppingListDetailsTitle.SetBold();
            shoppingListDetailsTitle.SetTextAlignment(TextAlignment.CENTER);

            shoppingListDetailsTable = new Table(2);
            shoppingListDetailsTable.SetWidth(PageSize.A4.GetWidth() - 30);

            Paragraph shoppingDetailsHeader1 = new Paragraph("Método de pago")
                    .SetBold()
                    .SetFontSize(10)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            Paragraph shoppingDetailsHeader2 = new Paragraph("Total")
                    .SetBold()
                    .SetFontSize(10)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            shoppingListDetailsTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader1));

            shoppingListDetailsTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader2));

            var ordersFilteredPaymentMethod = orders.Where(x => !(x.PaymentMethodSystemName == "Payments.Replacement" || x.PaymentMethodSystemName == "Payments.Benefits")).ToList();
            var groupedByPaymentMethod = ordersFilteredPaymentMethod.GroupBy(x => x.PaymentMethodSystemName).OrderBy(x => x.Key).ToList();

            foreach (var paymentMethod in groupedByPaymentMethod)
            {

                string paymentMethodString = string.IsNullOrEmpty(paymentMethod.Key) ? "Sin especificar" : paymentMethod.Key.Split('.')[1];
                var total = paymentMethod.Sum(x => x.OrderTotal);

                Paragraph p1 = new Paragraph(paymentMethodString)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetMarginLeft(5);

                Paragraph p2 = new Paragraph(total.ToString("C"))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetMarginLeft(5);

                shoppingListDetailsTable.AddCell(new Cell()
                            .SetFontSize(8)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(p1));

                shoppingListDetailsTable.AddCell(new Cell()
                            .SetFontSize(8)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(p2));
            }

            Paragraph totalText = new Paragraph("TOTAL")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetMarginLeft(5);

            var sumTotal = ordersFilteredPaymentMethod.Sum(x => x.OrderTotal);

            Paragraph totalProducts = new Paragraph(sumTotal.ToString("C"))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetMarginLeft(5);


            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(totalText));

            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(totalProducts));

            shoppingListDetailsTable.AddCell(new Cell(1, 2)
                    .SetPadding(3)
                    .SetBorder(Border.NO_BORDER)
                    .Add(new Paragraph()));
        }

        private void PrepareShippingCostByRoute(IList<Order> orders)
        {

            var shippingCost = orders.Sum(x => x.OrderShippingInclTax);

            Paragraph shoppingDetailsHeader1 = new Paragraph("Total costo de envío")
                    .SetBold()
                    .SetFontSize(10)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            Paragraph shoppingDetailsHeader2 = new Paragraph(shippingCost.ToString("C"))
                    .SetBold()
                    .SetFontSize(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

            totalShippingCostTable = new Table(2);
            totalShippingCostTable.SetWidth(PageSize.A4.GetWidth() - 30);

            totalShippingCostTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader1));

            totalShippingCostTable.AddHeaderCell(new Cell()
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader2));

            totalShippingCostTable.AddCell(new Cell(1, 2)
                    .SetPadding(3)
                    .SetBorder(Border.NO_BORDER)
                    .Add(new Paragraph()));
        }
        private void PrepareTotalDiscountsByRoute(IList<Order> orders)
        {

            var totalDiscount = orders.Sum(x => x.OrderDiscount);

            Paragraph shoppingDetailsHeader1 = new Paragraph("Total descontado en las órdenes")
                    .SetBold()
                    .SetFontSize(10)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            Paragraph shoppingDetailsHeader2 = new Paragraph(totalDiscount.ToString("C"))
                    .SetBold()
                    .SetFontSize(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

            totalOrdenDiscountTable = new Table(2);
            totalOrdenDiscountTable.SetWidth(PageSize.A4.GetWidth() - 30);

            totalOrdenDiscountTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader1));

            totalOrdenDiscountTable.AddHeaderCell(new Cell()
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader2));

            totalOrdenDiscountTable.AddCell(new Cell(1, 2)
                    .SetPadding(3)
                    .SetBorder(Border.NO_BORDER)
                    .Add(new Paragraph()));

            discountByCouponsTitle = new Paragraph("Total descontado por cupones");
            discountByCouponsTitle.SetFontSize(10);
            discountByCouponsTitle.SetBold();
            discountByCouponsTitle.SetTextAlignment(TextAlignment.CENTER);

            shoppingListDetailsTable = new Table(2);
            shoppingListDetailsTable.SetWidth(PageSize.A4.GetWidth() - 30);

            Paragraph discountByCouponsHeader = new Paragraph("Tipo de cupón")
                    .SetBold()
                    .SetFontSize(10)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            Paragraph discountByCouponsHeader2 = new Paragraph("Total")
                    .SetBold()
                    .SetFontSize(10)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);


            ordenDiscountByCouponsTable = new Table(2);
            ordenDiscountByCouponsTable.SetWidth(PageSize.A4.GetWidth() - 30);

            ordenDiscountByCouponsTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(discountByCouponsHeader));

            ordenDiscountByCouponsTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(discountByCouponsHeader2));

            var ordersFilteredPaymentMethod = orders.Where(x => !(x.PaymentMethodSystemName == "Payments.Replacement" || x.PaymentMethodSystemName == "Payments.Benefits")).ToList();
            var ordersIds = ordersFilteredPaymentMethod.Select(x => x.Id).ToList();

            var couponsAppliedByType = _discountService.GetAllDiscountUsageHistory().Where(x => ordersIds.Contains(x.OrderId)).GroupBy(x => x.Discount.DiscountTypeId).ToList();
            decimal sumDiscountByCoupon = 0;

            if (couponsAppliedByType.Count() > 0)
            {
                var availableDiscountTypes = DiscountType.AssignedToOrderTotal.ToSelectList(false).ToList();
                foreach (var couponType in couponsAppliedByType)
                {
                    var discountType = availableDiscountTypes.Where(x => int.Parse(x.Value) == couponType.Key).Select(x => x.Text).FirstOrDefault();
                    var totalDiscountByCoupon = couponType.Sum(x => x.Discount.DiscountAmount);
                    sumDiscountByCoupon += totalDiscountByCoupon;

                    Paragraph p1 = new Paragraph(discountType)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetMarginLeft(5);

                    Paragraph p2 = new Paragraph(totalDiscountByCoupon.ToString("C"))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                        .SetMarginLeft(5);

                    ordenDiscountByCouponsTable.AddCell(new Cell()
                                .SetFontSize(8)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(p1));

                    ordenDiscountByCouponsTable.AddCell(new Cell()
                                .SetFontSize(8)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(p2));
                }
            }

            Paragraph totalText = new Paragraph("TOTAL")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetMarginLeft(5);

            Paragraph totalProducts = new Paragraph(sumDiscountByCoupon.ToString("C"))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetMarginLeft(5);


            ordenDiscountByCouponsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(totalText));

            ordenDiscountByCouponsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(totalProducts));

            ordenDiscountByCouponsTable.AddCell(new Cell(1, 2)
                    .SetPadding(3)
                    .SetBorder(Border.NO_BORDER)
                    .Add(new Paragraph()));


        }
        private void PrepareReplacementCostByRoute(IList<Order> orders)
        {
            var ordersFilteredPaymentMethod = orders.Where(x => x.PaymentMethodSystemName == "Payments.Replacement").ToList();
            var replacementCost = ordersFilteredPaymentMethod.Sum(x => x.OrderTotal);

            Paragraph replacementHeader1 = new Paragraph("Total reposiciones")
                    .SetBold()
                    .SetFontSize(10)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            Paragraph replacementHeader2 = new Paragraph(replacementCost.ToString("C"))
                    .SetBold()
                    .SetFontSize(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

            totalReplacementCostTable = new Table(2);
            totalReplacementCostTable.SetWidth(PageSize.A4.GetWidth() - 30);

            totalReplacementCostTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(replacementHeader1));

            totalReplacementCostTable.AddHeaderCell(new Cell()
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(replacementHeader2));

            totalReplacementCostTable.AddCell(new Cell(1, 2)
                    .SetPadding(3)
                    .SetBorder(Border.NO_BORDER)
                    .Add(new Paragraph()));
        }

        [HttpGet]
        [Route("[controller]/[action]/{id}")]
        public virtual IActionResult CancelOrder(int id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null || order.Deleted) return NotFound();

            if (_workContext.CurrentCustomer.Id != order.CustomerId) return Unauthorized();

            var dayDate = DateTime.Now.Date;

            if (order.SelectedShippingDate <= dayDate) return NotFound();

            var pedido = OrderUtils.GetOrdersInPedidoByOrder(order, _orderService);
            var orderSettings = _settingService.LoadSetting<OrderSettings>();
            var allOrdersSubTotal = (pedido.Select(x => x.OrderSubtotalInclTax).DefaultIfEmpty().Sum()) - order.OrderSubtotalInclTax;
            var allOrdersTotal = (pedido.Select(x => x.OrderTotal).DefaultIfEmpty().Sum()) - order.OrderTotal;
            if (allOrdersSubTotal >= orderSettings.MinOrderSubtotalAmount || allOrdersTotal >= orderSettings.MinOrderTotalAmount)
            {
                CancelOrderbyOrder(order);
            }
            else
            {
                foreach (var orderByPedido in pedido)
                {
                    CancelOrderbyOrder(orderByPedido);
                }
            }
            return RedirectToAction("Details", "Order", new { Area = "", orderId = id });
        }

        private void CancelOrderbyOrder(Order order)
        {
            _orderProcessingService.CancelOrder(order, true);
            foreach (var item in order.OrderItems)
            {
                if (item.Product.StockQuantity > 0)
                {
                    item.Product.Published = true;
                }
            }
            order.OrderNotes.Add(new OrderNote
            {
                Note = "Orden cancelada por el cliente.",
                DisplayToCustomer = true,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);
        }

        [HttpGet]
        public virtual IActionResult ExportOrdersData(string selectedDate,
        bool byBuyer = false,
        bool provider = false,
        bool forClient = false,
        int buyerId = 0,
        bool byRoute = false,
        bool byManufacturer = false,
        int manufacturerId = 0,
        bool labels = false,
        bool buyerLabels = false,
        bool labelsV2 = false,
        bool buyerLabelsV2 = false,
        bool fridge = false,
        bool bunch = false,
        int routeId = 0)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders) &&
                !_permissionService.Authorize(TeedGroceriesPermissionProvider.DeliveryReportPrint) &&
                !_permissionService.Authorize(TeedGroceriesPermissionProvider.BuyerReportPrint))
                return AccessDeniedView();

            parsedDate = DateTime.ParseExact(selectedDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            NewPageHandler.ParsedDate = parsedDate;
            NewPageHandler.CurrentUserEmail = _workContext.CurrentCustomer.Email;

            var orders = _orderService.GetAllOrdersQuery()
                .Where(x => x.SelectedShippingDate == parsedDate)
                .Where(x => x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10))
                .ToList();

            var bytes = ExportOrderDataToPdf(orders,
                parsedDate,
                provider,
                byBuyer,
                forClient,
                buyerId,
                byRoute,
                byManufacturer,
                manufacturerId,
                labels,
                buyerLabels,
                labelsV2,
                buyerLabelsV2,
                fridge,
                bunch,
                routeId);

            return File(bytes, MimeTypes.ApplicationPdf);//, $"lista_compras_{parsedDate.ToString("dd-MM-yyyy")}.pdf");
        }

        [HttpGet]
        public virtual IActionResult ExportShippingData(string selectedDate, bool managerData = false, bool isPdf = true)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            parsedDate = DateTime.ParseExact(selectedDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var orders = _orderService.GetAllOrdersQuery()
                .Where(x => x.SelectedShippingDate == parsedDate)
                .Where(x => x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10))
                .ToList();

            var bytes = new byte[] { };
            if (managerData)
                bytes = ExportOrdersDataToXlsx(orders, parsedDate);
            else
                ExportOrderShippingData(orders, parsedDate, isPdf);

            if (isPdf)
                return File(shippingDataStream, MimeTypes.ApplicationPdf);
            else
                return File(shippingDataStream, MimeTypes.TextXlsx, $"envios_{selectedDate}.xlsx");
        }

        private IActionResult ExportOrderShippingData(List<Order> orders, DateTime shippingDate, bool isPdf)
        {
            var ordersByClient = orders.OrderBy(x => x.RouteId)
                .ThenBy(x => x.RouteDisplayOrder).ThenBy(x => x.SelectedShippingTime)
                .GroupBy(x => x.ShippingAddress.Address1 + x.ShippingAddress.FirstName + x.ShippingAddress.LastName);

            var orderIds = orders.Select(x => x.Id).ToList();
            var notDeliveredItems = _notDeliveredOrderItemService.GetAll().Where(x => orderIds.Contains(x.OrderId)).ToList();
            var zones = _shippingZoneService.GetAll().Select(x => new { x.Id, x.ZoneName }).ToList();

            Color bgColour = new DeviceRgb(221, 235, 247);
            if (!isPdf)
            {
                using (var stream = new MemoryStream())
                {
                    using (var xlPackage = new ExcelPackage(stream))
                    {
                        var worksheet = xlPackage.Workbook.Worksheets.Add(shippingDate.ToString("dd-MM-yyyy"));
                        var lastOrder = ordersByClient.LastOrDefault();
                        int row = 1;
                        var lastRoute = 0;
                        var ordersTotal = 0;
                        var productsTotal = 0;
                        //var volumesTotal = (decimal)0;
                        var soldTotal = (decimal)0;

                        worksheet.Cells[row, 1].Value = "R";
                        worksheet.Cells[row, 2].Value = "#";
                        worksheet.Cells[row, 3].Value = "Horario";
                        worksheet.Cells[row, 4].Value = "Orden";
                        worksheet.Cells[row, 5].Value = "Cliente";
                        worksheet.Cells[row, 6].Value = "Direccíon";
                        worksheet.Cells[row, 7].Value = "Colonia";
                        worksheet.Cells[row, 8].Value = "CP";
                        worksheet.Cells[row, 9].Value = "Zona";
                        worksheet.Cells[row, 10].Value = "Método de pago";
                        worksheet.Cells[row, 11].Value = "Total de la orden";
                        worksheet.Cells[row, 12].Value = "Total";
                        worksheet.Cells[row, 13].Value = "Prod";

                        foreach (var client in ordersByClient)
                        {
                            var clientOrders = client.Select(x => x);
                            var routeOrders = orders.Where(x => x.RouteId == client.Select(y => y.RouteId).FirstOrDefault()).ToList();
                            var groceryOrderItems = client.SelectMany(x => x.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService, notDeliveredItems, false, false));
                            if (row == 1)
                            {
                                var optimized = GetStringIfOptimized(routeOrders);
                                if (!string.IsNullOrEmpty(optimized))
                                {
                                    row++;
                                    worksheet.Cells[row, 1].Value = optimized + " - " + shippingDate.ToString("dd-MM-yyyy");
                                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                                }
                                else
                                {
                                    row++;
                                    worksheet.Cells[row, 1].Value = shippingDate.ToString("dd-MM-yyyy");
                                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                                }
                            }
                            row++;
                            if (lastRoute == client.Select(x => x.RouteId).FirstOrDefault() && row > 3)
                            {
                                ordersTotal++;
                                productsTotal += groceryOrderItems.Count();
                            }
                            else
                            {
                                if (row > 3)
                                {
                                    worksheet.Cells[row, 2].Value = ordersTotal;
                                    worksheet.Cells[row, 11].Value = soldTotal.ToString("C");
                                    worksheet.Cells[row, 12].Value = soldTotal;
                                    worksheet.Cells[row, 13].Value = productsTotal;
                                    var optimized = GetStringIfOptimized(routeOrders);
                                    if (!string.IsNullOrEmpty(optimized))
                                    {
                                        row++;
                                        worksheet.Cells[row, 1].Value = optimized + " - " + shippingDate.ToString("dd-MM-yyyy");
                                        worksheet.Cells[row, 1].Style.Font.Bold = true;
                                    }
                                    else
                                    {
                                        row++;
                                        worksheet.Cells[row, 1].Value = shippingDate.ToString("dd-MM-yyyy");
                                        worksheet.Cells[row, 1].Style.Font.Bold = true;
                                    }
                                    row++;
                                }
                                ordersTotal = 1;
                                productsTotal = groceryOrderItems.Count();
                                soldTotal = 0;
                                lastRoute = client.Select(x => x.RouteId).FirstOrDefault();
                            }

                            var routeIds = client.Select(x => x.RouteId).ToList();
                            var routeNames = _shippingRouteService.GetAll().Where(x => routeIds.Contains(x.Id)).Select(x => x.RouteName + " (" + x.VehicleName + ")").ToList();

                            worksheet.Cells[row, 1].Value = string.Join(", ", routeNames.GroupBy(x => x).Select(x => x.First()));
                            worksheet.Cells[row, 2].Value = ordersTotal.ToString();
                            worksheet.Cells[row, 3].Value = string.Join(", ", client.Select(x => x.SelectedShippingTime).GroupBy(x => x).Select(x => x.First()));

                            worksheet.Cells[row, 4].Value = string.Join(", ", client.Select(x => x.CustomOrderNumber));
                            worksheet.Cells[row, 5].Value = client.Select(x => x.ShippingAddress).FirstOrDefault().FirstName + " " + client.Select(x => x.ShippingAddress).FirstOrDefault().LastName;
                            worksheet.Cells[row, 6].Value = client.Select(x => x.ShippingAddress).FirstOrDefault().Address1;
                            //worksheet.Cells[row, 6].Value = _addressAttributeFormatter.FormatAttributes(client.Select(x => x.ShippingAddress).FirstOrDefault().CustomAttributes, htmlEncode: false).Replace("Indicaciones adicionales para el repartidor (opcional): ", "");
                            worksheet.Cells[row, 7].Value = client.Select(x => x.ShippingAddress).FirstOrDefault().Address2;
                            worksheet.Cells[row, 8].Value = client.Select(x => x.ShippingAddress).FirstOrDefault().ZipPostalCode;

                            var orderZoneId = client.Select(x => x.ZoneId).FirstOrDefault();
                            var zone = zones.Where(x => x.Id == orderZoneId).FirstOrDefault();
                            worksheet.Cells[row, 9].Value = orderZoneId == null ? "N/A" : (zone == null ? "N/A" : zone.ZoneName);

                            if (client.Count() > 1)
                            {
                                worksheet.Cells[row, 10].Value = string.Join(", ", client.Select(x => x.PaymentStatus == PaymentStatus.Paid ? $"Pagado (#{x.CustomOrderNumber})" : OrderUtils.GetPaymentOptionName(x.PaymentMethodSystemName) + $" (#{x.CustomOrderNumber})"));
                            }
                            else
                            {
                                worksheet.Cells[row, 10].Value = client.Select(x => x.PaymentStatus).First() == PaymentStatus.Paid ? "Pagado" : OrderUtils.GetPaymentOptionName(client.Select(x => x.PaymentMethodSystemName).First());
                            }

                            if (client.Count() > 1)
                            {
                                worksheet.Cells[row, 11].Value = string.Join(", ", client.Select(x => x.OrderTotal.ToString("C") + $" (#{x.CustomOrderNumber})"));
                                decimal total = client.Select(x => x.OrderTotal).Sum(x => x);
                                worksheet.Cells[row, 12].Value = total;

                            }
                            else
                            {
                                worksheet.Cells[row, 11].Value = client.Select(x => x.OrderTotal.ToString("C")).First();
                                worksheet.Cells[row, 12].Value = client.Select(x => x.OrderTotal).First();
                            }
                            soldTotal += client.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                            //var currentVolume = GetOrderVolumes(clientOrders.ToList());
                            //volumesTotal += currentVolume;
                            worksheet.Cells[row, 13].Value = groceryOrderItems.Count();

                        }

                        row++;
                        worksheet.Cells[row, 2].Value = ordersTotal;
                        worksheet.Cells[row, 11].Value = soldTotal.ToString("C");
                        worksheet.Cells[row, 12].Value = soldTotal;
                        worksheet.Cells[row, 13].Value = productsTotal;

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }

                        xlPackage.Save();
                    }

                    shippingDataStream = stream.ToArray();
                }
            }
            else
            {
                MemoryStream stream = new MemoryStream();
                PdfWriter pdfWriter = new PdfWriter(stream);
                PdfDocument pdfDoc = new PdfDocument(pdfWriter);
                ps = pdfDoc.GetDefaultPageSize();
                doc = new Document(pdfDoc, PageSize.LETTER, false);
                doc.SetMargins(20, 20, 20, 20);

                shippingDataTable = new Table(UnitValue.CreatePercentArray(new float[] { 0.3f, 0.2f, 0.7f, 0.3f, 0.5f, 2, 0.5f, 0.4f, 0.3f, 0.3f, 0.3f, 0.2f })).SetWidth(UnitValue.CreatePercentValue(100));

                var lastRoute = 0;
                var ordersTotal = 0;
                var productsTotal = 0;
                var row = 1;
                var soldTotal = (decimal)0;

                shippingDataTable.AddCell(new Cell()
                    .SetFontSize(8)
                    .SetBold()
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                            .SetBackgroundColor(bgColour)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph("R")));

                shippingDataTable.AddCell(new Cell()
                    .SetFontSize(8)
                    .SetBold()
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                            .SetBackgroundColor(bgColour)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph("#")));

                shippingDataTable.AddCell(new Cell()
                    .SetFontSize(8)
                    .SetBold()
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                            .SetBackgroundColor(bgColour)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph("Horario")));

                shippingDataTable.AddCell(new Cell()
                    .SetFontSize(8)
                    .SetBold()
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                            .SetBackgroundColor(bgColour)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph("Orden")));

                shippingDataTable.AddCell(new Cell()
                    .SetFontSize(8)
                    .SetBold()
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                            .SetBackgroundColor(bgColour)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph("Cliente")));

                shippingDataTable.AddCell(new Cell()
                    .SetFontSize(8)
                    .SetBold()
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                            .SetBackgroundColor(bgColour)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph("Direccíon")));

                shippingDataTable.AddCell(new Cell()
                    .SetFontSize(8)
                    .SetBold()
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                            .SetBackgroundColor(bgColour)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph("Colonia")));

                shippingDataTable.AddCell(new Cell()
                    .SetFontSize(8)
                    .SetBold()
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                            .SetBackgroundColor(bgColour)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph("CP")));

                shippingDataTable.AddCell(new Cell()
                    .SetFontSize(8)
                    .SetBold()
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                            .SetBackgroundColor(bgColour)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph("Zona")));

                shippingDataTable.AddCell(new Cell()
                    .SetFontSize(8)
                    .SetBold()
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                            .SetBackgroundColor(bgColour)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph("Método de pago")));

                shippingDataTable.AddCell(new Cell()
                    .SetFontSize(8)
                    .SetBold()
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                            .SetBackgroundColor(bgColour)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph("Total de la orden")));

                shippingDataTable.AddCell(new Cell()
                    .SetFontSize(8)
                    .SetBold()
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                            .SetBackgroundColor(bgColour)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph("Prod")));

                foreach (var client in ordersByClient)
                {
                    var clientOrders = client.Select(x => x).GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToList();
                    var routeOrders = orders.Where(x => x.RouteId == client.Select(y => y.RouteId).FirstOrDefault()).ToList();
                    var groceryOrderItems = client.SelectMany(x => x.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService, notDeliveredItems, false, false));
                    if (row == 1)
                    {
                        var optimized = GetStringIfOptimized(routeOrders);
                        if (!string.IsNullOrEmpty(optimized))
                        {
                            row++;
                            shippingDataTable.AddCell(new Cell(1, 6)
                            .SetBold()
                            .SetFontSize(8)
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                            .SetBorder(Border.NO_BORDER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .Add(new Paragraph(optimized + " - " + shippingDate.ToString("dd-MM-yyyy"))));
                            for (int i = 0; i < 6; i++)
                            {
                                shippingDataTable.AddCell(new Cell().SetHeight(5).SetBorder(Border.NO_BORDER).Add(new Paragraph()));
                            }
                        }
                        else
                        {
                            row++;
                            shippingDataTable.AddCell(new Cell(1, 6)
                            .SetBold()
                            .SetFontSize(8)
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                            .SetBorder(Border.NO_BORDER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .Add(new Paragraph(shippingDate.ToString("dd-MM-yyyy"))));
                            for (int i = 0; i < 6; i++)
                            {
                                shippingDataTable.AddCell(new Cell().SetHeight(5).SetBorder(Border.NO_BORDER).Add(new Paragraph()));
                            }
                        }
                    }
                    row++;
                    if (lastRoute == client.Select(x => x.RouteId).FirstOrDefault() && row > 3)
                    {
                        ordersTotal++;
                        productsTotal += groceryOrderItems.Count();
                    }
                    else
                    {
                        if (row > 3)
                        {
                            shippingDataTable.AddCell(new Cell()
                            .SetBackgroundColor(bgColour).Add(new Paragraph()));

                            shippingDataTable.AddCell(new Cell()
                            .SetFontSize(8)
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBackgroundColor(bgColour).Add(new Paragraph(ordersTotal.ToString())));

                            for (int i = 0; i < 8; i++)
                            {
                                shippingDataTable.AddCell(new Cell()
                                .SetBackgroundColor(bgColour).Add(new Paragraph()));
                            }

                            shippingDataTable.AddCell(new Cell()
                            .SetFontSize(8)
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBackgroundColor(bgColour).Add(new Paragraph(soldTotal.ToString("C"))));

                            shippingDataTable.AddCell(new Cell()
                            .SetFontSize(8)
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBackgroundColor(bgColour).Add(new Paragraph(productsTotal.ToString())));

                            var optimized = GetStringIfOptimized(routeOrders);
                            if (!string.IsNullOrEmpty(optimized))
                            {
                                row++;
                                for (int i = 0; i < 12; i++)
                                {
                                    shippingDataTable.AddCell(new Cell().SetHeight(5).SetBorder(Border.NO_BORDER).Add(new Paragraph()));
                                }
                                shippingDataTable.AddCell(new Cell(1, 6)
                                    .SetBold()
                                    .SetFontSize(8)
                                    .SetPadding(0)
                                    .SetPaddingLeft(2)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                    .Add(new Paragraph(optimized + " - " + shippingDate.ToString("dd-MM-yyyy"))));
                                for (int i = 0; i < 6; i++)
                                {
                                    shippingDataTable.AddCell(new Cell().SetHeight(5).SetBorder(Border.NO_BORDER).Add(new Paragraph()));
                                }
                            }
                            else
                            {
                                row++;
                                for (int i = 0; i < 12; i++)
                                {
                                    shippingDataTable.AddCell(new Cell().SetHeight(5).SetBorder(Border.NO_BORDER).Add(new Paragraph()));
                                }
                                shippingDataTable.AddCell(new Cell(1, 6)
                                .SetBold()
                                .SetFontSize(8)
                                .SetPadding(0)
                                .SetPaddingLeft(2)
                                .SetBorder(Border.NO_BORDER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .Add(new Paragraph(shippingDate.ToString("dd-MM-yyyy"))));
                                for (int i = 0; i < 6; i++)
                                {
                                    shippingDataTable.AddCell(new Cell().SetHeight(5).SetBorder(Border.NO_BORDER).Add(new Paragraph()));
                                }
                            }
                            row += 2;
                        }
                        ordersTotal = 1;
                        productsTotal = groceryOrderItems.Count();
                        //volumesTotal = 0;
                        soldTotal = 0;
                        lastRoute = client.Select(x => x.RouteId).FirstOrDefault();
                    }

                    var routeIds = client.Select(x => x.RouteId).ToList();
                    var routeNames = _shippingRouteService.GetAll().Where(x => routeIds.Contains(x.Id)).Select(x => x.RouteName + " (" + x.VehicleName + ")").ToList();

                    shippingDataTable.AddCell(new Cell()
                        .SetFontSize(8)
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph(string.Join(", ", routeNames.GroupBy(x => x).Select(x => x.First())))));

                    shippingDataTable.AddCell(new Cell()
                        .SetFontSize(8)
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph(ordersTotal.ToString())));

                    shippingDataTable.AddCell(new Cell()
                        .SetFontSize(8)
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph(string.Join(", ", client.Select(x => x.SelectedShippingTime).GroupBy(x => x).Select(x => x.First())))));

                    shippingDataTable.AddCell(new Cell()
                        .SetFontSize(8)
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph(string.Join(", ", client.Select(x => x.CustomOrderNumber)))));

                    shippingDataTable.AddCell(new Cell()
                        .SetFontSize(8)
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph(client.Select(x => x.ShippingAddress).FirstOrDefault().FirstName + " " + client.Select(x => x.ShippingAddress).FirstOrDefault().LastName)
                        .SetFixedLeading(10)));

                    var address1 = new Paragraph(client.Select(x => x.ShippingAddress).FirstOrDefault().Address1 ?? "");
                    shippingDataTable.AddCell(new Cell()
                        .SetFontSize(8)
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(address1.SetFixedLeading(10))
                        );

                    var address2 = new Paragraph(client.Select(x => x.ShippingAddress).FirstOrDefault().Address2 ?? "");
                    shippingDataTable.AddCell(new Cell()
                        .SetFontSize(8)
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(address2.SetFixedLeading(10)));

                    shippingDataTable.AddCell(new Cell()
                        .SetFontSize(8)
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph(client.Select(x => x.ShippingAddress).FirstOrDefault().ZipPostalCode)));

                    var orderZoneId = client.Select(x => x.ZoneId).FirstOrDefault();
                    var zone = zones.Where(x => x.Id == orderZoneId).FirstOrDefault();
                    var zoneName = orderZoneId == null ? "N/A" : (zone == null ? "N/A" : zone.ZoneName);
                    shippingDataTable.AddCell(new Cell()
                        .SetFontSize(8)
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph(zoneName)));

                    if (client.Count() > 1)
                    {
                        shippingDataTable.AddCell(new Cell()
                        .SetFontSize(8)
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph(string.Join(", ", client.Select(x => x.PaymentStatus == PaymentStatus.Paid ? $"Pagado (#{x.CustomOrderNumber})" : OrderUtils.GetPaymentOptionName(x.PaymentMethodSystemName) + $" (#{x.CustomOrderNumber})")))));
                    }
                    else
                    {
                        shippingDataTable.AddCell(new Cell()
                        .SetFontSize(8)
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph(client.Select(x => x.PaymentStatus).First() == PaymentStatus.Paid ? "Pagado" : OrderUtils.GetPaymentOptionName(client.Select(x => x.PaymentMethodSystemName).First()))));
                    }

                    if (client.Count() > 1)
                    {
                        shippingDataTable.AddCell(new Cell()
                        .SetFontSize(8)
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph(string.Join(", ", client.Select(x => x.OrderTotal.ToString("C") + $" (#{x.CustomOrderNumber})")))));
                    }
                    else
                    {
                        shippingDataTable.AddCell(new Cell()
                        .SetFontSize(8)
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph(client.Select(x => x.OrderTotal.ToString("C")).First())));
                    }

                    soldTotal += client.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                    //var currentVolume = GetOrderVolumes(clientOrders);
                    //volumesTotal += currentVolume;
                    shippingDataTable.AddCell(new Cell()
                        .SetFontSize(8)
                            .SetPadding(0)
                            .SetPaddingLeft(2)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(new Paragraph(groceryOrderItems.Count().ToString())));

                }
                shippingDataTable.AddCell(new Cell()
                            .SetBackgroundColor(bgColour).Add(new Paragraph()));

                shippingDataTable.AddCell(new Cell()
                .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetBackgroundColor(bgColour).Add(new Paragraph(ordersTotal.ToString())));

                for (int i = 0; i < 8; i++)
                {
                    shippingDataTable.AddCell(new Cell()
                    .SetBackgroundColor(bgColour).Add(new Paragraph()));
                }

                shippingDataTable.AddCell(new Cell()
                .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetBackgroundColor(bgColour).Add(new Paragraph(soldTotal.ToString("C"))));

                shippingDataTable.AddCell(new Cell()
                .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetBackgroundColor(bgColour).Add(new Paragraph(productsTotal.ToString())));

                doc.Add(shippingDataTable);
                doc.Flush();
                doc.Close();
                shippingDataStream = stream.ToArray();
            }
            return Ok();
        }

        //private decimal GetOrderVolumes(List<Order> orders)
        //{
        //    var orderTypes = _orderTypeService.GetAll().ToList();
        //    int totalItems = orders.Select(x => x.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService).Count).DefaultIfEmpty().Sum();
        //    return orderTypes.Where(x => totalItems >= x.MinimumProductQty && totalItems <= x.MaxProductQty)
        //        .Select(x => x.CargoSpace).FirstOrDefault();
        //}

        private string GetStringIfOptimized(List<Order> orders)
        {
            var isNotOptimized = orders.Where(x => string.IsNullOrEmpty(x.ShippingAddress.Latitude) || string.IsNullOrEmpty(x.ShippingAddress.Longitude)).Any();
            return (isNotOptimized ? string.Empty : "(Optimizado por Google)");
            //return (isNotOptimized ? string.Empty : string.Empty);
        }

        private byte[] ExportOrdersDataToXlsx(List<Order> orders, DateTime shippingDate)
        {
            var parsedOrderItems = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService);
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add(shippingDate.ToString("dd-MM-yyyy"));
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Fecha entrega";
                    worksheet.Cells[row, 2].Value = "Categoría";
                    worksheet.Cells[row, 3].Value = "Producto";
                    worksheet.Cells[row, 4].Value = "Identif";
                    worksheet.Cells[row, 5].Value = "Precio (kg/pz)";
                    worksheet.Cells[row, 6].Value = "Pedido";
                    worksheet.Cells[row, 7].Value = "Orden";
                    worksheet.Cells[row, 8].Value = "Programado";
                    worksheet.Cells[row, 9].Value = "Cliente";
                    worksheet.Cells[row, 10].Value = "Dirección";
                    worksheet.Cells[row, 11].Value = "Colonia";
                    worksheet.Cells[row, 12].Value = "CP";
                    worksheet.Cells[row, 13].Value = "Ruta";
                    worksheet.Cells[row, 14].Value = "Pos. entrega";
                    worksheet.Cells[row, 15].Value = "Método pago";
                    worksheet.Cells[row, 16].Value = "Specs";
                    worksheet.Cells[row, 17].Value = "Cantidad";
                    worksheet.Cells[row, 18].Value = "U. Medida";
                    worksheet.Cells[row, 19].Value = "Bodega";
                    worksheet.Cells[row, 20].Value = "Costo (kg/pz)";

                    foreach (var item in parsedOrderItems)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = shippingDate;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mm-yyyy";
                        string category = item.Product.ProductCategories.Where(x => x.Category.ParentCategoryId == 0).FirstOrDefault()?.Category.Name;
                        worksheet.Cells[row, 2].Value = string.IsNullOrWhiteSpace("category") ? "Sin categoría padre" : category;
                        worksheet.Cells[row, 3].Value = item.Product.Name;
                        worksheet.Cells[row, 4].Value = string.Empty;
                        worksheet.Cells[row, 5].Value = item.Product.Price;
                        worksheet.Cells[row, 6].Value = item.PriceInclTax;
                        worksheet.Cells[row, 7].Value = item.Order.CustomOrderNumber;
                        worksheet.Cells[row, 8].Value = item.Order.SelectedShippingTime;
                        worksheet.Cells[row, 9].Value = item.Order.ShippingAddress.FirstName + " " + item.Order.ShippingAddress.LastName;
                        worksheet.Cells[row, 10].Value = item.Order.ShippingAddress.Address1;
                        worksheet.Cells[row, 11].Value = item.Order.ShippingAddress.Address2;
                        worksheet.Cells[row, 12].Value = item.Order.ShippingAddress.ZipPostalCode;
                        var route = _shippingRouteService.GetAll().Where(x => x.Id == item.Order.RouteId).Select(x => x.RouteName).FirstOrDefault();
                        worksheet.Cells[row, 13].Value = string.IsNullOrWhiteSpace(route) ? string.Empty : route;
                        worksheet.Cells[row, 14].Value = item.Order.RouteDisplayOrder;
                        worksheet.Cells[row, 15].Value = item.Order.PaymentStatus == PaymentStatus.Paid ? $"Pagado" : OrderUtils.GetPaymentOptionName(item.Order.PaymentMethodSystemName);
                        worksheet.Cells[row, 16].Value = item.SelectedPropertyOption;

                        string unit = "pz";
                        decimal weight = item.Quantity;
                        if (item.EquivalenceCoefficient > 0 && item.BuyingBySecondary)
                        {
                            weight = (item.Quantity * 1000) / item.EquivalenceCoefficient;
                            unit = "gr";
                        }
                        else if (item.WeightInterval > 0)
                        {
                            weight = item.Quantity * item.WeightInterval;
                            unit = "gr";
                        }

                        if (weight >= 1000)
                        {
                            weight = (weight / 1000);
                            unit = "kg";
                        }

                        worksheet.Cells[row, 17].Value = weight;
                        worksheet.Cells[row, 18].Value = unit;
                        worksheet.Cells[row, 19].Value = string.Empty;
                        worksheet.Cells[row, 20].Value = item.Product.ProductCost;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return stream.ToArray();
            }
        }

        public virtual IActionResult PyL()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/PyL/PyL.cshtml");
        }

        public virtual IActionResult BuyerPaymentCommitment(string date)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.BuyerReportPrint))
                return AccessDeniedView();

            var controlDate = DateTime.Now.Date;
            if (!string.IsNullOrEmpty(date))
                controlDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            MemoryStream stream = new MemoryStream();
            PdfWriter pdfWriter = new PdfWriter(stream);
            PdfDocument pdfDoc = new PdfDocument(pdfWriter);
            ps = pdfDoc.GetDefaultPageSize();
            doc = new Document(pdfDoc, PageSize.LETTER, false);
            doc.SetMargins(20, 20, 50, 20);

            NewPageHandler newPageHandler = new NewPageHandler();

            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate == controlDate).ToList();
            List<OrderItem> parsedOrderItems = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService);

            List<int> orderItemIdsOfTheDay = parsedOrderItems.Select(x => x.Id).ToList();
            var orderItemBuyerQuery = _orderItemBuyerService.GetAll().Where(x => orderItemIdsOfTheDay.Contains(x.OrderItemId));
            bool notContains = orderItemBuyerQuery.Where(x => x.CustomerId == 0).Any();
            var manufacturers = _manufacturerService.GetAllManufacturers().ToList();
            var mainManufacturers = _productMainManufacturerService.GetAll().ToList();

            if (notContains)
            {
                string text = "Todos los productos deben ser asignados a un comprador para poder generar los pagarés.";
                shoppingListTitle = new Paragraph($"{text} Si deseas asignar los compradores para esta fecha da clic ");
                var url = new Link("aquí.", PdfAction.CreateURI((Request.IsHttps ? "https" : "http") + $"://{Request.Host}/Admin/OrderItemBuyer/AssignBuyers?date={date}"));
                shoppingListTitle.Add(url);
                doc.Add(shoppingListTitle);
            }
            else
            {
                var orderItemBuyer = orderItemBuyerQuery.ToList();
                var buyerIds = orderItemBuyer.Select(x => x.CustomerId).Distinct().ToList();

                if (buyerIds.Count() > 0)
                {
                    decimal accumulated = 0;
                    for (int i = 0; i < buyerIds.Count(); i++)
                    {
                        var control = i + 1;
                        decimal buyerCash = RoundBuyerCashAmount(OrderUtils.GetBuyerCashAndTransferAmount(parsedOrderItems, buyerIds[i], orderItemBuyer, manufacturers, mainManufacturers).Cash);
                        accumulated += buyerCash;
                        PrepareBuyerPaymentCommitment(buyerCash, controlDate, control % 2 != 0);
                        if (control % 2 == 0) doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                    }

                    PrepareBuyerPaymentCommitment(4000, controlDate, buyerIds.Count % 2 == 0);

                    if (buyerIds.Count % 2 == 0)
                        doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                    PrepareBuyerPaymentCommitment(accumulated + 4000, controlDate, false);

                    doc.Flush();
                }
                else
                {
                    doc.Add(new Paragraph("No existen compradores para el día seleccionado."));
                }
            }

            doc.Close();

            return File(stream.ToArray(), MimeTypes.ApplicationPdf);
        }

        private void PrepareBuyerPaymentCommitment(decimal buyerCash, DateTime controlDate, bool addExtraSpace)
        {
            var buyerPaymentCommitmentTitle1 = new Paragraph("No. 1/1 - COMPRADORES");
            buyerPaymentCommitmentTitle1.SetFontSize(10);
            buyerPaymentCommitmentTitle1.SetTextAlignment(TextAlignment.RIGHT);
            doc.Add(buyerPaymentCommitmentTitle1);

            var buyerPaymentCommitmentTitle2 = new Paragraph("PAGARÉ");
            buyerPaymentCommitmentTitle2.SetFontSize(30);
            buyerPaymentCommitmentTitle2.SetBold();
            buyerPaymentCommitmentTitle2.SetTextAlignment(TextAlignment.CENTER);
            doc.Add(buyerPaymentCommitmentTitle2);

            var buyerPaymentCommitmentBody = new Paragraph();
            buyerPaymentCommitmentBody.SetFontSize(10);
            buyerPaymentCommitmentBody.SetTextAlignment(TextAlignment.LEFT);
            buyerPaymentCommitmentBody.Add("A través de este pagaré, yo ______________________________________________ reconozco que debo y me comprometo a pagar la cantidad de ");
            buyerPaymentCommitmentBody.Add(new Text($"{buyerCash:C} MXN ({NumToLetterEsp(buyerCash).ToLower()} pesos 00/100 M.N.)").SetBold());
            buyerPaymentCommitmentBody.Add($" a la orden de Experiencia Gourmet Uvas y Moras, S.A.P.I de C.V. (Central en línea). Dicha cantidad será pagada para el día ");
            buyerPaymentCommitmentBody.Add(new Text($"{controlDate.ToString("dd 'de' MMMM 'de' yyyy", new CultureInfo("es-MX"))} ").SetBold());
            buyerPaymentCommitmentBody.Add("de la forma en la que lo solicite el beneficiario de este documento.");
            doc.Add(buyerPaymentCommitmentBody);

            var table = new Table(3);
            table.SetWidth(PageSize.A4.GetWidth() - 30);
            table.AddCell(new Cell(0, 3).SetBorder(Border.NO_BORDER).Add(new Paragraph("")).SetHeight(30));

            table.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph("Acepto pagar a su vencimiento").SetFontSize(10).SetTextAlignment(TextAlignment.CENTER)));
            table.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetWidth(30));
            table.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph("Huella digital").SetFontSize(10).SetTextAlignment(TextAlignment.CENTER)));

            table.AddCell(new Cell().Add(new Paragraph("")).SetBorder(Border.NO_BORDER));
            table.AddCell(new Cell().SetHeight(100).SetBorder(Border.NO_BORDER));
            table.AddCell(new Cell());

            table.AddCell(new Cell()
                .SetBorderLeft(Border.NO_BORDER)
                .SetBorderRight(Border.NO_BORDER)
                .SetBorderBottom(Border.NO_BORDER)
                .Add(new Paragraph("Nombre completo y firma")
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)));
            table.AddCell(new Cell().Add(new Paragraph("")).SetBorder(Border.NO_BORDER));
            table.AddCell(new Cell().Add(new Paragraph("")).SetBorder(Border.NO_BORDER));
            if (addExtraSpace)
            {
                table.AddCell(new Cell(1, 3).Add(new Paragraph("")).SetBorder(Border.NO_BORDER).SetHeight(16));
                table.AddCell(new Cell(1, 3).Add(new Paragraph("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -")).SetBorder(Border.NO_BORDER).SetHeight(16));
                table.AddCell(new Cell(1, 3).Add(new Paragraph("")).SetBorder(Border.NO_BORDER).SetHeight(16));
            }
            doc.Add(table);
        }

        public virtual IActionResult DeliveryPaymentCommitment(string date)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.DeliveryReportPrint))
                return AccessDeniedView();

            var controlDate = DateTime.Now.Date;
            if (!string.IsNullOrEmpty(date))
                controlDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            MemoryStream stream = new MemoryStream();
            PdfWriter pdfWriter = new PdfWriter(stream);
            PdfDocument pdfDoc = new PdfDocument(pdfWriter);
            ps = pdfDoc.GetDefaultPageSize();
            doc = new Document(pdfDoc, PageSize.LETTER, false);
            doc.SetMargins(20, 20, 50, 20);

            NewPageHandler newPageHandler = new NewPageHandler();

            var notAssigned = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate == controlDate && x.RouteId == 0).Any();
            if (notAssigned)
            {
                string text = "Todas las órdenes deben estar asignadas a una ruta para poder generar los pagarés.";
                shoppingListTitle = new Paragraph(text);
                doc.Add(shoppingListTitle);
            }
            else
            {
                var routesGroup = OrderUtils.GetFilteredOrders(_orderService)
                    .Where(x => x.SelectedShippingDate == controlDate && (x.PaymentMethodSystemName == "Payments.CashOnDelivery" || x.PaymentMethodSystemName == "Payments.CardOnDelivery" || x.PaymentMethodSystemName == "Payments.MercadoPagoQr"))
                    .Select(x => new
                    {
                        x.RouteId,
                        x.OrderTotal
                    })
                    .GroupBy(x => x.RouteId)
                    .OrderBy(x => x.Key)
                    .ToList();
                var routes = _shippingRouteService.GetAll().ToList();

                if (routesGroup.Count() > 0)
                {
                    for (int i = 0; i < routesGroup.Count(); i++)
                    {
                        decimal routeOnDeliveryTotal = routesGroup[i].Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                        string routeName = routes.Where(x => x.Id == routesGroup[i].Key).Select(x => x.RouteName).FirstOrDefault();
                        PrepareDeliveryPaymentCommitment(routeOnDeliveryTotal, controlDate, routeName);
                        if (i < routesGroup.Count() - 1)
                            doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                    }

                    doc.Flush();
                }
                else
                {
                    doc.Add(new Paragraph("No existen órdenes para la fecha seleccionada."));
                }
            }

            doc.Close();

            return File(stream.ToArray(), MimeTypes.ApplicationPdf);
        }

        private void PrepareDeliveryPaymentCommitment(decimal orderOnDeliveryTotal, DateTime controlDate, string routeName)
        {
            for (int i = 0; i < 2; i++)
            {
                var deliveryPaymentCommitmentTitle1 = new Paragraph("No. 1/1 - REPARTIDORES");
                deliveryPaymentCommitmentTitle1.SetFontSize(10);
                deliveryPaymentCommitmentTitle1.SetTextAlignment(TextAlignment.RIGHT);
                doc.Add(deliveryPaymentCommitmentTitle1);

                var deliveryPaymentCommitmentTitle2 = new Paragraph("PAGARÉ");
                deliveryPaymentCommitmentTitle2.SetFontSize(25);
                deliveryPaymentCommitmentTitle2.SetBold();
                deliveryPaymentCommitmentTitle2.SetTextAlignment(TextAlignment.CENTER);
                doc.Add(deliveryPaymentCommitmentTitle2);

                var deliveryPaymentCommitmentTitle3 = new Paragraph(routeName);
                deliveryPaymentCommitmentTitle3.SetFontSize(12);
                deliveryPaymentCommitmentTitle3.SetBold();
                deliveryPaymentCommitmentTitle3.SetTextAlignment(TextAlignment.CENTER);
                doc.Add(deliveryPaymentCommitmentTitle3);

                var deliveryPaymentCommitmentBody = new Paragraph();
                deliveryPaymentCommitmentBody.SetFontSize(10);
                deliveryPaymentCommitmentBody.SetTextAlignment(TextAlignment.LEFT);
                deliveryPaymentCommitmentBody.Add("A través de este pagaré, yo ______________________________________________ reconozco que debo y me comprometo a pagar la cantidad de ");
                deliveryPaymentCommitmentBody.Add(new Text($"{orderOnDeliveryTotal:C} MXN ({NumToLetterEsp(orderOnDeliveryTotal).ToLower()} pesos 00/100 M.N.)").SetBold());
                deliveryPaymentCommitmentBody.Add($" a la orden de Experiencia Gourmet Uvas y Moras, S.A.P.I de C.V. (Central en línea). Dicha cantidad será pagada para el día ");
                deliveryPaymentCommitmentBody.Add(new Text($"{controlDate.ToString("dd 'de' MMMM 'de' yyyy", new CultureInfo("es-MX"))} ").SetBold());
                deliveryPaymentCommitmentBody.Add("de la forma en la que lo solicite el beneficiario de este documento.");
                doc.Add(deliveryPaymentCommitmentBody);

                var table = new Table(3);
                table.SetWidth(PageSize.A4.GetWidth() - 30);
                table.AddCell(new Cell(0, 3).SetBorder(Border.NO_BORDER).Add(new Paragraph("")).SetHeight(30));

                table.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph("Acepto pagar a su vencimiento").SetFontSize(10).SetTextAlignment(TextAlignment.CENTER)));
                table.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetWidth(30));
                table.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph("Huella digital").SetFontSize(10).SetTextAlignment(TextAlignment.CENTER)));

                table.AddCell(new Cell().Add(new Paragraph("")).SetBorder(Border.NO_BORDER));
                table.AddCell(new Cell().SetHeight(80).SetBorder(Border.NO_BORDER));
                table.AddCell(new Cell());

                table.AddCell(new Cell()
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetBorderRight(Border.NO_BORDER)
                    .SetBorderBottom(Border.NO_BORDER)
                    .Add(new Paragraph("Nombre completo y firma")
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.CENTER)));
                table.AddCell(new Cell().Add(new Paragraph("")).SetBorder(Border.NO_BORDER));
                table.AddCell(new Cell().Add(new Paragraph("")).SetBorder(Border.NO_BORDER));

                if (i == 0)
                {
                    table.AddCell(new Cell(1, 3).Add(new Paragraph("")).SetBorder(Border.NO_BORDER).SetHeight(16));
                    table.AddCell(new Cell(1, 3).Add(new Paragraph("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -")).SetBorder(Border.NO_BORDER).SetHeight(16));
                    table.AddCell(new Cell(1, 3).Add(new Paragraph("")).SetBorder(Border.NO_BORDER).SetHeight(16));
                }

                doc.Add(table);
            }
        }

        [HttpPost]
        public IActionResult PylListData(DataSourceRequest command, PyLSearchModel datesForSearch)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var query = _orderService.GetAllOrdersQuery().Where(x => x.OrderGuid != null && x.OrderStatusId != 40);

            IOrderedQueryable<IGrouping<DateTime?, Order>> queryList = null;

            //// Search by date
            DateTime? dateFirst = null;
            DateTime? dateLast = null;

            if (!string.IsNullOrEmpty(datesForSearch.DateF))
            {
                dateFirst = DateTime.ParseExact(datesForSearch.DateF, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(datesForSearch.DateL))
            {
                dateLast = DateTime.ParseExact(datesForSearch.DateL, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            if (dateFirst != null && dateLast != null)
            {
                bool result = Nullable.Compare(dateFirst, dateLast) > 0;
                if (result)
                {
                    DateTime? tempDate = new DateTime();
                    tempDate = dateLast;
                    dateLast = dateFirst;
                    dateFirst = tempDate;
                }
                queryList = query.GroupBy(x => x.SelectedShippingDate).Where(x => x.Key.Value >= dateFirst.Value && x.Key <= dateLast.Value).OrderByDescending(x => x.Key);
            }
            else if (dateFirst == null && dateLast != null)
            {
                queryList = query.GroupBy(x => x.SelectedShippingDate).Where(x => x.Key.Value == dateLast.Value).OrderByDescending(x => x.Key);
            }
            else if (dateFirst != null && dateLast == null)
            {
                queryList = query.GroupBy(x => x.SelectedShippingDate).Where(x => x.Key.Value == dateFirst.Value).OrderByDescending(x => x.Key);
            }
            else
            {
                queryList = query.GroupBy(x => x.SelectedShippingDate).OrderByDescending(x => x.Key);
            }
            ////

            var pagedList = GroupedPageList(queryList, command.Page - 1, command.PageSize);
            //pagedList = pagedList.Where(x => x.Select(y => y.OrderStatus != OrderStatus.Cancelled));

            DataSourceResult gridModel = new DataSourceResult();

            gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    Date = (x.Key.Value).ToString("dd/MM/yyyy"),
                    Sales = (Math.Round(x.Sum(y => y.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService).Sum(z => z.PriceInclTax)) * 100) / 100).ToString("C", CultureInfo.CurrentCulture),
                    Expenses = (Math.Round(GetExpenses(x.Select(y => y.Id).ToList(), x.Key.Value) * 100) / 100).ToString("C", CultureInfo.CurrentCulture),
                    Profit = (Math.Round((x.Sum(y => y.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService).Sum(z => z.PriceInclTax)) - GetExpenses(x.Select(y => y.Id).ToList(), x.Key.Value)) * 100) / 100).ToString("C", CultureInfo.CurrentCulture).Replace("(", "").Replace(")", ""),
                    Porcentage = (Math.Round(((1 - (GetExpenses(x.Select(y => y.Id).ToList(), x.Key.Value) / x.Sum(y => y.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService).Sum(z => z.PriceInclTax)))) * 100) * 100) / 100).ToString() + "%",
                }).ToList(),
                Total = queryList.Count()
            };

            return Json(gridModel);
        }

        public class PyLSearchModel
        {
            public string DateF { get; set; }
            public string DateL { get; set; }
        }

        private List<IGrouping<DateTime?, Order>> GroupedPageList(IQueryable<IGrouping<DateTime?, Order>> source, int pageIndex, int pageSize)
        {
            List<IGrouping<DateTime?, Order>> filteredList = new List<IGrouping<DateTime?, Order>>();
            var total = source.Count();
            var totalCount = total;
            var totalPages = total / pageSize;

            if (total % pageSize > 0)
                totalPages++;

            var newPageSize = pageSize;
            var newPageIndex = pageIndex;

            filteredList.AddRange(source.Skip(pageIndex * pageSize).Take(pageSize).ToList());

            return filteredList;
        }

        [HttpGet]
        public async Task<IActionResult> PylOrderListData(string date)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //var dateFinal = (Convert.ToDateTime().Date;
            var dateFinal = DateTime.ParseExact(date.Replace("-", "/"), "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var orders = OrderUtils.GetFilteredOrders(_orderService).ToList().Where(x => x.SelectedShippingDate.Value == dateFinal.Date && x.OrderStatusId != 40).ToList();
            var parsedOrderItems = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService);

            var itemsGroup = parsedOrderItems.GroupBy(x => x.Product);

            List<PlOrder> plOrders = new List<PlOrder>();
            foreach (var group in itemsGroup)
            {
                plOrders.Add(new PlOrder()
                {
                    Product = group.Key.Name,
                    Data = group.Select(x => new PlOrderData()
                    {
                        OrderNumber = x.OrderId,
                        Sales = x.UnitPriceInclTax,
                        Requested = x.PriceInclTax,
                        Expenses = GetItemExpenses(x.Id, dateFinal.Date),
                        Profit = (x.PriceInclTax - GetItemExpenses(x.Id, dateFinal.Date)),
                    }).ToList(),
                    ScrapperPrices = await GetScrapperPrices(group.Select(x => x.Product).FirstOrDefault(), dateFinal.Date)
                });
            }

            List<Domain.OrderReports.AditionalCost> extras = new List<Domain.OrderReports.AditionalCost>();
            List<PlExtraCostData> extraCosts = new List<PlExtraCostData>();
            extras = _aditionalCostService.GetAll().Where(y => y.Date == dateFinal && y.Deleted == false).ToList();

            if (extras.Count > 0)
            {
                foreach (var extra in extras)
                {
                    extraCosts.Add(new PlExtraCostData()
                    {
                        Description = string.IsNullOrEmpty(extra.Description) ? "Sin descripción" : extra.Description,
                        Cost = extra.Cost
                    });
                }
            }
            else
            {
                extraCosts.Add(new PlExtraCostData()
                {
                    Description = "No hay costos extras para este día",
                    Cost = 0
                });
            }

            PlAll plAll = new PlAll
            {
                PlOrders = plOrders,
                ExtraCosts = extraCosts
            };

            return Ok(plAll);
        }

        public decimal GetExpenses(IList<int> orderIds, DateTime date)
        {
            decimal final = new decimal(0);
            foreach (var orderId in orderIds)
            {
                final += _orderReportService.GetAll().Where(y => y.OrderId == orderId).Select(y => y.RequestedQtyCost).DefaultIfEmpty().Sum(y => y);
            }
            var extras = _aditionalCostService.GetAll().Where(y => y.Date == date && y.Deleted == false).Select(y => y.Cost).DefaultIfEmpty().Sum(y => y);
            if (!extras.Equals(null) && !extras.Equals(0))
                final += extras;

            return (final);
        }

        public decimal GetItemExpenses(int orderItemExpenses, DateTime date)
        {
            decimal final = new decimal(0);
            final = _orderReportService.GetAll().Where(y => y.OrderItemId == orderItemExpenses).Select(y => y.RequestedQtyCost).FirstOrDefault();

            return (final);
        }

        public async Task<PlScrapperData> GetScrapperPrices(Product product, DateTime date)
        {
            PlScrapperData prices = new PlScrapperData
            {
                ChedrauiPrice = ((Math.Round(await GetHistoricPrice(date, product.ChedrauiProductId)) * 100) / 100)
                .ToString("C", CultureInfo.CurrentCulture),

                LaComerPrice = ((Math.Round(await GetHistoricPrice(date, product.LaComerProductId)) * 100) / 100)
                .ToString("C", CultureInfo.CurrentCulture),

                SuperamaPrice = ((Math.Round(await GetHistoricPrice(date, product.SuperamaProductId)) * 100) / 100)
                .ToString("C", CultureInfo.CurrentCulture),

                WalmartPrice = ((Math.Round(await GetHistoricPrice(date, product.WalmartProductId)) * 100) / 100)
                .ToString("C", CultureInfo.CurrentCulture),
            };

            return (prices);
        }

        private async Task<decimal> GetHistoricPrice(DateTime date, int wsUnitId)
        {
            using (var client = new HttpClient())
            {
                string url = (Request.IsHttps ? "https://" : "http://") + $"{Request.Host}/api/webscraping/GetProdutPriceByDate?date={date}&wsUnitId={wsUnitId}";
                HttpResponseMessage result = await client.GetAsync(url);

                if (result.IsSuccessStatusCode)
                {
                    string value = await result.Content.ReadAsStringAsync();
                    decimal.TryParse(value, out decimal parsedResult);
                    return parsedResult;
                }

                return 0;
            }
        }

        [HttpGet]
        public IActionResult GetRoutesOfTheDay(string selectedDate)
        {
            parsedDate = DateTime.ParseExact(selectedDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var routes = _shippingRouteService.GetAll().Select(x => new { x.Id, x.RouteName }).ToList();
            var routesOfTheDay = _orderService.GetAllOrdersQuery()
                .Where(x => x.SelectedShippingDate == parsedDate)
                .Where(x => x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10))
                .Select(x => x.RouteId).Distinct()
                .ToList();

            if (routesOfTheDay.Where(x => x == 0).Any())
            {
                return BadRequest("route0");
            }
            else if (!routesOfTheDay.Any())
            {
                return BadRequest("noOrders");
            }
            else
            {
                var finalRoutes = routes.Where(x => routesOfTheDay.Contains(x.Id)).Select(x => new
                {
                    Name = x.RouteName,
                    x.Id
                }).OrderBy(x => x.Name).ToList();
                return Ok(finalRoutes);
            }
        }

        public IActionResult ViewOrders(string date)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/PyL/PyLOrders.cshtml", date);
        }

        public class PlAll
        {
            public List<PlOrder> PlOrders { get; set; }
            public List<PlExtraCostData> ExtraCosts { get; set; }
        }

        public class PlOrder
        {
            public string Product { get; set; }
            public List<PlOrderData> Data { get; set; }
            public PlScrapperData ScrapperPrices { get; set; }
        }

        public class PlOrderData
        {
            public int OrderNumber { get; set; }
            public decimal Sales { get; set; }
            public decimal Requested { get; set; }
            public decimal Expenses { get; set; }
            public decimal Profit { get; set; }
        }

        public class PlExtraCostData
        {
            public string Description { get; set; }
            public decimal Cost { get; set; }
        }

        public class PlScrapperData
        {
            public string ChedrauiPrice { get; set; }
            public string LaComerPrice { get; set; }
            public string SuperamaPrice { get; set; }
            public string WalmartPrice { get; set; }
        }

        [HttpPost]
        public IActionResult PaymentMethodsList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var methods = _paymentService.LoadActivePaymentMethods().ToList();
            var elements = methods.Select(x => new
            {
                Name = x.PluginDescriptor.FriendlyName
            }).ToList();

            return Json(elements);
        }

        [HttpPost]
        public IActionResult UpdateOrderPaymentMethod(UpdateOrderPaymentMethodModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null) return NotFound();

            var newSystemName = _paymentService.LoadActivePaymentMethods().Where(x => x.PluginDescriptor.FriendlyName.Equals(model.PaymentName))
                .Select(x => x.PluginDescriptor.SystemName).FirstOrDefault().ToString();
            if (newSystemName == null) return NotFound();

            order.PaymentMethodSystemName = newSystemName;
            order.OrderNotes.Add(new OrderNote
            {
                Note = $"{_workContext.CurrentCustomer.Email} cambió el método de pago de {model.OldPaymentName} a {model.PaymentName}.",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = _workContext.CurrentCustomer.Id
            });
            _orderService.UpdateOrder(order);

            return NoContent();
        }

        [HttpPost]
        public IActionResult UpdateOrderShippingDate(string oldShippingTime,
            string shippingTime,
            string oldShippingDate,
            string shippingDate,
            string orderId)
        {
            var isChanged = false;
            var order = _orderService.GetOrderById(Int32.Parse(orderId));
            DateTime? date = null;
            if (!string.IsNullOrEmpty(shippingDate))
            {
                date = DateTime.ParseExact(shippingDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            if ((oldShippingTime != shippingTime) || (oldShippingDate != shippingDate))
            {
                if (order != null)
                {
                    if (date != null)
                    {
                        if (oldShippingDate != shippingDate)
                        {
                            order.SelectedShippingDate = date;
                            isChanged = true;
                        }
                    }
                    if (oldShippingTime != shippingTime)
                    {
                        if (order != null)
                        {
                            order.SelectedShippingTime = shippingTime;
                            isChanged = true;
                        }
                    }
                    if (isChanged)
                    {
                        order.OrderNotes.Add(new OrderNote
                        {
                            Note = $"{_workContext.CurrentCustomer.Email} cambió la fecha de entrega de {oldShippingDate} {oldShippingTime} a {shippingDate} {shippingTime}.",
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow,
                            CustomerId = _workContext.CurrentCustomer.Id
                        });
                        _orderService.UpdateOrder(order);

                        var orderItemBuyers = _orderItemBuyerService.GetAll().Where(x => x.OrderId == order.Id).ToList();
                        foreach (var orderItemBuyer in orderItemBuyers)
                            _orderItemBuyerService.Delete(orderItemBuyer);

                        return Ok();
                    }
                }
            }
            else
            {
                return Ok();
            }

            return NotFound();
        }

        [HttpGet]
        [Route("[controller]/[action]")]
        [AllowAnonymous]
        public IActionResult CheckHolidayTimes(string date)
        {
            if (string.IsNullOrWhiteSpace(date)) return BadRequest();

            DateTime selectedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            int ordersTime1 = _orderService.GetAllOrdersQuery()
                .Where(x => DbFunctions.TruncateTime(x.SelectedShippingDate) == selectedDate && x.SelectedShippingTime == "1:00 PM - 3:00 PM")
                .AsEnumerable()
                .Where(x => x.OrderStatus != OrderStatus.Cancelled)
                .Count();
            int ordersTime2 = _orderService.GetAllOrdersQuery()
                .Where(x => DbFunctions.TruncateTime(x.SelectedShippingDate) == selectedDate && x.SelectedShippingTime == "3:00 PM - 5:00 PM")
                .AsEnumerable()
                .Where(x => x.OrderStatus != OrderStatus.Cancelled)
                .Count();
            return Ok(new List<int>() { ordersTime1, ordersTime2 });
        }

        private void DailyRouteFormatReport(IList<Order> orders)
        {
            doc.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));

            var routeIds = orders.Select(x => x.RouteId).ToList();
            var routes = _shippingRouteService.GetAll().Where(x => routeIds.Contains(x.Id)).Select(x => x.RouteName).OrderBy(x => x).ToList();

            string imageFile = $"wwwroot/images/logo.png";
            ImageData data = ImageDataFactory.Create(imageFile);

            Image img = new Image(data);
            img.SetHeight(20);
            img.SetWidth(120);

            Paragraph Img = new Paragraph();
            Img.Add(img);
            Img.SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
            Img.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);

            Paragraph header1 = new Paragraph("REPORTE DIARIO DE COORDINACIÓN DE RUTAS");
            header1.SetBold();
            header1.SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            var shippingFormatTableHeader = new Table(2);
            shippingFormatTableHeader.SetWidth(PageSize.A4.GetWidth() - 30);
            shippingFormatTableHeader.SetBorder(Border.NO_BORDER);

            shippingFormatTableHeader.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .Add(img));

            shippingFormatTableHeader.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetBold()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .Add(header1));

            shippingFormatTableHeader.AddCell(new Cell().SetPaddingTop(12).SetBorder(Border.NO_BORDER));

            var shippingFormatTableHeader2 = new Table(new float[] { 1, 2 }, true);
            shippingFormatTableHeader2.SetWidth(PageSize.A4.GetWidth() - 30);

            Paragraph p1 = new Paragraph("FECHA")
                .SetFontSize(10)
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            shippingFormatTableHeader2.AddCell(new Cell().Add(p1));
            shippingFormatTableHeader2.AddCell(new Cell().Add(new Paragraph(orders.FirstOrDefault().SelectedShippingDate.Value.ToString("dd-MM-yyyy"))));

            Paragraph p2 = new Paragraph("RESPONSABLE")
                .SetFontSize(10)
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            shippingFormatTableHeader2.AddCell(new Cell().Add(p2));
            shippingFormatTableHeader2.AddCell(new Cell());

            Paragraph p3 = new Paragraph("NÚMERO DE RUTAS")
                .SetFontSize(10)
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            shippingFormatTableHeader2.AddCell(new Cell().Add(p3));
            shippingFormatTableHeader2.AddCell(new Cell().Add(new Paragraph(routes.Count.ToString())));

            Paragraph p4 = new Paragraph("TOTAL COBRADO EN RUTAS")
                .SetFontSize(10)
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            shippingFormatTableHeader2.AddCell(new Cell().Add(p4));
            shippingFormatTableHeader2.AddCell(new Cell());

            Paragraph p5 = new Paragraph("TOTAL PROPINAS EN TARJETA")
                .SetFontSize(10)
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            shippingFormatTableHeader2.AddCell(new Cell().Add(p5));
            shippingFormatTableHeader2.AddCell(new Cell());

            Paragraph p6 = new Paragraph("TOTAL GASTOS DE RUTA")
                .SetFontSize(10)
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            shippingFormatTableHeader2.AddCell(new Cell().Add(p6));
            shippingFormatTableHeader2.AddCell(new Cell());

            Paragraph p7 = new Paragraph("TOTAL A ENTREGAR")
                .SetFontSize(10)
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            shippingFormatTableHeader2.AddCell(new Cell().Add(p7));
            shippingFormatTableHeader2.AddCell(new Cell());

            shippingFormatTableHeader2.AddCell(new Cell(1, 2).SetPaddingTop(12).SetBorder(Border.NO_BORDER));

            var shippingFormatMainTable = new Table(6)
            .SetWidth(PageSize.A4.GetWidth() - 30);

            Paragraph p8 = new Paragraph("DESGLOSE DE RUTAS")
                .SetFontSize(10)
                .SetBold()
                .SetFontColor(new DeviceRgb(255, 255, 255))
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

            shippingFormatMainTable.AddCell(new Cell(1, 6)
                .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                .Add(p8));

            Paragraph p9 = new Paragraph("RUTA")
                .SetFontSize(10)
                .SetBold()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

            shippingFormatMainTable.AddCell(new Cell()
                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                .Add(p9));

            Paragraph p10 = new Paragraph("FONDO INICIAL")
                .SetFontSize(10)
                .SetBold()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

            shippingFormatMainTable.AddCell(new Cell()
                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                .Add(p10));

            Paragraph p11 = new Paragraph("(+) COBRADO EN RUTA")
                .SetFontSize(10)
                .SetBold()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

            shippingFormatMainTable.AddCell(new Cell()
                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                .Add(p11));

            Paragraph p12 = new Paragraph("(-) PROPINAS EN TARJETA")
                .SetFontSize(10)
                .SetBold()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

            shippingFormatMainTable.AddCell(new Cell()
                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                .Add(p12));

            Paragraph p13 = new Paragraph("(-) GASTOS EN RUTA")
               .SetFontSize(10)
               .SetBold()
               .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

            shippingFormatMainTable.AddCell(new Cell()
                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                .Add(p13));

            Paragraph p14 = new Paragraph("TOTAL A ENTREGAR")
               .SetFontSize(10)
               .SetBold()
               .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

            shippingFormatMainTable.AddCell(new Cell()
                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                .Add(p14));

            foreach (var route in routes)
            {
                shippingFormatMainTable.AddCell(new Cell().Add(new Paragraph(route)));
                shippingFormatMainTable.AddCell(new Cell());
                shippingFormatMainTable.AddCell(new Cell());
                shippingFormatMainTable.AddCell(new Cell());
                shippingFormatMainTable.AddCell(new Cell());
                shippingFormatMainTable.AddCell(new Cell());
            }

            Paragraph p15 = new Paragraph("TOTAL")
                .SetFontSize(10)
                .SetBold()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

            shippingFormatMainTable.AddCell(new Cell()
                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                .Add(p15));

            shippingFormatMainTable.AddCell(new Cell().SetBackgroundColor(new DeviceRgb(220, 220, 220)));
            shippingFormatMainTable.AddCell(new Cell().SetBackgroundColor(new DeviceRgb(220, 220, 220)));
            shippingFormatMainTable.AddCell(new Cell().SetBackgroundColor(new DeviceRgb(220, 220, 220)));
            shippingFormatMainTable.AddCell(new Cell().SetBackgroundColor(new DeviceRgb(220, 220, 220)));
            shippingFormatMainTable.AddCell(new Cell().SetBackgroundColor(new DeviceRgb(220, 220, 220)));

            shippingFormatMainTable.AddCell(new Cell().SetPaddingTop(12).SetBorder(Border.NO_BORDER));

            var shippingFormaFooterTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2 }))
            .SetWidth(PageSize.A4.GetWidth() - 30);

            Paragraph p16 = new Paragraph("NOMBRE Y FIRMA DEL RESPONSABLE")
               .SetFontSize(10)
               .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            shippingFormaFooterTable.AddCell(new Cell()
                .SetMarginBottom(2)
                .Add(p16));
            shippingFormaFooterTable.AddCell(new Cell());

            Paragraph p17 = new Paragraph(new Text("NOMBRE Y FIRMA DE \nQUIEN RECIBE"))
               .SetFontSize(10)
               .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            shippingFormaFooterTable.AddCell(new Cell()
                .SetMarginBottom(2)
                .Add(p17));
            shippingFormaFooterTable.AddCell(new Cell());

            doc.Add(shippingFormatTableHeader);
            doc.Add(shippingFormatTableHeader2);
            doc.Add(shippingFormatMainTable);
            doc.Add(shippingFormaFooterTable);
        }

        public void GenerateDailyExpensesFormat(DateTime shippingDate)
        {
            doc.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));

            string imageFile = $"wwwroot/images/logo.png";
            ImageData data = ImageDataFactory.Create(imageFile);

            Image img = new Image(data);
            img.SetHeight(20);
            img.SetWidth(120);

            Paragraph Img = new Paragraph();
            Img.Add(img);
            Img.SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
            Img.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);

            Paragraph header1 = new Paragraph("REPORTE DIARIO DE COORDINACIÓN DE COMPRAS");
            header1.SetBold();
            header1.SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            var shippingFormatTableHeader = new Table(2);
            shippingFormatTableHeader.SetWidth(PageSize.A4.GetWidth() - 30);
            shippingFormatTableHeader.SetBorder(Border.NO_BORDER);

            shippingFormatTableHeader.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .Add(img));

            shippingFormatTableHeader.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetBold()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .Add(header1));

            shippingFormatTableHeader.AddCell(new Cell().SetPaddingTop(12).SetBorder(Border.NO_BORDER));

            var shippingFormatTableHeader2 = new Table(new float[] { 1, 2 }, true);
            shippingFormatTableHeader2.SetWidth(PageSize.A4.GetWidth() - 30);

            Paragraph p1 = new Paragraph("FECHA")
                .SetFontSize(10)
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            shippingFormatTableHeader2.AddCell(new Cell().Add(p1));
            shippingFormatTableHeader2.AddCell(new Cell().Add(new Paragraph(shippingDate.ToString("dd-MM-yyyy"))));

            Paragraph p2 = new Paragraph("RESPONSABLE")
                .SetFontSize(10)
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            shippingFormatTableHeader2.AddCell(new Cell().Add(p2));
            shippingFormatTableHeader2.AddCell(new Cell());

            Paragraph p3 = new Paragraph("SALDO INICIAL")
                .SetFontSize(10)
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            shippingFormatTableHeader2.AddCell(new Cell().Add(p3));
            shippingFormatTableHeader2.AddCell(new Cell());

            Paragraph p4 = new Paragraph("TOTAL DE GASTOS DEL DÍA")
                .SetFontSize(10)
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            shippingFormatTableHeader2.AddCell(new Cell().Add(p4));
            shippingFormatTableHeader2.AddCell(new Cell());

            Paragraph p5 = new Paragraph("SALDO FINAL")
                .SetFontSize(10)
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            shippingFormatTableHeader2.AddCell(new Cell().Add(p5));
            shippingFormatTableHeader2.AddCell(new Cell());

            shippingFormatTableHeader2.AddCell(new Cell(1, 2).SetPaddingTop(12).SetBorder(Border.NO_BORDER));

            var shippingFormatMainTable = new Table(3)
            .SetWidth(PageSize.A4.GetWidth() - 30);

            Paragraph p6 = new Paragraph("GASTOS DE OPERACIÓN")
                .SetFontSize(10)
                .SetBold()
                .SetFontColor(new DeviceRgb(255, 255, 255))
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

            shippingFormatMainTable.AddCell(new Cell(1, 3)
                .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                .Add(p6));

            Paragraph p7 = new Paragraph("CONCEPTO")
                .SetFontSize(10)
                .SetBold()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

            shippingFormatMainTable.AddCell(new Cell()
                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                .Add(p7));

            Paragraph p8 = new Paragraph("DESCRIPCIÓN Y COMENTARIOS")
                .SetFontSize(10)
                .SetBold()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

            shippingFormatMainTable.AddCell(new Cell()
                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                .Add(p8));

            Paragraph p9 = new Paragraph("IMPORTE TOTAL")
                .SetFontSize(10)
                .SetBold()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

            shippingFormatMainTable.AddCell(new Cell()
                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                .Add(p9));

            for (int i = 0; i < 30; i++)
            {
                shippingFormatMainTable.AddCell(new Cell().SetPaddingTop(12));
            }

            Paragraph p10 = new Paragraph("TOTAL")
                .SetFontSize(10)
                .SetBold()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

            shippingFormatMainTable.AddCell(new Cell(1, 2)
                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                .Add(p10));

            shippingFormatMainTable.AddCell(new Cell()
                .SetBackgroundColor(new DeviceRgb(220, 220, 220)));

            shippingFormatMainTable.AddCell(new Cell().SetPaddingTop(12).SetBorder(Border.NO_BORDER));

            var shippingTotalsFooterTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1, 1, 1, 1 }))
            .SetWidth(PageSize.A4.GetWidth() - 30);

            shippingTotalsFooterTable.AddCell(new Cell()
                .SetBold()
                .SetFontSize(8)
                .Add(new Paragraph("Comprador")));

            shippingTotalsFooterTable.AddCell(new Cell()
                .SetBold()
                .SetFontSize(8)
                .Add(new Paragraph("Productos")));

            shippingTotalsFooterTable.AddCell(new Cell()
                .SetBold()
                .SetFontSize(8)
                .Add(new Paragraph("Presupuesto en efectivo asignado")));

            shippingTotalsFooterTable.AddCell(new Cell()
                .SetBold()
                .SetFontSize(8)
                .Add(new Paragraph("Total gastado y comprobado con tickets")));

            shippingTotalsFooterTable.AddCell(new Cell()
                .SetBold()
                .SetFontSize(8)
                .Add(new Paragraph("Total de saldo en efectivo a entregar")));

            var productsCount = 0;
            var totalBudget = (decimal)0.00;
            foreach (var buyerTotals in buyersTotals)
            {
                try
                {
                    productsCount += Int32.Parse(buyerTotals.Products);
                    totalBudget += decimal.Parse(buyerTotals.Budget.Replace("$", ""));
                }
                catch (Exception err)
                {
                    Debugger.Break();
                }

                shippingTotalsFooterTable.AddCell(new Cell()
                    .SetPadding(0)
                    .SetMargin(0)
                    .SetPaddingLeft(5)
                    .SetFontSize(8)
                    .Add(new Paragraph(buyerTotals.Name)
                        .SetFixedLeading(10)));

                shippingTotalsFooterTable.AddCell(new Cell()
                    .SetPadding(0)
                    .SetMargin(0)
                    .SetPaddingRight(5)
                    .SetFontSize(8)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .Add(new Paragraph(buyerTotals.Products)));

                shippingTotalsFooterTable.AddCell(new Cell()
                    .SetPadding(0)
                    .SetMargin(0)
                    .SetPaddingRight(5)
                    .SetFontSize(8)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .Add(new Paragraph(buyerTotals.Budget)));

                shippingTotalsFooterTable.AddCell(new Cell()
                    .SetPadding(0)
                    .SetMargin(0)
                    .SetHeight(5)
                    .SetFontSize(8)
                    .Add(new Paragraph()));

                shippingTotalsFooterTable.AddCell(new Cell()
                    .SetPadding(0)
                    .SetMargin(0)
                    .SetHeight(5)
                    .SetFontSize(8)
                    .Add(new Paragraph()));
            }

            shippingTotalsFooterTable.AddCell(new Cell()
                .SetBold()
                .SetFontSize(8)
                .Add(new Paragraph("Total")));

            shippingTotalsFooterTable.AddCell(new Cell()
                .SetPadding(0)
                .SetMargin(0)
                .SetPaddingRight(5)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetFontSize(8)
                .Add(new Paragraph(productsCount.ToString())));

            shippingTotalsFooterTable.AddCell(new Cell()
                .SetPadding(0)
                .SetMargin(0)
                .SetPaddingRight(5)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetFontSize(8)
                .Add(new Paragraph(totalBudget.ToString("C"))));

            shippingTotalsFooterTable.AddCell(new Cell()
                .SetPadding(0)
                .SetMargin(0)
                .SetHeight(5)
                .SetFontSize(8)
                .Add(new Paragraph()));

            shippingTotalsFooterTable.AddCell(new Cell()
                .SetPadding(0)
                .SetMargin(0)
                .SetHeight(5)
                .SetFontSize(8)
                .Add(new Paragraph()));

            shippingTotalsFooterTable.AddCell(new Cell().SetPaddingTop(12).SetBorder(Border.NO_BORDER));

            var shippingFormaFooterTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2 }))
            .SetWidth(PageSize.A4.GetWidth() - 30);

            Paragraph p11 = new Paragraph("NOMBRE Y FIRMA DEL RESPONSABLE")
               .SetFontSize(10)
               .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            shippingFormaFooterTable.AddCell(new Cell()
                .SetMarginBottom(2)
                .Add(p11));
            shippingFormaFooterTable.AddCell(new Cell());

            Paragraph p12 = new Paragraph(new Text("NOMBRE Y FIRMA DE \nQUIEN RECIBE"))
               .SetFontSize(10)
               .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            shippingFormaFooterTable.AddCell(new Cell()
                .SetMarginBottom(2)
                .Add(p12));
            shippingFormaFooterTable.AddCell(new Cell());

            doc.Add(shippingFormatTableHeader);
            doc.Add(shippingFormatTableHeader2);
            doc.Add(shippingFormatMainTable);
            doc.Add(shippingTotalsFooterTable);
            doc.Add(shippingFormaFooterTable);
        }

        [HttpGet]
        [Route("[controller]/[action]")]
        [AllowAnonymous]
        public IActionResult CheckTimes(string date)
        {
            if (string.IsNullOrWhiteSpace(date)) return BadRequest();

            int currentCustomerId = _workContext.CurrentCustomer.Id;

            DateTime selectedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var ordersQuery = _orderService.GetAllOrdersQuery()
                .Where(x => DbFunctions.TruncateTime(x.SelectedShippingDate) == selectedDate)
                .Where(x => x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10))
                .GroupBy(x => x.CustomerId)
                .Select(x => x.FirstOrDefault());

            var ordersTime1Query = ordersQuery.Where(x => x.SelectedShippingTime == "1:00 PM - 3:00 PM");
            int ordersTime1 = ordersTime1Query.Where(x => x.CustomerId == currentCustomerId).Any() ? 0 : ordersTime1Query.Count();

            var ordersTime2Query = ordersQuery.Where(x => x.SelectedShippingTime == "3:00 PM - 5:00 PM");
            int ordersTime2 = ordersTime2Query.Where(x => x.CustomerId == currentCustomerId).Any() ? 0 : ordersTime2Query.Count();

            var ordersTime3Query = ordersQuery.Where(x => x.SelectedShippingTime == "5:00 PM - 7:00 PM");
            int ordersTime3 = ordersTime3Query.Where(x => x.CustomerId == currentCustomerId).Any() ? 0 : ordersTime3Query.Count();

            var ordersTime4Query = ordersQuery.Where(x => x.SelectedShippingTime == "7:00 PM - 9:00 PM");
            int ordersTime4 = ordersTime4Query.Where(x => x.CustomerId == currentCustomerId).Any() ? 0 : ordersTime4Query.Count();

            return Ok(new List<int>() { ordersTime1, ordersTime2, ordersTime3, ordersTime4, ordersTime1 + ordersTime2 + ordersTime3 + ordersTime4 });
        }

        [HttpGet]
        public IActionResult MonitorDateList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Order/MonitorDateList.cshtml");
        }

        [HttpGet]
        public IActionResult CheckTimes2(string date)
        {
            if (string.IsNullOrWhiteSpace(date)) return BadRequest();

            DateTime selectedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;
            var model = OrderUtils.GetTimesPedidosData(selectedDate, _settingService, _orderService, _shippingRegionZoneService, _shippingZoneService, _corcelCustomerService);

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Order/Monitor.cshtml", model);
        }

        [HttpPost]
        public IActionResult MonitorDateListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var query = OrderUtils.GetFilteredOrders(_orderService).GroupBy(x => x.SelectedShippingDate.Value).Select(x => x.Key).OrderByDescending(x => x);
            var pagedList = new PagedList<DateTime>(query, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    Date = x.ToString("dddd, dd 'de' MMMM 'de' yyyy", new CultureInfo("es-MX")),
                    DateShort = x.ToString("dd-MM-yyyy")
                }).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        [HttpGet]
        [Route("[controller]/[action]")]
        public IActionResult GetWebTimes(string date)
        {
            Customer currentCustomer = _workContext.CurrentCustomer;
            string postalCode = currentCustomer.ShippingAddress.ZipPostalCode;

            var regionZonesQuery = _shippingRegionZoneService.GetAll()
                .Where(x => x.Zone.PostalCodes.Contains(postalCode) || x.Zone.AdditionalPostalCodes.Contains(postalCode));
            if (!regionZonesQuery.Any()) return NotFound();
            ShippingRegion region = regionZonesQuery.FirstOrDefault().Region;
            List<string> postalCodeList = _shippingRegionZoneService.GetAll()
                .Where(x => x.RegionId == region.Id)
                .Select(x => x.Zone.PostalCodes + "," + x.Zone.AdditionalPostalCodes)
                .ToList();
            string[] postalCodes = string.Join(",", postalCodeList).Split(',').Select(x => x.Trim()).ToArray();

            DateTime parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;
            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate == parsedDate);
            ordersQuery = OrderUtils.GetPedidosOnly(ordersQuery);

            var allOrdersScheduleQuery = ordersQuery
                .Select(x => new { x.SelectedShippingTime, x.CustomerId })
                .ToList();
            var regionOrdersScheduleQuery = ordersQuery.Where(x => postalCodes.Contains(x.ShippingAddress.ZipPostalCode))
                .Select(x => new { x.SelectedShippingTime, x.CustomerId })
                .ToList();

            var globalSchedule = _settingService.LoadSetting<ScheduleSettings>();

            var disabledDate = OrderUtils.DisabledDates.Contains(parsedDate);

            string defaultOptionValue = "<option disabled value=\"0\">Selecciona el horario de entrega...</option>";

            // "1:00 PM - 3:00 PM"
            var shouldBlockTime1 = allOrdersScheduleQuery.Where(x => x.CustomerId == currentCustomer.Id && x.SelectedShippingTime != "1:00 PM - 3:00 PM").Any();
            var ordersTime1Query = allOrdersScheduleQuery.Where(x => x.SelectedShippingTime == "1:00 PM - 3:00 PM");
            var ordersTime1RegionQuery = regionOrdersScheduleQuery.Where(x => x.SelectedShippingTime == "1:00 PM - 3:00 PM");

            int ordersTime1 = ordersTime1Query.Where(x => x.CustomerId == currentCustomer.Id).Any() ? 0 : ordersTime1Query.Count();
            int ordersTime1ByRegion = ordersTime1RegionQuery.Where(x => x.CustomerId == currentCustomer.Id).Any() ? 0 : ordersTime1RegionQuery.Count();

            bool optionTime1Disabled = (region.Schedule1Quantity <= ordersTime1ByRegion) || (globalSchedule.Schedule1Quantity <= ordersTime1) || disabledDate || shouldBlockTime1;
            string optionTime1 = $"<option {(optionTime1Disabled ? "disabled" : "")} value=\"1:00 PM - 3:00 PM\">1:00 PM - 3:00 PM{(optionTime1Disabled ? " (HORARIO LLENO)" : "")}</option>";

            // "3:00 PM - 5:00 PM"
            var shouldBlockTime2 = allOrdersScheduleQuery.Where(x => x.CustomerId == currentCustomer.Id && x.SelectedShippingTime != "3:00 PM - 5:00 PM").Any();
            var ordersTime2Query = allOrdersScheduleQuery.Where(x => x.SelectedShippingTime == "3:00 PM - 5:00 PM");
            var ordersTime2RegionQuery = regionOrdersScheduleQuery.Where(x => x.SelectedShippingTime == "3:00 PM - 5:00 PM");

            int ordersTime2 = ordersTime2Query.Where(x => x.CustomerId == currentCustomer.Id).Any() ? 0 : ordersTime2Query.Count();
            int ordersTime2ByRegion = ordersTime2RegionQuery.Where(x => x.CustomerId == currentCustomer.Id).Any() ? 0 : ordersTime2RegionQuery.Count();

            bool optionTime2Disabled = (region.Schedule2Quantity <= ordersTime2ByRegion) || (globalSchedule.Schedule2Quantity <= ordersTime2) || disabledDate || shouldBlockTime2;
            string optionTime2 = $"<option {(optionTime2Disabled ? "disabled" : "")} value=\"3:00 PM - 5:00 PM\">3:00 PM - 5:00 PM{(optionTime2Disabled ? " (HORARIO LLENO)" : "")}</option>";

            bool isSpecialDate = parsedDate.Month == 12 && (parsedDate.Day == 24 || parsedDate.Day == 31);
            // "5:00 PM - 7:00 PM"
            var shouldBlockTime3 = allOrdersScheduleQuery.Where(x => x.CustomerId == currentCustomer.Id && x.SelectedShippingTime != "5:00 PM - 7:00 PM").Any();
            var ordersTime3Query = allOrdersScheduleQuery.Where(x => x.SelectedShippingTime == "5:00 PM - 7:00 PM");
            var ordersTime3RegionQuery = regionOrdersScheduleQuery.Where(x => x.SelectedShippingTime == "5:00 PM - 7:00 PM");

            int ordersTime3 = ordersTime3Query.Where(x => x.CustomerId == currentCustomer.Id).Any() ? 0 : ordersTime3Query.Count();
            int ordersTime3ByRegion = ordersTime3RegionQuery.Where(x => x.CustomerId == currentCustomer.Id).Any() ? 0 : ordersTime3RegionQuery.Count();

            bool optionTime3Disabled = (region.Schedule3Quantity <= ordersTime3ByRegion) || (globalSchedule.Schedule3Quantity <= ordersTime3) || disabledDate || shouldBlockTime3;
            string optionTime3 = $"<option {(optionTime3Disabled || isSpecialDate ? "disabled" : "")} value=\"5:00 PM - 7:00 PM\">5:00 PM - 7:00 PM{(optionTime3Disabled ? " (HORARIO LLENO)" : "")}</option>";

            // "7:00 PM - 9:00 PM"
            var shouldBlockTime4 = allOrdersScheduleQuery.Where(x => x.CustomerId == currentCustomer.Id && x.SelectedShippingTime != "7:00 PM - 9:00 PM").Any();
            var ordersTime4Query = allOrdersScheduleQuery.Where(x => x.SelectedShippingTime == "7:00 PM - 9:00 PM");
            var ordersTime4RegionQuery = regionOrdersScheduleQuery.Where(x => x.SelectedShippingTime == "7:00 PM - 9:00 PM");

            int ordersTime4 = ordersTime4Query.Where(x => x.CustomerId == currentCustomer.Id).Any() ? 0 : ordersTime4Query.Count();
            int ordersTime4ByRegion = ordersTime4RegionQuery.Where(x => x.CustomerId == currentCustomer.Id).Any() ? 0 : ordersTime4RegionQuery.Count();

            bool optionTime4Disabled = (region.Schedule4Quantity <= ordersTime4ByRegion) || (globalSchedule.Schedule4Quantity <= ordersTime4) || disabledDate || shouldBlockTime4;
            string optionTime4 = $"<option {(optionTime4Disabled || isSpecialDate ? "disabled" : "")} value=\"7:00 PM - 9:00 PM\">7:00 PM - 9:00 PM{(optionTime4Disabled ? " (HORARIO LLENO)" : "")}</option>";

            return Ok(new
            {
                optionValue = defaultOptionValue + optionTime1 + optionTime2 + optionTime3 + optionTime4,
                anyDisabled = optionTime1Disabled || optionTime2Disabled || optionTime3Disabled || optionTime4Disabled
            });
        }

        [HttpGet]
        [Route("[controller]/[action]")]
        public IActionResult GetOrderMinimumPedidoCheck(string date)
        {
            Customer currentCustomer = _workContext.CurrentCustomer;
            DateTime parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;

            return Ok(OrderUtils.GetOrderMinimumPedidoCheckMsg(currentCustomer, parsedDate, _orderService, _orderProcessingService));
        }

        // NOT CONSIDERING REGIONS
        //[HttpGet]
        //[Route("[controller]/[action]")]
        //public IActionResult GetWebTimes(string date)
        //{
        //    Customer currentCustomer = _workContext.CurrentCustomer;
        //    string postalCode = currentCustomer.ShippingAddress.ZipPostalCode;
        //    var postalCodes = _shippingAreaService.GetAll().Select(x => x.PostalCode.Trim()).ToList();

        //    if (!postalCodes.Contains(postalCode)) return NotFound();

        //    DateTime parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;
        //    var ordersQuery = OrderUtils.GetFilteredOrders(_orderService)
        //        .Where(x => x.SelectedShippingDate == parsedDate);
        //    ordersQuery = OrderUtils.GetPedidosOnly(ordersQuery);

        //    var allOrdersScheduleQuery = ordersQuery
        //        .Select(x => new { x.SelectedShippingTime, x.CustomerId })
        //        .ToList();

        //    var globalSchedule = _settingService.LoadSetting<ScheduleSettings>();

        //    var disabledDate = OrderUtils.DisabledDates.Contains(parsedDate);

        //    string defaultOptionValue = "<option disabled value=\"0\">Selecciona el horario de entrega...</option>";

        //    // "1:00 PM - 3:00 PM"
        //    var shouldBlockTime1 = allOrdersScheduleQuery.Where(x => x.CustomerId == currentCustomer.Id && x.SelectedShippingTime != "1:00 PM - 3:00 PM").Any();
        //    var ordersTime1Query = allOrdersScheduleQuery.Where(x => x.SelectedShippingTime == "1:00 PM - 3:00 PM");

        //    int ordersTime1 = ordersTime1Query.Where(x => x.CustomerId == currentCustomer.Id).Any() ? 0 : ordersTime1Query.Count();
        //    bool optionTime1Disabled = (globalSchedule.Schedule1Quantity <= ordersTime1) || disabledDate || shouldBlockTime1;
        //    string optionTime1 = $"<option {(optionTime1Disabled ? "disabled" : "")} value=\"1:00 PM - 3:00 PM\">1:00 PM - 3:00 PM{(optionTime1Disabled ? " (HORARIO LLENO)" : "")}</option>";

        //    // "3:00 PM - 5:00 PM"
        //    var shouldBlockTime2 = allOrdersScheduleQuery.Where(x => x.CustomerId == currentCustomer.Id && x.SelectedShippingTime != "3:00 PM - 5:00 PM").Any();
        //    var ordersTime2Query = allOrdersScheduleQuery.Where(x => x.SelectedShippingTime == "3:00 PM - 5:00 PM");

        //    int ordersTime2 = ordersTime2Query.Where(x => x.CustomerId == currentCustomer.Id).Any() ? 0 : ordersTime2Query.Count();
        //    bool optionTime2Disabled = (globalSchedule.Schedule2Quantity <= ordersTime2) || disabledDate || shouldBlockTime2;
        //    string optionTime2 = $"<option {(optionTime2Disabled ? "disabled" : "")} value=\"3:00 PM - 5:00 PM\">3:00 PM - 5:00 PM{(optionTime2Disabled ? " (HORARIO LLENO)" : "")}</option>";

        //    // "5:00 PM - 7:00 PM"
        //    var shouldBlockTime3 = allOrdersScheduleQuery.Where(x => x.CustomerId == currentCustomer.Id && x.SelectedShippingTime != "5:00 PM - 7:00 PM").Any();
        //    var ordersTime3Query = allOrdersScheduleQuery.Where(x => x.SelectedShippingTime == "5:00 PM - 7:00 PM");

        //    int ordersTime3 = ordersTime3Query.Where(x => x.CustomerId == currentCustomer.Id).Any() ? 0 : ordersTime3Query.Count();
        //    bool optionTime3Disabled = (globalSchedule.Schedule3Quantity <= ordersTime3) || disabledDate || shouldBlockTime3;
        //    string optionTime3 = $"<option {(optionTime3Disabled ? "disabled" : "")} value=\"5:00 PM - 7:00 PM\">5:00 PM - 7:00 PM{(optionTime3Disabled ? " (HORARIO LLENO)" : "")}</option>";

        //    // "7:00 PM - 9:00 PM"
        //    var shouldBlockTime4 = allOrdersScheduleQuery.Where(x => x.CustomerId == currentCustomer.Id && x.SelectedShippingTime != "7:00 PM - 9:00 PM").Any();
        //    var ordersTime4Query = allOrdersScheduleQuery.Where(x => x.SelectedShippingTime == "7:00 PM - 9:00 PM");

        //    int ordersTime4 = ordersTime4Query.Where(x => x.CustomerId == currentCustomer.Id).Any() ? 0 : ordersTime4Query.Count();
        //    bool optionTime4Disabled = (globalSchedule.Schedule4Quantity <= ordersTime4) || disabledDate || shouldBlockTime4;
        //    string optionTime4 = $"<option {(optionTime4Disabled ? "disabled" : "")} value=\"7:00 PM - 9:00 PM\">7:00 PM - 9:00 PM{(optionTime4Disabled ? " (HORARIO LLENO)" : "")}</option>";

        //    return Ok(new
        //    {
        //        optionValue = defaultOptionValue + optionTime1 + optionTime2 + optionTime3 + optionTime4,
        //        anyDisabled = optionTime1Disabled || optionTime2Disabled || optionTime3Disabled || optionTime4Disabled
        //    });
        //}

        private byte[] ExportOrderDataToPdf(IList<Order> orders,
            DateTime selectedDate,
            bool provider,
            bool byBuyer,
            bool forClients,
            int buyerId,
            bool byRoute,
            bool byManufacturer,
            int manufacturerId,
            bool labels,
            bool buyerLabels,
            bool labelsV2,
            bool buyerLabelsV2,
            bool fridge,
            bool bunch,
            int routeId)
        {
            MemoryStream stream = new MemoryStream();
            PdfWriter pdfWriter = new PdfWriter(stream);
            PdfDocument pdfDoc = new PdfDocument(pdfWriter);
            ps = pdfDoc.GetDefaultPageSize();
            doc = new Document(pdfDoc, PageSize.LETTER, false);
            doc.SetMargins(20, 20, 50, 20);

            NewPageHandler newPageHandler = new NewPageHandler();
            pdfDoc.AddEventHandler(PdfDocumentEvent.START_PAGE, newPageHandler);

            List<OrderItem> parsedOrderItems = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService);

            if (byBuyer)
            {
                List<int> orderItemIdsOfTheDay = parsedOrderItems.Select(x => x.Id).ToList();
                var orderItemBuyerQuery = _orderItemBuyerService.GetAll().Where(x => orderItemIdsOfTheDay.Contains(x.OrderItemId));
                bool notContains = orderItemBuyerQuery.Where(x => x.CustomerId == 0).Any();
                bool routeNotAssigned = orders.Where(x => x.RouteId == 0).Any();

                bool buyerAllowed = _buyerListDownloadService.GetAll().Where(x => x.OrderShippingDate == selectedDate).Select(x => x.AllowDownload).FirstOrDefault();

                bool isAdmin = _workContext.CurrentCustomer.IsInCustomerRole("Administrators");
                if (notContains || routeNotAssigned || (!buyerAllowed && !isAdmin))
                {
                    string text = "La descarga del reporte debe estar activa, todos los productos deben ser asignados a un comprador y todas las órdenes a una ruta para poder generar el reporte.";
                    if (_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                    {
                        shoppingListTitle = new Paragraph($"{text} Si deseas asignar los compradores para esta fecha da clic ");
                        var url = new Link("aquí.", PdfAction.CreateURI((Request.IsHttps ? "https" : "http") + $"://{Request.Host}/Admin/OrderItemBuyer/AssignBuyers?date={selectedDate.ToString("dd-MM-yyyy")}"));
                        shoppingListTitle.Add(url);
                    }
                    else
                    {
                        shoppingListTitle = new Paragraph($"El reporte no está listo. Por favor, inténtalo más tarde.");
                    }
                    doc.Add(shoppingListTitle);
                }
                else
                {
                    if (buyerId > 0)
                        orderItemBuyerQuery = orderItemBuyerQuery.Where(x => x.CustomerId == buyerId);
                    var orderItemBuyer = orderItemBuyerQuery.ToList();

                    var buyerIds = orderItemBuyer.GroupBy(x => x.CustomerId).Select(x => x.First()).Select(x => x.CustomerId).ToList();
                    var customers = _customerService.GetAllCustomersQuery().Where(x => buyerIds.Contains(x.Id)).ToList();
                    buyerIds = buyerIds.OrderBy(x => customers.Where(y => y.Id == x).FirstOrDefault()?.GetFullName()).ToList();

                    var manufacturers = _manufacturerService.GetAllManufacturers().ToList();
                    var productMainManufacturers = _productMainManufacturerService.GetAll().ToList();

                    //PrepareAllOrdersAndReports();
                    if (buyerIds.Count() > 0)
                    {
                        for (int i = 0; i < buyerIds.Count(); i++)
                        {
                            Customer customer = customers.Where(y => y.Id == buyerIds[i]).FirstOrDefault();
                            if (customer == null) continue;
                            PrepareShoppingListByBuyerHeader(customers.Where(y => y.Id == buyerIds[i]).FirstOrDefault(), orderItemBuyer, selectedDate, parsedOrderItems, manufacturers, productMainManufacturers);
                            PrepareShoppingListByBuyer(orders, buyerIds[i], orderItemBuyer, parsedOrderItems);
                            doc.Add(shoppingListHeaderTable);
                            doc.Add(budgetBuyerTable);
                            doc.Add(shoppingListDetailsTitle);
                            doc.Add(shoppingListDetailsTable);
                            doc.Add(shoppingListNotesTable);
                            doc.Add(shoppingListProductTable);
                            PrepareShoppingListByBuyer(orders, buyerIds[i], orderItemBuyer, parsedOrderItems);
                            if (i < buyerIds.Count() - 1) doc.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));
                        }
                        newPageHandler.WriteTotal(pdfDoc);
                        pdfDoc.RemoveEventHandler(PdfDocumentEvent.START_PAGE, newPageHandler);

                        if (buyerId == 0)
                            GenerateDailyExpensesFormat(orders.FirstOrDefault().SelectedShippingDate.Value);

                        doc.Flush();
                    }
                    else
                    {
                        doc.Add(new Paragraph("El comprador seleccionado no tiene órdenes asignadas."));
                    }
                }

                doc.Close();
            }
            else if (byRoute)
            {
                bool notContains = orders.Where(x => x.RouteId == 0).Any();
                if (notContains)
                {
                    string text = "Todas las órdenes deben tener asignada una ruta para poder generar el reporte.";
                    shoppingListTitle = new Paragraph($"{text} Si deseas asignar las rutas para esta fecha da clic ");
                    var url = new Link("aquí.", PdfAction.CreateURI((Request.IsHttps ? "https" : "http") + $"://{Request.Host}/Admin/ShippingRoute/AssignRouteOrder?date={selectedDate.ToString("dd-MM-yyyy")}"));
                    shoppingListTitle.Add(url);
                    doc.Add(shoppingListTitle);
                }
                else
                {
                    var routeIds = orders.Select(x => x.RouteId).GroupBy(x => x).OrderBy(x =>
                    _shippingRouteService.GetAll().Where(y => y.Id == x.Key).FirstOrDefault().RouteName).Select(x => x.Key).ToList();
                    //PrepareAllOrdersAndReports();
                    for (int i = 0; i < routeIds.Count(); i++)
                    {
                        PrepareShoppingListByRouteHeader(routeIds[i], orders);
                        //PrepareShoppingListByRoute(orders, routeIds[i]);
                        doc.Add(shoppingListHeaderTable);

                        PrepareShoppingListExtraByRoute(routeIds[i], orders);
                        doc.Add(shoppingListExtraTable);

                        //doc.Add(shoppingListDetailsTitle);
                        //doc.Add(secondaryShoppingListDetailsTable);
                        //doc.Add(shoppingListDetailsTable);
                        //doc.Add(shoppingListProductTable);
                        if (i < routeIds.Count() - 1) doc.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));
                    }

                    newPageHandler.WriteTotal(pdfDoc);
                    pdfDoc.RemoveEventHandler(PdfDocumentEvent.START_PAGE, newPageHandler);
                    DailyRouteFormatReport(orders);
                    doc.Flush();
                }

                doc.Close();
            }
            else if (byManufacturer)
            {
                var allowDownload = _manufacturerListDownloadService.GetAll()
                    .Where(x => x.OrderShippingDate == selectedDate)
                    .Select(x => x.AllowDownload)
                    .FirstOrDefault();

                if (!allowDownload)
                {
                    shoppingListTitle = new Paragraph("La descarga del reporte aún no está disponible. Por favor, inténtalo de nuevo más tarde.");
                    doc.Add(shoppingListTitle);
                }
                else
                {
                    var mainManufacturers = _productMainManufacturerService.GetAll().ToList();
                    var manufacturerOrderItems = parsedOrderItems
                        .Select(x => new ManufacturerOrderItemModel()
                        {
                            OrderItem = x,
                            MainManufacturer = ProductUtils.GetMainManufacturer(x.Product.ProductManufacturers, mainManufacturers.Where(y => x.ProductId == y.ProductId).FirstOrDefault())
                        })
                        .Where(x => x.MainManufacturer != null && x.MainManufacturer.IsIncludeInReportByManufacturer)
                        .ToList();
                    if (manufacturerId > 0)
                    {
                        manufacturerOrderItems = manufacturerOrderItems
                           .Where(x => x.MainManufacturer.Id == manufacturerId)
                           .ToList();
                    }

                    var manufacturers = manufacturerOrderItems.Select(x => x.MainManufacturer).Distinct().OrderBy(x => x.Name).ToList();
                    for (int i = 0; i < manufacturers.Count; i++)
                    {
                        PrepareShoppingListByManufacturerHeader(manufacturers[i]);
                        var currentManufacturerItems = manufacturerOrderItems
                            .Where(x => x.MainManufacturer.Id == manufacturers[i].Id)
                            .Select(x => x.OrderItem)
                            .ToList();
                        var itemOrders = currentManufacturerItems
                            .Select(x => x.Order)
                            .Distinct()
                            .ToList();
                        PrepareShoppingListNotes(itemOrders);
                        PrepareShoppingListByManufacturer(currentManufacturerItems);
                        doc.Add(shoppingListHeaderTable);
                        doc.Add(shoppingListNotesTable);
                        doc.Add(shoppingListProductTable);

                        if (i < manufacturers.Count() - 1) doc.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));
                    }

                    if (manufacturers.Count == 0)
                        doc.Add(new Paragraph("No se encontraron productos para la fecha y/o fabricante seleccionado."));

                    newPageHandler.WriteTotal(pdfDoc);
                    pdfDoc.RemoveEventHandler(PdfDocumentEvent.START_PAGE, newPageHandler);

                    doc.Flush();
                }


                doc.Close();
            }
            else if (forClients)
            {
                pdfDoc.RemoveEventHandler(PdfDocumentEvent.START_PAGE, newPageHandler);

                if (routeId > 0)
                    orders = orders.Where(x => x.RouteId == routeId).ToList();
                List<Order> orderedOrders = orders.OrderBy(x => x.RouteId).ThenBy(x => x.RouteDisplayOrder).ToList();
                int count = orderedOrders.Count();
                for (int i = 0; i < count; i++)
                {
                    for (int e = 0; e < 3; e++)
                    {
                        PrepareOrderDetails(orderedOrders[i], e);
                        Table detailsTable = new Table(1)
                        .SetWidth(PageSize.A4.GetWidth())
                        .SetBorder(Border.NO_BORDER);

                        detailsTable.AddCell(new Cell().Add(orderDetailsHeaderTable).SetBorder(Border.NO_BORDER));
                        detailsTable.AddCell(new Cell().Add(orderDeliveredOrNot).SetBorder(Border.NO_BORDER));
                        detailsTable.AddCell(new Cell().Add(orderDetailsTitle).SetBorder(Border.NO_BORDER));
                        detailsTable.AddCell(new Cell().Add(orderDetailsProductTable).SetBorder(Border.NO_BORDER));
                        detailsTable.AddCell(new Cell().Add(orderDetailsFooterTable).SetBorder(Border.NO_BORDER));

                        doc.Add(detailsTable);

                        if (i + 1 < orderedOrders.Count || e != 2)
                        {
                            doc.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));
                        }
                    }
                    doc.Flush();
                }
                //doc.Flush();
                doc.Close();
            }
            else if (labels || labelsV2)
            {
                bool newVersion = labelsV2;
                doc.SetMargins(newVersion ? 5.6f : 5f, 0f, 0f, 3f);

                pdfDoc.RemoveEventHandler(PdfDocumentEvent.START_PAGE, newPageHandler);

                List<int> orderItemIdsOfTheDay = parsedOrderItems
                    .Where(x => x.Product.ProductSpecificationAttributes.Where(y => y.SpecificationAttributeOption.Name.ToLower().Contains("etiqueta 1/30 (2.625 x 1 in)")).Any())
                    .Select(x => x.Id)
                    .ToList();
                var orderItemBuyerQuery = _orderItemBuyerService.GetAll().Where(x => orderItemIdsOfTheDay.Contains(x.OrderItemId)).ToList();
                var buyerIds = orderItemBuyerQuery.GroupBy(x => x.CustomerId).Select(x => x.First()).Select(x => x.CustomerId)
                    .OrderBy(x => _customerService.GetCustomerById(x).GetFullName()).ToList();
                var routes = _shippingRouteService.GetAll().ToList();

                if (buyerIds.Count() > 0)
                {
                    for (int e = 0; e < buyerIds.Count; e++)
                    {
                        var orderItemIds = orderItemBuyerQuery.Where(x => x.CustomerId == buyerIds[e]).Select(x => x.OrderItemId);
                        var currentOrderItems = parsedOrderItems
                            .Where(x => orderItemIds.Contains(x.Id))
                            .OrderBy(x => x.Product.ProductManufacturers.OrderBy(y => y.Manufacturer?.DisplayOrder).Select(y => y.Manufacturer).FirstOrDefault()?.Name)
                            .ThenBy(x => x.Product?.Name)
                            .ThenBy(x => routes.Where(y => y.Id == x.Order?.RouteId).FirstOrDefault()?.RouteName)
                            .ThenBy(x => x.OrderId).ToList();
                        for (int i = 0; i < Math.Ceiling((decimal)currentOrderItems.Count / (newVersion ? 80 : 30)); i++)
                        {
                            var itemsPaged = new PagedList<OrderItem>(currentOrderItems, i, (newVersion ? 80 : 30));
                            PrepareShoppingListLabels(itemsPaged, newVersion);
                            doc.Add(new Paragraph(buyerIds[e] == 0 ? "Sin comprador" : _customerService.GetCustomerById(buyerIds[e]).GetFullName() +
                                " - Fecha de entrega: " + parsedDate.ToString("dd-MM-yyyy"))
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .SetMarginBottom(13));
                            doc.Add(shoppingListProductTable);
                            if (itemsPaged.Count == (newVersion ? 80 : 30)) doc.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));
                        }
                        if (e + 1 < buyerIds.Count)
                        {
                            doc.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));
                        }
                    }

                    doc.Flush();
                    doc.Close();
                }
                else
                {
                    doc.Add(new Paragraph("No hay compradores o productos con etiqueta para este día").SetFontSize(8).SetMarginBottom(13));
                    doc.Flush();
                    doc.Close();
                }
            }
            else if (buyerLabels || buyerLabelsV2)
            {
                bool newVersion = buyerLabelsV2;
                doc.SetMargins(newVersion ? 34.6f : 34f, 0f, 0f, 3f);

                pdfDoc.RemoveEventHandler(PdfDocumentEvent.START_PAGE, newPageHandler);

                List<int> orderItemIdsOfTheDay = parsedOrderItems.Select(x => x.Id).ToList();
                var orderItemBuyerQuery = _orderItemBuyerService.GetAll().Where(x => orderItemIdsOfTheDay.Contains(x.OrderItemId));
                bool notContains = orderItemBuyerQuery.Where(x => x.CustomerId == 0).Any();
                bool routeNotAssigned = orders.Where(x => x.RouteId == 0).Any();

                var manufacturers = _manufacturerService.GetAllManufacturers().ToList();
                var productMainManufacturers = _productMainManufacturerService.GetAll().ToList();

                if (notContains || routeNotAssigned)
                {
                    string text = "Todos los productos deben ser asignados a un comprador para poder generar las etiquetas.";
                    shoppingListTitle = new Paragraph($"{text} Si deseas asignar los compradores para esta fecha da clic ");
                    var url = new Link("aquí.", PdfAction.CreateURI((Request.IsHttps ? "https" : "http") + $"://{Request.Host}/Admin/OrderItemBuyer/AssignBuyers?date={selectedDate.ToString("dd-MM-yyyy")}"));
                    shoppingListTitle.Add(url);
                    doc.Add(shoppingListTitle);
                }
                else
                {
                    shoppingListProductTable = newVersion ? new Table(new float[] { 1, 1, 1, 1 }, true) : new Table(new float[] { 1, 1, 1 }, true);
                    shoppingListProductTable.SetBorderCollapse(BorderCollapsePropertyValue.SEPARATE);
                    shoppingListProductTable.SetHorizontalBorderSpacing(10);
                    shoppingListProductTable.SetWidth(PageSize.A4.GetWidth() + 12);

                    List<IGrouping<int, OrderItemBuyer>> group = orderItemBuyerQuery.GroupBy(x => x.CustomerId).ToList();
                    var count = 0;
                    for (int e = 0; e < group.Count; e++)
                    {
                        count++;
                        PrepareBuyerLabels(group[e].Select(x => x).ToList(), selectedDate, parsedOrderItems, manufacturers, productMainManufacturers, false, newVersion);
                        if ((e + 1) % (newVersion ? 80 : 30) == 0) doc.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));
                    }

                    // We add the total label
                    PrepareBuyerLabels(group.SelectMany(x => x).ToList(), selectedDate, parsedOrderItems, manufacturers, productMainManufacturers, true, newVersion);
                    if ((group.Count() + 1) % (newVersion ? 80 : 30) == 0) doc.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));

                    var pendingCells = (shoppingListProductTable.GetNumberOfRows() * shoppingListProductTable.GetNumberOfColumns()) - count;
                    if (pendingCells > 0)
                    {
                        for (int i = 0; i < pendingCells; i++)
                        {
                            shoppingListProductTable.AddCell(new Cell().SetBorder(Border.NO_BORDER));
                        }
                    }
                    doc.Add(shoppingListProductTable);
                }

                doc.Flush();
                doc.Close();
            }
            else if (fridge)
            {
                var fridgeItems = parsedOrderItems
                    .Where(x => x.Product.ProductSpecificationAttributes.Where(y => y.SpecificationAttributeOption.Name.ToLower().Contains("nevera")).Any())
                    .ToList();

                if (fridgeItems.Count == 0)
                {
                    doc.Add(new Paragraph("No se encontraron productos para la fecha seleccionada."));
                }
                else
                {
                    PrepareSimpleShoppingListHeader("Armado de Nevera");
                    PrepareShoppingListWithAttributes(fridgeItems);
                }

                newPageHandler.WriteTotal(pdfDoc);
                pdfDoc.RemoveEventHandler(PdfDocumentEvent.START_PAGE, newPageHandler);

                doc.Flush();
                doc.Close();
            }
            else if (bunch)
            {
                var bunchItems = parsedOrderItems
                    .Where(x => x.Product.ProductSpecificationAttributes.Where(y => y.SpecificationAttributeOption.Name.ToLower().Contains("manojo")).Any())
                    .ToList();

                if (bunchItems.Count == 0)
                {
                    doc.Add(new Paragraph("No se encontraron productos para la fecha seleccionada."));
                }
                else
                {
                    PrepareSimpleShoppingListHeader("Armado de Manojos");
                    PrepareShoppingListWithAttributes(bunchItems);
                }

                newPageHandler.WriteTotal(pdfDoc);
                pdfDoc.RemoveEventHandler(PdfDocumentEvent.START_PAGE, newPageHandler);

                doc.Flush();
                doc.Close();
            }
            else
            {
                //PrepareAllOrdersAndReports();
                PrepareMainHeader(orders, provider, selectedDate);
                PrepareShoppingList(parsedOrderItems, provider, generalList: true);

                doc.Add(shoppingListHeaderTable);
                if (!provider)
                {
                    // PRODUCTS QTY
                    PrepareShoppingListDetails(orders);
                    doc.Add(shoppingListDetailsTitle);
                    doc.Add(shoppingListDetailsTable);
                    doc.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));
                    // PRODUCTS QTY
                }
                doc.Add(shoppingListTitle);
                doc.Add(shoppingListProductTable);

                newPageHandler.WriteTotal(pdfDoc);
                pdfDoc.RemoveEventHandler(PdfDocumentEvent.START_PAGE, newPageHandler);
                doc.Flush();
                doc.Close();
            }

            return stream.ToArray();
        }

        [HttpGet]
        public virtual IActionResult ExportPylData(string selectedDate)
        {
            parsedDate = DateTime.ParseExact(selectedDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .ToList()
                .Where(x => x.SelectedShippingDate.HasValue && x.SelectedShippingDate.Value.Date == parsedDate.Date)
                .ToList();
            var parsedOrderItems = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService);
            var products = parsedOrderItems.Select(x => x.Product).OrderBy(x => x.Name).Distinct().ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add(parsedDate.ToString("dd-MM-yyyy"));
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Producto";
                    worksheet.Cells[row, 2].Value = "Categoría padre";
                    worksheet.Cells[row, 3].Value = "Categoría hijo";
                    worksheet.Cells[row, 4].Value = "Cantidad vendida";
                    worksheet.Cells[row, 5].Value = "Costo de lo vendido";
                    worksheet.Cells[row, 6].Value = "Ventas";
                    worksheet.Cells[row, 7].Value = "Utilidad en dinero";
                    worksheet.Cells[row, 8].Value = "Utilidad en porcentaje";

                    foreach (var product in products)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = product.Name;

                        var productCategories = product.ProductCategories.Select(x => x.Category);
                        var parentCategories = productCategories.Where(x => x.ParentCategoryId == 0).ToList();
                        var childCategories = productCategories.Where(x => x.ParentCategoryId > 0).ToList();
                        worksheet.Cells[row, 2].Value =
                            parentCategories.Any() ? string.Join(", ", parentCategories.Select(x => x.Name)) : "Producto sin categoría padre";
                        worksheet.Cells[row, 3].Value =
                            childCategories.Any() ? string.Join(", ", childCategories.Select(x => x.Name)) : "Producto sin categoría hijo";

                        var productCount =
                            GetTotalQuantity(product, parsedOrderItems.Where(x => x.ProductId == product.Id).Select(x => x.Quantity).DefaultIfEmpty().Sum()).ToString() +
                            (product.EquivalenceCoefficient > 0 || product.WeightInterval > 0 ? "kg" : "pz");
                        worksheet.Cells[row, 4].Value = productCount;

                        var productCost = _orderReportService.GetAll()
                            .Where(x => x.OrderShippingDate == parsedDate.Date && x.ProductId == product.Id).FirstOrDefault();
                        if (productCost != null)
                            worksheet.Cells[row, 5].Value = Math.Round(productCost.RequestedQtyCost, 4);
                        else
                            worksheet.Cells[row, 5].Value = "N/A";

                        var productPrice = parsedOrderItems.Where(x => x.ProductId == product.Id).Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 6].Value = productPrice.ToString("C");

                        if (productCost != null)
                        {
                            var moneyUtility = productPrice - productCost.RequestedQtyCost;
                            worksheet.Cells[row, 7].Value = moneyUtility;
                            worksheet.Cells[row, 8].Value = Math.Round(moneyUtility / productPrice, 4);
                        }
                        else
                        {
                            worksheet.Cells[row, 7].Value = "N/A";
                            worksheet.Cells[row, 8].Value = "N/A";
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                shippingDataStream = stream.ToArray();
            }

            return File(shippingDataStream, MimeTypes.TextXlsx, $"pyl_{selectedDate}.xlsx");
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

        //private void PrepareAllOrdersAndReports()
        //{
        //    allOrderItems = _orderService.GetAllOrdersQuery()
        //        .Where(x => x.OrderStatusId != 40)
        //        .Select(x => x.OrderItems)
        //        .SelectMany(x => x)
        //        .Select(x => new AllOrderItemsModel()
        //        {
        //            OrderItemId = x.Id,
        //            ProductId = x.ProductId
        //        }).ToList();
        //    allReports = _orderReportService.GetAll()
        //        .Where(x => x.UnitCost > 0)
        //        .Select(x => new AllReportsModel()
        //        {
        //            CreatedOnUtc = x.CreatedOnUtc,
        //            OrderItemId = x.OrderItemId,
        //            UnitCost = x.UnitCost
        //        }).ToList();
        //}

        public string GetLetter(int count)
        {
            var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (count < letters.Length)
            {
                try
                {
                    return (letters[count].ToString());
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                var final = "";
                List<string> datalist = new List<string>();
                datalist.AddRange(count.ToString().Select(c => c.ToString()));
                foreach (var num in datalist)
                {
                    final += letters[Int32.Parse(num)].ToString();
                }
                return (final);
            }
        }

        private void PrepareOrderDetails(Order order, int groupId)
        {
            var routes = _shippingRouteService.GetAll().ToList();
            bool clientCopy = groupId == 1;
            bool buildCopy = groupId == 2;
            var parsedOrderItems = order.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService);

            string imageFile = $"wwwroot/images/logo.png";
            ImageData data = ImageDataFactory.Create(imageFile);

            Image img = new Image(data);
            img.SetHeight(20);
            img.SetWidth(120);
            img.SetMarginRight(10);

            Paragraph Img = new Paragraph();
            Img.Add(img);
            Img.SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
            Img.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);


            if (lastRouteId != order.RouteId)
            {
                lastRouteId = order.RouteId;
                usersInfo.Clear();
                orderCount = 0;
            }

            if (groupId == 0)
                letter = "";

            var countFinal = 0;
            if (usersInfo.Where(x => (x.ShippingAddressId == order.ShippingAddressId || x.Address1 == order.ShippingAddress.Address1) &&
                 x.CustomerId == order.CustomerId).Count() > 0)
            {
                var user = usersInfo.Where(x => (x.ShippingAddressId == order.ShippingAddressId || x.Address1 == order.ShippingAddress.Address1) &&
                 x.CustomerId == order.CustomerId).FirstOrDefault();
                if (groupId == 0)
                {
                    letter = GetLetter(user.ComplementaryCount);
                    user.ComplementaryCount += 1;
                }
                countFinal = user.OrderNumber;
            }
            else
            {
                if (groupId == 0)
                {
                    orderCount++;
                    countFinal = orderCount;
                    usersInfo.Add(new UserInfoForClient
                    {
                        CustomerId = order.CustomerId,
                        ShippingAddressId = order.ShippingAddressId,
                        Address1 = order.ShippingAddress.Address1,
                        OrderNumber = orderCount,
                        ComplementaryCount = 0
                    });
                }
            }
            //if (!string.IsNullOrEmpty(letter))
            //    Debugger.Break();

            Paragraph header1 = new Paragraph("Número de orden: " + order.CustomOrderNumber + " " + GetRouteName(order.RouteId, routes).Replace("R", "") + "-" + countFinal + letter);
            //Paragraph header2 = new Paragraph(order.CustomOrderNumber);
            header1.SetBold();
            header1.SetFontSize(10);
            header1.SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

            var header2Text = order.SelectedShippingDate.Value.ToString("dd-MM-yyyy");
            header2Text = header2Text + " / " + order.SelectedShippingTime;
            Paragraph header2 = new Paragraph("Fecha y hora de entrega: " + header2Text);
            header2.SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);
            header2.SetFontSize(9);

            Paragraph header5 = new Paragraph("Información de envío:");
            header5.SetBold();
            header5.SetFontSize(9);
            header5.SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

            Paragraph header6 = new Paragraph($"{order.ShippingAddress.FirstName} {order.ShippingAddress.LastName},\n{order.ShippingAddress.Address1} {order.ShippingAddress.Address2}\n{_addressAttributeFormatter.FormatAttributes(order.ShippingAddress.CustomAttributes, htmlEncode: false)}");
            header6.SetFontSize(8);
            header6.SetFixedLeading(10);
            header6.SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

            orderDetailsHeaderTable = new Table(2);
            orderDetailsHeaderTable.SetWidth(PageSize.A4.GetWidth() - 50);
            orderDetailsHeaderTable.SetBorder(Border.NO_BORDER);

            orderDetailsHeaderTable.AddCell(new Cell(2, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(img));

            orderDetailsHeaderTable.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(header1));
            orderDetailsHeaderTable.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(header2));


            if (!buildCopy)
            {
                orderDetailsHeaderTable.AddCell(new Cell()
                                    .SetBorderRight(Border.NO_BORDER)
                                    .SetBorderLeft(Border.NO_BORDER)
                                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                    .Add(header5));

                orderDetailsHeaderTable.AddCell(new Cell()
                        .SetBorderRight(Border.NO_BORDER)
                        .SetBorderLeft(Border.NO_BORDER)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(header6));
            }

            if (!clientCopy)
            {
                orderDetailsHeaderTable.AddCell(new Cell(1, 2)
                        .SetBorder(Border.NO_BORDER)
                        .SetPadding(0)
                        .SetMargin(0)
                        .SetHeight(5)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(new Paragraph()));

                orderDeliveredOrNot = new Table(6);
                orderDeliveredOrNot.SetWidth(PageSize.A4.GetWidth() - 50);
                orderDeliveredOrNot.SetBorder(Border.NO_BORDER);
                orderDeliveredOrNot.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.LEFT);
                orderDeliveredOrNot.SetMarginRight(50);

                if (!buildCopy)
                {
                    Paragraph OrderCreator = new Paragraph("Su pedido fue armado por: ")
                    .SetBold()
                    .SetFontSize(8)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                    Paragraph NameSpace = new Paragraph("______________________________")
                        .SetFontColor(new DeviceRgb(255, 255, 255))
                        .SetBold()
                        .SetFontSize(8)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                    Paragraph Delivered = new Paragraph("Entregado")
                        .SetBold()
                        .SetFontSize(8)
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                    Paragraph CheckBlock = new Paragraph(" ")
                        .SetFontColor(new DeviceRgb(255, 255, 255))
                        .SetBold()
                        .SetFontSize(8)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                    Paragraph NotDelivered = new Paragraph("No entregado")
                        .SetBold()
                        .SetFontSize(8)
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                    orderDeliveredOrNot.AddHeaderCell(new Cell()
                            .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(OrderCreator));

                    orderDeliveredOrNot.AddHeaderCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .SetWidth(15)
                            .Add(NameSpace));

                    orderDeliveredOrNot.AddHeaderCell(new Cell()
                            .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(NotDelivered));

                    orderDeliveredOrNot.AddHeaderCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .SetWidth(15)
                            .Add(CheckBlock));

                    orderDeliveredOrNot.AddHeaderCell(new Cell()
                            .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(Delivered));

                    orderDeliveredOrNot.AddHeaderCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .SetWidth(15)
                            .Add(CheckBlock));
                }

                orderDeliveredOrNot.AddCell(new Cell(1, 6)
                        .SetHeight(5)
                        .SetPadding(0)
                        .SetMargin(0)
                        .SetBorder(Border.NO_BORDER)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(new Paragraph()));
            }
            else
            {
                orderDetailsHeaderTable.AddCell(new Cell(1, 2)
                        .SetBorder(Border.NO_BORDER)
                        .SetHeight(5)
                        .SetPadding(0)
                        .SetMargin(0)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(new Paragraph()));

                //orderDeliveredOrNot = new Table(1);
                //orderDeliveredOrNot.SetWidth(PageSize.A4.GetWidth() - 50);
                //orderDeliveredOrNot.SetBorder(Border.NO_BORDER);

                orderDeliveredOrNot = new Table(2);
                orderDeliveredOrNot.SetWidth(PageSize.A4.GetWidth() - 200);
                orderDeliveredOrNot.SetBorder(Border.NO_BORDER);
                orderDeliveredOrNot.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.LEFT);
                orderDeliveredOrNot.SetMarginRight(50);

                Paragraph OrderCreator = new Paragraph("Su pedido fue armado por: ")
                    .SetFontSize(8)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                Paragraph NameSpace = new Paragraph("___________________________________")
                   .SetFontColor(new DeviceRgb(255, 255, 255))
                   .SetBold()
                   .SetFontSize(8)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                orderDeliveredOrNot.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(OrderCreator));

                orderDeliveredOrNot.AddHeaderCell(new Cell()
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .SetWidth(15)
                        .Add(NameSpace));
            }

            orderDetailsTitle = new Paragraph("Detalles de la orden");
            orderDetailsTitle.SetFontSize(10);
            orderDetailsTitle.SetBold();
            orderDetailsTitle.SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            orderDetailsProductTable = new Table(5);
            orderDetailsProductTable.SetWidth(PageSize.A4.GetWidth() - 50);
            orderDetailsProductTable.SetBorder(Border.NO_BORDER);

            Paragraph productsDetailsHeader0 = new Paragraph("#")
                    .SetBold()
                    .SetFontSize(8)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            orderDetailsProductTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(productsDetailsHeader0));

            Paragraph productsDetailsHeader3 = new Paragraph("Cant.")
                    .SetBold()
                    .SetFontSize(8)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            orderDetailsProductTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(productsDetailsHeader3));

            Paragraph productsDetailsHeader1 = new Paragraph("Producto")
                    .SetBold()
                    .SetFontSize(8)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            orderDetailsProductTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(productsDetailsHeader1));

            Paragraph productsDetailsHeader2 = new Paragraph("Precio")
                    .SetBold()
                    .SetFontSize(8)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            orderDetailsProductTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(productsDetailsHeader2));

            Paragraph productsDetailsHeader4 = new Paragraph("Total")
                    .SetBold()
                    .SetFontSize(8)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            orderDetailsProductTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(productsDetailsHeader4));

            //var orderedItems = order.OrderItems.OrderBy(x => x.Product.Name);
            List<OrderItem> orderedItems = new List<OrderItem>();
            if (buildCopy)
            {
                orderedItems = parsedOrderItems.OrderBy(x => x.Product.ProductSpecificationAttributes
                     .Where(y => y.SpecificationAttributeOption.Name.ToLower().Contains("nevera") ||
                     y.SpecificationAttributeOption.Name.ToLower().Contains("manojos")).FirstOrDefault()?.SpecificationAttributeOption.Name)
                     .ThenByDescending(x => x.Product.ProductSpecificationAttributes
                     .Where(y => y.SpecificationAttributeOption.Name.ToLower().Contains("dureza"))
                     .FirstOrDefault() == null ? "z6" : x.Product.ProductSpecificationAttributes
                     .Where(y => y.SpecificationAttributeOption.Name.ToLower().Contains("dureza"))
                     .FirstOrDefault().SpecificationAttributeOption.Name).ThenByDescending(x => x.Product.Name).ToList();
            }
            else
            {
                orderedItems = parsedOrderItems.OrderByDescending(x => x.Product.ProductSpecificationAttributes
                    .Where(y => y.SpecificationAttributeOption.Name.ToLower().Contains("nevera") ||
                    y.SpecificationAttributeOption.Name.ToLower().Contains("manojos")).FirstOrDefault()?.SpecificationAttributeOption.Name)
                    .ThenBy(x => x.Product.ProductSpecificationAttributes
                    .Where(y => y.SpecificationAttributeOption.Name.ToLower().Contains("dureza"))
                    .FirstOrDefault() == null ? "z6" : x.Product.ProductSpecificationAttributes
                    .Where(y => y.SpecificationAttributeOption.Name.ToLower().Contains("dureza"))
                    .FirstOrDefault().SpecificationAttributeOption.Name).ThenBy(x => x.Product.Name).ToList();
            }

            var count = 0;
            foreach (var item in orderedItems)
            {
                count++;
                Paragraph text0 = new Paragraph(count.ToString())
                    .SetFontSize(7)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                string attributeText = item.Product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.Name.ToLower().Contains("nevera")).Any() ? " (N)" :
                    item.Product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.Name.ToLower().Contains("manojos")).Any() ? " (M)" : string.Empty;

                Paragraph text1 = new Paragraph(item.Product.Name + (string.IsNullOrWhiteSpace(item.SelectedPropertyOption) ? "" : $" ({item.SelectedPropertyOption})") + attributeText
                    //+ (item.Product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.Name.ToLower().Contains("dureza")).Any() ?
                    //" " + item.Product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.Name.ToLower().Contains("dureza")).FirstOrDefault()?.SpecificationAttributeOption.Name : "")
                    )
                    .SetFontSize(7)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

                decimal sellPrice = item.UnitPriceInclTax;
                string unit = " / pz";
                if (item.EquivalenceCoefficient > 0)
                {
                    sellPrice = (1000 * item.UnitPriceInclTax) / ((item.Quantity * 1000) / item.EquivalenceCoefficient);
                    unit = " / kg";
                }
                else if (item.WeightInterval > 0)
                {
                    sellPrice = (1000 * item.UnitPriceInclTax) / (item.Quantity * item.WeightInterval);
                    unit = " / kg";
                }

                Paragraph text4 = new Paragraph("$" + sellPrice.ToString("0.##") + unit)
                    .SetFontSize(7)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                var weight = string.Empty;
                decimal result = 0;
                if (item.EquivalenceCoefficient > 0)
                {
                    result = (item.Quantity * 1000) / item.EquivalenceCoefficient;
                }
                else if (item.WeightInterval > 0)
                {
                    result = item.Quantity * item.WeightInterval;
                }

                if (result >= 1000)
                {
                    weight = (result / 1000).ToString("0.##") + " kg";
                }
                else if (result > 0)
                {
                    weight = result.ToString("0.##") + " gr";
                }

                var qty = (item.BuyingBySecondary && item.EquivalenceCoefficient > 0 && !string.IsNullOrWhiteSpace(weight)) || item.WeightInterval > 0 ? $"{weight}" : $"{item.Quantity.ToString()} pz";

                Paragraph text2 = new Paragraph(qty)
                    .SetFontSize(7)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                Paragraph text3 = new Paragraph("$" + item.PriceInclTax.ToString("0.##"))
                    .SetFontSize(7)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                orderDetailsProductTable.AddCell(new Cell()
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetBorderRight(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(text0));

                orderDetailsProductTable.AddCell(new Cell()
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetBorderRight(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(text2));

                orderDetailsProductTable.AddCell(new Cell()
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetBorderRight(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(text1));

                orderDetailsProductTable.AddCell(new Cell()
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetBorderRight(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(text4));

                orderDetailsProductTable.AddCell(new Cell()
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetBorderRight(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(text3));
            }

            var orderNotes = order.OrderNotes.Where(x => x.DisplayToCustomer).Select(x => x.Note).ToList();
            if (orderNotes.Count() > 0)
            {
                var routeName = routes.Where(x => x.Id == order.RouteId).Select(x => x.RouteName).FirstOrDefault();

                orderDetailsProductTable.AddCell(new Cell(1, 5)
                    .SetBorder(Border.NO_BORDER)
                    .Add(new Paragraph().SetMarginTop(10)));

                Paragraph orderNotesText = new Paragraph("Notas de la orden: " + string.Join(". ", orderNotes.Select(x => x + $" ({routeName})")))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

                orderDetailsProductTable.AddCell(new Cell(1, 5)
                   .SetBorder(Border.NO_BORDER)
                   .Add(orderNotesText));
            }

            orderDetailsFooterTable = new Table(2)
                .SetWidth(PageSize.A4.GetWidth() - 50)
                .SetBorderLeft(Border.NO_BORDER)
                .SetKeepTogether(true)
                .SetBorderRight(Border.NO_BORDER);

            if (parsedOrderItems.Count() <= 15 && !clientCopy || parsedOrderItems.Count() <= 18 && clientCopy)
            {
                //orderDetailsFooterTable.SetFixedPosition(doc.GetLeftMargin(), doc.GetBottomMargin(), ps.GetWidth() - doc.GetLeftMargin() - doc.GetRightMargin());
                orderDetailsFooterTable.SetMarginTop(0);
            }
            else
            {
                orderDetailsFooterTable.SetMarginTop(0);
            }

            if (!buildCopy)
            {
                var pm = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
                var paymentStatusText = order.PaymentStatus == Nop.Core.Domain.Payments.PaymentStatus.Paid ? "(PAGADO)" : "";

                Paragraph paymentMethod = new Paragraph("Método de pago: " + (pm != null ? pm.PluginDescriptor.FriendlyName : order.PaymentMethodSystemName) + paymentStatusText)
                    .SetFontSize(7)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                Paragraph subTotalText1 = new Paragraph("SUBTOTAL:")
                    .SetFontSize(7)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                Paragraph subTotalText2 = new Paragraph(order.OrderSubtotalInclTax.ToString("C"))
                    .SetFontSize(7)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                Paragraph shippingText1 = new Paragraph("ENVÍO:")
                    .SetFontSize(7)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                Paragraph shippingText2 = new Paragraph(order.OrderShippingInclTax.ToString("C"))
                    .SetFontSize(7)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                Paragraph totalText1 = new Paragraph("TOTAL:")
                    .SetBold()
                    .SetFontSize(7)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                Paragraph totalText2 = new Paragraph(order.OrderTotal.ToString("C"))
                     .SetFontSize(7)
                     .SetBold()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                orderDetailsFooterTable.AddCell(new Cell()
                    .SetPadding(0)
                    .SetMargin(0)
                    .SetBorderRight(Border.NO_BORDER)
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(subTotalText1));

                orderDetailsFooterTable.AddCell(new Cell()
                    .SetBorderRight(Border.NO_BORDER)
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(subTotalText2));

                if (order.OrderSubTotalDiscountInclTax != 0 || order.OrderDiscount != 0)
                {
                    Paragraph discountText1 = new Paragraph("DESCUENTO:")
                    .SetFontSize(7)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                    var discountValue = order.OrderDiscount > 0 ? order.OrderDiscount : order.OrderSubTotalDiscountInclTax;
                    Paragraph discountText2 = new Paragraph(discountValue.ToString("C"))
                        .SetFontSize(7)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                    orderDetailsFooterTable.AddCell(new Cell()
                        .SetBorderRight(Border.NO_BORDER)
                        .SetBorderLeft(Border.NO_BORDER)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(discountText1));

                    orderDetailsFooterTable.AddCell(new Cell()
                        .SetBorderRight(Border.NO_BORDER)
                        .SetBorderLeft(Border.NO_BORDER)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(discountText2));
                }

                orderDetailsFooterTable.AddCell(new Cell()
                    .SetBorderRight(Border.NO_BORDER)
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(shippingText1));

                orderDetailsFooterTable.AddCell(new Cell()
                    .SetBorderRight(Border.NO_BORDER)
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(shippingText2));

                if (order.CustomerBalanceUsedAmount > 0)
                {
                    Paragraph customerBalanceUsed1 = new Paragraph("PAGO CON SALDO:")
                        .SetFontSize(7)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                    Paragraph customerBalanceUsed2 = new Paragraph((order.CustomerBalanceUsedAmount ?? 0).ToString("C"))
                        .SetFontSize(7)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                    orderDetailsFooterTable.AddCell(new Cell()
                        .SetPadding(0)
                        .SetMargin(0)
                        .SetBorderRight(Border.NO_BORDER)
                        .SetBorderLeft(Border.NO_BORDER)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(customerBalanceUsed1));

                    orderDetailsFooterTable.AddCell(new Cell()
                        .SetBorderRight(Border.NO_BORDER)
                        .SetBorderLeft(Border.NO_BORDER)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(customerBalanceUsed2));

                }

                orderDetailsFooterTable.AddCell(new Cell()
                    .SetBorderRight(Border.NO_BORDER)
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(totalText1));

                orderDetailsFooterTable.AddCell(new Cell()
                    .SetBorderRight(Border.NO_BORDER)
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(totalText2));

                if (order.DiscountUsageHistory.Count() > 0)
                {
                    Paragraph discounts = new Paragraph("Descuentos aplicados: " + string.Join(", ", order.DiscountUsageHistory.Select(x => x.Discount.Name + " " + (x.Discount.LimitationTimes > 1 && !string.IsNullOrWhiteSpace(x.Discount.CouponCode) && x.Discount.DiscountType != Nop.Core.Domain.Discounts.DiscountType.AssignedToShipping ? $"({x.Discount.CouponCode})" : string.Empty))))
                                    .SetFontSize(7)
                                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

                    orderDetailsFooterTable.AddCell(new Cell()
                       .SetBorder(Border.NO_BORDER)
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                       .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                       .Add(discounts));
                }

                orderDetailsFooterTable.AddCell(new Cell(1, order.DiscountUsageHistory.Count() > 0 ? 1 : 2)
                    .SetBorder(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(paymentMethod));

                if (!clientCopy)
                {
                    orderDetailsFooterTable.AddCell(new Cell(1, 2)
                        .SetBorder(Border.NO_BORDER)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(new Paragraph().SetMarginTop(40)));

                    Paragraph sign = new Paragraph("Nombre, firma y comentarios de quien recibe")
                    .SetFontSize(9)
                    .SetBold()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                    orderDetailsFooterTable.AddCell(new Cell(1, 2)
                        .SetBorderRight(Border.NO_BORDER)
                        .SetBorderLeft(Border.NO_BORDER)
                        .SetBorderBottom(Border.NO_BORDER)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(sign));
                }
            }

            Paragraph footerText = new Paragraph($"{_storeContext.CurrentStore.Name}\n{_storeContext.CurrentStore.CompanyAddress}\nWhatsApp de atención: 55 4072 9627\n{_storeContext.CurrentStore.Url.Replace("http:", "").Replace("/", "")}")
                        .SetFontSize(7)
                        .SetMargin(0)
                        .SetPadding(0)
                        .SetFixedLeading(8)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            orderDetailsFooterTable.AddCell(new Cell(1, 2)
                    .SetPadding(0)
                    .SetMargin(0)
                    .SetBorder(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(footerText));

            Paragraph orderDetailsCopy = new Paragraph(clientCopy ? "- Copia cliente -" : buildCopy ? "- Hoja de armado -" : "- Copia comercio -")
            .SetFontSize(8)
            .SetBold()
            .SetFontColor(new DeviceRgb(255, 99, 71))
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            orderDetailsFooterTable.AddCell(new Cell(1, 2)
                    .SetPadding(0)
                    .SetMargin(0)
                    .SetBorder(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(orderDetailsCopy));

        }

        private void PrepareMainHeader(IList<Order> orders, bool provider, DateTime selectedDate)
        {
            string imageFile = $"wwwroot/images/logo.png";
            ImageData data = ImageDataFactory.Create(imageFile);

            Image img = new Image(data);
            img.SetHeight(20);
            img.SetMarginRight(10);
            Paragraph Img = new Paragraph();
            Img.Add(img);
            Img.SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
            Img.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);

            shoppingListHeaderTable = new Table(4);
            shoppingListHeaderTable.SetWidth(PageSize.A4.GetWidth() - 30);

            shoppingListHeaderTable.AddCell(new Cell(3, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(img));
            shoppingListHeaderTable.AddCell(new Cell(3, 1)
                    .SetBorder(Border.NO_BORDER)
                    .Add(new Paragraph().SetMarginLeft(270)));

            if (!provider)
            {
                Paragraph header1 = new Paragraph("# de órdenes");
                header1.SetBold();
                header1.SetFontSize(8);
                header1.SetBackgroundColor(new DeviceRgb(220, 220, 220));
                header1.SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                Paragraph header2 = new Paragraph(orders.Count().ToString());
                header2.SetBold();
                header2.SetFontSize(8);
                header2.SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);
                header2.SetFontColor(new DeviceRgb(255, 99, 71));

                shoppingListHeaderTable.AddCell(new Cell()
                        .SetBorder(Border.NO_BORDER)
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(header1));

                shoppingListHeaderTable.AddCell(new Cell()
                        .SetBorder(Border.NO_BORDER)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(header2));
            }

            Paragraph header3 = new Paragraph("Fecha de entrega");
            header3.SetBold();
            header3.SetFontSize(8);
            header3.SetBackgroundColor(new DeviceRgb(220, 220, 220));
            header3.SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

            Paragraph header4 = new Paragraph(selectedDate.ToString("dd-MM-yyyy"));
            header4.SetBold();
            header4.SetFontSize(8);
            header4.SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

            shoppingListHeaderTable.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(header3));
            shoppingListHeaderTable.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(header4));

            if (!provider)
            {
                Paragraph header5 = new Paragraph("Total vendido");
                header5.SetBold();
                header5.SetFontSize(8);
                header5.SetBackgroundColor(new DeviceRgb(220, 220, 220));
                header5.SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                Paragraph header6 = new Paragraph(orders.Sum(x => x.OrderTotal).ToString("C"));
                header6.SetFontSize(8);
                header6.SetBold();
                header6.SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                shoppingListHeaderTable.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(header5));
                shoppingListHeaderTable.AddCell(new Cell()
                        .SetBorder(Border.NO_BORDER)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(header6));
            }
        }

        private void PrepareShoppingListByRouteDetails(IList<OrderItem> orderItems)
        {
            shoppingListDetailsTitle = new Paragraph("Detalles de la ruta");
            shoppingListDetailsTitle.SetFontSize(15);
            shoppingListDetailsTitle.SetBold();
            shoppingListDetailsTitle.SetTextAlignment(TextAlignment.CENTER);

            shoppingListDetailsTable = new Table(3);
            shoppingListDetailsTable.SetWidth(PageSize.A4.GetWidth() - 30);

            Paragraph shoppingDetailsHeader1 = new Paragraph("Categoría")
                    .SetBold()
                    .SetFontSize(10)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            Paragraph shoppingDetailsHeader2 = new Paragraph("Cant. de productos")
                    .SetBold()
                    .SetFontSize(10)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            Paragraph shoppingDetailsHeader3 = new Paragraph("Cant. de productos diferentes")
                    .SetBold()
                    .SetFontSize(10)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            shoppingListDetailsTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader1));

            shoppingListDetailsTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader2));

            shoppingListDetailsTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader3));

            var groupedByCategory = orderItems.GroupBy(x => x.Product.ProductCategories.Where(y => y.Category.ParentCategoryId == 0).Select(y => y.Category).FirstOrDefault());
            int differentProductsCount = 0;
            int productsCount = 0;
            decimal totalCost = 0;

            foreach (var category in groupedByCategory)
            {
                int differentProductsQty = category.GroupBy(x => x.ProductId).Count();
                differentProductsCount += differentProductsQty;

                int productsQty = category.GroupBy(x => x.ProductId).SelectMany(x => x).Count();
                productsCount += productsQty;

                List<int> categoryOrderItemIds = category.Select(x => x.Id).ToList();
                decimal categoryCost = _orderItemBuyerService.GetAll().Where(x => categoryOrderItemIds.Contains(x.OrderItemId)).Select(x => x.Cost).DefaultIfEmpty().Sum();
                totalCost += categoryCost;

                Paragraph p1 = new Paragraph((category.Key == null ? "Productos sin categoría padre" : category.Key.Name))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetMarginLeft(5);

                Paragraph p2 = new Paragraph(productsQty.ToString())
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetMarginLeft(5);

                Paragraph p3 = new Paragraph(differentProductsQty.ToString())
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetMarginLeft(5);

                shoppingListDetailsTable.AddCell(new Cell()
                            .SetFontSize(8)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(p1));

                shoppingListDetailsTable.AddCell(new Cell()
                            .SetFontSize(8)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(p2));

                shoppingListDetailsTable.AddCell(new Cell()
                            .SetFontSize(8)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(p3));
            }

            Paragraph totalText = new Paragraph("TOTAL")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetMarginLeft(5);

            Paragraph totalProducts = new Paragraph(productsCount.ToString())
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetMarginLeft(5);

            Paragraph totalDifferentProducts = new Paragraph(differentProductsCount.ToString())
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetMarginLeft(5);

            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(totalText));

            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(totalProducts));

            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(totalDifferentProducts));

            shoppingListDetailsTable.AddCell(new Cell(1, 3)
                    .SetPadding(10)
                    .SetBorder(Border.NO_BORDER)
                    .Add(new Paragraph()));
        }

        private void PrepareShoppingListByBuyerDetails(IList<OrderItem> orderItems)
        {
            shoppingListDetailsTitle = new Paragraph("Detalles del día");
            shoppingListDetailsTitle.SetFontSize(10);
            shoppingListDetailsTitle.SetBold();
            shoppingListDetailsTitle.SetTextAlignment(TextAlignment.CENTER);

            shoppingListDetailsTable = new Table(4);
            shoppingListDetailsTable.SetWidth(PageSize.A4.GetWidth() - 30);

            Paragraph shoppingDetailsHeader1 = new Paragraph("Categoría")
                    .SetBold()
                    .SetFontSize(10)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            Paragraph shoppingDetailsHeader2 = new Paragraph("Cant. de productos")
                    .SetBold()
                    .SetFontSize(10)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            Paragraph shoppingDetailsHeader3 = new Paragraph("Cant. de productos diferentes")
                    .SetBold()
                    .SetFontSize(10)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            Paragraph shoppingDetailsHeader4 = new Paragraph("Costo por categoría")
                    .SetBold()
                    .SetFontSize(10)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            shoppingListDetailsTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader1));

            shoppingListDetailsTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader2));

            shoppingListDetailsTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader3));

            shoppingListDetailsTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader4));



            var orderItemsWhereProductIsNull = orderItems.Where(x => x.Product == null).ToList();
            if (orderItemsWhereProductIsNull != null && orderItemsWhereProductIsNull.Count() > 0)
            {
                foreach (var orderItem in orderItemsWhereProductIsNull)
                {
                    Product product = _productService.GetProductById(orderItem.ProductId);
                    orderItem.Product = product;
                }

            }

            var groupedByCategory = orderItems.GroupBy(x => x.Product.ProductCategories.Where(y => y.Category.ParentCategoryId == 0).Select(y => y.Category).FirstOrDefault()).ToList();

            int differentProductsCount = 0;
            int productsCount = 0;
            decimal totalCost = 0;

            foreach (var category in groupedByCategory)
            {
                int differentProductsQty = category.GroupBy(x => x.ProductId).Count();
                differentProductsCount += differentProductsQty;

                int productsQty = category.GroupBy(x => x.ProductId).SelectMany(x => x).Count();
                productsCount += productsQty;

                List<int> categoryOrderItemIds = category.Select(x => x.Id).ToList();
                decimal categoryCost = _orderItemBuyerService.GetAll().Where(x => categoryOrderItemIds.Contains(x.OrderItemId)).Select(x => x.Cost).DefaultIfEmpty().Sum();
                totalCost += categoryCost;

                Paragraph p1 = new Paragraph((category.Key == null ? "Productos sin categoría padre" : category.Key.Name))
                    .SetFontSize(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetMarginLeft(5);

                Paragraph p2 = new Paragraph(productsQty.ToString())
                    .SetFontSize(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetMarginLeft(5);

                Paragraph p3 = new Paragraph(differentProductsQty.ToString())
                    .SetFontSize(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetMarginLeft(5);

                Paragraph p4 = new Paragraph(categoryCost.ToString("C"))
                    .SetFontSize(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetMarginLeft(5);

                shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(p1));

                shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(p2));

                shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(p3));

                shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(p4));
            }

            Paragraph totalText = new Paragraph("TOTAL")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetFontSize(10)
                    .SetMarginLeft(5);

            Paragraph totalProducts = new Paragraph(productsCount.ToString())
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetFontSize(10)
                    .SetMarginLeft(5);

            if (buyersTotals.Any())
                buyersTotals.Last().Products = productsCount.ToString();

            Paragraph totalDifferentProducts = new Paragraph(differentProductsCount.ToString())
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetFontSize(10)
                    .SetMarginLeft(5);

            Paragraph totalSalesText = new Paragraph(totalCost.ToString("C"))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetFontSize(10)
                    .SetMarginLeft(5);

            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(totalText));

            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(totalProducts));

            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(totalDifferentProducts));

            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(totalSalesText));

            shoppingListDetailsTable.AddCell(new Cell(1, 4)
                    .SetPadding(10)
                    .SetBorder(Border.NO_BORDER)
                    .Add(new Paragraph()));
        }

        private void PrepareShoppingListDetails(IList<Order> orders)
        {
            shoppingListDetailsTitle = new Paragraph("Detalles del día");
            shoppingListDetailsTitle.SetFontSize(15);
            shoppingListDetailsTitle.SetBold();
            shoppingListDetailsTitle.SetTextAlignment(TextAlignment.CENTER);

            shoppingListDetailsTable = new Table(4);
            shoppingListDetailsTable.SetWidth(PageSize.A4.GetWidth() - 30);

            var routes = _shippingRouteService.GetAll().ToList();

            Paragraph shoppingDetailsHeader1 = new Paragraph("Categoría")
                    .SetBold()
                    .SetFontSize(8)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            Paragraph shoppingDetailsHeader2 = new Paragraph("Cant. de productos")
                    .SetBold()
                    .SetFontSize(8)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            Paragraph shoppingDetailsHeader3 = new Paragraph("Cant. de productos diferentes")
                    .SetBold()
                    .SetFontSize(8)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            Paragraph shoppingDetailsHeader4 = new Paragraph("Monto de venta")
                    .SetBold()
                    .SetFontSize(8)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            shoppingListDetailsTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader1));

            shoppingListDetailsTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader2));

            shoppingListDetailsTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader3));

            shoppingListDetailsTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader4));

            var parsedOrderItems = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService);
            var groupedByCategory = parsedOrderItems.GroupBy(x => x.Product.ProductCategories.Where(y => y.Category.ParentCategoryId == 0).Select(y => y.Category).FirstOrDefault());

            int differentProductsCount = 0;
            int productsCount = 0;

            decimal subTotalSales = 0;

            foreach (var category in groupedByCategory)
            {
                int differentProductsQty = category.GroupBy(x => x.ProductId).Count();
                differentProductsCount += differentProductsQty;

                int productsQty = category.GroupBy(x => x.ProductId).SelectMany(x => x).Count();
                productsCount += productsQty;

                decimal sales = category.Sum(x => x.PriceInclTax);
                subTotalSales += sales;

                Paragraph p1 = new Paragraph((category.Key == null ? "Productos sin categoría padre" : category.Key.Name))
                    .SetFontSize(8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetMarginLeft(5);

                Paragraph p2 = new Paragraph(productsQty.ToString())
                    .SetFontSize(8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetMarginLeft(5);

                Paragraph p3 = new Paragraph(differentProductsQty.ToString())
                    .SetFontSize(8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetMarginLeft(5);

                Paragraph p4 = new Paragraph(category.Sum(x => x.PriceInclTax).ToString("C"))
                    .SetFontSize(8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetMarginLeft(5);

                shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(p1));

                shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(p2));

                shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(p3));

                shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(p4));
            }

            // ENVIOS
            //Paragraph shipping = new Paragraph("Envíos")
            //        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
            //        .SetMarginLeft(5);

            //var totalShippingsQty = orders.Where(x => x.OrderShippingInclTax > 0).Count();
            //Paragraph shippingProductQty = new Paragraph(totalShippingsQty.ToString())
            //    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
            //    .SetMarginLeft(5);

            //Paragraph shippingDifQty = new Paragraph(totalShippingsQty.ToString())
            //    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
            //    .SetMarginLeft(5);

            //decimal shippingTotal = orders.Select(x => x.OrderShippingInclTax).DefaultIfEmpty().Sum();
            //Paragraph shippingTotalText = new Paragraph(shippingTotal.ToString("C"))
            //    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
            //    .SetMarginLeft(5);

            //shoppingListDetailsTable.AddCell(new Cell()
            //            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
            //            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
            //            .Add(shipping));

            //shoppingListDetailsTable.AddCell(new Cell()
            //            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
            //            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
            //            .Add(shippingProductQty));

            //shoppingListDetailsTable.AddCell(new Cell()
            //            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
            //            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
            //            .Add(shippingDifQty));

            //shoppingListDetailsTable.AddCell(new Cell()
            //            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
            //            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
            //            .Add(shippingTotalText));

            Paragraph subtotalText = new Paragraph("SUBTOTAL")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetFontSize(9)
                    .SetBold()
                    .SetMarginLeft(5);

            Paragraph subtotalProducts = new Paragraph(productsCount.ToString())
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetFontSize(9)
                    .SetMarginLeft(5);

            Paragraph subtotalDifferentProducts = new Paragraph(differentProductsCount.ToString())
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetFontSize(9)
                    .SetMarginLeft(5);

            Paragraph subtotalSalesText = new Paragraph(subTotalSales.ToString("C"))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetFontSize(9)
                    .SetMarginLeft(5);

            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(subtotalText));

            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(subtotalProducts));

            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(subtotalDifferentProducts));

            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(subtotalSalesText));

            Paragraph discountText = new Paragraph("DESCUENTOS")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetFontSize(9)
                    .SetMarginLeft(5);

            Paragraph discountProducts = new Paragraph("-")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetFontSize(9)
                    .SetMarginLeft(5);

            Paragraph discountDifferentProducts = new Paragraph("-")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetFontSize(9)
                    .SetMarginLeft(5);

            decimal totalDiscount = orders.DefaultIfEmpty().Sum(x => x.OrderSubTotalDiscountInclTax);
            Paragraph discountSalesText = new Paragraph(totalDiscount.ToString("C"))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetFontSize(9)
                    .SetMarginLeft(5);

            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(discountText));

            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(discountProducts));

            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(discountDifferentProducts));

            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(discountSalesText));

            Paragraph totalText = new Paragraph("TOTAL")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetFontSize(9)
                    .SetMarginLeft(5);

            Paragraph totalProducts = new Paragraph(productsCount.ToString())
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetFontSize(9)
                    .SetMarginLeft(5);

            Paragraph totalDifferentProducts = new Paragraph(differentProductsCount.ToString())
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetFontSize(9)
                    .SetMarginLeft(5);

            Paragraph totalSalesText = new Paragraph((subTotalSales - totalDiscount).ToString("C"))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBold()
                    .SetFontSize(9)
                    .SetMarginLeft(5);

            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(totalText));

            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(totalProducts));

            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(totalDifferentProducts));

            shoppingListDetailsTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(totalSalesText));

            var isFirts = true;
            foreach (var order in orders)
            {
                var routeName = routes.Where(x => x.Id == order.RouteId).Select(x => x.RouteName).FirstOrDefault();
                if (string.IsNullOrEmpty(routeName))
                    routeName = "Sin ruta asignada";

                var orderNotes = order.OrderNotes.Where(x => x.DisplayToCustomer).Select(x => x.Note).ToList();
                if (orderNotes.Count() > 0)
                {
                    if (isFirts)
                    {
                        shoppingListDetailsTable.AddCell(new Cell(1, 4)
                                   .SetBorder(Border.NO_BORDER)
                                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                   .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                   .SetFontSize(15)
                                   .SetBold()
                                   .SetPaddingTop(20)
                                   .Add(new Paragraph("Notas de órdenes")));
                    }

                    Paragraph orderNotesText = new Paragraph("Orden #" + order.Id + ": " + string.Join(". ", orderNotes) + $" ({routeName})").SetFontSize(10);

                    shoppingListDetailsTable.AddCell(new Cell(1, 4)
                       .SetBorder(Border.NO_BORDER)
                       .Add(orderNotesText));

                    isFirts = false;
                }
            }
        }

        private void PrepareShoppingListByBuyerHeader(Customer buyer,
            List<OrderItemBuyer> orderItemBuyer,
            DateTime selectedDate,
            List<OrderItem> parsedOrderItems,
            List<Manufacturer> manufacturers,
            List<ProductMainManufacturer> productMainManufacturers)
        {

            var buyerMoney = OrderUtils.GetBuyerCashAndTransferAmount(parsedOrderItems, buyer.Id, orderItemBuyer, manufacturers, productMainManufacturers);

            shoppingListTitle = new Paragraph(buyer.GetFullName());
            shoppingListTitle.SetFontSize(15);
            shoppingListTitle.SetBold();
            shoppingListTitle.SetTextAlignment(TextAlignment.LEFT);

            shoppingListHeaderTable = new Table(2);
            shoppingListHeaderTable.SetWidth(PageSize.A4.GetWidth() - 30);

            shoppingListHeaderTable.AddCell(new Cell(2, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(shoppingListTitle));

            shoppingListHeaderTable.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .SetPaddingBottom(0)
                    .SetMarginBottom(0)
                    .Add(new Paragraph("Efectivo disponible comprador: " + RoundBuyerCashAmount(buyerMoney.Cash).ToString("C")).SetPaddingTop(0).SetMarginTop(0))
                    .SetTextAlignment(TextAlignment.RIGHT));

            shoppingListHeaderTable.AddCell(new Cell()
                   .SetBorder(Border.NO_BORDER)
                   .SetPaddingTop(0)
                   .SetMarginTop(0)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                   .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                   .Add(new Paragraph("Pago por transferencia: " + buyerMoney.Transfer.ToString("C")).SetPaddingTop(0).SetMarginTop(0))
                   .SetTextAlignment(TextAlignment.RIGHT));

            shoppingListHeaderTable.AddCell(new Cell(2, 2)
                   .SetBorder(Border.NO_BORDER)
                   .SetPaddingTop(0)
                   .SetMarginTop(0)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                   .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                   .Add(new Paragraph("Pago con tarjeta corporativa: " + buyerMoney.Card.ToString("C")).SetPaddingTop(0).SetMarginTop(0))
                   .SetTextAlignment(TextAlignment.RIGHT));

            buyersTotals.Add(new BuyersTotals
            {
                Name = buyer.GetFullName(),
                Budget = RoundBuyerCashAmount(buyerMoney.Cash).ToString("C"),
                Products = ""
            });

            budgetBuyerTable = new Table(2);
            budgetBuyerTable.SetWidth(PageSize.A4.GetWidth() - 30);

            Paragraph buyerName = new Paragraph(buyer.GetFullName())
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            Paragraph shoppingDetailsHeader1 = new Paragraph("FECHA")
                    .SetBold()
                    .SetFontSize(10)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

            Paragraph shoppingDetailsHeader2 = new Paragraph("COMPRADOR")
                    .SetBold()
                    .SetFontSize(10)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

            Paragraph shoppingDetailsHeader3 = new Paragraph("EFECTIVO ASIGNADO")
                    .SetBold()
                    .SetFontSize(10)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

            Paragraph shoppingDetailsHeader4 = new Paragraph($"{"GASTOS EN EFECTIVO COMPROBADOS,"}\n {"UNICAMENTE CON TICKET"}")
                    .SetBold()
                    .SetFontSize(10)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

            Paragraph shoppingDetailsHeader5 = new Paragraph("EFECTIVO A ENTREGAR")
                    .SetBold()
                    .SetFontSize(10)
                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

            budgetBuyerTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader1));

            budgetBuyerTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(255, 255, 255))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .SetWidth(300)
                        .Add(new Paragraph(selectedDate.ToString("dd-MM-yyyy"))));

            budgetBuyerTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader2));

            budgetBuyerTable.AddHeaderCell(new Cell()
                       .SetBackgroundColor(new DeviceRgb(255, 255, 255))
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                       .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                       .SetWidth(300)
                       .Add(buyerName));

            budgetBuyerTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader3));

            budgetBuyerTable.AddHeaderCell(new Cell()
                       .SetBackgroundColor(new DeviceRgb(255, 255, 255))
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                       .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                       .SetWidth(300)
                       .Add(new Paragraph(RoundBuyerCashAmount(buyerMoney.Cash).ToString("C"))));

            budgetBuyerTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(shoppingDetailsHeader4));

            budgetBuyerTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(255, 255, 255))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .SetWidth(300)
                        .Add(new Paragraph()));

            budgetBuyerTable.AddHeaderCell(new Cell()
                       .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                       .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                       .Add(shoppingDetailsHeader5));

            budgetBuyerTable.AddHeaderCell(new Cell()
                        .SetBackgroundColor(new DeviceRgb(255, 255, 255))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .SetWidth(300)
                        .Add(new Paragraph()));

            budgetBuyerTable.AddCell(new Cell(1, 2)
                       .SetBorder(Border.NO_BORDER)
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                       .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                       .Add(new Paragraph().SetMarginTop(40)));

            Paragraph sign = new Paragraph("Nombre, firma y comentarios del comprador")
            .SetFontSize(9)
            .SetBold()
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            budgetBuyerTable.AddCell(new Cell(1, 2)
                .SetBorderRight(Border.NO_BORDER)
                .SetBorderLeft(Border.NO_BORDER)
                .SetBorderBottom(Border.NO_BORDER)
                .SetMarginBottom(10)
                .SetPaddingBottom(10)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                .Add(sign));

        }

        private void PrepareShoppingListByRouteHeader(int routeId, IList<Order> orders)
        {
            var route = _shippingRouteService.GetAll().Where(x => x.Id == routeId).FirstOrDefault();

            shoppingListTitle = new Paragraph(route.RouteName);
            shoppingListTitle.SetFontSize(15);
            shoppingListTitle.SetBold();
            shoppingListTitle.SetTextAlignment(TextAlignment.LEFT);

            shoppingListHeaderTable = new Table(3);
            shoppingListHeaderTable.SetWidth(PageSize.A4.GetWidth() - 30);

            shoppingListHeaderTable.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(shoppingListTitle));

            shoppingListHeaderTable.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(new Paragraph("Cantidad de órdenes: " + orders.Where(x => x.RouteId == routeId).Count()))
                    .SetTextAlignment(TextAlignment.RIGHT));

            shoppingListHeaderTable.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(new Paragraph("Cantidad de entregas: " + orders.Where(x => x.RouteId == routeId).GroupBy(x => x.CustomerId).Count()))
                    .SetTextAlignment(TextAlignment.RIGHT));
        }

        private void PrepareShoppingListByManufacturerHeader(Manufacturer manufacturer)
        {
            shoppingListTitle = new Paragraph(manufacturer == null ? "Sin fabricante" : manufacturer.Name);
            shoppingListTitle.SetFontSize(15);
            shoppingListTitle.SetBold();
            shoppingListTitle.SetTextAlignment(TextAlignment.LEFT);

            shoppingListHeaderTable = new Table(1);
            shoppingListHeaderTable.SetWidth(PageSize.A4.GetWidth() - 30);

            shoppingListHeaderTable.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(shoppingListTitle));
        }

        private void PrepareShoppingListNotes(List<Order> orders)
        {
            shoppingListNotesTable = new Table(4);
            shoppingListNotesTable.SetWidth(PageSize.A4.GetWidth() - 30);

            var routes = _shippingRouteService.GetAll().ToList();

            var isFirts = true;
            foreach (var order in orders.OrderBy(x => x.RouteId).ToList())
            {
                var routeName = routes.Where(x => x.Id == order.RouteId).Select(x => x.RouteName).FirstOrDefault();
                if (string.IsNullOrEmpty(routeName))
                    routeName = "Sin ruta asignada";

                var orderNotes = order.OrderNotes.Where(x => x.DisplayToCustomer).Select(x => x.Note).ToList();
                if (orderNotes.Count() > 0)
                {
                    if (isFirts)
                        shoppingListNotesTable.AddCell(new Cell(1, 4)
                                   .SetBorder(Border.NO_BORDER)
                                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                   .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                   .SetFontSize(15)
                                   .SetBold()
                                   .SetPaddingTop(20)
                                   .Add(new Paragraph("Indicaciones especiales")));

                    Paragraph orderNotesText = new Paragraph("Orden #" + order.Id + ": " + string.Join(". ", orderNotes) + $" ({routeName})").SetFontSize(10);

                    shoppingListNotesTable.AddCell(new Cell(1, 4)
                       .SetBorder(Border.NO_BORDER)
                       .Add(orderNotesText));

                    isFirts = false;
                }
            }

            shoppingListNotesTable.AddCell(new Cell(1, 4)
                    .SetPadding(10)
                    .SetBorder(Border.NO_BORDER)
                    .Add(new Paragraph()));
        }

        private void PrepareSimpleShoppingListHeader(string title)
        {
            shoppingListTitle = new Paragraph(title);
            shoppingListTitle.SetFontSize(15);
            shoppingListTitle.SetBold();
            shoppingListTitle.SetTextAlignment(TextAlignment.LEFT);

            shoppingListHeaderTable = new Table(1);
            shoppingListHeaderTable.SetWidth(PageSize.A4.GetWidth() - 30);

            shoppingListHeaderTable.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(shoppingListTitle));
        }

        private void PrepareShoppingListByRoute(IList<Order> orders, int routeId)
        {
            var routeOrders = orders.Where(x => x.RouteId == routeId).ToList();
            var parsedOrderItems = routeOrders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService);
            PrepareShoppingListByRouteDetails(parsedOrderItems);
            PrepareSecondaryShoppingListByRouteDetails(orders, routeId);
            PrepareShoppingList(parsedOrderItems, false, false, true);
        }

        private void PrepareShoppingListExtraByRoute(int routeId, IList<Order> orders)
        {
            var orderIds = orders.Where(x => x.RouteId == routeId).Select(x => x.Id);
            shoppingListExtraTable = new Table(2)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(Border.NO_BORDER)
                .SetPaddingBottom(10);

            Table tips = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1.5f })).SetBorder(Border.NO_BORDER);
            Table payments = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1.5f })).SetBorder(Border.NO_BORDER);

            // TIPS TABLE
            tips.AddCell(new Cell(1, 2)
                .Add(new Paragraph("Propinas con tarjeta"))
                .SetFontSize(10)
                .SetBold()
                .SetTextAlignment(TextAlignment.LEFT));

            foreach (var orderId in orderIds)
            {
                tips.AddCell(new Cell()
                    .Add(new Paragraph("Propinas con tarjeta Pedido " + orderId.ToString()))
                    .SetFontSize(8)
                    .SetPadding(0)
                    .SetPaddingLeft(2)
                    .SetTextAlignment(TextAlignment.LEFT));

                tips.AddCell(new Cell()
                    .Add(new Paragraph())
                    .SetFontSize(8)
                    .SetPadding(0)
                    .SetPaddingLeft(2)
                    .SetTextAlignment(TextAlignment.LEFT));
            }

            tips.AddCell(new Cell(1, 2)
                .Add(new Paragraph("Total propinas con tarjeta"))
                .SetFontSize(10)
                .SetBold()
                .SetTextAlignment(TextAlignment.LEFT));

            tips.AddCell(new Cell(1, 2)
                .Add(new Paragraph())
                .SetFontSize(10)
                .SetBold()
                .SetPaddingTop(15)
                .SetBorderLeft(Border.NO_BORDER)
                .SetBorderRight(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.LEFT));

            tips.AddCell(new Cell(1, 2)
                .Add(new Paragraph("Gastos de ruta"))
                .SetFontSize(10)
                .SetBold()
                .SetTextAlignment(TextAlignment.LEFT));

            for (int i = 0; i < 4; i++)
            {
                tips.AddCell(new Cell()
                    .Add(new Paragraph())
                    .SetFontSize(8)
                    .SetPaddingTop(13)
                    .SetTextAlignment(TextAlignment.LEFT));

                tips.AddCell(new Cell()
                    .Add(new Paragraph())
                    .SetFontSize(8)
                    .SetPaddingTop(13)
                    .SetTextAlignment(TextAlignment.LEFT));
            }

            tips.AddCell(new Cell()
                .Add(new Paragraph("Total de gastos de ruta"))
                .SetFontSize(10)
                .SetBold()
                .SetTextAlignment(TextAlignment.LEFT));

            tips.AddCell(new Cell()
                .Add(new Paragraph())
                .SetFontSize(8)
                .SetBold()
                .SetTextAlignment(TextAlignment.LEFT));
            //

            // PAYMENTS TABLE
            payments.AddCell(new Cell(1, 2)
                .Add(new Paragraph("Cobros"))
                .SetFontSize(10)
                .SetBold()
                .SetTextAlignment(TextAlignment.LEFT));

            foreach (var orderId in orderIds)
            {
                payments.AddCell(new Cell()
                    .Add(new Paragraph("Cobro en efectivo Pedido " + orderId.ToString()))
                    .SetFontSize(8)
                    .SetPadding(0)
                    .SetPaddingLeft(2)
                    .SetTextAlignment(TextAlignment.LEFT));

                payments.AddCell(new Cell()
                    .Add(new Paragraph())
                    .SetFontSize(8)
                    .SetPadding(0)
                    .SetPaddingLeft(2)
                    .SetTextAlignment(TextAlignment.LEFT));
            }

            payments.AddCell(new Cell(1, 2)
                .Add(new Paragraph("Total cobrado en efectivo"))
                .SetFontSize(10)
                .SetBold()
                .SetTextAlignment(TextAlignment.LEFT));

            payments.AddCell(new Cell(1, 2)
                .Add(new Paragraph())
                .SetFontSize(10)
                .SetBold()
                .SetPaddingTop(15)
                .SetBorderLeft(Border.NO_BORDER)
                .SetBorderRight(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.LEFT));

            payments.AddCell(new Cell(1, 2)
                .Add(new Paragraph("Entrega de cuentas de ruta"))
                .SetFontSize(10)
                .SetBold()
                .SetTextAlignment(TextAlignment.LEFT));

            for (int i = 0; i < 4; i++)
            {
                payments.AddCell(new Cell()
                    .Add(new Paragraph(
                        i == 0 ? "Fondo inicial" :
                        i == 1 ? "(+) Total de cobros en efectivo" :
                        i == 2 ? "(-) Total de propinas con tarjeta" :
                        "(-) Total de gastos de ruta"))
                    .SetFontSize(8)
                    .SetPadding(0)
                    .SetPaddingLeft(2)
                    .SetTextAlignment(TextAlignment.LEFT));

                payments.AddCell(new Cell()
                    .Add(new Paragraph())
                    .SetFontSize(8)
                    .SetTextAlignment(TextAlignment.LEFT));
            }

            payments.AddCell(new Cell()
                .Add(new Paragraph("Total a entregar"))
                .SetFontSize(10)
                .SetBold()
                .SetTextAlignment(TextAlignment.LEFT));

            payments.AddCell(new Cell()
                .Add(new Paragraph())
                .SetFontSize(8)
                .SetBold()
                .SetTextAlignment(TextAlignment.LEFT));
            //

            shoppingListExtraTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(tips));
            shoppingListExtraTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(payments));
        }

        private void PrepareShoppingListLabels(List<OrderItem> orderItems, bool newVersion)
        {
            var routes = _shippingRouteService.GetAll().ToList();
            if (newVersion)
            {
                shoppingListProductTable = new Table(new float[] { 1, 1, 1, 1 }, true);
            }
            else
            {
                shoppingListProductTable = new Table(new float[] { 1, 1, 1 }, true);
            }
            shoppingListProductTable.SetBorderCollapse(BorderCollapsePropertyValue.SEPARATE);
            shoppingListProductTable.SetHorizontalBorderSpacing(10);
            shoppingListProductTable.SetWidth(PageSize.A4.GetWidth() + 12);

            var count = 0;
            foreach (var item in orderItems)
            {
                count++;
                var cell = new Cell();
                cell.SetBorder(Border.NO_BORDER);
                cell.SetPaddingLeft(newVersion ? 12 : 10);
                cell.SetPaddingRight(newVersion ? 12 : 10);
                cell.SetPaddingTop(3.2f);
                cell.SetHeight(newVersion ? 31 : 67);
                cell.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
                cell.SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                cell.SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE);

                var productName = item.Product.Name;
                productName = productName.Count() > 60 ? productName.Substring(0, 60) + "..." : productName;

                decimal result = 0;
                var weight = string.Empty;
                decimal unitPrice = GetSellPrice(item.Product);
                decimal requestedPrice = unitPrice;

                if (item.EquivalenceCoefficient > 0)
                {
                    result = (item.Quantity * 1000) / item.EquivalenceCoefficient;
                    requestedPrice = (result * unitPrice) / 1000;
                }
                else if (item.WeightInterval > 0)
                {
                    result = item.Quantity * item.WeightInterval;
                    requestedPrice = (unitPrice * result) / 1000;
                }
                else
                {
                    requestedPrice = unitPrice * item.Quantity;
                }

                if (result >= 1000)
                {
                    weight = (result / 1000).ToString("0.##") + " kg";
                }
                else if (result > 0)
                {
                    weight = result.ToString("0.##") + " gr";
                }

                var qty = (item.BuyingBySecondary && item.EquivalenceCoefficient > 0) || item.WeightInterval > 0 ? $"{weight}" : $"{item.Quantity.ToString()} pz";

                var product = new Paragraph(productName);
                var cantidad = new Paragraph($"{GetRouteName(item.Order.RouteId, routes)} - Orden #{item.Order.CustomOrderNumber} - {qty}");
                var propiedad = new Paragraph(string.IsNullOrWhiteSpace(item.SelectedPropertyOption) ? string.Empty : StringHelpers.FirstCharToUpper(item.SelectedPropertyOption));

                product.SetFixedLeading(10);

                cell.Add(product.SetBold().SetFontSize(newVersion ? 6 : 10));
                cell.Add(cantidad.SetFontSize(newVersion ? 6 : 8));
                cell.Add(propiedad.SetFontSize(newVersion ? 6 : 8));
                shoppingListProductTable.AddCell(cell);
            }

            var pendingCells = (shoppingListProductTable.GetNumberOfRows() * shoppingListProductTable.GetNumberOfColumns()) - count;
            if (pendingCells > 0)
            {
                for (int i = 0; i < pendingCells; i++)
                {
                    shoppingListProductTable.AddCell(new Cell().SetBorder(Border.NO_BORDER));
                }
            }
        }

        private void PrepareBuyerLabels(List<OrderItemBuyer> orderItemBuyer,
            DateTime selectedShippingDate,
            List<OrderItem> parsedOrderItems,
            List<Manufacturer> manufacturers,
            List<ProductMainManufacturer> productMainManufacturers,
            bool isTotal,
            bool newVersion)
        {
            if (orderItemBuyer.FirstOrDefault() == null) return;
            Customer buyer = null;
            if (!isTotal)
                buyer = _customerService.GetCustomerById(orderItemBuyer.FirstOrDefault().CustomerId);

            decimal cashMoney = 0;
            var buyerIds = orderItemBuyer.Select(x => x.CustomerId).Distinct().ToList();
            foreach (var buyerId in buyerIds)
            {
                var buyerMoney = OrderUtils.GetBuyerCashAndTransferAmount(parsedOrderItems,
                buyerId,
                orderItemBuyer,
                manufacturers,
                productMainManufacturers);
                cashMoney += RoundBuyerCashAmount(buyerMoney.Cash);
            }

            var cell = new Cell();
            cell.SetBorder(Border.NO_BORDER);
            cell.SetPaddingLeft(newVersion ? 12 : 10);
            cell.SetPaddingRight(newVersion ? 12 : 10);
            cell.SetPaddingTop(3.2f);
            cell.SetHeight(newVersion ? 31 : 67);
            cell.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
            cell.SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
            cell.SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE);

            var buyerName = buyer == null ? "TOTAL" : buyer.GetFullName();
            buyerName = buyerName.Count() > 60 ? buyerName.Substring(0, 60) + "..." : buyerName;

            var buyerParagraph = new Paragraph(buyerName);
            var shippingDate = new Paragraph($"{selectedShippingDate.ToString("dd-MM-yyyy")}");
            var amount = new Paragraph(cashMoney.ToString("C"));

            buyerParagraph.SetFixedLeading(10);

            cell.Add(buyerParagraph.SetBold().SetFontSize(newVersion ? 6 : 10));
            cell.Add(shippingDate.SetFontSize(newVersion ? 6 : 8));
            cell.Add(amount.SetFontSize(newVersion ? 6 : 8));
            shoppingListProductTable.AddCell(cell);
        }

        public string GetProductQuantity(OrderItem item, bool includeUnit = false)
        {
            var weight = string.Empty;
            decimal result = 0;
            if (item.EquivalenceCoefficient > 0)
            {
                result = (item.Quantity * 1000) / item.EquivalenceCoefficient;
            }
            else if (item.WeightInterval > 0)
            {
                result = item.Quantity * item.WeightInterval;
            }

            string unit = " pz";
            if (result >= 1000)
            {
                weight = (result / 1000).ToString("0.##");
                unit = " kg";
            }
            else if (result > 0)
            {
                weight = result.ToString("0.##");
                unit = " gr";
            }

            return (item.BuyingBySecondary || item.WeightInterval > 0 ? $"{weight}" : $"{item.Quantity.ToString()}") + (includeUnit ? unit : string.Empty);
        }

        private decimal RoundBuyerCashAmount(decimal costSum)
        {
            if (costSum == 0) return 0;
            costSum += 50;
            costSum = Math.Ceiling(costSum / 100) * 100;
            return costSum;
        }

        private void PrepareShoppingListByBuyer(IList<Order> orders, int buyerId, List<OrderItemBuyer> orderItemBuyer, List<OrderItem> parsedOrderItems)
        {
            var orderItemIds = orderItemBuyer.Where(x => x.CustomerId == buyerId).Select(x => x.OrderItemId);
            var filteredOrderItems = parsedOrderItems.Where(x => orderItemIds.Contains(x.Id)).ToList();
            currentBuyerId = buyerId;
            PrepareShoppingListNotes(filteredOrderItems.GroupBy(x => x.OrderId).Select(x => x.FirstOrDefault().Order).ToList());
            PrepareShoppingListByBuyerDetails(filteredOrderItems);
            PrepareShoppingList(filteredOrderItems, false, true, deliveryProducts: isSecondShippoingList);
            if (isSecondShippoingList)
                isSecondShippoingList = false;
            else
                isSecondShippoingList = true;
        }

        private void PrepareShoppingListByManufacturer(List<OrderItem> orderItems)
        {
            PrepareShoppingList(orderItems, false, false, false, true);
        }

        private void PrepareSimpleShoppingList(List<OrderItem> orderItems)
        {
            PrepareShoppingList(orderItems, false, false, false, true);
        }

        private void PrepareSecondaryShoppingListByRouteDetails(IList<Order> orders, int routeId)
        {
            secondaryShoppingListDetailsTable = new Table(6);
            secondaryShoppingListDetailsTable.SetWidth(PageSize.A4.GetWidth() - 30);

            Paragraph secondaryHeader4 = new Paragraph("Hora")
                       .SetBold()
                       .SetFontSize(10)
                       .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            secondaryShoppingListDetailsTable.AddHeaderCell(new Cell()
                         .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                         .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                         .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                         .Add(secondaryHeader4));

            Paragraph secondaryHeader1 = new Paragraph("Orden")
                       .SetBold()
                       .SetFontSize(10)
                       .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            secondaryShoppingListDetailsTable.AddHeaderCell(new Cell()
                         .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                         .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                         .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                         .Add(secondaryHeader1));

            Paragraph secondaryHeader2 = new Paragraph("Cliente")
                       .SetBold()
                       .SetFontSize(10)
                       .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            secondaryShoppingListDetailsTable.AddHeaderCell(new Cell()
                         .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                         .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                         .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                         .Add(secondaryHeader2));

            Paragraph secondaryHeader3 = new Paragraph("Dirección")
                       .SetBold()
                       .SetFontSize(10)
                       .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            secondaryShoppingListDetailsTable.AddHeaderCell(new Cell()
                         .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                         .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                         .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                         .Add(secondaryHeader3));

            Paragraph secondaryHeader6 = new Paragraph("Método de pago")
                       .SetBold()
                       .SetFontSize(10)
                       .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            secondaryShoppingListDetailsTable.AddHeaderCell(new Cell()
                         .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                         .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                         .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                         .Add(secondaryHeader6));

            Paragraph secondaryHeader5 = new Paragraph("Cant. de productos")
                       .SetBold()
                       .SetFontSize(10)
                       .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

            secondaryShoppingListDetailsTable.AddHeaderCell(new Cell()
                         .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                         .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                         .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                         .Add(secondaryHeader5));

            var routeOrders = orders.Where(x => x.RouteId == routeId).OrderBy(x => x.RouteDisplayOrder);
            foreach (var order in routeOrders)
            {
                Paragraph p4 = new Paragraph(order.SelectedShippingTime)
                    .SetFontSize(8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

                secondaryShoppingListDetailsTable.AddCell(new Cell()
                        .SetFontSize(8)
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(p4));

                Paragraph p1 = new Paragraph(order.CustomOrderNumber)
                       .SetFontSize(8)
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

                secondaryShoppingListDetailsTable.AddCell(new Cell()
                            .SetFontSize(8)
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(p1));

                Paragraph p2 = new Paragraph($"{order.ShippingAddress.FirstName} {order.ShippingAddress.LastName}")
                    .SetFontSize(8)
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

                secondaryShoppingListDetailsTable.AddCell(new Cell()
                .SetFontSize(8)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                .Add(p2));

                Paragraph p3 = new Paragraph($"{order.ShippingAddress.Address1} {order.ShippingAddress.Address2} {_addressAttributeFormatter.FormatAttributes(order.ShippingAddress.CustomAttributes, htmlEncode: false)}")
                    .SetFontSize(8)
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

                secondaryShoppingListDetailsTable.AddCell(new Cell()
                    .SetFontSize(8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(p3));

                Paragraph p6 = new Paragraph(order.PaymentStatus == PaymentStatus.Paid ? $"Pagado" : OrderUtils.GetPaymentOptionName(order.PaymentMethodSystemName))
                    .SetFontSize(8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

                secondaryShoppingListDetailsTable.AddCell(new Cell()
                .SetFontSize(8)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                .Add(p6));

                Paragraph p5 = new Paragraph(order.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService).Count.ToString())
                    .SetFontSize(8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                secondaryShoppingListDetailsTable.AddCell(new Cell()
                .SetFontSize(8)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                .Add(p5));
            }

            secondaryShoppingListDetailsTable.AddCell(new Cell(1, 6)
                    .SetPadding(10)
                    .SetBorder(Border.NO_BORDER)
                    .Add(new Paragraph()));
        }

        private void PrepareShoppingList(List<OrderItem> orderItems, bool provider, bool buyer = false, bool byRoute = false, bool byManufacturer = false, bool deliveryProducts = false, bool generalList = false)
        {
            var routes = _shippingRouteService.GetAll().ToList();
            var categories = _categoryService.GetAllCategories();

            if (!buyer && !byRoute)
            {
                shoppingListTitle = new Paragraph("Lista de compras");
                shoppingListTitle.SetFontSize(10);
                shoppingListTitle.SetBold();
                shoppingListTitle.SetTextAlignment(TextAlignment.CENTER);
            }

            shoppingListProductTable = new Table(byRoute ? 6 : 8);
            shoppingListProductTable.SetWidth(PageSize.A4.GetWidth() - 30);

            if (!deliveryProducts)
            {
                //var categoryGroup = orderItems
                //    .GroupBy(x => x.Product.ProductCategories.Where(y => y.Category.ParentCategoryId == 0 && !y.Category.Deleted).Select(y => y.Category).FirstOrDefault())
                //    .OrderBy(x => x.Key?.DisplayOrder)
                //    .ToList();

                var mainManufacturers = _productMainManufacturerService.GetAll().ToList();
                var manufacturerGroup = orderItems
                    .GroupBy(x => ProductUtils.GetMainManufacturer(x.Product.ProductManufacturers, mainManufacturers.Where(y => y.ProductId == x.ProductId).FirstOrDefault()))
                    .OrderBy(x => x.Key?.Name)
                    .ToList();

                foreach (var manufacturer in manufacturerGroup)
                {
                    List<IGrouping<Product, OrderItem>> productGroup = manufacturer
                        .Select(x => x)
                        .GroupBy(x => x.Product)
                        .OrderBy(x => x.Key?.ProductCategories.Where(y => y.Category.ParentCategoryId == 0 && !y.Category.Deleted).Select(y => y.Category).FirstOrDefault()?.DisplayOrder)
                        .ThenBy(x => x.Key?.ProductCategories.Where(y => y.Category.ParentCategoryId == 0 && !y.Category.Deleted).Select(y => y.Category).FirstOrDefault()?.Name)
                        .ThenBy(x => x.Key?.ProductCategories.Where(y => y.Category.ParentCategoryId != 0 && !y.Category.Deleted).Select(y => y.Category).FirstOrDefault()?.Name)
                        .ThenBy(x => x.Key.Name)
                        .ToList();

                    for (int i = 0; i < productGroup.Count(); i++)
                    {
                        Product product = productGroup[i].Key;
                        Category parentCategory = product.ProductCategories.Select(x => x.Category).Where(x => x.ParentCategoryId == 0 && !x.Deleted).OrderBy(x => x.DisplayOrder).FirstOrDefault();
                        Category childCategory = product.ProductCategories.Select(x => x.Category).Where(x => x.ParentCategoryId != 0 && !x.Deleted).OrderBy(x => x.DisplayOrder).FirstOrDefault();

                        Paragraph productsDetailsHeader1 = new Paragraph((parentCategory == null ? "Producto sin categoría padre" : parentCategory.Name) + " - " + (childCategory == null ? "Sin categoría hijo" : childCategory.Name))
                        .SetBold()
                        .SetFontSize(10)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                        Paragraph productsDetailsHeader2 = new Paragraph(generalList ? "Precio (kg/pz)" : "Costo (kg/pz)")
                            .SetBold()
                            .SetFontSize(8)
                            .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                        Paragraph productsDetailsHeader8 = new Paragraph("Pedido")
                            .SetBold()
                            .SetFontSize(8)
                            .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                        Paragraph productsDetailsHeader3 = new Paragraph("Ruta/Orden")
                            .SetBold()
                            .SetFontSize(8)
                            .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                        Paragraph productsDetailsHeader4 = new Paragraph("Specs")
                            .SetBold()
                            .SetFontSize(8)
                            .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                        Paragraph productsDetailsHeader5 = new Paragraph("Cantidad")
                            .SetBold()
                            .SetFontSize(8)
                            .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                        shoppingListProductTable.AddCell(new Cell()
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(productsDetailsHeader1));

                        shoppingListProductTable.AddCell(new Cell()
                                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(productsDetailsHeader2));

                        shoppingListProductTable.AddCell(new Cell()
                                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(productsDetailsHeader8));

                        shoppingListProductTable.AddCell(new Cell()
                                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(productsDetailsHeader3));

                        shoppingListProductTable.AddCell(new Cell()
                                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(productsDetailsHeader4));

                        shoppingListProductTable.AddCell(new Cell()
                                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(productsDetailsHeader5));

                        if (!byRoute)
                        {
                            Paragraph productsDetailsHeader6 = new Paragraph("Bodega")
                            .SetBold()
                            .SetFontSize(8)
                            .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                            Paragraph productsDetailsHeader7 = new Paragraph("Costo (kg/pz)")
                                .SetBold()
                                .SetFontSize(8)
                                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                            shoppingListProductTable.AddCell(new Cell()
                                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                    .Add(productsDetailsHeader6));

                            shoppingListProductTable.AddCell(new Cell()
                                    .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                    .Add(productsDetailsHeader7));
                        }

                        Paragraph productText1 = new Paragraph()
                        .SetMarginLeft(2)
                        .SetMarginRight(1)
                        .SetMaxWidth(200)
                        .SetFixedLeading(10)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetFontSize(8);

                        productText1.Add(new Text(product.Name));

                        if (!provider && !byManufacturer)
                        {
                            var mainManufacturerId = mainManufacturers.Where(x => x.ProductId == product.Id).Select(x => x.ManufacturerId).FirstOrDefault();
                            string manufacturers = string.Join(", ", ProductUtils.OrderManufacturers(product.ProductManufacturers, mainManufacturerId).Select(x => x.Manufacturer.Name).ToArray());
                            Text warehouseText = new Text("  (" + manufacturers + ")").SetFontSize(8);
                            productText1.Add(warehouseText);
                        }

                        IEnumerable<Order> ordersInGroup = productGroup[i].Where(x => x.ProductId == product.Id)
                            .Select(x => x.Order)
                            .OrderBy(x => x.RouteId)
                            .ThenByDescending(x => x.RouteDisplayOrder);

                        shoppingListProductTable.AddCell(new Cell(ordersInGroup.Count(), 1)
                            .SetPadding(0)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                            .SetMarginLeft(1)
                            .SetMarginRight(1)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(productText1));

                        foreach (var order in ordersInGroup)
                        {
                            //var item = order.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService).Where(x => x.ProductId == product.Id).FirstOrDefault();
                            var item = orderItems.Where(x => x.ProductId == product.Id && x.OrderId == order.Id).FirstOrDefault();
                            if (item == null)
                                continue;
                            var weight = string.Empty;
                            //decimal sellPrice = item.UnitPriceInclTax;
                            decimal result = 0;

                            string qty = string.Empty;
                            string sellPriceText = string.Empty;
                            string priceInclTax = string.Empty;

                            if (!generalList)
                            {
                                decimal unitPrice = GetSellPrice(item.Product);
                                decimal requestedPrice = unitPrice;

                                if (item.EquivalenceCoefficient > 0)
                                {
                                    result = (item.Quantity * 1000) / item.EquivalenceCoefficient;
                                    //requestedPrice = (1000 * unitPrice) / result;
                                    requestedPrice = (result * unitPrice) / 1000;
                                }
                                else if (item.WeightInterval > 0)
                                {
                                    result = item.Quantity * item.WeightInterval;
                                    requestedPrice = (unitPrice * result) / 1000;
                                }
                                else
                                {
                                    requestedPrice = unitPrice * item.Quantity;
                                }

                                if (result >= 1000)
                                {
                                    weight = (result / 1000).ToString("0.##") + " kg";
                                }
                                else if (result > 0)
                                {
                                    weight = result.ToString("0.##") + " gr";
                                }

                                sellPriceText = provider || byManufacturer ? "-" : "$" + unitPrice.ToString("0.##");
                                priceInclTax = provider || byManufacturer ? "-" : "$" + requestedPrice.ToString("0.##");
                            }
                            else
                            {
                                decimal sellPrice = item.UnitPriceInclTax;
                                if (item.EquivalenceCoefficient > 0)
                                {
                                    result = (item.Quantity * 1000) / item.EquivalenceCoefficient;
                                    sellPrice = (1000 * item.UnitPriceInclTax) / result;
                                }
                                else if (item.WeightInterval > 0)
                                {
                                    result = item.Quantity * item.WeightInterval;
                                    if (result > 0)
                                        sellPrice = (1000 * item.UnitPriceInclTax) / result;
                                }

                                if (result >= 1000)
                                {
                                    weight = (result / 1000).ToString("0.##") + " kg";
                                }
                                else if (result > 0)
                                {
                                    weight = result.ToString("0.##") + " gr";
                                }

                                sellPriceText = provider || byManufacturer ? "-" : "$" + sellPrice.ToString("0.##");
                                priceInclTax = provider || byManufacturer ? "-" : "$" + item.PriceInclTax.ToString("0.##");
                            }

                            qty = (item.BuyingBySecondary && item.EquivalenceCoefficient > 0) || item.WeightInterval > 0 ? $"{weight}" : $"{item.Quantity.ToString()} pz";

                            Paragraph productText2 = new Paragraph(sellPriceText)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                            .SetMarginRight(2)
                            .SetFontSize(8);

                            Paragraph productText8 = new Paragraph(priceInclTax)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                            .SetMarginRight(2)
                            .SetFontSize(8);

                            Paragraph productText3 = new Paragraph($"{GetRouteName(order.RouteId, routes)}/#{order.CustomOrderNumber}")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                            .SetMarginLeft(2)
                            .SetFontSize(8);

                            Paragraph productText4 = new Paragraph(string.IsNullOrWhiteSpace(item.SelectedPropertyOption) ? "-" : item.SelectedPropertyOption)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                            .SetMarginLeft(2)
                            .SetFontSize(8);

                            Paragraph productText5 = new Paragraph($"{qty}")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                            .SetMarginRight(2)
                            .SetFontSize(8);

                            shoppingListProductTable.AddCell(new Cell()
                            .SetPadding(0)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(productText2));

                            shoppingListProductTable.AddCell(new Cell()
                            .SetPadding(0)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(productText8));

                            shoppingListProductTable.AddCell(new Cell()
                            .SetPadding(0)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(productText3));

                            shoppingListProductTable.AddCell(new Cell()
                            .SetPadding(0)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(productText4));

                            shoppingListProductTable.AddCell(new Cell()
                            .SetPadding(0)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(productText5));

                            if (!byRoute)
                            {
                                Paragraph productText6 = new Paragraph("")
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetFontSize(8);

                                Paragraph productText7 = new Paragraph("")
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetFontSize(8);

                                shoppingListProductTable.AddCell(new Cell()
                                .SetPadding(0)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(productText6));

                                shoppingListProductTable.AddCell(new Cell()
                                .SetPadding(0)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(productText7));
                            }
                        }

                        var propertyGroups = productGroup[i].GroupBy(x => x.SelectedPropertyOption?.ToLower());
                        decimal qtyTotal = 0m;
                        decimal eqTotal = 0m;
                        bool onlyWeight = false;
                        foreach (var propertyGroup in propertyGroups)
                        {
                            Paragraph totalText = new Paragraph("TOTAL")
                            .SetBold()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                            .SetMarginLeft(5)
                            .SetFontSize(8);

                            Paragraph totalProperty = new Paragraph(string.IsNullOrWhiteSpace(propertyGroup.Key) ? "-" : propertyGroup.Key)
                            .SetBold()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                            .SetMarginLeft(5)
                            .SetFontSize(8);

                            var item = propertyGroup.FirstOrDefault();
                            var weight = string.Empty;
                            decimal result = 0;
                            if (item.EquivalenceCoefficient > 0)
                            {
                                result = (propertyGroup.Sum(x => x.Quantity) * 1000) / item.EquivalenceCoefficient;
                            }
                            else if (item.WeightInterval > 0)
                            {
                                result = propertyGroup.Sum(x => x.Quantity) * item.WeightInterval;
                            }

                            if (result >= 1000)
                            {
                                weight = " = " + (result / 1000).ToString("0.##") + " kg";
                            }
                            else if (result > 0)
                            {
                                weight = " = " + result.ToString("0.##") + " gr";
                            }

                            eqTotal += result;
                            qtyTotal += propertyGroup.Sum(x => x.Quantity);

                            onlyWeight = item.WeightInterval > 0;
                            var groupText = onlyWeight && !string.IsNullOrEmpty(weight) ? weight.Substring(2) : $"{propertyGroup.Sum(x => x.Quantity)} pz{weight}";
                            Paragraph totalPropertyValue = new Paragraph(groupText)
                            .SetBold()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                            .SetMarginRight(5)
                            .SetFontSize(8);

                            shoppingListProductTable.AddCell(new Cell()
                               .SetPadding(0)
                               .SetBorder(Border.NO_BORDER)
                               .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                               .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                               .Add(new Paragraph()));

                            shoppingListProductTable.AddCell(new Cell()
                            .SetPadding(0)
                            .SetBorder(Border.NO_BORDER)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(new Paragraph()));

                            shoppingListProductTable.AddCell(new Cell(1, 2)
                            .SetPadding(0)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(totalText));

                            shoppingListProductTable.AddCell(new Cell()
                            .SetPadding(0)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(totalProperty));

                            shoppingListProductTable.AddCell(new Cell()
                            .SetPadding(0)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(totalPropertyValue));

                            if (!byRoute)
                            {
                                shoppingListProductTable.AddCell(new Cell()
                                .SetPadding(0)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(new Paragraph()));

                                shoppingListProductTable.AddCell(new Cell()
                                .SetPadding(0)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(new Paragraph()));
                            }
                        }

                        if (propertyGroups.Count() > 1)
                        {
                            string finalWeight = string.Empty;
                            if (eqTotal >= 1000)
                            {
                                finalWeight = " = " + (eqTotal / 1000).ToString("0.##") + " kg";
                            }
                            else if (eqTotal > 0)
                            {
                                finalWeight = " = " + eqTotal.ToString("0.##") + " gr";
                            }

                            Paragraph finalTotalText = new Paragraph("TOTAL")
                                .SetBold()
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                                .SetMarginLeft(5)
                                .SetFontSize(8);

                            Paragraph finalTotalProperty = new Paragraph("-")
                                .SetBold()
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                                .SetMarginLeft(5)
                                .SetFontSize(8);

                            var groupText = onlyWeight && !string.IsNullOrEmpty(finalWeight) ? finalWeight.Substring(2) : $"{qtyTotal} pz{finalWeight}";
                            Paragraph finalTotalPropertyValue = new Paragraph(groupText)
                                .SetBold()
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                                .SetMarginRight(5)
                                .SetFontSize(8);

                            shoppingListProductTable.AddCell(new Cell()
                                   .SetPadding(0)
                                   .SetBorder(Border.NO_BORDER)
                                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                   .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                   .Add(new Paragraph()));

                            shoppingListProductTable.AddCell(new Cell()
                            .SetPadding(0)
                            .SetBorder(Border.NO_BORDER)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(new Paragraph()));

                            shoppingListProductTable.AddCell(new Cell(1, 2)
                            .SetPadding(0)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(finalTotalText));

                            shoppingListProductTable.AddCell(new Cell()
                            .SetPadding(0)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(finalTotalProperty));

                            shoppingListProductTable.AddCell(new Cell()
                            .SetPadding(0)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(finalTotalPropertyValue));

                            if (!byRoute)
                            {
                                shoppingListProductTable.AddCell(new Cell()
                                .SetPadding(0)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(new Paragraph()));

                                shoppingListProductTable.AddCell(new Cell()
                                .SetPadding(0)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(new Paragraph()));
                            }
                        }

                        shoppingListProductTable.AddCell(new Cell(1, 8)
                            .SetPadding(2)
                            .SetBorder(Border.NO_BORDER)
                            .Add(new Paragraph()));
                    }
                }
            }
            else
            {
                shoppingListProductTable = new Table(UnitValue.CreatePercentArray(new float[] { 0.7f, 0.7f, 2, 1 })).SetWidth(UnitValue.CreatePercentValue(100));
                var routeGroup = orderItems
                    .GroupBy(x => _shippingRouteService.GetAll().Where(y => y.Id == x.Order.RouteId).FirstOrDefault()?.RouteName)
                    .OrderBy(x => x.Key).ToList();
                shoppingListProductTable.AddCell(new Cell(1, 4)
                    .Add(new Paragraph("Productos a entregar"))
                    .SetBold()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetPaddingTop(15).SetBorder(Border.NO_BORDER));
                doc.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));
                var lastRoute = routeGroup.LastOrDefault();

                foreach (var orderItemsByRoute in routeGroup)
                {
                    Paragraph routTitle = new Paragraph(orderItemsByRoute.Key == null ? "Sin nombre de ruta" : orderItemsByRoute.Key
                        + " - " + _customerService.GetCustomerById(currentBuyerId).GetFullName())
                        .SetFontSize(10)
                        .SetBorder(Border.NO_BORDER)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    shoppingListProductTable.AddCell(new Cell(1, 4).Add(routTitle)
                    .SetBold().SetPaddingTop(5).SetBorder(Border.NO_BORDER));

                    var orderItemsByRouteFinal = orderItemsByRoute.OrderByDescending(x => x.Product.ProductSpecificationAttributes
                        .Where(y => y.SpecificationAttributeOption.Name.ToLower().Contains("nevera") ||
                        y.SpecificationAttributeOption.Name.ToLower().Contains("manojos")).FirstOrDefault()?.SpecificationAttributeOption.Name)
                        .ThenBy(x => x.Product.Name).ToList();

                    Paragraph productsDetailsHeader1 = new Paragraph("Orden")
                         .SetBold()
                         .SetFontSize(10)
                         .SetFontSize(8)
                         .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                    Paragraph productsDetailsHeader4 = new Paragraph("Cantidad")
                        .SetBold()
                        .SetFontSize(10)
                        .SetFontSize(8)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                    Paragraph productsDetailsHeader2 = new Paragraph("Producto")
                        .SetBold()
                        .SetFontSize(10)
                        .SetFontSize(8)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                    Paragraph productsDetailsHeader3 = new Paragraph("Specs")
                        .SetBold()
                        .SetFontSize(10)
                        .SetFontSize(8)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                    shoppingListProductTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(productsDetailsHeader1));

                    shoppingListProductTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(productsDetailsHeader4));

                    shoppingListProductTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(productsDetailsHeader2));

                    shoppingListProductTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(productsDetailsHeader3));

                    foreach (var orderItem in orderItemsByRouteFinal)
                    {
                        Paragraph productsDetails1 = new Paragraph("#" + orderItem.Order.Id.ToString())
                            .SetFontSize(8)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

                        decimal result = 0;
                        var weight = string.Empty;
                        decimal unitPrice = GetSellPrice(orderItem.Product);
                        decimal requestedPrice = unitPrice;

                        if (orderItem.EquivalenceCoefficient > 0)
                        {
                            result = (orderItem.Quantity * 1000) / orderItem.EquivalenceCoefficient;
                            requestedPrice = (result * unitPrice) / 1000;
                        }
                        else if (orderItem.WeightInterval > 0)
                        {
                            result = orderItem.Quantity * orderItem.WeightInterval;
                            requestedPrice = (unitPrice * result) / 1000;
                        }
                        else
                        {
                            requestedPrice = unitPrice * orderItem.Quantity;
                        }

                        if (result >= 1000)
                        {
                            weight = (result / 1000).ToString("0.##") + " kg";
                        }
                        else if (result > 0)
                        {
                            weight = result.ToString("0.##") + " gr";
                        }

                        var qty = (orderItem.BuyingBySecondary && orderItem.EquivalenceCoefficient > 0) || orderItem.WeightInterval > 0 ? $"{weight}" : $"{orderItem.Quantity.ToString()} pz";
                        Paragraph productsDetails4 = new Paragraph(qty)
                            .SetFontSize(8)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                        string attributeText = orderItem.Product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.Name.ToLower().Contains("nevera")).Any() ? " (N)" :
                            orderItem.Product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.Name.ToLower().Contains("manojos")).Any() ? " (M)" : string.Empty;

                        Paragraph productsDetails2 = new Paragraph(orderItem.Product.Name + attributeText)
                            .SetFontSize(8)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

                        Paragraph productsDetails3 = new Paragraph(string.IsNullOrWhiteSpace(orderItem.SelectedPropertyOption) ? "-" : orderItem.SelectedPropertyOption)
                            .SetFontSize(8)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

                        shoppingListProductTable.AddCell(new Cell()
                                .SetPadding(0)
                                .SetPaddingLeft(2)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(productsDetails1));

                        shoppingListProductTable.AddCell(new Cell()
                                .SetPadding(0)
                                .SetPaddingRight(2)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(productsDetails4));

                        shoppingListProductTable.AddCell(new Cell()
                                .SetPadding(0)
                                .SetPaddingLeft(2)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(productsDetails2));

                        shoppingListProductTable.AddCell(new Cell()
                                .SetPadding(0)
                                .SetPaddingLeft(2)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(productsDetails3));

                        //shoppingListProductTable.AddCell(new Cell(1, 8)
                        //    .SetPadding(2)
                        //    .SetBorder(Border.NO_BORDER)
                        //    .Add(new Paragraph()));
                    }
                    shoppingListProductTable.AddCell(new Cell(1, 4)
                        .Add(new Paragraph("FIRMA DEL RESPONSABLE DE LA RUTA"))
                        .SetBold()
                        .SetFontSize(10)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetPaddingTop(10).SetBorder(Border.NO_BORDER));

                    shoppingListProductTable.AddCell(new Cell(1, 4)
                        .SetFontSize(10)
                        .Add(new Paragraph("Yo, responsable de la ruta,  reconozco que recibí todos estos productos completos, de buena calidad, y en buen estado. Asumo la responsabilidad total y absoluta en caso de que alguno de estos productos no sea entregado al cliente."))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetPaddingBottom(20).SetBorder(Border.NO_BORDER));

                    shoppingListProductTable.AddCell(new Cell(1, 4)
                        .SetBorderRight(Border.NO_BORDER)
                        .SetBorderLeft(Border.NO_BORDER)
                        .SetBorderBottom(Border.NO_BORDER)
                        .SetMarginBottom(10)
                        .SetPaddingBottom(10)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                        .Add(new Paragraph("Nombre y firma del responsable de la ruta")));

                    doc.Add(shoppingListProductTable);
                    if (lastRoute != orderItemsByRoute)
                        doc.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));
                    shoppingListProductTable = new Table(UnitValue.CreatePercentArray(new float[] { 0.7f, 0.7f, 2, 1 })).SetWidth(UnitValue.CreatePercentValue(100));
                }
            }
        }

        private void PrepareShoppingListWithAttributes(IList<OrderItem> orderItems)
        {
            var routeGroup = orderItems
                .GroupBy(x => _shippingRouteService.GetAll().Where(y => y.Id == x.Order.RouteId).FirstOrDefault()?.RouteName)
                .OrderBy(x => x.Key).ToList();
            var lastGroup = routeGroup.LastOrDefault();
            foreach (var orderItemsByRoute in routeGroup)
            {
                doc.Add(shoppingListHeaderTable);

                shoppingListProductTable = new Table(UnitValue.CreatePercentArray(new float[] { 0.7f, 2, 1 })).SetWidth(UnitValue.CreatePercentValue(100));

                Paragraph routTitle = new Paragraph(string.IsNullOrEmpty(orderItemsByRoute.Key) ? "Sin ruta asignada" : orderItemsByRoute.Key)
                    .SetFontSize(10)
                    .SetBorder(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                shoppingListProductTable.AddCell(new Cell(1, 3).Add(routTitle)
                .SetBold().SetPaddingTop(5).SetBorder(Border.NO_BORDER));

                var orderItemsByRouteTemp = orderItemsByRoute.OrderBy(x => x.Order.CustomerId).ThenBy(x => x.OrderId).ToList();
                var orderItemsByRouteFinal = orderItemsByRouteTemp.GroupBy(x => x.OrderId).ToList();

                foreach (var orderItemsByOrder in orderItemsByRouteFinal)
                {
                    Paragraph orderTitle = new Paragraph("Pedido " + orderItemsByOrder.Key.ToString())
                       .SetFontSize(10)
                       .SetBorder(Border.NO_BORDER)
                       .SetMarginLeft(10)
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    shoppingListProductTable.AddCell(new Cell(1, 3).Add(orderTitle)
                    .SetBold().SetPaddingTop(5).SetBorder(Border.NO_BORDER));

                    Paragraph productsDetailsHeader4 = new Paragraph("Cantidad")
                        .SetBold()
                        .SetFontSize(8)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                    Paragraph productsDetailsHeader2 = new Paragraph("Producto")
                        .SetBold()
                        .SetFontSize(8)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                    Paragraph productsDetailsHeader3 = new Paragraph("Specs")
                        .SetBold()
                        .SetFontSize(8)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                    shoppingListProductTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(productsDetailsHeader4));

                    shoppingListProductTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(productsDetailsHeader2));

                    shoppingListProductTable.AddCell(new Cell()
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                            .Add(productsDetailsHeader3));

                    foreach (var orderItem in orderItemsByOrder)
                    {
                        decimal result = 0;
                        var weight = string.Empty;
                        decimal unitPrice = GetSellPrice(orderItem.Product);
                        decimal requestedPrice = unitPrice;

                        if (orderItem.EquivalenceCoefficient > 0)
                        {
                            result = (orderItem.Quantity * 1000) / orderItem.EquivalenceCoefficient;
                            requestedPrice = (result * unitPrice) / 1000;
                        }
                        else if (orderItem.WeightInterval > 0)
                        {
                            result = orderItem.Quantity * orderItem.WeightInterval;
                            requestedPrice = (unitPrice * result) / 1000;
                        }
                        else
                        {
                            requestedPrice = unitPrice * orderItem.Quantity;
                        }

                        if (result >= 1000)
                        {
                            weight = (result / 1000).ToString("0.##") + " kg";
                        }
                        else if (result > 0)
                        {
                            weight = result.ToString("0.##") + " gr";
                        }

                        var qty = (orderItem.BuyingBySecondary && orderItem.EquivalenceCoefficient > 0) || orderItem.WeightInterval > 0 ? $"{weight}" : $"{orderItem.Quantity.ToString()} pz";
                        Paragraph productsDetails4 = new Paragraph(qty)
                            .SetFontSize(8)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                        Paragraph productsDetails2 = new Paragraph(orderItem.Product.Name)
                            .SetFontSize(8)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

                        Paragraph productsDetails3 = new Paragraph(string.IsNullOrWhiteSpace(orderItem.SelectedPropertyOption) ? "-" : orderItem.SelectedPropertyOption)
                            .SetFontSize(8)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

                        shoppingListProductTable.AddCell(new Cell()
                                .SetPadding(0)
                                .SetPaddingRight(2)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(productsDetails4));

                        shoppingListProductTable.AddCell(new Cell()
                                .SetPadding(0)
                                .SetPaddingLeft(2)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(productsDetails2));

                        shoppingListProductTable.AddCell(new Cell()
                                .SetPadding(0)
                                .SetPaddingLeft(2)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .Add(productsDetails3));
                    }
                }
                shoppingListProductTable.AddCell(new Cell(1, 4)
                       .Add(new Paragraph("FIRMA DEL RESPONSABLE DE LA RUTA"))
                       .SetBold()
                       .SetFontSize(10)
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetPaddingTop(10).SetBorder(Border.NO_BORDER));

                shoppingListProductTable.AddCell(new Cell(1, 4)
                    .SetFontSize(10)
                    .Add(new Paragraph("Yo, responsable de la ruta, reconozco que recibí todos estos productos completos, de buena calidad, y en buen estado. Asumo la responsabilidad total y absoluta en caso de que alguno de estos productos no sea entregado al cliente."))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetPaddingBottom(20).SetBorder(Border.NO_BORDER));

                shoppingListProductTable.AddCell(new Cell(1, 4)
                    .SetBorderRight(Border.NO_BORDER)
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetBorderBottom(Border.NO_BORDER)
                    .SetMarginBottom(10)
                    .SetPaddingBottom(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)

                    .Add(new Paragraph("Nombre y firma del responsable de la ruta")));
                doc.Add(shoppingListProductTable);
                if (lastGroup != orderItemsByRoute)
                    doc.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));
            }
        }

        private decimal GetSellPrice(Product product)
        {
            if (product.ProductCost == 0)
            {
                var utility = product.ProductCategories
                    .Select(x => x.Category)
                    .Where(x => x.ParentCategoryId > 0)
                    .OrderBy(x => x.DisplayOrder)
                    .Select(x => x.PercentageOfUtility)
                    .FirstOrDefault();
                if (utility == 0) return product.Price;
                return product.Price * (1 - (utility / 100));
            }

            return product.ProductCost;
        }

        private string GetRouteName(int orderRouteId, List<ShippingRoute> routes)
        {
            ShippingRoute route = routes.Where(x => x.Id == orderRouteId).FirstOrDefault();
            if (route == null) return "N/A";
            string routeNumber = new String(route.RouteName.Where(Char.IsDigit).ToArray());
            return string.IsNullOrWhiteSpace(routeNumber) ? route.RouteName : "R" + routeNumber;
        }

        private string NumToLetterEsp(decimal number)
        {
            string num2Text;
            var value = Math.Truncate(number);

            if (value == 0) num2Text = "CERO";
            else if (value == 1) num2Text = "UNO";
            else if (value == 2) num2Text = "DOS";
            else if (value == 3) num2Text = "TRES";
            else if (value == 4) num2Text = "CUATRO";
            else if (value == 5) num2Text = "CINCO";
            else if (value == 6) num2Text = "SEIS";
            else if (value == 7) num2Text = "SIETE";
            else if (value == 8) num2Text = "OCHO";
            else if (value == 9) num2Text = "NUEVE";
            else if (value == 10) num2Text = "DIEZ";
            else if (value == 11) num2Text = "ONCE";
            else if (value == 12) num2Text = "DOCE";
            else if (value == 13) num2Text = "TRECE";
            else if (value == 14) num2Text = "CATORCE";
            else if (value == 15) num2Text = "QUINCE";
            else if (value < 20) num2Text = "DIECI" + NumToLetterEsp(value - 10);
            else if (value == 20) num2Text = "VEINTE";
            else if (value < 30) num2Text = "VEINTI" + NumToLetterEsp(value - 20);
            else if (value == 30) num2Text = "TREINTA";
            else if (value == 40) num2Text = "CUARENTA";
            else if (value == 50) num2Text = "CINCUENTA";
            else if (value == 60) num2Text = "SESENTA";
            else if (value == 70) num2Text = "SETENTA";
            else if (value == 80) num2Text = "OCHENTA";
            else if (value == 90) num2Text = "NOVENTA";
            else if (value < 100) num2Text = NumToLetterEsp(Math.Truncate(value / 10) * 10) + " Y " + NumToLetterEsp(value % 10);
            else if (value == 100) num2Text = "CIEN";
            else if (value < 200) num2Text = "CIENTO " + NumToLetterEsp(value - 100);
            else if ((value == 200) || (value == 300) || (value == 400) || (value == 600) || (value == 800)) num2Text = NumToLetterEsp(Math.Truncate(value / 100)) + "CIENTOS";
            else if (value == 500) num2Text = "QUINIENTOS";
            else if (value == 700) num2Text = "SETECIENTOS";
            else if (value == 900) num2Text = "NOVECIENTOS";
            else if (value < 1000) num2Text = NumToLetterEsp(Math.Truncate(value / 100) * 100) + " " + NumToLetterEsp(value % 100);
            else if (value == 1000) num2Text = "MIL";
            else if (value < 2000) num2Text = "MIL " + NumToLetterEsp(value % 1000);
            else if (value < 1000000)
            {
                num2Text = NumToLetterEsp(Math.Truncate(value / 1000)) + " MIL";
                if ((value % 1000) > 0)
                {
                    num2Text = num2Text + " " + NumToLetterEsp(value % 1000);
                }
            }
            else if (value == 1000000)
            {
                num2Text = "UN MILLON";
            }
            else if (value < 2000000)
            {
                num2Text = "UN MILLON " + NumToLetterEsp(value % 1000000);
            }
            else if (value < 1000000000000)
            {
                num2Text = NumToLetterEsp(Math.Truncate(value / 1000000)) + " MILLONES ";
                if ((value - Math.Truncate(value / 1000000) * 1000000) > 0)
                {
                    num2Text = num2Text + " " + NumToLetterEsp(value - Math.Truncate(value / 1000000) * 1000000);
                }
            }
            else if (value == 1000000000000) num2Text = "UN BILLÓNN";
            else if (value < 2000000000000) num2Text = "UN BILLÓN " + NumToLetterEsp(value - Math.Truncate(value / 1000000000000) * 1000000000000);
            else
            {
                num2Text = NumToLetterEsp(Math.Truncate(value / 1000000000000)) + " BILLONES";
                if ((value - Math.Truncate(value / 1000000000000) * 1000000000000) > 0)
                {
                    num2Text = num2Text + " " + NumToLetterEsp(value - Math.Truncate(value / 1000000000000) * 1000000000000);
                }
            }
            return num2Text;
        }

        [HttpGet]
        public IActionResult GetAllowedQuantities(int orderItemId)
        {
            var data = OrderUtils.GetAllowedQuantities(orderItemId, _orderService, 400);
            if (data == null) return NotFound();
            return Ok(data);
        }

        [HttpGet]
        public IActionResult GetNotDeliveredReasons()
        {
            var reasons = (Enum.GetValues(typeof(NotDeliveredReason)))
                .Cast<NotDeliveredReason>()
                .Select(x => new
                {
                    Text = EnumHelper.GetDisplayName(x),
                    Value = (int)x
                });

            return Ok(reasons);
        }

        //Reporte depedidos al día.
        [HttpPost]
        [Route("/Admin/Order/ReportShippingForDay")]
        public IActionResult ReportShippingForDay()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var day = DateTime.Now.Date;
            var filteredOrders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate == day).ToList();
            var pedidos = OrderUtils.GetPedidosGroupByList(filteredOrders);
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Numero de pedido";
                    worksheet.Cells[row, 2].Value = "Nombre de quien recibe";
                    worksheet.Cells[row, 3].Value = "Teléfono";
                    //worksheet.Cells[row, 4].Value = "Total de las compras hechas por el usuario";

                    foreach (var group in pedidos)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = string.Join(",", group.Select(x => x.CustomOrderNumber));
                        worksheet.Cells[row, 2].Value = group.Select(x => x.ShippingAddress.FirstName).FirstOrDefault() + " " + group.Select(x => x.ShippingAddress.LastName).FirstOrDefault();
                        worksheet.Cells[row, 3].Value = group.Select(x => x.ShippingAddress.PhoneNumber).FirstOrDefault();
                        //worksheet.Cells[row, 4].Value = ordersOfClient.Sum(x => x.OrderTotal);
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"{DateTime.Now.ToString("dd-MM-yyyy")}_Reporte_de_pedidos_diarios.xlsx");
            }
        }

        [HttpPost]
        public IActionResult UpdateOrderItem(UpdateOrderItemDto dto)
        {
            OrderItem orderItem = _orderService.GetOrderItemById(dto.OrderItemId);
            if (orderItem == null) return NotFound();
            Order order = orderItem.Order;
            decimal currentQuantity = OrderUtils.GetTotalQuantity(orderItem.Quantity, orderItem.EquivalenceCoefficient, orderItem.BuyingBySecondary, orderItem.WeightInterval);
            string currentUnit = OrderUtils.GetProductRequestedUnit(orderItem.EquivalenceCoefficient, orderItem.BuyingBySecondary, orderItem.WeightInterval);
            decimal currentPriceInclTax = orderItem.PriceInclTax;
            decimal currentPriceExclTax = orderItem.PriceExclTax;
            decimal currentOrderTotal = order.OrderTotal;

            bool afterDeliver = DateTime.Now.Date >= order.SelectedShippingDate.Value;

            Customer customer = _workContext.CurrentCustomer;

            if (dto.RawQuantity > 0)
            {
                orderItem.Quantity = dto.RawQuantity;
                var updatedPrice = OrderUtils.GetUpdatedPrice(orderItem, currentQuantity);
                orderItem.PriceInclTax = updatedPrice;
                orderItem.PriceExclTax = _taxService.GetProductPrice(orderItem.Product, orderItem.PriceInclTax, false, customer, out _);
                if (orderItem.EquivalenceCoefficient > 0 || orderItem.WeightInterval > 0)
                {
                    orderItem.UnitPriceInclTax = orderItem.PriceInclTax;
                    orderItem.UnitPriceExclTax = orderItem.PriceExclTax;
                }
                decimal priceDifferenceInclTax = orderItem.PriceInclTax - currentPriceInclTax;
                decimal priceDifferenceExclTax = orderItem.PriceExclTax - currentPriceExclTax;
                order.OrderSubtotalInclTax += priceDifferenceInclTax;
                order.OrderSubtotalExclTax += priceDifferenceExclTax;
                order.OrderTotal += priceDifferenceInclTax;
                string balanceNote = string.Empty;
                if (!(order.OrderTotal > 0))
                {
                    var difference = order.OrderTotal * -1;
                    order.OrderTotal = 0;
                    if (order.CustomerBalanceUsedAmount > 0)
                    {
                        order.CustomerBalanceUsedAmount -= difference;
                        balanceNote = $". Se regresaron {difference:C} MXN de saldo al cliente.";
                    }
                }

                decimal newQuantity = OrderUtils.GetTotalQuantity(orderItem.Quantity, orderItem.EquivalenceCoefficient, orderItem.BuyingBySecondary, orderItem.WeightInterval);
                string newUnit = OrderUtils.GetProductRequestedUnit(orderItem.EquivalenceCoefficient, orderItem.BuyingBySecondary, orderItem.WeightInterval);

                OrderItemLog orderItemLog = new OrderItemLog()
                {
                    CustomerId = _workContext.CurrentCustomer.Id,
                    NewPrice = orderItem.PriceInclTax,
                    NewQuantity = orderItem.Quantity,
                    OrderItemId = orderItem.Id,
                    OrderId = orderItem.OrderId,
                    ProductId = orderItem.ProductId,
                    OriginalPrice = currentPriceInclTax,
                    OriginalQuantity = currentQuantity,
                };
                _orderItemLogService.Insert(orderItemLog);

                order.OrderNotes.Add(new OrderNote()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    DisplayToCustomer = false,
                    Note = $"El usuario {customer.Email} ({customer.Id}) modificó la cantidad {(afterDeliver ? "entregada" : "")} del producto '{orderItem.Product.Name}' de {currentQuantity} {currentUnit} a {newQuantity} {newUnit}. Se ajustó el precio a pagar del producto de {currentPriceInclTax} a {orderItem.PriceInclTax} y el total de la orden de {currentOrderTotal} a {order.OrderTotal}{balanceNote}",
                    CustomerId = _workContext.CurrentCustomer.Id
                });
            }
            else if (dto.NotWanted)
            {
                order.OrderSubtotalInclTax -= orderItem.PriceInclTax;
                order.OrderSubtotalExclTax -= orderItem.PriceExclTax;
                order.OrderTotal -= orderItem.PriceInclTax;
                string balanceNote = string.Empty;
                if (!(order.OrderTotal > 0))
                {
                    var difference = order.OrderTotal * -1;
                    order.OrderTotal = 0;
                    if (order.CustomerBalanceUsedAmount > 0)
                    {
                        order.CustomerBalanceUsedAmount -= difference;
                        balanceNote = $". Se regresaron {difference:C} MXN de saldo al cliente.";
                    }
                }

                order.OrderNotes.Add(new OrderNote()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    DisplayToCustomer = false,
                    Note = $"El usuario {customer.Email} ({customer.Id}) eliminó el producto '{orderItem.Product.Name}' de la orden. El total de la orden pasó de {currentOrderTotal} a {order.OrderTotal}{balanceNote}",
                    CustomerId = _workContext.CurrentCustomer.Id
                });

                _orderService.DeleteOrderItem(orderItem);
            }
            else
            {
                NotDeliveredOrderItem notDeliveredOrderItem = new NotDeliveredOrderItem()
                {
                    AttributeDescription = orderItem.AttributeDescription,
                    DiscountAmountExclTax = orderItem.DiscountAmountExclTax,
                    AttributesXml = orderItem.AttributesXml,
                    DiscountAmountInclTax = orderItem.DiscountAmountInclTax,
                    BuyingBySecondary = orderItem.BuyingBySecondary,
                    NotDeliveredReason = dto.NotDeliveredReason ?? EnumHelper.GetDisplayName((NotDeliveredReason)dto.NotDeliveredReasonId),
                    EquivalenceCoefficient = orderItem.EquivalenceCoefficient,
                    ItemWeight = orderItem.ItemWeight,
                    NotDeliveredReasonId = dto.NotDeliveredReasonId,
                    OrderId = orderItem.OrderId,
                    OriginalProductCost = orderItem.OriginalProductCost,
                    PriceExclTax = orderItem.PriceExclTax,
                    PriceInclTax = orderItem.PriceInclTax,
                    ProductId = orderItem.ProductId,
                    Quantity = orderItem.Quantity,
                    ReportedByUserId = customer.Id,
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
                string balanceNote = string.Empty;
                if (!(order.OrderTotal > 0))
                {
                    var difference = order.OrderTotal * -1;
                    order.OrderTotal = 0;
                    if (order.CustomerBalanceUsedAmount > 0)
                    {
                        order.CustomerBalanceUsedAmount -= difference;
                        balanceNote = $". Se regresaron {difference:C} MXN de saldo al cliente.";
                    }
                }

                order.OrderNotes.Add(new OrderNote()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    DisplayToCustomer = false,
                    Note = $"El usuario {customer.Email} ({customer.Id}) reportó el producto '{orderItem.Product.Name}' como no entregado. El total de la orden pasó de {currentOrderTotal} a {order.OrderTotal}{balanceNote}",
                    CustomerId = _workContext.CurrentCustomer.Id
                });

                _orderService.DeleteOrderItem(orderItem);
            }
            _orderService.UpdateOrder(order);

            if (order.OrderItems.Count == 0 && order.PaymentStatus != Nop.Core.Domain.Payments.PaymentStatus.Paid)
            {
                _orderProcessingService.CancelOrder(order, false);
                order.OrderNotes.Add(new OrderNote()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    DisplayToCustomer = false,
                    Note = $"La orden fue cancelada automáticamente por el sistema debido a que todos sus productos se reportaron como no entregados."
                });
            }
            return Ok();
        }

        [HttpGet]
        public IActionResult GetSuspiciousOrders()
        {
            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);
            var ordersTodayTomorrow = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate == tomorrow || x.SelectedShippingDate == today).ToList();
            var suspiciousOrders = new List<SuspiciousOrder>();
            var customerIds = ordersTodayTomorrow.Select(x => x.CustomerId).Distinct().ToList();
            var customersOrders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => customerIds.Contains(x.CustomerId))
                .ToList();
            foreach (var order in ordersTodayTomorrow)
            {
                var customerOrders = customersOrders.Where(x => x.CustomerId == order.CustomerId).ToList();
                if (customerOrders.Count >= 4) continue;
                var elements = OrderUtils.GetOrderSuspiciousElements(order, customerOrders, _customerService);
                if (elements.Count > 0)
                {
                    suspiciousOrders.Add(new SuspiciousOrder()
                    {
                        OrderId = order.Id,
                        OrderCustomOrderNumber = order.CustomOrderNumber,
                        SelectedShippingDate = order.SelectedShippingDate.Value.ToString("dd-MM-yyyy"),
                        SuspiciousElements = elements
                    });
                }
            }

            return Ok(suspiciousOrders);
        }

        public IActionResult FirstOrdersDates()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/FirstOrderList/DateList.cshtml");
        }

        [HttpPost]
        public IActionResult ListOrderDatesData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var orderDates = _orderService.GetAllOrdersQuery()
                .Select(x => x.SelectedShippingDate.Value)
                .Distinct()
                .OrderByDescending(x => x);
            var pagedList = new PagedList<DateTime>(orderDates, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    Date = x.ToString("dd-MM-yyyy"),
                }).ToList(),
                Total = orderDates.Count()
            };

            return Ok(gridModel);
        }

        public IActionResult FirstOrderList(string date)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/FirstOrderList/OrderList.cshtml", date);
        }

        [HttpPost]
        public IActionResult FirstOrderListData(DataSourceRequest command, string date)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var controlDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var customerIdsWithPrevOrders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate < controlDate)
                .Select(x => x.CustomerId)
                .Distinct()
                .ToList();
            var firstOrdersOfDate = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate == controlDate)
                .Select(x => new
                {
                    x.Id,
                    x.RouteId,
                    x.CustomerId
                })
                .ToList()
                .Where(x => !customerIdsWithPrevOrders.Contains(x.CustomerId))
                .ToList();
            var routes = _shippingRouteService.GetAll().ToList();
            var customerIds = firstOrdersOfDate.Select(x => x.CustomerId).ToList();
            var customers = _customerService.GetAllCustomersQuery().Where(x => customerIds.Contains(x.Id)).ToList();

            var gridModel = new DataSourceResult
            {
                Data = firstOrdersOfDate.Select(x => new
                {
                    OrderId = x.Id,
                    CustomerName = customers.Where(y => y.Id == x.CustomerId).FirstOrDefault().GetFullName(),
                    RouteName = x.RouteId == 0 ? "Sin ruta asignada" : routes.Where(y => y.Id == x.RouteId).FirstOrDefault()?.RouteName,
                    RouteId = x.RouteId
                }).OrderBy(x => x.RouteId).ThenBy(x => x.OrderId).ToList(),
                Total = firstOrdersOfDate.Count()
            };

            return Ok(gridModel);
        }
    }

    namespace IText.EventHandler
    {
        public class NewPageHandler : IEventHandler
        {
            public static DateTime ParsedDate { get; set; }
            public static string CurrentUserEmail { get; set; }

            public Rectangle pageSize;
            public float x;
            public float y;
            public int side = 20;
            PdfCanvas pdfCanvas;
            public int fontSize = 10;

            public void HandleEvent(Event @event)
            {
                PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
                PdfDocument pdf = docEvent.GetDocument();
                PdfPage page = docEvent.GetPage();
                int pageNumber = pdf.GetPageNumber(page);
                pageSize = page.GetPageSize();

                x = pageSize.GetWidth() - 40;
                y = pageSize.GetBottom() + 30;

                pdfCanvas = new PdfCanvas(
                    page.NewContentStreamBefore(), page.GetResources(), pdf);

                Canvas canvas = new Canvas(pdfCanvas, pdf, pageSize);
                Style style = new Style();
                style.SetFontSize(fontSize);
                Paragraph p = new Paragraph()
                    .AddStyle(style)
                    .Add("Fecha de entrega: ").Add(ParsedDate.Date.ToString("dd/MM/yyyy"))
                    .Add(" | Creado por: ").Add(CurrentUserEmail).Add(" el ").Add(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"))
                    .Add(" | Página ").Add(pageNumber.ToString()).Add(" de");

                canvas.ShowTextAligned(p, x, y, TextAlignment.RIGHT);
            }

            public void WriteTotal(PdfDocument pdf)
            {
                if (pdfCanvas == null) return;
                for (int i = 1; i < pdf.GetNumberOfPages() + 1; i++)
                {
                    Style style = new Style();
                    style.SetFontSize(fontSize);
                    Paragraph p = new Paragraph()
                        .AddStyle(style)
                        .Add(pdf.GetNumberOfPages().ToString());
                    Canvas canvas = new Canvas(pdf.GetPage(i), pageSize);
                    canvas.ShowTextAligned(p,
                        x + 2, y, TextAlignment.LEFT);
                }
                pdfCanvas.Release();
            }
        }
    }

    public class AllOrderItemsModel
    {
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
    }

    public class AllReportsModel
    {
        public int OrderItemId { get; set; }
        public decimal UnitCost { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }

    public class BuyerAmountModel
    {
        public int BuyerId { get; set; }
        public int ManufacturerId { get; set; }
        public decimal Cash { get; set; }
        public decimal Transfer { get; set; }
        public decimal Card { get; set; }
    }

    public class BuyersTotals
    {
        public string Name { get; set; }
        public string Products { get; set; }
        public string Budget { get; set; }
    }

    public class RoundedAmount
    {
        public string AmountString { get; set; }
        public decimal Amount { get; set; }
    }

    public class SuspiciousOrder
    {
        public int OrderId { get; set; }
        public string OrderCustomOrderNumber { get; set; }
        public string SelectedShippingDate { get; set; }
        public List<string> SuspiciousElements { get; set; }
    }

    public class ManufacturerOrderItemModel
    {
        public OrderItem OrderItem { get; set; }
        public Manufacturer MainManufacturer { get; set; }
    }
}