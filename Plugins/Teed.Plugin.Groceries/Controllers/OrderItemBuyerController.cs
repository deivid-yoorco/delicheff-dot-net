using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Teed.Plugin.Groceries.Domain.OrderItemBuyers;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class OrderItemBuyerController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly OrderItemBuyerService _orderItemBuyerService;
        private readonly BuyerListDownloadService _buyerListDownloadService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICategoryService _categoryService;
        private readonly IWorkContext _workContext;
        private readonly SupermarketBuyerService _supermarketBuyerService;
        private readonly ManufacturerBuyerService _manufacturerBuyerService;
        private readonly ProductMainManufacturerService _productMainManufacturerService;

        Table dataTable;
        Document doc;
        PageSize ps;

        public OrderItemBuyerController(IPermissionService permissionService, IOrderService orderService, ICustomerService customerService,
            OrderItemBuyerService orderItemBuyerService, IManufacturerService manufacturerService,
            ICategoryService categoryService, BuyerListDownloadService buyerListDownloadService,
            IWorkContext workContext, SupermarketBuyerService supermarketBuyerService,
            ManufacturerBuyerService manufacturerBuyerService, ProductMainManufacturerService productMainManufacturerService)
        {
            _permissionService = permissionService;
            _orderService = orderService;
            _customerService = customerService;
            _orderItemBuyerService = orderItemBuyerService;
            _manufacturerService = manufacturerService;
            _categoryService = categoryService;
            _buyerListDownloadService = buyerListDownloadService;
            _workContext = workContext;
            _supermarketBuyerService = supermarketBuyerService;
            _manufacturerBuyerService = manufacturerBuyerService;
            _productMainManufacturerService = productMainManufacturerService;
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderItemBuyer))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/OrderItemBuyer/List.cshtml");
        }

        public IActionResult AssignBuyers(string date, bool byUnassigned = false, int buyerId = 0, int manufacturerId = 0)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderItemBuyer))
                return AccessDeniedView();

            OrderItemCustomerModel model = GetOrderItemCustomerModel(date, true, byUnassigned, buyerId, manufacturerId);

            model.Buyers.Insert(0, new ForSelect() { Value = "0", Text = "No asignado" });

            ViewBag.BuyerId = buyerId;
            ViewBag.ManufacturerId = manufacturerId;

            return View("~/Plugins/Teed.Plugin.Groceries/Views/OrderItemBuyer/AssignBuyers.cshtml", model);
        }

        private OrderItemCustomerModel GetOrderItemCustomerModel(string date,
            bool withCategories,
            bool byUnassigned = false,
            int buyerId = 0,
            int manufacturerId = 0)
        {
            var categories = _categoryService.GetAllCategories();
            var dateDelivery = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var role = _customerService.GetCustomerRoleBySystemName("buyer");
            var buyers = _customerService.GetAllCustomers(customerRoleIds: new int[] { role.Id });

            var orderItems = GetOrderItems(dateDelivery).ToList();

            var mainManufacturers = _productMainManufacturerService.GetAll().ToList();

            var products = orderItems.GroupBy(x => x.Product);
            int[] orderItemsIds = orderItems.Select(x => x.Id).ToArray();
            var assignedProducts = _orderItemBuyerService.GetAll().Where(x => orderItemsIds.Contains(x.OrderItemId)).ToList();

            var assignedProductsByBuyer = assignedProducts.GroupBy(x => x.CustomerId)/*.Where(x => x.Key > 0)*/.ToList();
            var assignedProductsByManufacturers = orderItems.Where(y => assignedProducts.Select(z => z.OrderItemId).Contains(y.Id))
                                                            .GroupBy(y => ProductUtils.GetMainManufacturer(y.Product?.ProductManufacturers, mainManufacturers.Where(z => z.ProductId == y.ProductId).FirstOrDefault()));

            var manufacturers = orderItems.Select(x => x.Product.ProductManufacturers.Where(y => !y.Manufacturer.Deleted).Select(y => y.Manufacturer)).SelectMany(y => y).Distinct().ToList();

            var buyerTable = assignedProductsByBuyer.Select(x =>
            {
                var buyer = buyers.Where(y => y.Id == x.Key).FirstOrDefault();
                return new ProductsByBuyerTable()
                {
                    Buyer = x.Key == 0 ? "No asignado" : buyer == null ? $"Usuario no repartidor o no activo ({x.Key})" : buyer.GetFullName(),
                    BuyerId = x.Key == 0 ? 0 : buyer == null ? x.Key : buyer.Id,
                    TotalProducts = orderItems.Where(y => x.Select(z => z.OrderItemId).Contains(y.Id)).GroupBy(y => y.ProductId).Count(),
                    ProductsByManufacturer = string.Join(", ", orderItems.Where(y => x.Select(z => z.OrderItemId).Contains(y.Id))
                                                                     .GroupBy(y => ProductUtils.GetMainManufacturer(y.Product?.ProductManufacturers, mainManufacturers.Where(z => z.ProductId == y.ProductId).FirstOrDefault()))
                                                                     .Select(y => (y.Key == null ? "Sin fabricante" : y.Key.Name) + " (" + y.GroupBy(z => z.ProductId).Count() + ")")),
                    CorporateCard = RoundBuyerCashAmount(GetBuyerCorporateCardAmount(orderItems.Select(y => y.Order).ToList(), x.Key, x.Select(y => y).ToList())),
                    Cash = RoundBuyerCashAmount(GetBuyerCash(orderItems.Select(y => y.Order).ToList(), x.Key, x.Select(y => y).ToList())),
                    Transfer = GetBuyerTransferAmount(orderItems.Select(y => y.Order).ToList(), x.Key, x.Select(y => y).ToList()),
                };
            }).OrderBy(x => x.Buyer).ToList();

            var manufacturerTable = assignedProductsByManufacturers.Select(x => new ProductsByManufacturerTable()
            {
                Manufacturer = (x.Key == null ? "Sin fabricante" : x.Key.Name),
                ManufacturerId = (x.Key == null ? 0 : x.Key.Id),
                TotalProducts = x.Select(y => y).GroupBy(y => y.ProductId).Count(),
                BuyerIds = assignedProducts.Where(y => x.Select(z => z.Id).Contains(y.OrderItemId)).GroupBy(y => y.CustomerId).Select(y => y.Key).ToList(),
                Cash = x.Key == null || (!x.Key.IsPaymentWhithTransfer && !x.Key.IsPaymentWhithCorporateCard) ? assignedProducts.Where(y => x.Select(z => z.Id).Contains(y.OrderItemId)).DefaultIfEmpty().Sum(y => y.Cost) : 0,
                Transfer = x.Key != null && x.Key.IsPaymentWhithTransfer ? assignedProducts.Where(y => x.Select(z => z.Id).Contains(y.OrderItemId)).DefaultIfEmpty().Sum(y => y.Cost) : 0,
                CorporateCard = x.Key != null && x.Key.IsPaymentWhithCorporateCard ? assignedProducts.Where(y => x.Select(z => z.Id).Contains(y.OrderItemId)).DefaultIfEmpty().Sum(y => y.Cost) : 0
            }).OrderBy(x => x.Manufacturer).ToList();

            var superMarkedBuyer = _supermarketBuyerService.GetAll().Where(x => x.ShippingDate == dateDelivery && x.Deleted == false).Select(x => x.BuyerId).FirstOrDefault();

            var model = new OrderItemCustomerModel()
            {
                ProductsByBuyerTable = buyerTable,
                ProductsByManufacturerTable = manufacturerTable,
                Date = dateDelivery,
                Buyers = buyers.Select(x => new ForSelect()
                {
                    Value = x.Id.ToString(),
                    Text = x.GetFullName(),
                    TotalProducts = assignedProducts.Where(y => y.CustomerId == x.Id).Count(),
                }).OrderBy(x => x.Text).ToList(),
                Manufacturers = manufacturers.Select(x => new ForSelect()
                {
                    Value = x == null ? "0" : x.Id.ToString(),
                    Text = x == null ? "Sin fabricante" : x.Name,
                    TotalProducts = orderItems.Where(y => ProductUtils.GetMainManufacturer(y.Product?.ProductManufacturers, mainManufacturers.Where(z => y.ProductId == z.ProductId).FirstOrDefault())?.Id == x.Id).Count()
                }).OrderBy(x => x.Text).ToList(),
                SupermarketBuyer = superMarkedBuyer
            };

            if (withCategories)
            {
                // CHECK ORDER BY'S
                if (manufacturerId > 0)
                {
                    orderItems = orderItems.Where(x => ProductUtils.GetMainManufacturer(x.Product?.ProductManufacturers, mainManufacturers.Where(z => x.ProductId == z.ProductId).FirstOrDefault()).Id == manufacturerId).ToList();
                    var ordersOfManufacturerIds = orderItems.Select(x => x.Id).ToList();
                    assignedProducts = assignedProducts.Where(x => ordersOfManufacturerIds.Contains(x.OrderItemId)).ToList();
                }
                if (byUnassigned)
                {
                    assignedProducts = assignedProducts.Where(x => x.CustomerId == 0).ToList();
                    var ordersOfBuyerIds = assignedProducts.Select(x => x.OrderItemId).ToList();
                    orderItems = orderItems.Where(x => ordersOfBuyerIds.Contains(x.Id)).ToList();
                }
                else
                {
                    if (buyerId > 0)
                    {
                        assignedProducts = assignedProducts.Where(x => x.CustomerId == buyerId).ToList();
                        var ordersOfBuyerIds = assignedProducts.Select(x => x.OrderItemId).ToList();
                        orderItems = orderItems.Where(x => ordersOfBuyerIds.Contains(x.Id)).ToList();
                    }
                }
                products = orderItems.GroupBy(x => x.Product);
                var productsByCategory = products.GroupBy(x => x.Key.ProductCategories.Where(y => y.Category.ParentCategoryId != 0).Select(y => y.Category).FirstOrDefault());

                model.Categories = productsByCategory.ToList().Select(x => new CategoryDataModel()
                {
                    Category = x.Key == null ? "Sin categoría padre - Producto sin categoría hijo" : categories.Where(y => y.Id == x.Key.ParentCategoryId).FirstOrDefault()?.Name + " - " + x.Key.Name,
                    CategoryId = x.Key == null ? 0 : x.Key.Id,
                    Products = x.Select(y => new ProductDataModel()
                    {
                        Id = y.Key.Id,
                        Name = y.Key.Name,
                        Manufacturer = (new List<Manufacturer>() { ProductUtils.GetMainManufacturer(y.Key.ProductManufacturers, mainManufacturers.Where(z => z.ProductId == y.Key.Id).FirstOrDefault()) }).Where(z => z != null).Select(z => new ManufacturerDataModel() { Id = z.Id, Name = z.Name }).FirstOrDefault(),
                        SelectedBuyerId = assignedProducts.Where(z => y.Select(q => q.Id).Contains(z.OrderItemId)).Select(z => z.CustomerId).FirstOrDefault()
                    }).ToList()
                }).OrderBy(x => x.Category).ToList();
            }

            model.AllowDownload = _buyerListDownloadService.GetAll().Where(x => x.OrderShippingDate == dateDelivery).Select(x => x.AllowDownload).FirstOrDefault();
            model.AllowReport = _buyerListDownloadService.GetAll().Where(x => x.OrderShippingDate == dateDelivery).Select(x => x.AllowReport).FirstOrDefault() ?? false;

            var manufacturerBuyersQuery = _manufacturerBuyerService.GetAll();
            var allManufacturers = _manufacturerService.GetAllManufacturers().Where(x => !x.Deleted);
            model.PendingManufacturerBuyer = allManufacturers.Count() > manufacturerBuyersQuery.Count() || manufacturerBuyersQuery.Where(x => x.BuyerId == 0).Any();

            return model;
        }

        [HttpGet]
        public virtual IActionResult ExportDataTables(string date)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderItemBuyer))
                return AccessDeniedView();

            var model = GetOrderItemCustomerModel(date, false);

            MemoryStream stream = new MemoryStream();
            PdfWriter pdfWriter = new PdfWriter(stream);
            PdfDocument pdfDoc = new PdfDocument(pdfWriter);
            ps = pdfDoc.GetDefaultPageSize();
            doc = new Document(pdfDoc, PageSize.LETTER, false);
            doc.SetMargins(20, 20, 20, 20);

            // BUYERS TABLE
            dataTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 0.5f, 2, 0.6f, 0.6f })).SetWidth(UnitValue.CreatePercentValue(100));

            dataTable.AddCell(new Cell(1, 5)
                .SetBold()
                .SetFontSize(15)
                .SetBorder(Border.NO_BORDER)
                .Add(new Paragraph("Asignación de compradores a la fecha de entrega " + date)));

            dataTable.AddCell(new Cell()
                .SetBold()
                .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                .Add(new Paragraph("Comprador")));

            dataTable.AddCell(new Cell()
                .SetBold()
                .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                .Add(new Paragraph("Total de productos")));

            dataTable.AddCell(new Cell()
                .SetBold()
                .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                .Add(new Paragraph("Productos por fabricante")));

            dataTable.AddCell(new Cell()
                .SetBold()
                .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                .Add(new Paragraph("Efectivo")));

            dataTable.AddCell(new Cell()
                .SetBold()
                .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                .Add(new Paragraph("Transferencia")));

            foreach (var buyer in model.ProductsByBuyerTable)
            {
                dataTable.AddCell(new Cell()
                    .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                    .Add(new Paragraph(buyer.Buyer).SetFixedLeading(10)));

                dataTable.AddCell(new Cell()
                    .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                    .Add(new Paragraph(buyer.TotalProducts.ToString())));

                dataTable.AddCell(new Cell()
                    .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                    .Add(new Paragraph(buyer.ProductsByManufacturer).SetFixedLeading(10)));

                dataTable.AddCell(new Cell()
                    .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                    .Add(new Paragraph(buyer.Cash.ToString("C"))));

                dataTable.AddCell(new Cell()
                    .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                    .Add(new Paragraph(buyer.Transfer.ToString("C"))));
            }

            dataTable.AddCell(new Cell().SetBorder(Border.NO_BORDER));

            dataTable.AddCell(new Cell().SetBorder(Border.NO_BORDER));

            dataTable.AddCell(new Cell()
                .SetBold()
                .SetFontSize(8)
            .SetPadding(0)
            .SetPaddingLeft(2)
                .Add(new Paragraph("Total")));

            dataTable.AddCell(new Cell()
                .SetFontSize(8)
            .SetPadding(0)
            .SetPaddingLeft(2)
                .Add(new Paragraph(model.ProductsByBuyerTable.Select(x => x.Cash).DefaultIfEmpty().Sum().ToString("C"))));

            dataTable.AddCell(new Cell()
                .SetFontSize(8)
            .SetPadding(0)
            .SetPaddingLeft(2)
                .Add(new Paragraph(model.ProductsByBuyerTable.Select(x => x.Transfer).DefaultIfEmpty().Sum().ToString("C"))));


            dataTable.AddCell(new Cell().SetPaddingTop(12).SetBorder(Border.NO_BORDER));
            doc.Add(dataTable);

            // MORE THAN ONE TABLE
            dataTable = new Table(UnitValue.CreatePercentArray(new float[] { 1 })).SetWidth(UnitValue.CreatePercentValue(100));
            dataTable.AddCell(new Cell()
                .SetBold()
                .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                .Add(new Paragraph("Fabricantes con más de un comprador asignado")));

            dataTable.AddCell(new Cell()
                .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                .Add(new Paragraph(string.Join(", ", model.ProductsByManufacturerTable.Where(x => x.BuyerIds.Count() > 1).Select(x => x.Manufacturer)))));

            dataTable.AddCell(new Cell().SetPaddingTop(12).SetBorder(Border.NO_BORDER));
            doc.Add(dataTable);

            // MANUFACTURERS TABLE
            dataTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 0.5f, 2, 0.6f, 0.6f })).SetWidth(UnitValue.CreatePercentValue(100));
            dataTable.AddCell(new Cell()
                .SetBold()
                .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                .Add(new Paragraph("Fabricante")));

            dataTable.AddCell(new Cell()
                .SetBold()
                .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                .Add(new Paragraph("Total de productos")));

            dataTable.AddCell(new Cell()
                .SetBold()
                .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                .Add(new Paragraph("Compradores")));

            dataTable.AddCell(new Cell()
                .SetBold()
                .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                .Add(new Paragraph("Efectivo")));

            dataTable.AddCell(new Cell()
                .SetBold()
                .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                .Add(new Paragraph("Transferencia")));

            foreach (var manufacturer in model.ProductsByManufacturerTable)
            {
                dataTable.AddCell(new Cell()
                    .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                    .Add(new Paragraph(manufacturer.Manufacturer).SetFixedLeading(10)));

                dataTable.AddCell(new Cell()
                    .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                    .Add(new Paragraph(manufacturer.TotalProducts.ToString())));

                dataTable.AddCell(new Cell()
                    .SetFontSize(8)
                .SetPadding(0)
                .SetPaddingLeft(2)
                    .Add(new Paragraph(string.Join(", ", model.Buyers.Where(x => manufacturer.BuyerIds.Contains(int.Parse(x.Value))).Select(x => x.Text))).SetFixedLeading(10)));

                dataTable.AddCell(new Cell()
                    .SetFontSize(8)
            .SetPadding(0)
            .SetPaddingLeft(2)
                    .Add(new Paragraph(manufacturer.Cash.ToString("C"))));

                dataTable.AddCell(new Cell()
                    .SetFontSize(8)
            .SetPadding(0)
            .SetPaddingLeft(2)
                    .Add(new Paragraph(manufacturer.Transfer.ToString("C"))));
            }

            dataTable.AddCell(new Cell().SetBorder(Border.NO_BORDER));

            dataTable.AddCell(new Cell().SetBorder(Border.NO_BORDER));

            dataTable.AddCell(new Cell()
                .SetBold()
                .SetFontSize(8)
            .SetPadding(0)
            .SetPaddingLeft(2)
                .Add(new Paragraph("Total")));

            dataTable.AddCell(new Cell()
                .SetFontSize(8)
            .SetPadding(0)
            .SetPaddingLeft(2)
                .Add(new Paragraph(model.ProductsByManufacturerTable.Select(x => x.Cash).DefaultIfEmpty().Sum().ToString("C"))));

            dataTable.AddCell(new Cell()
                .SetFontSize(8)
            .SetPadding(0)
            .SetPaddingLeft(2)
                .Add(new Paragraph(model.ProductsByManufacturerTable.Select(x => x.Transfer).DefaultIfEmpty().Sum().ToString("C"))));

            doc.Add(dataTable);

            doc.Flush();
            doc.Close();
            return File(stream.ToArray(), MimeTypes.ApplicationPdf);
        }

        private decimal RoundBuyerCashAmount(decimal costSum)
        {
            if (costSum == 0) return 0;
            costSum += 50;
            costSum = Math.Ceiling(costSum / 100) * 100;
            return costSum;
        }

        private decimal GetBuyerCash(IList<Order> orders, int buyerId, List<OrderItemBuyer> orderItemBuyer)
        {
            var productsIdsWithTransferManufacturer = _manufacturerService.GetProductManufacturers().Where(x => x.Manufacturer.IsPaymentWhithTransfer).Select(x => x.ProductId).ToList();
            var orderItemIdsWhithTransferManufacturer = orders.Select(x => x.OrderItems.Where(y => productsIdsWithTransferManufacturer.Contains(y.ProductId)))
                .SelectMany(x => x)
                .Select(x => x.Id)
                .ToList();

            decimal amountInCash = orderItemBuyer.Where(x => x.CustomerId == buyerId && !orderItemIdsWhithTransferManufacturer.Contains(x.OrderItemId))
                                     .Select(x => x.Cost).DefaultIfEmpty().Sum();

            return amountInCash;
        }

        private decimal GetBuyerTransferAmount(IList<Order> orders, int buyerId, List<OrderItemBuyer> orderItemBuyer)
        {
            var productsIdsWithTransferManufacturer = _manufacturerService.GetProductManufacturers().Where(x => x.Manufacturer.IsPaymentWhithTransfer).Select(x => x.ProductId).ToList();
            var orderItemIdsWhithTransferManufacturer = orders.Select(x => x.OrderItems.Where(y => productsIdsWithTransferManufacturer.Contains(y.ProductId)))
                .SelectMany(x => x)
                .Select(x => x.Id)
                .ToList();

            decimal amountToTransfer = orderItemBuyer.Where(x => x.CustomerId == buyerId && orderItemIdsWhithTransferManufacturer.Contains(x.OrderItemId))
                                     .Select(x => x.Cost).DefaultIfEmpty().Sum();

            return amountToTransfer;
        }

        private decimal GetBuyerCorporateCardAmount(IList<Order> orders, int buyerId, List<OrderItemBuyer> orderItemBuyer)
        {
            var productsIdsWithCorporateCardManufacturer = _manufacturerService.GetProductManufacturers().Where(x => x.Manufacturer.IsPaymentWhithCorporateCard).Select(x => x.ProductId).ToList();
            var orderItemIdsWhithCorporateCardManufacturer = orders.Select(x => x.OrderItems.Where(y => productsIdsWithCorporateCardManufacturer.Contains(y.ProductId)))
                .SelectMany(x => x)
                .Select(x => x.Id)
                .ToList();

            decimal amountToCorporateCard = orderItemBuyer.Where(x => x.CustomerId == buyerId && orderItemIdsWhithCorporateCardManufacturer.Contains(x.OrderItemId))
                                     .Select(x => x.Cost).DefaultIfEmpty().Sum();

            return amountToCorporateCard;
        }

        [HttpPost]
        public IActionResult GetLastAssigned(AutoAssignModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderItemBuyer))
                return AccessDeniedView();

            var dateDelivery = DateTime.ParseExact(model.Date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var role = _customerService.GetCustomerRoleBySystemName("buyer");
            var buyers = _customerService.GetAllCustomers(customerRoleIds: new int[] { role.Id });
            var dateOrderItems = GetOrderItems(dateDelivery);
            var allOrderItems = GetOrderItems().Select(x => new { x.ProductId, x.Id });

            var productIds = dateOrderItems.Select(x => x.ProductId).GroupBy(x => x).Select(x => x.FirstOrDefault()).ToList();

            var result = new List<AutoAssingModel>();
            foreach (var productId in productIds)
            {
                var orderItemIds = allOrderItems.Where(x => x.ProductId == productId).Select(x => x.Id).ToList();
                var lastBuyer = _orderItemBuyerService.GetAll()
                    .Where(x => orderItemIds.Contains(x.OrderItemId) && model.BuyerIds.Contains(x.CustomerId))
                    .OrderByDescending(x => x.CreatedOnUtc)
                    .Select(x => x.CustomerId)
                    .FirstOrDefault();
                result.Add(new AutoAssingModel() { BuyerId = lastBuyer, ProductId = productId });
            }

            return Ok(result);
        }

        private IQueryable<OrderItem> GetOrderItems(DateTime? dateDelivery = null)
        {
            var orders = _orderService.GetAllOrdersQuery()
                                .Where(x => x.OrderStatusId != 40 &&
                                    !(x.PaymentMethodSystemName == "Payments.PayPalStandard" &&
                                    x.PaymentStatusId == 10));

            if (dateDelivery.HasValue)
            {
                var date = dateDelivery.Value;
                orders = orders.Where(x => x.SelectedShippingDate == date);
            }

            return orders.Select(x => x.OrderItems).SelectMany(x => x);
        }

        [HttpPost]
        public IActionResult AssignBuyers(AssignResultModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderItemBuyer))
                return AccessDeniedView();

            var dateDelivery = model.Date;
            var orderItems = OrderUtils.GetFilteredOrders(_orderService)
                                .Where(x => x.SelectedShippingDate == model.Date)
                                .Select(x => x.OrderItems)
                                .SelectMany(x => x)
                                .Select(x => new { OrderItemId = x.Id, x.ProductId, x.Product.ProductCost, x.PriceInclTax, x.EquivalenceCoefficient, x.WeightInterval, x.Quantity, x.OrderId, x.Order.SelectedShippingDate })
                                .ToList();
            var orderItemIds = orderItems.Select(x => x.OrderItemId).ToList();
            var orderItemBuyers = _orderItemBuyerService.GetAll().Where(x => orderItemIds.Contains(x.OrderItemId)).ToList();

            foreach (var item in orderItems)
            {
                AssignResultData assing = model.Result.Where(x => x.ProductId == item.ProductId).FirstOrDefault();
                OrderItemBuyer orderItemBuyer = orderItemBuyers.Where(x => x.OrderItemId == item.OrderItemId).FirstOrDefault();

                if (orderItemBuyer == null)
                {
                    orderItemBuyer = new OrderItemBuyer()
                    {
                        OrderItemId = item.OrderItemId,
                        CustomerId = assing.CustomerId,
                        OrderId = item.OrderId,
                        SelectedShippingDate = item.SelectedShippingDate,
                        Cost = item.ProductCost > 0 ? CalculateCost(item.ProductCost, item.Quantity, item.EquivalenceCoefficient, item.WeightInterval) : item.PriceInclTax
                    };
                    _orderItemBuyerService.Insert(orderItemBuyer);
                }
#if DEBUG
                else if (true)
#else
                 else if (model.Date.AddDays(7) > DateTime.Now)
#endif
                {
                    orderItemBuyer.CustomerId = assing.CustomerId;
                    orderItemBuyer.OrderId = item.OrderId;
                    orderItemBuyer.SelectedShippingDate = item.SelectedShippingDate;
                    orderItemBuyer.Cost = item.ProductCost > 0 ? CalculateCost(item.ProductCost, item.Quantity, item.EquivalenceCoefficient, item.WeightInterval) : item.PriceInclTax;
                    _orderItemBuyerService.Update(orderItemBuyer);
                }
            }

            var supermarkedBuyer = _supermarketBuyerService.GetAll().Where(x => x.ShippingDate == dateDelivery && x.Deleted == false).FirstOrDefault();
            if (supermarkedBuyer == null)
            {
                if (model.SupermarketBuyerId != 0)
                {

                    string supermarketBuyerLog = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) seleccionó a {_customerService.GetCustomerById(model.SupermarketBuyerId)?.GetFullName()} ({model.SupermarketBuyerId}) como comprador para el supermercado para el día {dateDelivery.ToString("dd-MM-yyyy")}.\n";
                    supermarkedBuyer = new SupermarketBuyer()
                    {
                        BuyerId = model.SupermarketBuyerId,
                        ShippingDate = dateDelivery,
                        Log = supermarketBuyerLog
                    };
                    _supermarketBuyerService.Insert(supermarkedBuyer);
                }
            }
            else
            {
                string supermarketBuyerLog = null;
                if (supermarkedBuyer.BuyerId != model.SupermarketBuyerId)
                {
                    string previusSupermarketBuyerName = supermarkedBuyer.BuyerId == 0 ? "No asignado" : _customerService.GetCustomerById(supermarkedBuyer.BuyerId)?.GetFullName();
                    string supermarketBuyerName = model.SupermarketBuyerId == 0 ? "No asignado" : _customerService.GetCustomerById(model.SupermarketBuyerId)?.GetFullName();

                    supermarketBuyerLog = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) modificó el comprador para el supermercado {previusSupermarketBuyerName} ({supermarkedBuyer.BuyerId}) por {supermarketBuyerName} ({model.SupermarketBuyerId}).\n";
                    supermarkedBuyer.Log += supermarketBuyerLog;
                    supermarkedBuyer.BuyerId = model.SupermarketBuyerId;
                }
                _supermarketBuyerService.Update(supermarkedBuyer);
            }

            var buyerListDownload = _buyerListDownloadService.GetAll().Where(x => x.OrderShippingDate == dateDelivery).FirstOrDefault();
            string status = model.AllowDownload ? "activó" : "desactivó";
            string log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) {status} la descarga de la lista de compras.\n";
            if (buyerListDownload == null)
            {

                buyerListDownload = new BuyerListDownload()
                {
                    AllowDownload = model.AllowDownload,
                    AllowReport = model.AllowReport,
                    OrderShippingDate = dateDelivery,
                    Log = log
                };
                _buyerListDownloadService.Insert(buyerListDownload);
            }
            else
            {
                buyerListDownload.AllowDownload = model.AllowDownload;
                buyerListDownload.AllowReport = model.AllowReport;
                buyerListDownload.Log += log;
                _buyerListDownloadService.Update(buyerListDownload);
            }

            return NoContent();
        }

        public IActionResult AutomaticDistributionByManufacturer(string date, int[] selectedManufacturersIds)
        {
            var dateDelivery = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var orderItems = GetOrderItems(dateDelivery).ToList();

            var role = _customerService.GetCustomerRoleBySystemName("buyer");
            var buyers = _customerService.GetAllCustomers(customerRoleIds: new int[] { role.Id });

            var products = orderItems.GroupBy(x => x.Product);
            int[] orderItemsIds = orderItems.Select(x => x.Id).ToArray();
            var assignedProducts = _orderItemBuyerService.GetAll().Where(x => orderItemsIds.Contains(x.OrderItemId)).ToList();

            var assignedProductsByBuyer = assignedProducts.GroupBy(x => x.CustomerId).ToList();

            var mainManufacturers = _productMainManufacturerService.GetAll().ToList();
            var assignedProductsByManufacturers = orderItems.Where(y => assignedProducts.Select(z => z.OrderItemId).Contains(y.Id))
                                                  .GroupBy(y => Utils.ProductUtils.GetMainManufacturer(y.Product.ProductManufacturers, mainManufacturers.Where(z => z.ProductId == y.ProductId).FirstOrDefault()));

            var manufacturerTable = assignedProductsByManufacturers.Select(x => new OrderItemsByManufacturer()
            {
                Manufacturer = (x.Key == null ? "Sin fabricante" : x.Key.Name),
                ManufacturerId = (x.Key == null ? 0 : x.Key.Id),
                BuyerIds = assignedProducts.Where(y => x.Select(z => z.Id).Contains(y.OrderItemId)).GroupBy(y => y.CustomerId).Select(y => y.Key).ToList(),
                OrderItemList = x.Select(y => y).ToList(),
                NumberProducts = x.GroupBy(y => y.ProductId).Count()
            }).OrderBy(x => x.Manufacturer).ToList();

            var manufacturersWhitManyBuyers = manufacturerTable.Where(x => x.BuyerIds.Count() > 1 && x.Manufacturer != "Sin fabricante").ToList();
            if (selectedManufacturersIds != null)
            {
                manufacturersWhitManyBuyers = manufacturersWhitManyBuyers.Where(x => selectedManufacturersIds.Contains(x.ManufacturerId)).ToList();
            }

            var buyerTable = assignedProductsByBuyer.Select(x => new OrderItemsBuyerByBuyer()
            {
                Buyer = x.Key == 0 ? "No asignado" : buyers.Where(y => y.Id == x.Key).FirstOrDefault().GetFullName(),
                BuyerId = x.Key,
                TotalProducts = orderItems.Where(y => x.Select(z => z.OrderItemId).Contains(y.Id)).GroupBy(y => y.ProductId).Count(),
                ProductsByManufacturer = orderItems.Where(y => x.Select(z => z.OrderItemId).Contains(y.Id))
                                                                    .GroupBy(y => Utils.ProductUtils.GetMainManufacturer(y.Product.ProductManufacturers, mainManufacturers.Where(z => z.ProductId == y.ProductId).FirstOrDefault()))
                                                                    .Select(y => new ProductsByManufacturer()
                                                                    {
                                                                        Manufacturer = y.Key == null ? "Sin fabricante" : y.Key.Name,
                                                                        ManufacturerId = y.Key == null ? 0 : y.Key.Id,
                                                                        NumberProducts = y.GroupBy(z => z.ProductId).Count()
                                                                    }).ToList(),
                OrderItemBuyer = x.ToList()
            }).OrderBy(x => x.Buyer).ToList();

            foreach (var manufacturer in manufacturersWhitManyBuyers)
            {
                var orderItemsBuyersByManufacturer = buyerTable.Where(x => manufacturer.BuyerIds.Contains(x.BuyerId)).ToList();
                int countProducts = 0;
                var buyerIdWhitAllProducts = 0;
                string buyerIdWhitAllProductsName = null;
                List<OrderItemBuyer> allOrderItemsBuyerToReassigned = new List<OrderItemBuyer>();
                foreach (var productsByBuyerAndManufacturer in orderItemsBuyersByManufacturer)
                {

                    var sumProducts = 0;
                    foreach (var item in productsByBuyerAndManufacturer.ProductsByManufacturer.Where(x => x.Manufacturer == manufacturer.Manufacturer))
                    {
                        sumProducts = sumProducts + item.NumberProducts;
                    }

                    if (sumProducts > countProducts)
                    {
                        if (productsByBuyerAndManufacturer.BuyerId != 0)
                        {
                            countProducts = sumProducts;
                            buyerIdWhitAllProducts = productsByBuyerAndManufacturer.BuyerId;
                            buyerIdWhitAllProductsName = productsByBuyerAndManufacturer.Buyer;
                        }
                    }

                    var orderItemsBuyerToReassignedByBuyer = productsByBuyerAndManufacturer.OrderItemBuyer.Where(x => manufacturer.OrderItemList.Select(y => y.Id).Contains(x.OrderItemId)).ToList();
                    allOrderItemsBuyerToReassigned.AddRange(orderItemsBuyerToReassignedByBuyer);
                }

                foreach (var item in allOrderItemsBuyerToReassigned.Where(x => x.CustomerId != buyerIdWhitAllProducts))
                {

                    item.CustomerId = buyerIdWhitAllProducts;
                    _orderItemBuyerService.Update(item);
                }
            }
            return Ok();
        }

        [HttpGet]
        public IActionResult AssignByDefaultManufacturer(string date)
        {
            var dateDelivery = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;
            var orderItems = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate == dateDelivery)
                .SelectMany(x => x.OrderItems)
                .ToList();
            var mainManufacturers = _productMainManufacturerService.GetAll().ToList();
            var orderItemsGroup = orderItems
                .GroupBy(x => Utils.ProductUtils.GetMainManufacturer(x.Product.ProductManufacturers, mainManufacturers.Where(y => y.ProductId == x.ProductId).FirstOrDefault()))
                .ToList();
            var orderItemIds = orderItems.Select(x => x.Id).ToList();
            var manufacturerBuyerAssing = _manufacturerBuyerService.GetAll().ToList();
            var orderItemBuyer = _orderItemBuyerService.GetAll().Where(x => orderItemIds.Contains(x.OrderItemId)).ToList();

            foreach (var group in orderItemsGroup)
            {
                if (group.Key == null) continue;
                var assignedBuyer = manufacturerBuyerAssing.Where(x => x.ManufacturerId == group.Key.Id).Select(x => x.BuyerId).FirstOrDefault();
                if (assignedBuyer == 0)
                    continue;

                foreach (var orderItem in group)
                {
                    OrderItemBuyer currentAssign = orderItemBuyer.Where(x => x.OrderItemId == orderItem.Id).FirstOrDefault();
                    if (currentAssign == null)
                    {
                        currentAssign = new OrderItemBuyer()
                        {
                            OrderItemId = orderItem.Id,
                            CustomerId = assignedBuyer,
                            Cost = orderItem.Product.ProductCost > 0 ? CalculateCost(orderItem.Product.ProductCost, orderItem.Quantity, orderItem.EquivalenceCoefficient, orderItem.WeightInterval) : orderItem.PriceInclTax
                        };
                        _orderItemBuyerService.Insert(currentAssign);
                    }
                    else if (currentAssign.CustomerId != assignedBuyer)
                    {
                        currentAssign.CustomerId = assignedBuyer;
                        _orderItemBuyerService.Update(currentAssign);
                    }
                }
            }

            return NoContent();
        }

        [HttpGet]
        public IActionResult VerifyAutoAssignManufacturer(string date)
        {
            var dateDelivery = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;
            var orderItems = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate == dateDelivery)
                .SelectMany(x => x.OrderItems)
                .ToList();
            var mainManufacturers = _productMainManufacturerService.GetAll().ToList();
            var orderItemsGroup = orderItems
                .GroupBy(x => Utils.ProductUtils.GetMainManufacturer(x.Product.ProductManufacturers, mainManufacturers.Where(y => y.ProductId == x.ProductId).FirstOrDefault()))
                .ToList();
            var orderItemIds = orderItems.Select(x => x.Id).ToList();
            var manufacturerBuyerAssing = _manufacturerBuyerService.GetAll().ToList();
            var orderItemBuyer = _orderItemBuyerService.GetAll().Where(x => orderItemIds.Contains(x.OrderItemId)).ToList();

            var buyerRole = _customerService.GetCustomerRoleBySystemName("buyer");
            var buyers = _customerService.GetAllCustomers(customerRoleIds: new int[] { buyerRole.Id });

            var badAssignedBuyers = new List<string>();
            foreach (var group in orderItemsGroup)
            {
                if (group.Key == null) continue;
                var assignedBuyer = manufacturerBuyerAssing.Where(x => x.ManufacturerId == group.Key.Id).Select(x => x.BuyerId).FirstOrDefault();
                if (assignedBuyer == 0)
                    continue;

                foreach (var orderItem in group)
                {
                    OrderItemBuyer currentAssign = orderItemBuyer.Where(x => x.OrderItemId == orderItem.Id).FirstOrDefault();
                    if (currentAssign == null) continue;
                    if (currentAssign.CustomerId != assignedBuyer)
                    {
                        var buyer = buyers.Where(x => x.Id == currentAssign.CustomerId).FirstOrDefault()?.GetFullName();
                        badAssignedBuyers.Add(buyer + " con el producto " + (orderItem.Product == null ? orderItem.ProductId.ToString() : orderItem.Product.Name));
                    }
                }
            }

            var result = badAssignedBuyers.Count == 0 ? new List<string>() { "Todos los compradores por defecto están asignados correctamente" } : badAssignedBuyers.GroupBy(x => x).Select(x => x.Key);

            return Ok(result);
        }

        private decimal CalculateCost(decimal productCost, int quantity, decimal equivalenceCoefficient, decimal weightInterval)
        {
            decimal weight = quantity;
            if (equivalenceCoefficient > 0)
                weight = quantity / equivalenceCoefficient;
            else if (weightInterval > 0)
                weight = (quantity * weightInterval) / 1000;

            return weight * productCost;
        }

        [HttpGet]
        public IActionResult GetBuyersSelectList()
        {
            var role = _customerService.GetCustomerRoleBySystemName("buyer");
            var buyers = _customerService.GetAllCustomers(customerRoleIds: new int[] { role.Id });
            return Ok(buyers.Select(x => new { Name = x.GetFullName(), x.Id }).OrderBy(x => x.Name).ToList());
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OrderItemBuyer))
                return AccessDeniedView();

            var query = _orderService.GetAllOrdersQuery()
                                    .Where(x => x.OrderStatusId != 40 &&
                                    !(x.PaymentMethodSystemName == "Payments.PayPalStandard" &&
                                    x.PaymentStatusId == 10));
            var queryList = query.GroupBy(x => x.SelectedShippingDate).OrderByDescending(x => x.Key);
            var pagedList = GroupedPageList(queryList, command.Page - 1, command.PageSize);

            var orderItems = pagedList.Select(x => x.Select(y => y.OrderItems).SelectMany(y => y)).SelectMany(x => x).ToList();

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    Id = x.Key,
                    Date = x.Key.Value.ToString("dd-MM-yyyy"),
                    Pending = GetStatus(orderItems.Where(y => y.Order.SelectedShippingDate == x.Key.Value).Select(y => y.Id).ToList())
                }).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        private string GetStatus(List<int> orderItemIds)
        {
            var filteredOrderItemBuyer = _orderItemBuyerService.GetAll().Where(x => orderItemIds.Contains(x.OrderItemId)).Select(x => x.CustomerId).ToList();
            return filteredOrderItemBuyer.Count() == 0 ? "Ninguno asignado" : orderItemIds.Count > filteredOrderItemBuyer.Count ? "Pendientes por asignar" : "Todos asignados";
        }

        private List<IGrouping<DateTime?, Order>> GroupedPageList(IQueryable<IGrouping<DateTime?, Order>> source, int pageIndex, int pageSize)
        {
            List<IGrouping<DateTime?, Order>> filteredList = new List<IGrouping<DateTime?, Order>>();
            filteredList.AddRange(source.Skip(pageIndex * pageSize).Take(pageSize).ToList());
            return filteredList;
        }
    }

    public class AssignResultModel
    {
        public DateTime Date { get; set; }
        public bool AllowDownload { get; set; }
        public bool AllowReport { get; set; }
        public List<AssignResultData> Result { get; set; }
        public int SupermarketBuyerId { get; set; }
    }

    public class AssignResultData
    {
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
    }

    public class OrderItemCustomerModel
    {
        public DateTime Date { get; set; }
        public bool AllowDownload { get; set; }
        public bool AllowReport { get; set; }
        public List<ForSelect> Buyers { get; set; }
        public List<ForSelect> Manufacturers { get; set; }
        public List<CategoryDataModel> Categories { get; set; }
        public List<ProductsByBuyerTable> ProductsByBuyerTable { get; set; }
        public List<ProductsByManufacturerTable> ProductsByManufacturerTable { get; set; }
        public int SupermarketBuyer { get; set; }
        public bool PendingManufacturerBuyer { get; set; }
    }

    public class ForSelect
    {
        public string Value { get; set; }
        public string Text { get; set; }
        public int TotalProducts { get; set; }
    }

    public class CategoryDataModel
    {
        public string Category { get; set; }
        public int CategoryId { get; set; }
        public List<ProductDataModel> Products { get; set; }
    }

    public class ProductDataModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SelectedBuyerId { get; set; }
        public ManufacturerDataModel Manufacturer { get; set; }
    }

    public class ManufacturerDataModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class AutoAssingModel
    {
        public int ProductId { get; set; }
        public int BuyerId { get; set; }
    }

    public class PrepareAutoAssingModel
    {
        public int ProductId { get; set; }
        public int OrderItemId { get; set; }
    }

    public class AutoAssignModel
    {
        public string Date { get; set; }
        public int[] BuyerIds { get; set; }
    }

    public class ProductsByBuyerTable
    {
        public string Buyer { get; set; }
        public int BuyerId { get; set; }
        public int TotalProducts { get; set; }
        public string ProductsByManufacturer { get; set; }
        public decimal CorporateCard { get; set; }
        public decimal Cash { get; set; }
        public decimal Transfer { get; set; }
    }

    public class ProductsByManufacturerTable
    {
        public string Manufacturer { get; set; }
        public int ManufacturerId { get; set; }
        public int TotalProducts { get; set; }
        public List<int> BuyerIds { get; set; }
        public decimal CorporateCard { get; set; }
        public decimal Cash { get; set; }
        public decimal Transfer { get; set; }
    }

    public class OrderItemsByManufacturer
    {
        public string Manufacturer { get; set; }
        public int ManufacturerId { get; set; }
        public List<int> BuyerIds { get; set; }
        public List<OrderItem> OrderItemList { get; set; }
        public int NumberProducts { get; set; }
    }

    public class OrderItemsBuyerByBuyer
    {
        public string Buyer { get; set; }
        public int BuyerId { get; set; }
        public List<OrderItemBuyer> OrderItemBuyer { get; set; }
        public int TotalProducts { get; set; }

        public List<ProductsByManufacturer> ProductsByManufacturer { get; set; }
    }

    public class ProductsByManufacturer
    {
        public string Manufacturer { get; set; }
        public int ManufacturerId { get; set; }
        public int NumberProducts { get; set; }

    }
}
