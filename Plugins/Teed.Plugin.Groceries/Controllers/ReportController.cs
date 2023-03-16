using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Data;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Teed.Plugin.Api.Services;
using Teed.Plugin.Groceries.Domain.OrderReports;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;
using Nop.Services.Security;
using Teed.Plugin.Groceries.Security;
using Nop.Services.Discounts;
using Nop.Core.Domain.Discounts;
using Nop.Services.Payments;
using System.Drawing;
using Nop.Services.Messages;
using Nop.Services.Helpers;
using Teed.Plugin.Groceries.Extensions;
using Teed.Plugin.Groceries.Models.SimplePedido;
using Nop.Services.Rewards;
using Nop.Core.Domain.Tax;
using Nop.Services.Tax;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    public class ReportController : BasePluginController
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductTagService _productTagService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IOrderService _orderService;
        private readonly IShippingService _shippingService;
        private readonly IUrlRecordService _recordService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly Services.OrderReportService _orderReportService;
        private readonly Services.OrderReportStatusService _orderReportStatusService;
        private readonly OrderItemBuyerService _orderItemBuyerService;
        private readonly ICustomerService _customerService;
        private readonly ShippingZoneService _shippingZoneService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ShippingRouteService _shippingRouteService;
        //private readonly CustomerSecurityTokenService _customerSecurityTokenService;
        private readonly ShippingRegionZoneService _shippingRegionZoneService;
        private readonly ShippingVehicleRouteService _shippingVehicleRouteService;
        private readonly ShippingVehicleService _shippingVehicleService;
        private readonly ShippingAreaService _shippingAreaService;
        private readonly IncidentsService _incidentsService;
        private readonly FranchiseBonusService _franchiseBonusService;
        private readonly IDbContext _dbContext;
        private readonly IPermissionService _permissionService;
        private readonly NotDeliveredOrderItemService _notDeliveredOrderItemService;
        private readonly IProductLogService _productLogService;
        private readonly IDiscountService _discountService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly IPaymentService _paymentService;
        private readonly ProductMainManufacturerService _productMainManufacturerService;
        private readonly ShippingRouteUserService _shippingRouteUserService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly BuyerPaymentService _buyerPaymentService;
        private readonly IAddressService _addressService;
        private readonly ICustomerBalanceService _customerBalanceService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly CostsIncreaseWarningService _costsIncreaseWarningService;
        private readonly CostsDecreaseWarningService _costsDecreaseWarningService;
        private readonly ITaxCategoryService _taxCategoryService;

        public ReportController(BuyerPaymentService buyerPaymentService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IProductService productService, ICategoryService categoryService,
            ISpecificationAttributeService specificationAttributeService,
            IProductTagService productTagService, IProductAttributeService productAttributeService,
            IOrderService orderService, Services.OrderReportService orderReportService, IShippingService shippingService,
            IUrlRecordService recordService, Services.OrderReportStatusService orderReportStatusService,
            OrderItemBuyerService orderItemBuyerService, ICustomerService customerService,
            ShippingZoneService shippingZoneService, IManufacturerService manufacturerService,
            ShippingRouteService shippingRouteService,
            ProductMainManufacturerService productMainManufacturerService,
            //CustomerSecurityTokenService customerSecurityTokenService,
            ShippingRegionZoneService shippingRegionZoneService,
            IGenericAttributeService genericAttributeService,
            ShippingVehicleRouteService shippingVehicleRouteService,
            ShippingVehicleService shippingVehicleService,
            ShippingAreaService shippingAreaService,
            ICustomerRegistrationService customerRegistrationService,
            FranchiseBonusService franchiseBonusService,
            NotDeliveredOrderItemService notDeliveredOrderItemService,
            IProductLogService productLogService,
            IDiscountService discountService,
            IPaymentService paymentService,
            ShippingRouteUserService shippingRouteUserService,
            IncidentsService incidentsService, IDbContext dbContext, IPermissionService permissionService,
            IAddressService addressService,
            ICustomerBalanceService customerBalanceService,
            IOrderTotalCalculationService orderTotalCalculationService,
            CostsIncreaseWarningService costsIncreaseWarningService,
            CostsDecreaseWarningService costsDecreaseWarningService,
            ITaxCategoryService taxCategoryService)
        {
            _buyerPaymentService = buyerPaymentService;
            _paymentService = paymentService;
            _franchiseBonusService = franchiseBonusService;
            _incidentsService = incidentsService;
            _productService = productService;
            _categoryService = categoryService;
            _specificationAttributeService = specificationAttributeService;
            _productTagService = productTagService;
            _productAttributeService = productAttributeService;
            _orderService = orderService;
            _orderReportService = orderReportService;
            _shippingService = shippingService;
            _recordService = recordService;
            _orderItemBuyerService = orderItemBuyerService;
            _customerService = customerService;
            _orderReportStatusService = orderReportStatusService;
            _shippingZoneService = shippingZoneService;
            _manufacturerService = manufacturerService;
            _shippingRouteService = shippingRouteService;
            //_customerSecurityTokenService = customerSecurityTokenService;
            _shippingRegionZoneService = shippingRegionZoneService;
            _genericAttributeService = genericAttributeService;
            _shippingVehicleRouteService = shippingVehicleRouteService;
            _shippingVehicleService = shippingVehicleService;
            _shippingAreaService = shippingAreaService;
            _dbContext = dbContext;
            _customerRegistrationService = customerRegistrationService;
            _permissionService = permissionService;
            _notDeliveredOrderItemService = notDeliveredOrderItemService;
            _productLogService = productLogService;
            _discountService = discountService;
            _productMainManufacturerService = productMainManufacturerService;
            _shippingRouteUserService = shippingRouteUserService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _addressService = addressService;
            _customerBalanceService = customerBalanceService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _costsIncreaseWarningService = costsIncreaseWarningService;
            _costsDecreaseWarningService = costsDecreaseWarningService;
            _taxCategoryService = taxCategoryService;
        }

        // Productos vendidos ultimos X dias, categoria padre, categoría hijo, unidad
        [HttpGet]
        public IActionResult GenerateExcel1(int days = 30)
        {
            var controlDate = DateTime.Now.AddDays(days * -1).Date;
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate.HasValue && DbFunctions.TruncateTime(x.SelectedShippingDate.Value) >= controlDate);

            var orderItems = orders.Select(x => x.OrderItems).SelectMany(x => x).GroupBy(x => x.Product).ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Producto";
                    worksheet.Cells[row, 2].Value = "Categoría padre";
                    worksheet.Cells[row, 3].Value = "Categoría hijo";
                    worksheet.Cells[row, 4].Value = "Monto de venta";
                    worksheet.Cells[row, 5].Value = "Cantidad vendida";
                    worksheet.Cells[row, 6].Value = "Unidad";

                    foreach (var item in orderItems)
                    {
                        row++;
                        var qty = GetQty(item.Key, item.Select(x => x.Quantity).Sum());

                        worksheet.Cells[row, 1].Value = item.Select(x => x.Product).FirstOrDefault().Name;
                        string parentCategory = item.Select(x => x.Product).FirstOrDefault().ProductCategories.Where(x => x.Category.ParentCategoryId == 0).FirstOrDefault()?.Category.Name;
                        worksheet.Cells[row, 2].Value = string.IsNullOrWhiteSpace(parentCategory) ? "Sin categoría padre" : parentCategory;
                        string childCategory = item.Select(x => x.Product).FirstOrDefault().ProductCategories.Where(x => x.Category.ParentCategoryId != 0).FirstOrDefault()?.Category.Name;
                        worksheet.Cells[row, 3].Value = string.IsNullOrWhiteSpace(childCategory) ? "Sin categoría hijo" : childCategory;
                        worksheet.Cells[row, 4].Value = item.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 5].Value = qty.Item1;
                        worksheet.Cells[row, 6].Value = qty.Item2;

                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_productos_vendidos_ult_" + days + "_dias.xlsx");
            }
        }

        // Productos vendidos ultimos 6 meses, monto vendido por dia (en dinero)
        [HttpGet]
        public IActionResult GenerateExcel2()
        {
            var controlDate = DateTime.Now.AddMonths(-6).Date;
            var today = DateTime.Now.Date;

            var orders = _orderService.GetAllOrdersQuery().Where(x => x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10))
                .Where(x => x.SelectedShippingDate.HasValue && DbFunctions.TruncateTime(x.SelectedShippingDate.Value) >= controlDate);

            var orderItems = orders.Select(x => x.OrderItems).SelectMany(x => x).GroupBy(x => x.Product).ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int column = 1;
                    worksheet.Cells[row, column].Value = "Producto";
                    for (int i = 0; i < (today - controlDate).TotalDays; i++)
                    {
                        column++;
                        worksheet.Cells[row, column].Value = controlDate.AddDays(column - 2);
                        worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";
                    }

                    foreach (var item in orderItems)
                    {
                        row++;
                        column = 1;
                        worksheet.Cells[row, column].Value = item.Key.Name;
                        for (int i = 0; i < (today - controlDate).TotalDays; i++)
                        {
                            column++;
                            var columnDate = controlDate.AddDays(column - 2);
                            var dateTotal = item.Where(x => x.Order.SelectedShippingDate.Value == columnDate)
                                .Select(x => x.PriceInclTax)
                                .DefaultIfEmpty()
                                .Sum(x => x);
                            worksheet.Cells[row, column].Value = dateTotal;
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_productos_vendidos_ult_6_meses_monto_diario.xlsx");
            }
        }

        // Productos vendidos ultimos 6 meses, monto vendido por dia (en unidades)
        [HttpGet]
        public IActionResult GenerateExcel3()
        {
            var controlDate = DateTime.Now.AddMonths(-6).Date;
            var today = DateTime.Now.Date;

            var orders = _orderService.GetAllOrdersQuery().Where(x => x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10))
                .Where(x => x.SelectedShippingDate.HasValue && DbFunctions.TruncateTime(x.SelectedShippingDate.Value) >= controlDate);

            var orderItems = orders.Select(x => x.OrderItems).SelectMany(x => x).GroupBy(x => x.Product).ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int column = 1;
                    worksheet.Cells[row, column].Value = "Producto";
                    for (int i = 0; i < (today - controlDate).TotalDays; i++)
                    {
                        column++;
                        worksheet.Cells[row, column].Value = controlDate.AddDays(column - 2);
                        worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";
                    }

                    foreach (var item in orderItems)
                    {
                        row++;
                        column = 1;
                        worksheet.Cells[row, column].Value = item.Key.Name;
                        for (int i = 0; i < (today - controlDate).TotalDays; i++)
                        {
                            column++;
                            var columnDate = controlDate.AddDays(column - 2);
                            var dateTotal = item.Where(x => x.Order.SelectedShippingDate.Value == columnDate)
                                .Select(x => x.Quantity)
                                .DefaultIfEmpty()
                                .Sum(x => x);
                            worksheet.Cells[row, column].Value = dateTotal;
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_productos_vendidos_ult_6_meses_unidades_diarias.xlsx");
            }
        }

        // Productos vendidos ultimos 6 meses, costo por dia
        [HttpGet]
        public IActionResult GenerateExcel4()
        {
            var controlDate = DateTime.Now.AddMonths(-6).Date;
            var today = DateTime.Now.Date;

            var orders = _orderService.GetAllOrdersQuery().Where(x => x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10))
                .Where(x => x.SelectedShippingDate.HasValue && DbFunctions.TruncateTime(x.SelectedShippingDate.Value) >= controlDate);

            var orderReports = _orderReportService.GetAll().Select(x => new { x.OrderItemId, x.RequestedQtyCost }).ToList();

            var orderItems = orders.Select(x => x.OrderItems).SelectMany(x => x).GroupBy(x => x.Product).ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int column = 1;
                    worksheet.Cells[row, column].Value = "Producto";
                    for (int i = 0; i < (today - controlDate).TotalDays; i++)
                    {
                        column++;
                        worksheet.Cells[row, column].Value = controlDate.AddDays(column - 2);
                        worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";
                    }

                    foreach (var item in orderItems)
                    {
                        row++;
                        column = 1;
                        worksheet.Cells[row, column].Value = item.Key.Name;
                        for (int i = 0; i < (today - controlDate).TotalDays; i++)
                        {
                            column++;
                            var columnDate = controlDate.AddDays(column - 2);
                            var dateOrderItemIds = item.Where(x => x.Order.SelectedShippingDate.Value == columnDate)
                                .Select(x => x.Id)
                                .ToList();
                            var totalCost = orderReports
                                .Where(x => dateOrderItemIds.Contains(x.OrderItemId))
                                .Select(x => x.RequestedQtyCost)
                                .DefaultIfEmpty()
                                .Sum(x => x);
                            worksheet.Cells[row, column].Value = totalCost;
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_productos_vendidos_ult_6_meses_costo_diario.xlsx");
            }
        }

        // Frutas y verduras mas vendidas ultimos x dias en dinero
        [HttpGet]
        public IActionResult GenerateExcel5(int days)
        {
            var controlDate = DateTime.Now.AddMonths(days * -1).Date;
            var today = DateTime.Now.Date;

            var orders = _orderService.GetAllOrdersQuery().Where(x => x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10))
                .Where(x => x.SelectedShippingDate.HasValue && DbFunctions.TruncateTime(x.SelectedShippingDate.Value) >= controlDate);

            decimal totalSales = orders.Select(x => x.OrderTotal).DefaultIfEmpty().Sum(x => x);

            var orderItems = orders.Select(x => x.OrderItems)
                .SelectMany(x => x)
                .Where(x => x.Product.ProductCategories.Select(y => y.CategoryId).Contains(22))
                .GroupBy(x => x.Product)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Producto";
                    worksheet.Cells[row, 2].Value = "Monto vendido";
                    worksheet.Cells[row, 3].Value = "Total vendido";

                    foreach (var item in orderItems)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = item.Key.Name;
                        worksheet.Cells[row, 2].Value = item.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum(x => x);
                        worksheet.Cells[row, 3].Value = totalSales;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_fruver_vendidos_monto.xlsx");
            }
        }

        // Promedio de días entre compras
        [HttpGet]
        public IActionResult GenerateExcel6()
        {
            var orders = _orderService.GetAllOrdersQuery().Where(x => x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10));

            var groupedByCustomer = orders.Select(x => new { x.SelectedShippingDate, x.CustomerId }).GroupBy(x => x.CustomerId).Where(x => x.Count() > 1).ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Id del usuario";
                    worksheet.Cells[row, 2].Value = "Promedio de días entre compras";

                    foreach (var group in groupedByCustomer)
                    {
                        var dates = group.Select(x => x.SelectedShippingDate.Value.Date).GroupBy(x => x).Select(x => x.First()).OrderBy(x => x);
                        var calc = (dates.Last() - dates.First()).TotalDays / dates.Count();
                        if (calc == 0) continue;

                        row++;
                        worksheet.Cells[row, 1].Value = group.Key;
                        worksheet.Cells[row, 2].Value = calc;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_dias_entre_compras.xlsx");
            }
        }

        // Monto comprado a fabricante
        [HttpGet]
        public IActionResult GenerateExcel7()
        {
            var controlDate = DateTime.Now.AddDays(-10);
            var orders = _orderService.GetAllOrdersQuery().Where(x => x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10))
                .Where(x => x.SelectedShippingDate > controlDate);

            var amount = orders
                .Select(x => x.OrderItems)
                .SelectMany(x => x)
                .Where(x => x.Product.ProductManufacturers.Where(y => y.ManufacturerId == 84).Any())
                .Select(x => x.PriceInclTax)
                .DefaultIfEmpty()
                .Sum(); //Rincon tapatio

            return Ok(amount);
        }

        // Productos despublicados nunca vendidos
        [HttpGet]
        public IActionResult GenerateExcel8()
        {
            var controlDate = DateTime.Now.AddDays(-10);
            var soldProductIds = _orderService.GetAllOrdersQuery()
                .Select(x => x.OrderItems)
                .SelectMany(x => x).Select(x => x.ProductId).GroupBy(x => x).Select(x => x.FirstOrDefault()).ToList();
            var products = _productService.GetAllProductsQuery().Where(x => !x.Deleted && !x.Published && !soldProductIds.Contains(x.Id)).ToList();

            for (int i = 0; i < products.Count(); i++)
            {
                products[i].Deleted = true;
                products[i].UpdatedOnUtc = DateTime.UtcNow;
                _productService.UpdateProduct(products[i]);
            }

            return Ok(products.Count);
        }

        // Productos con atributos
        [HttpGet]
        public IActionResult GenerateExcel9()
        {
            var products = _productService.GetAllProductsQuery().Where(x => !x.Deleted).ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Id Producto";
                    worksheet.Cells[row, 2].Value = "Producto";
                    worksheet.Cells[row, 3].Value = "Manojo/Nevera";
                    worksheet.Cells[row, 4].Value = "Dureza";

                    foreach (var product in products)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = product.Id;
                        worksheet.Cells[row, 2].Value = product.Name;
                        worksheet.Cells[row, 3].Value = product.ProductSpecificationAttributes
                            .Select(x => x.SpecificationAttributeOption)
                            .Where(x => x.Name.ToLower().Contains("manojo") || x.Name.ToLower().Contains("nevera"))
                            .FirstOrDefault()?
                            .Name;
                        worksheet.Cells[row, 4].Value = product.ProductSpecificationAttributes
                            .Select(x => x.SpecificationAttributeOption)
                            .Where(x => x.Name.ToLower().Contains("dureza"))
                            .FirstOrDefault()?
                            .Name;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_producto_atributo.xlsx");
            }
        }

        // Productos con atributos
        [HttpGet]
        public IActionResult UpdateAttributes()
        {
            var products = _productService.GetAllProductsQuery().Where(x => !x.Deleted).ToList();
            var cuidado = _specificationAttributeService.GetSpecificationAttributes().Where(x => x.Name == "Tipo de cuidado").ToList();
            var dureza = _specificationAttributeService.GetSpecificationAttributes().Where(x => x.Name == "Dureza").ToList();

            #region MASSIVE_DATA

            var massiveData = new List<MassiveData1>()
            {
                new MassiveData1() { ProductId = 0, CarefulAttribute = "", StringAttribute = "" },
            };

            #endregion

            massiveData = massiveData.Where(x => !string.IsNullOrEmpty(x.StringAttribute) || !string.IsNullOrEmpty(x.StringAttribute)).ToList();
            foreach (var data in massiveData)
            {
                var product = products.Where(x => x.Id == data.ProductId).FirstOrDefault();
                if (product == null) continue;

                foreach (var existingSA in product.ProductSpecificationAttributes.ToList())
                {
                    _specificationAttributeService.DeleteProductSpecificationAttribute(existingSA);
                }

                if (!string.IsNullOrWhiteSpace(data.CarefulAttribute))
                {
                    var option = cuidado.Select(x => x.SpecificationAttributeOptions)
                        .SelectMany(x => x)
                        .Where(x => x.Name == data.CarefulAttribute)
                        .FirstOrDefault();

                    if (option != null)
                    {
                        var psa = new ProductSpecificationAttribute
                        {
                            AttributeTypeId = (int)SpecificationAttributeType.Option,
                            SpecificationAttributeOptionId = option.Id,
                            ProductId = product.Id,
                            CustomValue = null,
                            AllowFiltering = false,
                            ShowOnProductPage = false,
                            DisplayOrder = 0,
                        };
                        _specificationAttributeService.InsertProductSpecificationAttribute(psa);
                    }
                }

                if (!string.IsNullOrWhiteSpace(data.StringAttribute))
                {
                    var strong = dureza.Select(x => x.SpecificationAttributeOptions)
                        .SelectMany(x => x)
                        .Where(x => x.Name == data.StringAttribute)
                        .FirstOrDefault();

                    if (strong != null)
                    {
                        var psa = new ProductSpecificationAttribute
                        {
                            AttributeTypeId = (int)SpecificationAttributeType.Option,
                            SpecificationAttributeOptionId = strong.Id,
                            ProductId = product.Id,
                            CustomValue = null,
                            AllowFiltering = false,
                            ShowOnProductPage = false,
                            DisplayOrder = 0,
                        };
                        _specificationAttributeService.InsertProductSpecificationAttribute(psa);
                    }
                }
            }
            return Ok("Listo");
        }

        // Cantidad de ordenes por horario
        [HttpGet]
        public IActionResult GenerateExcel10(int days = 30)
        {
            var controlDate = DateTime.UtcNow.AddDays(days * -1).Date;
            var orders = GetFilteredOrders().Where(x => x.CreatedOnUtc >= controlDate && x.CreatedOnUtc <= DateTime.UtcNow).ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Horario";
                    worksheet.Cells[row, 2].Value = "No. Órdenes";

                    for (int j = 0; j < 24; j++)
                    {
                        row++;
                        var hourR1 = j < 10 ? "0" + j.ToString() : j.ToString();
                        var hourR2 = (j + 1) < 10 ? "0" + (j + 1).ToString() : (j + 1).ToString();
                        var numberOrders = orders.Where(x => x.CreatedOnUtc.ToLocalTime().Hour >= j && x.CreatedOnUtc.ToLocalTime().Hour < (j + 1)).Count();

                        worksheet.Cells[row, 1].Value = hourR1 + ":00 - " + hourR2 + ":00";
                        worksheet.Cells[row, 2].Value = numberOrders;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_orders_byHour.xlsx");
            }
        }


        // Venta diaria por categoria
        [HttpGet]
        public IActionResult GenerateExcel11(int months = 12)
        {
            var controlDate = DateTime.Now.AddMonths(months * -1).Date;
            var orderItemsByDates = GetFilteredOrders()
                .OrderBy(x => x.SelectedShippingDate)
                .Where(x => x.SelectedShippingDate.HasValue && DbFunctions.TruncateTime(x.SelectedShippingDate.Value) >= controlDate)
                .Select(x => x.OrderItems)
                .SelectMany(x => x)
                .GroupBy(x => x.Order.SelectedShippingDate)
                .ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("Categorias");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Fecha";
                    worksheet.Cells[row, 2].Value = "Categoría";
                    worksheet.Cells[row, 3].Value = "Cant. de productos";
                    worksheet.Cells[row, 4].Value = "Cant. de productos diferentes";
                    worksheet.Cells[row, 5].Value = "Monto de venta";
                    worksheet.Cells[row, 6].Value = "# de órdenes";
                    worksheet.Cells[row, 7].Value = "Total vendido";

                    foreach (var orderItemsByDate in orderItemsByDates)
                    {
                        var orderItemsByCategories = orderItemsByDate
                            .GroupBy(x => x.Product.ProductCategories.Where(y => y.Category.ParentCategoryId == 0).Select(y => y.Category).FirstOrDefault()).ToList();
                        var categoriesTotal = (decimal)0;
                        foreach (var orderItemsByCategory in orderItemsByCategories)
                        {
                            int productsQty = orderItemsByCategory.GroupBy(x => x.ProductId).SelectMany(x => x).Count();
                            int differentProductsQty = orderItemsByCategory.GroupBy(x => x.ProductId).Count();

                            row++;
                            var orderItems = orderItemsByCategory.ToList();
                            worksheet.Cells[row, 1].Value = orderItemsByDate.Key?.ToString("dd-MM-yyyy");
                            worksheet.Cells[row, 2].Value = orderItemsByCategory.Key == null ?
                                "Productos sin categoría padre" : orderItemsByCategory.Key.Name;
                            worksheet.Cells[row, 3].Value = productsQty;
                            worksheet.Cells[row, 4].Value = differentProductsQty;
                            worksheet.Cells[row, 5].Value = orderItemsByCategory.Sum(x => x.PriceInclTax).ToString("C");
                            categoriesTotal += orderItemsByCategory.Sum(x => x.PriceInclTax);
                        }
                        row++;
                        worksheet.Cells[row, 1].Value = orderItemsByDate.Key?.ToString("dd-MM-yyyy");
                        worksheet.Cells[row, 2].Value = "Órdenes";
                        worksheet.Cells[row, 6].Value = orderItemsByDate
                            .Select(x => x.Order).Distinct().Count();
                        row++;
                        worksheet.Cells[row, 1].Value = orderItemsByDate.Key?.ToString("dd-MM-yyyy");
                        worksheet.Cells[row, 2].Value = "Vendido";
                        worksheet.Cells[row, 7].Value = categoriesTotal.ToString("C");
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_venta_diaria_por_categoria.xlsx");
            }
        }

        // Cantidad comprada a Fabricante ultimos x dias
        public IActionResult GenerateValue2(int fabId, int days = 30)
        {
            var controlDate = DateTime.UtcNow.AddDays(days * -1);
            var orders = GetFilteredOrders()
                .Where(x => !x.Deleted && x.CreatedOnUtc >= controlDate && x.CreatedOnUtc <= DateTime.UtcNow);
            var orderItems = orders.Select(x => x.OrderItems).SelectMany(x => x)
                .Where(x => x.Product.ProductManufacturers.Where(y => y.Manufacturer.Id == fabId).Any());

            return Ok(orderItems.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum());

        }

        // Cantidad comprada a categoría ultimos x dias
        public IActionResult GenerateValue3(int catId, int days = 30)
        {
            var controlDate = DateTime.UtcNow.AddDays(days * -1);
            var orders = GetFilteredOrders()
                .Where(x => !x.Deleted && x.CreatedOnUtc >= controlDate && x.CreatedOnUtc <= DateTime.UtcNow);
            var orderItems = orders.Select(x => x.OrderItems).SelectMany(x => x)
                .Where(x => x.Product.ProductCategories.Where(y => y.Category.Id == catId).Any());

            return Ok(orderItems.Select(x => x.OriginalProductCost).DefaultIfEmpty().Sum());

        }

        // Lista de costos reportados por categoría
        [HttpGet]
        public IActionResult GenerateExcel12(int catId, int months = 12)
        {
            var controlDate = DateTime.Now.AddMonths(months * -1).Date;
            var orderItems = GetFilteredOrders()
                .Where(x => x.SelectedShippingDate.HasValue && DbFunctions.TruncateTime(x.SelectedShippingDate.Value) >= controlDate)
                .Select(x => x.OrderItems)
                .SelectMany(x => x)
                .Where(x => x.Product.ProductCategories.Where(y => y.CategoryId == catId).Any())
                .ToList();
            var orderItemIds = orderItems.Select(y => y.Id).ToList();

            var orderReport = _orderReportService.GetAll().ToList().Where(x => orderItemIds.Contains(x.OrderItemId) && (!x.NotBuyedReasonId.HasValue || x.NotBuyedReasonId == 0)).ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Fecha de entrega";
                    worksheet.Cells[row, 2].Value = "Producto";
                    worksheet.Cells[row, 3].Value = "Categoría padre";
                    worksheet.Cells[row, 4].Value = "Categoría hijo";
                    worksheet.Cells[row, 5].Value = "Cantidad pedida";
                    worksheet.Cells[row, 6].Value = "Costo unitario reportado (KG/PZ)";
                    worksheet.Cells[row, 7].Value = "Costo del pedido";

                    foreach (var report in orderReport)
                    {
                        var orderItem = orderItems.Where(x => x.Id == report.OrderItemId).FirstOrDefault();
                        row++;
                        if (orderItem.Order.SelectedShippingDate.Value.Date == DateTime.Now.Date)
                        {
                            var test = true;
                        }
                        worksheet.Cells[row, 1].Value = orderItem.Order.SelectedShippingDate.Value;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 2].Value = orderItem.Product.Name;
                        worksheet.Cells[row, 3].Value = orderItem.Product.ProductCategories
                                                            .Select(x => x.Category)
                                                            .Where(x => x.ParentCategoryId == 0)
                                                            .OrderBy(x => x.DisplayOrder)
                                                            .FirstOrDefault()?.Name;
                        worksheet.Cells[row, 4].Value = orderItem.Product.ProductCategories
                                                            .Select(x => x.Category)
                                                            .Where(x => x.ParentCategoryId != 0)
                                                            .OrderBy(x => x.DisplayOrder)
                                                            .FirstOrDefault()?.Name;
                        worksheet.Cells[row, 5].Value = GetParsedWeightAndPrice(orderItem).Weight;
                        worksheet.Cells[row, 6].Value = report.UnitCost;
                        worksheet.Cells[row, 7].Value = GetRequestedCost(orderItem, report.UnitCost);

                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_costo_productos_por_categoria.xlsx");
            }
        }

        // Lista de proveedores, su información y las categorias de sus productos
        [HttpGet]
        public IActionResult GenerateExcel18()
        {
            var productManufacturers = GetFilteredOrders()
                .Select(x => x.OrderItems)
                .SelectMany(x => x)
                .Select(x => x.Product.ProductManufacturers)
                .SelectMany(x => x)
                .ToList();

            var manufacturers = GetFilteredOrders()
               .Select(x => x.OrderItems)
               .SelectMany(x => x)
               .Select(x => x.Product.ProductManufacturers)
               .SelectMany(x => x)
               .Select(x => x.Manufacturer)
               .Distinct()
               .OrderBy(x => x.Name)
               .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Proveedor / Fabricante";
                    worksheet.Cells[row, 2].Value = "Local";
                    worksheet.Cells[row, 3].Value = "Contacto";
                    worksheet.Cells[row, 4].Value = "Teléfono";
                    worksheet.Cells[row, 5].Value = "Hora de apertura";
                    worksheet.Cells[row, 6].Value = "Hora de cierre";
                    worksheet.Cells[row, 7].Value = "Comprador";
                    worksheet.Cells[row, 8].Value = "Categoría de productos (padre)";
                    worksheet.Cells[row, 9].Value = "Categoría de productos (hijo)";

                    foreach (var manufacturer in manufacturers)
                    {
                        row++;
                        var categories = productManufacturers.Where(x => x.ManufacturerId == manufacturer.Id)
                            .Select(x => x.Product.ProductCategories).SelectMany(x => x).Select(x => x.Category);
                        var parentCategories = categories.Where(x => x.ParentCategoryId < 1).OrderBy(x => x.DisplayOrder).Distinct().ToList();
                        var childCategories = categories.Where(x => x.ParentCategoryId > 0).OrderBy(x => x.DisplayOrder).Distinct().ToList();

                        worksheet.Cells[row, 1].Value = manufacturer.Name;
                        worksheet.Cells[row, 8].Value = string.Join(", ", parentCategories.Select(x => x.Name));
                        worksheet.Cells[row, 9].Value = string.Join(", ", childCategories.Select(x => x.Name));
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }
                    for (int i = 2; i < 8; i++)
                    {
                        worksheet.Column(i).Width = 17;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_costo_productos_por_categoria.xlsx");
            }
        }

        // Monto comprado a proveedores/fabricante
        [HttpGet]
        public IActionResult GenerateExcel20(int days = 180)
        {
            var controlDate = DateTime.UtcNow.AddDays(days * -1);
            var orders = GetFilteredOrders().Where(x => !x.Deleted && x.CreatedOnUtc >= controlDate && x.CreatedOnUtc <= DateTime.UtcNow).ToList();

            var groupedByManufacturer = orders.Select(x => x.OrderItems)
                .SelectMany(x => x)
                .GroupBy(x => x.Product.ProductManufacturers.Select(y => y.Manufacturer).OrderBy(y => y.DisplayOrder).FirstOrDefault())
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Proveedor / Fabricante";
                    worksheet.Cells[row, 2].Value = "Categorías padre";
                    worksheet.Cells[row, 3].Value = "Categorías hijo";
                    worksheet.Cells[row, 2].Value = "Monto vendido";

                    foreach (var manufacturerGroup in groupedByManufacturer.Where(x => x.Key != null))
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = manufacturerGroup.Key.Name;

                        var categories = manufacturerGroup.Select(x => x.Product.ProductCategories).SelectMany(x => x).Select(x => x.Category);
                        var parentCategories = categories.Where(x => x.ParentCategoryId < 1).OrderBy(x => x.DisplayOrder).Distinct().ToList();
                        var childCategories = categories.Where(x => x.ParentCategoryId > 0).OrderBy(x => x.DisplayOrder).Distinct().ToList();

                        worksheet.Cells[row, 2].Value = string.Join(", ", parentCategories.Select(x => x.Name));
                        worksheet.Cells[row, 3].Value = string.Join(", ", childCategories.Select(x => x.Name));
                        worksheet.Cells[row, 4].Value = manufacturerGroup.DefaultIfEmpty().Sum(x => x.PriceInclTax);
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }
                    for (int i = 2; i < 8; i++)
                    {
                        worksheet.Column(i).Width = 17;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_comprado_fabricantes_{days}_dias.xlsx");
            }
        }

        // Lista precios por dia de las categorias hijo de "Carnes"
        [HttpGet]
        public IActionResult GenerateExcel19()
        {
            var controlDate = DateTime.Now.AddDays(-90).Date;
            var currentDate = DateTime.Now.Date;
            var orderItems = GetFilteredOrders().Where(x => x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10))
                .Where(x => x.SelectedShippingDate.HasValue && DbFunctions.TruncateTime(x.SelectedShippingDate.Value) >= controlDate)
                .Select(x => x.OrderItems)
                .SelectMany(x => x);
            var meatCategory = _categoryService.GetAllCategories()
                .Where(x => x.Name.Contains("Carnes") && x.ParentCategoryId == 0).FirstOrDefault();
            var subMeatCategories = _categoryService.GetAllCategories()
                .Where(x => x.ParentCategoryId == meatCategory.Id).OrderBy(x => x.DisplayOrder).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[1, 1].Value = "Carnes";

                    for (int i = 0; i < subMeatCategories.Count(); i++)
                    {
                        worksheet.Cells[i + 2, 1].Value = subMeatCategories[i].Name;
                    }

                    for (int i = 0; i < 90; i++)
                    {
                        var innerCurrentDate = currentDate.AddDays(i * -1).Date;
                        worksheet.Cells[1, i + 2].Value = innerCurrentDate.ToString("dd-MM-yyyy");
                        var orderItemsOfDate = orderItems.Where(x => x.Order.SelectedShippingDate == innerCurrentDate).ToList();

                        if (orderItemsOfDate.Any())
                        {
                            for (int e = 0; e < subMeatCategories.Count(); e++)
                            {
                                var orderItemsOfSubCategory = orderItemsOfDate
                                    .Where(x => x.Product.ProductCategories.Select(y => y.Category.Id).Contains(subMeatCategories[e].Id)).ToList();
                                if (orderItemsOfSubCategory.Any())
                                {
                                    worksheet.Cells[e + 2, i + 2].Value =
                                        orderItemsOfSubCategory.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum().ToString("C");
                                }
                                else
                                {
                                    worksheet.Cells[e + 2, i + 2].Value = "$0.00";
                                }
                            }
                        }
                        else
                        {
                            for (int e = 0; e < subMeatCategories.Count(); e++)
                            {
                                worksheet.Cells[e + 2, i + 2].Value = "$0.00";
                            }
                        }
                        row++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_precio_carnes_90_dias.xlsx");
            }
        }

        // Cantidad de órdenes por usuario los últimos X días 
        [HttpGet]
        public IActionResult GenerateExcel13(int days = 30)
        {
            var controlDate = DateTime.UtcNow.AddDays(days * -1);
            var allOrders = GetFilteredOrders().OrderBy(x => x.SelectedShippingDate).ToList();
            var filteredOrders = GetFilteredOrders()
                .Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= DateTime.UtcNow)
                .GroupBy(x => x.Customer)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Correo electrónico";
                    worksheet.Cells[row, 2].Value = "Fecha de primera entrega";
                    worksheet.Cells[row, 3].Value = "Fecha de última entrega";
                    worksheet.Cells[row, 4].Value = "Cantidad de órdenes hechas por el usuario desde siempre";

                    foreach (var order in filteredOrders)
                    {
                        var orders = order.OrderBy(x => x.SelectedShippingDate);
                        row++;
                        worksheet.Cells[row, 1].Value = order.Key.Email;
                        worksheet.Cells[row, 2].Value = orders.FirstOrDefault().SelectedShippingDate;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 3].Value = orders.LastOrDefault().SelectedShippingDate;
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 4].Value = allOrders.Where(x => x.CustomerId == order.Key.Id).Count();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_pedidos_por_usuario.xlsx");
            }
        }

        // Productos sin imagenes del fabricante x
        [HttpGet]
        public IActionResult GenerateExcel21(int manufacturerId)
        {
            if (manufacturerId < 1)
                return Ok();

            var products = _productService.GetAllProductsQuery()
                .Where(x => x.ProductManufacturers.Select(y => y.ManufacturerId).Contains(manufacturerId) &&
                !x.ProductPictures.Any())
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Fabricante";
                    worksheet.Cells[row, 2].Value = "Sku";
                    worksheet.Cells[row, 3].Value = "Nombre del producto";

                    foreach (var product in products)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = product.ProductManufacturers.FirstOrDefault().Manufacturer.Name;
                        worksheet.Cells[row, 2].Value = product.Sku;
                        worksheet.Cells[row, 3].Value = product.Name;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_productos_sin_imagen_{manufacturerId}.xlsx");
            }
        }


        // Productos y totales por Id de comprador de los ultimos dos meses
        [HttpGet]
        public IActionResult GenerateExcel22(int buyerId)
        {
            if (buyerId < 1)
                return Ok();

            var buyer = _customerService.GetAllCustomersQuery().Where(x => x.Id == buyerId).FirstOrDefault();
            if (buyer == null)
                return BadRequest("Couldn't find buyer with Id: " + buyerId);

            var dateTwoMonthsBefore = DateTime.UtcNow.AddMonths(-2);

            var orderItemIds = _orderItemBuyerService.GetAll()
                .Where(x => x.CustomerId == buyerId).Select(x => x.OrderItemId).ToList();

            var orderItems = _orderService.GetOrders()
                .Where(x => dateTwoMonthsBefore <= x.CreatedOnUtc && x.CreatedOnUtc <= DateTime.UtcNow)
                .Select(x => x.OrderItems).SelectMany(x => x)
                .Distinct()
                .Where(x => orderItemIds.Contains(x.Id));

            var products = orderItems
                .Where(x => x.Product.ProductCategories.Select(y => y.CategoryId).Contains(22))
                .GroupBy(x => x.Product)
                .OrderBy(x => x.Key.Name).ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Productos comprados por " + buyer.GetFullName() + "de los últimos dos meses";
                    row++;
                    worksheet.Cells[row, 1].Value = "Producto";
                    worksheet.Cells[row, 2].Value = "Cantidad de producto";
                    worksheet.Cells[row, 3].Value = "Unidad";
                    worksheet.Cells[row, 4].Value = "Monto vendido";
                    worksheet.Cells[row, 5].Value = "Precio del producto";
                    worksheet.Cells[row, 6].Value = "Fabricante";
                    worksheet.Cells[row, 7].Value = "Último costo registrado";

                    foreach (var product in products)
                    {
                        var orderItemsInner = product.ToList();
                        //if (product.Key.Name.ToLower().Contains("valencia"))
                        //{
                        //    var quantities = orderItemsInner.Select(x => x.Quantity);
                        //    var qtys = new List<Tuple<decimal, string>>();
                        //    var total = (decimal)0;
                        //    foreach (var quantity in quantities)
                        //    {
                        //        qtys.Add(GetQty(product.Key, quantity));
                        //    }
                        //    if (!qtys.Where(x => x.Item2 == "pz").Any())
                        //    {
                        //        foreach (var qty1 in qtys)
                        //        {
                        //            if (qty1.Item2 == "gr")
                        //                total += (qty1.Item1 / 1000);
                        //            else
                        //                total += qty1.Item1;
                        //        }
                        //    }
                        //    var test = GetQty(product.Key, orderItemsInner.Select(x => x.Quantity).Sum());
                        //}
                        row++;
                        var qty = GetQty(product.Key, orderItemsInner.Select(x => x.Quantity).Sum());
                        var orderReport = _orderReportService.GetAll()
                            .Where(x => x.ProductId == product.Key.Id)
                            .OrderByDescending(x => x.ReportedDateUtc)
                            .FirstOrDefault();
                        worksheet.Cells[row, 1].Value = product.Key.Name;
                        worksheet.Cells[row, 2].Value = qty.Item1;
                        worksheet.Cells[row, 3].Value = qty.Item2;
                        worksheet.Cells[row, 4].Value = orderItemsInner.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum(x => x);
                        worksheet.Cells[row, 5].Value = product.Key.Price;
                        worksheet.Cells[row, 6].Value = string.Join(", ", product.Key.ProductManufacturers.Select(x => x.Manufacturer.Name));
                        if (orderReport != null)
                            worksheet.Cells[row, 7].Value = orderReport.UnitCost;
                        else
                            worksheet.Cells[row, 7].Value = "";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                        worksheet.Cells[2, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_productos_por_comprador_dos_ultimos_meses_{buyerId}.xlsx");
            }
        }

        /// <summary>
        /// Item1 = quantity; Item2 = unit
        /// </summary>
        /// <param name="item"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public Tuple<decimal, string> GetQty(Product item, int quantity)
        {
            var unit = "pz";
            decimal result = quantity;
            if (item.EquivalenceCoefficient > 0)
            {
                result = quantity / item.EquivalenceCoefficient;
            }
            else if (item.WeightInterval > 0)
            {
                result = (quantity * item.WeightInterval) / 1000;
            }

            if (item.EquivalenceCoefficient > 0 || item.WeightInterval > 0)
                unit = "kg";

            return new Tuple<decimal, string>(result, unit);
        }

        //[HttpGet]
        //public IActionResult GenerateValue6()
        //{
        //    var controlDate = DateTime.UtcNow.AddMonths(-6);
        //    var orders = GetFilteredOrders()
        //        .Where(x => !x.Deleted && x.CreatedOnUtc >= controlDate && x.CreatedOnUtc <= DateTime.UtcNow)
        //         ;
        //    var orderItems = orders.Select(x => x.OrderItems).SelectMany(x => x)
        //        .Where(x => x.Product.Name.ToLower().Contains("huevo"))
        //         ;

        //    var total = orderItems.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
        //    return Ok(total);

        //}

        //[HttpGet]
        //public IActionResult GenerateValue66()
        //{
        //    var controlDate = DateTime.UtcNow.AddMonths(-6);
        //    var orders = GetFilteredOrders()
        //        .Where(x => !x.Deleted && x.CreatedOnUtc >= controlDate && x.CreatedOnUtc <= DateTime.UtcNow)
        //         ;
        //    var orderItems = orders.Select(x => x.OrderItems).SelectMany(x => x)
        //        .Where(x => x.Product.Name.ToLower().Contains("leche"))
        //         ;

        //    var total = orderItems.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
        //    return Ok(total);

        //}

        [HttpGet]
        public IActionResult FixProducts()
        {
            var controlDate = DateTime.Now.AddDays(-1);
            var products = _productService.SearchProducts().Where(x => x.CreatedOnUtc > controlDate);

            foreach (var product in products)
            {
                string seName = product.ValidateSeName(null, product.Name, true);
                _recordService.SaveSlug(product, seName, 0);

                _productService.AddStockQuantityHistoryEntry(product, 0, product.StockQuantity);
            }

            return Ok();
        }

        // Cantidad vendida del dia
        public IActionResult GenerateValue4(string date = null)
        {

            var orders = GetFilteredOrders();
            DateTime controlDate = !string.IsNullOrWhiteSpace(date) ? DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date : DateTime.Now.Date;
            orders = orders.Where(x => x.SelectedShippingDate == controlDate);
            var grouped = orders.GroupBy(x => x.PaymentMethodSystemName).ToList();
            return Ok(grouped.Select(x => x.Key + ": " + x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()));
        }

        private decimal GetParsedQuantity(int quantity, decimal weightInterval, decimal equivalenceCoefficient)
        {
            decimal result = quantity;
            if (equivalenceCoefficient > 0)
            {
                result = quantity / equivalenceCoefficient;
            }
            else if (weightInterval > 0)
            {
                result = (quantity * weightInterval) / 1000;
            }

            return result;
        }

        private WeightPriceCalculationResult GetParsedWeightAndPrice(OrderItem item)
        {
            decimal sellPrice = item.UnitPriceInclTax;
            decimal result = 0;
            var weight = $"{item.Quantity.ToString()} pz";

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

            return new WeightPriceCalculationResult()
            {
                SellPrice = sellPrice,
                Weight = (item.BuyingBySecondary && item.EquivalenceCoefficient > 0 && !string.IsNullOrWhiteSpace(weight))
                || item.WeightInterval > 0 ? $"{weight}" :
                $"{item.Quantity.ToString()} pz"
            };
        }

        // Ordenes de la ruta X con el producto Y
        public IActionResult GenerateValue3()
        {
            var orders = GetFilteredOrders()
                .Where(x => !x.Deleted && x.RouteId == 17 && x.OrderItems.Where(y => y.ProductId == 9517).Any())
                .Select(x => x.CustomOrderNumber)
                .ToList();

            return Ok(string.Join(", ", orders));

        }

        // Ventas mensuales desde el inicio de operación
        [HttpGet]
        public IActionResult GenerateExcel14()
        {
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Select(x => new GenerateExcel14Model()
                {
                    ShippingAddress = x.ShippingAddress.Address1,
                    CustomerId = x.CustomerId,
                    SelectedShippingDate = x.SelectedShippingDate,
                    SelectedShippingTime = x.SelectedShippingTime,
                    OrderTotal = x.OrderTotal,
                    Id = x.Id
                })
                .ToList();
            var ordersDate = orders.Select(x => x.SelectedShippingDate).OrderBy(x => x).ToList();
            var initDate = new DateTime(ordersDate.FirstOrDefault().Value.Year, ordersDate.FirstOrDefault().Value.Month, 1);
            var endDate = new DateTime(ordersDate.LastOrDefault().Value.Year, ordersDate.LastOrDefault().Value.Month, 1).AddMonths(1).AddDays(-1);
            var orderIds = orders.Select(x => x.Id).ToList();
            var reports = _orderReportService.GetAll()
                .Select(x => new
                {
                    x.OrderId,
                    x.OrderShippingDate,
                    x.ProductId,
                    x.UpdatedRequestedQtyCost,
                    x.OriginalBuyerId,
                    x.ManufacturerId
                })
                .ToList();
            var reportStatus = _orderReportStatusService.GetAll().Where(x => x.StatusTypeId == 2)
                .Select(x => new { x.BuyerId, x.ShippingDate }).ToList();

            var manufacturers = _manufacturerService.GetAllManufacturers();
            var transferManufacturerIds = manufacturers.Where(x => x.IsPaymentWhithTransfer).Select(x => x.Id).ToList();
            var cashManufacturerIds = manufacturers.Where(x => !x.IsPaymentWhithTransfer && !x.IsPaymentWhithCorporateCard).Select(x => x.Id).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Mes";
                    worksheet.Cells[row, 2].Value = "Monto vendido";
                    worksheet.Cells[row, 3].Value = "Número de pedidos";
                    worksheet.Cells[row, 4].Value = "Monto de compra de producto";
                    worksheet.Cells[row, 5].Value = "Monto de compra de producto en efectivo";
                    worksheet.Cells[row, 6].Value = "Monto de compra de producto por transferencia";

                    for (DateTime i = initDate; i <= endDate; i = i.AddMonths(1))
                    {
                        row++;
                        var filteredOrders = orders.Where(x => x.SelectedShippingDate.Value.Month == i.Month && x.SelectedShippingDate.Value.Year == i.Year).ToList();
                        var filteredOrderIds = filteredOrders.Select(x => x.Id).ToList();
                        var pedidos = OrderUtils.GetSimplePedidosGroupByList<GenerateExcel14Model>(filteredOrders);

                        var filteredReports = reports.Where(x => filteredOrderIds.Contains(x.OrderId));
                        var filteredReportsConfirmed = reports.Where(x => filteredOrderIds.Contains(x.OrderId) && reportStatus.Contains(new { BuyerId = x.OriginalBuyerId, ShippingDate = x.OrderShippingDate }));
                        var reportsGroupedByDateConfirmed = filteredReportsConfirmed.GroupBy(x => x.OrderShippingDate).ToList();
                        var updateQtyCostsConfirmed = reportsGroupedByDateConfirmed.SelectMany(x => x.GroupBy(y => y.ProductId).Select(z => z.FirstOrDefault()));

                        worksheet.Cells[row, 1].Value = i;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "mmmm, yyyy";
                        worksheet.Cells[row, 2].Value = filteredOrders.Sum(x => x.OrderTotal);
                        worksheet.Cells[row, 3].Value = pedidos.Count();
                        worksheet.Cells[row, 4].Value = updateQtyCostsConfirmed.Select(x => x.UpdatedRequestedQtyCost).DefaultIfEmpty().Sum();

                        var transfer = updateQtyCostsConfirmed.Where(x => x.ManufacturerId.HasValue && transferManufacturerIds.Contains(x.ManufacturerId.Value)).ToList();
                        var cash = updateQtyCostsConfirmed.Where(x => x.ManufacturerId.HasValue && cashManufacturerIds.Contains(x.ManufacturerId.Value)).ToList();
                        worksheet.Cells[row, 5].Value = transfer.Select(x => x.UpdatedRequestedQtyCost).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 6].Value = cash.Select(x => x.UpdatedRequestedQtyCost).DefaultIfEmpty().Sum();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ventas_pedidos_mensuales.xlsx");
            }
        }

        // Lista de órdenes con fecha y monto
        [HttpGet]
        public IActionResult GenerateExcel15()
        {
            var ordersGroupedByDate = GetFilteredOrders()
                .GroupBy(x => x.SelectedShippingDate.Value)
                .OrderBy(x => x.Key)
                .ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Fecha";
                    worksheet.Cells[row, 2].Value = "Orden";
                    worksheet.Cells[row, 3].Value = "Monto";

                    foreach (var dateGroup in ordersGroupedByDate)
                    {
                        var groupedByAddress = dateGroup.GroupBy(x => x.ShippingAddress.Address1).ToList();
                        foreach (var addressGroup in groupedByAddress)
                        {
                            row++;
                            worksheet.Cells[row, 1].Value = dateGroup.Key;
                            worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[row, 2].Value = string.Join(", ", addressGroup.Select(x => x.CustomOrderNumber));
                            worksheet.Cells[row, 3].Value = addressGroup.Sum(x => x.OrderTotal);
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ordenes.xlsx");
            }
        }

        // Ventas semanales desde el inicio de operación
        [HttpGet]
        public IActionResult GenerateExcel16()
        {
            var orders = GetFilteredOrders();
            var groupedByWeek = orders
                .OrderBy(x => x.SelectedShippingDate)
                .ToList()
                .GroupBy(x => x.SelectedShippingDate.Value.AddDays(-(int)x.SelectedShippingDate.Value.DayOfWeek))
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int weekCount = 1;
                    int currentYear = 0;

                    worksheet.Cells[row, 1].Value = "Semana";
                    worksheet.Cells[row, 2].Value = "Monto";

                    foreach (var group in groupedByWeek)
                    {
                        if (currentYear != group.Key.Year)
                        {
                            currentYear = group.Key.Year;
                            weekCount = 1;
                        }
                        row++;
                        worksheet.Cells[row, 1].Value = $"{currentYear}_{weekCount}";
                        worksheet.Cells[row, 2].Value = group.Sum(x => x.OrderTotal);
                        weekCount++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ventas_semanales.xlsx");
            }
        }

        // Porcentaje de lo vendido por categoría padre
        [HttpGet]
        public IActionResult GenerateExcel17()
        {
            var allOrders = GetFilteredOrders();
            var totalSold = allOrders.Sum(x => x.OrderTotal);
            var groupedByCategory = allOrders
                .Select(x => x.OrderItems)
                .SelectMany(x => x)
                .GroupBy(x => x.Product.ProductCategories.Select(y => y.Category).Where(y => y.ParentCategoryId == 0).OrderBy(y => y.DisplayOrder).FirstOrDefault())
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Categoría";
                    worksheet.Cells[row, 3].Value = "Porcentaje";

                    foreach (var group in groupedByCategory)
                    {
                        row++;
                        decimal categoryTotalSold = group.Sum(x => x.PriceInclTax);
                        worksheet.Cells[row, 1].Value = group.Key == null ? "Sin categoría padre" : group.Key.Name;
                        worksheet.Cells[row, 2].Value = categoryTotalSold;
                        worksheet.Cells[row, 3].Value = categoryTotalSold / totalSold;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_porcentaje_ventas_por_categoria.xlsx");
            }
        }

        // Productos más vendidos ultimos x dias
        [HttpGet]
        public IActionResult GenerateExcel25(int days = 30)
        {
            var controlDate = DateTime.Now.AddDays(days * -1).Date;
            var today = DateTime.Now.Date;

            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate.HasValue && DbFunctions.TruncateTime(x.SelectedShippingDate.Value) >= controlDate);

            var orderItems = orders.Select(x => x.OrderItems)
                .SelectMany(x => x)
                .GroupBy(x => x.Product)
                .OrderByDescending(x => x.Count())
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Producto";
                    worksheet.Cells[row, 2].Value = "Cantidad de órdenes";
                    worksheet.Cells[row, 3].Value = "Categoría padre";
                    worksheet.Cells[row, 4].Value = "Categoría hijo";
                    worksheet.Cells[row, 5].Value = "Monto total de venta";
                    worksheet.Cells[row, 6].Value = "Cantidad vendida";
                    worksheet.Cells[row, 7].Value = "Unidad";

                    foreach (var item in orderItems)
                    {
                        row++;
                        var qty = GetQty(item.Key, item.Select(x => x.Quantity).DefaultIfEmpty().Sum());
                        worksheet.Cells[row, 1].Value = item.Key.Name;
                        worksheet.Cells[row, 2].Value = item.Count();
                        worksheet.Cells[row, 3].Value = string.Join(", ", item.Key.ProductCategories
                                                            .Where(x => x.Category.ParentCategoryId == 0)
                                                            .Select(x => x.Category.Name));
                        worksheet.Cells[row, 4].Value = string.Join(", ", item.Key.ProductCategories
                                                           .Where(x => x.Category.ParentCategoryId > 0)
                                                           .Select(x => x.Category.Name));
                        worksheet.Cells[row, 5].Value = item.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 6].Value = qty.Item1;
                        worksheet.Cells[row, 7].Value = qty.Item2;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_productos_vendidos_{days}_dias.xlsx");
            }
        }

        // Suma de clientes por cantidades de ordenes totales de los ultimos 12 meses
        [HttpGet]
        public IActionResult GenerateExcel24()
        {
            var today = DateTime.Now.Date;
            var forDate = today.AddMonths(-12);
            var tempDate = new DateTime(forDate.Year, forDate.Month, 1).Date;
            var amountsCount = 7;

            var orders = _orderService.GetAllOrdersQuery().Where(x => x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10))
                .Where(x => x.SelectedShippingDate.HasValue && DbFunctions.TruncateTime(x.SelectedShippingDate.Value) >= tempDate);

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    var months = 12;
                    worksheet.Cells[row, 1].Value = "Fecha de " + today.ToString("dd/MM/yyyy") + " a " + tempDate.ToString("dd/MM/yyyy");
                    row++;
                    worksheet.Cells[row, 1].Value = "Monto";
                    worksheet.Cells[row, 2].Value = "Cantidad de clientes";

                    for (int i = 0; i < amountsCount; i++)
                    {
                        row++;
                        var amountText = "";
                        Tuple<decimal, decimal> numbers = Tuple.Create((decimal)0, (decimal)0);
                        switch (i)
                        {
                            case 0:
                                amountText = "1 a 500 pesos";
                                numbers = Tuple.Create((decimal)1, (decimal)500);
                                break;
                            case 1:
                                amountText = "501 a 1,000 pesos";
                                numbers = Tuple.Create((decimal)501, (decimal)1000);
                                break;
                            case 2:
                                amountText = "1,001 a 1,500 pesos";
                                numbers = Tuple.Create((decimal)1001, (decimal)1500);
                                break;
                            case 3:
                                amountText = "2,500 a 5,000 pesos";
                                numbers = Tuple.Create((decimal)2500, (decimal)5000);
                                break;
                            case 4:
                                amountText = "5,001 a 10,000 pesos";
                                numbers = Tuple.Create((decimal)5001, (decimal)10000);
                                break;
                            case 5:
                                amountText = "10,001 a 20,000 pesos";
                                numbers = Tuple.Create((decimal)10001, (decimal)20000);
                                break;
                            case 6:
                                amountText = "Más de 20,000";
                                numbers = Tuple.Create((decimal)20000, (decimal)20000);
                                break;
                            default:
                                break;
                        }
                        var clientsCount = 0;
                        if (numbers.Item1 == 20000)
                            clientsCount =
                            orders.Where(x => x.OrderTotal > numbers.Item1).GroupBy(x => x.Customer).Count();
                        else
                            clientsCount =
                                orders.Where(x => x.OrderTotal > numbers.Item1 && x.OrderTotal < numbers.Item2).GroupBy(x => x.Customer).Count();
                        worksheet.Cells[row, 1].Value = amountText;
                        worksheet.Cells[row, 2].Value = clientsCount;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                        worksheet.Cells[2, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_cantidad_clientes_por_monto.xlsx");
            }
        }

        // Ordenes de los ultimos 30 días
        //[HttpGet]
        //public IActionResult GenerateExcel25()
        //{
        //    var today = DateTime.Now.Date;
        //    var forDate = today.AddDays(-30);
        //    var tempDate = new DateTime(forDate.Year, forDate.Month, forDate.Day).Date;

        //    var orders = _orderService.GetAllOrdersQuery().Where(x => x.OrderStatusId != 40 &&
        //        !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10))
        //        .Where(x =>
        //        x.SelectedShippingDate.HasValue &&
        //        today >= DbFunctions.TruncateTime(x.SelectedShippingDate.Value) &&
        //        DbFunctions.TruncateTime(x.SelectedShippingDate.Value) >= tempDate);

        //    using (var stream = new MemoryStream())
        //    {
        //        using (var xlPackage = new ExcelPackage(stream))
        //        {
        //            var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
        //            int row = 1;
        //            worksheet.Cells[row, 1].Value = "Día";
        //            worksheet.Cells[row, 2].Value = "Ordenes";
        //            worksheet.Cells[row, 3].Value = "Cliente";
        //            worksheet.Cells[row, 4].Value = "Dirreccíon";
        //            worksheet.Cells[row, 5].Value = "Métodos de pago";
        //            worksheet.Cells[row, 6].Value = "Total";

        //            var ordersGrouping = orders
        //                .OrderByDescending(x => x.SelectedShippingDate)
        //                .ThenBy(x => x.Id)
        //                .GroupBy(x => DbFunctions.TruncateTime(x.SelectedShippingDate.Value)
        //                + x.ShippingAddress.Address1 + x.CustomerId).ToList();
        //            var currentDate = "";
        //            foreach (var orderGrouping in ordersGrouping)
        //            {
        //                row++;
        //                var groupingDate = orderGrouping.Key.Substring(0, orderGrouping.Key.IndexOf(" "));
        //                if (currentDate != groupingDate)
        //                {
        //                    if (row > 2)
        //                        row++;
        //                    currentDate = groupingDate;
        //                    worksheet.Cells[row, 1].Value = currentDate;
        //                    worksheet.Cells[row, 1].Style.Font.Bold = true;
        //                    row++;
        //                }
        //                worksheet.Cells[row, 1].Value = groupingDate;
        //                worksheet.Cells[row, 2].Value = string.Join(", ", orderGrouping.Select(x => x.CustomOrderNumber));
        //                worksheet.Cells[row, 3].Value = orderGrouping.Select(x => x.ShippingAddress.FirstName + " " + x.ShippingAddress.LastName).FirstOrDefault();
        //                worksheet.Cells[row, 4].Value = orderGrouping.Select(x => x.ShippingAddress.Address1).FirstOrDefault();
        //                worksheet.Cells[row, 5].Value = string.Join(", ", orderGrouping.Select(x => x.PaymentMethodSystemName));
        //                worksheet.Cells[row, 6].Value = orderGrouping.Select(x => x.OrderTotal).Sum();
        //            }

        //            for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
        //            {
        //                worksheet.Column(i).AutoFit();
        //                worksheet.Cells[1, i].Style.Font.Bold = true;
        //            }

        //            xlPackage.Save();
        //        }

        //        return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ordenes_ultimos_30_dias.xlsx");
        //    }
        //}

        // Margen bruto por categoría padre
        [HttpGet]
        public IActionResult GenerateExcel26(int days = 30)
        {
            var controlDate = DateTime.Now.AddDays(days * -1).Date;

            var reports = GetOrderItemReport();
            var validReports = reports.Where(x => x.OrderReport.OrderShippingDate > controlDate);
            var grouped = validReports.GroupBy(x => x.OrderItem.Product.ProductCategories
                .Select(y => y.Category)
                .Where(y => y.ParentCategoryId == 0 && !y.Deleted)
                .OrderBy(y => y.DisplayOrder)
                .FirstOrDefault()
            ).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Categoría padre";
                    worksheet.Cells[row, 2].Value = "Total venta";
                    worksheet.Cells[row, 3].Value = "Total costo";
                    worksheet.Cells[row, 4].Value = "Utilidad";

                    foreach (var groupedItem in grouped)
                    {
                        row++;

                        var totalSold = groupedItem.Select(x => x.OrderItem).Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
                        var totalCost = groupedItem.Select(x => GetRequestedCost(x.OrderItem, x.OrderReport.UpdatedUnitCost)).DefaultIfEmpty().Sum();

                        var categoryName = groupedItem.Key == null ? "Sin categoría padre" : groupedItem.Key.Name;
                        worksheet.Cells[row, 1].Value = categoryName;
                        worksheet.Cells[row, 2].Value = totalSold;
                        worksheet.Cells[row, 3].Value = totalCost;
                        worksheet.Cells[row, 4].Value = (totalSold - totalCost) / totalSold;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_margen_categoria_padre.xlsx");
            }
        }

        // Cantidad de productos vendidos en los ultimos X dias
        //[HttpGet]
        //public IActionResult GenerateExcel33(int days = 120)
        //{
        //    var today = DateTime.Now.Date;
        //    var controlDate = today.AddDays(days * -1).Date;
        //    var test0 = new DateTime(2020, 09, 10).Date;

        //    var products = _productService.GetAllProductsQuery()
        //        .Where(x => !x.Deleted && x.Published)
        //        .OrderBy(x => x.Name)
        //        .ToList();

        //    var ordersFiltered = GetFilteredOrders()
        //        .Where(x =>
        //        x.SelectedShippingDate.HasValue &&
        //        today >= DbFunctions.TruncateTime(x.SelectedShippingDate.Value) &&
        //        DbFunctions.TruncateTime(x.SelectedShippingDate.Value) >= controlDate);
        //    var ordersByDate = ordersFiltered
        //        .GroupBy(x => x.SelectedShippingDate)
        //        .OrderByDescending(x => x.Key)
        //        .ToList();

        //    using (var stream = new MemoryStream())
        //    {
        //        using (var xlPackage = new ExcelPackage(stream))
        //        {
        //            var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
        //            int row = 1;
        //            int col = 1;
        //            worksheet.Cells[row, col].Value = "Producto";
        //            col++;
        //            worksheet.Cells[row, col].Value = "Categoría padre";
        //            col++;
        //            worksheet.Cells[row, col].Value = "Categoría hijo";
        //            col++;
        //            worksheet.Cells[row, col].Value = "Unidad";
        //            col++;
        //            worksheet.Cells[row, col].Value = "Fabricante";
        //            foreach (var orders in ordersByDate)
        //            {
        //                col++;
        //                worksheet.Cells[row, col].Value = orders.Key?.ToString("dd/MM/yyyy");
        //            }
        //            col = 1;

        //            foreach (var product in products)
        //            {
        //                var qty = GetQty(product, 1);
        //                var categories = product.ProductCategories.Select(x => x.Category);
        //                var manufacturer = ProductUtils.GetMainManufacturer(product.ProductManufacturers);
        //                row++;
        //                worksheet.Cells[row, col].Value = product.Name;
        //                col++;
        //                worksheet.Cells[row, col].Value =
        //                    string.Join(", ", categories.Where(x => x.ParentCategoryId == 0).Select(x => x.Name));
        //                col++;
        //                worksheet.Cells[row, col].Value =
        //                    string.Join(", ", categories.Where(x => x.ParentCategoryId != 0).Select(x => x.Name));
        //                col++;
        //                worksheet.Cells[row, col].Value = qty.Item2;
        //                col++;
        //                worksheet.Cells[row, col].Value = manufacturer != null ?
        //                    manufacturer.Name : "N/A";

        //                foreach (var singleDate in ordersByDate)
        //                {
        //                    col++;
        //                    var orderItems = singleDate.ToList()
        //                        .SelectMany(x => x.OrderItems)
        //                        .Where(x => x.ProductId == product.Id);
        //                    qty = GetQty(product, orderItems.Select(x => x.Quantity).DefaultIfEmpty().Sum());
        //                    worksheet.Cells[row, col].Value = qty.Item1;
        //                }
        //                col = 1;
        //            }

        //            for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
        //            {
        //                worksheet.Column(i).AutoFit();
        //                worksheet.Cells[1, i].Style.Font.Bold = true;
        //            }

        //            xlPackage.Save();
        //        }

        //        return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_cantidad_productos_vendidos_ultimos_{days}_dias.xlsx");
        //    }
        //}

        public string MonthName(int month)
        {
            DateTimeFormatInfo dtinfo = new CultureInfo("es-ES", false).DateTimeFormat;
            return dtinfo.GetMonthName(month);
        }

        // Venta por categoría hijo
        [HttpGet]
        public IActionResult GenerateExcel23(int days = 30)
        {
            var controlDate = DateTime.Now.AddDays(days * -1).Date;

            var orders = _orderService.GetAllOrdersQuery().Where(x => x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10))
                .Where(x => x.SelectedShippingDate.HasValue && DbFunctions.TruncateTime(x.SelectedShippingDate.Value) >= controlDate);

            var orderItems = orders.Select(x => x.OrderItems)
                .SelectMany(x => x)
                .GroupBy(x => x.Product.ProductCategories.Where(y => y.Category.ParentCategoryId > 0).OrderBy(y => y.Category.DisplayOrder).Select(y => y.Category).FirstOrDefault())
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Categoría hijo";
                    worksheet.Cells[row, 2].Value = "Monto vendido";
                    worksheet.Cells[row, 3].Value = "Cantidad vendida";
                    worksheet.Cells[row, 4].Value = "Unidad";

                    foreach (var item in orderItems)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = item.Key == null ? "Sin categoría hijo" : item.Key.Name;
                        worksheet.Cells[row, 2].Value = item.DefaultIfEmpty().Sum(x => x.PriceInclTax);
                        worksheet.Cells[row, 3].Value = GetParsedQuantity(item.DefaultIfEmpty().Sum(x => x.Quantity), item.FirstOrDefault().Product.WeightInterval, item.FirstOrDefault().Product.EquivalenceCoefficient);
                        //worksheet.Cells[row, 4].Value = GetProductUnit(item.FirstOrDefault()?.Product);
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_vendido_cathijo.xlsx");
            }
        }

        //Monto vendido por mes
        [HttpGet]
        public IActionResult GenerateExcel27(int year = 2020)
        {
            var orders = GetFilteredOrders()
                .Where(x => x.SelectedShippingDate.HasValue && DbFunctions.TruncateTime(x.SelectedShippingDate.Value).Value.Year == year);

            var groupedOrders = orders.OrderBy(x => x.SelectedShippingDate.Value)
                .ToList()
                .GroupBy(x => x.SelectedShippingDate.Value.ToString("MMMM, yyyy", new CultureInfo("es-MX")))
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Mes";
                    worksheet.Cells[row, 2].Value = "Monto vendido";

                    foreach (var item in groupedOrders)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = item.Key;
                        worksheet.Cells[row, 2].Value = item.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_vendido_mensual.xlsx");
            }
        }

        //Monto vendido por dia ultimos x dias
        [HttpGet]
        public IActionResult GenerateExcel35(int days = 120)
        {
            var controlDate = DateTime.Now.AddDays(days * -1);

            var orders = GetFilteredOrders()
                .Where(x => x.SelectedShippingDate >= controlDate);

            var groupedOrders = orders.OrderBy(x => x.SelectedShippingDate.Value)
                .ToList()
                .GroupBy(x => x.SelectedShippingDate.Value.ToString("dd-MM-yyyy", new CultureInfo("es-MX")))
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Fecha";
                    worksheet.Cells[row, 2].Value = "Monto vendido";

                    foreach (var item in groupedOrders)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = item.Key;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-MM-yyyy";
                        worksheet.Cells[row, 2].Value = item.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_vendido_diario_ultimos_{days}_dias.xlsx");
            }
        }

        // Productos y en cuántas órdenes se ha vendido, categoria padre, categoría hijo
        [HttpGet]
        public IActionResult GenerateExcel36(int days = 120)
        {
            var controlDate = DateTime.Now.AddDays(days * -1).Date;
            var now = DateTime.Now.Date;
            var orders = GetFilteredOrders()
                .Where(x => x.SelectedShippingDate.HasValue && DbFunctions.TruncateTime(x.SelectedShippingDate.Value) >= controlDate && DbFunctions.TruncateTime(x.SelectedShippingDate.Value) <= now);

            var orderItems = orders.Select(x => x.OrderItems).SelectMany(x => x);
            var products = _productService.GetAllProductsQuery().Where(x => !x.Deleted).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Producto";
                    worksheet.Cells[row, 2].Value = "Categoría padre";
                    worksheet.Cells[row, 3].Value = "Categoría hijo";
                    worksheet.Cells[row, 4].Value = "Número de órdenes";

                    foreach (var product in products)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = product.Name;
                        string parentCategory = product.ProductCategories.Where(x => x.Category.ParentCategoryId == 0).FirstOrDefault()?.Category.Name;
                        worksheet.Cells[row, 2].Value = string.IsNullOrWhiteSpace(parentCategory) ? "Sin categoría padre" : parentCategory;
                        string childCategory = product.ProductCategories.Where(x => x.Category.ParentCategoryId != 0).FirstOrDefault()?.Category.Name;
                        worksheet.Cells[row, 3].Value = string.IsNullOrWhiteSpace(childCategory) ? "Sin categoría hijo" : childCategory;
                        worksheet.Cells[row, 4].Value = orderItems.Where(x => x.ProductId == product.Id).Count();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_productos_cantidad_ordenes_ultimos_{days}_dias.xlsx");
            }
        }

        // Venta y costo mensual total NORTHSTAR
        [HttpGet]
        public IActionResult GenerateExcel28(int month = 0, int year = 0, bool onlySocios = false)
        {

            var ordersQuery = GetFilteredOrders();
            var orderReportsQuery = _orderReportService.GetAll();
            if (month > 0 && year > 0)
            {
                ordersQuery = ordersQuery.Where(x => x.SelectedShippingDate.Value.Month == month && x.SelectedShippingDate.Value.Year == year);
                orderReportsQuery = orderReportsQuery.Where(x => x.OrderShippingDate.Month == month && x.OrderShippingDate.Year == year);
            }
            if (onlySocios)
            {
                ordersQuery = ordersQuery.Where(x => x.PaymentMethodSystemName == "Payments.Benefits");
            }
            var orders = ordersQuery.ToList();
            var orderReports = orderReportsQuery.ToList();
            var groupByDate = orders.GroupBy(x => x.SelectedShippingDate.Value.ToString("MMMM, yyyy", new CultureInfo("es-MX"))).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    foreach (var dateGroup in groupByDate)
                    {
                        var worksheet = xlPackage.Workbook.Worksheets.Add(dateGroup.Key);
                        int row = 1;
                        worksheet.Cells[row, 1].Value = "Producto";
                        worksheet.Cells[row, 2].Value = "Sku";
                        worksheet.Cells[row, 3].Value = "Unidades vendidas";
                        worksheet.Cells[row, 4].Value = "Unidad de medida";
                        worksheet.Cells[row, 5].Value = "Precio unitario";
                        worksheet.Cells[row, 6].Value = "Venta total";
                        worksheet.Cells[row, 7].Value = "Costo unitario promedio";
                        worksheet.Cells[row, 8].Value = "Categoría padre";
                        worksheet.Cells[row, 9].Value = "Categoría hijo";

                        var productGroup = dateGroup.SelectMany(x => x.OrderItems).GroupBy(x => x.Product).ToList();
                        foreach (var group in productGroup)
                        {
                            var qty = GetQty(group.Key, group.Select(x => x.Quantity).Sum());
                            var totalSale = group.Select(x => x.PriceInclTax).Sum();
                            var avgUnitPrice = totalSale / qty.Item1;
                            var itemIds = group.Select(x => x.Id).ToList();
                            var cost = orderReports.Where(x => itemIds.Contains(x.OrderItemId)).Select(x => x.UnitCost).DefaultIfEmpty().Average();
                            var parentCategories = group.Key.ProductCategories.Where(x => x.Category.ParentCategoryId == 0).Select(x => x.Category.Name).ToList();
                            var childCategories = group.Key.ProductCategories.Where(x => x.Category.ParentCategoryId != 0).Select(x => x.Category.Name).ToList();

                            row++;
                            worksheet.Cells[row, 1].Value = group.Key.Name;
                            worksheet.Cells[row, 2].Value = group.Key.Sku;
                            worksheet.Cells[row, 3].Value = qty.Item1;
                            worksheet.Cells[row, 4].Value = qty.Item2;
                            worksheet.Cells[row, 5].Value = avgUnitPrice;
                            worksheet.Cells[row, 6].Value = totalSale;
                            worksheet.Cells[row, 7].Value = cost;
                            worksheet.Cells[row, 8].Value = parentCategories.Count() > 0 ? string.Join(", ", parentCategories) : "Sin categoría padre";
                            worksheet.Cells[row, 9].Value = childCategories.Count() > 0 ? string.Join(", ", childCategories) : "Sin categoría hijo"; ;
                        }

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_productos_vendidos_{month}{year}{(onlySocios ? "_socios" : "")}.xlsx");
            }
        }

        // Lista de producto, venta y costo por fabricante
        [HttpGet]
        public IActionResult GenerateExcel29(int fbId, int days = 30)
        {
            var today = DateTime.Now.Date;
            var controlDate = today.AddDays(days * -1).Date;

            var products = _productService.GetAllProductsQuery()
                .Where(x => !x.Deleted && x.ProductManufacturers.Where(y => y.ManufacturerId == fbId).Any())
                .OrderBy(x => x.Name)
                .ToList();
            var productIds = products.Select(x => x.Id).ToList();
            string manufacturer = null;
            if (products.Count > 0)
            {
                manufacturer = products.FirstOrDefault().ProductManufacturers.Where(x => x.ManufacturerId == fbId).FirstOrDefault().Manufacturer.Name;
            }

            var ordersFiltered = GetFilteredOrders()
                .Where(x =>
                x.SelectedShippingDate.HasValue &&
                DbFunctions.TruncateTime(x.SelectedShippingDate.Value) <= today &&
                DbFunctions.TruncateTime(x.SelectedShippingDate.Value) >= controlDate &&
                x.OrderItems.Select(y => y.ProductId).Intersect(productIds).Any());

            var orderItems = ordersFiltered.SelectMany(x => x.OrderItems).Where(x => productIds.Contains(x.ProductId)).ToList();
            var orderItemIds = orderItems.Select(y => y.Id).ToList();

            //var reports = GetOrderItemReport();
            //var reports = GetOrderItemReport();
            //var validReports = reports.Where(x => x.OrderReport.OrderShippingDate >= controlDate && x.OrderReport.OrderShippingDate <= today);

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Producto";
                    worksheet.Cells[row, 2].Value = "Categoría padre";
                    worksheet.Cells[row, 3].Value = "Categoría hijo";
                    worksheet.Cells[row, 4].Value = "Fabricante";
                    worksheet.Cells[row, 5].Value = "Unidad";
                    worksheet.Cells[row, 6].Value = "Cantidad vendida";
                    worksheet.Cells[row, 7].Value = "Precio total";
                    //worksheet.Cells[row, 8].Value = "Costo total";

                    foreach (var product in products)
                    {
                        var filteredItems = orderItems.Where(x => x.ProductId == product.Id).ToList();
                        var qty = GetQty(product, filteredItems.Select(x => x.Quantity).DefaultIfEmpty().Sum());
                        var parentCategories = product.ProductCategories.Where(x => x.Category.ParentCategoryId == 0).Select(x => x.Category.Name).ToList();
                        var childCategories = product.ProductCategories.Where(x => x.Category.ParentCategoryId != 0).Select(x => x.Category.Name).ToList();
                        row++;

                        worksheet.Cells[row, 1].Value = product.Name;
                        worksheet.Cells[row, 2].Value = string.Join(", ", parentCategories);
                        worksheet.Cells[row, 3].Value = string.Join(", ", childCategories);
                        worksheet.Cells[row, 4].Value = manufacturer;
                        worksheet.Cells[row, 5].Value = qty.Item2;
                        worksheet.Cells[row, 6].Value = qty.Item1;
                        worksheet.Cells[row, 7].Value = filteredItems.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
                        //worksheet.Cells[row, 8].Value = validReports.Where(x => filteredItems.Select(y => y.Id).Contains(x.OrderItem.Id))
                        //.Select(x => GetRequestedCost(x.OrderItem, x.OrderReport.UpdatedUnitCost.Value)).DefaultIfEmpty().Sum();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_cantidad_productos_vendidos_ultimos_{days}_dias_{manufacturer}.xlsx");
            }
        }

        // Monto vendido con paypal y tarjeta contra entrega por mes
        [HttpGet]
        public IActionResult GenerateExcel30()
        {
            var today = DateTime.Now.Date;
            var controlDate = new DateTime(2020, 1, 1).Date;

            var ordersGroupedByMonth = GetFilteredOrders()
                .Where(x => x.PaymentMethodSystemName == "Payments.CardOnDelivery" || x.PaymentMethodSystemName == "Payments.PayPalStandard")
                .Where(x => x.SelectedShippingDate >= controlDate)
                .OrderBy(x => x.SelectedShippingDate.Value)
                .ToList()
                .GroupBy(x => x.SelectedShippingDate.Value.Month)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Mes";
                    worksheet.Cells[row, 2].Value = "Monto paypal";
                    worksheet.Cells[row, 3].Value = "Monto tarjeta contra entrega";

                    foreach (var orderGroup in ordersGroupedByMonth)
                    {
                        var date1 = new DateTime(2020, orderGroup.Key, 1).AddMonths(1).AddDays(-2).Date;
                        var date2 = new DateTime(2020, orderGroup.Key, 1).AddMonths(1).AddDays(-1).Date;
                        var validOrders = orderGroup.Where(x => x.SelectedShippingDate >= date1 && x.SelectedShippingDate <= date2).ToList();
                        var groupedValidOrders = validOrders.GroupBy(x => x.SelectedShippingDate.Value.ToString("dd MMMM yyyy", new CultureInfo("es-MX"))).ToList();
                        foreach (var group in groupedValidOrders)
                        {
                            row++;
                            var totalPaypal = group.Where(x => x.PaymentMethodSystemName == "Payments.PayPalStandard").Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                            var totalCard = group.Where(x => x.PaymentMethodSystemName == "Payments.CardOnDelivery").Select(x => x.OrderTotal).DefaultIfEmpty().Sum();

                            worksheet.Cells[row, 1].Value = group.Key;
                            worksheet.Cells[row, 2].Value = totalPaypal;
                            worksheet.Cells[row, 3].Value = totalCard;
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_cantidad_vendida_paypal_tarjetacontraentrega.xlsx");
            }
        }

        // Reporte de la suma de la venta de los últimos {days} días por categoría hijo
        [HttpGet]
        public IActionResult GenerateExcel31(int days = 120)
        {
            var controlDate = DateTime.Now.AddDays(days * -1).Date;
            var today = DateTime.Now.Date;
            var orders = _orderService.GetAllOrdersQuery().Where(x => x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10))
                .Where(x => x.SelectedShippingDate.HasValue &&
                controlDate <= DbFunctions.TruncateTime(x.SelectedShippingDate.Value) &&
                DbFunctions.TruncateTime(x.SelectedShippingDate.Value) <= today);

            var count = _orderService.GetAllOrdersQuery().Where(x => x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10))
                .Where(x => x.ShippingAddress.ZipPostalCode == "01220").ToList();
            var orderItems = orders.SelectMany(x => x.OrderItems).ToList();
            var categories = orderItems
                .SelectMany(x => x.Product.ProductCategories)
                .Select(x => x.Category)
                .Distinct()
                .OrderBy(x => x.Name)
                .ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Categoría padre";
                    worksheet.Cells[row, 2].Value = "Categoría hijo";
                    worksheet.Cells[row, 3].Value = "Monto de venta";

                    foreach (var category in categories.Where(x => x.ParentCategoryId > 0))
                    {
                        row++;
                        var parentCategory = categories.Where(x => x.Id == category.ParentCategoryId).FirstOrDefault();
                        worksheet.Cells[row, 1].Value = parentCategory.Name;
                        worksheet.Cells[row, 2].Value = category.Name;
                        var sold = orderItems
                            .Where(x => x.Product.ProductCategories.Select(y => y.CategoryId).Contains(category.Id))
                            .Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 3].Value = sold;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_suma_venta_ultimos_{days}_dias_por_categoria_hijo.xlsx");
            }
        }

        // Cuántos pedidos han habido por mes, pero sin contar los complementarios
        // O si se usa {byMonthlyCount} como falso:
        // Por mes cuántos pedidos han sido de cuentas que ese mes haya sido su primer pedido
        [HttpGet]
        public IActionResult GenerateExcel32(int months = 12, bool byMonthlyCount = true)
        {
            var controlDate = DateTime.Now.AddMonths(months * -1).Date;
            var today = DateTime.Now.Date;
            var orders = GetFilteredOrders()
                .Where(x => x.SelectedShippingDate.HasValue &&
                controlDate <= DbFunctions.TruncateTime(x.SelectedShippingDate.Value) &&
                DbFunctions.TruncateTime(x.SelectedShippingDate.Value) <= today)
                .OrderByDescending(x => x.SelectedShippingDate)
                .ToList();
            var byMonthGrouping = orders
                .GroupBy(x => x.SelectedShippingDate.Value.ToString("MM-yyyy"))
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    if (byMonthlyCount)
                        worksheet.Cells[row, 1].Value = "Cuántos pedidos han habido por mes, pero sin contar los complementarios";
                    else
                        worksheet.Cells[row, 1].Value = "Por mes cuántos pedidos han sido de cuentas que ese mes haya sido su primer pedido";
                    row++;
                    worksheet.Cells[row, 1].Value = "Mes";
                    worksheet.Cells[row, 2].Value = "Cantidad de ordenes";
                    if (byMonthlyCount)
                        worksheet.Cells[row, 3].Value = "Cantidad de usuarios únicos";

                    foreach (var byMonth in byMonthGrouping)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = byMonth.Key;
                        var byCustomers = byMonth.GroupBy(x => x.Customer);
                        if (byMonthlyCount)
                        {
                            var count = byCustomers.Select(x => x.GroupBy(y => y.SelectedShippingDate.Value.ToString("dd-MM-yyyy")).Count()).DefaultIfEmpty().Sum();
                            worksheet.Cells[row, 2].Value = count;
                            worksheet.Cells[row, 3].Value = byCustomers.Count();
                        }
                        else
                        {
                            var sameMonth = 0;
                            foreach (var client in byCustomers)
                            {
                                var dateCreatedCustomer = client.Key.CreatedOnUtc;
                                var ordersOfClient =
                                    client.Where(x => x.CreatedOnUtc.Month == dateCreatedCustomer.Month
                                    && x.CreatedOnUtc.Year == dateCreatedCustomer.Year).Count();
                                if (ordersOfClient > 0)
                                    sameMonth++;
                            }
                            worksheet.Cells[row, 2].Value = sameMonth;
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                        worksheet.Cells[2, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                if (byMonthlyCount)
                    return File(stream.ToArray(), MimeTypes.TextXlsx,
                        $"cel_cantidad_ordenes_por_mes_sin_complementarios_ultimos_{months}_meses.xlsx");
                else
                    return File(stream.ToArray(), MimeTypes.TextXlsx,
                        $"cel_cantidad_ordenes_por_mes_primer_pedido_ultimos_{months}_meses.xlsx");
            }
        }

        // Lista de pedidos en CP
        [HttpGet]
        public IActionResult GenerateExcel31(string cp)
        {
            var ordersByPostalCode = GetFilteredOrders()
                .Where(x => x.ShippingAddress.ZipPostalCode == cp)
                .OrderBy(x => x.SelectedShippingDate.Value)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "CP";
                    worksheet.Cells[row, 2].Value = "Usuario";
                    worksheet.Cells[row, 3].Value = "# Orden";
                    worksheet.Cells[row, 4].Value = "Fecha solicitada de entrega";
                    worksheet.Cells[row, 5].Value = "Monto";

                    foreach (var order in ordersByPostalCode)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = order.ShippingAddress.ZipPostalCode;
                        worksheet.Cells[row, 2].Value = order.Customer.Email;
                        worksheet.Cells[row, 3].Value = order.CustomOrderNumber;
                        worksheet.Cells[row, 4].Value = order.SelectedShippingDate;
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "dd-MM-yyyy";
                        worksheet.Cells[row, 5].Value = order.OrderTotal;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ordenes_cp_{cp}.xlsx");
            }
        }

        // Cantidad de órdenes diarias por ruta
        [HttpGet]
        public IActionResult GenerateExcel37(int days = 30)
        {
            var controlDate = DateTime.Now.AddDays(days * -1).Date;
            var today = DateTime.Now.Date;

            var orders = GetFilteredOrders()
                .Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today);

            var routes = _shippingRouteService.GetAll().ToList();

            var groupedByRoute = orders.GroupBy(x => x.RouteId).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int column = 1;
                    worksheet.Cells[row, column].Value = "Ruta";
                    for (int i = 0; i < (today - controlDate).TotalDays; i++)
                    {
                        column++;
                        worksheet.Cells[row, column].Value = controlDate.AddDays(column - 2);
                        worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";
                    }

                    foreach (var item in groupedByRoute)
                    {
                        row++;
                        column = 1;
                        worksheet.Cells[row, column].Value = routes.Where(x => x.Id == item.Key).FirstOrDefault().RouteName;
                        for (int i = 0; i < (today - controlDate).TotalDays; i++)
                        {
                            column++;
                            var columnDate = controlDate.AddDays(column - 2);
                            var dateCount = item.Where(x => x.SelectedShippingDate.Value == columnDate).Count();
                            worksheet.Cells[row, column].Value = dateCount;
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ordenes_por_ruta_diario_{days}_dias.xlsx");
            }
        }

        // Cantidad de pedidos por dia
        [HttpGet]
        public IActionResult GenerateExcel38(int days = 120)
        {
            var controlDate = DateTime.Now.AddDays(days * -1).Date;
            var today = DateTime.Now.Date;

            var orders = GetFilteredOrders()
                .Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today);
            var groupedByDate = orders.GroupBy(x => x.SelectedShippingDate.Value).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Fecha";
                    worksheet.Cells[row, 2].Value = "Cantidad de pedidos";

                    foreach (var group in groupedByDate)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = group.Key;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-MM-yyyy";
                        worksheet.Cells[row, 2].Value = group.GroupBy(x => x.ShippingAddress.Address1).Count();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_pedidos_diarios_{days}_dias.xlsx");
            }
        }

        // Productos vendidos monto vendido por dia (en dinero) NORTHSTAR
        [HttpGet]
        public IActionResult GenerateExcel39(int limitDay, int limitMonth, int limitYear, int month, int year)
        {
            //var today = DateTime.Now.Date;
            var today = new DateTime(limitYear, limitMonth, limitDay);
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate.Value.Month == month && x.SelectedShippingDate.Value.Year == year && x.SelectedShippingDate <= today);
            var dates = orders.GroupBy(x => x.SelectedShippingDate).Select(x => x.Key).OrderBy(x => x).ToList();
            var orderItems = orders.Select(x => x.OrderItems).SelectMany(x => x).GroupBy(x => x.Product).ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Producto";
                    worksheet.Cells[row, 2].Value = "Sku";
                    worksheet.Cells[row, 3].Value = "Categoría padre";
                    worksheet.Cells[row, 4].Value = "Categoría hijo";
                    int column = 4;
                    foreach (var date in dates)
                    {
                        column++;
                        worksheet.Cells[row, column].Value = date;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";
                    }

                    foreach (var item in orderItems)
                    {
                        row++;

                        var parentCategories = item.Key.ProductCategories.Where(x => x.Category.ParentCategoryId == 0).Select(x => x.Category.Name).ToList();
                        var childCategories = item.Key.ProductCategories.Where(x => x.Category.ParentCategoryId != 0).Select(x => x.Category.Name).ToList();

                        worksheet.Cells[row, 1].Value = item.Key.Name;
                        worksheet.Cells[row, 2].Value = item.Key.Sku;
                        worksheet.Cells[row, 3].Value = parentCategories;
                        worksheet.Cells[row, 4].Value = childCategories;

                        column = 4;

                        foreach (var date in dates)
                        {
                            column++;
                            var dateTotal = item.Where(x => x.Order.SelectedShippingDate.Value == date)
                                .Select(x => x.PriceInclTax)
                                .DefaultIfEmpty()
                                .Sum(x => x);
                            worksheet.Cells[row, column].Value = dateTotal;
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_productos_vendidos_monto_diario_{month}_{year}.xlsx");
            }
        }

        //// Venta diaria por categoria
        //[HttpGet]
        //public IActionResult GenerateExcel40(int catId, int days = 30)
        //{
        //    var today = DateTime.Now.Date;
        //    var controlDate = DateTime.Now.AddDays(days * -1).Date;
        //    var orders = OrderUtils.GetFilteredOrders(_orderService)
        //        .Where(x => x.SelectedShippingDate.Value >= controlDate && x.SelectedShippingDate <= today);
        //    var dates = orders.GroupBy(x => x.SelectedShippingDate).Select(x => x.Key).OrderBy(x => x).ToList();
        //    var orderItems = orders
        //        .Select(x => x.OrderItems)
        //        .SelectMany(x => x)
        //        .Where(x => x.Product.ProductCategories.Where(y => y.CategoryId == catId).Any())
        //        .GroupBy(x => x.Product)
        //        .ToList();

        //    using (var stream = new MemoryStream())
        //    {
        //        using (var xlPackage = new ExcelPackage(stream))
        //        {
        //            var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
        //            int row = 1;
        //            worksheet.Cells[row, 1].Value = "Producto";
        //            worksheet.Cells[row, 2].Value = "Sku";
        //            worksheet.Cells[row, 3].Value = "Categoría padre";
        //            worksheet.Cells[row, 4].Value = "Categoría hijo";
        //            int column = 4;
        //            foreach (var date in dates)
        //            {
        //                column++;
        //                worksheet.Cells[row, column].Value = date;
        //                worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";
        //            }

        //            foreach (var item in orderItems)
        //            {
        //                row++;

        //                var parentCategories = item.Key.ProductCategories.Where(x => x.Category.ParentCategoryId == 0).Select(x => x.Category.Name).ToList();
        //                var childCategories = item.Key.ProductCategories.Where(x => x.Category.ParentCategoryId != 0).Select(x => x.Category.Name).ToList();

        //                worksheet.Cells[row, 1].Value = item.Key.Name;
        //                worksheet.Cells[row, 2].Value = item.Key.Sku;
        //                worksheet.Cells[row, 3].Value = parentCategories;
        //                worksheet.Cells[row, 4].Value = childCategories;

        //                column = 4;

        //                foreach (var date in dates)
        //                {
        //                    column++;
        //                    var dateTotal = item.Where(x => x.Order.SelectedShippingDate.Value == date)
        //                        .Select(x => x.PriceInclTax)
        //                        .DefaultIfEmpty()
        //                        .Sum(x => x);
        //                    worksheet.Cells[row, column].Value = dateTotal;
        //                }
        //            }

        //            for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
        //            {
        //                worksheet.Column(i).AutoFit();
        //                worksheet.Cells[1, i].Style.Font.Bold = true;
        //            }

        //            xlPackage.Save();
        //        }

        //        var category = orderItems.FirstOrDefault().Select(x => x.Product.ProductCategories.Where(y => y.CategoryId == catId).FirstOrDefault()).FirstOrDefault().Category.Name;

        //        return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_productos_vendidos_diario_{days}_dias_{category}.xlsx");
        //    }
        //}

        // Lista de ordenes con region
        [HttpGet]
        public IActionResult GenerateExcel41(int days = 30)
        {
            var controlDate = DateTime.Now.AddDays(days * -1).Date;
            var today = DateTime.Now.Date;

            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today);
            var pedidos = OrderUtils.GetPedidosGroup(ordersQuery).ToList();
            var regions = _shippingRegionZoneService.GetAll().ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Numero de pedido";
                    worksheet.Cells[row, 2].Value = "Fecha en la que se hizo el pedido";
                    worksheet.Cells[row, 3].Value = "Hora en la que se hizo el pedido";
                    worksheet.Cells[row, 4].Value = "Fecha de entrega seleccionada";
                    worksheet.Cells[row, 5].Value = "Horario de entrega seleccionado";
                    worksheet.Cells[row, 6].Value = "Código postal";
                    worksheet.Cells[row, 7].Value = "Region";
                    worksheet.Cells[row, 8].Value = "Numero de productos diferentes";
                    worksheet.Cells[row, 9].Value = "Total en dinero";

                    foreach (var pedidoGroup in pedidos)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = string.Join(", ", pedidoGroup.Select(x => "#" + x.CustomOrderNumber).ToList());
                        worksheet.Cells[row, 2].Value = pedidoGroup.Select(x => x.CreatedOnUtc.ToLocalTime()).FirstOrDefault();
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 3].Value = pedidoGroup.Select(x => x.CreatedOnUtc.ToLocalTime()).FirstOrDefault();
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "hh:mm:ss";
                        worksheet.Cells[row, 4].Value = pedidoGroup.Select(x => x.SelectedShippingDate.Value).FirstOrDefault();
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 5].Value = pedidoGroup.Select(x => x.SelectedShippingTime).FirstOrDefault();
                        worksheet.Cells[row, 6].Value = pedidoGroup.Select(x => x.ShippingAddress.ZipPostalCode).FirstOrDefault();
                        worksheet.Cells[row, 7].Value = regions
                            .Where(x => (x.Zone.PostalCodes + "," + x.Zone.AdditionalPostalCodes)
                            .Contains(pedidoGroup
                            .Select(y => y.ShippingAddress.ZipPostalCode).FirstOrDefault())).FirstOrDefault()?.Region?.Name;
                        worksheet.Cells[row, 8].Value = pedidoGroup.Select(x => x.OrderItems).SelectMany(x => x).GroupBy(x => x.ProductId).Count();
                        worksheet.Cells[row, 9].Value = pedidoGroup.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ordenes_region_{days}_dias.xlsx");
            }
        }

        // Usuarios, ventas, ticket promedio por mes NorthStar
        [HttpGet]
        public IActionResult GenerateExcel42(int month, int year, int byMonths = 0, int fabId = 0)
        {
            var today = DateTime.Now.Date;
            if (year == 0) year = today.Year;

            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService);
            if (byMonths > 0)
            {
                var controlDate = today.AddMonths(byMonths * -1);
                ordersQuery = ordersQuery.Where(x => x.SelectedShippingDate.Value >= controlDate);
            }
            else
            {
                ordersQuery = ordersQuery.Where(x => x.SelectedShippingDate.Value.Month == month && x.SelectedShippingDate.Value.Year == year);
            }

            var grouped = ordersQuery.ToList()
                .GroupBy(x => new DateTime(x.SelectedShippingDate.Value.Year, x.SelectedShippingDate.Value.Month, 1))
                .ToList();
            var customers = _customerService.GetAllCustomers().Where(x => !string.IsNullOrEmpty(x.Email)).ToList();
            var products = _productService.GetAllProductsQuery().ToList();
            var allPedidos = OrderUtils.GetPedidosOnly(OrderUtils.GetFilteredOrders(_orderService)).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Mes";
                    worksheet.Cells[row, 2].Value = "Usuarios registrados (acumulado)";
                    worksheet.Cells[row, 3].Value = "Ventas del mes";
                    worksheet.Cells[row, 4].Value = "Ticket promedio por pedido";
                    worksheet.Cells[row, 5].Value = "Numero de productos en catálogo";
                    worksheet.Cells[row, 6].Value = "Numero de clientes con más de un pedido";
                    worksheet.Cells[row, 7].Value = "Promedio de órdenes realizadas por los clientes que han hecho más de un pedido (acumulado)";
                    worksheet.Cells[row, 8].Value = "Número de clientes que hicieron su primer pedido en ese mes";
                    worksheet.Cells[row, 9].Value = "Número de clientes que hicieron algún pedido en el mes";

                    List<decimal> ordersCountList = new List<decimal>();
                    foreach (var groupedItem in grouped)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = groupedItem.Key.ToString("MMMM, yyyy", new CultureInfo("es-MX"));
                        worksheet.Cells[row, 2].Value = customers.Where(x => x.CreatedOnUtc.ToLocalTime() <= groupedItem.Key.AddMonths(1).AddSeconds(-1)).Count();
                        worksheet.Cells[row, 3].Value = groupedItem.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();

                        var pedidos = OrderUtils.GetPedidosGroupByList(groupedItem.Select(x => x).ToList());
                        var averageByPedido = pedidos.Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).Average();
                        worksheet.Cells[row, 4].Value = averageByPedido;
                        worksheet.Cells[row, 5].Value = products.Where(x => x.CreatedOnUtc.ToLocalTime() <= groupedItem.Key.AddMonths(1).AddSeconds(-1)).Count();

                        int customerWithMultipleOrdersCount = 0;
                        int customerFirstOrder = 0;
                        int orderThisMonthCount = 0;
                        var customerIds = groupedItem.GroupBy(x => x.CustomerId).Select(x => x.Key).ToList();

                        foreach (var customerId in customerIds)
                        {
                            int previousOrderCount = allPedidos
                                .Where(x => x.SelectedShippingDate < groupedItem.Key.AddMonths(1).AddSeconds(-1) && x.CustomerId == customerId)
                                .Count();
                            if (previousOrderCount > 0)
                            {
                                customerWithMultipleOrdersCount++;
                                ordersCountList.Add(previousOrderCount);
                            }

                            bool hasPreviousOrder = allPedidos
                                .Where(x => x.SelectedShippingDate < groupedItem.Key && x.CustomerId == customerId)
                                .Any();
                            if (!hasPreviousOrder) customerFirstOrder++;

                            bool orderThisMonth = allPedidos
                                .Where(x => x.SelectedShippingDate.Value.Month == groupedItem.Key.Month && x.SelectedShippingDate.Value.Year == groupedItem.Key.Year && x.CustomerId == customerId)
                                .Any();
                            if (orderThisMonth) orderThisMonthCount++;
                        }
                        worksheet.Cells[row, 6].Value = customerWithMultipleOrdersCount;
                        worksheet.Cells[row, 7].Value = ordersCountList.Average();
                        worksheet.Cells[row, 8].Value = customerFirstOrder;
                        worksheet.Cells[row, 9].Value = orderThisMonthCount;

                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_usuarios_ventas_ticketpromedio_{month}_{year}.xlsx");
            }
        }

        // Monto vendido por region NORTHSTAR
        [HttpGet]
        public IActionResult GenerateExcel43(int month, int year = 0)
        {
            var today = DateTime.Now.Date;
            if (year == 0) year = today.Year;
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate.Value.Month == month && x.SelectedShippingDate.Value.Year == year)
                .ToList();
            var regions = _shippingRegionZoneService.GetAll().GroupBy(x => x.Region).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Región";
                    worksheet.Cells[row, 2].Value = "Cantidad de pedidos";
                    worksheet.Cells[row, 3].Value = "Monto vendido";

                    foreach (var region in regions)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = region.Key.Name;

                        var postalCodes = region.Select(x => x.Zone.PostalCodes + "," + x.Zone.AdditionalPostalCodes);
                        var postalCodeList = postalCodes.SelectMany(x => x.Split(',').Select(y => y.Trim())).ToList();
                        var regionOrders = orders.Where(x => postalCodeList.Contains(x.ShippingAddress.ZipPostalCode)).ToList();
                        var pedidos = OrderUtils.GetPedidosGroupByList(regionOrders);

                        worksheet.Cells[row, 2].Value = pedidos.Count();
                        worksheet.Cells[row, 3].Value = regionOrders.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ventas_por_region_{month}_{year}.xlsx");
            }
        }

        // pedidos por dia NORTHSTAR
        [HttpGet]
        public IActionResult GenerateExcel44(int month, int year = 0)
        {
            var today = DateTime.Now.Date;
            if (year == 0) year = today.Year;
            var ordersGroup = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate.Value.Month == month && x.SelectedShippingDate.Value.Year == year)
                .GroupBy(x => x.SelectedShippingDate.Value)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Fecha";
                    worksheet.Cells[row, 2].Value = "Cantidad de pedidos";
                    worksheet.Cells[row, 3].Value = "Monto vendido";

                    foreach (var group in ordersGroup)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = group.Key;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mm-yyyy";
                        var pedidos = OrderUtils.GetPedidosGroupByList(group.Select(x => x).ToList());
                        worksheet.Cells[row, 2].Value = pedidos.Count();
                        worksheet.Cells[row, 3].Value = pedidos.Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Sum();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_pedidos_por_dia_{month}_{year}.xlsx");
            }
        }

        // venta por producto
        [HttpGet]
        public IActionResult GenerateExcel45(string product, int days = 90)
        {
            var today = DateTime.Now.Date;
            var controlDate = DateTime.Now.AddDays(days * -1).Date;
            var grouped = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate > controlDate && x.SelectedShippingDate <= today)
                .SelectMany(x => x.OrderItems)
                .Where(x => x.Product.Name.ToLower().Contains(product))
                .GroupBy(x => x.Product)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Producto";
                    worksheet.Cells[row, 2].Value = "Unidad";
                    worksheet.Cells[row, 3].Value = "Cantidad vendida";
                    worksheet.Cells[row, 4].Value = "Monto vendido";

                    foreach (var group in grouped)
                    {
                        row++;
                        var qty = GetQty(group.Key, group.Select(x => x.Quantity).Sum());
                        worksheet.Cells[row, 1].Value = group.Key.Name;
                        worksheet.Cells[row, 2].Value = qty.Item2;
                        worksheet.Cells[row, 3].Value = qty.Item1;
                        worksheet.Cells[row, 4].Value = group.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_venta_{product}_{days}_dias.xlsx");
            }
        }

        [HttpGet]
        public IActionResult GenerateExcel46(int month)
        {
            var customers = _customerService.GetAllCustomers().Where(x => x.Active && x.Email != null);
            var orders = OrderUtils.GetFilteredOrders(_orderService).Select(x => new GenerateExcel46Model()
            {
                ShippingAddress = x.ShippingAddress.Address1,
                CustomerId = x.CustomerId,
                SelectedShippingDate = x.SelectedShippingDate,
                SelectedShippingTime = x.SelectedShippingTime,
                OrderTotal = x.OrderTotal
            }).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Correo";
                    worksheet.Cells[row, 2].Value = "Nombre";
                    worksheet.Cells[row, 3].Value = "Apellido";
                    worksheet.Cells[row, 4].Value = "Telefono";
                    worksheet.Cells[row, 5].Value = "# de pedidos histórico";
                    worksheet.Cells[row, 6].Value = "Total de ventas histórico";
                    worksheet.Cells[row, 7].Value = "Fecha de última orden";
                    worksheet.Cells[row, 8].Value = "Fecha de nacimiento";

                    foreach (var customer in customers)
                    {
                        var birthDay = customer.GetAttribute<DateTime?>(SystemCustomerAttributeNames.DateOfBirth);
                        if (!birthDay.HasValue) continue;
                        if (birthDay.HasValue && birthDay.Value.Month != month) continue;
                        row++;

                        var filteredOrders = orders.Where(x => x.CustomerId == customer.Id).ToList();
                        var pedidosCount = 0;
                        if (filteredOrders.Count() > 0)
                            pedidosCount = OrderUtils.GetSimplePedidosGroupByList(filteredOrders).Count();

                        worksheet.Cells[row, 1].Value = customer.Email;
                        worksheet.Cells[row, 2].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                        worksheet.Cells[row, 3].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
                        var phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                        worksheet.Cells[row, 4].Value = string.IsNullOrEmpty(phone) ? customer.Addresses.Select(x => x.PhoneNumber).FirstOrDefault() : phone;
                        worksheet.Cells[row, 5].Value = pedidosCount;
                        worksheet.Cells[row, 6].Value = filteredOrders.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 7].Value = filteredOrders.Select(x => x.SelectedShippingDate).OrderByDescending(x => x).FirstOrDefault();
                        worksheet.Cells[row, 7].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 8].Value = birthDay;
                        worksheet.Cells[row, 8].Style.Numberformat.Format = "dd-mm-yyyy";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_fechanacimiento_mes_{month}.xlsx");
            }
        }

        // Ventas mensuales por producto
        [HttpGet]
        public IActionResult GenerateExcel47(int months = 6, int fbId = 0)
        {
            var today = DateTime.Now.Date;
            var controlDate = DateTime.Now.AddMonths(months * -1);
            var controlDate2 = new DateTime(controlDate.Year, controlDate.Month, 1);
            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate >= controlDate2 && x.SelectedShippingDate <= today);
            string extraName = string.Empty;
            if (fbId > 0)
            {
                extraName = _manufacturerService.GetManufacturerById(fbId)?.Name;
                ordersQuery = ordersQuery.Where(x => x.OrderItems.Select(y => y.Product.ProductManufacturers.Where(z => !z.Manufacturer.Deleted && z.ManufacturerId == fbId).Any()).Any());
            }

            var grouped = ordersQuery.ToList()
                .GroupBy(x => new DateTime(x.SelectedShippingDate.Value.Year, x.SelectedShippingDate.Value.Month, 1))
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Mes";
                    worksheet.Cells[row, 2].Value = "Venta total";

                    foreach (var group in grouped)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = group.Key;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "mm yyyy";
                        worksheet.Cells[row, 2].Value = group.SelectMany(x => x.OrderItems.Where(y => y.Product.ProductManufacturers.Where(z => !z.Manufacturer.Deleted && z.ManufacturerId == fbId).Any()))
                            .Select(x => x.PriceInclTax)
                            .DefaultIfEmpty()
                            .Sum();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ventas_mensuales_{months}_meses{(!string.IsNullOrWhiteSpace(extraName) ? ("_" + extraName) : "")}.xlsx");
            }
        }

        // Ventas mensuales por categoría padre
        [HttpGet]
        public IActionResult GenerateExcel48(int initMonth, int initYear, int endMonth, int endYear)
        {
            var initDate = new DateTime(initYear, initMonth, 1);
            var endDate = new DateTime(endYear, endMonth, 1).AddMonths(1).AddDays(-1);
            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= initDate && x.SelectedShippingDate <= endDate);

            var orderItems = ordersQuery.SelectMany(x => x.OrderItems).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int col = 1;

                    worksheet.Cells[row, col].Value = "Categoría padre";

                    for (DateTime d = initDate; d <= endDate; d = d.AddMonths(1))
                    {
                        col++;
                        worksheet.Cells[row, col].Value = d;
                        worksheet.Cells[row, col].Style.Numberformat.Format = "mmmm, yyyy";
                    }

                    col = 1;
                    var groupedByCategory = orderItems.GroupBy(x => x.Product.ProductCategories.Where(y => !y.Category.Deleted && y.Category.ParentCategoryId == 0).FirstOrDefault()?.Category).ToList();
                    foreach (var group in groupedByCategory)
                    {
                        row++;
                        var categoryName = group.Key == null ? "Sin categoría padre" : group.Key.Name;
                        worksheet.Cells[row, col].Value = categoryName;
                        for (DateTime d = initDate; d <= endDate; d = d.AddMonths(1))
                        {
                            col++;
                            var filteredItems = group
                                .Where(x => x.Order.SelectedShippingDate.Value.Month == d.Month &&
                                x.Order.SelectedShippingDate.Value.Year == d.Year);
                            worksheet.Cells[row, col].Value = filteredItems.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ventas_mensuales_categoria_padre_{initDate.ToString("MM-yyyy")}_{endDate.ToString("MM-yyyy")}.xlsx");
            }
        }

        // Ventas mensuales por categoría hijo
        [HttpGet]
        public IActionResult GenerateExcel49(int initMonth, int initYear, int endMonth, int endYear)
        {
            var initDate = new DateTime(initYear, initMonth, 1);
            var endDate = new DateTime(endYear, endMonth, 1).AddMonths(1).AddDays(-1);
            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= initDate && x.SelectedShippingDate <= endDate);

            var orderItems = ordersQuery.SelectMany(x => x.OrderItems).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int col = 1;

                    worksheet.Cells[row, col].Value = "Categoría hijo";

                    for (DateTime d = initDate; d <= endDate; d = d.AddMonths(1))
                    {
                        col++;
                        worksheet.Cells[row, col].Value = d;
                        worksheet.Cells[row, col].Style.Numberformat.Format = "mmmm, yyyy";
                    }

                    col = 1;
                    var groupedByCategory = orderItems.GroupBy(x => x.Product.ProductCategories.Where(y => !y.Category.Deleted && y.Category.ParentCategoryId > 0).FirstOrDefault()?.Category).ToList();
                    foreach (var group in groupedByCategory)
                    {
                        row++;
                        var categoryName = group.Key == null ? "Sin categoría hijo" : group.Key.Name;
                        worksheet.Cells[row, col].Value = categoryName;
                        for (DateTime d = initDate; d <= endDate; d = d.AddMonths(1))
                        {
                            col++;
                            var filteredItems = group
                                .Where(x => x.Order.SelectedShippingDate.Value.Month == d.Month &&
                                x.Order.SelectedShippingDate.Value.Year == d.Year);
                            worksheet.Cells[row, col].Value = filteredItems.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ventas_mensuales_categoria_hijo_{initDate.ToString("MM-yyyy")}_{endDate.ToString("MM-yyyy")}.xlsx");
            }
        }

        // Ventas mensuales de web y app
        [HttpGet]
        public IActionResult GenerateExcel50(int initMonth, int initYear, int endMonth, int endYear)
        {
            var initDate = new DateTime(initYear, initMonth, 1);
            var endDate = new DateTime(endYear, endMonth, 1).AddMonths(1).AddDays(-1);
            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= initDate && x.SelectedShippingDate <= endDate);
            var orderGroup = ordersQuery.ToList()
                .GroupBy(x => new DateTime(x.SelectedShippingDate.Value.Year, x.SelectedShippingDate.Value.Month, 1))
                .ToList();
            var mobileOrderIds = ordersQuery.Where(x => x.OrderNotes.Where(y => y.Note == "Orden creada desde aplicación móvil.").Any()).Select(x => x.Id).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Mes";
                    worksheet.Cells[row, 2].Value = "Ventas web";
                    worksheet.Cells[row, 3].Value = "Pedidos web";
                    worksheet.Cells[row, 4].Value = "Ventas app móvil";
                    worksheet.Cells[row, 5].Value = "Pedidos app móvil";

                    foreach (var group in orderGroup)
                    {
                        row++;
                        var webPedidos = OrderUtils.GetPedidosGroupByList(group.Where(x => !mobileOrderIds.Contains(x.Id)).ToList());
                        var appPedidos = OrderUtils.GetPedidosGroupByList(group.Where(x => mobileOrderIds.Contains(x.Id)).ToList());
                        worksheet.Cells[row, 1].Value = group.Key;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "mmmm, yyyy";
                        worksheet.Cells[row, 2].Value = group.Where(x => !mobileOrderIds.Contains(x.Id)).Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 3].Value = webPedidos.Count();
                        worksheet.Cells[row, 4].Value = group.Where(x => mobileOrderIds.Contains(x.Id)).Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 5].Value = appPedidos.Count();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ventas_mensuales_web_app_{initDate:MM-yyyy}_{endDate:MM-yyyy}.xlsx");
            }
        }

        // Ventas mensuales por método de pago
        [HttpGet]
        public IActionResult GenerateExcel51(int initMonth, int initYear, int endMonth, int endYear)
        {
            var initDate = new DateTime(initYear, initMonth, 1);
            var endDate = new DateTime(endYear, endMonth, 1).AddMonths(1).AddDays(-1);
            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= initDate && x.SelectedShippingDate <= endDate);
            var orderGroup = ordersQuery.ToList()
                .GroupBy(x => new DateTime(x.SelectedShippingDate.Value.Year, x.SelectedShippingDate.Value.Month, 1))
                .OrderBy(x => x.Key)
                .ToList();
            var paymentMethods = ordersQuery.GroupBy(x => x.PaymentMethodSystemName).Select(x => x.Key).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int col = 1;

                    worksheet.Cells[row, col].Value = "Método de pago";
                    foreach (var group in orderGroup)
                    {
                        col++;
                        worksheet.Cells[row, col].Value = group.Key;
                        worksheet.Cells[row, col].Style.Numberformat.Format = "mmmm, yyyy";
                    }

                    foreach (var paymentMethod in paymentMethods)
                    {
                        row++;
                        col = 1;
                        worksheet.Cells[row, col].Value = paymentMethod;
                        foreach (var group in orderGroup)
                        {
                            col++;
                            worksheet.Cells[row, col].Value = group.Where(x => x.PaymentMethodSystemName == paymentMethod).Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ventas_mensuales_metodo_pago_{initDate:MM-yyyy}_{endDate:MM-yyyy}.xlsx");
            }
        }

        // Cantidad de fabricantes por categoría padre
        [HttpGet]
        public IActionResult GenerateExcel52()
        {
            var products = _productService.GetAllProductsQuery().Where(x => !x.Deleted).ToList();
            var categories = _categoryService.GetAllCategories().Where(x => !x.Deleted && x.ParentCategoryId == 0);

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Categoría padre";
                    worksheet.Cells[row, 2].Value = "Cantidad de fabricantes";
                    foreach (var category in categories)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = category.Name;
                        var filteredProducts = products.Where(x => x.ProductCategories.Where(y => !y.Category.Deleted).Select(y => y.CategoryId).Contains(category.Id));
                        var productManufacturers = filteredProducts.SelectMany(x => x.ProductManufacturers.Where(y => !y.Manufacturer.Deleted).Select(y => y.ManufacturerId)).GroupBy(x => x).Select(x => x.Key);
                        worksheet.Cells[row, 2].Value = productManufacturers.Count();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_fabricantes_por_categoria_padre.xlsx");
            }
        }

        // Lista de categorias por fabricante
        [HttpGet]
        public IActionResult GenerateExcel53()
        {
            var products = _productService.GetAllProductsQuery().Where(x => !x.Deleted).ToList();
            var manufacturers = _manufacturerService.GetAllManufacturers().Where(x => !x.Deleted);

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Fabricante";
                    worksheet.Cells[row, 2].Value = "Categorías padre";
                    foreach (var manufacturer in manufacturers)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = manufacturer.Name;
                        var filteredProducts = products.Where(x => x.ProductManufacturers.Where(y => !y.Manufacturer.Deleted).Select(y => y.ManufacturerId).Contains(manufacturer.Id));
                        var parentCategories = filteredProducts.SelectMany(x => x.ProductCategories.Where(y => !y.Category.Deleted && y.Category.ParentCategoryId == 0).Select(y => y.Category)).GroupBy(x => x.Id).Select(x => x.FirstOrDefault());
                        worksheet.Cells[row, 2].Value = string.Join(", ", parentCategories.Select(x => x.Name));
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_categorias_por_fabricante.xlsx");
            }
        }

        // Lista de pedidos entregados tarde por franquicia
        [HttpGet]
        public IActionResult GenerateExcel54(int franchiseId)
        {
            var franchiseVehicles = _shippingVehicleService.GetAll().Where(x => x.FranchiseId == franchiseId).ToList();
            var franchiseOrders = OrderUtils.GetAllFranchiseOrders(franchiseVehicles, _shippingVehicleRouteService, _orderService).Where(x => x.OrderStatusId == 30);
            var franchisePedidos = OrderUtils.GetPedidosGroup(franchiseOrders).ToList();
            var franchiseName = franchiseVehicles.FirstOrDefault()?.Franchise?.Name;

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Pedido";
                    worksheet.Cells[row, 2].Value = "Fecha seleccionada de entrega";
                    worksheet.Cells[row, 3].Value = "Horario seleccionado de entrega";
                    worksheet.Cells[row, 4].Value = "Hora registrada de entrega";
                    worksheet.Cells[row, 5].Value = "Minutos de retraso";
                    foreach (var pedido in franchisePedidos)
                    {
                        var endTimeObject = GetSelectedEndTimeSpan(pedido.FirstOrDefault().SelectedShippingTime);
                        var delivered = pedido.SelectMany(x => x.Shipments.Select(y => y.DeliveryDateUtc)).Where(x => x.HasValue).OrderBy(x => x.Value).FirstOrDefault().Value.ToLocalTime();
                        var controlDate = new DateTime(delivered.Year, delivered.Month, delivered.Day, endTimeObject.Hours, endTimeObject.Minutes, endTimeObject.Seconds);
                        var diff = (delivered - controlDate).TotalMinutes;
                        if (diff < 0) continue;

                        row++;
                        worksheet.Cells[row, 1].Value = string.Join(", ", pedido.Select(x => "#" + x.Id));
                        worksheet.Cells[row, 2].Value = pedido.FirstOrDefault().SelectedShippingDate.Value;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 3].Value = pedido.FirstOrDefault().SelectedShippingTime;
                        worksheet.Cells[row, 4].Value = delivered;
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "hh:MM:ss";
                        worksheet.Cells[row, 5].Value = diff;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_pedidos_entregados_tarde_{franchiseName}.xlsx");
            }
        }

        // Clientes meses consecutivos comprando desde su registro
        [HttpGet]
        public IActionResult GenerateExcel55()
        {
            var customers = OrderUtils.GetFilteredOrders(_orderService).Select(x => x.Customer).GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToList();
            var orders = OrderUtils.GetFilteredOrders(_orderService).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Correo electrónico";
                    worksheet.Cells[row, 2].Value = "Fecha de registro";
                    worksheet.Cells[row, 3].Value = "Cantidad de pedidos";
                    worksheet.Cells[row, 4].Value = "Fecha de primera entrega";
                    worksheet.Cells[row, 5].Value = "Fecha de última entrega";
                    worksheet.Cells[row, 6].Value = "Cantidad de meses consecutivos pidiendo";

                    foreach (var customer in customers)
                    {
                        var customerOrders = orders.Where(x => x.CustomerId == customer.Id).ToList();

                        row++;
                        worksheet.Cells[row, 1].Value = customer.Email;
                        worksheet.Cells[row, 2].Value = customer.CreatedOnUtc.ToLocalTime();
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 3].Value = OrderUtils.GetPedidosGroupByList(customerOrders).Count();

                        var firstOrderDate = customerOrders.Select(x => x.SelectedShippingDate.Value).OrderBy(x => x).FirstOrDefault();
                        worksheet.Cells[row, 4].Value = firstOrderDate;
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "dd-mm-yyyy";

                        var lastOrderDate = customerOrders.Select(x => x.SelectedShippingDate.Value).OrderBy(x => x).Last();
                        worksheet.Cells[row, 5].Value = lastOrderDate;
                        worksheet.Cells[row, 5].Style.Numberformat.Format = "dd-mm-yyyy";

                        int count = 0;
                        bool keepCounting = true;
                        DateTime controlDate = firstOrderDate;
                        do
                        {
                            controlDate = controlDate.AddMonths(1);
                            keepCounting = orders.Where(x => x.SelectedShippingDate.Value.Month == controlDate.Month && x.SelectedShippingDate.Value.Year == controlDate.Year && x.CustomerId == customer.Id).Any();
                            if (keepCounting) count++;
                        } while (keepCounting);
                        worksheet.Cells[row, 6].Value = count;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_pedidos_consecutivos_por_cliente.xlsx");
            }
        }

        // Venta por categoría padre
        [HttpGet]
        public IActionResult GenerateExcel56(int days = 30)
        {
            var today = DateTime.Now.Date;
            var controlDate = today.AddDays(days * -1).Date;

            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today);

            var orderItems = orders.Select(x => x.OrderItems)
                .SelectMany(x => x)
                .GroupBy(x => x.Product.ProductCategories.Where(y => y.Category.ParentCategoryId == 0).OrderBy(y => y.Category.DisplayOrder).Select(y => y.Category).FirstOrDefault())
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Categoría padre";
                    worksheet.Cells[row, 2].Value = "Monto vendido";

                    foreach (var item in orderItems)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = item.Key == null ? "Sin categoría padre" : item.Key.Name;
                        worksheet.Cells[row, 2].Value = item.DefaultIfEmpty().Sum(x => x.PriceInclTax);
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_vendido_categoria_padre_{days}_dias.xlsx");
            }
        }

        // Lista de clasificación de entrega por pedido
        [HttpGet]
        public IActionResult GenerateExcel57(int days)
        {
            var controlDate = DateTime.Now.AddDays(days * -1).Date;
            var today = DateTime.Now.Date;
            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today && x.ShippingStatusId == 40);
            var pedidos = OrderUtils.GetPedidosOnly(ordersQuery).ToList();
            var months = ordersQuery.Select(x => x.SelectedShippingDate.Value).ToList().GroupBy(x => new DateTime(x.Year, x.Month, 1)).Select(x => x.Key).OrderBy(x => x).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Mes";
                    worksheet.Cells[row, 2].Value = "Entregados a tiempo";
                    worksheet.Cells[row, 3].Value = "0 a 14 minutos";
                    worksheet.Cells[row, 4].Value = "más de 15 minutos";

                    foreach (var date in months)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = date;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "mmmm, yyyy";

                        var filteredPedidos = pedidos.Where(x => x.SelectedShippingDate.Value.Month == date.Month && x.SelectedShippingDate.Value.Year == date.Year);

                        int total = filteredPedidos.Count();
                        int inTimeCount = 0;
                        int lateLevel1Count = 0;
                        int lateLevel2Count = 0;
                        foreach (var pedido in filteredPedidos)
                        {
                            var endTimeObject = GetSelectedEndTimeSpan(pedido.SelectedShippingTime);
                            var delivered = pedido.Shipments.Select(x => x.DeliveryDateUtc).Where(x => x.HasValue).OrderBy(x => x.Value).FirstOrDefault().Value.ToLocalTime();

                            DateTime deliveredLimit = new DateTime(delivered.Year, delivered.Month, delivered.Day, endTimeObject.Hours, endTimeObject.Minutes, endTimeObject.Seconds);
                            double diff = (delivered - deliveredLimit).TotalMinutes;
                            if (diff <= 0) inTimeCount++;
                            if (diff > 0 && diff <= 14) lateLevel1Count++;
                            if (diff > 14) lateLevel2Count++;
                        }

                        worksheet.Cells[row, 2].Value = (decimal)inTimeCount / (decimal)total;
                        worksheet.Cells[row, 3].Value = (decimal)lateLevel1Count / (decimal)total;
                        worksheet.Cells[row, 4].Value = (decimal)lateLevel2Count / (decimal)total;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_porcentaje_tiempo_entrega_pedidos_{days}_dias.xlsx");
            }
        }

        // Ticket promedio mensual
        [HttpGet]
        public IActionResult GenerateExcel58(int reportMonths)
        {
            var controlDate = DateTime.Now.AddMonths(reportMonths * -1).Date;
            var today = DateTime.Now.Date;
            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today);
            var pedidos = OrderUtils.GetPedidosGroup(ordersQuery).ToList();
            var months = ordersQuery.Select(x => x.SelectedShippingDate.Value).ToList().GroupBy(x => new DateTime(x.Year, x.Month, 1)).Select(x => x.Key).OrderBy(x => x).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Mes";
                    worksheet.Cells[row, 2].Value = "Ticket promedio";

                    foreach (var date in months)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = date;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "mmmm, yyyy";

                        var filteredPedidos = pedidos.Where(x => x.FirstOrDefault().SelectedShippingDate.Value.Month == date.Month && x.FirstOrDefault().SelectedShippingDate.Value.Year == date.Year);
                        decimal averageTicket = filteredPedidos.Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();
                        worksheet.Cells[row, 2].Value = averageTicket;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ticket_promedio_mensual_{reportMonths}_meses.xlsx");
            }
        }

        // Ventas por fabricante
        [HttpGet]
        public IActionResult GenerateExcel59(int fbId, int days = 30)
        {
            var today = DateTime.Now.Date;
            var controlDate = today.AddDays(-1 * days);

            var products = _productService.GetAllProductsQuery()
                .Where(x => !x.Deleted && x.ProductManufacturers.Where(y => y.ManufacturerId == fbId).Any())
                .OrderBy(x => x.Name)
                .ToList();
            var productIds = products.Select(x => x.Id).ToList();

            var manufacturer = products.SelectMany(x => x.ProductManufacturers).Where(x => x.ManufacturerId == fbId).FirstOrDefault()?.Manufacturer?.Name;

            var orderItems = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today)
                .SelectMany(x => x.OrderItems)
                .Where(x => productIds.Contains(x.ProductId));

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Producto";
                    worksheet.Cells[row, 2].Value = "Monto vendido";

                    foreach (var product in products)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = product.Name;
                        worksheet.Cells[row, 2].Value = orderItems.Where(x => x.ProductId == product.Id).Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ventas_por_fabricante_{manufacturer}.xlsx");
            }
        }

        // cantidad de productos por fabricante
        //[HttpGet]
        //public IActionResult GenerateExcel60()
        //{
        //    var manufacturers = _manufacturerService.GetAllManufacturers().Where(x => !x.Deleted);
        //    var products = _productService.GetAllProductsQuery().Where(x => !x.Deleted).ToList();

        //    using (var stream = new MemoryStream())
        //    {
        //        using (var xlPackage = new ExcelPackage(stream))
        //        {
        //            var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
        //            int row = 1;

        //            worksheet.Cells[row, 1].Value = "Fabricante";
        //            worksheet.Cells[row, 2].Value = "Cantidad de productos";

        //            foreach (var manufacturer in manufacturers)
        //            {
        //                row++;
        //                worksheet.Cells[row, 1].Value = manufacturer.Name;
        //                worksheet.Cells[row, 2].Value = products.Where(x => ProductUtils.GetMainManufacturer(x.ProductManufacturers)?.Id == manufacturer.Id).Count();
        //            }

        //            for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
        //            {
        //                worksheet.Column(i).AutoFit();
        //                worksheet.Cells[1, i].Style.Font.Bold = true;
        //            }

        //            xlPackage.Save();
        //        }

        //        return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_cantidad_productos_por_fabricante.xlsx");
        //    }
        //}

        // Número de pedidos por semana
        [HttpGet]
        public IActionResult GenerateExcel61(int days = 30)
        {
            var today = DateTime.Now.Date;
            var controlDate = today.AddDays(-1 * days);
            while (controlDate.DayOfWeek > 0)
            {
                controlDate = controlDate.AddDays(-1);
            }

            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today);
            var groupedByWeek = orders
                .OrderBy(x => x.SelectedShippingDate)
                .ToList()
                .GroupBy(x => x.SelectedShippingDate.Value.AddDays(-(int)x.SelectedShippingDate.Value.DayOfWeek))
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int weekCount = 1;

                    worksheet.Cells[row, 1].Value = "Semana";
                    worksheet.Cells[row, 2].Value = "Fecha inicio";
                    worksheet.Cells[row, 3].Value = "Fecha final";
                    worksheet.Cells[row, 4].Value = "Número de pedidos";

                    foreach (var group in groupedByWeek)
                    {
                        row++;
                        var pedidos = OrderUtils.GetPedidosGroupByList(group.Select(x => x).ToList()).ToList();

                        worksheet.Cells[row, 1].Value = weekCount;
                        worksheet.Cells[row, 2].Value = group.Key;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 3].Value = group.Key.AddDays(6);
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 4].Value = pedidos.Count;
                        weekCount++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_pedidos_semanales_{days}_dias.xlsx");
            }
        }

        //Monto de ventas por semana
        [HttpGet]
        public IActionResult GenerateExcel62(int days = 30)
        {
            var today = DateTime.Now.Date;
            var controlDate = today.AddDays(-1 * days);
            while (controlDate.DayOfWeek > 0)
            {
                controlDate = controlDate.AddDays(-1);
            }

            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today);
            var groupedByWeek = orders
                .OrderBy(x => x.SelectedShippingDate)
                .ToList()
                .GroupBy(x => x.SelectedShippingDate.Value.AddDays(-(int)x.SelectedShippingDate.Value.DayOfWeek))
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int weekCount = 1;

                    worksheet.Cells[row, 1].Value = "Semana";
                    worksheet.Cells[row, 2].Value = "Fecha inicio";
                    worksheet.Cells[row, 3].Value = "Fecha final";
                    worksheet.Cells[row, 4].Value = "Venta total";

                    foreach (var group in groupedByWeek)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = weekCount;
                        worksheet.Cells[row, 2].Value = group.Key;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 3].Value = group.Key.AddDays(6);
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 4].Value = group.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                        weekCount++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ventas_semanales_{days}_dias.xlsx");
            }
        }

        //Cuentas activas nuevas por semana
        [HttpGet]
        public IActionResult GenerateExcel63(int days = 30)
        {
            var today = DateTime.Now.Date;
            var controlDate = today.AddDays(-1 * days);
            while (controlDate.DayOfWeek > 0)
            {
                controlDate = controlDate.AddDays(-1);
            }

            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today);
            var groupedByWeek = orders
                .OrderBy(x => x.SelectedShippingDate)
                .ToList()
                .GroupBy(x => x.SelectedShippingDate.Value.AddDays(-(int)x.SelectedShippingDate.Value.DayOfWeek))
                .ToList();

            var allOrders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate <= today).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int weekCount = 1;

                    worksheet.Cells[row, 1].Value = "Semana";
                    worksheet.Cells[row, 2].Value = "Fecha inicio";
                    worksheet.Cells[row, 3].Value = "Fecha final";
                    worksheet.Cells[row, 4].Value = "Número de usuarios que hicieron su primera compra esa semana";

                    foreach (var group in groupedByWeek)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = weekCount;
                        worksheet.Cells[row, 2].Value = group.Key;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 3].Value = group.Key.AddDays(6);
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "dd-mm-yyyy";

                        var firstOrderCount = 0;
                        foreach (var item in group)
                        {
                            if (!allOrders.Where(x => x.CreatedOnUtc < item.CreatedOnUtc && x.CustomerId == item.CustomerId).Any())
                                firstOrderCount++;
                        }

                        worksheet.Cells[row, 4].Value = firstOrderCount;
                        weekCount++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_primera_compra_semanal_{days}_dias.xlsx");
            }
        }

        //Número de cuentas que hicieron esos pedidos de la semana
        [HttpGet]
        public IActionResult GenerateExcel64(int days = 30)
        {
            var today = DateTime.Now.Date;
            var controlDate = today.AddDays(-1 * days);
            while (controlDate.DayOfWeek > 0)
            {
                controlDate = controlDate.AddDays(-1);
            }

            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today);
            var groupedByWeek = orders
                .OrderBy(x => x.SelectedShippingDate)
                .ToList()
                .GroupBy(x => x.SelectedShippingDate.Value.AddDays(-(int)x.SelectedShippingDate.Value.DayOfWeek))
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int weekCount = 1;

                    worksheet.Cells[row, 1].Value = "Semana";
                    worksheet.Cells[row, 2].Value = "Fecha inicio";
                    worksheet.Cells[row, 3].Value = "Fecha final";
                    worksheet.Cells[row, 4].Value = "Número de cuentas diferentes que hicieron pedidos";

                    foreach (var group in groupedByWeek)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = weekCount;
                        worksheet.Cells[row, 2].Value = group.Key;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 3].Value = group.Key.AddDays(6);
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 4].Value = group.GroupBy(x => x.CustomerId).Count();
                        weekCount++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_cantidad_usuarios_compra_semanal_{days}_dias.xlsx");
            }
        }

        // Ventas por fabricante
        [HttpGet]
        public IActionResult GenerateExcel65(int days = 30)
        {
            var today = DateTime.Now.Date;
            var controlDate = today.AddDays(-1 * days);

            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate < today);
            var pedidos = OrderUtils.GetPedidosGroup(ordersQuery).ToList();
            var routes = _shippingRouteService.GetAll().ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Órdenes del pedido";
                    worksheet.Cells[row, 2].Value = "Número de productos";
                    worksheet.Cells[row, 3].Value = "Fecha de entrega";
                    worksheet.Cells[row, 4].Value = "Horario de entrega";
                    worksheet.Cells[row, 5].Value = "Ruta";
                    worksheet.Cells[row, 6].Value = "Hora en que se marcó como entregada";

                    foreach (var group in pedidos)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = string.Join(", ", group.Select(x => "#" + x.Id.ToString()).ToList());
                        worksheet.Cells[row, 2].Value = group.SelectMany(x => x.OrderItems).GroupBy(x => x.ProductId).Count();
                        worksheet.Cells[row, 3].Value = group.FirstOrDefault().SelectedShippingDate;
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 4].Value = group.FirstOrDefault().SelectedShippingTime;
                        worksheet.Cells[row, 5].Value = routes.Where(x => x.Id == group.FirstOrDefault().RouteId).FirstOrDefault()?.RouteName;
                        var tst = group.SelectMany(x => x.Shipments)
                            .Where(x => x.DeliveryDateUtc.HasValue)
                            .Select(x => x.DeliveryDateUtc)
                            .OrderBy(x => x)
                            .FirstOrDefault();
                        if (tst.HasValue)
                        {
                            worksheet.Cells[row, 6].Value = tst.Value.ToLocalTime();
                            worksheet.Cells[row, 6].Style.Numberformat.Format = "hh:mm:ss";
                        }
                        else
                            worksheet.Cells[row, 6].Value = "No se registró hora de entrega";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_entrega_por_pedido_{days}_dias.xlsx");
            }
        }

        //Número de cuentas que se registraron de la semana
        [HttpGet]
        public IActionResult GenerateExcel66(int days = 30)
        {
            var today = DateTime.Now.Date;
            var controlDate = today.AddDays(-1 * days);
            while (controlDate.DayOfWeek > 0)
            {
                controlDate = controlDate.AddDays(-1);
            }

            var groupedByWeek = _customerService.GetAllCustomers()
                .Where(x => x.CreatedOnUtc >= controlDate && x.CreatedOnUtc <= today && x.Email != "" && x.Email != null)
                .OrderBy(x => x.CreatedOnUtc)
                .GroupBy(x => x.CreatedOnUtc.Date.AddDays(-(int)x.CreatedOnUtc.DayOfWeek))
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int weekCount = 1;

                    worksheet.Cells[row, 1].Value = "Semana";
                    worksheet.Cells[row, 2].Value = "Fecha inicio";
                    worksheet.Cells[row, 3].Value = "Fecha final";
                    worksheet.Cells[row, 4].Value = "Número de cuentas registradas esa semana";

                    foreach (var group in groupedByWeek)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = weekCount;
                        worksheet.Cells[row, 2].Value = group.Key;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 3].Value = group.Key.AddDays(6);
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 4].Value = group.Count();
                        weekCount++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_cantidad_usuarios_registrados_semanal_{days}_dias.xlsx");
            }
        }

        // Reporte mkt semanal
        [HttpGet]
        public IActionResult GenerateExcel67(int days = 30)
        {
            var today = DateTime.Now.Date;
            var controlDate = today.AddDays(-1 * days);
            while (controlDate.DayOfWeek > 0)
            {
                controlDate = controlDate.AddDays(-1);
            }

            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today);
            var groupedByWeek = orders
                .OrderBy(x => x.SelectedShippingDate)
                .ToList()
                .GroupBy(x => x.SelectedShippingDate.Value.AddDays(-(int)x.SelectedShippingDate.Value.DayOfWeek).AddDays(1))
                .ToList();
            var customers = _customerService.GetAllCustomers()
                .Where(x => x.CreatedOnUtc >= controlDate && x.CreatedOnUtc <= today && x.Email != "" && x.Email != null)
                .ToList();
            var allOrders = OrderUtils.GetFilteredOrders(_orderService).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int col = 1;

                    worksheet.Cells[row, col].Value = "Variables para indicadores semanales de MKT";
                    worksheet.Cells[row + 1, col].Value = "ID";
                    worksheet.Cells[row + 1, col + 1].Value = "Variable";

                    col = 3;
                    row = 1;
                    foreach (var week in groupedByWeek)
                    {
                        worksheet.Cells[row, col].Value = week.Key;
                        worksheet.Cells[row, col].Style.Numberformat.Format = "dd-mm-yyyy";
                        row++;
                        worksheet.Cells[row, col].Value = week.Key.AddDays(6);
                        worksheet.Cells[row, col].Style.Numberformat.Format = "dd-mm-yyyy";
                        col++;
                        row = 1;
                    }

                    for (int i = 1; i < 14; i++)
                    {
                        worksheet.Cells[i + 2, 1].Value = "MKV00" + i.ToString("00");
                    }

                    worksheet.Cells[3, 2].Value = "Gasto publicitario del periodo";
                    worksheet.Cells[4, 2].Value = "Nuevos registros en el periodo";
                    worksheet.Cells[5, 2].Value = "Nuevas cuentas activas (al menos han hecho un pedido en la vida) del periodo";
                    worksheet.Cells[6, 2].Value = "Total de pedidos del periodo";
                    worksheet.Cells[7, 2].Value = "Total de ventas del periodo";
                    worksheet.Cells[8, 2].Value = "Número de días trabajados en el periodo";
                    worksheet.Cells[9, 2].Value = "Total de pedidos de los últimos 30 días a partir de la fecha final del periodo";
                    worksheet.Cells[10, 2].Value = "Número de clientes que hayan hecho al menos un pedido en los últimos 60 días a partir de la fecha final del periodo";
                    worksheet.Cells[11, 2].Value = "Número de clientes que hayan hecho al menos un pedido en los últimos 30 días a partir de la fecha final del periodo";
                    worksheet.Cells[12, 2].Value = "Número de clientes que hayan hecho su primer pedido en los últimos 30 días a partir de la fecha final del periodo";
                    worksheet.Cells[13, 2].Value = "Número de clientes que hayan hecho al menos un pedido en los últimos 120 días a partir de la fecha final del periodo";
                    worksheet.Cells[14, 2].Value = "Número de clientes que hayan hecho al menos un pedido en los últimos 90 días a partir de la fecha final del periodo";
                    worksheet.Cells[15, 2].Value = "Número de clientes que hayan hecho su primer pedido en los últimos 90 días a partir de la fecha final del periodo";

                    col = 3;

                    foreach (var group in groupedByWeek)
                    {
                        var initDate = group.Key;
                        var endDate = group.Key.AddDays(6);

                        //worksheet.Cells[3, col].Value = "";
                        var registeredUsers = customers.Where(x => x.CreatedOnUtc >= initDate && x.CreatedOnUtc <= endDate).ToList();
                        //worksheet.Cells[4, col].Value = registeredUsers.Count();

                        int activeCount = 0;
                        foreach (var customer in registeredUsers)
                        {
                            if (group.Where(x => x.CustomerId == customer.Id).Any())
                                activeCount++;
                        }
                        worksheet.Cells[5, col].Value = activeCount;
                        //worksheet.Cells[6, col].Value = OrderUtils.GetPedidosGroupByList(group.Select(x => x).ToList()).Count();
                        //worksheet.Cells[7, col].Value = group.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();

                        //int workingDays = 0;
                        //for (DateTime i = initDate; i <= endDate; i = i.AddDays(1))
                        //{
                        //    if (group.Where(x => x.SelectedShippingDate == i).Any())
                        //        workingDays++;
                        //}
                        //worksheet.Cells[8, col].Value = workingDays;

                        //var controlDate30DaysFromEnd = endDate.AddDays(-30);
                        //var controlDate60DaysFromEnd = endDate.AddDays(-60);
                        //var controlDate90DaysFromEnd = endDate.AddDays(-90);
                        //var controlDate120DaysFromEnd = endDate.AddDays(-120);

                        //worksheet.Cells[9, col].Value = OrderUtils.GetPedidosGroupByList(allOrders
                        //    .Where(x => x.SelectedShippingDate >= controlDate30DaysFromEnd && x.SelectedShippingDate <= endDate)
                        //    .ToList())
                        //    .Count();

                        //worksheet.Cells[10, col].Value = allOrders
                        //    .Where(x => x.SelectedShippingDate >= controlDate60DaysFromEnd && x.SelectedShippingDate <= endDate)
                        //    .GroupBy(x => x.CustomerId)
                        //    .Count();

                        //worksheet.Cells[11, col].Value = allOrders
                        //    .Where(x => x.SelectedShippingDate >= controlDate30DaysFromEnd && x.SelectedShippingDate <= endDate)
                        //    .GroupBy(x => x.CustomerId)
                        //    .Count();

                        //var customerIdsPeriodOrders = allOrders
                        //    .Where(x => x.SelectedShippingDate >= controlDate30DaysFromEnd && x.SelectedShippingDate <= endDate)
                        //    .GroupBy(x => x.CustomerId)
                        //    .Select(x => x.Key)
                        //    .ToList();
                        //var filteredOrders = allOrders.Where(x => x.SelectedShippingDate < controlDate30DaysFromEnd).ToList();
                        //int firstOrderCount = 0;
                        //foreach (var customerId in customerIdsPeriodOrders)
                        //{
                        //    if (!filteredOrders.Where(x => x.CustomerId == customerId).Any())
                        //        firstOrderCount++;
                        //}

                        //worksheet.Cells[12, col].Value = firstOrderCount;

                        //worksheet.Cells[13, col].Value = allOrders
                        //    .Where(x => x.SelectedShippingDate >= controlDate120DaysFromEnd && x.SelectedShippingDate <= endDate)
                        //    .GroupBy(x => x.CustomerId)
                        //    .Count();

                        //worksheet.Cells[14, col].Value = allOrders
                        //    .Where(x => x.SelectedShippingDate >= controlDate90DaysFromEnd && x.SelectedShippingDate <= endDate)
                        //    .GroupBy(x => x.CustomerId)
                        //    .Count();

                        //filteredOrders = allOrders.Where(x => x.SelectedShippingDate < controlDate90DaysFromEnd).ToList();
                        //customerIdsPeriodOrders = allOrders
                        //    .Where(x => x.SelectedShippingDate >= controlDate90DaysFromEnd && x.SelectedShippingDate <= endDate)
                        //    .GroupBy(x => x.CustomerId)
                        //    .Select(x => x.Key)
                        //    .ToList();
                        //firstOrderCount = 0;
                        //foreach (var customerId in customerIdsPeriodOrders)
                        //{
                        //    if (!filteredOrders.Where(x => x.CustomerId == customerId).Any())
                        //        firstOrderCount++;
                        //}
                        //worksheet.Cells[15, col].Value = firstOrderCount;

                        col++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_reporte_mkt_semanal_{days}_dias.xlsx");
            }
        }

        [HttpGet]
        public IActionResult GenerateExcel68()
        {
            var customerGroup = OrderUtils.GetFilteredOrders(_orderService).GroupBy(x => x.Customer).ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Datos";

                    foreach (var item in customerGroup)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = item.Key.Email;

                        var current = item.OrderBy(x => x.SelectedShippingDate)
                            .GroupBy(x => x.SelectedShippingDate.Value)
                            .ToList();
                        foreach (var order in current)
                        {
                            row++;
                            worksheet.Cells[row, 1].Value = order.Key;
                            worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mm-yyyy";
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_compras_por_usuario.xlsx");
            }
        }

        // Ticket promedio por zona por semana
        [HttpGet]
        public IActionResult GenerateExcel69(int days = 30)
        {
            var today = DateTime.Now.Date;
            var controlDate = today.AddDays(-1 * days);
            while (controlDate.DayOfWeek > 0)
            {
                controlDate = controlDate.AddDays(-1);
            }

            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today);
            var groupedByWeek = orders
                .OrderBy(x => x.SelectedShippingDate)
                .ToList()
                .GroupBy(x => x.SelectedShippingDate.Value.AddDays(-(int)x.SelectedShippingDate.Value.DayOfWeek).AddDays(1))
                .ToList();
            var zones = _shippingZoneService.GetAll().ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int col = 1;

                    worksheet.Cells[2, 1].Value = "Zona";

                    col++;
                    foreach (var week in groupedByWeek)
                    {
                        worksheet.Cells[1, col].Value = week.Key;
                        worksheet.Cells[1, col].Style.Numberformat.Format = "dd-mm-yyyy";
                        row++;
                        worksheet.Cells[2, col].Value = week.Key.AddDays(6);
                        worksheet.Cells[2, col].Style.Numberformat.Format = "dd-mm-yyyy";
                        col++;
                        row = 1;
                    }

                    row = 3;

                    foreach (var zone in zones)
                    {
                        col = 1;
                        worksheet.Cells[row, col].Value = zone.ZoneName;
                        var postalCodeString = zone.PostalCodes + "," + zone.AdditionalPostalCodes;
                        var postalCodes = postalCodeString.Split(',').Select(x => x.Trim()).ToList();

                        foreach (var group in groupedByWeek)
                        {
                            col++;
                            var filteredOrders = group.Where(x => postalCodes.Contains(x.ShippingAddress.ZipPostalCode)).ToList();
                            worksheet.Cells[row, col].Value = OrderUtils.GetPedidosGroupByList(filteredOrders).Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();
                        }

                        row++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ticket_promedio_zona_semanal_{days}_dias.xlsx");
            }
        }

        // Ventas por categoria
        [HttpGet]
        public IActionResult GenerateExcel70(string initDate, string endDate, int catId)
        {
            if (string.IsNullOrEmpty(initDate) || string.IsNullOrEmpty(endDate) || catId == 0) return NotFound();

            var initDateParsed = DateTime.ParseExact(initDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var endDateParsed = DateTime.ParseExact(endDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var category = _categoryService.GetCategoryById(catId);

            var productsGroup = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= initDateParsed && x.SelectedShippingDate <= endDateParsed)
                .SelectMany(x => x.OrderItems)
                .Where(x => x.Product.ProductCategories.Where(y => !y.Category.Deleted).Select(y => y.CategoryId).Contains(catId))
                .GroupBy(x => x.Product)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Producto";
                    worksheet.Cells[row, 2].Value = "Monto vendido";
                    worksheet.Cells[row, 3].Value = "Cantidad vendida";
                    worksheet.Cells[row, 4].Value = "Unidad";

                    foreach (var group in productsGroup)
                    {
                        row++;
                        var qty = GetQty(group.Key, group.Select(x => x.Quantity).Sum());
                        worksheet.Cells[row, 1].Value = group.Key.Name;
                        worksheet.Cells[row, 2].Value = group.Select(x => x.PriceInclTax).Sum();
                        worksheet.Cells[row, 3].Value = qty.Item1;
                        worksheet.Cells[row, 4].Value = qty.Item2;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ventas_categoria_{category.Name}_{initDate}_{endDate}.xlsx");
            }
        }

        //Lista de ordenes por correo
        [HttpGet]
        public IActionResult GenerateExcel71(string initDate, string endDate)
        {
            if (string.IsNullOrEmpty(initDate) || string.IsNullOrEmpty(endDate)) return NotFound();

            var initDateParsed = DateTime.ParseExact(initDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var endDateParsed = DateTime.ParseExact(endDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var ordersGroup = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.OrderStatusId != 50)
                .Where(x => x.SelectedShippingDate >= initDateParsed && x.SelectedShippingDate <= endDateParsed)
                .GroupBy(x => x.Customer)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Correo electrónico";
                    worksheet.Cells[row, 2].Value = "Nombre";
                    worksheet.Cells[row, 3].Value = "Total comprado";
                    worksheet.Cells[row, 4].Value = "Números de orden";

                    foreach (var group in ordersGroup)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = group.Key.Email;
                        worksheet.Cells[row, 2].Value = group.Key.GetFullName();
                        worksheet.Cells[row, 3].Value = group.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 4].Value = string.Join(", ", group.Select(x => "#" + x.CustomOrderNumber));
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ventas_{initDate}_{endDate}.xlsx");
            }
        }

        // Productos vendidos de frabricante un dia
        [HttpGet]
        public IActionResult GenerateExcel72(string date, int fbId)
        {
            if (fbId < 1)
                return BadRequest();

            var manufacturer = _manufacturerService.GetManufacturerById(fbId);
            if (manufacturer == null) return NotFound();

            var parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var groupedItems = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate == parsedDate)
                .SelectMany(x => x.OrderItems)
                .Where(x => x.Product.ProductManufacturers.Where(y => y.ManufacturerId == fbId && !y.Manufacturer.Deleted).Any())
                .GroupBy(x => x.Product)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Producto";
                    worksheet.Cells[row, 2].Value = "Monto vendido";
                    worksheet.Cells[row, 3].Value = "Cantidad vendida";
                    worksheet.Cells[row, 4].Value = "Unidad";

                    foreach (var product in groupedItems)
                    {
                        row++;
                        var qty = GetQty(product.Key, product.Select(x => x.Quantity).DefaultIfEmpty().Sum());
                        worksheet.Cells[row, 1].Value = product.Key.Name;
                        worksheet.Cells[row, 2].Value = product.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 3].Value = qty.Item1;
                        worksheet.Cells[row, 4].Value = qty.Item2;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ventas_{manufacturer.Name}_{date}.xlsx");
            }
        }

        // Cantidad de ventas mensuales por cliente (desde el inicio)
        [HttpGet]
        public IActionResult GenerateExcel73()
        {
            var allOrders = OrderUtils.GetFilteredOrders(_orderService).ToList();
            var customerIds = allOrders.GroupBy(x => x.CustomerId).Select(x => x.Key).ToList();
            var dates = allOrders
                .GroupBy(x => new DateTime(x.SelectedShippingDate.Value.Year, x.SelectedShippingDate.Value.Month, 1))
                .OrderByDescending(x => x.Key)
                .Select(x => x.Key)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int col = 1;
                    worksheet.Cells[row, col].Value = "Cliente (Id)";

                    foreach (var date in dates)
                    {
                        col++;
                        worksheet.Cells[row, col].Value = date.ToString("MM-yyyy");
                    }

                    row = 1;
                    col = 1;

                    foreach (var customerId in customerIds)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = customerId;
                        var customerOrders = allOrders.Where(x => x.CustomerId == customerId);
                        foreach (var date in dates)
                        {
                            col++;
                            var filteredOrders = customerOrders.Where(x => x.SelectedShippingDate.Value.Month == date.Month && x.SelectedShippingDate.Value.Year == date.Year).ToList();
                            worksheet.Cells[row, col].Value = OrderUtils.GetPedidosGroupByList(filteredOrders).Count();
                        }
                        col = 1;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_pedidos_mensuales_por_cliente.xlsx");
            }
        }

        // Cantidad de pedidos mensuales por cliente (desde el inicio)
        [HttpGet]
        public IActionResult GenerateExcel74()
        {
            var allOrders = OrderUtils.GetFilteredOrders(_orderService).ToList();
            var customerIds = allOrders.GroupBy(x => x.CustomerId).Select(x => x.Key).ToList();
            var dates = allOrders
                .GroupBy(x => new DateTime(x.SelectedShippingDate.Value.Year, x.SelectedShippingDate.Value.Month, 1))
                .OrderByDescending(x => x.Key)
                .Select(x => x.Key)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int col = 1;
                    worksheet.Cells[row, col].Value = "Cliente (Id)";

                    foreach (var date in dates)
                    {
                        col++;
                        worksheet.Cells[row, col].Value = date.ToString("MM-yyyy");
                    }

                    row = 1;
                    col = 1;

                    foreach (var customerId in customerIds)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = customerId;
                        var customerOrders = allOrders.Where(x => x.CustomerId == customerId);
                        foreach (var date in dates)
                        {
                            col++;
                            var filteredOrders = customerOrders.Where(x => x.SelectedShippingDate.Value.Month == date.Month && x.SelectedShippingDate.Value.Year == date.Year);
                            worksheet.Cells[row, col].Value = filteredOrders.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                        }
                        col = 1;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ventas_mensuales_por_cliente.xlsx");
            }
        }

        // Ticket promedio mensual
        [HttpGet]
        public IActionResult GenerateExcel75()
        {
            var orders = OrderUtils.GetFilteredOrders(_orderService).ToList();
            var controlDateUtc = DateTime.UtcNow;

            var groupedByMonth = orders
                .OrderBy(x => x.SelectedShippingDate)
                .ToList()
                .GroupBy(x => new DateTime(x.SelectedShippingDate.Value.Year, x.SelectedShippingDate.Value.Month, 1))
                .OrderByDescending(x => x.Key)
                .ToList();

            var data = new List<MarketingDashboardData>();

            foreach (var group in groupedByMonth)
            {
                if (group.Key.Month > DateTime.Now.Month && group.Key.Year == DateTime.Now.Year) continue;
                var initDate = group.Key;
                var endDate = group.Key.AddMonths(1).AddDays(-1);

                var firstOrders = new List<Order>();
                var notFirstOrders = new List<Order>();
                PrepareFirstAndLastOrders(firstOrders, notFirstOrders, orders, initDate, group.GroupBy(x => x.CustomerId).ToList());

                List<int> firstOrderClientIds = firstOrders.Select(x => x.CustomerId).ToList();

                //Clientes que hicieron su primera compra en el periodo
                int firstPedidosCount = OrderUtils.GetPedidosGroupByList(firstOrders).Count();

                int client30DaysAfterCount = 0;
                int client60DaysAfterCount = 0;
                int client90DaysAfterCount = 0;
                int client120DaysAfterCount = 0;
                int client150DaysAfterCount = 0;
                int client180DaysAfterCount = 0;
                int client210DaysAfterCount = 0;
                int client240DaysAfterCount = 0;
                int client270DaysAfterCount = 0;
                int client300DaysAfterCount = 0;
                int client330DaysAfterCount = 0;
                int client360DaysAfterCount = 0;
                int client390DaysAfterCount = 0;
                int client420DaysAfterCount = 0;
                int client450DaysAfterCount = 0;
                int client480DaysAfterCount = 0;
                int client510DaysAfterCount = 0;
                int client540DaysAfterCount = 0;
                int client570DaysAfterCount = 0;
                int client600DaysAfterCount = 0;
                int client630DaysAfterCount = 0;
                int client660DaysAfterCount = 0;
                int client690DaysAfterCount = 0;
                int client720DaysAfterCount = 0;
                decimal client30DaysAfterPercentage = 0;
                decimal client60DaysAfterPercentage = 0;
                decimal client90DaysAfterPercentage = 0;
                decimal client120DaysAfterPercentage = 0;
                decimal client150DaysAfterPercentage = 0;
                decimal client180DaysAfterPercentage = 0;
                decimal client210DaysAfterPercentage = 0;
                decimal client240DaysAfterPercentage = 0;
                decimal client270DaysAfterPercentage = 0;
                decimal client300DaysAfterPercentage = 0;
                decimal client330DaysAfterPercentage = 0;
                decimal client360DaysAfterPercentage = 0;
                decimal client390DaysAfterPercentage = 0;
                decimal client420DaysAfterPercentage = 0;
                decimal client450DaysAfterPercentage = 0;
                decimal client480DaysAfterPercentage = 0;
                decimal client510DaysAfterPercentage = 0;
                decimal client540DaysAfterPercentage = 0;
                decimal client570DaysAfterPercentage = 0;
                decimal client600DaysAfterPercentage = 0;
                decimal client630DaysAfterPercentage = 0;
                decimal client660DaysAfterPercentage = 0;
                decimal client690DaysAfterPercentage = 0;
                decimal client720DaysAfterPercentage = 0;
                decimal client30DaysAfterTicket = 0;
                decimal client60DaysAfterTicket = 0;
                decimal client90DaysAfterTicket = 0;
                decimal client120DaysAfterTicket = 0;
                decimal client150DaysAfterTicket = 0;
                decimal client180DaysAfterTicket = 0;
                decimal client210DaysAfterTicket = 0;
                decimal client240DaysAfterTicket = 0;
                decimal client270DaysAfterTicket = 0;
                decimal client300DaysAfterTicket = 0;
                decimal client330DaysAfterTicket = 0;
                decimal client360DaysAfterTicket = 0;
                decimal client390DaysAfterTicket = 0;
                decimal client420DaysAfterTicket = 0;
                decimal client450DaysAfterTicket = 0;
                decimal client480DaysAfterTicket = 0;
                decimal client510DaysAfterTicket = 0;
                decimal client540DaysAfterTicket = 0;
                decimal client570DaysAfterTicket = 0;
                decimal client600DaysAfterTicket = 0;
                decimal client630DaysAfterTicket = 0;
                decimal client660DaysAfterTicket = 0;
                decimal client690DaysAfterTicket = 0;
                decimal client720DaysAfterTicket = 0;
                decimal client30DaysAfterRecurrence = 0;
                decimal client60DaysAfterRecurrence = 0;
                decimal client90DaysAfterRecurrence = 0;
                decimal client120DaysAfterRecurrence = 0;
                decimal client150DaysAfterRecurrence = 0;
                decimal client180DaysAfterRecurrence = 0;
                decimal client210DaysAfterRecurrence = 0;
                decimal client240DaysAfterRecurrence = 0;
                decimal client270DaysAfterRecurrence = 0;
                decimal client300DaysAfterRecurrence = 0;
                decimal client330DaysAfterRecurrence = 0;
                decimal client360DaysAfterRecurrence = 0;
                decimal client390DaysAfterRecurrence = 0;
                decimal client420DaysAfterRecurrence = 0;
                decimal client450DaysAfterRecurrence = 0;
                decimal client480DaysAfterRecurrence = 0;
                decimal client510DaysAfterRecurrence = 0;
                decimal client540DaysAfterRecurrence = 0;
                decimal client570DaysAfterRecurrence = 0;
                decimal client600DaysAfterRecurrence = 0;
                decimal client630DaysAfterRecurrence = 0;
                decimal client660DaysAfterRecurrence = 0;
                decimal client690DaysAfterRecurrence = 0;
                decimal client720DaysAfterRecurrence = 0;

                PrepareClientDaysData(ref client30DaysAfterCount,
                    ref client60DaysAfterCount,
                    ref client90DaysAfterCount,
                    ref client120DaysAfterCount,
                    ref client150DaysAfterCount,
                    ref client180DaysAfterCount,
                    ref client210DaysAfterCount,
                    ref client240DaysAfterCount,
                    ref client270DaysAfterCount,
                    ref client300DaysAfterCount,
                    ref client330DaysAfterCount,
                    ref client360DaysAfterCount,
                    ref client390DaysAfterCount,
                    ref client420DaysAfterCount,
                    ref client450DaysAfterCount,
                    ref client480DaysAfterCount,
                    ref client510DaysAfterCount,
                    ref client540DaysAfterCount,
                    ref client570DaysAfterCount,
                    ref client600DaysAfterCount,
                    ref client630DaysAfterCount,
                    ref client660DaysAfterCount,
                    ref client690DaysAfterCount,
                    ref client720DaysAfterCount,
                    ref client30DaysAfterPercentage,
                    ref client60DaysAfterPercentage,
                    ref client90DaysAfterPercentage,
                    ref client120DaysAfterPercentage,
                    ref client150DaysAfterPercentage,
                    ref client180DaysAfterPercentage,
                    ref client210DaysAfterPercentage,
                    ref client240DaysAfterPercentage,
                    ref client270DaysAfterPercentage,
                    ref client300DaysAfterPercentage,
                    ref client330DaysAfterPercentage,
                    ref client360DaysAfterPercentage,
                    ref client390DaysAfterPercentage,
                    ref client420DaysAfterPercentage,
                    ref client450DaysAfterPercentage,
                    ref client480DaysAfterPercentage,
                    ref client510DaysAfterPercentage,
                    ref client540DaysAfterPercentage,
                    ref client570DaysAfterPercentage,
                    ref client600DaysAfterPercentage,
                    ref client630DaysAfterPercentage,
                    ref client660DaysAfterPercentage,
                    ref client690DaysAfterPercentage,
                    ref client720DaysAfterPercentage,
                    ref client30DaysAfterTicket,
                    ref client60DaysAfterTicket,
                    ref client90DaysAfterTicket,
                    ref client120DaysAfterTicket,
                    ref client150DaysAfterTicket,
                    ref client180DaysAfterTicket,
                    ref client210DaysAfterTicket,
                    ref client240DaysAfterTicket,
                    ref client270DaysAfterTicket,
                    ref client300DaysAfterTicket,
                    ref client330DaysAfterTicket,
                    ref client360DaysAfterTicket,
                    ref client390DaysAfterTicket,
                    ref client420DaysAfterTicket,
                    ref client450DaysAfterTicket,
                    ref client480DaysAfterTicket,
                    ref client510DaysAfterTicket,
                    ref client540DaysAfterTicket,
                    ref client570DaysAfterTicket,
                    ref client600DaysAfterTicket,
                    ref client630DaysAfterTicket,
                    ref client660DaysAfterTicket,
                    ref client690DaysAfterTicket,
                    ref client720DaysAfterTicket,
                    ref client30DaysAfterRecurrence,
                    ref client60DaysAfterRecurrence,
                    ref client90DaysAfterRecurrence,
                    ref client120DaysAfterRecurrence,
                    ref client150DaysAfterRecurrence,
                    ref client180DaysAfterRecurrence,
                    ref client210DaysAfterRecurrence,
                    ref client240DaysAfterRecurrence,
                    ref client270DaysAfterRecurrence,
                    ref client300DaysAfterRecurrence,
                    ref client330DaysAfterRecurrence,
                    ref client360DaysAfterRecurrence,
                    ref client390DaysAfterRecurrence,
                    ref client420DaysAfterRecurrence,
                    ref client450DaysAfterRecurrence,
                    ref client480DaysAfterRecurrence,
                    ref client510DaysAfterRecurrence,
                    ref client540DaysAfterRecurrence,
                    ref client570DaysAfterRecurrence,
                    ref client600DaysAfterRecurrence,
                    ref client630DaysAfterRecurrence,
                    ref client660DaysAfterRecurrence,
                    ref client690DaysAfterRecurrence,
                    ref client720DaysAfterRecurrence,
                    orders,
                    firstOrderClientIds,
                    firstPedidosCount,
                    endDate);

                data.Add(new MarketingDashboardData()
                {
                    InitDate = initDate,
                    EndDate = endDate,
                    FirstPedidosCount = firstPedidosCount,
                    Client120DaysAfterCount = client120DaysAfterCount,
                    Client120DaysAfterPercentage = client120DaysAfterPercentage,
                    Client150DaysAfterCount = client150DaysAfterCount,
                    Client150DaysAfterPercentage = client150DaysAfterPercentage,
                    Client180DaysAfterCount = client180DaysAfterCount,
                    Client180DaysAfterPercentage = client180DaysAfterPercentage,
                    Client210DaysAfterCount = client210DaysAfterCount,
                    Client210DaysAfterPercentage = client210DaysAfterPercentage,
                    Client240DaysAfterCount = client240DaysAfterCount,
                    Client240DaysAfterPercentage = client240DaysAfterPercentage,
                    Client270DaysAfterCount = client270DaysAfterCount,
                    Client270DaysAfterPercentage = client270DaysAfterPercentage,
                    Client300DaysAfterCount = client300DaysAfterCount,
                    Client300DaysAfterPercentage = client300DaysAfterPercentage,
                    Client30DaysAfterCount = client30DaysAfterCount,
                    Client30DaysAfterPercentage = client30DaysAfterPercentage,
                    Client330DaysAfterCount = client330DaysAfterCount,
                    Client330DaysAfterPercentage = client330DaysAfterPercentage,
                    Client360DaysAfterCount = client360DaysAfterCount,
                    Client360DaysAfterPercentage = client360DaysAfterPercentage,
                    Client60DaysAfterCount = client60DaysAfterCount,
                    Client60DaysAfterPercentage = client60DaysAfterPercentage,
                    Client90DaysAfterCount = client90DaysAfterCount,
                    Client90DaysAfterPercentage = client90DaysAfterPercentage,
                    Client390DaysAfterCount = client390DaysAfterCount,
                    Client390DaysAfterPercentage = client390DaysAfterPercentage,
                    Client420DaysAfterCount = client420DaysAfterCount,
                    Client420DaysAfterPercentage = client420DaysAfterPercentage,
                    Client450DaysAfterCount = client450DaysAfterCount,
                    Client450DaysAfterPercentage = client450DaysAfterPercentage,
                    Client480DaysAfterCount = client480DaysAfterCount,
                    Client480DaysAfterPercentage = client480DaysAfterPercentage,
                    Client510DaysAfterCount = client510DaysAfterCount,
                    Client510DaysAfterPercentage = client510DaysAfterPercentage,
                    Client540DaysAfterCount = client540DaysAfterCount,
                    Client540DaysAfterPercentage = client540DaysAfterPercentage,
                    Client570DaysAfterCount = client570DaysAfterCount,
                    Client570DaysAfterPercentage = client570DaysAfterPercentage,
                    Client600DaysAfterCount = client600DaysAfterCount,
                    Client600DaysAfterPercentage = client600DaysAfterPercentage,
                    Client630DaysAfterCount = client630DaysAfterCount,
                    Client630DaysAfterPercentage = client630DaysAfterPercentage,
                    Client660DaysAfterCount = client660DaysAfterCount,
                    Client660DaysAfterPercentage = client660DaysAfterPercentage,
                    Client690DaysAfterCount = client690DaysAfterCount,
                    Client690DaysAfterPercentage = client690DaysAfterPercentage,
                    Client720DaysAfterCount = client720DaysAfterCount,
                    Client720DaysAfterPercentage = client720DaysAfterPercentage,
                    Client120DaysAfterRecurrence = client120DaysAfterRecurrence,
                    Client120DaysAfterTicket = client120DaysAfterTicket,
                    Client150DaysAfterRecurrence = client150DaysAfterRecurrence,
                    Client150DaysAfterTicket = client150DaysAfterTicket,
                    Client180DaysAfterRecurrence = client180DaysAfterRecurrence,
                    Client180DaysAfterTicket = client180DaysAfterTicket,
                    Client210DaysAfterRecurrence = client210DaysAfterRecurrence,
                    Client210DaysAfterTicket = client210DaysAfterTicket,
                    Client240DaysAfterRecurrence = client240DaysAfterRecurrence,
                    Client240DaysAfterTicket = client240DaysAfterTicket,
                    Client270DaysAfterRecurrence = client270DaysAfterRecurrence,
                    Client270DaysAfterTicket = client270DaysAfterTicket,
                    Client300DaysAfterRecurrence = client300DaysAfterRecurrence,
                    Client300DaysAfterTicket = client300DaysAfterTicket,
                    Client30DaysAfterRecurrence = client30DaysAfterRecurrence,
                    Client30DaysAfterTicket = client30DaysAfterTicket,
                    Client330DaysAfterRecurrence = client330DaysAfterRecurrence,
                    Client330DaysAfterTicket = client330DaysAfterTicket,
                    Client360DaysAfterRecurrence = client360DaysAfterRecurrence,
                    Client360DaysAfterTicket = client360DaysAfterTicket,
                    Client390DaysAfterRecurrence = client390DaysAfterRecurrence,
                    Client390DaysAfterTicket = client390DaysAfterTicket,
                    Client420DaysAfterRecurrence = client420DaysAfterRecurrence,
                    Client420DaysAfterTicket = client420DaysAfterTicket,
                    Client450DaysAfterRecurrence = client450DaysAfterRecurrence,
                    Client450DaysAfterTicket = client450DaysAfterTicket,
                    Client480DaysAfterRecurrence = client480DaysAfterRecurrence,
                    Client480DaysAfterTicket = client480DaysAfterTicket,
                    Client510DaysAfterRecurrence = client510DaysAfterRecurrence,
                    Client510DaysAfterTicket = client510DaysAfterTicket,
                    Client540DaysAfterRecurrence = client540DaysAfterRecurrence,
                    Client540DaysAfterTicket = client540DaysAfterTicket,
                    Client570DaysAfterRecurrence = client570DaysAfterRecurrence,
                    Client570DaysAfterTicket = client570DaysAfterTicket,
                    Client600DaysAfterRecurrence = client600DaysAfterRecurrence,
                    Client600DaysAfterTicket = client600DaysAfterTicket,
                    Client60DaysAfterRecurrence = client60DaysAfterRecurrence,
                    Client60DaysAfterTicket = client60DaysAfterTicket,
                    Client630DaysAfterRecurrence = client630DaysAfterRecurrence,
                    Client630DaysAfterTicket = client630DaysAfterTicket,
                    Client660DaysAfterRecurrence = client660DaysAfterRecurrence,
                    Client660DaysAfterTicket = client660DaysAfterTicket,
                    Client690DaysAfterRecurrence = client690DaysAfterRecurrence,
                    Client690DaysAfterTicket = client690DaysAfterTicket,
                    Client720DaysAfterRecurrence = client720DaysAfterRecurrence,
                    Client720DaysAfterTicket = client720DaysAfterTicket,
                    Client90DaysAfterRecurrence = client90DaysAfterRecurrence,
                    Client90DaysAfterTicket = client90DaysAfterTicket
                });
            }

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("Cosechas 30");

                    #region INDICADORES 4

                    var row = 1;
                    worksheet.Cells[row, 3].Value = "Clientes";
                    worksheet.Cells[row, 4].Value = "Mes 1";
                    worksheet.Cells[row, 5].Value = "Mes 2";
                    worksheet.Cells[row, 6].Value = "Mes 3";
                    worksheet.Cells[row, 7].Value = "Mes 4";
                    worksheet.Cells[row, 8].Value = "Mes 5";
                    worksheet.Cells[row, 9].Value = "Mes 6";
                    worksheet.Cells[row, 10].Value = "Mes 7";
                    worksheet.Cells[row, 11].Value = "Mes 8";
                    worksheet.Cells[row, 12].Value = "Mes 9";
                    worksheet.Cells[row, 13].Value = "Mes 10";
                    worksheet.Cells[row, 14].Value = "Mes 11";
                    worksheet.Cells[row, 15].Value = "Mes 12";
                    worksheet.Cells[row, 16].Value = "Mes 13";
                    worksheet.Cells[row, 17].Value = "Mes 14";
                    worksheet.Cells[row, 18].Value = "Mes 15";
                    worksheet.Cells[row, 19].Value = "Mes 16";
                    worksheet.Cells[row, 20].Value = "Mes 17";
                    worksheet.Cells[row, 21].Value = "Mes 18";
                    worksheet.Cells[row, 22].Value = "Mes 19";
                    worksheet.Cells[row, 23].Value = "Mes 20";
                    worksheet.Cells[row, 24].Value = "Mes 21";
                    worksheet.Cells[row, 25].Value = "Mes 22";
                    worksheet.Cells[row, 26].Value = "Mes 23";
                    worksheet.Cells[row, 27].Value = "Mes 24";

                    worksheet.Cells[row, 31].Value = "Clientes";
                    worksheet.Cells[row, 32].Value = "Mes 1";
                    worksheet.Cells[row, 33].Value = "Mes 2";
                    worksheet.Cells[row, 34].Value = "Mes 3";
                    worksheet.Cells[row, 35].Value = "Mes 4";
                    worksheet.Cells[row, 36].Value = "Mes 5";
                    worksheet.Cells[row, 37].Value = "Mes 6";
                    worksheet.Cells[row, 38].Value = "Mes 7";
                    worksheet.Cells[row, 39].Value = "Mes 8";
                    worksheet.Cells[row, 40].Value = "Mes 9";
                    worksheet.Cells[row, 41].Value = "Mes 10";
                    worksheet.Cells[row, 42].Value = "Mes 11";
                    worksheet.Cells[row, 43].Value = "Mes 12";
                    worksheet.Cells[row, 44].Value = "Mes 13";
                    worksheet.Cells[row, 45].Value = "Mes 14";
                    worksheet.Cells[row, 46].Value = "Mes 15";
                    worksheet.Cells[row, 47].Value = "Mes 16";
                    worksheet.Cells[row, 48].Value = "Mes 17";
                    worksheet.Cells[row, 49].Value = "Mes 18";
                    worksheet.Cells[row, 50].Value = "Mes 19";
                    worksheet.Cells[row, 51].Value = "Mes 20";
                    worksheet.Cells[row, 52].Value = "Mes 21";
                    worksheet.Cells[row, 53].Value = "Mes 22";
                    worksheet.Cells[row, 54].Value = "Mes 23";
                    worksheet.Cells[row, 55].Value = "Mes 24";

                    worksheet.Cells[row, 59].Value = "Clientes";
                    worksheet.Cells[row, 60].Value = "Mes 1";
                    worksheet.Cells[row, 61].Value = "Mes 2";
                    worksheet.Cells[row, 62].Value = "Mes 3";
                    worksheet.Cells[row, 63].Value = "Mes 4";
                    worksheet.Cells[row, 64].Value = "Mes 5";
                    worksheet.Cells[row, 65].Value = "Mes 6";
                    worksheet.Cells[row, 66].Value = "Mes 7";
                    worksheet.Cells[row, 67].Value = "Mes 8";
                    worksheet.Cells[row, 68].Value = "Mes 9";
                    worksheet.Cells[row, 69].Value = "Mes 10";
                    worksheet.Cells[row, 70].Value = "Mes 11";
                    worksheet.Cells[row, 71].Value = "Mes 12";
                    worksheet.Cells[row, 72].Value = "Mes 13";
                    worksheet.Cells[row, 73].Value = "Mes 14";
                    worksheet.Cells[row, 74].Value = "Mes 15";
                    worksheet.Cells[row, 75].Value = "Mes 16";
                    worksheet.Cells[row, 76].Value = "Mes 17";
                    worksheet.Cells[row, 77].Value = "Mes 18";
                    worksheet.Cells[row, 78].Value = "Mes 19";
                    worksheet.Cells[row, 79].Value = "Mes 20";
                    worksheet.Cells[row, 80].Value = "Mes 21";
                    worksheet.Cells[row, 81].Value = "Mes 22";
                    worksheet.Cells[row, 82].Value = "Mes 23";
                    worksheet.Cells[row, 83].Value = "Mes 24";

                    worksheet.Cells[row, 87].Value = "Clientes";
                    worksheet.Cells[row, 88].Value = "Mes 1";
                    worksheet.Cells[row, 89].Value = "Mes 2";
                    worksheet.Cells[row, 90].Value = "Mes 3";
                    worksheet.Cells[row, 91].Value = "Mes 4";
                    worksheet.Cells[row, 92].Value = "Mes 5";
                    worksheet.Cells[row, 93].Value = "Mes 6";
                    worksheet.Cells[row, 94].Value = "Mes 7";
                    worksheet.Cells[row, 95].Value = "Mes 8";
                    worksheet.Cells[row, 96].Value = "Mes 9";
                    worksheet.Cells[row, 97].Value = "Mes 10";
                    worksheet.Cells[row, 98].Value = "Mes 11";
                    worksheet.Cells[row, 99].Value = "Mes 12";
                    worksheet.Cells[row, 100].Value = "Mes 13";
                    worksheet.Cells[row, 101].Value = "Mes 14";
                    worksheet.Cells[row, 102].Value = "Mes 15";
                    worksheet.Cells[row, 103].Value = "Mes 16";
                    worksheet.Cells[row, 104].Value = "Mes 17";
                    worksheet.Cells[row, 105].Value = "Mes 18";
                    worksheet.Cells[row, 106].Value = "Mes 19";
                    worksheet.Cells[row, 107].Value = "Mes 20";
                    worksheet.Cells[row, 108].Value = "Mes 21";
                    worksheet.Cells[row, 109].Value = "Mes 22";
                    worksheet.Cells[row, 110].Value = "Mes 23";
                    worksheet.Cells[row, 111].Value = "Mes 24";

                    foreach (var item in data)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = item.InitDate;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 2].Value = item.EndDate;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 3].Value = item.FirstPedidosCount;
                        worksheet.Cells[row, 4].Value = item.Client30DaysAfterCount;
                        worksheet.Cells[row, 5].Value = item.Client60DaysAfterCount;
                        worksheet.Cells[row, 6].Value = item.Client90DaysAfterCount;
                        worksheet.Cells[row, 7].Value = item.Client120DaysAfterCount;
                        worksheet.Cells[row, 8].Value = item.Client150DaysAfterCount;
                        worksheet.Cells[row, 9].Value = item.Client180DaysAfterCount;
                        worksheet.Cells[row, 10].Value = item.Client210DaysAfterCount;
                        worksheet.Cells[row, 11].Value = item.Client240DaysAfterCount;
                        worksheet.Cells[row, 12].Value = item.Client270DaysAfterCount;
                        worksheet.Cells[row, 13].Value = item.Client300DaysAfterCount;
                        worksheet.Cells[row, 14].Value = item.Client330DaysAfterCount;
                        worksheet.Cells[row, 15].Value = item.Client360DaysAfterCount;
                        worksheet.Cells[row, 16].Value = item.Client390DaysAfterCount;
                        worksheet.Cells[row, 17].Value = item.Client420DaysAfterCount;
                        worksheet.Cells[row, 18].Value = item.Client450DaysAfterCount;
                        worksheet.Cells[row, 19].Value = item.Client480DaysAfterCount;
                        worksheet.Cells[row, 20].Value = item.Client510DaysAfterCount;
                        worksheet.Cells[row, 21].Value = item.Client540DaysAfterCount;
                        worksheet.Cells[row, 22].Value = item.Client570DaysAfterCount;
                        worksheet.Cells[row, 23].Value = item.Client600DaysAfterCount;
                        worksheet.Cells[row, 24].Value = item.Client630DaysAfterCount;
                        worksheet.Cells[row, 25].Value = item.Client660DaysAfterCount;
                        worksheet.Cells[row, 26].Value = item.Client690DaysAfterCount;
                        worksheet.Cells[row, 27].Value = item.Client720DaysAfterCount;

                        worksheet.Cells[row, 29].Value = item.InitDate;
                        worksheet.Cells[row, 29].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 30].Value = item.EndDate;
                        worksheet.Cells[row, 30].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 31].Value = item.FirstPedidosCount;
                        worksheet.Cells[row, 32].Value = item.Client30DaysAfterPercentage;
                        worksheet.Cells[row, 32].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 33].Value = item.Client60DaysAfterPercentage;
                        worksheet.Cells[row, 33].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 34].Value = item.Client90DaysAfterPercentage;
                        worksheet.Cells[row, 34].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 35].Value = item.Client120DaysAfterPercentage;
                        worksheet.Cells[row, 35].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 36].Value = item.Client150DaysAfterPercentage;
                        worksheet.Cells[row, 36].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 37].Value = item.Client180DaysAfterPercentage;
                        worksheet.Cells[row, 37].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 38].Value = item.Client210DaysAfterPercentage;
                        worksheet.Cells[row, 38].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 39].Value = item.Client240DaysAfterPercentage;
                        worksheet.Cells[row, 39].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 40].Value = item.Client270DaysAfterPercentage;
                        worksheet.Cells[row, 40].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 41].Value = item.Client300DaysAfterPercentage;
                        worksheet.Cells[row, 41].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 42].Value = item.Client330DaysAfterPercentage;
                        worksheet.Cells[row, 42].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 43].Value = item.Client360DaysAfterPercentage;
                        worksheet.Cells[row, 43].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 44].Value = item.Client390DaysAfterPercentage;
                        worksheet.Cells[row, 44].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 45].Value = item.Client420DaysAfterPercentage;
                        worksheet.Cells[row, 45].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 46].Value = item.Client450DaysAfterPercentage;
                        worksheet.Cells[row, 46].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 47].Value = item.Client480DaysAfterPercentage;
                        worksheet.Cells[row, 47].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 48].Value = item.Client510DaysAfterPercentage;
                        worksheet.Cells[row, 48].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 49].Value = item.Client540DaysAfterPercentage;
                        worksheet.Cells[row, 49].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 50].Value = item.Client570DaysAfterPercentage;
                        worksheet.Cells[row, 50].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 51].Value = item.Client600DaysAfterPercentage;
                        worksheet.Cells[row, 51].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 52].Value = item.Client630DaysAfterPercentage;
                        worksheet.Cells[row, 52].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 53].Value = item.Client660DaysAfterPercentage;
                        worksheet.Cells[row, 53].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 54].Value = item.Client690DaysAfterPercentage;
                        worksheet.Cells[row, 54].Style.Numberformat.Format = "#0.00%";
                        worksheet.Cells[row, 55].Value = item.Client720DaysAfterPercentage;
                        worksheet.Cells[row, 55].Style.Numberformat.Format = "#0.00%";

                        worksheet.Cells[row, 57].Value = item.InitDate;
                        worksheet.Cells[row, 57].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 58].Value = item.EndDate;
                        worksheet.Cells[row, 58].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 59].Value = item.FirstPedidosCount;
                        worksheet.Cells[row, 60].Value = item.Client30DaysAfterTicket;
                        worksheet.Cells[row, 61].Value = item.Client60DaysAfterTicket;
                        worksheet.Cells[row, 62].Value = item.Client90DaysAfterTicket;
                        worksheet.Cells[row, 63].Value = item.Client120DaysAfterTicket;
                        worksheet.Cells[row, 64].Value = item.Client150DaysAfterTicket;
                        worksheet.Cells[row, 65].Value = item.Client180DaysAfterTicket;
                        worksheet.Cells[row, 66].Value = item.Client210DaysAfterTicket;
                        worksheet.Cells[row, 67].Value = item.Client240DaysAfterTicket;
                        worksheet.Cells[row, 68].Value = item.Client270DaysAfterTicket;
                        worksheet.Cells[row, 69].Value = item.Client300DaysAfterTicket;
                        worksheet.Cells[row, 70].Value = item.Client330DaysAfterTicket;
                        worksheet.Cells[row, 71].Value = item.Client360DaysAfterTicket;
                        worksheet.Cells[row, 72].Value = item.Client390DaysAfterTicket;
                        worksheet.Cells[row, 73].Value = item.Client420DaysAfterTicket;
                        worksheet.Cells[row, 74].Value = item.Client450DaysAfterTicket;
                        worksheet.Cells[row, 75].Value = item.Client480DaysAfterTicket;
                        worksheet.Cells[row, 76].Value = item.Client510DaysAfterTicket;
                        worksheet.Cells[row, 77].Value = item.Client540DaysAfterTicket;
                        worksheet.Cells[row, 78].Value = item.Client570DaysAfterTicket;
                        worksheet.Cells[row, 79].Value = item.Client600DaysAfterTicket;
                        worksheet.Cells[row, 80].Value = item.Client630DaysAfterTicket;
                        worksheet.Cells[row, 81].Value = item.Client660DaysAfterTicket;
                        worksheet.Cells[row, 82].Value = item.Client690DaysAfterTicket;
                        worksheet.Cells[row, 83].Value = item.Client720DaysAfterTicket;

                        worksheet.Cells[row, 85].Value = item.InitDate;
                        worksheet.Cells[row, 85].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 86].Value = item.EndDate;
                        worksheet.Cells[row, 86].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 87].Value = item.FirstPedidosCount;
                        worksheet.Cells[row, 88].Value = item.Client30DaysAfterRecurrence;
                        worksheet.Cells[row, 89].Value = item.Client60DaysAfterRecurrence;
                        worksheet.Cells[row, 90].Value = item.Client90DaysAfterRecurrence;
                        worksheet.Cells[row, 91].Value = item.Client120DaysAfterRecurrence;
                        worksheet.Cells[row, 92].Value = item.Client150DaysAfterRecurrence;
                        worksheet.Cells[row, 93].Value = item.Client180DaysAfterRecurrence;
                        worksheet.Cells[row, 94].Value = item.Client210DaysAfterRecurrence;
                        worksheet.Cells[row, 95].Value = item.Client240DaysAfterRecurrence;
                        worksheet.Cells[row, 96].Value = item.Client270DaysAfterRecurrence;
                        worksheet.Cells[row, 97].Value = item.Client300DaysAfterRecurrence;
                        worksheet.Cells[row, 98].Value = item.Client330DaysAfterRecurrence;
                        worksheet.Cells[row, 99].Value = item.Client360DaysAfterRecurrence;
                        worksheet.Cells[row, 100].Value = item.Client390DaysAfterRecurrence;
                        worksheet.Cells[row, 101].Value = item.Client420DaysAfterRecurrence;
                        worksheet.Cells[row, 102].Value = item.Client450DaysAfterRecurrence;
                        worksheet.Cells[row, 103].Value = item.Client480DaysAfterRecurrence;
                        worksheet.Cells[row, 104].Value = item.Client510DaysAfterRecurrence;
                        worksheet.Cells[row, 105].Value = item.Client540DaysAfterRecurrence;
                        worksheet.Cells[row, 106].Value = item.Client570DaysAfterRecurrence;
                        worksheet.Cells[row, 107].Value = item.Client600DaysAfterRecurrence;
                        worksheet.Cells[row, 108].Value = item.Client630DaysAfterRecurrence;
                        worksheet.Cells[row, 109].Value = item.Client660DaysAfterRecurrence;
                        worksheet.Cells[row, 110].Value = item.Client690DaysAfterRecurrence;
                        worksheet.Cells[row, 111].Value = item.Client720DaysAfterRecurrence;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    #endregion

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_reporte_mkt.xlsx");
            }
        }

        // Distribución de ventas por categoría padre
        [HttpGet]
        public IActionResult GenerateExcel76(int months = 6)
        {
            var controlDate = DateTime.Now.AddMonths(months * -1).Date;
            var allOrders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate >= controlDate).ToList();
            var groupedByMonth = allOrders.GroupBy(x => new DateTime(x.SelectedShippingDate.Value.Year, x.SelectedShippingDate.Value.Month, 1));
            var categories = _categoryService.GetAllCategories().Where(x => x.ParentCategoryId == 0 && !x.Deleted).OrderBy(x => x.DisplayOrder).ToList();


            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int col = 1;
                    worksheet.Cells[row, 1].Value = "Distribución de ventas";

                    foreach (var date in groupedByMonth)
                    {
                        col++;
                        worksheet.Cells[row, col].Value = date.Key.ToString("MM-yyyy");
                    }

                    row = 1;
                    foreach (var category in categories)
                    {
                        col = 1;
                        row++;
                        worksheet.Cells[row, 1].Value = category.Name;
                        foreach (var date in groupedByMonth)
                        {
                            col++;
                            var itemsCount = date.SelectMany(x => x.OrderItems).Count();
                            var itemsOfCategoryCount = date.SelectMany(x => x.OrderItems)
                                .Where(x => x.Product.ProductCategories
                                .Where(z => z.Category.ParentCategoryId == 0 && !z.Category.Deleted)
                                .OrderBy(z => z.DisplayOrder).FirstOrDefault()?.CategoryId == category.Id).Count();
                            worksheet.Cells[row, col].Value = itemsCount == 0 ? 0 : (decimal)itemsOfCategoryCount / (decimal)itemsCount;
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_distribucion_ventas_cat_padre_{months}_meses.xlsx");
            }
        }

        // Cantidad de kilos vendidos por categoría
        [HttpGet]
        public IActionResult GenerateExcel77(int months = 6)
        {
            var controlDate = DateTime.Now.AddMonths(months * -1).Date;
            var allOrders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate >= controlDate).ToList();
            var groupedByMonth = allOrders.GroupBy(x => new DateTime(x.SelectedShippingDate.Value.Year, x.SelectedShippingDate.Value.Month, 1));
            var categories = _categoryService.GetAllCategories().Where(x => x.ParentCategoryId == 0 && !x.Deleted).OrderBy(x => x.DisplayOrder).ToList();
            var shippingRouteVehicle = _shippingVehicleRouteService.GetAll().ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int col = 1;
                    worksheet.Cells[row, 1].Value = "Kilos por camioneta";

                    foreach (var date in groupedByMonth)
                    {
                        col++;
                        worksheet.Cells[row, col].Value = date.Key.ToString("MM-yyyy");
                    }

                    row = 1;
                    foreach (var category in categories)
                    {
                        col = 1;
                        row++;
                        worksheet.Cells[row, 1].Value = category.Name;
                        foreach (var date in groupedByMonth)
                        {
                            col++;
                            var validItems = date.SelectMany(x => x.OrderItems)
                                .Where(x => x.Product.ProductCategories
                                .Where(z => z.Category.ParentCategoryId == 0 && !z.Category.Deleted)
                                .OrderBy(z => z.DisplayOrder).FirstOrDefault()?.CategoryId == category.Id)
                                .Where(x => x.Product.WeightInterval > 0 || x.Product.EquivalenceCoefficient > 0)
                                .GroupBy(x => x.Product)
                                .ToList();
                            if (validItems.Count > 0)
                            {
                                decimal totalWeight = 0;
                                foreach (var item in validItems)
                                {
                                    var result = GetQty(item.Key, item.Select(x => x.Quantity).DefaultIfEmpty().Sum());
                                    totalWeight += result.Item1;
                                }

                                var vehiclesCountInMonth = shippingRouteVehicle.Where(x => x.ShippingDate.Month == date.Key.Month && x.ShippingDate.Year == date.Key.Year).Count();
                                worksheet.Cells[row, col].Value = vehiclesCountInMonth == 0 ? 0 : (decimal)totalWeight / (decimal)vehiclesCountInMonth;
                            }
                            else worksheet.Cells[row, col].Value = "N/A";
                        }

                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_kilos_x_camioneta_cat_padre_{months}_meses.xlsx");
            }
        }

        // Hora final de entrega por ruta
        [HttpGet]
        public IActionResult GenerateExcel78(int months = 6)
        {
            var controlDate = DateTime.Now.AddMonths(months * -1).Date;
            var allOrders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate)
                .GroupBy(x => x.SelectedShippingDate)
                .ToList();
            var routes = _shippingRouteService.GetAll().OrderBy(x => x.RouteName).ToList();
            var shippingVehicleRoute = _shippingVehicleRouteService.GetAll().Where(x => x.ShippingDate >= controlDate).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int col = 1;
                    worksheet.Cells[row, 1].Value = "Hora de última entrega";

                    foreach (var route in routes)
                    {
                        col++;
                        worksheet.Cells[row, col].Value = route.RouteName;
                    }

                    row = 1;
                    foreach (var group in allOrders)
                    {
                        col = 1;
                        row++;
                        worksheet.Cells[row, 1].Value = group.Key.Value;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mm-yyyy";
                        foreach (var route in routes)
                        {
                            col++;
                            var deliveryTime = group.Where(x => x.RouteId == route.Id)
                                .SelectMany(x => x.Shipments)
                                .Select(x => x.DeliveryDateUtc)
                                .Where(x => x.HasValue)
                                .OrderByDescending(x => x)
                                .FirstOrDefault();

                            if (deliveryTime.HasValue)
                            {
                                var date = deliveryTime.Value.ToLocalTime();
                                worksheet.Cells[row, col].Value = date.ToString("HH:mm");
                            }
                            else worksheet.Cells[row, col].Value = "S/I";
                        }

                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_hora_entrega_por_ruta_{months}_meses.xlsx");
            }
        }

        //Venta y costo por categoria
        [HttpGet]
        public IActionResult GenerateExcel79(int catId, int days = 30)
        {
            var controlDate = DateTime.Now.AddDays(days * -1);
            var category = _categoryService.GetCategoryById(catId);
            if (category == null) return NotFound();

            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate >= controlDate).ToList();
            var orderIds = orders.Select(x => x.Id).ToList();
            var orderItems = orders.SelectMany(x => x.OrderItems).ToList();
            var orderReports = _orderReportService.GetAll().Where(x => orderIds.Contains(x.OrderId)).ToList();

            var items = orderItems.GroupBy(x => x.ProductId).ToList();
            var products = _productService.GetAllProductsQuery().ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Producto";
                    worksheet.Cells[row, 2].Value = "Categoría padre";
                    worksheet.Cells[row, 3].Value = "Categoría hijo";
                    worksheet.Cells[row, 4].Value = "Unidad";
                    worksheet.Cells[row, 5].Value = "Costo unitario promedio";
                    worksheet.Cells[row, 6].Value = "Precio de venta unitario promedio";
                    worksheet.Cells[row, 7].Value = "Cantidad vendida";

                    foreach (var group in items)
                    {
                        var product = products.Where(x => group.Key == x.Id).FirstOrDefault();
                        if (product == null || !product.ProductCategories.Where(x => x.CategoryId == catId).Any())
                            continue;

                        row++;

                        var qty = GetQty(product, group.Select(x => x.Quantity).DefaultIfEmpty().Sum());
                        var totalSale = group.Select(x => x.PriceInclTax).Sum();
                        var avgUnitPrice = totalSale / qty.Item1;
                        var itemIds = group.Select(x => x.Id).ToList();
                        var cost = orderReports.Where(x => itemIds.Contains(x.OrderItemId)).Select(x => x.UnitCost).DefaultIfEmpty().Average();
                        var parentCategories = product.ProductCategories.Where(x => x.Category.ParentCategoryId == 0).Select(x => x.Category.Name).ToList();
                        var childCategories = product.ProductCategories.Where(x => x.Category.ParentCategoryId != 0).Select(x => x.Category.Name).ToList();

                        worksheet.Cells[row, 1].Value = product.Name;
                        worksheet.Cells[row, 2].Value = parentCategories.Count() > 0 ? string.Join(", ", parentCategories) : "Sin categoría padre";
                        worksheet.Cells[row, 3].Value = childCategories.Count() > 0 ? string.Join(", ", childCategories) : "Sin categoría hijo"; ;
                        worksheet.Cells[row, 4].Value = qty.Item2;
                        worksheet.Cells[row, 5].Value = cost;
                        worksheet.Cells[row, 6].Value = avgUnitPrice;
                        worksheet.Cells[row, 7].Value = qty.Item1;

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_productos_vendidos_{category.Name}_{days}_dias.xlsx");
            }
        }

        // Reporte de franquicias
        [HttpGet]
        public IActionResult GenerateExcel80()
        {
            var vehicleRoute = _shippingVehicleRouteService.GetAll().Include(x => x.Vehicle).Include(x => x.Vehicle.Franchise).ToList();
            var dates = vehicleRoute.GroupBy(x => x.ShippingDate).Select(x => x.Key).OrderByDescending(x => x).ToList();
            var groupedByFranchise = vehicleRoute.GroupBy(x => x.Vehicle.Franchise).ToList();
            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => dates.Contains(x.SelectedShippingDate.Value)).ToList();
            var incidents = _incidentsService.GetAll().Where(x => x.AuthorizedStatusId == 1).ToList();
            var bonus = _franchiseBonusService.GetAll().ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    foreach (var franchiseGroup in groupedByFranchise)
                    {
                        var worksheet = xlPackage.Workbook.Worksheets.Add(franchiseGroup.Key.Name);
                        int row = 1;

                        worksheet.Cells[row, 1].Value = "Camioneta";
                        worksheet.Cells[row, 2].Value = "Parámetro";

                        int col = 2;

                        foreach (var date in dates)
                        {
                            col++;
                            worksheet.Cells[row, col].Value = date;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "dd-mm-yyyy";
                        }

                        var vehicleRouteGroup = franchiseGroup.GroupBy(x => x.Vehicle).ToList();
                        row = 2;
                        foreach (var group in vehicleRouteGroup)
                        {
                            worksheet.Cells[row, 1].Value = VehicleUtils.GetVehicleName(group.Key);
                            worksheet.Cells[row, 2].Value = "Total base";
                            worksheet.Cells[row + 1, 1].Value = VehicleUtils.GetVehicleName(group.Key);
                            worksheet.Cells[row + 1, 2].Value = "Cantidad de pedidos";
                            worksheet.Cells[row + 2, 1].Value = VehicleUtils.GetVehicleName(group.Key);
                            worksheet.Cells[row + 2, 2].Value = "Comisión";
                            worksheet.Cells[row + 3, 1].Value = VehicleUtils.GetVehicleName(group.Key);
                            worksheet.Cells[row + 3, 2].Value = "Bonos";
                            worksheet.Cells[row + 4, 1].Value = VehicleUtils.GetVehicleName(group.Key);
                            worksheet.Cells[row + 4, 2].Value = "Monto incidencias";
                            worksheet.Cells[row + 5, 1].Value = VehicleUtils.GetVehicleName(group.Key);
                            worksheet.Cells[row + 5, 2].Value = "Tipo de incidencias";

                            col = 3;

                            foreach (var date in dates)
                            {
                                var routeDateCombination = group.Select(x => new { x.ShippingDate, x.RouteId }).ToList();
                                var fitleredOrders = orders
                                    .Where(x => routeDateCombination.Contains(new { ShippingDate = x.SelectedShippingDate.Value, x.RouteId }))
                                    .Where(x => x.SelectedShippingDate == date)
                                    .ToList();

                                var pedidos = OrderUtils.GetPedidosGroupByList(fitleredOrders);

                                var baseNum = fitleredOrders.Select(x => x.OrderTotal - x.OrderShippingInclTax)
                                    .DefaultIfEmpty().Sum();
                                var comission = (baseNum * 8) / 100;
                                var bonusNum = bonus.Where(x => x.VehicleId == group.Key.Id && x.Date == date).Select(x => x.Amount).DefaultIfEmpty().Sum();
                                var filteredIncidents = incidents.Where(x => x.Date == date && x.VehicleId == group.Key.Id).ToList();

                                worksheet.Cells[row, col].Value = baseNum;
                                worksheet.Cells[row + 1, col].Value = pedidos.Count();
                                worksheet.Cells[row + 2, col].Value = comission;
                                worksheet.Cells[row + 3, col].Value = bonusNum;
                                worksheet.Cells[row + 4, col].Value = filteredIncidents.Select(x => x.Amount).DefaultIfEmpty().Sum();
                                worksheet.Cells[row + 5, col].Value = string.Join(((char)10).ToString(), filteredIncidents.Select(x => x.IncidentCode + " - " + x.Amount.ToString("C")));
                                worksheet.Cells[row + 5, col].Style.WrapText = true;
                                col++;
                            }

                            row += 6;

                            for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                            {
                                worksheet.Column(i).AutoFit();
                                worksheet.Cells[1, i].Style.Font.Bold = true;
                            }
                        }
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_reporte_franquicias.xlsx");
            }
        }

        // Porcentaje de compra por dia de la semana
        [HttpGet]
        public IActionResult GenerateExcel81()
        {
            var orders = OrderUtils.GetFilteredOrders(_orderService).ToList();
            var group = orders.GroupBy(x => x.SelectedShippingDate.Value.DayOfWeek).ToList();
            var allPedidos = OrderUtils.GetPedidosGroupByList(orders).ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Dia";
                    worksheet.Cells[row, 2].Value = "Cantidad de pedidos";
                    worksheet.Cells[row, 3].Value = "Porcentaje";

                    foreach (var item in group)
                    {
                        row++;
                        var pedidos = OrderUtils.GetPedidosGroupByList(item.Select(x => x).ToList());
                        worksheet.Cells[row, 1].Value = item.Key.ToString();
                        worksheet.Cells[row, 2].Value = pedidos.Count();
                        worksheet.Cells[row, 3].Value = (decimal)pedidos.Count() / (decimal)allPedidos.Count;

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"porcentaje_ventas_dia.xlsx");
            }
        }

        // Datos de clientes
        [HttpGet]
        public IActionResult GenerateExcel82(int months = 12)
        {
            var controlDate = DateTime.Now.AddMonths(months * -1).Date;
            var today = DateTime.Now.Date;
            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today).ToList();
            var customerIds = orders.Select(x => x.CustomerId).Distinct().ToList();
            var pedidos = OrderUtils.GetPedidosGroupByList(orders).ToList();
            var customers = _customerService.GetAllCustomersQuery().Where(x => x.Email != null && x.Email != "" && customerIds.Contains(x.Id)).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Id";
                    worksheet.Cells[row, 2].Value = "Nombre";
                    worksheet.Cells[row, 3].Value = "Apellido";
                    worksheet.Cells[row, 4].Value = "Correo";
                    worksheet.Cells[row, 5].Value = "Teléfono";
                    worksheet.Cells[row, 6].Value = "Colonia";
                    worksheet.Cells[row, 7].Value = "Delegación";
                    worksheet.Cells[row, 8].Value = "CP";
                    worksheet.Cells[row, 9].Value = "Cantidad de pedidos";
                    worksheet.Cells[row, 10].Value = "Total comprado";
                    worksheet.Cells[row, 11].Value = "Fecha de último pedido";
                    worksheet.Cells[row, 12].Value = "Compra por app o web";

                    foreach (var customer in customers)
                    {
                        row++;

                        var shippingAddress = customer.Addresses.FirstOrDefault();
                        var customerPedidos = pedidos.Where(x => x.Key.CustomerId == customer.Id).ToList();
                        var completedPedidos = customerPedidos.Where(x => x.Select(y => y.OrderStatus == OrderStatus.Complete).Any()).ToList();

                        worksheet.Cells[row, 1].Value = customer.Id;
                        worksheet.Cells[row, 2].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                        worksheet.Cells[row, 3].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
                        worksheet.Cells[row, 4].Value = customer.Email;
                        worksheet.Cells[row, 5].Value = shippingAddress?.PhoneNumber;
                        worksheet.Cells[row, 6].Value = shippingAddress?.Address2;
                        worksheet.Cells[row, 7].Value = shippingAddress?.City;
                        worksheet.Cells[row, 8].Value = shippingAddress?.ZipPostalCode;
                        worksheet.Cells[row, 9].Value = customerPedidos.Count;
                        worksheet.Cells[row, 10].Value = customerPedidos.Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 11].Value = customerPedidos.Select(x => x.Select(y => y.SelectedShippingDate.Value).OrderBy(y => y).LastOrDefault()).OrderBy(x => x).LastOrDefault();
                        worksheet.Cells[row, 11].Style.Numberformat.Format = "dd-mm-yyyy";

                        var hasMobile = customerPedidos.SelectMany(x => x).Where(x => x.OrderNotes.Where(y => y.Note == "Orden creada desde aplicación móvil.").Any()).Any();
                        worksheet.Cells[row, 12].Value = hasMobile ? "App" : "Web";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"demografico_clientes_{months}_meses.xlsx");
            }
        }

        private int DeliveredInTimeCount(List<IGrouping<dynamic, Order>> completedPedidos)
        {
            int inTimeCount = 0;
            foreach (var pedido in completedPedidos)
            {
                var deliveredDate = pedido.FirstOrDefault()
                    .Shipments.Where(x => x.DeliveryDateUtc.HasValue)
                    .Select(x => x.DeliveryDateUtc.Value)
                    .OrderBy(x => x)
                    .FirstOrDefault();
                if (deliveredDate == default) continue;
                var deliveredDateCom = deliveredDate.ToLocalTime();
                var selectedShippingTime = pedido.FirstOrDefault().SelectedShippingTime;
                if (OrderUtils.CheckIfDeliveredInTime(selectedShippingTime, deliveredDateCom))
                    inTimeCount++;
            }
            return inTimeCount;
        }

        // Porcentaje de compra por bloque de horario
        [HttpGet]
        public IActionResult GenerateExcel83()
        {
            var orders = OrderUtils.GetFilteredOrders(_orderService).ToList();
            var group = orders.GroupBy(x => x.SelectedShippingTime).ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Horario";
                    worksheet.Cells[row, 2].Value = "Cantidad de pedidos";
                    worksheet.Cells[row, 3].Value = "Porcentaje";

                    foreach (var item in group)
                    {
                        row++;
                        var pedidos = OrderUtils.GetPedidosGroupByList(item.Select(x => x).ToList());
                        worksheet.Cells[row, 1].Value = item.Key.ToString();
                        worksheet.Cells[row, 2].Value = pedidos.Count();
                        worksheet.Cells[row, 3].Value = (decimal)pedidos.Count() / (decimal)orders.Count;

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"porcentaje_ventas_horario.xlsx");
            }
        }

        // Clientes que mas compran
        [HttpGet]
        public IActionResult GenerateExcel84(int months = 6)
        {
            var controlDate = DateTime.Now.AddMonths(months * -1).Date;
            var today = DateTime.Now.Date;
            var customers = _customerService.GetAllCustomersQuery().Where(x => x.Email != null && x.Email != "").ToList();
            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today).ToList();
            var pedidos = OrderUtils.GetPedidosGroupByList(orders).ToList();
            var pedidosGroupedByClient = pedidos.GroupBy(x => x.Key.CustomerId).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Nombre";
                    worksheet.Cells[row, 2].Value = "Edad";
                    worksheet.Cells[row, 3].Value = "Género";
                    worksheet.Cells[row, 4].Value = "Código postal";
                    worksheet.Cells[row, 5].Value = "Teléfono";
                    worksheet.Cells[row, 6].Value = "Correo electrónico";
                    worksheet.Cells[row, 7].Value = "Cantidad de pedidos";
                    worksheet.Cells[row, 8].Value = "Monto total vendido";

                    foreach (var item in pedidosGroupedByClient)
                    {
                        row++;
                        var customer = _customerService.GetCustomerById((int)item.Key);
                        if (customer == null) continue;
                        worksheet.Cells[row, 1].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName) + " " + customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
                        var birthDate = customer.GetAttribute<DateTime?>(SystemCustomerAttributeNames.DateOfBirth);
                        if (birthDate.HasValue)
                        {
                            var age = today.Year - birthDate.Value.Year;
                            if (birthDate.Value.Date > today.AddYears(-age)) age--;
                            worksheet.Cells[row, 2].Value = age;
                        }
                        else
                        {
                            worksheet.Cells[row, 2].Value = "S/I";
                        }
                        worksheet.Cells[row, 3].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender);
                        var address = customer.Addresses.FirstOrDefault();
                        worksheet.Cells[row, 4].Value = address == null ? "S/I" : address.ZipPostalCode;
                        worksheet.Cells[row, 5].Value = address == null ? "S/I" : address.PhoneNumber;
                        worksheet.Cells[row, 6].Value = customer.Email;
                        worksheet.Cells[row, 7].Value = item.Count();
                        worksheet.Cells[row, 8].Value = item.Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Sum();

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"clientes_mas_compran_{months}_meses.xlsx");
            }
        }

        // Clientes dias transcurridos entre registro y primera orden
        [HttpGet]
        public IActionResult GenerateExcel85()
        {
            var ordersGroup = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.CreatedOnUtc.Year == 2021).GroupBy(x => x.Customer).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "ID Cliente";
                    worksheet.Cells[row, 2].Value = "Fecha de registro";
                    worksheet.Cells[row, 3].Value = "Fecha primera orden";
                    worksheet.Cells[row, 4].Value = "Cantidad de dias";

                    foreach (var item in ordersGroup)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = item.Key.Id;
                        var registrationDate = item.Key.CreatedOnUtc.ToLocalTime();
                        worksheet.Cells[row, 2].Value = registrationDate;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                        var firstOrderDate = item.Select(x => x.CreatedOnUtc.ToLocalTime()).OrderBy(x => x).FirstOrDefault();
                        worksheet.Cells[row, 3].Value = firstOrderDate;
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 4].Value = (registrationDate - firstOrderDate).Days;

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_dias_entre_registro_primera_orden.xlsx");
            }
        }

        // Clientes que nunca han hecho una compra
        [HttpGet]
        public IActionResult GenerateExcel86()
        {
            var customerIdsWithOrder = OrderUtils.GetFilteredOrders(_orderService).GroupBy(x => x.CustomerId).Select(x => x.Key).ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => x.Email != "" && x.Email != null && !x.Deleted)
                .ToList()
                .Where(x => !customerIdsWithOrder.Contains(x.Id))
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "ID Cliente";
                    worksheet.Cells[row, 2].Value = "Nombre";
                    worksheet.Cells[row, 3].Value = "Apellido";
                    worksheet.Cells[row, 4].Value = "Correo";
                    worksheet.Cells[row, 5].Value = "Teléfono";
                    worksheet.Cells[row, 6].Value = "Fecha de registro";

                    foreach (var customer in customers)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = customer.Id;
                        worksheet.Cells[row, 2].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                        worksheet.Cells[row, 3].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
                        worksheet.Cells[row, 4].Value = customer.Email;
                        string phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                        if (string.IsNullOrEmpty(phone))
                        {
                            phone = customer.Addresses.Select(x => x.PhoneNumber).FirstOrDefault();
                        }
                        worksheet.Cells[row, 5].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                        worksheet.Cells[row, 6].Value = customer.CreatedOnUtc.ToLocalTime();
                        worksheet.Cells[row, 6].Style.Numberformat.Format = "dd-MM-yyyy";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_sin_pedidos.xlsx");
            }
        }

        // Suma de venta de productos mes por mes de los ultimos 12 meses
        [HttpGet]
        public IActionResult GenerateExcel87()
        {
            var today = DateTime.Now.Date;
            var forDate = today.AddMonths(-12);
            var tempDate = new DateTime(forDate.Year, forDate.Month, 1).Date;

            var orders = _orderService.GetAllOrdersQuery().Where(x => x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10))
                .Where(x => x.SelectedShippingDate.HasValue && DbFunctions.TruncateTime(x.SelectedShippingDate.Value) >= tempDate);

            var groupingProducts = orders.Select(x => x.OrderItems)
                .SelectMany(x => x)
                .Where(x => x.Product.ProductCategories.Where(y => y.Category.ParentCategoryId != 0).Any())
                .GroupBy(x => x.Product)
                .OrderByDescending(x => x.Key.DisplayOrder)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    var months = 12;
                    worksheet.Cells[row, 1].Value = "Producto";
                    worksheet.Cells[row, 2].Value = "Categoría hijo";
                    worksheet.Cells[row, 3].Value = "Categoría padre";
                    for (int i = 0; i < months; i++)
                    {
                        var dateCell = "";
                        if (i > 0)
                            dateCell = MonthName(today.AddMonths(-i).Month) + " " + today.AddMonths(-i).ToString("yyyy");
                        else
                            dateCell = MonthName(today.Month) + " " + today.ToString("yyyy");

                        worksheet.Cells[row, 4 + i].Value = dateCell;
                    }

                    foreach (var item in groupingProducts)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = item.Key.Name;
                        worksheet.Cells[row, 2].Value =
                            string.Join(", ", item.Key.ProductCategories.Where(x => x.Category.ParentCategoryId != 0).Select(x => x.Category.Name));
                        worksheet.Cells[row, 3].Value =
                            string.Join(", ", item.Key.ProductCategories.Where(x => x.Category.ParentCategoryId == 0).Select(x => x.Category.Name));
                        for (int i = 0; i < months; i++)
                        {
                            var minusMonths = 0;
                            if (i > 0)
                                minusMonths = -1 * i;
                            forDate = today.AddMonths(minusMonths);
                            var currentMonthDateMax = new DateTime(forDate.Year, forDate.Month, DateTime.DaysInMonth(forDate.Year, forDate.Month)).Date;
                            var currentMonthDateMin = new DateTime(forDate.Year, forDate.Month, 1).Date;
                            var orderItemList = item.ToList();
                            var productOrderItemsByMonth = orderItemList
                            .Where(x => x.Order.SelectedShippingDate.HasValue &&
                            currentMonthDateMin <= x.Order.SelectedShippingDate.Value.Date &&
                            x.Order.SelectedShippingDate.Value.Date <= currentMonthDateMax)
                            .ToList();
                            var monthSales = productOrderItemsByMonth.Select(x => x.PriceInclTax).Sum();
                            worksheet.Cells[row, 4 + i].Value = monthSales;
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_venta_productos_12_meses.xlsx");
            }
        }

        // Compras por mes hechas en una dirección
        [HttpGet]
        public IActionResult GenerateExcel88(string address)
        {
            var ordersGroup = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.ShippingAddress.Address1.ToLower().Contains(address.ToLower()))
                .ToList()
                .GroupBy(x => new DateTime(x.SelectedShippingDate.Value.Year, x.SelectedShippingDate.Value.Month, 1))
                .OrderBy(x => x.Key)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Mes";
                    worksheet.Cells[row, 2].Value = "Cantidad de pedidos en " + address;

                    foreach (var group in ordersGroup)
                    {
                        row++;
                        var pedidos = OrderUtils.GetPedidosGroupByList(group.Select(x => x).ToList()).ToList();

                        worksheet.Cells[row, 1].Value = group.Key;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "MMMM, yyyy";
                        worksheet.Cells[row, 2].Value = pedidos.Count();

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_pedidos_mensuales_{address}.xlsx");
            }
        }

        // Clientes que su primer pedido compraron X, cuanto compraron por mes luego
        [HttpGet]
        public IActionResult GenerateExcel89(int orderTotal)
        {
            var allOrders = OrderUtils.GetFilteredOrders(_orderService).ToList();
            var customAmountOrders = allOrders.Where(x => x.OrderTotal < orderTotal).ToList();
            var removingPedidos = OrderUtils.GetPedidosGroupByList(customAmountOrders).Where(x => x.Count() == 1).SelectMany(x => x).ToList();
            var firstOrder = new List<Order>();
            foreach (var item in removingPedidos)
            {
                if (!allOrders.Where(x => x.SelectedShippingDate < item.SelectedShippingDate.Value && x.CustomerId == item.CustomerId).Any())
                    firstOrder.Add(item);
            }

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Id Usuario";
                    worksheet.Cells[row, 2].Value = "Monto primera compra";

                    for (int i = 1; i <= 24; i++)
                        worksheet.Cells[1, i + 2].Value = "Mes " + i;

                    foreach (var order in firstOrder)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = order.CustomerId;
                        worksheet.Cells[row, 2].Value = order.OrderTotal;

                        for (int i = 1; i <= 24; i++)
                        {
                            DateTime controlDate = order.SelectedShippingDate.Value.AddMonths(i);
                            var amount = allOrders
                                .Where(x => x.CustomerId == order.CustomerId && x.SelectedShippingDate.Value.Month == controlDate.Month && x.SelectedShippingDate.Value.Year == controlDate.Year)
                                .Select(x => x.OrderTotal)
                                .DefaultIfEmpty()
                                .ToList();
                            worksheet.Cells[row, i + 2].Value = amount;
                        }

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_compra_mensual_monto_minimo_{orderTotal}.xlsx");
            }
        }

        // Dia con mas pedidos
        [HttpGet]
        public IActionResult GenerateExcel90(int daysCount = 20, int months = 12)
        {
            var controlDate = DateTime.Now.AddMonths(months * -1).Date;
            var allOrders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate >= controlDate).ToList();
            var pedidos = OrderUtils.GetPedidosGroupByList(allOrders).ToList().GroupBy(x => x.Key.SelectedShippingDate).OrderByDescending(x => x.Count()).Take(daysCount);

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Fecha";
                    worksheet.Cells[row, 2].Value = "Cantidad de pedidos";

                    foreach (var group in pedidos)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = group.Key;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 2].Value = group.Count();

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_{daysCount}_dias_mas_pedidos_ultimos_{months}_meses.xlsx");
            }
        }

        // Lista de costos y cantidades de productos vendidos por semana, seleccionando fabricante y meses
        [HttpGet]
        public IActionResult GenerateExcel91(int fbId, int months = 2)
        {
            var today = DateTime.Now.Date;
            var controlDate = today.AddMonths(months * -1).Date;
            while (controlDate.DayOfWeek > 0)
            {
                controlDate = controlDate.AddDays(-1);
            }

            var manufacturer = _manufacturerService.GetManufacturerById(fbId);
            if (manufacturer != null)
            {
                var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today)
                    .Where(x => x.OrderItems.SelectMany(y => y.Product.ProductManufacturers).Select(y => fbId == y.ManufacturerId).Any());
                var groupedByWeek = orders
                    .OrderBy(x => x.SelectedShippingDate)
                    .ToList()
                    .GroupBy(x => x.SelectedShippingDate.Value.AddDays(-(int)x.SelectedShippingDate.Value.DayOfWeek))
                    .ToList();

                var products = _productService.GetAllProductsQuery()
                    .Where(x => !x.Deleted && x.ProductManufacturers.Where(y => fbId == y.ManufacturerId).Any())
                    .OrderBy(x => x.Name)
                    .ToList();
                var productIds = products.Select(x => x.Id).ToList();

                var orderReports = _orderReportService.GetAll()
                    .Where(x => productIds.Contains(x.ProductId) && controlDate <= x.OrderShippingDate).ToList();

                using (var stream = new MemoryStream())
                {
                    using (var xlPackage = new ExcelPackage(stream))
                    {
                        var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                        int row = 1;
                        worksheet.Cells[row, 1].Value = "Día inicial y final de la semana";
                        worksheet.Cells[row, 2].Value = "Producto";
                        worksheet.Cells[row, 3].Value = "Cantidad vendida";
                        worksheet.Cells[row, 4].Value = "Unidad";
                        worksheet.Cells[row, 5].Value = "Monto vendido";
                        //worksheet.Cells[row, 5].Value = "Costo";
                        row++;

                        foreach (var group in groupedByWeek)
                        {
                            var productGroup = group.SelectMany(x => x.OrderItems).Where(x => productIds.Contains(x.ProductId)).GroupBy(x => x.Product).ToList();
                            foreach (var product in productGroup)
                            {
                                var qty = GetQty(product.Key, product.Select(x => x.Quantity).Sum());
                                var itemIds = group.Select(x => x.Id).ToList();
                                //var cost = GetMedian(orderReports.Where(x => x.ProductId == product.Key.Id)
                                //    .Select(x => x.UnitCost).ToList());

                                worksheet.Cells[row, 1].Value = group.Key.ToString("dd/MM/yyyy") + " - " + group.Key.AddDays(7).ToString("dd/MM/yyyy");
                                worksheet.Cells[row, 2].Value = product.Key.Name;
                                worksheet.Cells[row, 3].Value = qty.Item1;
                                worksheet.Cells[row, 4].Value = qty.Item2;
                                worksheet.Cells[row, 5].Value = product.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
                                //worksheet.Cells[row, 5].Value = qty.Item1 * cost;
                                row++;

                            }
                        }

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }

                        xlPackage.Save();
                    }

                    return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_cantidad_productos_vendidos_farbricante_{manufacturer.Name}_ultimos_{months}_meses.xlsx");
                }
            }
            return BadRequest();
        }

        // Lista de costos y cantidades de productos vendidos por semana, seleccionando fabricante y meses
        [HttpGet]
        public IActionResult GenerateExcel92(int days = 100)
        {
            var today = DateTime.Now.Date;
            var controlDate = today.AddDays(days * -1).Date;
            var orders = GetFilteredOrders().Where(x => controlDate <= x.SelectedShippingDate && x.SelectedShippingDate <= today);

            return Ok("Ordenes: " + orders.Count() + ", Productos: " + orders.SelectMany(x => x.OrderItems).Count());
        }

        // Tiempo por ruta
        [HttpGet]
        public IActionResult GenerateExcel93(int days = 30)
        {
            var today = DateTime.Now.Date;
            var controlDate = today.AddDays(days * -1).Date;
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => controlDate <= x.SelectedShippingDate && x.SelectedShippingDate <= today)
                .ToList();
            var groupedByDate = OrderUtils.GetPedidosGroupByList(orders)
                .GroupBy(x => x.Key.SelectedShippingDate)
                .OrderBy(x => x.Key)
                .ToList();
            var routes = _shippingRouteService.GetAll().ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Fecha";
                    worksheet.Cells[row, 2].Value = "Ruta";
                    worksheet.Cells[row, 3].Value = "Pedido";
                    worksheet.Cells[row, 4].Value = "Tiempo (en minutos) de traslado desde último pedido (según Google)";
                    worksheet.Cells[row, 5].Value = "Número de productos";
                    worksheet.Cells[row, 6].Value = "Hora de entrega";
                    row++;

                    foreach (var dateGroup in groupedByDate)
                    {
                        var groupedByRoute = dateGroup.GroupBy(x => x.Select(y => y.RouteId).FirstOrDefault()).OrderBy(x => x.Key).ToList();
                        foreach (var routeGroup in groupedByRoute)
                        {
                            var orderedData = routeGroup.OrderBy(x => x.Select(y => y.RouteDisplayOrder).FirstOrDefault()).ToList();
                            for (int i = 1; i < orderedData.Count(); i++)
                            {
                                worksheet.Cells[row, 1].Value = dateGroup.Key;
                                worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mm-yyyy";
                                worksheet.Cells[row, 2].Value = routes.Where(x => x.Id == routeGroup.Key).Select(x => x.RouteName).FirstOrDefault();
                                worksheet.Cells[row, 3].Value = $"Pedido {i + 1}";
                                worksheet.Cells[row, 4].Value = orderedData[i].Select(x => x.PreviousPointTransferTime).DefaultIfEmpty().Sum() / 60;
                                worksheet.Cells[row, 5].Value = orderedData[i].SelectMany(x => x.OrderItems).Count();
                                worksheet.Cells[row, 6].Value = orderedData[i].SelectMany(x => x.Shipments.Where(y => y != null && y.DeliveryDateUtc.HasValue)).Select(x => x.DeliveryDateUtc).OrderBy(x => x).FirstOrDefault()?.ToLocalTime();
                                worksheet.Cells[row, 6].Style.Numberformat.Format = "dd-mm-yyyy";
                                row++;
                            }
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_reporte_entregas_{days}_dias.xlsx");
            }
        }

        // Numero de productos por dia
        [HttpGet]
        public IActionResult GenerateExcel94(int days = 30)
        {
            var today = DateTime.Now.Date;
            var controlDate = today.AddDays(days * -1).Date;
            var allOrders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today).ToList();
            var pedidos = OrderUtils.GetPedidosGroupByList(allOrders).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Fecha";
                    worksheet.Cells[row, 2].Value = "Pedido";
                    worksheet.Cells[row, 3].Value = "Numero de productos";

                    foreach (var group in pedidos)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = group.Key.SelectedShippingDate.Date;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 2].Value = string.Join(", ", group.Select(x => $"Orden {x.CustomOrderNumber}").ToList());
                        worksheet.Cells[row, 3].Value = group.SelectMany(x => x.OrderItems).Count();

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_numero_productos_dia_{days}_dias.xlsx");
            }
        }

        //Promedio de productos por mes
        [HttpGet]
        public IActionResult GenerateExcel95(int year = 2021)
        {
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate.HasValue && DbFunctions.TruncateTime(x.SelectedShippingDate.Value).Value.Year == year)
                .ToList();
            var pedidosGroup = OrderUtils.GetPedidosGroupByList(orders)
                .OrderBy(x => x.Key.SelectedShippingDate)
                .GroupBy(x => x.Key.SelectedShippingDate.ToString("MMMM, yyyy", new CultureInfo("es-MX")))
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Mes";
                    worksheet.Cells[row, 2].Value = "Promedio de productos";

                    foreach (var item in pedidosGroup)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = item.Key;
                        worksheet.Cells[row, 2].Value = item.Select(x => x.SelectMany(y => y.OrderItems).Count()).DefaultIfEmpty().Average();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_cantidad_productos_promedio_mensual_{year}.xlsx");
            }
        }

        //Historial de costos por producto
        [HttpGet]
        public IActionResult GenerateExcel96()
        {
            var reportStatus = _orderReportStatusService.GetAll().Where(x => x.StatusTypeId == 2)
                .GroupBy(x => DbFunctions.AddMilliseconds(x.ShippingDate, x.BuyerId))
                .Select(x => x.Key)
                .ToList();
            List<IGrouping<int, Domain.OrderReports.OrderReport>> reportGroup = _orderReportService.GetAll()
                .Where(x => reportStatus.Contains(DbFunctions.AddMilliseconds(x.OrderShippingDate, x.OriginalBuyerId)) && x.Invoice != null)
                .ToList()
                .GroupBy(x => x.ProductId)
                .Where(x => x.Key > 0)
                .ToList();
            var reportedProductIds = reportGroup.Select(x => x.Key).ToList();
            var products = _productService.GetAllProductsQuery().Where(x => reportedProductIds.Contains(x.Id)).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Id producto";
                    worksheet.Cells[row, 2].Value = "Categoría padre";
                    worksheet.Cells[row, 3].Value = "Categoría hijo";
                    worksheet.Cells[row, 4].Value = "Nombre del producto";
                    worksheet.Cells[row, 5].Value = "Último costo reportado";
                    for (int i = 0; i < 30; i++)
                    {
                        worksheet.Cells[row, i + 6].Value = "Último costo reportado -" + (i + 1);
                    }

                    foreach (var group in reportGroup)
                    {
                        var product = products.Where(x => x.Id == group.Key).FirstOrDefault();
                        if (product == null) continue;
                        row++;
                        var parentCat = product.ProductCategories.Where(x => x.Category.ParentCategoryId == 0).FirstOrDefault();
                        var childCat = product.ProductCategories.Where(x => x.Category.ParentCategoryId > 0).FirstOrDefault();
                        var orderedReports = group.Select(x => x).OrderBy(x => x.OrderShippingDate).Take(31).ToList();

                        worksheet.Cells[row, 1].Value = group.Key;
                        worksheet.Cells[row, 2].Value = parentCat == null ? "Sin categoría padre" : parentCat.Category.Name;
                        worksheet.Cells[row, 3].Value = childCat == null ? "Sin categoría hijo" : childCat.Category.Name;
                        worksheet.Cells[row, 4].Value = product.Name;

                        for (int i = 0; i < orderedReports.Count; i++)
                        {
                            worksheet.Cells[row, i + 5].Value = orderedReports[i].UpdatedUnitCost;
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_historico_de_costos.xlsx");
            }
        }

        // Productos modificados de fabricante
        [HttpGet]
        public IActionResult GenerateExcel97(int fbId)
        {
            var products = _productService.GetAllProductsQuery().Where(x => x.ProductManufacturers.Where(y => y.ManufacturerId == fbId).Any()).ToList();
            var productIds = products.Select(x => x.Id).ToList();
            var productLog = _productLogService.GetAll()
                .Where(x => productIds.Contains(x.ProductId))
                .OrderBy(x => x.CreatedOnUtc)
                .ToList();

            var manufacturer = products.FirstOrDefault().ProductManufacturers.Where(x => x.ManufacturerId == fbId).FirstOrDefault().Manufacturer.Name;

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Producto";
                    worksheet.Cells[row, 2].Value = "Fecha";
                    worksheet.Cells[row, 3].Value = "Modificación";

                    foreach (var log in productLog)
                    {
                        var product = products.Where(x => x.Id == log.ProductId).FirstOrDefault();
                        if (product == null) continue;
                        row++;

                        worksheet.Cells[row, 1].Value = product.Name;
                        worksheet.Cells[row, 2].Value = log.CreatedOnUtc.ToLocalTime();
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 3].Value = log.Message;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_modificaciones_{manufacturer}.xlsx");
            }
        }

        //Fecha de venta y si hubo faltante
        [HttpGet]
        public IActionResult GenerateExcel98(int fbId)
        {
            var products = _productService.GetAllProductsQuery().Where(x => x.ProductManufacturers.Any(y => y.ManufacturerId == fbId)).ToList();
            var productIds = products.Select(x => x.Id).ToList();
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.OrderItems.Where(y => productIds.Contains(y.ProductId)).Any())
                .ToList();
            var notDeliveredOrderIds = _notDeliveredOrderItemService.GetAll().Where(x => productIds.Contains(x.ProductId)).GroupBy(x => x.OrderId).Select(x => x.Key).ToList();
            var ordersGroup = orders.GroupBy(x => x.SelectedShippingDate.Value).OrderBy(x => x.Key);
            var manufacturer = products.Select(x => x.ProductManufacturers.Where(y => y.ManufacturerId == fbId).FirstOrDefault()).FirstOrDefault()?.Manufacturer?.Name;

            var notDeliveredDates = OrderUtils.GetFilteredOrders(_orderService).Where(x => notDeliveredOrderIds.Contains(x.Id)).GroupBy(x => x.SelectedShippingDate.Value).Select(x => x.Key).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Fecha";
                    worksheet.Cells[row, 2].Value = "¿Hubo faltante?";

                    foreach (var item in ordersGroup)
                    {
                        row++;
                        var dateOrderIds = item.Select(x => x.Id).ToList();
                        worksheet.Cells[row, 1].Value = item.Key;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 2].Value = notDeliveredDates.Where(x => x == item.Key).Any() ? "Si" : "No";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_faltantes_{manufacturer}.xlsx");
            }
        }

        // Cupones atención al cliente
        [HttpGet]
        public IActionResult GenerateExcel99()
        {
            var coupons = _discountService.GetAllDiscounts().Where(x => x.Name.StartsWith("REP") || x.Name.StartsWith("FAL") || x.Name.StartsWith("ATN") || x.Name.StartsWith("SAL") || x.Name.StartsWith("CAN")).ToList();
            var couponIds = coupons.Select(x => x.Id).ToList();
            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.DiscountUsageHistory.Where(y => couponIds.Contains(y.DiscountId)).Any()).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Prefijo de cupón";
                    worksheet.Cells[row, 2].Value = "Cantidad generados";
                    worksheet.Cells[row, 3].Value = "Cantidad usados";

                    row++;
                    worksheet.Cells[row, 1].Value = "REP";
                    var rep = coupons.Where(x => x.Name.StartsWith("REP")).ToList();
                    var repIds = rep.Select(x => x.Id).ToList();
                    var repOrders = orders.Where(x => x.DiscountUsageHistory.Where(y => repIds.Contains(y.DiscountId)).Any()).ToList();
                    worksheet.Cells[row, 2].Value = rep.Count;
                    worksheet.Cells[row, 3].Value = repOrders.Count;

                    row++;
                    worksheet.Cells[row, 1].Value = "FAL";
                    var fal = coupons.Where(x => x.Name.StartsWith("FAL")).ToList();
                    var falIds = fal.Select(x => x.Id).ToList();
                    var falOrders = orders.Where(x => x.DiscountUsageHistory.Where(y => falIds.Contains(y.DiscountId)).Any()).ToList();
                    worksheet.Cells[row, 2].Value = fal.Count;
                    worksheet.Cells[row, 3].Value = falOrders.Count;

                    row++;
                    worksheet.Cells[row, 1].Value = "ATN";
                    var atn = coupons.Where(x => x.Name.StartsWith("ATN")).ToList();
                    var atnIds = atn.Select(x => x.Id).ToList();
                    var atnOrders = orders.Where(x => x.DiscountUsageHistory.Where(y => atnIds.Contains(y.DiscountId)).Any()).ToList();
                    worksheet.Cells[row, 2].Value = atn.Count;
                    worksheet.Cells[row, 3].Value = atnOrders.Count;

                    row++;
                    worksheet.Cells[row, 1].Value = "SAL";
                    var sal = coupons.Where(x => x.Name.StartsWith("SAL")).ToList();
                    var salIds = sal.Select(x => x.Id).ToList();
                    var salOrders = orders.Where(x => x.DiscountUsageHistory.Where(y => salIds.Contains(y.DiscountId)).Any()).ToList();
                    worksheet.Cells[row, 2].Value = sal.Count;
                    worksheet.Cells[row, 3].Value = salOrders.Count;

                    row++;
                    worksheet.Cells[row, 1].Value = "CAN";
                    var can = coupons.Where(x => x.Name.StartsWith("CAN")).ToList();
                    var canIds = can.Select(x => x.Id).ToList();
                    var canOrders = orders.Where(x => x.DiscountUsageHistory.Where(y => canIds.Contains(y.DiscountId)).Any()).ToList();
                    worksheet.Cells[row, 2].Value = can.Count;
                    worksheet.Cells[row, 3].Value = canOrders.Count;

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_reporte_cupones_alicia.xlsx");
            }
        }

        // Retencion por cupon atencion a clientes
        [HttpGet]
        public IActionResult GenerateExcel100()
        {
            var controlDate = new DateTime(2021, 8, 1);
            var coupons = _discountService.GetAllDiscounts()
                .Where(x => x.Name.StartsWith("REP") || x.Name.StartsWith("FAL") || x.Name.StartsWith("ATN") || x.Name.StartsWith("SAL") || x.Name.StartsWith("CAN"))
                .ToList();
            var couponIds = coupons.Select(x => x.Id).ToList();
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate > controlDate)
                .ToList();
            var ordersWithCoupon = orders.Where(x => x.DiscountUsageHistory.Where(y => couponIds.Contains(y.DiscountId)).Any()).ToList();
            var ordersWithoutCoupon = orders.Where(x => !x.DiscountUsageHistory.Where(y => couponIds.Contains(y.DiscountId)).Any()).ToList();
            var couponUsedDates = ordersWithCoupon.GroupBy(x => x.SelectedShippingDate).Select(x => x.Key.Value).OrderBy(x => x).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Fecha";
                    worksheet.Cells[row, 2].Value = "Cantidad de clientes que usaron cupón";
                    worksheet.Cells[row, 3].Value = "Cantidad de clientes que no usaron cupón";
                    worksheet.Cells[row, 4].Value = "Cantidad de clientes que hicieron un pedido después de usar cupón";
                    worksheet.Cells[row, 5].Value = "Cantidad de clientes que hicieron un pedido después de no usar cupón";

                    foreach (var date in couponUsedDates)
                    {
                        row++;

                        var filteredOrdersWithCoupon = ordersWithCoupon.Where(x => x.SelectedShippingDate == date).ToList();
                        var filteredOrdersWithoutCoupon = ordersWithoutCoupon.Where(x => x.SelectedShippingDate == date).ToList();
                        var customerIdsWithCoupon = filteredOrdersWithCoupon.Select(x => x.CustomerId).ToList();
                        var customerIdsWithoutCoupon = filteredOrdersWithoutCoupon.Select(x => x.CustomerId).ToList();

                        worksheet.Cells[row, 1].Value = date;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 2].Value = filteredOrdersWithCoupon.GroupBy(x => x.CustomerId).Count();
                        worksheet.Cells[row, 3].Value = filteredOrdersWithoutCoupon.GroupBy(x => x.CustomerId).Count();
                        worksheet.Cells[row, 4].Value = orders.Where(x => x.SelectedShippingDate > date && customerIdsWithCoupon.Contains(x.CustomerId)).GroupBy(x => x.CustomerId).Count();
                        worksheet.Cells[row, 5].Value = orders.Where(x => x.SelectedShippingDate > date && customerIdsWithoutCoupon.Contains(x.CustomerId)).GroupBy(x => x.CustomerId).Count();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_retencion_cupones_alicia.xlsx");
            }
        }

        // Lista de pedidos por cliente con producto especificado
        [HttpGet]
        public IActionResult GenerateExcel101(int productId = 26716)
        {
            var product = _productService.GetAllProductsQuery().Where(x => x.Id == productId).FirstOrDefault();
            if (product == null)
                return BadRequest("Product with specified Id was not found or is deleted.");
            var ordersWithProduct = GetFilteredOrders()
                .Where(x => x.OrderItems.Select(y => y.ProductId).Contains(productId))
                .OrderBy(x => x.SelectedShippingDate)
                .ToList();
            var ordersGroupingByCustomer = ordersWithProduct
                .OrderBy(x => x.Customer.GetFullName())
                .GroupBy(x => x.CustomerId)
                .ToList();
            var customerIds = ordersGroupingByCustomer.Select(y => y.Key).ToList();
            var allOrdersOfCurrentCustomers = GetFilteredOrders()
                .Where(x => customerIds.Contains(x.CustomerId))
                .ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => customerIds.Contains(x.Id))
                .ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Nombre cliente";
                    worksheet.Cells[row, 2].Value = "Email cliente";
                    worksheet.Cells[row, 3].Value = "# de pedido";
                    worksheet.Cells[row, 4].Value = "¿Compraron después de la compra de una bolsa?";
                    worksheet.Cells[row, 5].Value = "# pedidos en total";
                    worksheet.Cells[row, 6].Value = "Código Postal";
                    worksheet.Cells[row, 7].Value = "¿Ha recomprado bolsa?";
                    row++;

                    foreach (var group in ordersGroupingByCustomer)
                    {
                        var currentCustomer = customers.Where(x => x.Id == group.Key).FirstOrDefault();
                        var firstOrderWithProduct = group.OrderBy(x => x.SelectedShippingDate).FirstOrDefault();
                        var allOrdersOfCustomer = allOrdersOfCurrentCustomers.Where(x => x.CustomerId == group.Key)
                            .OrderBy(x => x.SelectedShippingDate).ToList();
                        var allPedidosOfCustomer = OrderUtils.GetPedidosGroupByList(allOrdersOfCustomer).ToList();

                        worksheet.Cells[row, 1].Value = currentCustomer.GetFullName();
                        worksheet.Cells[row, 2].Value = currentCustomer.Email;

                        var pedidoWithFirstOrderProduct = allPedidosOfCustomer.Where(x => x.Select(y => y.Id).Contains(firstOrderWithProduct.Id)).FirstOrDefault();
                        var indexOfProductOrder = allPedidosOfCustomer.IndexOf(pedidoWithFirstOrderProduct) + 1;
                        worksheet.Cells[row, 3].Value = indexOfProductOrder;

                        var orderedAfterProduct = allOrdersOfCustomer.Where(x => firstOrderWithProduct.SelectedShippingDate < x.SelectedShippingDate).Any();
                        worksheet.Cells[row, 4].Value = orderedAfterProduct ? "Si" : "No";
                        worksheet.Cells[row, 5].Value = allPedidosOfCustomer.Count();
                        worksheet.Cells[row, 6].Value = firstOrderWithProduct.ShippingAddress.ZipPostalCode;

                        var hasReboughtProduct = group.Count() > 1;
                        worksheet.Cells[row, 7].Value = hasReboughtProduct ? "Si" : "No";
                        row++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_informacion_producto_{product.Name}_por_pedidos_de_clientes.xlsx");
            }
        }

        // Información de todos los usuarios sin ninguna orden
        [HttpGet]
        public IActionResult GenerateExcel102()
        {
            var customersWithOrders = GetFilteredOrders()
                .Select(x => x.CustomerId).Distinct().ToList();
            var customers = _customerService.GetAllCustomers()
                .Where(x => !x.Deleted && !customersWithOrders.Contains(x.Id) &&
                !string.IsNullOrEmpty(x.Email))
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Email";
                    worksheet.Cells[row, 2].Value = "Teléfono";
                    row++;

                    foreach (var customer in customers)
                    {
                        var phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                        var phoneNumber = !string.IsNullOrEmpty(phone) ? phone : !string.IsNullOrEmpty(customer.ShippingAddress?.PhoneNumber) ?
                            customer.ShippingAddress.PhoneNumber : customer.Addresses.Count > 0 ?
                            customer.Addresses.OrderByDescending(x => x.CreatedOnUtc).FirstOrDefault()?.PhoneNumber :
                            "";
                        worksheet.Cells[row, 1].Value = customer.Email;
                        worksheet.Cells[row, 2].Value = phoneNumber;
                        row++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_sin_ninguna_orden.xlsx");
            }
        }

        // Información de cupones que empiezan con letras especificadas, no usados
        [HttpGet]
        public IActionResult GenerateExcel103(string prefix = "REP,FAL,ATN,SAL,CAN")
        {
            if (string.IsNullOrEmpty(prefix))
                return BadRequest("No prefixes given");
            var prefixes = prefix.Split(',').ToList();
            if (!prefixes.Any())
                return BadRequest("No prefixes given");
            var coupons = new List<Discount>();
            foreach (var pref in prefixes)
            {
                var couponsWithPrefix = _discountService.GetAllDiscounts()
                    .Where(x => !string.IsNullOrEmpty(x.Name) &&
                    x.Name.ToLower().StartsWith(pref.ToLower())).ToList();
                if (couponsWithPrefix.Any())
                    coupons.AddRange(couponsWithPrefix);
            }

            if (coupons.Any())
            {
                var couponIds = coupons.Select(x => x.Id).ToList();
                var discountUsage = _discountService.GetAllDiscountUsageHistory()
                    .Where(x => couponIds.Contains(x.DiscountId))
                    .GroupBy(x => x.DiscountId)
                    .ToList();
                var usedCouponIds = discountUsage.Select(x => x.Key).ToList();
                coupons = coupons.Where(x => !usedCouponIds.Contains(x.Id)).ToList();
                var couponsFormat = string.Join(",", coupons.Select(x => x.Id));

                var customerOwnerIds = coupons.Select(x => x.CustomerOwnerId).ToList();
                var customers = _customerService.GetAllCustomersQuery()
                    .Where(x => customerOwnerIds.Contains(x.Id)).ToList();

                using (var stream = new MemoryStream())
                {
                    using (var xlPackage = new ExcelPackage(stream))
                    {
                        var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                        int row = 1;
                        worksheet.Cells[row, 1].Value = "Correo del usuario";
                        worksheet.Cells[row, 2].Value = "Nombre del usuario";
                        worksheet.Cells[row, 3].Value = "Teléfono";
                        worksheet.Cells[row, 4].Value = "Nombre del cupón";
                        worksheet.Cells[row, 5].Value = "Código del cupón";
                        row++;

                        foreach (var coupon in coupons)
                        {
                            var customerOfCoupon = customers.Where(x => x.Id == coupon.CustomerOwnerId).FirstOrDefault();
                            if (customerOfCoupon != null)
                            {
                                var phone = customerOfCoupon.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                                var phoneNumber = !string.IsNullOrEmpty(phone) ? phone : !string.IsNullOrEmpty(customerOfCoupon.ShippingAddress?.PhoneNumber) ?
                                    customerOfCoupon.ShippingAddress.PhoneNumber : customerOfCoupon.Addresses.Count > 0 ?
                                    customerOfCoupon.Addresses.OrderByDescending(x => x.CreatedOnUtc).FirstOrDefault()?.PhoneNumber :
                                    "";
                                worksheet.Cells[row, 1].Value = customerOfCoupon.Email;
                                worksheet.Cells[row, 2].Value = customerOfCoupon.GetFullName();
                                worksheet.Cells[row, 3].Value = phoneNumber;
                            }
                            worksheet.Cells[row, 4].Value = coupon.Name;
                            worksheet.Cells[row, 5].Value = coupon.CouponCode;
                            row++;
                        }

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }

                        xlPackage.Save();
                    }

                    return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_informacion_clientes_dueños_de_cupones.xlsx");
                }
            }
            return Ok();
        }

        // Información de productos que tienen tag especificado
        [HttpGet]
        public IActionResult GenerateExcel104(string tag = "ofrenda mexicana")
        {
            if (string.IsNullOrEmpty(tag))
                return BadRequest("No tag name given");
            var tagSpecified = _productTagService.GetProductTagByName(tag);
            if (tagSpecified == null)
                return BadRequest("No tag found with given name");

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Nombre";
                    worksheet.Cells[row, 2].Value = "Descripción corta";
                    worksheet.Cells[row, 3].Value = "Descripción larga";
                    worksheet.Cells[row, 4].Value = "Precio";
                    row++;

                    foreach (var product in tagSpecified.Products.OrderBy(x => x.Name))
                    {
                        worksheet.Cells[row, 1].Value = product.Name;
                        worksheet.Cells[row, 2].Value = product.ShortDescription;
                        worksheet.Cells[row, 3].Value = product.FullDescription;
                        worksheet.Cells[row, 4].Value = product.Price;
                        row++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_productos_con_tag_{tagSpecified.Name}.xlsx");
            }
        }

        // Número de clientes que han usado código de recomendación
        [HttpGet]
        public IActionResult GenerateExcel105()
        {
            var discounts = _discountService.GetAllDiscounts()
                .Where(x => x.CustomerOwnerId != 0)
                .ToList();
            var discountIds = discounts.Select(x => x.Id).ToList();

            var discountUsage = _discountService.GetAllDiscountUsageHistory()
                .Where(x => discountIds.Contains(x.DiscountId))
                .GroupBy(x => x.DiscountId)
                .ToList();
            var discountsUsedIds = discountUsage.Select(x => x.Key).ToList();
            var customerOwnerIds = discountUsage.SelectMany(x => x.Select(y => y.Discount.CustomerOwnerId)).Distinct().ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => customerOwnerIds.Contains(x.Id))
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Nombre";
                    worksheet.Cells[row, 2].Value = "Email";
                    worksheet.Cells[row, 3].Value = "Cupón";
                    row++;

                    foreach (var customer in customers)
                    {
                        var discountsOfCustomer = discounts.Where(x => x.CustomerOwnerId == customer.Id && discountsUsedIds.Contains(x.Id)).ToList();

                        worksheet.Cells[row, 1].Value = customer.GetFullName();
                        worksheet.Cells[row, 2].Value = customer.Email;
                        worksheet.Cells[row, 3].Value = string.Join(", ", discountsOfCustomer.Select(x => x.Name));
                        row++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_han_usado_su_codigo_de_recomendacion.xlsx");
            }

            //return Ok(discountUsage.Count());
        }

        // Información de productos que tienen tag especificado
        [HttpGet]
        public IActionResult GenerateExcel106(string from = null, string until = null)
        {
            var exponentialDate = DateTime.Now.Date;
            DateTime fromDate = exponentialDate.AddMonths(-12);
            if (!string.IsNullOrEmpty(from))
                fromDate = DateTime.ParseExact(from, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var untilDate = exponentialDate;
            if (!string.IsNullOrEmpty(until))
                untilDate = DateTime.ParseExact(until, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            if (fromDate > untilDate)
            {
                var temp = fromDate;
                fromDate = untilDate;
                untilDate = temp;
            }
            fromDate = new DateTime(fromDate.Year, fromDate.Month, 1);
            untilDate = new DateTime(untilDate.Year, untilDate.Month, DateTime.DaysInMonth(untilDate.Year, untilDate.Month));
            exponentialDate = fromDate;

            var orders = GetFilteredOrders()
                .Where(x => x.SelectedShippingDate != null &&
                fromDate <= x.SelectedShippingDate && x.SelectedShippingDate <= untilDate)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Mes/Año";
                    worksheet.Cells[row, 2].Value = "Cantidad";
                    row++;

                    do
                    {
                        if (row != 2)
                            exponentialDate = exponentialDate.AddMonths(1);
                        worksheet.Cells[row, 1].Value = exponentialDate.ToString("MM/yyyy");

                        var ordersWithinDateByCustomerId = orders.Where(x => (x.SelectedShippingDate ?? DateTime.Now).ToString("MM/yyyy") == exponentialDate.ToString("MM/yyyy"))
                            .GroupBy(x => x.CustomerId)
                            .ToList();
                        worksheet.Cells[row, 2].Value = ordersWithinDateByCustomerId.Count();
                        row++;
                    } while (exponentialDate.ToString("MM/yyyy") != untilDate.ToString("MM/yyyy"));

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ordenes_clientes_diferentes_por_mes_de_{fromDate:dd-MM-yyyy}_a_{untilDate:dd-MM-yyyy}.xlsx");
            }
        }

        // Información de cupones que empiezan con letras especificadas, no usados
        [HttpGet]
        public IActionResult GenerateExcel107(string prefix = "REP,FAL,ATN,SAL,CAN")
        {
            if (string.IsNullOrEmpty(prefix))
                return BadRequest("No prefixes given");
            var prefixes = prefix.Split(',').ToList();
            if (!prefixes.Any())
                return BadRequest("No prefixes given");
            var customersByPrefixes = new List<CustomerIdssOfCouponPrefix>();
            foreach (var pref in prefixes)
            {
                var couponsWithPrefix = _discountService.GetAllDiscounts()
                    .Where(x => !string.IsNullOrEmpty(x.Name) &&
                    x.Name.ToLower().StartsWith(pref.ToLower())).ToList();
                if (couponsWithPrefix.Any())
                {
                    var discountIds = couponsWithPrefix.Select(x => x.Id).ToList();
                    var existingList = customersByPrefixes.Where(x => x.Prefix == pref).FirstOrDefault();
                    if (existingList != null)
                        existingList.DiscountIds.AddRange(discountIds);
                    else
                        customersByPrefixes.Add(new CustomerIdssOfCouponPrefix
                        {
                            Prefix = pref,
                            DiscountIds = discountIds
                        });
                }
            }

            if (customersByPrefixes.Any())
            {
                using (var stream = new MemoryStream())
                {
                    using (var xlPackage = new ExcelPackage(stream))
                    {
                        var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                        int count = 1;
                        foreach (var customersByPrefix in customersByPrefixes)
                        {

                            var couponIds = customersByPrefix.DiscountIds;
                            var discountUsage = _discountService.GetAllDiscountUsageHistory()
                                .Where(x => couponIds.Contains(x.DiscountId))
                                .ToList();
                            var orderIds = discountUsage.Select(x => x.OrderId).ToList();
                            var ordersWithCoupon = _orderService.GetAllOrdersQuery()
                                .Where(x => orderIds.Contains(x.Id))
                                .ToList();

                            int row = 1;
                            worksheet.Cells[row, count].Value = customersByPrefix.Prefix;
                            row++;
                            foreach (var orderWithCoupon in ordersWithCoupon)
                            {
                                worksheet.Cells[row, count].Value = orderWithCoupon.CustomerId;
                                row++;
                            }
                            count++;
                        }

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }

                        xlPackage.Save();
                    }

                    return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ids_de_clientes_por_prefijo_de_cupones.xlsx");
                }
            }
            return Ok();
        }

        // Cantidad de pedidos mensuales por cliente
        [HttpGet]
        public IActionResult GenerateExcel108()
        {
            var ordersControlDate = DateTime.Now.AddMonths(-24);
            var allOrders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= ordersControlDate)
                .ToList();
            var groupedByClient = allOrders.GroupBy(x => x.CustomerId).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Id Usuario";
                    worksheet.Cells[row, 2].Value = "Cantidad total de pedidos";

                    for (int i = 1; i <= 24; i++)
                        worksheet.Cells[1, i + 2].Value = "Mes " + i;

                    foreach (var group in groupedByClient)
                    {
                        row++;
                        var pedidos = OrderUtils.GetPedidosGroupByList(group.Select(x => x).ToList()).ToList();
                        worksheet.Cells[row, 1].Value = group.Key;
                        worksheet.Cells[row, 2].Value = pedidos.Count();
                        DateTime controlDate = group.OrderBy(x => x.SelectedShippingDate)
                            .Select(x => x.SelectedShippingDate)
                            .FirstOrDefault()
                            .Value;

                        for (int i = 0; i < 24; i++)
                        {
                            var date = controlDate.AddMonths(i);
                            var filteredPedidosCount = pedidos.Where(x => x.Key.SelectedShippingDate.Month == date.Month && x.Key.SelectedShippingDate.Year == date.Year)
                                .Count();
                            worksheet.Cells[row, i + 3].Value = filteredPedidosCount;
                        }

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cantidad_pedidos_mensuales_por_cliente.xlsx");
            }
        }

        // Monto de pedidos mensuales por cliente
        [HttpGet]
        public IActionResult GenerateExcel109()
        {
            var ordersControlDate = DateTime.Now.AddMonths(-24);
            var allOrders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate >= ordersControlDate).ToList();
            var groupedByClient = allOrders.GroupBy(x => x.CustomerId).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Id Usuario";
                    worksheet.Cells[row, 2].Value = "Fecha primer pedido";

                    for (int i = 1; i <= 24; i++)
                        worksheet.Cells[1, i + 2].Value = "Mes " + i;

                    foreach (var group in groupedByClient)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = group.Key;
                        worksheet.Cells[row, 2].Value = group.Select(x => x.SelectedShippingDate.Value).OrderBy(x => x).FirstOrDefault();
                        worksheet.Cells[row, 2].Value = "dd-mm-yyyy";
                        DateTime controlDate = group.OrderBy(x => x.SelectedShippingDate)
                            .Select(x => x.SelectedShippingDate)
                            .FirstOrDefault()
                            .Value;

                        for (int i = 0; i < 24; i++)
                        {
                            var date = controlDate.AddMonths(i);
                            var monthAmount = group.Where(x => x.SelectedShippingDate.Value.Month == date.Month && x.SelectedShippingDate.Value.Year == date.Year)
                                .Select(x => x.OrderTotal)
                                .DefaultIfEmpty().Sum();
                            worksheet.Cells[row, i + 3].Value = monthAmount;
                        }

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cantidad_pedidos_mensuales_por_cliente.xlsx");
            }
        }

        // Clientes que no han pedido desde hace X días
        [HttpGet]
        public IActionResult GenerateExcel110(int days = 60)
        {
            var controlDate = DateTime.Now.AddDays(days * -1);
            var allOrders = OrderUtils.GetFilteredOrders(_orderService).ToList();
            var orders60Days = allOrders.Where(x => x.SelectedShippingDate <= controlDate).ToList();
            var customerIdsAfter60Days = allOrders.Where(x => x.SelectedShippingDate > controlDate).Select(x => x.CustomerId).Distinct().ToList();
            var customers60Days = orders60Days.Where(x => !customerIdsAfter60Days.Contains(x.CustomerId))
                .GroupBy(x => x.Customer)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Id";
                    worksheet.Cells[row, 2].Value = "Nombre";
                    worksheet.Cells[row, 3].Value = "Apellido";
                    worksheet.Cells[row, 4].Value = "Correo electrónico";
                    worksheet.Cells[row, 5].Value = "Teléfono";
                    worksheet.Cells[row, 6].Value = "Número de pedidos";
                    worksheet.Cells[row, 7].Value = "Total vendido";
                    worksheet.Cells[row, 8].Value = "Fecha de primera compra";
                    worksheet.Cells[row, 9].Value = "Fecha de última compra";
                    worksheet.Cells[row, 10].Value = "¿Está activo?";

                    foreach (var customerGroup in customers60Days)
                    {
                        row++;
                        var customer = customerGroup.Key;
                        worksheet.Cells[row, 1].Value = customer.Id;
                        worksheet.Cells[row, 2].Value = customerGroup.Select(x => x.ShippingAddress.FirstName).Where(x => !string.IsNullOrWhiteSpace(x)).FirstOrDefault();
                        worksheet.Cells[row, 3].Value = customerGroup.Select(x => x.ShippingAddress.LastName).Where(x => !string.IsNullOrWhiteSpace(x)).FirstOrDefault();
                        worksheet.Cells[row, 4].Value = customer.Email;
                        worksheet.Cells[row, 5].Value = customerGroup.Select(x => x.ShippingAddress.PhoneNumber).Where(x => !string.IsNullOrWhiteSpace(x)).FirstOrDefault();
                        worksheet.Cells[row, 6].Value = OrderUtils.GetPedidosGroupByList(customerGroup.Select(x => x).ToList()).Count();
                        worksheet.Cells[row, 7].Value = customerGroup.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 8].Value = customerGroup.Select(x => x.SelectedShippingDate.Value).OrderBy(x => x).FirstOrDefault();
                        worksheet.Cells[row, 8].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 9].Value = customerGroup.Select(x => x.SelectedShippingDate.Value).OrderBy(x => x).LastOrDefault();
                        worksheet.Cells[row, 9].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 10].Value = customer.Active ? "SI" : "NO";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_sin_pedido_desde_{days}_dias.xlsx");
            }
        }

        // Matriz de productos comprados
        [HttpGet]
        public IActionResult GenerateExcel111(int days)
        {
            var controlIds = new List<int>() { 2596, 9746, 9410, 9490, 9749, 9677, 2604, 9448, 9460, 9747 };
            var controlDate = DateTime.Now.AddDays(days * -1);
            var allOrders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate.Value >= controlDate).ToList();
            var productIds = allOrders.SelectMany(x => x.OrderItems)
                .Where(x => x.Product.Published)
                .Select(x => x.ProductId)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Id de producto";
                    for (int i = 0; i < controlIds.Count; i++)
                    {
                        worksheet.Cells[row, i + 2].Value = controlIds[i];
                    }

                    foreach (var productId in productIds)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = productId;
                        for (int i = 0; i < controlIds.Count; i++)
                        {
                            var filteredOrders = allOrders
                                .Where(x => x.OrderItems.Where(y => y.ProductId == productId).Any() && x.OrderItems.Where(y => y.ProductId == controlIds[i]).Any())
                                .ToList();
                            var pedidos = OrderUtils.GetPedidosGroupByList(filteredOrders);
                            worksheet.Cells[row, i + 2].Value = pedidos.Count();
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_matriz_productos_vendidos_{days}_dias.xlsx");
            }
        }

        // productos mas vendidos para actualización de precio
        [HttpGet]
        public IActionResult GenerateExcel112(int count, int days)
        {
            var controlDate = DateTime.Now.AddDays(-1 * days).Date;

            var orderItemsGroup = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate)
                .SelectMany(x => x.OrderItems)
                .GroupBy(x => x.Product)
                .ToList()
                .OrderByDescending(x => x.Select(y => _paymentService.CalculateGroceryPrice(y.Product, null, y.Quantity, false)).DefaultIfEmpty().Sum())
                .Take(count)
                .ToList();
            var productIds = orderItemsGroup.Select(x => x.Key.Id).ToList();
            var reportStatus = _orderReportStatusService.GetAll()
                .Where(x => x.StatusTypeId == 2 && x.ShippingDate >= controlDate)
                .Select(x => DbFunctions.AddMilliseconds(x.ShippingDate, x.BuyerId))
                .Distinct()
                .ToList();
            var reports = _orderReportService.GetAll()
                .Select(x => new { x.OrderShippingDate, x.ProductId, x.ReportedByCustomerId, x.UpdatedUnitCost })
                .Where(x => x.OrderShippingDate >= controlDate && productIds.Contains(x.ProductId))
                .ToList()
                .Where(x => reportStatus.Contains(x.OrderShippingDate.AddMilliseconds(x.ReportedByCustomerId)))
                .ToList();

            var mainManufacturers = _productMainManufacturerService.GetAll().Where(x => productIds.Contains(x.ProductId)).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var categoriesCount = orderItemsGroup.Count;
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Producto";
                    worksheet.Cells[row, 2].Value = $"Simulación de ventas de últimos {days} días con precios actuales";
                    worksheet.Cells[row, 3].Value = "Costo según plataforma";
                    worksheet.Cells[row, 4].Value = "Margen con costo según plataforma";
                    worksheet.Cells[row, 5].Value = "Costo según último reporte de compradores (liquidado)";
                    worksheet.Cells[row, 6].Value = "Margen con costo según último reporte de compradores (liquidado)";
                    worksheet.Cells[row, 7].Value = "Ponderación";
                    worksheet.Cells[row, 8].Value = "Contribución de margen con costo según plataforma";
                    worksheet.Cells[row, 9].Value = "Contribución de margen con costo según último reporte de compradores (liquidado)";
                    worksheet.Cells[row, 10].Value = "Costo actual en plataforma";
                    worksheet.Cells[row, 11].Value = "Fabricante";
                    worksheet.Cells[row, 12].Value = "Precio actual en plataforma";

                    foreach (var productGroup in orderItemsGroup)
                    {
                        row++;
                        var product = productGroup.Key;
                        var filteredItems = productGroup.Select(x => x).ToList();
                        var sellPrices = new List<decimal>();
                        var currentCosts = new List<decimal>();
                        var lastReportedCosts = new List<decimal>();
                        foreach (var item in filteredItems)
                        {
                            var price = _paymentService.CalculateGroceryPrice(product, null, item.Quantity, false);
                            var lastReportedUnitCost = reports.Where(x => x.ProductId == item.ProductId)
                                .OrderByDescending(x => x.OrderShippingDate)
                                .Select(x => x.UpdatedUnitCost)
                                .FirstOrDefault();
                            if (price == 0)
                                price = item.Quantity * product.Price;
                            sellPrices.Add(price);

                            currentCosts.Add(CalculateGroceryCost(product, item.Quantity, product.ProductCost));
                            lastReportedCosts.Add(CalculateGroceryCost(product, item.Quantity, lastReportedUnitCost));
                        }

                        worksheet.Cells[row, 1].Value = product.Name;
                        worksheet.Cells[row, 2].Value = sellPrices.DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        worksheet.Cells[row, 3].Value = currentCosts.DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        worksheet.Cells[row, 4].Formula = $"=IF(B{row},1-(C{row}/$B{row}),0)";
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "0%";
                        worksheet.Cells[row, 5].Value = lastReportedCosts.DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 5].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        worksheet.Cells[row, 6].Formula = $"=IF(B{row},1-(E{row}/$B{row}),0)";
                        worksheet.Cells[row, 6].Style.Numberformat.Format = "0%";
                        worksheet.Cells[row, 7].Formula = $"=IF(B${categoriesCount + 2},B{row}/B${categoriesCount + 2},0)";
                        worksheet.Cells[row, 7].Style.Numberformat.Format = "0.0000%";
                        worksheet.Cells[row, 8].Formula = $"=G{row}*D{row}";
                        worksheet.Cells[row, 8].Style.Numberformat.Format = "0.0000%";
                        worksheet.Cells[row, 9].Formula = $"=G{row}*F{row}";
                        worksheet.Cells[row, 9].Style.Numberformat.Format = "0.0000%";
                        worksheet.Cells[row, 10].Value = product.ProductCost;
                        worksheet.Cells[row, 10].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        worksheet.Cells[row, 11].Value = ProductUtils.GetMainManufacturer(product.ProductManufacturers, mainManufacturers.Where(x => x.ProductId == product.Id).FirstOrDefault())?.Name;
                        worksheet.Cells[row, 12].Value = product.Price;
                        worksheet.Cells[row, 12].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                    }

                    row++;
                    worksheet.Cells[row, 1].Value = "Total";
                    worksheet.Cells[row, 2].Formula = $"=SUM(B2:B{orderItemsGroup.Count() + 1})";
                    worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                    worksheet.Cells[row, 3].Formula = $"=SUM(C2:C{orderItemsGroup.Count() + 1})";
                    worksheet.Cells[row, 3].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                    worksheet.Cells[row, 5].Formula = $"=SUM(E2:E{orderItemsGroup.Count() + 1})";
                    worksheet.Cells[row, 5].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                    worksheet.Cells[row, 7].Formula = $"=SUM(G2:G{orderItemsGroup.Count() + 1})";
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "0.0000%";
                    worksheet.Cells[row, 8].Formula = $"=SUM(H2:H{orderItemsGroup.Count() + 1})";
                    worksheet.Cells[row, 8].Style.Numberformat.Format = "0.0000%";
                    worksheet.Cells[row, 9].Formula = $"=SUM(I2:I{orderItemsGroup.Count() + 1})";
                    worksheet.Cells[row, 9].Style.Numberformat.Format = "0.0000%";

                    worksheet.Cells[$"A{row}:{ExcelCellAddress.GetColumnLetter(worksheet.Dimension.End.Column)}{row}"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[$"A{row}:{ExcelCellAddress.GetColumnLetter(worksheet.Dimension.End.Column)}{row}"].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    worksheet.Cells[$"A{row}:{ExcelCellAddress.GetColumnLetter(worksheet.Dimension.End.Column)}{row}"].Style.Font.Bold = true;

                    row++;
                    row++;

                    DateTime now = DateTime.Now;
                    string dateString = now.ToString("dd-MM-yyyy");
                    string timeString = now.ToString("hh:mm:ss tt");

                    worksheet.Cells[row, 1].Value = "Fecha de consulta de reporte";
                    worksheet.Cells[row, 2].Value = dateString;

                    row++;
                    worksheet.Cells[row, 1].Value = "Hora de consulta de reporte";
                    worksheet.Cells[row, 2].Value = timeString;

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"ReporteMargenBrutoPromedioPonderado_{count}_masvendidos_{days}dias_{DateTime.Now:dd-MM-yyyy}_{DateTime.Now:HHmmss}.xlsx");
            }
        }

        // Matriz simple de productos comprados
        [HttpGet]
        public IActionResult GenerateExcel113(int days)
        {
            var controlDate = DateTime.Now.AddDays(days * -1);
            var allOrders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate.Value >= controlDate).ToList();
            var allPedidos = OrderUtils.GetPedidosGroupByList(allOrders).ToList();
            var productIds = allOrders.SelectMany(x => x.OrderItems)
                .Where(x => x.Product.Published)
                .Select(x => x.ProductId)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Id de producto";
                    for (int i = 0; i < productIds.Count; i++)
                    {
                        worksheet.Cells[row, i + 2].Value = productIds[i];
                    }

                    foreach (var productId in productIds)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = productId;
                    }

                    worksheet = xlPackage.Workbook.Worksheets.Add("PRODUCTOS POR PEDIDO");
                    row = 1;
                    worksheet.Cells[row, 1].Value = "Id de producto";
                    foreach (var productId in productIds)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = productId;
                    }

                    int column = 2;
                    foreach (var pedido in allPedidos)
                    {
                        column++;
                        var pedidoProductIds = pedido.SelectMany(x => x.OrderItems).Select(x => x.ProductId).Distinct().ToList();
                        row = 1;

                        worksheet.Cells[row, column].Value = string.Join(", ", pedido.Select(x => "#" + x.Id));
                        foreach (var productId in productIds)
                        {
                            row++;
                            worksheet.Cells[row, column].Value = pedidoProductIds.Contains(productId) ? 1 : 0;
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_matriz_productos_vendidos_{days}_dias.xlsx");
            }
        }

        // Compras por categoria hijo
        [HttpGet]
        public IActionResult GenerateExcel114(int days)
        {
            var controlDate = DateTime.Now.AddDays(days * -1);
            var childCategories = _categoryService.GetAllCategories()
                .Where(x => x.ParentCategoryId > 0 && !x.Deleted)
                .ToList();
            var childCategoryIds = childCategories.Select(x => x.Id)
                .Distinct()
                .ToList();
            var products = _productService.GetAllProductsQuery()
                .Where(x => x.Published && childCategoryIds.Intersect(x.ProductCategories.Select(y => y.CategoryId).ToList()).Any())
                .ToList();
            var productIds = products.Select(x => x.Id).Distinct().ToList();
            var orderItems = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate.Value >= controlDate)
                .SelectMany(x => x.OrderItems)
                .Where(x => productIds.Contains(x.ProductId))
                .ToList();


            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Categoría hijo";
                    worksheet.Cells[row, 2].Value = "Cantidad de productos";
                    worksheet.Cells[row, 3].Value = "Monto vendido";

                    foreach (var childCategory in childCategories)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = childCategory.Name;
                        worksheet.Cells[row, 2].Value = products
                            .Where(x => x.ProductCategories.Select(y => y.CategoryId).Contains(childCategory.Id))
                            .Count();
                        worksheet.Cells[row, 3].Value = orderItems
                            .Where(x => x.Product.ProductCategories.Select(y => y.CategoryId).Contains(childCategory.Id))
                            .Select(x => x.PriceInclTax)
                            .DefaultIfEmpty()
                            .Sum();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_venta_cantidad_categoriahijo_{days}_dias.xlsx");
            }
        }

        // Ordenes de clientes usando cupones con prefijo
        [HttpGet]
        public IActionResult GenerateExcel115(string prefix = "PE01")
        {
            var discounts = _discountService.GetAllDiscounts(showHidden: true)
                .Where(x => x.Name.ToLower().StartsWith(prefix.ToLower()) ||
                (!string.IsNullOrEmpty(x.CouponCode) &&
                x.CouponCode.ToLower().StartsWith(prefix.ToLower())))
                .ToList();
            var discountIds = discounts.Select(x => x.Id).ToList();
            var discountUsage = _discountService.GetAllDiscountUsageHistory()
                .Where(x => discountIds.Contains(x.DiscountId))
                .ToList();
            var orderIds = discountUsage.Select(x => x.OrderId).ToList();
            var ordersOfDiscount = _orderService.GetAllOrdersQuery()
                .Where(x => orderIds.Contains(x.Id))
                .ToList();
            var customerIdsOfOrders = ordersOfDiscount.Select(x => x.CustomerId).ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => customerIdsOfOrders.Contains(x.Id))
                .ToList();
            var ordersGroupingByCustomer = GetFilteredOrders()
                .Where(x => customerIdsOfOrders.Contains(x.CustomerId))
                .GroupBy(x => x.CustomerId).ToList();
            var test = _orderService.GetAllOrdersQuery()
                .Where(x => customerIdsOfOrders.Contains(x.CustomerId))
                .GroupBy(x => x.CustomerId).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Email del clinte";
                    worksheet.Cells[row, 2].Value = "Número de pedidos";
                    worksheet.Cells[row, 3].Value = "Monto de las órdenes";


                    foreach (var grouping in ordersGroupingByCustomer)
                    {
                        var customerOfGrouping = customers.Where(x => x.Id == grouping.Key).FirstOrDefault();
                        row++;
                        worksheet.Cells[row, 1].Value = customerOfGrouping.Email;
                        worksheet.Cells[row, 2].Value = OrderUtils.GetPedidosGroupByList(grouping.ToList()).Count();
                        worksheet.Cells[row, 3].Value = grouping.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ordenes_de_clientes_usando_cupones_con_prefijo_{prefix}.xlsx");
            }
        }

        // Detalle de uso de cupón
        [HttpGet]
        public IActionResult GenerateExcel116(string couponCode)
        {
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.DiscountUsageHistory.Where(y => y.Discount.CouponCode.ToLower() == couponCode.ToLower()).Any())
                .Select(x => new { x.Id, x.DiscountUsageHistory, x.OrderTotal, x.OrderSubTotalDiscountInclTax, x.OrderDiscount })
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "# orden";
                    worksheet.Cells[row, 2].Value = "Cupones usados";
                    worksheet.Cells[row, 3].Value = "Total de la orden";
                    worksheet.Cells[row, 4].Value = "Monto de descuentos";

                    row++;
                    foreach (var order in orders)
                    {
                        worksheet.Cells[row, 1].Value = order.Id;
                        worksheet.Cells[row, 2].Value = string.Join("; ", order.DiscountUsageHistory.Select(x => x.Discount.Name).ToList());
                        worksheet.Cells[row, 3].Value = order.OrderTotal;
                        worksheet.Cells[row, 4].Value = order.OrderSubTotalDiscountInclTax + order.OrderDiscount;
                        row++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_detalle_uso_cupon_{couponCode}.xlsx");
            }
        }

        // Compra mayor o igual a cantidad y entre fechas espeicificada
        [HttpGet]
        public IActionResult GenerateExcel117(decimal amount = 1200, string fromDate = "10-11-2021", string toDate = "16-11-2021", bool byPrimaryGrouping = false)
        {
            var parsedDateFrom = DateTime.ParseExact(fromDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;
            var parsedDateTo = DateTime.ParseExact(toDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;
            var orders = new List<Order>();
            var pedidos = OrderUtils.GetPedidosGroupByList(orders);
            if (!byPrimaryGrouping)
            {
                orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.OrderTotal >= amount && parsedDateFrom <= x.SelectedShippingDate && x.SelectedShippingDate <= parsedDateTo)
                .ToList();
                pedidos = OrderUtils.GetPedidosGroupByList(orders);
            }
            else
            {
                orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => parsedDateFrom <= x.SelectedShippingDate && x.SelectedShippingDate <= parsedDateTo)
                .ToList();
                pedidos = OrderUtils.GetPedidosGroupByList(orders).ToList();
                pedidos = pedidos.Where(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum() >= amount).ToList();
            }
            var customerIds = orders.Select(x => x.CustomerId).ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => customerIds.Contains(x.Id)).ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Nombre completo";
                    worksheet.Cells[row, 2].Value = "Correo";
                    worksheet.Cells[row, 3].Value = "Teléfono";
                    worksheet.Cells[row, 4].Value = "# de ordenes";
                    worksheet.Cells[row, 5].Value = "Totales de ordenes";
                    row++;

                    foreach (var pedido in pedidos)
                    {
                        var customer = customers.Where(x => x.Id == pedido.Key.CustomerId).FirstOrDefault();
                        var phone = customer.Addresses.Where(x => !string.IsNullOrEmpty(x.PhoneNumber)).FirstOrDefault()?.PhoneNumber;
                        phone = string.IsNullOrEmpty(phone) ? customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone) : phone;
                        worksheet.Cells[row, 1].Value = customer.GetFullName();
                        worksheet.Cells[row, 2].Value = customer.Email;
                        worksheet.Cells[row, 3].Value = phone;
                        worksheet.Cells[row, 4].Value = string.Join(", ", pedido.Select(x => "#" + x.Id));
                        worksheet.Cells[row, 5].Value = pedido.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                        row++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ordenes_con_total_mayor_a_monto_entre_el_{fromDate}_a_{toDate}.xlsx");
            }
        }


        // Compra mayor o igual a cantidad y entre fechas espeicificada
        [HttpGet]
        public IActionResult GenerateExcel118(decimal amount = 5000, int days = 30)
        {
            var controlDate = DateTime.Today.AddDays(-1 * days);
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => controlDate <= x.SelectedShippingDate)
                .ToList();
            var pedidos = OrderUtils.GetPedidosGroupByList(orders).ToList()
                .Where(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum() >= amount)
                .ToList();
            var customerIds = orders.Select(x => x.CustomerId).ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => customerIds.Contains(x.Id)).ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Nombre completo";
                    worksheet.Cells[row, 2].Value = "Correo";
                    worksheet.Cells[row, 3].Value = "Teléfono";
                    worksheet.Cells[row, 4].Value = "# de ordenes";
                    worksheet.Cells[row, 5].Value = "Totales de ordenes";
                    row++;

                    foreach (var pedido in pedidos)
                    {
                        var customer = customers.Where(x => x.Id == pedido.Key.CustomerId).FirstOrDefault();
                        worksheet.Cells[row, 1].Value = customer.GetFullName();
                        worksheet.Cells[row, 2].Value = customer.Email;
                        worksheet.Cells[row, 5].Value = pedido.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                        row++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_monto_mayor_al_especificado_desde_el_{controlDate:dd/MM/yyyy}.xlsx");
            }
        }

        // Datos de entregas y pagos
        [HttpGet]
        public IActionResult GenerateExcel119(int days = 30)
        {
            var controlDate = DateTime.Today.AddDays(-1 * days);
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate && (x.PaymentMethodSystemName == "Payments.CardOnDelivery" || x.PaymentMethodSystemName == "Payments.CashOnDelivery"))
                .ToList();
            var routes = _shippingRouteService.GetAll().ToList();
            var vehicleRoutes = _shippingVehicleRouteService.GetAll().Where(x => x.ShippingDate >= controlDate).ToList();
            var allPedidos = OrderUtils.GetPedidosGroupByList(orders).ToList();

            var delivered = _shippingRouteUserService.GetAll().Where(x => x.ResponsableDateUtc >= controlDate).ToList();
            var customerIds = delivered.Select(x => x.UserInChargeId).ToList();
            var customers = _customerService.GetAllCustomersQuery().Where(x => customerIds.Contains(x.Id)).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Fecha";
                    worksheet.Cells[row, 2].Value = "Ruta";
                    worksheet.Cells[row, 3].Value = "Franquicia";
                    worksheet.Cells[row, 4].Value = "Número de orden";
                    worksheet.Cells[row, 5].Value = "Monto del pedido";
                    worksheet.Cells[row, 6].Value = "Método de pago original";
                    worksheet.Cells[row, 7].Value = "Método de pago final";
                    worksheet.Cells[row, 8].Value = "Repartidores";
                    row++;

                    foreach (var pedido in allPedidos)
                    {
                        var routeId = pedido.Select(y => y.RouteId).FirstOrDefault();
                        var routeUsers = delivered.Where(x => x.ResponsableDateUtc == pedido.Key.SelectedShippingDate && x.ShippingRouteId == routeId).ToList();
                        if (!routeUsers.Where(x => x.UserInChargeId == 18005236 || x.UserInChargeId == 18233599).Any()) continue;
                        var routeName = routes.Where(x => x.Id == routeId)
                            .Select(x => x.RouteName)
                            .FirstOrDefault();
                        var vehicleRoute = vehicleRoutes.Where(x => x.ShippingDate == pedido.Key.SelectedShippingDate && x.RouteId == routeId).FirstOrDefault();
                        if (vehicleRoute == null) continue;
                        worksheet.Cells[row, 1].Value = pedido.Key.SelectedShippingDate;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 2].Value = routeName;
                        worksheet.Cells[row, 3].Value = vehicleRoute.Vehicle?.Franchise?.Name;
                        worksheet.Cells[row, 4].Value = string.Join(", ", pedido.Select(x => x.Id).ToList());
                        worksheet.Cells[row, 5].Value = pedido.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();

                        var paymentMehotd = pedido.Select(x => x.PaymentMethodSystemName).FirstOrDefault();
                        var note = pedido.SelectMany(x => x.OrderNotes.OrderBy(y => y.CreatedOnUtc).Select(y => y.Note))
                            .Where(x => x.Contains("cambió el método de pago"))
                            .FirstOrDefault();

                        if (!string.IsNullOrWhiteSpace(note))
                        {
                            var index = note.IndexOf("cambió el método de pago");
                            var lenght = note.Length;
                            var result = note.Substring(index + 28);
                            var index2 = result.IndexOf(" a ");
                            var result2 = result.Substring(0, index2);
                            worksheet.Cells[row, 6].Value = result2;
                        }
                        else
                        {
                            worksheet.Cells[row, 6].Value = paymentMehotd;
                        }

                        worksheet.Cells[row, 7].Value = paymentMehotd;

                        var routeUserIds = routeUsers.Select(x => x.UserInChargeId).ToList();
                        var names = string.Join(", ", customers.Where(x => routeUserIds.Contains(x.Id)).Select(x => x.GetFullName()));
                        worksheet.Cells[row, 8].Value = names;

                        row++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_entregas_pagos_{days}_dias.xlsx");
            }
        }

        // Clientes activos
        [HttpGet]
        public IActionResult GenerateExcel120(int days = 30)
        {
            var controlDate = DateTime.Today.AddDays(-1 * days);
            var customerIds = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => controlDate <= x.SelectedShippingDate)
                .Select(x => x.CustomerId)
                .Distinct()
                .ToList();
            var customersGroup = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => customerIds.Contains(x.CustomerId))
                .Select(x => new { x.CustomerId, x.OrderTotal, x.SelectedShippingDate })
                .GroupBy(x => x.CustomerId)
                .ToList();
            var newsletter = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions();
            var customers = _customerService.GetAllCustomersQuery().Where(x => customerIds.Contains(x.Id)).ToList();
            var zones = _shippingZoneService.GetAll().ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Nombre";
                    worksheet.Cells[row, 2].Value = "Telefono";
                    worksheet.Cells[row, 3].Value = "Monto de venta histórico";
                    worksheet.Cells[row, 4].Value = "Fecha de última orden";
                    worksheet.Cells[row, 5].Value = "Suscrito a news?";
                    worksheet.Cells[row, 6].Value = "Código postal";
                    worksheet.Cells[row, 7].Value = "Zona";
                    worksheet.Cells[row, 8].Value = "Correo";
                    worksheet.Cells[row, 9].Value = "Cantidad de ordenes histórico";
                    row++;

                    foreach (var customer in customersGroup)
                    {
                        var currentCustomer = customers.Where(x => x.Id == customer.Key).FirstOrDefault();
                        worksheet.Cells[row, 1].Value = currentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                        var phone = currentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                        worksheet.Cells[row, 2].Value = string.IsNullOrEmpty(phone) ? currentCustomer.Addresses.Select(x => x.PhoneNumber).FirstOrDefault() : phone;
                        worksheet.Cells[row, 3].Value = customer.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 4].Value = customer.Select(x => x.SelectedShippingDate).OrderByDescending(x => x).FirstOrDefault();
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 5].Value = newsletter.Where(x => x.Email == currentCustomer.Email).Select(x => x.Active).FirstOrDefault() ? "SI" : "NO";
                        var postalCode = currentCustomer.Addresses.Select(x => x.ZipPostalCode).FirstOrDefault()?.Trim();
                        var zone = zones.Where(x => (x.PostalCodes + "," + x.AdditionalPostalCodes).Split(',').Select(y => y.Trim()).Contains(postalCode)).FirstOrDefault();
                        worksheet.Cells[row, 6].Value = postalCode;
                        worksheet.Cells[row, 7].Value = zone == null ? "S/I" : zone.ZoneName;
                        worksheet.Cells[row, 8].Value = currentCustomer.Email;
                        worksheet.Cells[row, 9].Value = customer.Count();
                        row++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_activos_{days}_dias.xlsx");
            }
        }

        // Clientes no activos
        [HttpGet]
        public IActionResult GenerateExcel121(int days = 30)
        {
            var controlDate = DateTime.Today.AddDays(-1 * days);
            var customerIds = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => controlDate <= x.SelectedShippingDate)
                .Select(x => x.CustomerId)
                .Distinct()
                .ToList();
            var customersGroup = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => !customerIds.Contains(x.CustomerId))
                .Select(x => new { x.CustomerId, x.OrderTotal, x.SelectedShippingDate })
                .ToList()
                .GroupBy(x => x.CustomerId)
                .ToList();
            customerIds = customersGroup.Select(x => x.Key).ToList();
            var newsletter = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions();
            var customers = _customerService.GetAllCustomersQuery().Where(x => customerIds.Contains(x.Id)).ToList();
            var zones = _shippingZoneService.GetAll().ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Nombre";
                    worksheet.Cells[row, 2].Value = "Telefono";
                    worksheet.Cells[row, 3].Value = "Monto de venta histórico";
                    worksheet.Cells[row, 4].Value = "Fecha de última orden";
                    worksheet.Cells[row, 5].Value = "Suscrito a news?";
                    worksheet.Cells[row, 6].Value = "Código postal";
                    worksheet.Cells[row, 7].Value = "Zona";
                    worksheet.Cells[row, 8].Value = "Correo";
                    row++;

                    foreach (var customer in customersGroup)
                    {
                        var currentCustomer = customers.Where(x => x.Id == customer.Key).FirstOrDefault();
                        worksheet.Cells[row, 1].Value = currentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                        var phone = currentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                        worksheet.Cells[row, 2].Value = string.IsNullOrEmpty(phone) ? currentCustomer.Addresses.Select(x => x.PhoneNumber).FirstOrDefault() : phone;
                        worksheet.Cells[row, 3].Value = customer.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 4].Value = customer.Select(x => x.SelectedShippingDate).OrderByDescending(x => x).FirstOrDefault();
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 5].Value = newsletter.Where(x => x.Email == currentCustomer.Email).Select(x => x.Active).FirstOrDefault() ? "SI" : "NO";
                        var postalCode = currentCustomer.Addresses.Select(x => x.ZipPostalCode).FirstOrDefault()?.Trim();
                        var zone = zones.Where(x => (x.PostalCodes + "," + x.AdditionalPostalCodes).Split(',').Select(y => y.Trim()).Contains(postalCode)).FirstOrDefault();
                        worksheet.Cells[row, 6].Value = postalCode;
                        worksheet.Cells[row, 7].Value = zone == null ? "S/I" : zone.ZoneName;
                        worksheet.Cells[row, 8].Value = currentCustomer.Email;
                        row++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_no_activos_{days}_dias.xlsx");
            }
        }

        // Lista de todos los productos publicados
        [HttpGet]
        public IActionResult GenerateExcel122()
        {
            var products = _productService.GetAllProductsQuery()
                .Where(x => !x.Deleted && x.Published)
                .OrderBy(x => x.Name)
                .ToList();
            var allOrderItems = OrderUtils.GetFilteredOrders(_orderService)
                .SelectMany(x => x.OrderItems)
                .Select(x => new { x.ProductId, x.Quantity, x.PriceInclTax })
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Nombre";
                    worksheet.Cells[row, 2].Value = "Costo";
                    worksheet.Cells[row, 3].Value = "Precio";
                    worksheet.Cells[row, 4].Value = "Margen";
                    worksheet.Cells[row, 5].Value = "Monto vendido histórico";
                    worksheet.Cells[row, 6].Value = "Candidad vendida histórica";
                    worksheet.Cells[row, 7].Value = "Unidad";

                    foreach (var product in products)
                    {
                        var filteredItems = allOrderItems.Where(x => x.ProductId == product.Id).ToList();
                        var qty = GetQty(product, filteredItems.Select(x => x.Quantity).DefaultIfEmpty().Sum());
                        row++;

                        worksheet.Cells[row, 1].Value = product.Name;
                        worksheet.Cells[row, 2].Value = product.ProductCost;
                        worksheet.Cells[row, 3].Value = product.Price;
                        worksheet.Cells[row, 4].Value = product.Price > 0 ? 1 - (product.ProductCost / product.Price) : 0;
                        worksheet.Cells[row, 5].Value = filteredItems.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 6].Value = qty.Item1;
                        worksheet.Cells[row, 7].Value = qty.Item2;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_productos_publicados_con_venta.xlsx");
            }
        }

        // Monto comprado a fabricante mensual
        [HttpGet]
        public IActionResult GenerateExcel123(int months = 6, int fbId = 0)
        {
            var today = DateTime.Now.Date;
            var controlDate = DateTime.Now.AddMonths(months * -1);
            var controlDate2 = new DateTime(controlDate.Year, controlDate.Month, 1);

            var orderReports = _orderReportService.GetAll().Where(x => x.ManufacturerId == fbId && x.OrderShippingDate >= controlDate2 && x.NotBuyedReasonId == null).ToList();
            var grouped = orderReports
                .GroupBy(x => new DateTime(x.OrderShippingDate.Year, x.OrderShippingDate.Month, 1))
                .ToList();
            var manufacturer = _manufacturerService.GetManufacturerById(fbId);

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Mes";
                    worksheet.Cells[row, 2].Value = "Compra total a proveedor";

                    foreach (var group in grouped)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = group.Key;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "mm yyyy";
                        var amount = 0m;
                        var groupedByDate = group.Select(x => x)
                            .GroupBy(x => x.OrderShippingDate);
                        foreach (var dateGroup in groupedByDate)
                        {
                            amount += dateGroup.Select(x => x).GroupBy(x => x.ProductId).Select(x => x.FirstOrDefault())
                                .Where(x => x.UpdatedRequestedQtyCost != null)
                                .Select(x => x.UpdatedRequestedQtyCost.Value)
                                .DefaultIfEmpty()
                                .Sum();
                        }
                        worksheet.Cells[row, 2].Value = amount;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_compras_mensuales_{months}_meses_{manufacturer?.Name}.xlsx");
            }
        }

        // Descuentos no utilizados y vigentes
        [HttpGet]
        public IActionResult GenerateExcel124()
        {
            var today = DateTime.Now.Date;
            var discountUsageIds = _discountService.GetAllDiscountUsageHistory()
                .Select(x => x.DiscountId).Distinct().ToList();
            var discounts = _discountService.GetAllDiscounts()
                .Where(x => !discountUsageIds.Contains(x.Id)).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Nombre";
                    worksheet.Cells[row, 2].Value = "Cupón";
                    worksheet.Cells[row, 3].Value = "Tipo de descuento";
                    worksheet.Cells[row, 4].Value = "Tipo de valor";
                    worksheet.Cells[row, 5].Value = "Valor";
                    worksheet.Cells[row, 6].Value = "Fecha inicial";
                    worksheet.Cells[row, 7].Value = "Fecha final";

                    foreach (var discount in discounts)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = discount.Name;
                        worksheet.Cells[row, 2].Value = discount.CouponCode;
                        worksheet.Cells[row, 3].Value = discount.DiscountType.GetDisplayName();
                        worksheet.Cells[row, 4].Value = discount.UsePercentage ? "Porcentage" : "Cantidad";
                        worksheet.Cells[row, 5].Value = discount.UsePercentage ? discount.DiscountPercentage : discount.DiscountAmount;
                        worksheet.Cells[row, 6].Value = discount.StartDateUtc != null ? discount.StartDateUtc.Value.ToLocalTime() : DateTime.MinValue;
                        worksheet.Cells[row, 6].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 7].Value = discount.EndDateUtc != null ? discount.EndDateUtc.Value.ToLocalTime() : DateTime.MinValue;
                        worksheet.Cells[row, 7].Style.Numberformat.Format = "dd-mm-yyyy";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_descuentos_activos_sin_uso.xlsx");
            }
        }

        // Descuentos no utilizados y vigentes
        [HttpGet]
        public IActionResult GenerateExcel125(int manufacturerId, int months = 6)
        {
            var someMonthsAgo = DateTime.Now.AddMonths(-1 * months).Date;
            var someDaysAgoUtc = DateTime.UtcNow.AddDays(-1 * months).Date;
            var manufacturer = _manufacturerService.GetAllManufacturers()
                .Where(x => manufacturerId == x.Id).FirstOrDefault();
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => someMonthsAgo <= x.SelectedShippingDate.Value)
                .Select(x => new GenerateExcel125Model()
                {
                    ShippingAddress = x.ShippingAddress.Address1,
                    CustomerId = x.CustomerId,
                    SelectedShippingDate = x.SelectedShippingDate,
                    SelectedShippingTime = x.SelectedShippingTime,
                    Id = x.Id,
                    OrderItems = x.OrderItems.Select(y => new GenerateExcel125OrderitemModel()
                    {
                        Id = y.Id,
                        PriceInclTax = y.PriceInclTax
                    }).ToList()
                }).ToList();
            var productIds = new List<int>();
            var ids = _manufacturerService.GetProductManufacturersByManufacturerId(manufacturer.Id)
                .Select(x => x.ProductId).ToList();
            if (ids.Any())
                productIds.AddRange(ids);

            var orderIds = orders.Select(x => x.Id).ToList();
            var Ids = _orderService.GetAllOrderItemsQuery()
                .Where(x => productIds.Contains(x.ProductId) && orderIds.Contains(x.OrderId))
                .Select(x => new { OrderId = x.OrderId, Id = x.Id })
                .ToList();
            var orderItemIds = Ids.Select(x => x.Id).ToList();
            var notDelivered = _notDeliveredOrderItemService.GetAll()
                .Where(x => !x.Deleted && orderIds.Contains(x.OrderId) && productIds.Contains(x.ProductId))
                .ToList();
            orderIds = Ids.Select(x => x.OrderId).Distinct().ToList();
            orderIds.AddRange(notDelivered.Select(x => x.OrderId).ToList());
            orderIds = orderIds.Distinct().ToList();
            orders = orders.Where(x => orderIds.Contains(x.Id)).ToList();

            var pedidos = OrderUtils.GetSimplePedidosGroupByList<GenerateExcel125Model>(orders).ToList();
            var groupedPedidos = pedidos
                .GroupBy(x => new DateTime(x.Key.SelectedShippingDate.Value.Year, x.Key.SelectedShippingDate.Value.Month, 1))
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Mes";
                    worksheet.Cells[row, 2].Value = "Monto de venta de productos entregados";
                    worksheet.Cells[row, 3].Value = "Monto de productos no entregado";
                    worksheet.Cells[row, 4].Value = "Cantidad de pedidos entregados";
                    worksheet.Cells[row, 5].Value = "Cantidad de pedidos no entregados";

                    foreach (var pedidosGroup in groupedPedidos)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = pedidosGroup.Key;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "MMMM, yyyy";

                        worksheet.Cells[row, 2].Value = pedidosGroup
                            .SelectMany(x => x.SelectMany(y => y.OrderItems))
                            .Where(x => orderItemIds.Contains(x.Id))
                            .Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();

                        var currentOrderIds = pedidosGroup.SelectMany(x => x.Select(y => y.Id)).ToList();
                        worksheet.Cells[row, 3].Value = notDelivered.Where(x => currentOrderIds.Contains(x.OrderId))
                            .Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();

                        worksheet.Cells[row, 4].Value = pedidosGroup.Count();

                        var notDeliveredOrderIds = notDelivered.Select(x => x.OrderId).ToList();
                        worksheet.Cells[row, 5].Value = pedidosGroup.Where(x => x.Select(y => y.Id).Intersect(notDeliveredOrderIds).Any()).Count();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_pedidos_no_entregados_{manufacturer.Name}.xlsx");
            }
        }

        // Productos comrpados a frabricante en varios dias
        [HttpGet]
        public IActionResult GenerateExcel126(int days, int fbId)
        {
            if (fbId < 1)
                return BadRequest();

            var controlDate = DateTime.Now.AddDays(days * -1).Date;

            var manufacturer = _manufacturerService.GetManufacturerById(fbId);
            if (manufacturer == null) return NotFound();

            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate);
            var orderIds = ordersQuery.Select(x => x.Id).ToList();
            var notDeliveredItems = _notDeliveredOrderItemService.GetAll().Where(x => orderIds.Contains(x.OrderId)).ToList();
            var orderItems = ordersQuery.ToList()
                .SelectMany(x => x.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService, notDeliveredItems, true, true))
                .ToList();

            var manufacturerProductIds = _manufacturerService.GetProductManufacturersByManufacturerId(fbId).Select(x => x.ProductId).ToList();
            orderItems = orderItems.Where(x => manufacturerProductIds.Contains(x.ProductId)).ToList();
            var itemIds = orderItems.Select(x => x.Id).ToList();
            var reports = _orderReportService.GetAll().Where(x => itemIds.Contains(x.OrderItemId)).ToList();
            var itemsGroupedByDate = orderItems.GroupBy(x => x.Order.SelectedShippingDate).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Fecha";
                    worksheet.Cells[row, 2].Value = "Producto";
                    worksheet.Cells[row, 3].Value = "Cantidad estimada";
                    worksheet.Cells[row, 4].Value = "Cantidad comprada";
                    worksheet.Cells[row, 5].Value = "Unidad";

                    foreach (var dateGroup in itemsGroupedByDate)
                    {
                        var groupedByProduct = dateGroup.GroupBy(x => x.Product).ToList();
                        foreach (var productGroup in groupedByProduct)
                        {
                            row++;
                            var qty = GetQty(productGroup.Key, productGroup.Select(x => x.Quantity).DefaultIfEmpty().Sum());
                            worksheet.Cells[row, 1].Value = dateGroup.Key.Value;
                            worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-MM-yyyy";
                            worksheet.Cells[row, 2].Value = productGroup.Key.Name;
                            worksheet.Cells[row, 3].Value = qty.Item1;
                            worksheet.Cells[row, 4].Value = reports.Where(x => x.ProductId == productGroup.Key.Id && x.OrderShippingDate == dateGroup.Key.Value).Select(x => x.UpdatedQuantity).FirstOrDefault();
                            worksheet.Cells[row, 5].Value = qty.Item2;
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_compras_{manufacturer.Name}_{days}_dias.xlsx");
            }
        }

        // Resumen de pago a compradores
        [HttpGet]
        public IActionResult GenerateExcel127()
        {
            var buyerPayments = _buyerPaymentService.GetAll().ToList();
            var buyerId = buyerPayments.Select(x => x.BuyerId).ToList();
            var buyers = _customerService.GetAllCustomersQuery().Where(x => buyerId.Contains(x.Id)).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Fecha de compra";
                    worksheet.Cells[row, 2].Value = "Comprador";
                    worksheet.Cells[row, 3].Value = "Fecha de solicitud de pago";
                    worksheet.Cells[row, 4].Value = "Hora de solicitud";
                    worksheet.Cells[row, 5].Value = "Minutos de la solicitud";
                    worksheet.Cells[row, 6].Value = "Fecha de carga de comprobante";
                    worksheet.Cells[row, 7].Value = "Hora de carga de comprobante";
                    worksheet.Cells[row, 8].Value = "Minutos de carga de comprobante";

                    foreach (var payment in buyerPayments)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = payment.ShippingDate;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-MM-yyyy";
                        worksheet.Cells[row, 2].Value = buyers.Where(x => x.Id == payment.BuyerId).FirstOrDefault()?.GetFullName();
                        worksheet.Cells[row, 3].Value = payment.CreatedOnUtc.ToLocalTime();
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "dd-MM-yyyy";
                        worksheet.Cells[row, 4].Value = payment.CreatedOnUtc.ToLocalTime().Hour;
                        worksheet.Cells[row, 5].Value = payment.CreatedOnUtc.ToLocalTime().Minute;

                        var note = payment.Log.Split(new string[] { "\n" }, StringSplitOptions.None).Where(x => x.Contains("agregó un comprobante de pago de tipo")).FirstOrDefault();
                        if (payment.PaymentFileId == 0 || string.IsNullOrEmpty(note))
                        {
                            worksheet.Cells[row, 6].Value = "";
                            worksheet.Cells[row, 7].Value = "";
                            worksheet.Cells[row, 8].Value = "";
                        }
                        else
                        {
                            var dateString = note.Split(new string[] { " - " }, StringSplitOptions.None).FirstOrDefault();
                            var parsedDate = DateTime.ParseExact(dateString, "dd-MM-yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                            worksheet.Cells[row, 6].Value = parsedDate;
                            worksheet.Cells[row, 6].Style.Numberformat.Format = "dd-MM-yyyy";
                            worksheet.Cells[row, 7].Value = parsedDate.Hour;
                            worksheet.Cells[row, 8].Value = parsedDate.Minute;
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_reporte_pago_compradores.xlsx");
            }
        }


        // Clientes que no han comprado desde fecha
        [HttpGet]
        public IActionResult GenerateExcel128(string date)
        {
            var controlDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var allOrders = OrderUtils.GetFilteredOrders(_orderService);
            var customerIdsAfterControlDate = allOrders.Where(x => x.SelectedShippingDate > controlDate).Select(x => x.CustomerId).Distinct().ToList();
            var customersGroupBeforeControlDate = allOrders.Where(x => x.SelectedShippingDate <= controlDate)
                .Select(x => new GenerateExcel128Model()
                {
                    SelectedShippingDate = x.SelectedShippingDate.Value,
                    SelectedShippingTime = x.SelectedShippingTime,
                    ShippingAddress = x.ShippingAddress.Address1,
                    PhoneNumber = x.ShippingAddress.PhoneNumber,
                    OrderTotal = x.OrderTotal,
                    CustomerBalanceUsedAmount = x.CustomerBalanceUsedAmount ?? 0,
                    CustomerId = x.CustomerId,
                    FirstName = x.ShippingAddress.FirstName,
                    LastName = x.ShippingAddress.LastName,
                    PostalCode = x.ShippingAddress.ZipPostalCode,
                    Email = x.Customer.Email
                })
                .ToList()
                .Where(x => !customerIdsAfterControlDate.Contains(x.CustomerId))
                .GroupBy(x => x.CustomerId)
                .ToList();
            var zones = _shippingZoneService.GetAll().ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Correo";
                    worksheet.Cells[row, 2].Value = "Nombre";
                    worksheet.Cells[row, 3].Value = "Apellido";
                    worksheet.Cells[row, 4].Value = "Teléfono";
                    worksheet.Cells[row, 5].Value = "Código postal";
                    worksheet.Cells[row, 6].Value = "Zona";
                    worksheet.Cells[row, 7].Value = "Fecha de entrega de la última compra";
                    worksheet.Cells[row, 8].Value = "Total de compras en dinero";
                    worksheet.Cells[row, 9].Value = "Numero de pedidos completados";
                    worksheet.Cells[row, 10].Value = "Ticket promedio";

                    foreach (var group in customersGroupBeforeControlDate)
                    {
                        row++;
                        var postalCode = group.FirstOrDefault().PostalCode.Trim();
                        var zone = zones.Where(x => (x.PostalCodes + "," + x.AdditionalPostalCodes).Split(',').Select(y => y.Trim()).Contains(postalCode)).FirstOrDefault();
                        worksheet.Cells[row, 1].Value = group.FirstOrDefault().Email;
                        worksheet.Cells[row, 2].Value = group.FirstOrDefault().FirstName;
                        worksheet.Cells[row, 3].Value = group.FirstOrDefault().LastName;
                        worksheet.Cells[row, 4].Value = group.FirstOrDefault().PhoneNumber;
                        worksheet.Cells[row, 5].Value = postalCode;
                        worksheet.Cells[row, 6].Value = zone == null ? "S/I" : zone.ZoneName;
                        worksheet.Cells[row, 7].Value = group.Select(x => x.SelectedShippingDate).OrderByDescending(x => x).FirstOrDefault();
                        worksheet.Cells[row, 7].Style.Numberformat.Format = "dd-MM-yyyy";
                        worksheet.Cells[row, 8].Value = group.Select(x => x.OrderTotal + x.CustomerBalanceUsedAmount).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 9].Value = OrderUtils.GetSimplePedidosGroupByList(group.Select(x => x).ToList()).Count();
                        worksheet.Cells[row, 10].Value = group.Select(x => x.OrderTotal + x.CustomerBalanceUsedAmount).DefaultIfEmpty().Average();

                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_no_comprado_desde_{date}.xlsx");
            }
        }


        // Clientes que han comprado productos de fabricantes especificados desde fecha
        [HttpGet]
        public IActionResult GenerateExcel129(string dateFrom, string manufacturerIdsFormat)
        {
            var controlDate = DateTime.ParseExact(dateFrom, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;
            var manufacturerIds = manufacturerIdsFormat.Split(',').Select(x => int.Parse(x)).ToList();
            var manufacturers = _manufacturerService.GetAllManufacturers()
                .Where(x => manufacturerIds.Contains(x.Id))
                .OrderBy(x => x.Name).ToList();

            var zones = _shippingZoneService.GetAll()
                .Where(x => !x.Deleted).ToList();
            var regionZones = _shippingRegionZoneService.GetAll()
                .Where(x => !x.Deleted).ToList();

            var ordersAfterDate = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => controlDate <= x.SelectedShippingDate)
                .Select(x => new { OrderId = x.Id, x.CustomerId })
                .ToList();
            var orderIds = ordersAfterDate.Select(x => x.OrderId).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    foreach (var manufacturer in manufacturers)
                    {
                        var productIdsOfManufacturer = new List<int>();
                        var productsOfManufacturer = _productService.GetAllProductsQuery()
                            .Where(x => x.ProductManufacturers.Select(y => y.ManufacturerId).Contains(manufacturer.Id))
                            .Select(x => x.Id)
                            .ToList();
                        if (productsOfManufacturer.Any())
                            productIdsOfManufacturer.AddRange(productsOfManufacturer);

                        var orderIdsInItems = _orderService.GetAllOrderItemsQuery()
                            .Where(x => productIdsOfManufacturer.Contains(x.ProductId) && orderIds.Contains(x.OrderId))
                            .Select(x => x.OrderId)
                            .ToList();
                        var customerIds = ordersAfterDate.Where(x => orderIdsInItems.Contains(x.OrderId))
                            .Select(x => x.CustomerId)
                            .ToList();
                        var customers = _customerService.GetAllCustomersQuery()
                            .Where(x => customerIds.Contains(x.Id) && !x.Deleted)
                            .ToList()
                            .OrderBy(x => x.GetFullName())
                            .ToList();

                        var worksheet = xlPackage.Workbook.Worksheets.Add(manufacturer.Name);
                        int row = 1;
                        worksheet.Cells[row, 1].Value = "Nombre";
                        worksheet.Cells[row, 2].Value = "Apellido";
                        worksheet.Cells[row, 3].Value = "Correo";
                        worksheet.Cells[row, 4].Value = "Teléfono";
                        worksheet.Cells[row, 5].Value = "Zona";
                        worksheet.Cells[row, 6].Value = "Region";
                        worksheet.Cells[row, 7].Value = "CP";

                        foreach (var customer in customers)
                        {
                            row++;
                            var address = customer.ShippingAddress != null ? customer.ShippingAddress :
                                customer.BillingAddress != null ? customer.BillingAddress :
                                customer.Addresses.Count() > 0 ? customer.Addresses.OrderByDescending(x => x.CreatedOnUtc).FirstOrDefault() :
                                null;
                            var firstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                            if (string.IsNullOrEmpty(firstName))
                                firstName = address?.FirstName;

                            var lastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
                            if (string.IsNullOrEmpty(lastName))
                                lastName = address?.LastName;

                            var phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                            if (string.IsNullOrEmpty(phone))
                                phone = address?.PhoneNumber;

                            var postalCode = address.ZipPostalCode;
                            var zone = zones.Where(x => x.PostalCodes.Contains(postalCode)).FirstOrDefault();
                            if (zone == null)
                                zone = zones.Where(x => !string.IsNullOrEmpty(x.AdditionalPostalCodes) &&
                                x.AdditionalPostalCodes.Contains(postalCode)).FirstOrDefault();
                            var region = regionZones.Where(x => x.ZoneId == (zone?.Id ?? 0)).FirstOrDefault()?
                                .Region;

                            worksheet.Cells[row, 1].Value = firstName;
                            worksheet.Cells[row, 2].Value = lastName;
                            worksheet.Cells[row, 3].Value = customer.Email;
                            worksheet.Cells[row, 4].Value = phone;
                            worksheet.Cells[row, 5].Value = zone?.ZoneName;
                            worksheet.Cells[row, 6].Value = region?.Name;
                            worksheet.Cells[row, 7].Value = address?.ZipPostalCode;
                        }

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_comprado_productos_de_distribuidores_({string.Join(", ", manufacturers.Select(x => x.Name))})_desde_{dateFrom}.xlsx");
            }
        }

        // Reporte de compradores por semana
        [HttpGet]
        public IActionResult GenerateExcel130(int weeks)
        {
            var controlInitDate = DateTime.Now.AddDays(weeks * -7).AddDays(-(int)DateTime.Now.DayOfWeek + 1);
            var weekList = new List<DateTime>();
            for (int i = 0; i < weeks; i++)
            {
                weekList.Add(controlInitDate.AddDays(i * 7));
            }

            var reports = _orderReportService.GetAll().Where(x => x.OrderShippingDate > controlInitDate).ToList();
            var buyerIds = reports.Select(x => x.ReportedByCustomerId).Distinct().ToList();
            var buyers = _customerService.GetAllCustomersQuery().Where(x => buyerIds.Contains(x.Id)).ToList();
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlInitDate)
                .Select(x => new
                {
                    SelectedShippingDate = x.SelectedShippingDate.Value,
                    Shipments = x.Shipments
                }).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    foreach (var weekInitialDate in weekList)
                    {
                        var weekEndDate = weekInitialDate.AddDays(6);
                        var worksheet = xlPackage.Workbook.Worksheets.Add(weekInitialDate.ToString("dd-MM-yyyy") + "/" + weekEndDate.ToString("dd-MM-yyyy"));

                        int row = 1;
                        worksheet.Cells[row, 1].Value = "Comprador";
                        worksheet.Cells[row, 2].Value = "Promedio de: Número de productos diferentes que compró por día en el periodo de consulta";
                        worksheet.Cells[row, 3].Value = "Máximo de: Número de productos diferentes que compró por día en el periodo de consulta";
                        worksheet.Cells[row, 4].Value = "Mínimo de: Número de productos diferentes que compró por día en el periodo de consulta";
                        worksheet.Cells[row, 5].Value = "Promedio de: Total de dinero comprado por el comprador en un día en el periodo de consulta";
                        worksheet.Cells[row, 6].Value = "Máximo de: Total de dinero comprado por el comprador en un día en el periodo de consulta";
                        worksheet.Cells[row, 7].Value = "Mínimo de: Total de dinero comprado por el comprador en un día en el periodo de consulta";
                        worksheet.Cells[row, 8].Value = "Promedio de: Número de proveedores diferentes a los que les compró en un día el comprador";
                        worksheet.Cells[row, 9].Value = "Máximo de: Número de proveedores diferentes a los que les compró en un día el comprador";
                        worksheet.Cells[row, 10].Value = "Mínimo de: Número de proveedores diferentes a los que les compró en un día el comprador";
                        //worksheet.Cells[row, 11].Value = "Promedio de: Hora de registro en el reloj checador en el periodo de consulta";
                        //worksheet.Cells[row, 12].Value = "Máximo de: Hora de registro en el reloj checador en el periodo de consulta";
                        //worksheet.Cells[row, 13].Value = "Mínimo de: Hora de registro en el reloj checador en el periodo de consulta";
                        worksheet.Cells[row, 11].Value = "Promedio de: Hora de primera entrega al cliente del día en el periodo de consulta (este es igual para todos los compradores)";
                        worksheet.Cells[row, 12].Value = "Máximo de: Hora de primera entrega al cliente del día en el periodo de consulta (este es igual para todos los compradores)";
                        worksheet.Cells[row, 13].Value = "Mínimo de: Hora de primera entrega al cliente del día en el periodo de consulta (este es igual para todos los compradores)";

                        var reportsOfWeek = reports.Where(x => x.OrderShippingDate >= weekInitialDate && x.OrderShippingDate <= weekEndDate).ToList();
                        var buyerOfWeekIds = reportsOfWeek.Select(x => x.ReportedByCustomerId).Distinct().ToList();
                        var buyersOfWeek = buyers.Where(x => buyerOfWeekIds.Contains(x.Id)).ToList();
                        var deliveryFirstDeliveryTimesOfWeek = orders
                            .Where(x => x.SelectedShippingDate >= weekInitialDate && x.SelectedShippingDate <= weekEndDate)
                            .GroupBy(x => x.SelectedShippingDate)
                           .Select(x => x.SelectMany(y => y.Shipments).Where(y => y.DeliveryDateUtc.HasValue).Select(y => y.DeliveryDateUtc.Value.ToLocalTime().TimeOfDay).OrderBy(z => z).FirstOrDefault())
                           .ToList();

                        var minDeliveryTime = deliveryFirstDeliveryTimesOfWeek.Min();
                        var maxDeliveryTime = deliveryFirstDeliveryTimesOfWeek.Max();
                        var avgDeliveryTime = deliveryFirstDeliveryTimesOfWeek.Average(x => x.TotalMilliseconds);

                        foreach (var buyer in buyersOfWeek)
                        {
                            row++;
                            var reportsOfBuyer = reportsOfWeek.Where(x => x.OriginalBuyerId == buyer.Id).ToList();
                            worksheet.Cells[row, 1].Value = buyer.GetFullName();

                            var diffProductsCount = reportsOfBuyer.GroupBy(x => x.OrderShippingDate).Select(x => x.Select(y => y.ProductId).Distinct().Count());
                            worksheet.Cells[row, 2].Value = diffProductsCount.DefaultIfEmpty().Average();
                            worksheet.Cells[row, 3].Value = diffProductsCount.DefaultIfEmpty().Max();
                            worksheet.Cells[row, 4].Value = diffProductsCount.DefaultIfEmpty().Min();

                            var buyerProductAmount = reportsOfBuyer.GroupBy(x => x.OrderShippingDate)
                                .Select(x => x.GroupBy(y => y.ProductId).Select(z => z.Select(w => w.UpdatedRequestedQtyCost.Value).FirstOrDefault()).DefaultIfEmpty().Sum());
                            worksheet.Cells[row, 5].Value = buyerProductAmount.DefaultIfEmpty().Average();
                            worksheet.Cells[row, 6].Value = buyerProductAmount.DefaultIfEmpty().Max();
                            worksheet.Cells[row, 7].Value = buyerProductAmount.DefaultIfEmpty().Min();

                            var diffProductManufacturers = reportsOfBuyer.GroupBy(x => x.OrderShippingDate).Select(x => x.Select(y => y.ManufacturerId).Distinct().Count());
                            worksheet.Cells[row, 8].Value = diffProductManufacturers.DefaultIfEmpty().Average();
                            worksheet.Cells[row, 9].Value = diffProductManufacturers.DefaultIfEmpty().Max();
                            worksheet.Cells[row, 10].Value = diffProductManufacturers.DefaultIfEmpty().Min();

                            //worksheet.Cells[row, 11].Value = "";
                            //worksheet.Cells[row, 12].Value = "";
                            //worksheet.Cells[row, 13].Value = "";

                            worksheet.Cells[row, 11].Value = TimeSpan.FromMilliseconds(avgDeliveryTime);
                            worksheet.Cells[row, 11].Style.Numberformat.Format = "HH:mm";
                            worksheet.Cells[row, 12].Value = maxDeliveryTime;
                            worksheet.Cells[row, 12].Style.Numberformat.Format = "HH:mm";
                            worksheet.Cells[row, 13].Value = minDeliveryTime;
                            worksheet.Cells[row, 13].Style.Numberformat.Format = "HH:mm";

                        }

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_reporte_compradores_semanal_{weeks}_semanas.xlsx");
            }
        }

        // 
        [HttpGet]
        public IActionResult GenerateExcel131(int buyerId)
        {
            var reports = _orderReportService.GetAll().Where(x => x.ReportedByCustomerId == buyerId).ToList();
            var customer = _customerService.GetCustomerById(buyerId);
            var manufacturers = _manufacturerService.GetAllManufacturers();
            var productIds = reports.Select(x => x.ProductId).Distinct().ToList();
            var products = _productService.GetAllProductsQuery().Where(x => productIds.Contains(x.Id)).ToList();

            var groupedReportsByDate = reports.GroupBy(x => x.OrderShippingDate).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Fecha";
                    worksheet.Cells[row, 2].Value = "Producto";
                    worksheet.Cells[row, 3].Value = "Costo por unidad reportado por el comprador";
                    worksheet.Cells[row, 4].Value = "Costo por unidad liquidado";
                    worksheet.Cells[row, 5].Value = "Total pagado reportado por el comprador";
                    worksheet.Cells[row, 6].Value = "Total pagado liquidado";
                    worksheet.Cells[row, 7].Value = "Cantidad comprada reportada por el comprador";
                    worksheet.Cells[row, 8].Value = "Cantidad comprada liquidada";
                    worksheet.Cells[row, 9].Value = "Unidad";
                    worksheet.Cells[row, 10].Value = "Proveedor";
                    worksheet.Cells[row, 11].Value = "Forma de pago";
                    worksheet.Cells[row, 12].Value = "Folio";
                    worksheet.Cells[row, 13].Value = "Observaciones";

                    foreach (var dateGroup in groupedReportsByDate)
                    {
                        var gropedByProductId = dateGroup.GroupBy(x => x.ProductId).ToList();
                        foreach (var productIdGroup in gropedByProductId)
                        {
                            row++;
                            var product = products.Where(x => x.Id == productIdGroup.Key).FirstOrDefault();
                            var productReportOfDay = productIdGroup.FirstOrDefault();
                            worksheet.Cells[row, 1].Value = dateGroup.Key;
                            worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-MM-yyyy";
                            worksheet.Cells[row, 2].Value = product == null ? "S/I" : product.Name;
                            worksheet.Cells[row, 3].Value = productReportOfDay.UnitCost;
                            worksheet.Cells[row, 4].Value = productReportOfDay.UpdatedUnitCost;
                            worksheet.Cells[row, 5].Value = productReportOfDay.RequestedQtyCost;
                            worksheet.Cells[row, 6].Value = productReportOfDay.UpdatedRequestedQtyCost;
                            worksheet.Cells[row, 7].Value = productReportOfDay.Quantity;
                            worksheet.Cells[row, 8].Value = productReportOfDay.UpdatedQuantity;
                            worksheet.Cells[row, 9].Value = product == null ? "S/I" : product.WeightInterval > 0 || product.EquivalenceCoefficient > 0 ? "peso" : "pieza";
                            var manufacturer = manufacturers.Where(x => x.Id == productReportOfDay.ManufacturerId).FirstOrDefault();
                            worksheet.Cells[row, 10].Value = string.IsNullOrWhiteSpace(productReportOfDay.ShoppingStoreId) ? manufacturer == null ? "S/I" : manufacturer.Name : productReportOfDay.ShoppingStoreId;
                            worksheet.Cells[row, 11].Value = manufacturer == null ? "S/I" : manufacturer.IsPaymentWhithTransfer ? "Transferencia" : manufacturer.IsPaymentWhithCorporateCard ? "Tarjeta" : "Efectivo";
                            worksheet.Cells[row, 12].Value = productReportOfDay.Invoice;
                            worksheet.Cells[row, 13].Value = productReportOfDay.Comments;
                        }

                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_reporte_comprador_{customer?.GetFullName()}.xlsx");
            }
        }

        // Clientes registrados hace X dias que no han hecho una compra
        [HttpGet]
        public IActionResult GenerateExcel132(int days = 30)
        {
            var controlDate = DateTime.Now.Date.AddDays(-1 * days);

            var customerIdsWithOrders = OrderUtils.GetFilteredOrders(_orderService)
                .Select(x => x.CustomerId)
                .Distinct().ToList();

            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => x.Email != "" && x.Email != null && !x.Deleted && x.CreatedOnUtc >= controlDate)
                .ToList()
                .Where(x => !customerIdsWithOrders.Contains(x.Id))
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Nombre";
                    worksheet.Cells[row, 2].Value = "Correo";
                    worksheet.Cells[row, 3].Value = "Teléfono";
                    worksheet.Cells[row, 4].Value = "Fecha de registro";

                    foreach (var customer in customers)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                        worksheet.Cells[row, 2].Value = customer.Email;
                        worksheet.Cells[row, 3].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                        worksheet.Cells[row, 4].Value = customer.CreatedOnUtc.ToLocalTime();
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "dd-MM-yyyy";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_sin_pedidos_registrados_hace_{days}_dias.xlsx");
            }
        }

        // Monto comprado a fabricantes en meses
        [HttpGet]
        public IActionResult GenerateExcel133(int months = 12)
        {
            var controlDate = DateTime.Now.Date.AddMonths(-1 * months);

            var reportStatus = _orderReportStatusService.GetAll().Where(x => x.StatusTypeId == 2 && x.ShippingDate > controlDate)
                .Select(x => DbFunctions.AddMilliseconds(x.ShippingDate, x.BuyerId)).ToList();
            var reports = _orderReportService.GetAll()
                .Where(x => x.OrderShippingDate > controlDate)
                .Where(x => reportStatus.Contains(DbFunctions.AddMilliseconds(x.OrderShippingDate, x.OriginalBuyerId)))
                .Select(x => new GenerateExcel133Model()
                {
                    OrderShippingDate = x.OrderShippingDate,
                    ProductId = x.ProductId,
                    UpdatedRequestedQtyCost = x.UpdatedRequestedQtyCost,
                    OriginalBuyerId = x.OriginalBuyerId,
                    ManufacturerId = x.ManufacturerId,
                    ShoppingStoreId = x.ShoppingStoreId
                })
                .ToList();
            var manufacturers = _manufacturerService.GetAllManufacturers();
            var groupedReports = reports.GroupBy(x =>
            {
                if (x.ManufacturerId.HasValue)
                {
                    var manufacturer = manufacturers.Where(y => y.Id == x.ManufacturerId).FirstOrDefault();
                    if (manufacturer != null)
                        return manufacturer.Name;
                }
                return x.ShoppingStoreId;
            }).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Proveedor";
                    worksheet.Cells[row, 2].Value = "Compra total";
                    worksheet.Cells[row, 3].Value = "Forma de pago";

                    foreach (var group in groupedReports)
                    {
                        row++;
                        var manufacturer = manufacturers
                            .Where(x => x.Id == group.Where(y => y.ManufacturerId.HasValue).Select(y => y.ManufacturerId).FirstOrDefault()).FirstOrDefault();
                        var reportsGroupedByDate = group.GroupBy(x => x.OrderShippingDate).ToList();
                        var updateQtyCosts = reportsGroupedByDate.SelectMany(x => x.GroupBy(y => y.ProductId).Select(z => z.FirstOrDefault().UpdatedRequestedQtyCost));

                        worksheet.Cells[row, 1].Value = group.Key;
                        worksheet.Cells[row, 2].Value = updateQtyCosts.Where(x => x.HasValue).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 3].Value = manufacturer == null ? "S/I" : manufacturer.IsPaymentWhithTransfer ? "TRANSFERENCIA" : manufacturer.IsPaymentWhithCorporateCard ? "TARJETA CORPORATIVA" : "EFECTIVO";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_compra_fabricantes_{months}_meses.xlsx");
            }
        }

        [HttpGet]
        public IActionResult GenerateExcel134()
        {
            var discounts = _discountService.GetAllDiscounts(showHidden: true)
                .Where(x => x.Name.ToLower().Contains("código de recompensa por invitación"))
                .OrderBy(x => x.Name)
                .ToList();
            var discountIds = discounts.Select(x => x.Id).ToList();
            var history = _discountService.GetAllDiscountUsageHistory()
                .Where(x => discountIds.Contains(x.DiscountId))
                .ToList();
            var orderIds = history.Select(x => x.OrderId).ToList();
            var ordersWithDiscounts = _orderService.GetAllOrdersQuery()
                .Where(x => !x.Deleted && orderIds.Contains(x.Id))
                .ToList();
            var customerIds = ordersWithDiscounts
                .Select(x => x.CustomerId).Distinct()
                .ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => customerIds.Contains(x.Id))
                .ToList();
            var customerEmails = discounts.Select(x => x.LimitedToCustomerEmail).ToList();
            var customersOfCoupons = _customerService.GetAllCustomersQuery()
                .Where(x => customerEmails.Contains(x.Email))
                .ToList();

            customers.AddRange(customersOfCoupons);

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Nombre de cupón";
                    worksheet.Cells[row, 2].Value = "Correo al que está asignado ese cupón de recompensa";
                    worksheet.Cells[row, 3].Value = "Teléfono de la persona que tiene ese cupón";
                    worksheet.Cells[row, 4].Value = "Código de cupón";
                    worksheet.Cells[row, 5].Value = "Descuento";
                    worksheet.Cells[row, 6].Value = "Orden en la que se usó el código de recomendación del que recomendó";
                    worksheet.Cells[row, 7].Value = "Nombre de la persona que usó el cupón de recomendación";
                    worksheet.Cells[row, 8].Value = "Correo de la persona que usó el cupón de recomendación";

                    try
                    {
                        foreach (var discount in discounts)
                        {
                            if (row == 36)
                                _ = 0;
                            var customerOfCoupon = customers.Where(x => x.Email == discount.LimitedToCustomerEmail).FirstOrDefault();
                            var orderId = history.Where(x => x.DiscountId == discount.Id).FirstOrDefault()?.OrderId ?? 0;
                            var order = ordersWithDiscounts.Where(x => x.Id == orderId).FirstOrDefault();
                            var customer = customers.Where(x => x.Id == (order?.CustomerId ?? 0)).FirstOrDefault();

                            var phone = string.Empty;
                            if (customerOfCoupon != null)
                            {
                                phone = customerOfCoupon.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                                if (string.IsNullOrEmpty(phone) && customerOfCoupon.BillingAddress != null)
                                    phone = customerOfCoupon.BillingAddress.PhoneNumber;
                                if (string.IsNullOrEmpty(phone) && customerOfCoupon.ShippingAddress != null)
                                    phone = customerOfCoupon.ShippingAddress.PhoneNumber;
                            }

                            row++;
                            worksheet.Cells[row, 1].Value = discount.Name;
                            worksheet.Cells[row, 2].Value = discount.LimitedToCustomerEmail;
                            worksheet.Cells[row, 3].Value = phone;
                            worksheet.Cells[row, 4].Value = discount.CouponCode;
                            worksheet.Cells[row, 5].Value = discount.UsePercentage ? discount.DiscountPercentage + "%" : discount.DiscountAmount.ToString();
                            worksheet.Cells[row, 6].Value = order == null ? "---" : $"#{order.Id}";
                            worksheet.Cells[row, 7].Value = customer == null ? "---" : customer.GetFullName();
                            worksheet.Cells[row, 8].Value = customer == null ? "---" : customer.Email;
                        }

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }

                        xlPackage.Save();
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_descuentos_y_sus_usos_en_ordenes.xlsx");
            }
        }

        // Venta de fabricante por dia
        [HttpGet]
        public IActionResult GenerateExcel135(int fbId)
        {
            var manufacturerItems = OrderUtils.GetFilteredOrders(_orderService)
                .SelectMany(x => x.OrderItems.Where(y => y.Product.ProductManufacturers.Where(z => z.ManufacturerId == fbId).Any()))
                .ToList();
            var dateGroup = manufacturerItems.GroupBy(x => x.Order.SelectedShippingDate).OrderBy(x => x.Key).ToList();
            var manufacturer = _manufacturerService.GetManufacturerById(fbId);

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Fecha";
                    worksheet.Cells[row, 2].Value = "Monto vendido";

                    foreach (var group in dateGroup)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = group.Key;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-MM-yyyy";
                        worksheet.Cells[row, 2].Value = group.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ventas_por_dia_{manufacturer.Name}.xlsx");
            }
        }

        // Comprado a fabricante por producto
        [HttpGet]
        public IActionResult GenerateExcel136(int fbId)
        {
            var manufacturerItems = OrderUtils.GetFilteredOrders(_orderService)
                .SelectMany(x => x.OrderItems.Where(y => y.Product.ProductManufacturers.Where(z => z.ManufacturerId == fbId).Any()))
                .ToList();
            var productGroup = manufacturerItems.GroupBy(x => x.ProductId).ToList();
            var productIds = productGroup.Select(x => x.Key).Distinct().ToList();
            var products = _productService.GetAllProductsQuery().Where(x => productIds.Contains(x.Id)).ToList();
            var manufacturer = _manufacturerService.GetManufacturerById(fbId);

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Producto";
                    worksheet.Cells[row, 2].Value = "Monto vendido";

                    foreach (var group in productGroup)
                    {
                        row++;
                        var product = products.Where(x => x.Id == group.Key).FirstOrDefault();
                        worksheet.Cells[row, 1].Value = product.Name;
                        worksheet.Cells[row, 2].Value = group.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ventas_por_producto_{manufacturer.Name}.xlsx");
            }
        }

        // Usos de cupón de amigo
        [HttpGet]
        public IActionResult GenerateExcel137()
        {
            var discounts = _discountService.GetAllDiscounts().Where(x => x.CustomerOwnerId > 0).ToList();
            var discountIds = discounts.Select(x => x.Id).Distinct().ToList();
            var customerIds = discounts.Select(x => x.CustomerOwnerId).Distinct().ToList();
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Select(x => new
                {
                    x.Id,
                    DiscountUsageHistory = x.DiscountUsageHistory.Select(y => new
                    {
                        y.DiscountId,
                    }),
                    x.Customer
                })
                .ToList()
                .Where(x => x.DiscountUsageHistory.Select(y => y.DiscountId).Intersect(discountIds).Any())
                .ToList();
            var customers = _customerService.GetAllCustomersQuery().Where(x => customerIds.Contains(x.Id)).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Nombre de cupón";
                    worksheet.Cells[row, 2].Value = "Correo al que está asignado ese cupón de recompensa";
                    worksheet.Cells[row, 3].Value = "Teléfono de la persona que tiene ese cupón";
                    worksheet.Cells[row, 4].Value = "Código de cupón";
                    worksheet.Cells[row, 5].Value = "Descuento";
                    worksheet.Cells[row, 6].Value = "Orden en la que se usó el código de recomendación del que recomendó";
                    worksheet.Cells[row, 7].Value = "Nombre de la persona que usó el cupón de recomendación";
                    worksheet.Cells[row, 8].Value = "Correo de la persona que usó el cupón de recomendación";

                    foreach (var discount in discounts)
                    {
                        var ordersWithDiscount = orders.Where(x => x.DiscountUsageHistory.Where(y => y.DiscountId == discount.Id).Any()).ToList();
                        foreach (var order in ordersWithDiscount)
                        {
                            var customer = customers.Where(x => x.Id == discount.CustomerOwnerId).FirstOrDefault();
                            if (customer == null) continue;
                            row++;
                            worksheet.Cells[row, 1].Value = discount.Name;
                            worksheet.Cells[row, 2].Value = customer.Email;
                            var phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                            worksheet.Cells[row, 3].Value = string.IsNullOrEmpty(phone) ? customer.Addresses.Select(x => x.PhoneNumber).FirstOrDefault() : phone;
                            worksheet.Cells[row, 4].Value = discount.CouponCode;
                            worksheet.Cells[row, 5].Value = discount.DiscountAmount;
                            worksheet.Cells[row, 6].Value = order.Id;
                            worksheet.Cells[row, 7].Value = order.Customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                            worksheet.Cells[row, 8].Value = order.Customer.Email;
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_usos_cupon_recomendado.xlsx");
            }
        }

        // Clientes que usaron lista de cupones
        [HttpGet]
        public IActionResult GenerateExcel138()
        {
            var coupons = new List<string>()
            {
                "LIMPIEZA100",
                "CENA100",
                "SUPER100",
                "100PARASUPER",
                "SOLOHOY17",
                "SUPER150",
                "15QUINCENA",
                "POSADAS21",
                "SUPER200",
                "CENA200",
                "FLASH200",
                "YAMEACORDE",
                "CARNES15",
                "CUESTA2022",
                "CELMECUIDA1",
                "FINENERO",
                "2022CONCENTRAL",
                "FINDE22",
                "FINDELI",
                "FRESCOSFEB99",
                "SOLOFYV-DESAC",
                "ADIOSFEBRERO",
                "YOAMOCEL",
                "PUENTEDELI",
                "SOLOBYD",
                "SOLOLYH2511",
                "QUINCECEL",
                "ACTITUDCEL",
                "DOMINGOCEL",
                "VIERNESCEL",
                "ANIVERSARIO22",
                "CUARESMA22",
                "CEL15",
                "SUPERCASH",
                "LUNESCASH",
                "REGALOCEL",
                "CUMPLECEL"
            };

            var parsedCoupons = coupons.Select(x => x.Trim().ToLower()).ToList();
            var discounts = _discountService.GetAllDiscounts(showHidden: true).Where(x => parsedCoupons.Contains(x.CouponCode?.Trim().ToLower())).ToList();
            var discountIds = discounts.Select(x => x.Id).ToList();
            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.DiscountUsageHistory.Select(y => y.DiscountId).Intersect(discountIds).Any()).ToList();
            var customerIds = orders.Select(x => x.CustomerId).Distinct().ToList();
            var customers = _customerService.GetAllCustomersQuery().Where(x => customerIds.Contains(x.Id)).ToList();
            var newsletter = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    foreach (var discount in discounts)
                    {
                        var worksheet = xlPackage.Workbook.Worksheets.Add(discount.CouponCode.ToUpper());
                        int row = 1;

                        worksheet.Cells[row, 1].Value = "Nombre";
                        worksheet.Cells[row, 2].Value = "Apellido";
                        worksheet.Cells[row, 3].Value = "Teléfono";
                        worksheet.Cells[row, 4].Value = "Correo";
                        worksheet.Cells[row, 5].Value = "CP";
                        worksheet.Cells[row, 6].Value = "Activo en Newsletter?";

                        var customerWhoUsedDiscountIds = orders
                            .Where(x => x.DiscountUsageHistory.Where(y => y.DiscountId == discount.Id).Any())
                            .Select(x => x.CustomerId)
                            .Distinct()
                            .ToList();
                        var filteredCustomers = customers.Where(x => customerWhoUsedDiscountIds.Contains(x.Id)).ToList();
                        foreach (var customer in filteredCustomers)
                        {
                            row++;
                            worksheet.Cells[row, 1].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                            worksheet.Cells[row, 2].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
                            var phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                            worksheet.Cells[row, 3].Value = string.IsNullOrEmpty(phone) ? customer.ShippingAddress?.PhoneNumber : phone;
                            worksheet.Cells[row, 4].Value = customer.Email;
                            worksheet.Cells[row, 5].Value = customer.ShippingAddress?.ZipPostalCode;
                            worksheet.Cells[row, 6].Value = newsletter.Where(x => x.Email == customer.Email).Select(x => x.Active).FirstOrDefault() ? "SI" : "NO";
                        }

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_usos_cupones.xlsx");
            }
        }

        // Clientes con local en la direccion
        [HttpGet]
        public IActionResult GenerateExcel139()
        {
            var customerIds2022 = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate.Value.Year == 2022).Select(x => x.CustomerId).Distinct().ToList();
            var customerIds2021 = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate.Value.Year == 2021).Select(x => x.CustomerId).Distinct().ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => !customerIds2022.Contains(x.Id) && customerIds2021.Contains(x.Id) && x.Addresses.Where(y => y.Address1.ToLower().Contains("local")).Any())
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Nombre";
                    worksheet.Cells[row, 2].Value = "Apellido";
                    worksheet.Cells[row, 3].Value = "Teléfono";
                    worksheet.Cells[row, 4].Value = "Correo";
                    worksheet.Cells[row, 5].Value = "Direccion";
                    worksheet.Cells[row, 6].Value = "CP";
                    worksheet.Cells[row, 7].Value = "Fecha de última compra";

                    foreach (var customer in customers)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                        worksheet.Cells[row, 2].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
                        var phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                        worksheet.Cells[row, 3].Value = string.IsNullOrEmpty(phone) ? customer.ShippingAddress?.PhoneNumber : phone;
                        worksheet.Cells[row, 4].Value = customer.Email;
                        var address = customer.Addresses.Where(x => x.Address1.ToLower().Contains("local")).FirstOrDefault();
                        worksheet.Cells[row, 5].Value = address?.Address1;
                        worksheet.Cells[row, 6].Value = address?.ZipPostalCode;
                        var lastOrderDate = OrderUtils.GetFilteredOrders(_orderService)
                            .Where(x => x.CustomerId == customer.Id)
                            .Select(x => x.SelectedShippingDate.Value)
                            .OrderByDescending(x => x).FirstOrDefault();
                        worksheet.Cells[row, 7].Value = lastOrderDate;
                        worksheet.Cells[row, 7].Style.Numberformat.Format = "dd-mm-yyyy";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_direccion_con_local.xlsx");
            }
        }

        // Número de pedidos y rutas por semana
        [HttpGet]
        public IActionResult GenerateExcel140()
        {
            var controlDate = new DateTime(2021, 11, 2);
            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate >= controlDate && x.RouteId > 0);
            var groupedByWeek = orders
                .OrderBy(x => x.SelectedShippingDate)
                .ToList()
                .GroupBy(x => x.SelectedShippingDate.Value.AddDays(-(int)x.SelectedShippingDate.Value.DayOfWeek))
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int weekCount = 1;

                    worksheet.Cells[row, 1].Value = "Semana";
                    worksheet.Cells[row, 2].Value = "Fecha inicio";
                    worksheet.Cells[row, 3].Value = "Fecha final";
                    worksheet.Cells[row, 4].Value = "Número de pedidos de la semana";
                    worksheet.Cells[row, 4].Value = "Número de rutas de la semana";

                    foreach (var group in groupedByWeek)
                    {
                        row++;
                        var pedidos = OrderUtils.GetPedidosGroupByList(group.Select(x => x).ToList()).ToList();
                        var routesCount = group.GroupBy(x => x.SelectedShippingDate).Select(x => x.Select(y => y.RouteId).Distinct()).Select(x => x.Count()).DefaultIfEmpty().Sum();

                        worksheet.Cells[row, 1].Value = weekCount;
                        worksheet.Cells[row, 2].Value = group.Key;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 3].Value = group.Key.AddDays(6);
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 4].Value = pedidos.Count;
                        worksheet.Cells[row, 5].Value = routesCount;
                        weekCount++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_pedidos_rutas_semanales.xlsx");
            }
        }

        // Datos de lista de clientes
        [HttpGet]
        public IActionResult GenerateExcel141()
        {
            var emails = new List<string>()
            {
                "aaron.pantoja.r@gmail.com",
                "abrahamreza1987@gmail.com",
                "acardenas@abgrupo.com.mx",
                "adrix_1980@hotmail.com",
                "aiko1.21@gmail.com",
                "alberto.ramirez.pdc@gmail.com",
                "alemendigochea@gmail.com",
                "alexrobles07@gmail.com",
                "amarvel.am@gmail.com",
                "anabellsalas@gmail.com",
                "anagsiu@gmail.com",
                "anapaubermudez@gmail.com",
                "andrea_cornu@hotmail.com",
                "andrea.isantoyo@gmail.com",
                "andreargarza@hotmail.com",
                "aneudelabarrera@gmail.com",
                "arreola.cristina@outlook.com",
                "ash.pwp12041@gmail.com",
                "atenorio.udibi@gmail.com",
                "ayumi_si@hotmail.com",
                "barbapedro@hotmail.com",
                "becaiberri@yahoo.com.mx",
                "berdeja2806@hotmail.com",
                "bmaconcierge@gmail.com",
                "bulosnm@hotmail.com",
                "candiemartineza@gmail.com",
                "Carlayv@hotmail.com",
                "carocaba44@gmail.com",
                "carolina.jaime80@gmail.com",
                "Cbeyetter@gmail.com",
                "cedrela.lg@gmail.com",
                "cheveronikim@gmail.com",
                "chuy_agv@hotmail.com",
                "cocolitoforida@gmail.com",
                "colchero_mtz@outlook.com",
                "Dannymarquina@gmail.com",
                "danyfigueroa0911@gmail.com",
                "Dzoarada@yahoo.com",
                "Eaacortes@gmail.com",
                "eduhlujan@gmail.com",
                "ericksonic@gmail.com",
                "erika.carina.hg@gmail.com",
                "erinflores711@gmail.com",
                "estephanyh@hotmail.com",
                "estrella.padilla@gmail.com",
                "fabian.gonzalez08072011@gmail.com",
                "falexisft59@gmail.com",
                "fazio.perezmoreno@gmail.com",
                "ferdiazmp@hotmail.com",
                "gabyvv28@gmail.com",
                "gacm.3506@gmail.com",
                "galindoisabelc@gmail.com",
                "gigitejada@gmail.com",
                "gmoshi@icloud.com",
                "hagaluna@hotmail.com",
                "helenasolleiro@gmail.com",
                "Hildapersonales@gmail.com",
                "horacio.barros@live.com.mx",
                "iaa@godoynovoa.com.mx",
                "irisescamilla@hotmail.com",
                "irmaleticiav@gmail.com",
                "isabeltaracenaf@gmail.com",
                "itzel.gilrod@gmail.com",
                "itzia_sdoka@msn.com",
                "Jaaimecaarmonae@hotmail.com",
                "janeth.angie.hdez@gmail.com",
                "janettereyed19@gmail.com",
                "jangab_@hotmail.com",
                "jcarlos.bme@gmail.com",
                "jem.gewel@gmail.com",
                "jesus.ocampo211@hotmail.com",
                "Jorgeguzmang@outlook.com",
                "jose@ssanchez.net",
                "jreyes.acevedo@gmail.com",
                "k_marcial@hotmail.com",
                "k.salinas@labodigital.com.mx",
                "karenpzm@hotmail.com",
                "karina.orozcoe@gmail.com",
                "karla.1091@gmail.com",
                "karla.sofia.ontiveros@gmail.com",
                "krn_pg@hotmail.com",
                "lagapab@gmail.com",
                "laura_rojash@yahoo.com.mx",
                "lauriscs@hotmail.com",
                "laurishp46@gmail.com",
                "lgccecilia@yahoo.com.mx",
                "loregl2000@gmail.com",
                "luis.m.880317@gmail.com",
                "Luisa.jimena.rp@gmail.com",
                "mahela_21@hotmail.com",
                "mariajhi@gmail.com",
                "mariannespinosadelosmonteros@gmail.com",
                "mariselaramirezsan@gmail.com",
                "marmoni_87@hotmail.com",
                "maruinteriano@gmail.com",
                "mcchouza@gmail.com",
                "mensajesactv@gmail.com",
                "mhm_727@hotmail.com",
                "miamorochito@gmail.com",
                "miriam.audelo@yahoo.com",
                "natalia.lunabp@gmail.com",
                "Nike.hammeken@gmail.com",
                "oiich@outlook.com",
                "olyml@yahoo.com",
                "ortiz_susana@yahoo.com",
                "Pamies1604@gmail.com",
                "paodeyanira24@gmail.com",
                "paola.garfias.lemus@gmail.com",
                "Patygregory60@hotmail.com",
                "PEDREGAL@maricu.mx",
                "pedrofernandez@ceoconsultores.com",
                "Rebazaola@yahoo.com",
                "Rebecca_Ellis@gmx.net",
                "rkleinb@gmail.com",
                "rocharobledo@gmail.com",
                "rosaura.betanzos.lara@gmail.com",
                "rosbetlar2015@gmail.com",
                "roy_vera@hotmail.com",
                "sachiiko@yahoo.com",
                "SALFAGEME@YAHOO.COM",
                "srita_cometa2000@hotmail.com",
                "tania_rc@hotmail.com",
                "tersytoledo@gmail.com",
                "verotapiaj@hotmail.com",
                "victor_emmanuel_sp@outlook.com",
                "ximena.orozcog@gmail.com",
                "xtony65@hotmail.com",
                "Yaahra@yahoo.com.mx",
                "yasukato@hotmail.com",
                "yuriko.taniguchi@gmail.com"
            };

            var customers = _customerService.GetAllCustomersQuery().Where(x => emails.Contains(x.Email)).ToList();
            var customerIds = customers.Select(x => x.Id).Distinct().ToList();
            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => customerIds.Contains(x.CustomerId)).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Nombre";
                    worksheet.Cells[row, 2].Value = "Apellido";
                    worksheet.Cells[row, 3].Value = "Correo";
                    worksheet.Cells[row, 4].Value = "Teléfono";
                    worksheet.Cells[row, 5].Value = "Fecha primera compra";
                    worksheet.Cells[row, 6].Value = "Fecha última compra";
                    worksheet.Cells[row, 7].Value = "Monto de venta histórico";
                    worksheet.Cells[row, 8].Value = "Ticket promedio";
                    worksheet.Cells[row, 9].Value = "Recurrencia";

                    foreach (var customer in customers)
                    {
                        row++;
                        if (customer.Id == 17934853)
                            Debugger.Break();

                        var customerOrders = orders.Where(x => x.CustomerId == customer.Id).ToList();
                        var pedidos = OrderUtils.GetPedidosGroupByList(customerOrders).ToList();

                        worksheet.Cells[row, 1].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                        worksheet.Cells[row, 2].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
                        worksheet.Cells[row, 3].Value = customer.Email;
                        var phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                        worksheet.Cells[row, 4].Value = string.IsNullOrEmpty(phone) ? customer.ShippingAddress?.PhoneNumber : phone;
                        var firstPurchaseDate = customerOrders.Select(x => x.SelectedShippingDate).OrderBy(x => x).FirstOrDefault().Value;
                        worksheet.Cells[row, 5].Value = firstPurchaseDate;
                        worksheet.Cells[row, 5].Style.Numberformat.Format = "dd-mm-yyyy";
                        var lastPurchaseDate = customerOrders.Select(x => x.SelectedShippingDate).OrderBy(x => x).LastOrDefault().Value;
                        worksheet.Cells[row, 6].Value = lastPurchaseDate;
                        worksheet.Cells[row, 6].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 7].Value = customerOrders.Select(x => x.OrderTotal + (x.CustomerBalanceUsedAmount ?? 0)).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 8].Value = customerOrders.Select(x => x.OrderTotal + (x.CustomerBalanceUsedAmount ?? 0)).DefaultIfEmpty().Average();
                        worksheet.Cells[row, 9].Value = pedidos.Count == 0 ? 0 : (decimal)(lastPurchaseDate - firstPurchaseDate).TotalDays / (decimal)pedidos.Count;

                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_datos_lista_clientes.xlsx");
            }
        }

        // Reporte de costos de producto donde el costo nuevo es menor al original
        [HttpGet]
        public IActionResult GenerateExcel142(int months = 6)
        {
            var controlDate = DateTime.UtcNow.AddMonths(-1 * months).Date;
            var reportsQuery = _orderReportService.GetAll().Where(x => !x.Deleted);
            var orderReports = reportsQuery
                .Where(x => controlDate <= DbFunctions.TruncateTime(x.ReportedDateUtc))
                .Select(x => new CostsInfo
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    ReportedDateUtc = x.ReportedDateUtc,
                    UpdatedUnitCost = x.UpdatedUnitCost,
                })
                .OrderBy(x => x.ProductId)
                .ThenByDescending(x => x.ReportedDateUtc)
                .GroupBy(x => x.ProductId)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "ID del producto";
                    worksheet.Cells[row, 2].Value = "Fecha de reporte";
                    worksheet.Cells[row, 3].Value = "Costo anterior";
                    worksheet.Cells[row, 4].Value = "Costo nuevo";
                    row++;

                    foreach (var orderReport in orderReports)
                    {
                        var lastReported = orderReport.FirstOrDefault();
                        var penultimateReported = lastReported;
                        var count = 1;
                        do
                        {
                            penultimateReported = orderReport.ElementAtOrDefault(count);
                            count++;
                            if (penultimateReported == null)
                                penultimateReported = new CostsInfo();
                        } while (lastReported.ReportedDateUtc.Date == penultimateReported.ReportedDateUtc.Date &&
                        lastReported.UpdatedUnitCost == penultimateReported.UpdatedUnitCost);

                        if (penultimateReported.UpdatedUnitCost > lastReported.UpdatedUnitCost &&
                            lastReported.UpdatedUnitCost > 0)
                        {
                            worksheet.Cells[row, 1].Value = lastReported.ProductId;
                            worksheet.Cells[row, 2].Value = lastReported.ReportedDateUtc.ToLocalTime().Date;
                            worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";

                            worksheet.Cells[row, 3].Value = penultimateReported.UpdatedUnitCost;
                            worksheet.Cells[row, 3].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            worksheet.Cells[row, 4].Value = lastReported.UpdatedUnitCost;
                            worksheet.Cells[row, 4].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            row++;
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_reporte_costos_productos_menor_a_anterior.xlsx");
            }
        }

        // Reporte de clientes registrados desde X fecha
        [HttpGet]
        public IActionResult GenerateExcel143(string date = "01-01-2022")
        {
            var parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var utcDate = parsedDate.ToUniversalTime();
            var minutes = utcDate.Subtract(parsedDate).TotalMinutes;
            parsedDate = parsedDate.AddMinutes(-1 * minutes);
            var newsletter = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => !x.Deleted && parsedDate <= x.CreatedOnUtc).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Nombre";
                    worksheet.Cells[row, 2].Value = "Teléfono";
                    worksheet.Cells[row, 3].Value = "Correo";
                    worksheet.Cells[row, 4].Value = "Activo en news";
                    row++;

                    foreach (var customer in customers)
                    {
                        worksheet.Cells[row, 1].Value = customer.GetFullName();
                        var phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                        worksheet.Cells[row, 2].Value = string.IsNullOrEmpty(phone) ?
                            customer.ShippingAddress?.PhoneNumber : phone;
                        worksheet.Cells[row, 3].Value = customer.Email;
                        worksheet.Cells[row, 4].Value = newsletter.Where(y => y.Email == customer.Email).Select(y => y.Active).FirstOrDefault() ? "SI" : "NO";
                        row++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_reporte_costos_productos_menor_a_anterior.xlsx");
            }
        }

        // Ventas mensuales desde X fecha
        [HttpGet]
        public IActionResult GenerateExcel144(string date = "01-01-2022")
        {
            var parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => parsedDate <= x.SelectedShippingDate)
                .Select(x => new GenerateExcel14Model()
                {
                    ShippingAddress = x.ShippingAddress.Address1,
                    CustomerId = x.CustomerId,
                    SelectedShippingDate = x.SelectedShippingDate,
                    SelectedShippingTime = x.SelectedShippingTime,
                    OrderTotal = x.OrderTotal,
                    Id = x.Id
                })
                .ToList();
            var ordersDate = orders.Select(x => x.SelectedShippingDate).OrderBy(x => x).ToList();
            var initDate = new DateTime(ordersDate.FirstOrDefault().Value.Year, ordersDate.FirstOrDefault().Value.Month, 1);
            var endDate = new DateTime(ordersDate.LastOrDefault().Value.Year, ordersDate.LastOrDefault().Value.Month, 1).AddMonths(1).AddDays(-1);
            var orderIds = orders.Select(x => x.Id).ToList();
            var reports = _orderReportService.GetAll()
                .Select(x => new
                {
                    x.OrderId,
                    x.OrderShippingDate,
                    x.ProductId,
                    x.UpdatedRequestedQtyCost,
                    x.OriginalBuyerId,
                    x.ManufacturerId
                })
                .ToList();
            var reportStatus = _orderReportStatusService.GetAll().Where(x => x.StatusTypeId == 2)
                .Select(x => new { x.BuyerId, x.ShippingDate }).ToList();

            var manufacturers = _manufacturerService.GetAllManufacturers();
            var transferManufacturerIds = manufacturers.Where(x => x.IsPaymentWhithTransfer).Select(x => x.Id).ToList();
            var cashManufacturerIds = manufacturers.Where(x => !x.IsPaymentWhithTransfer && !x.IsPaymentWhithCorporateCard).Select(x => x.Id).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Mes";
                    worksheet.Cells[row, 2].Value = "Monto vendido";
                    worksheet.Cells[row, 3].Value = "Número de pedidos";
                    worksheet.Cells[row, 4].Value = "Monto de compra de producto";
                    worksheet.Cells[row, 5].Value = "Monto de compra de producto en efectivo";
                    worksheet.Cells[row, 6].Value = "Monto de compra de producto por transferencia";

                    for (DateTime i = initDate; i <= endDate; i = i.AddMonths(1))
                    {
                        row++;
                        var filteredOrders = orders.Where(x => x.SelectedShippingDate.Value.Month == i.Month && x.SelectedShippingDate.Value.Year == i.Year).ToList();
                        var filteredOrderIds = filteredOrders.Select(x => x.Id).ToList();
                        var pedidos = OrderUtils.GetSimplePedidosGroupByList<GenerateExcel14Model>(filteredOrders);

                        var filteredReports = reports.Where(x => filteredOrderIds.Contains(x.OrderId));
                        var filteredReportsConfirmed = reports.Where(x => filteredOrderIds.Contains(x.OrderId) && reportStatus.Contains(new { BuyerId = x.OriginalBuyerId, ShippingDate = x.OrderShippingDate }));
                        var reportsGroupedByDateConfirmed = filteredReportsConfirmed.GroupBy(x => x.OrderShippingDate).ToList();
                        var updateQtyCostsConfirmed = reportsGroupedByDateConfirmed.SelectMany(x => x.GroupBy(y => y.ProductId).Select(z => z.FirstOrDefault()));

                        worksheet.Cells[row, 1].Value = i;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "mmmm, yyyy";
                        worksheet.Cells[row, 2].Value = filteredOrders.Sum(x => x.OrderTotal);
                        worksheet.Cells[row, 3].Value = pedidos.Count();
                        worksheet.Cells[row, 4].Value = updateQtyCostsConfirmed.Select(x => x.UpdatedRequestedQtyCost).DefaultIfEmpty().Sum();

                        var transfer = updateQtyCostsConfirmed.Where(x => x.ManufacturerId.HasValue && transferManufacturerIds.Contains(x.ManufacturerId.Value)).ToList();
                        var cash = updateQtyCostsConfirmed.Where(x => x.ManufacturerId.HasValue && cashManufacturerIds.Contains(x.ManufacturerId.Value)).ToList();
                        worksheet.Cells[row, 5].Value = transfer.Select(x => x.UpdatedRequestedQtyCost).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 6].Value = cash.Select(x => x.UpdatedRequestedQtyCost).DefaultIfEmpty().Sum();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ventas_pedidos_mensuales.xlsx");
            }
        }

        // Total de suma de pedidos completados de todos los clientes
        [HttpGet]
        public IActionResult GenerateExcel145()
        {
            var orders = GetFilteredOrders()
                .Where(x => x.OrderStatusId == (int)OrderStatus.Complete)
                .Select(x => new { x.CustomerId, x.OrderTotal })
                .ToList();
            var customerIds = orders.Select(x => x.CustomerId).Distinct().ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => customerIds.Contains(x.Id)).ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Id del cliente";
                    worksheet.Cells[row, 2].Value = "Usuario";
                    worksheet.Cells[row, 3].Value = "Suma del total de pedidos completados";

                    foreach (var customer in customers)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = customer.Id;
                        worksheet.Cells[row, 2].Value = customer.Email;
                        worksheet.Cells[row, 3].Value = orders.Where(x => x.CustomerId == customer.Id)
                            .Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_suma_todas_ordenes.xlsx");
            }
        }

        // Total de suma de pedidos completados de todos los clientes
        [HttpGet]
        public IActionResult GenerateExcel146()
        {
            var date = new DateTime(2022, 1, 1);
            var ordersQuery = GetFilteredOrders()
                .Where(x => x.OrderStatusId == (int)OrderStatus.Complete);
            var orders = ordersQuery
                .Where(x => date <= x.SelectedShippingDate)
                .Select(x => new { x.CustomerId, SelectedShippingDate = DbFunctions.TruncateTime(x.SelectedShippingDate.Value) })
                .ToList();
            var customerIds = orders.Select(x => x.CustomerId).Distinct().ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => customerIds.Contains(x.Id)).ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Correo";
                    worksheet.Cells[row, 2].Value = "Fecha de primer pedido";
                    worksheet.Cells[row, 3].Value = "Demas fechas de pedidos de este año";

                    foreach (var customer in customers)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = customer.Email;
                        worksheet.Cells[row, 2].Value = ordersQuery
                            .Where(x => x.CustomerId == customer.Id && x.SelectedShippingDate != null)
                            .OrderBy(x => x.SelectedShippingDate)
                            .FirstOrDefault().SelectedShippingDate;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";

                        var orderDatesOfClient = orders.Where(x => x.CustomerId == customer.Id)
                            .OrderBy(x => x.SelectedShippingDate)
                            .Select(x => x.SelectedShippingDate)
                            .ToList();
                        var col = 3;
                        foreach (var orderDate in orderDatesOfClient.Distinct())
                        {
                            worksheet.Cells[row, col].Value = orderDate;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "dd-mm-yyyy";
                            col++;
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_fechas_de_pedidos_de_clientes_desde_inicio_de_año.xlsx");
            }
        }


        // Total de suma de pedidos completados de todos los clientes
        [HttpGet]
        public IActionResult GenerateExcel147(int days = 90)
        {
            var date = DateTime.UtcNow.AddDays(-1 * days);
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => !string.IsNullOrEmpty(x.Email) && !x.Deleted && date <= DbFunctions.TruncateTime(x.CreatedOnUtc))
                .ToList();
            var customerIds = customers.Select(x => x.Id).ToList();
            var orders = _orderService.GetAllOrdersQuery()
                .Where(x => !x.Deleted && customerIds.Contains(x.CustomerId))
                .Select(x => new { x.CustomerId, OrderId = x.Id })
                .ToList();
            var customerIdsWithOrders = orders.Select(x => x.CustomerId).ToList();
            var customersWithoutOrders = customers.Where(x => !customerIdsWithOrders.Contains(x.Id))
                .ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Nombre";
                    worksheet.Cells[row, 2].Value = "Apellido";
                    worksheet.Cells[row, 3].Value = "Correo";
                    worksheet.Cells[row, 4].Value = "Fecha de registro";
                    worksheet.Cells[row, 5].Value = "Teléfono";

                    foreach (var customer in customersWithoutOrders)
                    {
                        row++;
                        var firstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                        worksheet.Cells[row, 1].Value = firstName;
                        var lastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
                        worksheet.Cells[row, 2].Value = lastName;
                        worksheet.Cells[row, 3].Value = customer.Email;
                        worksheet.Cells[row, 4].Value = customer.CreatedOnUtc.ToLocalTime();
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "dd-mm-yyyy";
                        var phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                        worksheet.Cells[row, 5].Value = string.IsNullOrEmpty(phone) ?
                            customer.ShippingAddress?.PhoneNumber : phone;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_registrados_ultimos_{days}_dias_sin_ordenes.xlsx");
            }
        }

        // Total de suma de pedidos completados de todos los clientes
        [HttpGet]
        public IActionResult GenerateExcel148(string names = "")
        {
            var namesList = names.Split(',').Select(x => x.ToLower().Trim()).ToList();
            var products = _productService.GetAllProductsQuery()
                .Where(x => !x.Deleted && namesList.Contains(x.Name.ToLower().Trim()))
                .ToList();
            var productIds = products.Select(x => x.Id).ToList();

            var orderItemsWithProducts = _orderService.GetAllOrderItemsQuery()
                .Where(x => productIds.Contains(x.ProductId))
                .Select(x => new { x.Id, x.OrderId })
                .ToList();
            var orderIds = orderItemsWithProducts.Select(x => x.OrderId).Distinct().ToList();
            var orders = _orderService.GetAllOrdersQuery()
                .Where(x => !x.Deleted && orderIds.Contains(x.Id))
                .Select(x => new { x.CustomerId, OrderId = x.Id })
                .ToList();
            var customerIds = orders.Select(x => x.CustomerId).Distinct().ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => !string.IsNullOrEmpty(x.Email) && !x.Deleted && customerIds.Contains(x.Id))
                .ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Nombre";
                    worksheet.Cells[row, 2].Value = "Correo";
                    worksheet.Cells[row, 3].Value = "Teléfono";

                    foreach (var customer in customers)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = customer.GetFullName();
                        worksheet.Cells[row, 2].Value = customer.Email;
                        var phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                        worksheet.Cells[row, 3].Value = string.IsNullOrEmpty(phone) ?
                            customer.ShippingAddress?.PhoneNumber : phone;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_que_han_comprado_ciertos_productos.xlsx");
            }
        }

        // Total de suma de pedidos completados de todos los clientes
        [HttpGet]
        public IActionResult GenerateExcel149(string names = "")
        {
            var customerOrders = GetFilteredOrders()
                .Where(x => x.OrderStatusId == (int)OrderStatus.Complete &&
                x.PaymentMethodSystemName == "Payments.PayPalStandard" || x.PaymentMethodSystemName == "Payments.Visa" &&
                x.ShippingAddressId != null)
                .Select(x => new { x.CustomerId, x.Id, x.BillingAddressId, ShippingAddressId = x.ShippingAddressId.Value })
                .GroupBy(x => x.CustomerId)
                .ToList();
            var customersWithOnlyOneOrder = customerOrders.Where(x => x.Count() == 1).ToList();
            var customerIds = customerOrders.Select(x => x.Key).ToList();
            var addresseIds = customerOrders.SelectMany(x => x.Select(y => y.BillingAddressId))
                .ToList();
            addresseIds.AddRange(customerOrders.SelectMany(x => x.Select(y => y.ShippingAddressId)).Distinct().ToList());
            var addresses = _addressService.GetAllAddressesQuery()
                .Where(x => addresseIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Email = x.Email ?? "",
                    Address1 = x.Address1 ?? "",
                    Address2 = x.Address2 ?? "",
                    City = x.City ?? "",
                    FirstName = x.FirstName ?? "",
                    LastName = x.LastName ?? "",
                    PhoneNumber = x.PhoneNumber ?? "",
                    ZipPostalCode = x.ZipPostalCode ?? "",
                })
                .ToList();

            var customerOrdersCount = customerOrders.Count(); // 100%
            var customersWithDifferentAddressesCount = 0; // ?%
            var differentAddressesList = new List<string>();
            foreach (var customer in customerOrders)
            {
                var billingAddressIds = customer.Select(x => x.BillingAddressId).ToList();
                var billingAddresses = addresses.Where(x => billingAddressIds.Contains(x.Id)).ToList();
                var shippingAddressIds = customer.Select(x => x.ShippingAddressId).ToList();
                var shippingAddresses = addresses.Where(x => billingAddressIds.Contains(x.Id)).ToList();
                foreach (var shippingAddresse in shippingAddresses)
                {
                    var differentAddresses = billingAddresses.Where(x =>
                        x.Address1.ToLower().Trim() != shippingAddresse.Address1.ToLower().Trim() ||
                        x.Address2.ToLower().Trim() != shippingAddresse.Address2.ToLower().Trim() ||
                        x.City.ToLower().Trim() != shippingAddresse.City.ToLower().Trim() ||
                        x.Email.ToLower().Trim() != shippingAddresse.Email.ToLower().Trim() ||
                        x.FirstName.ToLower().Trim() != shippingAddresse.FirstName.ToLower().Trim() ||
                        x.LastName.ToLower().Trim() != shippingAddresse.LastName.ToLower().Trim() ||
                        x.PhoneNumber.ToLower().Trim() != shippingAddresse.PhoneNumber.ToLower().Trim() ||
                        x.ZipPostalCode.ToLower().Trim() != shippingAddresse.ZipPostalCode.ToLower().Trim())
                        .ToList();
                    if (differentAddresses.Any())
                    {
                        customersWithDifferentAddressesCount++;
                        var billing = differentAddresses.FirstOrDefault();
                        var joinedBilling = string.Join(", ", new List<string> {
                            $"{billing.FirstName} {billing.LastName}",
                            billing.Address1,
                            billing.Address2,
                            billing.City,
                            billing.ZipPostalCode,
                            billing.Email,
                            billing.PhoneNumber,
                        }.Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x)));
                        var joinedShipping = string.Join(", ", new List<string> {
                            $"{shippingAddresse.FirstName} {shippingAddresse.LastName}",
                            shippingAddresse.Address1,
                            shippingAddresse.Address2,
                            shippingAddresse.City,
                            shippingAddresse.ZipPostalCode,
                            shippingAddresse.Email,
                            shippingAddresse.PhoneNumber,
                        }.Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x)));
                        differentAddressesList.Add($"{joinedBilling}\n{joinedShipping}\n");
                        break;
                    }
                }
            }

            var customersWithOnlyOneOrderCount = customersWithOnlyOneOrder.Count(); // 100%
            var customersWithOneOrderDifferentAddressesCount = 0; // ?%
            var differentAddressesOneOrderList = new List<string>();
            foreach (var customer in customersWithOnlyOneOrder)
            {
                var billingAddressIds = customer.Select(x => x.BillingAddressId).ToList();
                var billingAddresses = addresses.Where(x => billingAddressIds.Contains(x.Id)).ToList();
                var shippingAddressIds = customer.Select(x => x.ShippingAddressId).ToList();
                var shippingAddresses = addresses.Where(x => billingAddressIds.Contains(x.Id)).ToList();
                foreach (var shippingAddresse in shippingAddresses)
                {
                    var differentAddresses = billingAddresses.Where(x =>
                        x.Address1.ToLower().Trim() != shippingAddresse.Address1.ToLower().Trim() ||
                        x.Address2.ToLower().Trim() != shippingAddresse.Address2.ToLower().Trim() ||
                        x.City.ToLower().Trim() != shippingAddresse.City.ToLower().Trim() ||
                        x.Email.ToLower().Trim() != shippingAddresse.Email.ToLower().Trim() ||
                        x.FirstName.ToLower().Trim() != shippingAddresse.FirstName.ToLower().Trim() ||
                        x.LastName.ToLower().Trim() != shippingAddresse.LastName.ToLower().Trim() ||
                        x.PhoneNumber.ToLower().Trim() != shippingAddresse.PhoneNumber.ToLower().Trim() ||
                        x.ZipPostalCode.ToLower().Trim() != shippingAddresse.ZipPostalCode.ToLower().Trim())
                        .ToList();
                    if (differentAddresses.Any())
                    {
                        customersWithOneOrderDifferentAddressesCount++;
                        var billing = differentAddresses.FirstOrDefault();
                        var joinedBilling = string.Join(", ", new List<string> {
                            $"{billing.FirstName} {billing.LastName}",
                            billing.Address1,
                            billing.Address2,
                            billing.City,
                            billing.ZipPostalCode,
                            billing.Email,
                            billing.PhoneNumber,
                        }.Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x)));
                        var joinedShipping = string.Join(", ", new List<string> {
                            $"{shippingAddresse.FirstName} {shippingAddresse.LastName}",
                            shippingAddresse.Address1,
                            shippingAddresse.Address2,
                            shippingAddresse.City,
                            shippingAddresse.ZipPostalCode,
                            shippingAddresse.Email,
                            shippingAddresse.PhoneNumber,
                        }.Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x)));
                        differentAddressesOneOrderList.Add($"{joinedBilling}\n{joinedShipping}\n");
                        break;
                    }
                }
            }

            return Ok(
                $@"One or more completed orders
----------------

Customers total: {customerOrdersCount}.
Customers different addresses total: {customersWithDifferentAddressesCount}.
Porcentage: {((customersWithDifferentAddressesCount * 100) / customerOrdersCount)}%.

Listing:
{string.Join("\n", differentAddressesList)}
________________



Only one completed order
----------------

Customers total: {customersWithOnlyOneOrderCount}.
Customers different addresses total: {customersWithOneOrderDifferentAddressesCount}.
Porcentage: {((customersWithOneOrderDifferentAddressesCount * 100) / customersWithOnlyOneOrderCount)}%.

Listing:
{string.Join("\n", differentAddressesOneOrderList)}
________________"
                );
        }

        [HttpGet]
        public IActionResult GenerateExcel150()
        {
            var balances = _customerBalanceService.GetAllCustomerBalancesQuery()
                .Where(x => !x.Deleted).ToList();
            var customerIds = balances.Select(x => x.CustomerId).Distinct().ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => !string.IsNullOrEmpty(x.Email) && !x.Deleted && customerIds.Contains(x.Id))
                .ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Nombre";
                    worksheet.Cells[row, 2].Value = "Correo";
                    worksheet.Cells[row, 3].Value = "Teléfono";
                    worksheet.Cells[row, 4].Value = "Fecha de ultima entrega";
                    worksheet.Cells[row, 5].Value = "Fecha de ultima asignacion de saldo";
                    worksheet.Cells[row, 6].Value = "Saldo actual";

                    foreach (var customer in customers)
                    {
                        var currentBalance = _orderTotalCalculationService.GetBalanceTotal(customer);
                        if (currentBalance > 0)
                        {
                            row++;
                            worksheet.Cells[row, 1].Value = customer.GetFullName();
                            worksheet.Cells[row, 2].Value = customer.Email;
                            var phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                            worksheet.Cells[row, 3].Value = string.IsNullOrEmpty(phone) ?
                                customer.ShippingAddress?.PhoneNumber : phone;
                            worksheet.Cells[row, 4].Value = GetFilteredOrders().Where(x => x.CustomerId == customer.Id)
                                .OrderByDescending(x => x.SelectedShippingDate).FirstOrDefault()?.SelectedShippingDate;
                            worksheet.Cells[row, 4].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[row, 5].Value = balances.Where(x => x.CustomerId == customer.Id).OrderByDescending(x => x.UpdatedOnUtc).FirstOrDefault().UpdatedOnUtc.ToLocalTime();
                            worksheet.Cells[row, 5].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[row, 6].Value = currentBalance;
                            worksheet.Cells[row, 6].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_que_han_comprado_ciertos_productos.xlsx");
            }
        }


        [HttpGet]
        public IActionResult GenerateExcel151(int days = 120, bool isQuantity = false)
        {
            var today = DateTime.Now.Date;
            var controlDate = DateTime.Now.AddDays(-1 * days).Date;
            var orders = _orderService.GetAllOrdersQuery()
                .Where(x => !x.Deleted && x.SelectedShippingDate != null && controlDate <= x.SelectedShippingDate)
                .Select(x => new { x.Id, SelectedShippingDate = x.SelectedShippingDate.Value })
                .ToList();
            var orderIds = orders.Select(x => x.Id).Distinct().ToList();
            var orderItems = _orderService.GetAllOrderItemsQuery()
                .Where(x => orderIds.Contains(x.OrderId))
                .Select(x => new { x.OrderId, x.ProductId, x.Quantity, x.PriceInclTax })
                .ToList();
            var productIds = orderItems.Select(x => x.ProductId).Distinct().ToList();
            var products = _productService.GetAllProductsQuery()
                .Where(x => productIds.Contains(x.Id))
                .Select(x => new { x.Name, x.Id })
                .ToList();
            var orderGrouping = orderItems.GroupBy(x => new { x.OrderId, x.ProductId }).ToList();

            var datesBetween = Enumerable.Range(0, 1 + today.Subtract(controlDate).Days)
             .Select(offset => controlDate.AddDays(offset))
             .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Id";
                    worksheet.Cells[row, 2].Value = "Nombre";

                    var col = 3;
                    foreach (var date in datesBetween)
                    {
                        worksheet.Cells[row, col].Value = date.ToString("dd-MM-yyyy");
                        col++;
                    }

                    foreach (var product in products)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = product.Id;
                        worksheet.Cells[row, 2].Value = product.Name;

                        col = 3;
                        foreach (var date in datesBetween)
                        {
                            var orderIdsOfDay = orders.Where(x => x.SelectedShippingDate == date).Select(x => x.Id).ToList();
                            var orderItemsOfDay = orderGrouping.Where(x => x.Key.ProductId == product.Id && orderIdsOfDay.Contains(x.Key.OrderId))
                                .SelectMany(x => x.ToList())
                                .ToList();
                            var total = isQuantity ? orderItemsOfDay.Select(x => x.Quantity).DefaultIfEmpty().Sum() : orderItemsOfDay.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
                            worksheet.Cells[row, col].Value = total;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            col++;
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_productos_cantidad_vendida_ultimos_{days}_dias_por_{(isQuantity ? "cantidad" : "monto")}.xlsx");
            }
        }



        [HttpGet]
        public IActionResult GenerateExcel152(int year = 2022, string daysOfWeek = "4,5,6", bool? notThisWeek = null)
        {
            var today = DateTime.Now.Date;
            var controlDate = new DateTime(year, 1, 1);

            var daysOfWeeksInts = daysOfWeek.Split(',').Select(x => int.Parse(x)).ToList();
            var onlyDates = Enumerable.Range(0, 1 + today.Subtract(controlDate).Days)
             .Select(offset => controlDate.AddDays(offset))
             .Where(x => daysOfWeeksInts.Contains((int)x.DayOfWeek))
             .ToList();

            var thisMonday = StartOfWeek(DateTime.Now, DayOfWeek.Monday);
            var thisFriday = thisMonday.AddDays(5);
            var thisWeek = Enumerable.Range(0, 1 + thisFriday.Subtract(thisMonday).Days)
             .Select(offset => thisMonday.AddDays(offset))
             .ToList();
            var ordersThisWeek = OrderUtils.GetFilteredOrders(_orderService)
                    .Where(x => x.OrderStatusId == (int)OrderStatus.Complete && !x.Deleted && x.SelectedShippingDate != null &&
                        thisWeek.Contains(x.SelectedShippingDate.Value))
                    .Select(x => new TempModel { Id = x.Id, SelectedShippingDate = x.SelectedShippingDate.Value, CustomerId = x.CustomerId })
                    .ToList();
            var customerIdsThisWeek = ordersThisWeek.Select(x => x.CustomerId).Distinct().ToList();

            var orders = new List<TempModel>();
            if (notThisWeek == null)
                orders = OrderUtils.GetFilteredOrders(_orderService)
                    .Where(x => x.OrderStatusId == (int)OrderStatus.Complete && !x.Deleted && x.SelectedShippingDate != null &&
                        onlyDates.Contains(x.SelectedShippingDate.Value))
                    .Select(x => new TempModel { Id = x.Id, SelectedShippingDate = x.SelectedShippingDate.Value, CustomerId = x.CustomerId })
                    .ToList();
            else if (notThisWeek ?? false)
                orders = OrderUtils.GetFilteredOrders(_orderService)
                    .Where(x => x.OrderStatusId == (int)OrderStatus.Complete && !x.Deleted && x.SelectedShippingDate != null &&
                        onlyDates.Contains(x.SelectedShippingDate.Value) &&
                        !customerIdsThisWeek.Contains(x.CustomerId)
                        )
                    .Select(x => new TempModel { Id = x.Id, SelectedShippingDate = x.SelectedShippingDate.Value, CustomerId = x.CustomerId })
                    .ToList();
            else if (!(notThisWeek ?? false))
                orders = OrderUtils.GetFilteredOrders(_orderService)
                    .Where(x => x.OrderStatusId == (int)OrderStatus.Complete && !x.Deleted && x.SelectedShippingDate != null &&
                        onlyDates.Contains(x.SelectedShippingDate.Value) &&
                        customerIdsThisWeek.Contains(x.CustomerId)
                        )
                    .Select(x => new TempModel { Id = x.Id, SelectedShippingDate = x.SelectedShippingDate.Value, CustomerId = x.CustomerId })
                    .ToList();
            var customerIds = orders.Select(x => x.CustomerId).Distinct().ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => customerIds.Contains(x.Id))
                .ToList();
            var newsletter = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Nombre";
                    worksheet.Cells[row, 2].Value = "Correo";
                    worksheet.Cells[row, 3].Value = "Teléfono";
                    worksheet.Cells[row, 4].Value = "Fecha de última entrega";
                    worksheet.Cells[row, 5].Value = "Si están suscritos o no";

                    foreach (var customer in customers)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = customer.GetFullName();
                        worksheet.Cells[row, 2].Value = customer.Email;
                        var phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                        worksheet.Cells[row, 3].Value = string.IsNullOrEmpty(phone) ? customer.Addresses.Select(x => x.PhoneNumber).FirstOrDefault() : phone;
                        worksheet.Cells[row, 4].Value = OrderUtils.GetFilteredOrders(_orderService)
                            .Where(x => x.SelectedShippingDate != null && x.CustomerId == customer.Id)
                            .OrderByDescending(x => x.SelectedShippingDate.Value).FirstOrDefault()?.SelectedShippingDate.Value;
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 5].Value = newsletter.Where(x => x.Email == customer.Email).Select(x => x.Active).FirstOrDefault() ? "SI" : "NO";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_con_ordenes_jueves_viernes_sabados.xlsx");
            }
        }

        public DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        [HttpGet]
        public IActionResult GenerateExcel153()
        {
            var today = DateTime.UtcNow.Date;
            var neintyDaysAgo = today.AddDays(-90);
            var costsDecreaseChangesByDate = _costsDecreaseWarningService
                .GetAll().Where(x => neintyDaysAgo <= x.CreatedOnUtc)
                .Select(x => new { x.Id, CreatedOnUtc = DbFunctions.TruncateTime(x.CreatedOnUtc) })
                .ToList();
            var costsIncreaseChangesByDate = _costsIncreaseWarningService
                .GetAll().Where(x => neintyDaysAgo <= x.CreatedOnUtc)
                .Select(x => new { x.Id, CreatedOnUtc = DbFunctions.TruncateTime(x.CreatedOnUtc) })
                .ToList();

            var datesBetween = Enumerable.Range(0, 1 + today.Subtract(neintyDaysAgo).Days)
             .Select(offset => neintyDaysAgo.AddDays(offset))
             .Select(x => x)
             .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    var col = 1;
                    foreach (var date in datesBetween)
                    {
                        worksheet.Cells[1, col].Value = date.ToLocalTime().ToString("dd-MM-yyyy");
                        worksheet.Cells[2, col].Value = costsDecreaseChangesByDate.Where(x => x.CreatedOnUtc == date).Count() +
                            costsIncreaseChangesByDate.Where(x => x.CreatedOnUtc == date).Count();
                        col++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_con_ordenes_sabados.xlsx");
            }
        }

        [HttpGet]
        public IActionResult GenerateExcel154(int count = 100)
        {
            var ordersGroup = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate != null)
                .Select(x => new
                {
                    x.CustomerId,
                    x.OrderTotal,
                    x.SelectedShippingDate
                }).GroupBy(x => x.CustomerId)
                .OrderByDescending(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum())
                .Take(count)
                .ToList();

            var customerIds = ordersGroup.Select(x => x.Key).Distinct().ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => customerIds.Contains(x.Id)).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Nombre";
                    worksheet.Cells[row, 2].Value = "Email";
                    worksheet.Cells[row, 3].Value = "Celular";
                    worksheet.Cells[row, 4].Value = "Número de pedidos históricos";
                    worksheet.Cells[row, 5].Value = "Monto de venta histórico";
                    worksheet.Cells[row, 6].Value = "Recurrencia";
                    worksheet.Cells[row, 7].Value = "Fecha de último pedido";

                    foreach (var orderGroup in ordersGroup)
                    {
                        row++;

                        var customer = customers.Where(x => x.Id == orderGroup.Key).FirstOrDefault();
                        worksheet.Cells[row, 1].Value = customer.GetFullName();
                        worksheet.Cells[row, 2].Value = customer.Email;
                        var phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                        worksheet.Cells[row, 3].Value = string.IsNullOrEmpty(phone) ? customer.Addresses.Select(x => x.PhoneNumber).FirstOrDefault() : phone;
                        worksheet.Cells[row, 4].Value = orderGroup.Count();
                        worksheet.Cells[row, 5].Value = orderGroup.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 5].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";

                        var monthlyOrders = orderGroup.GroupBy(x => x.SelectedShippingDate.Value.ToString("MM-yyyy")).ToList();
                        var totalOrders = monthlyOrders.Select(x => x.Count()).DefaultIfEmpty().Sum();
                        var totalMonthlyDates = monthlyOrders.Count();
                        var recurrency = totalOrders / totalMonthlyDates;
                        worksheet.Cells[row, 6].Value = recurrency;
                        worksheet.Cells[row, 6].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";

                        worksheet.Cells[row, 7].Value = orderGroup.OrderByDescending(x => x.SelectedShippingDate).FirstOrDefault().SelectedShippingDate;
                        worksheet.Cells[row, 7].Style.Numberformat.Format = "dd-mm-yyyy";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_{count}_clientes_con_mayor_compra.xlsx");
            }
        }

        [HttpGet]
        public IActionResult GenerateExcel155(string coupon = "DAYPASS")
        {
            if (string.IsNullOrEmpty(coupon))
                return BadRequest();

            coupon = coupon.Trim().ToLower();
            var discount = _discountService.GetAllDiscounts(couponCode: coupon, showHidden: true)
                .FirstOrDefault();

            if (discount == null)
                return BadRequest();

            var ordersWithCoupon = _orderService.GetAllOrdersQuery()
                .Where(x => x.DiscountUsageHistory.Select(y => y.DiscountId).Contains(discount.Id))
                .OrderBy(x => x.Id)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Número de orden";
                    worksheet.Cells[row, 2].Value = "Estatus de la orden";
                    worksheet.Cells[row, 3].Value = "Nombre del cliente";
                    worksheet.Cells[row, 4].Value = "Correo del cliente";
                    worksheet.Cells[row, 5].Value = "Teléfono";
                    worksheet.Cells[row, 6].Value = "Total de la orden";
                    worksheet.Cells[row, 7].Value = "Subtotal de la orden";
                    worksheet.Cells[row, 8].Value = "Día de entrega";

                    foreach (var orderWithCoupon in ordersWithCoupon)
                    {
                        row++;

                        worksheet.Cells[row, 1].Value = orderWithCoupon.Id;
                        worksheet.Cells[row, 2].Value = orderWithCoupon.OrderStatus.GetDisplayName();
                        worksheet.Cells[row, 3].Value = orderWithCoupon.Customer.GetFullName();
                        worksheet.Cells[row, 4].Value = orderWithCoupon.Customer.Email;
                        var phone = orderWithCoupon.ShippingAddress.PhoneNumber;
                        worksheet.Cells[row, 5].Value = string.IsNullOrEmpty(phone) ? orderWithCoupon.Customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone) : phone;
                        worksheet.Cells[row, 6].Value = orderWithCoupon.OrderTotal;
                        worksheet.Cells[row, 6].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        worksheet.Cells[row, 7].Value = orderWithCoupon.OrderSubtotalInclTax;
                        worksheet.Cells[row, 7].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        worksheet.Cells[row, 8].Value = orderWithCoupon.SelectedShippingDate;
                        worksheet.Cells[row, 8].Style.Numberformat.Format = "dd-mm-yyyy";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ordenes_con_cupon_{coupon.Replace(" ", "_")}.xlsx");
            }
        }

        [HttpGet]
        public IActionResult GenerateExcel156()
        {
            var orderIds = new List<int>
            {
                // 92050, // Esta orden se reporgramo al 17 por sospecha de fraude.
                92048,
                92047,
                92046,
                92045,
                92041,
                92040,
                92039,
                92038,
                92037,
                92029,
                92028,
                92025,
                92024,
                // 92023, // La orden fue cancelada porque el cliente no quizo pagar la diferencia del mal uso del cupón.
                92020,
                92017,
                92014,
                92013,
                // 92010, // Esta orden se reporgramo al 17 por sospecha de fraude.
                92000,
                91989,
                91984,
                91977,
                91974,
                // 91970, // Se canceló por sospecha de fraude.
                91968,
                91967,
                91964,
                91961,
                ////
                90335,
                90334,
                78599,
                78239,
                77644,
                76891,
                76209,
                74374,
                73440,
                73439,
                73434,
                73433,
                73416,
                73414,
                71161,
                70410,
                69654,
                53892,
                53890,
                53887,
                53886,
                53881,
                53880,
                53878,
                53873,
                53860,
                53857,
                53850,
                53849,
                53847,
                53845,
                53838,
                53836,
                53834,
                53832,
                53825,
                53823,
                53814,
                53813,
                53805,
                53803,
                53755,
                53566,
                53500,
                53302,
                53270,
            }.Distinct().ToList();

            var orders = _orderService.GetAllOrdersQuery()
                .Where(x => orderIds.Contains(x.Id))
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Fecha de entrega";
                    worksheet.Cells[row, 2].Value = "Número de orden";
                    worksheet.Cells[row, 3].Value = "Monto de la orden";
                    worksheet.Cells[row, 4].Value = "Cliente";
                    worksheet.Cells[row, 5].Value = "Email";
                    worksheet.Cells[row, 6].Value = "Número teléfonico";

                    foreach (var order in orders)
                    {
                        row++;

                        worksheet.Cells[row, 1].Value = order.SelectedShippingDate.Value;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 2].Value = order.Id;
                        worksheet.Cells[row, 3].Value = order.OrderTotal;
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        worksheet.Cells[row, 4].Value = order.Customer.GetFullName();
                        worksheet.Cells[row, 5].Value = order.Customer.Email;
                        var phone = order.ShippingAddress.PhoneNumber;
                        worksheet.Cells[row, 6].Value = string.IsNullOrEmpty(phone) ? order.Customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone) : phone;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ordenes_error_visa.xlsx");
            }
        }

        [HttpGet]
        public IActionResult GenerateExcel157()
        {
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.OrderStatusId != (int)OrderStatus.Cancelled &&
                    x.SelectedShippingDate != null)
                .Select(x => new { x.SelectedShippingDate, x.OrderTotal, x.PaymentMethodSystemName })
                .OrderBy(x => x.SelectedShippingDate)
                .ToList();

            var firstOrderDate = orders.FirstOrDefault().SelectedShippingDate.Value;
            var lastOrderDate = orders.LastOrDefault().SelectedShippingDate.Value;
            var datesBetween = Enumerable.Range(0, 1 + lastOrderDate.Subtract(firstOrderDate).Days)
             .Select(offset => firstOrderDate.AddDays(offset))
             .OrderBy(x => x)
             .GroupBy(x => x.ToString("MM/yyyy"))
             .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int col = 2;

                    foreach (var date in datesBetween)
                    {
                        worksheet.Cells[row, col].Value = date.Key;
                        col++;
                    }

                    var paymentMethodGroup = orders.GroupBy(x => x.PaymentMethodSystemName).ToList();
                    foreach (var ordersOfPayment in paymentMethodGroup)
                    {
                        col = 1;
                        row++;
                        var name = "Saldo";
                        var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(ordersOfPayment.Key);
                        if (paymentMethod != null)
                            name = paymentMethod.PluginDescriptor.FriendlyName;
                        worksheet.Cells[row, col].Value = $"{name} ({ordersOfPayment.Key})";
                        foreach (var date in datesBetween)
                        {
                            col++;
                            var ordersOfDate = ordersOfPayment
                                .Where(x => x.SelectedShippingDate.Value.ToString("MM/yyyy") == date.Key)
                                .ToList();
                            worksheet.Cells[row, col].Value = ordersOfDate.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_venta_por_metodo_de_pago_mensualmente.xlsx");
            }
        }

        [HttpGet]
        public IActionResult GenerateExcel158()
        {
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.OrderStatusId != (int)OrderStatus.Cancelled &&
                    x.SelectedShippingDate != null)
                .Select(x => new { x.SelectedShippingDate, x.OrderTotal, x.PaymentMethodSystemName })
                .OrderBy(x => x.SelectedShippingDate)
                .ToList();

            var firstOrderDate = orders.FirstOrDefault().SelectedShippingDate.Value;
            var lastOrderDate = orders.LastOrDefault().SelectedShippingDate.Value;
            var datesBetween = Enumerable.Range(0, 1 + lastOrderDate.Subtract(firstOrderDate).Days)
             .Select(offset => firstOrderDate.AddDays(offset))
             .OrderBy(x => x)
             .GroupBy(x => x.ToString("MM/yyyy"))
             .ToList();

            var orderReports = _orderReportService.GetAll()
                .Where(x => !x.Deleted)
                .Select(x => new
                {
                    x.OrderShippingDate,
                    x.Quantity,
                    x.UpdatedRequestedQtyCost,
                    x.OrderId,
                    x.OrderItemId,
                    x.ProductId,
                    x.ManufacturerId
                })
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int col = 2;

                    foreach (var date in datesBetween)
                    {
                        worksheet.Cells[row, col].Value = date.Key;
                        col++;
                    }

                    var manufacturers = _manufacturerService.GetAllManufacturers()
                        .Where(x => !x.Deleted)
                        .Select(x => new { x.Id, x.Name, x.IsPaymentWhithTransfer })
                        .ToList();

                    var payment1 = new
                    {
                        Name = "Pago en efectivo",
                        Manufacturers = manufacturers.Where(x => !x.IsPaymentWhithTransfer).ToList()
                    };
                    var payment2 = new
                    {
                        Name = "Pago con transferencia",
                        Manufacturers = manufacturers.Where(x => x.IsPaymentWhithTransfer).ToList()
                    };
                    var payments = new[] { payment1, payment2 }.ToList();
                    foreach (var payment in payments)
                    {
                        col = 1;
                        row++;
                        worksheet.Cells[row, col].Value = payment.Name;
                        foreach (var date in datesBetween)
                        {
                            col++;
                            var manufacturerIds = payment.Manufacturers.Select(x => x.Id).ToList();
                            var reportsOfDate = orderReports
                                .Where(x => manufacturerIds.Contains(x.ManufacturerId ?? 0) && x.OrderShippingDate.ToString("MM/yyyy") == date.Key)
                                .ToList();
                            worksheet.Cells[row, col].Value = reportsOfDate.Select(x => x.UpdatedRequestedQtyCost).DefaultIfEmpty().Sum();
                            worksheet.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_venta_por_metodo_de_pago_mensualmente.xlsx");
            }
        }


        [HttpGet]
        public IActionResult GenerateExcel159()
        {
            var couponCodes = new List<string>
            {
                "TAGV5TQ",
                "TA42UZO",
                "TAV8UJ8",
                "TA31FV3",
                "TASE4ZW",
                "TALAAFM",
                "TA5WYD9",
                "TAXY0AW",

                "TAKHG0D",
                "TAJPX6Z",
                "TATJWNT",
                "TA2PQ9J",
                "TA41PNT",
                "TA8JRKK",
            };

            var discounts = _discountService.GetAllDiscounts(showHidden: true)
                .Where(x => couponCodes.Contains((x.CouponCode ?? "").Trim().ToUpper()))
                .OrderBy(x => x.Name)
                .ToList();
            var discountIds = discounts.Select(x => x.Id).ToList();
            var discountUsage = _discountService.GetAllDiscountUsageHistoryQuery()
                .Where(x => discountIds.Contains(x.DiscountId))
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Nómbre del cupón";
                    worksheet.Cells[row, 2].Value = "Código del cupón";
                    worksheet.Cells[row, 3].Value = "Veces usado";

                    foreach (var discount in discounts)
                    {
                        row++;

                        worksheet.Cells[row, 1].Value = discount.Name;
                        worksheet.Cells[row, 2].Value = discount.CouponCode;
                        worksheet.Cells[row, 3].Value = discountUsage.Where(x => x.DiscountId == discount.Id).Count();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_codigos_cupones_especificados_utilizados.xlsx");
            }
        }


        [HttpGet]
        public IActionResult GenerateExcel160(int xDaysAgo = 15)
        {
            var today = DateTime.Now.Date;
            var fifteenDaysAgo = today.AddDays(-1 * xDaysAgo);
            var customerBalances = _customerBalanceService.GetAllCustomerBalancesQuery()
                .Where(x => !x.Deleted && x.CreatedOnUtc <= fifteenDaysAgo)
                .GroupBy(x => x.CustomerId)
                .ToList();
            var customerIds = customerBalances.Select(x => x.Key).Distinct().ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => customerIds.Contains(x.Id))
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Nómbre";
                    worksheet.Cells[row, 2].Value = "Correo";
                    worksheet.Cells[row, 3].Value = "Telefono";
                    worksheet.Cells[row, 4].Value = "Balance actual";
                    worksheet.Cells[row, 5].Value = "Fecha de ultimo abono";
                    worksheet.Cells[row, 6].Value = "Fecha de ultima orden";

                    foreach (var customer in customers)
                    {
                        var customerBalanceInfo = customerBalances.Where(x => x.Key == customer.Id).FirstOrDefault();
                        if (customerBalanceInfo != null)
                        {
                            var customerBalance = customerBalanceInfo.Select(x => x.Amount).DefaultIfEmpty().Sum();
                            var ordersWithBalanceUsed = _orderService.GetAllOrdersQuery().Where(x => x.OrderStatusId != 40)
                                .Where(x => x.CustomerId == customer.Id && x.CustomerBalanceUsedAmount > 0)
                                .Select(x => x.CustomerBalanceUsedAmount ?? 0)
                                .DefaultIfEmpty().Sum();
                            var currentBalance = customerBalance - ordersWithBalanceUsed;
                            if (currentBalance >= 50)
                            {
                                row++;

                                worksheet.Cells[row, 1].Value = customer.GetFullName();
                                worksheet.Cells[row, 2].Value = customer.Email;
                                var phone = customer.ShippingAddress?.PhoneNumber;
                                worksheet.Cells[row, 3].Value = string.IsNullOrEmpty(phone) ? customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone) : phone;
                                worksheet.Cells[row, 4].Value = currentBalance;
                                worksheet.Cells[row, 4].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                                worksheet.Cells[row, 5].Value = customerBalanceInfo.OrderByDescending(x => x.CreatedOnUtc).FirstOrDefault().CreatedOnUtc.ToLocalTime();
                                worksheet.Cells[row, 5].Style.Numberformat.Format = "dd-mm-yyyy";
                                worksheet.Cells[row, 6].Value = _orderService.GetAllOrdersQuery().Where(x => x.OrderStatusId != 40 && x.CustomerId == customer.Id)
                                .OrderByDescending(x => x.SelectedShippingDate).FirstOrDefault()?.SelectedShippingDate;
                                worksheet.Cells[row, 6].Style.Numberformat.Format = "dd-mm-yyyy";
                            }
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_con_balance_previo_a_los_{xDaysAgo}_dias.xlsx");
            }
        }

        [HttpGet]
        public IActionResult GenerateExcel161()
        {
            var roles = _customerService.GetAllCustomerRoles(showHidden: true)
                .OrderBy(x => x.Name)
                .ToList();
            var access = _permissionService.GetAllPermissionRecords().ToList();
            var employeeRoles = roles.Where(x => x.SystemName == "employee" || x.SystemName == "exemployee")
                .Select(x => x.Id).ToArray();
            var employees = _customerService.GetAllCustomers(customerRoleIds: employeeRoles)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Nómbre del rol";
                    worksheet.Cells[row, 2].Value = "Accesos del rol";
                    worksheet.Cells[row, 3].Value = "Usuarios con el rol";

                    foreach (var role in roles)
                    {
                        row++;
                        var accessOfRole = access.Where(x => x.CustomerRoles.Select(y => y.Id).Contains(role.Id)).Select(x => x.Name).ToList();
                        var customersOfRole = employees.Where(x => x.CustomerRoles.Select(y => y.Id).Contains(role.Id))
                            .Select(x => x.GetFullName()).ToList();

                        worksheet.Cells[row, 1].Value = $"{role.Name} ({role.SystemName})";
                        worksheet.Cells[row, 2].Value = string.Join("\n", accessOfRole);
                        worksheet.Cells[row, 2].Style.WrapText = true;
                        worksheet.Cells[row, 3].Value = string.Join("\n", customersOfRole);
                        worksheet.Cells[row, 3].Style.WrapText = true;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_roles_accesos_y_usuarios.xlsx");
            }
        }


        [HttpGet]
        public IActionResult GenerateExcel162(int customerId, string PaymentMethodSystemName)
        {
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.CustomerId == customerId &&
                x.PaymentMethodSystemName == PaymentMethodSystemName &&
                x.SelectedShippingDate != null)
                .Select(x => new
                {
                    x.Id,
                    SelectedShippingDate = x.SelectedShippingDate.Value,
                    x.OrderTotal
                })
                .ToList()
                .GroupBy(x => x.SelectedShippingDate.ToString("MM/yyyy"))
                .ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Mes";
                    worksheet.Cells[row, 2].Value = "Monto total";
                    worksheet.Cells[row, 3].Value = "Órdenes";

                    foreach (var order in orders)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = order.Key;
                        worksheet.Cells[row, 2].Value = order.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 3].Value = string.Join(", ", order.Select(x => x.Id).ToList());
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_roles_accesos_y_usuarios.xlsx");
            }
        }

        [HttpGet]
        public IActionResult GenerateExcel163(int days = 30)
        {
            var todayUtc = DateTime.UtcNow.Date;
            var daysAgoUtc = todayUtc.AddDays(-1 * days);
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => daysAgoUtc <= x.CreatedOnUtc && x.CreatedOnUtc <= todayUtc && x.Email != null)
                .ToList();
            var newsletter = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Id";
                    worksheet.Cells[row, 2].Value = "Nombre";
                    worksheet.Cells[row, 3].Value = "Apellido";
                    worksheet.Cells[row, 4].Value = "Género";
                    worksheet.Cells[row, 5].Value = "Correo";
                    worksheet.Cells[row, 6].Value = "Creado en";
                    worksheet.Cells[row, 7].Value = "Suscrito a newsletter";

                    foreach (var customer in customers)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = customer.Id;
                        worksheet.Cells[row, 2].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                        worksheet.Cells[row, 3].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
                        worksheet.Cells[row, 4].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender);
                        worksheet.Cells[row, 5].Value = customer.Email;
                        worksheet.Cells[row, 6].Value = customer.CreatedOnUtc.ToLocalTime();
                        worksheet.Cells[row, 6].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 7].Value = newsletter.Where(x => x.Email == customer.Email).Select(x => x.Active).FirstOrDefault() ? "SI" : "NO";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_usuarios_creados_ultimos_{days}_dias.xlsx");
            }
        }

        [HttpGet]
        public IActionResult GenerateExcel164(int days = 30)
        {
            var postalCodes = new List<string>
            {
                "04040",
                "04330",
                "04370",
                "04030",
                "04200",
                "04120",
                "04330",
                "04210",
                "04400",
                "04450",
                "11800",
                "03810",
                "03103",
                "06100",
                "03100",
                "03200",
                "03720",
                "03740",
                "06140",
                "06700",
                "06100",
                "06600",
            };
            var todayUtc = DateTime.UtcNow.Date;
            var daysAgoUtc = todayUtc.AddDays(-1 * days);
            var ordersByPostalCode = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => daysAgoUtc <= x.SelectedShippingDate && x.SelectedShippingDate <= todayUtc)
                .Select(x => new { x.Id, x.ShippingAddress })
                .ToList()
                .Where(x => postalCodes.Contains(x.ShippingAddress.ZipPostalCode?.Trim()))
                .GroupBy(x => x.ShippingAddress.ZipPostalCode)
                .OrderByDescending(x => x.Count())
                .ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Código postal";
                    worksheet.Cells[row, 2].Value = "Cantidad";

                    foreach (var orders in ordersByPostalCode)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = orders.Key;
                        worksheet.Cells[row, 2].Value = orders.Count();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ventas_por_codigo_postal_{days}_dias.xlsx");
            }
        }

        [HttpGet]
        public IActionResult GenerateExcel165()
        {
            var orderIds = new List<int>
            {
                93690,
93701,
93702,
93733,
93741,
93744,
93750,
93752,
93756,
93757,
93762,
93764,
93765,
93794,
93797,
93800,
93801,
93802,
93805,
93821,
93826,
93832,
93833,
93837,
93838,
93841,
93844,
93851,
93853,
93854,
93856,
93858,
93862,
93871,
93883,
93884,
93886,
93889,
93891,
93895,
93897,
93900,
93904,
93905,
93907,
93909,
93910,
93912,
93913,
93923,
93924,
93936,
93946,
93950,
93958,
93960,
93961,
93964,
93966,
93969,
93974,
93975,
93982,
93989,
93991,
93992,
93998,
94003,
94007,
94013,
94020,
94027,
94028,
94029,
94031,
94033,
94042,
94043,
94044,
94048,
94053,
94057,
94062,
94065,
94066,
94070,
94075,
94076,
94085,
94086,
94093,
94099,
94105,
94107,
94114,
94117,
94121,
94124,
94125,
94129,
94136,
94138,
94143,
94145,
94150,
94159,
94160,
            };
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => orderIds.Contains(x.Id))
                .Select(x => new { x.Id, x.OrderItems, x.DiscountUsageHistory, x.OrderDiscount, x.OrderTotal })
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "# orden";
                    worksheet.Cells[row, 2].Value = "Cupones usados";
                    worksheet.Cells[row, 3].Value = "Total descontado";
                    worksheet.Cells[row, 4].Value = "Total de la órden";

                    foreach (var order in orders)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = order.Id;
                        worksheet.Cells[row, 2].Value = string.Join(", ", order.DiscountUsageHistory.Select(x => x.Discount?.Name).ToList());
                        worksheet.Cells[row, 3].Value = order.OrderDiscount +
                            order.OrderItems.Select(x => x.DiscountAmountInclTax).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        worksheet.Cells[row, 4].Value = order.OrderTotal;
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ordenes_especificas_y_uso_de_cupones_con_totales.xlsx");
            }
        }

        [HttpGet]
        public IActionResult GenerateExcel166()
        {
            var products = _productService.GetAllProductsQuery()
                .Where(x => !x.Deleted)
                .Select(x => new { x.Id, x.Name, x.ProductCategories, x.IsTaxExempt, x.TaxCategoryId })
                .OrderBy(x => x.Name)
                .ToList();
            var taxs = _taxCategoryService.GetAllTaxCategories().ToList();
            var categories = _categoryService.GetAllCategories(showHidden: true)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Id";
                    worksheet.Cells[row, 2].Value = "Categoría padre";
                    worksheet.Cells[row, 3].Value = "Categoría  hijo";
                    worksheet.Cells[row, 4].Value = "Nombre del producto";
                    worksheet.Cells[row, 5].Value = "IVA";

                    foreach (var product in products)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = product.Id;
                        var productCategory = product.ProductCategories.FirstOrDefault();
                        var mainCategory = productCategory?.Category?.Name;
                        var childCategory = "---";
                        if (productCategory?.Category?.ParentCategoryId > 0)
                        {
                            childCategory = mainCategory;
                            mainCategory = categories.Where(x => x.Id == productCategory?.Category?.ParentCategoryId).FirstOrDefault()?.Name;
                        }
                        worksheet.Cells[row, 2].Value = mainCategory;
                        worksheet.Cells[row, 3].Value = childCategory;
                        worksheet.Cells[row, 4].Value = product.Name;
                        worksheet.Cells[row, 5].Value = !product.IsTaxExempt && product.TaxCategoryId > 0 ? 16 : 0;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_todos_productos.xlsx");
            }
        }


        [HttpGet]
        public IActionResult GenerateExcel167(int year = 2022)
        {
            var balances = _customerBalanceService.GetAllCustomerBalancesQuery()
                .Where(x => x.CreatedOnUtc.Year == year)
                .OrderBy(x => x.CreatedOnUtc)
                .ToList();
            var customerIds = balances.Select(x => x.CustomerId).Distinct().ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => customerIds.Contains(x.Id)).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Cliente";
                    worksheet.Cells[row, 2].Value = "Saldo abonado";
                    worksheet.Cells[row, 3].Value = "Motivo";
                    worksheet.Cells[row, 4].Value = "Fecha";

                    foreach (var balance in balances)
                    {
                        row++;
                        var customer = customers.Where(x => x.Id == balance.CustomerId).FirstOrDefault();
                        worksheet.Cells[row, 1].Value = customer.GetFullName() + $" ({customer.Email})";
                        worksheet.Cells[row, 2].Value = balance.Amount;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        worksheet.Cells[row, 3].Value = balance.Comment;
                        worksheet.Cells[row, 4].Value = balance.CreatedOnUtc.ToLocalTime();
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "dd-mm-yyyy";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_balances_dados_año_{year}.xlsx");
            }
        }


        [HttpGet]
        public IActionResult GenerateExcel168(string couponSufix = "Septiembre 2022")
        {
            var discounts = _discountService.GetAllDiscounts()
                .Where(x => x.Name?.ToLower()?.Contains(couponSufix.ToLower()) ?? false)
                .ToList();
            var discountIds = discounts.Select(x => x.Id).ToList();
            var discountUsage = _discountService.GetAllDiscountUsageHistoryQuery()
                .Where(x => discountIds.Contains(x.DiscountId)).ToList();
            var orderIds = discountUsage.Select(x => x.OrderId).Distinct().ToList();
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => orderIds.Contains(x.Id))
                .Select(x => new {
                    x.Id,
                    x.DiscountUsageHistory,
                    x.OrderItems,
                    x.OrderTotal,
                    OrderDiscount = x.OrderSubTotalDiscountInclTax,
                })
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "# orden";
                    worksheet.Cells[row, 2].Value = "Cupones usados";
                    worksheet.Cells[row, 3].Value = "Total descontado";
                    worksheet.Cells[row, 4].Value = "Total de la órden";

                    foreach (var order in orders)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = order.Id;
                        worksheet.Cells[row, 2].Value = string.Join(", ", order.DiscountUsageHistory.Select(x => x.Discount?.Name).ToList());
                        worksheet.Cells[row, 3].Value = order.OrderDiscount +
                            order.OrderItems.Select(x => x.DiscountAmountInclTax).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        worksheet.Cells[row, 4].Value = order.OrderTotal;
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_info_ordenes_con_sufijo_de_cupones_{couponSufix.ToLower().Trim().Replace(" ", "-")}.xlsx");
            }
        }


        [HttpGet]
        public IActionResult GenerateExcel169()
        {
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.PaymentMethodSystemName == "Payments.Benefits")
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "# orden";
                    worksheet.Cells[row, 2].Value = "Cliente";
                    worksheet.Cells[row, 3].Value = "Total de la órden";

                    foreach (var order in orders)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = order.Id;
                        worksheet.Cells[row, 2].Value = order.Customer.GetFullName() + $" ({order.Customer.Email})";
                        worksheet.Cells[row, 3].Value = order.OrderTotal;
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_benefits_orders.xlsx");
            }
        }


        [HttpGet]
        public IActionResult GenerateExcel170()
        {
            var emails = new List<string>
            {
                "yunes1@prodigy.net.mx"
, "bertha.scripta@gmail.com"
, "alejandra.pineda.villegas@gmail.com"
, "songie.facturacion@hotmail.com"
, "karinavillanueva3@hotmail.com"
, "andrearga@gmail.com"
, "nanutravesi@yahoo.com.mx"
, "Coh59@hotmail.com"
, "zugofe@gmail.com"
, "p_dbc@yahoo.com.mx"
, "acuevas_salud@hotmail.com"
, "naneth.l@gmail.com"
, "elblogdemileinfinita@gmail.com"
, "zay_o95@hotmail.com"
, "sildabia28@hotmail.com"
, "canelebakery@gmail.com"
, "rusips@hotmail.com"
, "giannavista@yahoo.com"
, "miland_2@live.com.mx"
, "lmlg_yo@hotmail.com"
, "mich.ramirez.lopez@gmail.com"
, "anahidia@hotmail.com"
, "kyrastorm24@hotmail.com"
, "joneibarrola@yahoo.com"
, "paguilar@teed.com.mx"
, "silvana929@gmail.com"
, "diana.lau.cruz10@gmail.com"
, "lual_4@hotmail.com"
, "luceroreg@gmail.com"
, "alejandra.aoc@hotmail.com"
, "admon.cgdos@outlook.com"
, "martinrubenlopez32@gmail.com"
, "itzzsoflores@gmail.com"
, "burgekas@gmail.com"
, "sonny8109@yahoo.com.mx"
, "carjona@teed.com.mx"
, "adeiana_sepulveda@hotmail.com"
, "edpa0709@yahoo.com.mx"
, "lilian.villicana@gmail.com"
, "ximenahleon@hotmail.com"
, "airammc@outlook.es"
, "perlam_torreso@hotmail.com"
, "jdlamora@aol.com"
, "raleza@hotmail.com"
, "irlandahrdzama@yahoo.com.mx"
, "jacque1209@hotmail.com"
, "makachis1119@gmail.com"
, "andreabcg@gmail.com"
, "gabybautista5@gmail.com"
, "camilaopc@hotmail.com"
, "f.hernandez.ramirez@gmail.com"
, "margotmer@yahoo.com"
, "anahnmpl@gmail.com"
, "perlajass@gmail.com"
, "Mledesma@centrogeo.edu.mx"
, "sergheinicodemo@gmail.com"
, "navarroariadna@yahoo.com.mx"
, "daylagalvan@gmail.com"
, "shinics@hotmail.com"
, "solperezj@gmail.com"
, "martha.aizpuru@gmail.com"
, "suzzana-pg@hotmail.com"
, "lilopecue@yahoo.com"
, "paturusa2000@yahoo.com"
, "moisesgarciasantiago41@gmail.com"
, "mercadomaru@hotmail.com"
, "addie.zitro@gmail.com"
, "lunajian527@gmail.com"
, "patriciaizquierdo@me.com"
, "cindy.lpz14@gmail.com"
, "glicona13@outlook.com"
, "veronica.pequis@gmail.com"
, "yahaira_ac@yahoo.com"
, "jhus23@hotmail.com"
, "ricardoharte@yahoo.com.mx"
, "lesliemazoch@gmail.com"
, "manesillac@gmail.com"
, "vargasmartinezg@gmail.com"
, "mayelatorresfacu@gmail.com"
, "olguinpatricia.79@gmail.com"
, "juliocrojasg1965@gmail.com"
, "e.mireles1969@gmail.com"
, "aragonsanan@gmail.com"
, "lilli087@hotmail.com"
, "leanpineiro@gmail.com"
, "azu.leal7@gmail.com"
, "ediaz.tam@gmail.com"
, "manuelvmmp@gmail.com"
, "shakertonic@hotmail.com"
, "lunahoffman@hotmail.com"
, "eulaliambg@gmail.com"
, "jose.gotes@gmail.com"
, "nardavelazquez@gmail.com"
, "rvn_123@hotmail.com"
, "alcast42@hotmail.com"
, "mauriciojrsilva@gmail.com"
, "patricia.castillot@live.com"
, "adriarizd@hotmail.com"
, "monagarciae@gmail.com"
, "hectorterrazas209@gmail.com"
, "gladsandoval@gmail.com"
, "dmbelmonte@icloud.com"
, "moanin_13@hotmail.com"
, "sol.gutierrez.mon@gmail.com"
, "anievizcarrab@gmail.com"
, "gloria.gasc70@gmail.com"
, "isamo22@hotmail.com"
, "nmercedes.suarez@hotmail.com"
, "danielperezjb89@gmail.com"
, "ivette_molina@hotmail.com"
, "claudhel.navarrete@gmail.com"
, "nubbyliliana@hotmail.com"
, "ramitos_rocio@hotmail.com"
, "lyrykopyp@gmail.com"
, "pazgarcaidiego@gmail.com"
, "nataliakuri@hotmail.com"
, "adriana_paola@hotmail.com"
, "vallerrasalt@gmail.com"
, "palomavazquezgomez@gmail.com"
, "silviavidales@hotmail.com"
, "granados.f.claudia@gmail.com"
, "guizar.diana@gmail.com"
, "teresarod@gmail.com"
, "lourdes.semaan@gmail.com"
, "mafafrausto7@gmail.com"
, "arpttt@hotmail.com"
, "emilpr82@gmail.com"
, "luciacb.di@gmail.com"
, "monicabarrosomx@gmail.com"
, "vientohablo@gmail.com"
, "cmichelortega@gmail.com"
, "oscar.vazvio.mx@gmail.com"
, "ale-eventos1@hotmail.com"
, "krtos231@gmail.com"
, "raymundomasse@hotmail.com"
, "gameza5@gmail.com"
, "jaqueline1309@gmail.com"
, "francodonati@hotmail.com"
, "dgante.mn@gmail.com"
, "samarahtopi@yahoo.com.mx"
, "jendley9@gmail.com"
, "vemi82@gmail.com"
, "claor@hotmail.com"
, "kenitagalvan@hotmail.com"
, "ladnigal@gmail.com"
, "lecoza13@gmail.com"
, "sandsambreach@hotmail.com"
, "michsoftster@gmail.com"
, "Claudiadgc@hotmail.com"
, "betto_cml@hotmail.com"
, "carolona.hdz.w@gmail.com"
, "alziram@yahoo.com"
, "r_realme@hotmail.com"
, "carolina.hdz.w@gmail.com"
, "eriveroo91@gmail.com"
, "rebecaalvarezmora@gmail.com"
, "vaniaceron@gmail.com"
, "sanchezsanchez.mars@gmail.com"
, "ale_carcamo@hotmail.com"
, "yzznaranjo@gmail.com"
, "ellasenescena@gmail.com"
, "monicavv11@hotmail.com"
, "ximenagtt@gmail.com"
, "milu_carvajal@hotmail.com"
, "gibran.vilchis24@gmail.com"
, "afentanesj@gmail.com"
, "ang_castaneda@yahoo.com"
, "riluca.br@gmail.com"
, "danaesegovia.9@gmail.com"
, "bernardofdza@gmail.com"
, "jcrbautista3@hotmail.com"
, "atenea.caballero@gmail.com"
, "ileanallanas@hotmail.com"
, "urbina.jav@hotmail.com"
, "denis-89@hotmail.com"
, "lauran.duartem@gmail.com"
, "dpantoja@atencion.com"
, "gabyfi61@hotmail.com"
, "valitoxd@gmail.com"
, "paloma_garcia00@hotmail.com"
, "marieltanairy@hotmail.com"
, "nashpm@hotmail.com"
, "emoreno_82@hotmail.com"
, "cristizara@gmail.com"
, "camanidaniela@gmail.com"
, "rocharobledo@gmail.com"
, "ceci9995@hotmail.com"
, "ahwences91@gmail.com"
, "tania.gabriela4@gmail.com"
, "xanicazul@gmail.com"
, "jussell86@hotmail.com"
, "gama060994@gmail.com"
, "lopsmariana07@gmail.com"
, "pau.valen.pv@hotmail.com"
, "gonzasestopal@gmail.com"
, "alex.scabies@gmail.com"
, "cpa2808@hotmail.com"
, "ana.p.bautista.h@outlook.com"
, "maggie.aleon@gmail.com"
, "israel.amigon@patrimorent.com"
, "bjoyce74@gmail.com"
, "majo.perez.boeneker@gmail.com"
, "fenano.fdj@gmail.com"
, "brendabereniice@hotmail.com"
, "caguilar@agbasc.com.mx"
, "karenpzm@hotmail.com"
, "marianasalamanca13@gmail.com"
, "rickie74@hotmail.com"
, "dianaqueso26@gmail.com"
, "dany.y.edna@gmail.com"
, "carmen.amezcua@gmail.com"
, "astrosuarez8@gmail.com"
, "guadarrama.mfgg@gmail.com"
, "paulinnarivera@gmail.com"
, "ferid9315@icloud.com"
, "ismazuro01@gmail.com"
, "chebecky@gmail.com"
, "dayanaydg@gmail.com"
, "erosa.sandrap@gmail.com"
, "rebecaa_jm@hotmail.com"
, "bat0805@hotmail.com"
, "ing.textil.mrpm@hotmail.es"
, "enerragde@hotmail.com"
, "karenflawer@hotmail.com"
, "paulinatlalpanaguilar@gmail.com"
, "symondslillian@gmail.com"
, "ny.cruz1@gmail.com"
, "jhcarlosgo@gmail.com"
, "rebraco@hotmail.com"
, "psic.josecarrillo@gmail.com"
, "evelynbernalc@gmail.com"
, "jennyamkie4@gmail.com"
, "irasemat@hotmail.com"
, "laural@infinitummail.com"
, "Alejandrolopes2020@outlook.com"
, "pauletteshalom@gmail.com"
, "samiasilva2000@gmail.com"
, "renatavomendd@outlook.com"
, "mariela.cesareo@gmail.com"
, "robertvela22@gmail.com"
, "letiosornio10@gmail.com"
, "deysi.reyes18@gmail.com"
, "thelmalandia01@gmail.com"
, "claumartini@yahoo.com"
, "did2817@yahoo.com"
, "karinas797@gmail.com"
, "sandyschs@gmail.com"
, "colibri.29@hotmail.com"
, "suzettec@webgang.mx"
, "gabriel.delcampogonzalez@gmail.com"
, "prinzezita1309@hotmail.com"
, "malmanzadelgado@yahoo.com.mx"
, "paz_itas@hotmail.com"
, "elizabethconzyth@gmail.com"
, "loops.rdl@gmail.com"
, "nadia.bleuciel@gmail.com"
, "kamunozte@outlook.com"
, "estebanwi@msn.com"
, "angelika371@gmail.com"
, "av1090@hotmail.com"
, "sofirios10002@gmail.com"
, "claguilar@prodigy.net.mx"
, "lorelein_73@hotmail.com"
, "josuet.godinez@hotmail.com"
, "dra.alor.pediatra@gmail.com"
, "naycit.wonder@gmail.com"
, "ruimej0712@gmail.com"
, "danylosan@hotmmail.com"
, "nayesnu@hotmail.com"
, "maritegutierrez88@gmail.com"
, "ricardotapia98@gmail.com"
, "arlette_vasquez@hotmail.com"
, "alfonso_ortega@hotmail.com"
, "cmontesinosg@gmail.com"
, "hildaguz_1@hotmail.com"
, "spulido@centralenlinea.com"
, "gfrgfr1@gmail.com"
, "diana.r.g@outlook.es"
, "victor.manuel.rangel@gmail.com"
, "ranobich_azul@hotmail.com"
, "matmon91@gmail.com"
, "lizarrmarlenne@gmail.com"
, "riverasotog@yahoo.com.mx"
, "sharonhervill@gmail.com"
, "alofergy@gmail.com"
, "brenmich79@yahoo.com.mx"
, "aduran01mx@gmail.com"
, "perezba@gmail.com"
, "bmce83@yahoo.com.mx"
, "lupi.rojas.p@gmail.com"
, "hlopez.10@hotmail.com"
, "ceci_lina@hotmail.com"
, "pere_andres@hotmail.com"
, "diegoalbertojuarez@yahoo.com"
, "c.arriola@hotmail.com"
, "sebasarevaloc@gmail.com"
, "ana.ruvalcaba@grupoplan.com"
, "shakspn@gmail.com"
, "olivsmitz@gmail.com"
, "monvaldes13@gmail.com"
, "rose_loveher@hotmail.com"
, "marce1009@yahoo.com"
, "carocastros@gmail.com"
, "casanovacolsa2309@gmail.com"
, "apatino@promostar.com.mx"
, "marian.zamano@gmail.com"
, "andressantiago95@gmail.com"
, "israelh.velez24@outlook.com"
, "manuelgutierrezs@icloud.com"
, "mariatrudy84@gmail.com"
, "missphoneme@gmail.com"
, "daninavarretefoto@gmail.com"
, "kar_eb@hotmail.com"
, "hector.seraht@gmail.com"
, "gusromero@hotmail.com"
, "luispadillatrejo@gmail.com"
, "ventas@safttec.com"
, "yamil.letayf@gmail.com"
, "esedesara@gmail.com"
, "sofimed@yahoo.com.mx"
, "lourdes.rugo@gmail.com"
, "ana_beatrizm@hotmail.com"
, "juliozilli@hotmail.com"
, "lic.ferrer90@outlook.com"
, "ana_sh3@hotmail.com"
, "wicho.91@gmail.com"
, "galacticcat@hotmail.com"
, "ilianapal@gmail.com"
, "uinli@hotmail.com"
, "myorimiri@icloud.com"
, "roxanaruizg@gmail.com"
, "moy10fe@gmail.com"
, "entroficass@gmail.com"
, "alex@tempo.mx"
, "eqg26@hotmail.com"
, "dcgumi@hotmail.com"
, "milagrosgomez44@hotmail.com"
, "maarpes@hotmail.com"
, "ext_raymundo.cisneros@coats.com"
, "dividromero@gmail.com"
, "estelamejia2524@gmail.com"
, "lu6014@yahoo.com.mx"
, "castillcar@hotmail.com"
, "memin12_janu@outlook.com"
, "szimerman@gmail.com"
, "renosanchez@yahoo.com"
, "aremiicheh@gmail.com"
, "miss.phonetics@gmail.com"
, "janettemartinez0816@gmail.com"
, "contacto@cafedelossentidos.com"
, "israelkirbytorres@gmail.com"
, "luisa.marren@gmail.com"
, "rosa12jasi@hotmail.com"
, "mcoca.daniela@gmail.com"
, "ricardo.fuentes.gonzalez@outlook.com"
, "ramireztmt80@gmail.com"
, "cparrayanez@gmail.com"
, "clocer@hotmail.com"
, "nary_100@hotmail.com"
, "Jeannygp@gmail.com"
, "cioreymau@gmail.com"
, "denisealavirule@gmail.com"
, "karinagidi@hotmail.com"
, "mccutcheon.sp@gmail.com"
, "mariazurutuzag@hotmail.com"
, "ilse@palma-mx.com"
, "carrankasky7053@gmail.com"
, "catalina.galvis@gmail.com"
, "slicker75@hotmail.com"
, "tinovp@hotmail.com"
, "sanmiki7477@gmail.com"
, "jmsg91@gmail.com"
, "franciscotejitas64@gmail.com"
, "monserratapia4@hotmail.com"
, "areli.cga@gmail.com"
, "elflakoconk@gmail.com"
, "guizar_diana@me.com"
, "Victorialoperena2017@gmail.com"
, "monicaa2mx@gmail.com"
, "chaveztejero@gmail.com"
, "carolina.vc.teaching@gmail.com"
, "gabyalvar@gmail.com"
, "karlaedgg@gmail.com"
, "dianitah06@gmail.com"
, "clopezregino@gmail.com"
, "feraceves.23@hotmail.com"
, "leetayra@gmail.com"
, "aleneskapolita@yahoo.com"
, "lorenzana.jorge@gmail.com"
, "laudomrey@yahoo.com"
, "isaidominguez06@gmail.com"
, "shaitansky@hotmail.com"
, "guerrero_cisneos@hotmail.com"
, "felipesouzamusic@live.com"
, "romvalenfanni_@hotmail.com"
, "r.galindoprieto@gmail.com"
, "sofia_agls@hotmail.com"
, "Julieta.ortiz0911@gmail.com"
, "angelandiano7@gmail.com"
, "gatolo0660@gmail.com"
, "rafamumo30@gmail.com"
, "martha@ryoku.mx"
, "rcuevasorendain@gmail.com"
, "mapidelt@hotmail.com"
, "yola24somo@gmail.com"
, "vic_14santana@hotmail.com"
, "evavillarauz@gmail.com"
, "jsaavedrabernal@yahoo.com.mx"
, "paulinahm96@hotmail.com"
, "hadryg@gmail.com"
, "cinthyadelangel@gmail.com"
, "rmariana_soria@hotmail.com"
, "andrea.roessner@gmail.com"
, "ultrahector7@gmail.com"
, "berman_2020@hotmail.com"
, "mai24.gpm@gmail.com"
, "marcela.moralesgz@gmail.com"
, "rrs25983@gmail.com"
, "dulcemavalle@gmail.com"
, "laura_rojash@yahoo.com.mx"
, "imorfinreyes@gmail.com"
, "tfourier83@gmail.com"
, "reyesgine@gmail.com"
, "lcarmona13@hotmail.com"
, "cmossa.99@gmail.com"
, "jfortegan@hotmail.com"
, "gotamel69@hotmail.com"
, "normaprocra@hotmail.com"
, "666_a@hotmail.es"
, "andre.josemartinez@gmail.com"
, "alejandra.bcega@gmail.con"
, "alexisag_2332@hotmail.com"
, "jimenegron@gmail.com"
, "marcelarochin@gmail.com"
, "araceli.glez.alanis@gmail.com"
, "danielfuentes.corp@gmail.com"
, "303838157unam@gmail.com"
, "jerry_hv@hotmail.com"
, "legoorretas@gmail.com"
, "cantucla@gmail.com"
, "doris.colin@outlook.com"
, "barpatlan@gmail.com"
, "zulemyvillegas@gmail.com"
, "edson_eam@hotmail.com"
, "guizar.diana@comunidad.unam.mx"
, "alexis_2332sh@hotmail.com"
, "adelaidagapr@gmail.com"
, "munrod48@gmail.com"
, "bq.carlos79@gmail.com"
, "m.luciaerre@gmail.com"
, "gabycadaz@gmail.com"
, "dora.u@innovacionsc.mx"
, "lili84dg@gmail.com"
, "medranojazz@yahoo.com.mx"
, "yani2123@hotmail.com"
, "demianernestop@gmail.com"
, "estrellamonclova2020@gamil.com"
, "cesarhy97@gmail.com"
, "v_cendejas@hotmail.com"
, "memocastillolara@yahoo.com"
, "monicajim94@gmail.com"
, "czmgabriela@gmail.com"
, "irmlv9@gmail.com"
, "renata_rz@hotmail.com"
, "GUNOSKURINY@GMAIL.COM"
, "ksjisj79@gmail.com"
, "katherinegarista2008@gmail.com"
, "danbasurto99@gmail.com"
, "amanecerlulu26@hotmail.com"
, "theresehawat@hotmail.com"
, "josefomexico@hotmail.com"
, "mauricio.corona.1102@gmail.com"
, "alberto.alejandro.cortes@outlook.com"
, "oascjd@gmail.com"
, "lescar_20@hotmail.com"
, "aocampo@centralenlinea.com"
, "maria.guzmanzor@gmail.com"
, "beetuuz@gmail.com"
, "dolores_galicia@hotmail.com"
, "cocina@niddo.mx"
, "guscasdel@gmail.com"
, "xojed19@gmail.com"
, "gabrielaosoriog@icloud.com"
, "patsylina@gmail.com"
, "sasaar16@hotmail.com"
, "pablos.lorena@gmail.com"
, "victorbalcazar_86@hotmail.com"
, "brendapantojag5988@gmail.com"
, "clar8889@gmail.com"
, "angelaferrarilassalotte@gmail.com"
, "alejandra.bcega@gmail.com"
, "adrihglez62@gmail.com"
, "karla130999@gmail.com"
, "princesitamontse_divina@hotmail.com"
, "mattymondragon@icloud.com"
, "bucio313@gmail.com"
, "leslie.ponce.morales@gmail.com"
, "mininmissadri@hotmail.com"
, "mbarro@fad.unam.mx"
, "luis.nino2828@gmail.com"
, "tere_campero@hotmail.com"
, "karimeabdo@outlook.com"
, "silresendiz@hotmail.com"
, "mgarcia@centralenlinea.com"
, "Jorgeguzmang@outlook.com"
, "lalosalazar036@gmail.com"
, "rplata135@mail.com"
, "chivigonix@gmail.com"
, "jorgediaz2332@gmail.com"
, "pirronaraceli@hotmail.com"
, "deniihale@hotmail.com"
, "mau_vivass@hotmail.com"
, "victor_becerril10@outlook.es"
, "gmaresr@hotmail.com"
, "adricarrillo27@yahoo.com.mx"
, "legarrea@prodigy.net.mx"
, "carolmd3@yahoo.com.mx"
, "juanmanuelzepeda@outlook.com"
, "anabelen_arredondo_ag@outlook.com"
, "deycun1980@yahoo.com.mx"
, "marielagc@hotmail.es"
, "mnahyeli@gmail.com"
, "rene.galindo@icloud.com"
, "renatags@outlook.es"
, "flores_osor@hotmail.com"
, "jimenacortez@lasallistas.org.mx"
, "lfrodriguez06@gmail.com"
, "paau_sg@hotmail.com"
, "americaodalis@gmail.com"
, "liliana.hmedina@gmail.com"
, "carmona_rm@yahoo.com.mx"
, "israelcastillo24@gmail.com"
, "frank_tejas12@hotmail.com"
, "vf091216@gmail.com"
, "jatjmz@gmail.com"
, "iivonne.reynoso@gmail.com"
, "haydeki@gmail.com"
, "ssssarma2008@gmail.com"
, "carmencruz10.cc@gmail.con"
, "carmencruz10.cc@gmail.com"
, "veronikalopez1971@yahoo.com.mx"
, "veronicaaramburo@yahoo.com.mx"
, "lopillam@yahoo.com.mx"
, "michelle.ceciliano@gmail.com"
, "angelicag.palapa@gmail.com"
, "enriqueaguilarc16@gmail.com"
, "goofytresar@gmail.com"
, "sagapo83@gmail.com"
, "edith_romero92@hotmail.com"
, "guinduri.rossell@gmail.com"
, "loolipunk@hotmail.com"
, "fatima.hernandezg@outlook.com"
, "berenice.limon.guzman@gmail.com"
, "pamelarroyomtz@gmail.com"
, "anakgutierrezr@gmail.com"
, "rodrigo.alvarado.92@gmail.com"
, "montse_vt@hotmail.com"
, "monzamayo@hotmail.com"
, "barbie_cute09@hotmail.com"
, "fernando_cruz12@hotmail.com"
, "paupausaucedo@gmail.com"
, "remmanuel.garciaruiz@gmail.com"
, "maricc.armen@gmail.com"
, "fhirym@gmail.com"
, "ismael_12atejas@hotmail.com"
, "roxmaleman@gmail.com"
, "reginagarciamor@hotmail.com"
, "marisolcp77@gmail.com"
, "gagralma@gmail.com"
, "armonia.solis@gmail.com"
, "anasenyk@gmail.com"
, "enzocavalie47@gmail.com"
, "castillcar@gmail.com"
, "fer8cm@gmail.com"
, "tinkerbell30@live.com.mx"
, "fabiolaqme@gmail.com"
, "julianarotman@gmail.com"
, "snz12_alexis@hotmail.com"
, "alancete2502@gmail.com"
, "mgeyerp@live.com.mx"
, "mavifitz@hotmail.com"
, "javierviniegra@gmail.com"
, "gabriela.ranero@gmail.com"
, "angiiemss@hotmail.com"
, "adrilunaa@gmail.com"
, "nayelilete@gmail.com"
, "karely.munoz19@gmail.com"
, "02sep12@gmail.com"
, "jangeles@tarq.com.mx"
, "joselyndavila891@gmail.com"
, "marigcas@gmail.com"
, "alfonsoafi81@gmail.com"
, "paulina@thrust.com.mx"
, "alezita99@hotmail.com"
, "oss5rol@gmail.com"
, "tsitsiki.olivares@gmail.com"
, "gdjojoau@unal.edu.co"
, "anadiuls@icloud.com"
, "abetancourt167@gmail.com"
, "imagen.automotriz.sport@gmail.com"
, "manmen.mm42@gmail.com"
, "julietaparada@hotmail.com"
, "alexs_12snzh@hotmail.com"
, "ricardo_t302@hotmail.com"
, "katerin.baena@live.com"
, "cancherita@hotmail.com"
, "marveld22@hotmail.com"
, "npgomez11@hotmail.com"
, "juanmtoledorios@hotmail.com"
, "ursulacamba@yahoo.com"
, "flacs80@yahoo.com"
, "yajaira2224@icloud.com"
, "ary.lpzv@gmail.com"
, "enanopower234@gmail.com"
, "franciscoarenas25@hotmail.com"
, "vans.osor@gmail.com"
, "klaudhia@gmail.com"
, "arochelibertad@gmail.com"
, "alicia.preza@gmail.com"
, "yoma.jorge@gmail.com"
, "nmbrismat@yahoo.com"
, "jcarlos_dc@hotmail.com"
, "toylalinh@gmail.com"
, "carlospozos548@gmail.com"
, "araceliriosperalta@gmail.com"
, "karinaap@hotmail.com"
, "araranarriola.20@gmail.com"
, "myba26@gmail.com"
, "anitaregens@gmail.com"
, "amje11@hotmail.com"
, "susanaa_AR@hotmail.com"
, "maru.medrano71@hotmail.com"
, "sebastiandiazcontreras47@gmail.com"
, "danielalejamdrocastillomora@gmail.com"
, "gpichardo26@gmail.com"
, "perlahgov@gmail.com"
, "Balaam_17@hotmail.com"
, "elizabethsc20@gmail.com"
, "albertoharwy@gmail.com"
, "tbaralt@gmail.com"
, "pablo.gutierrezdelgado@gmail.com"
, "eduedzu@gmail.com"
, "viriitzel@hotmail.com"
, "administracion@laostra.com"
, "j.beier@latinhotel.com"
, "jhaydee.rm@gmail.com"
, "biankampineda@gmail.com"
, "rafaelo_800@hotmail.com"
, "ginacasanova@hotmail.com"
, "renatavomendd@gmail.com"
, "luisao999@hotmail.com"
, "toronja19s@gmail.com"
, "sanchezechegoyenlaura@gmail.com"
, "medellinrocio@gmail.com"
, "darminc@gmail.com"
, "marianagtez@gmail.com"
, "yolanda_hernandez@colpal.com"
, "adricbm@hotmail.com"
, "ikaki99@gmail.com"
, "nvilchis00@gmail.com"
, "anniemtz791@gmail.com"
, "esc.omnim@gmail.com"
, "alexis_007snzh@hotmail.com"
, "gaby.sanchezcalvo@gmail.com"
, "nohemimendizabal@hotmail.com"
, "sisiaiko24@gmail.com"
, "libertad.barron@gmail.com"
, "rociochinea@gmail.com"
, "debshapirob@yahoo.com"
, "mvance942@gmail.com"
, "e.beier@yahoo.com"
, "panzonsin1994@hotmail.com"
, "alberto.chaia@gmail.com"
, "eliza.torres.rojas@gmail.com"
, "fernandatapiaa14@gmail.com"
, "dageresp@hotmail.com"
, "ortizp.mariana@gmail.com"
, "mayelagarza@gmail.com"
, "blanksflow@gmail.com"
, "rafasanchez7100@gmail.com"
, "juancdea1@gmail.com"
, "sara.hernandez@ingredienta.com"
, "alexandra_um@hotmail.com"
, "dianag0927@gmail.com"
, "more_ana@icloud.com"
, "aguacristalina.blue@hotmail.com"
, "valezi2@hotmail.com"
, "alejandrabrielacelis@gmail.com"
, "gerardoguillermo@msn.com"
, "liszethmartinez@gmail.com"
, "josealbertogarzadiaz@gmail.com"
, "amilkarjrd@gmail.com"
, "lucycordero3@hotmail.com"
, "rosaura.prieto@gmail.com"
, "kymfer@hotmail.com"
, "kiossdamm38@gmail.com"
, "missphonema@gmail.com"
, "aemm28@gmail.com"
, "yosbel.mp@gmail.com"
, "olga.patino@gmail.com"
, "nes78mx@yahoo.com.mx"
, "rodrigo.ibarraz@hotmail.com"
, "eurycm@yahoo.com.mx"
, "mariana.estrada00@gmail.com"
, "giglesiasc@outlook.com"
, "nicolebeja@hotmail.com"
, "angelica.e.duarte.islas@gmail.com"
, "saravia.mauricio@gmail.com"
, "marvinbernal0515@gmail.com"
, "alexchambuko@hotmail.com"
, "efrainl@gmail.com"
, "delgadillogomezma@gmail.com"
, "gutierrezarturo.2109@gmail.com"
, "rggc@garciaygarcia.com"
, "device22@hotmail.com"
, "mgdpe.gtz@gmail.com"
, "pmlzoe@gmail.com"
, "juancolorado029@gmail.com"
, "cristorre_94@hotmail.com"
, "mundodee40@gmail.com"
, "ponyta.achach@gmail.com"
, "sotorodriguezangela513@gmail.com"
, "nnc.397@gmail.com"
, "jorgeivan_1309@hotmail.com"
, "raispuro@hotmail.com"
, "roseme77@yahoo.com"
, "anapmbel@gmail.com"
, "sigurross_42@hotmail.com"
, "isabeloga@hotmail.com"
, "mafer.aduna@gmail.com"
, "compras@techelados.com"
, "annisoria.1009@gmail.com"
, "xavier.aguilera@aquilaemporium.com"
, "eduardode.do@hotmail.com"
, "yolanda.naranjo.lopez@gmail.com"
, "ferferamo@gmail.com"
, "sofiarrocco@icloud.com"
, "taniaaurora792@gmail.com"
, "egarciap@ipn.mx"
, "allysonmestre@gmail.com"
, "moni-k_virgo@hotmail.com"
, "sofianch@hotmail.com"
, "pamluna91@gmail.com"
, "ra-kauslualu@hotmail.com"
, "nancyhhe21@gmail.com"
, "itadeicaza@hotmail.com"
, "lalavilinaclcam@gmail.com"
, "yanethbaezrdz2@hotmail.com"
, "marianasabu@gmail.com"
, "l.boneta@hotmail.com"
, "caasa@dchrysler.com.mx"
, "Daniela_2822@live.com.mx"
, "daniela404chavez@gmail.com"
, "mruelas_20@hotmail.com"
, "dsanromanvera@ifc.org"
, "blissbuttersmx@gmail.com"
, "arrelu2817@gmail.com"
, "castrolaraa@cmail.com"
, "reyna_xmh@hotmail.com"
, "valeriacano@yahoo.com"
, "tanerik_08@hotmail.com"
, "bmejia@centralenlinea.com"
, "lacherry04@hotmail.com"
, "marianoorell@gmail.com"
, "caref@hotmail.com"
, "caindiaz14183@gmail.com"
, "lanoticia66@gmail.com"
, "martinezsptnk@gmail.com"
, "omarhcampos@gmail.com"
, "jessicavaleriano86@gmail.com"
, "gallodiano@yahoo.com"
, "daniela_albva@outlook.con"
, "anapaubello@hotmail.com"
, "nomada.alimentos@gmail.com"
, "isagile@gmail.com"
, "marocioboschg@hotmail.com"
, "conchisruizc@hotmail.com"
, "rambue92@gmail.com"
, "clement.burel@gmail.com"
, "rebueltangel@gmail.com"
, "juanj1226josej@gmail.com"
, "elena.gzamora@gmail.com"
, "cjuarezcruz@gmail.com"
, "BEZYQCB@HOTMAIL.COM"
, "angietello66@hotmail.com"
, "anna_castro25@hotmail.com"
, "vianny83@hotmail.com"
, "vanezzadennize17@gmail.com"
, "leondinino@gmail.com"
, "anicetocas@hotmail.com"
, "hectsan57@gmail.com"
, "jmbustamantea81@gmail.com"
, "alandini50@hotmail.com"
, "contla_karen@yahoo.com.mx"
, "balam.esquivel12@gmail.com"
, "cpaisfurtado@gmail.com"
, "tallullah_belle@hotmail.com"
, "jube2309@gmail.com"
, "grisnake1@hotmail.com"
, "brisalinda29@hotmail.com"
, "manuhoyosg5@gmail.com"
, "cervantes_mb@yahoo.com.mx"
, "greyesmol@hotmail.com"
, "contactosucarina@gmail.com"
, "angieglezzli@gmail.com"
, "myrna_bauer@hotmail.com"
, "yulianas1509@gmail.com"
, "mrosariohc22@gmail.com"
, "xiarreal2017@gmail.com"
, "noferrer.ch@gmail.com"
, "micheskat@gmail.com"
, "misandoval06@gmail.com"
, "gabo98gs@gmail.com"
, "pame.navarro12@hotmail.com"
, "karintrina1987@gmail.com"
, "soniarojas@prodigy.net.mx"
, "lizprovenzal@hotmail.com"
, "solpeske@gmail.com"
, "skynet300mx@gmail.com"
, "fany.guerreror@gmail.com"
, "jmvillafana@prodigy.net.mx"
, "rubencorona9@hotmail.com"
, "anapau.villalvazo07@gmail.com"
, "ratonianabel@hotmail.com"
, "marlit90@outlook.com"
, "dayusora@gmail.com"
, "dicolori.di@gmail.com"
, "moises.lg@gmail.com"
, "syan_lozano@hotmail.es"
, "maldonadomm@icloud.com"
, "ra324056@uaeh.edu.mx"
, "cristoaraiza0@gmail.com"
, "daniela.manzanoh@gmail.com"
, "pymemach2016@gmail.com"
, "tykky_mikk91@hotmail.com"
, "dulceperris7@gmail.com"
, "Mickyaleman85@gmail.com"
, "mramirezh91@gmail.com"
, "colmenares1975@yahoo.com.mx"
, "moli15.1993@hotmail.com"
, "dni.9209@gmail.com"
, "maliciamj@yahoo.es"
, "fergie109@hotmail.com"
, "claudianaor@hotmail.com"
, "k.maytegg96@gmail.com"
, "jessica.tgs@hotmail.com"
, "elunicom.cpaco@gmail.com"
, "alongora@gmail.com"
, "analiasiller@gmail.com"
, "iztnayana@hotmail.com"
, "pablodomingueza52@gmail.com"
, "arelihurtadovallejo@gmail.com"
, "ivanssinger@hotmail.com"
, "dkl23laura@gmail.com"
, "jdvcarrillo1@outlook.com"
, "chglezarn86@gmail.com"
, "riestragaytan@gmail.com"
, "anthares.gla@gmail.com"
, "rocio_g99@hotmail.com"
, "ade.dzg@gmail.com"
, "stephen.w.caldwell@gmail.com"
, "alfredo.espinozam@gmail.com"
, "samywill92@gmail.com"
, "roldan.janeth@yahoo.com.mx"
, "marian_you1009@hotmail.com"
, "gabystar3@hotmail.com"
, "ofrifor62@gmail.com"
, "perlalopwza@gmail.com"
, "lmra.7985@gmail.com"
, "aperalta.tp58@gmail.com"
, "marthaangelicamh16@gmail.com"
, "mmpn78@gmail.com"
, "mariasabinaram@gmail.com"
, "barbara_guislan@yahoo.com"
, "adalallo1@gmail.com"
, "joseromanmttz@gmail.com"
, "artcrimes911@hotmail.com"
, "emilio.garduno.ruiz@gmail.com"
, "alexei101@gmail.com"
, "clinicasdentalesbetadent@gmail.com"
, "Villamarsericka@gmail.com"
, "lau_sb@hotmail.com"
, "metanoia_miaen@hotmail.com"
, "sobriland@me.com"
, "gmonjaraz09@gmail.com"
, "luisplanet0@gmail.com"
, "alekzahuskein@gmail.com"
, "2015capote@gmail.com"
, "emelyr-p@hotmail.com"
, "araujoaristeo172@gmail.com"
, "itzel.nu@hotmail.com"
, "alex.espinosadelosmonteros@gmail.com"
, "b_tania23@hotmail.com"
, "alfredocab22@gmail.com"
, "trans_gorilaz_rob@hotmail.com"
, "contabilidad2@soho.com.mx"
, "corderasan@hotmail.com"
, "loregl2000@gmail.com"
, "gukieeminiee94@gmail.com"
, "agnesfournierjulien@gmail.com"
, "castor2509@yahoo.com"
, "cristrejo19@gmail.com"
, "danielapacheco42@gmail.com"
, "jomparq@gmail.com"
, "JOAHNA.HERNANDEZ@GMAIL.COM"
, "paolaulloarodriguez@gmail.com"
, "Gaelhernandezhernandez072@hotmail.com"
, "erikajrdz@outlook.com"
, "eimproyoung@gmail.com"
, "thelmafernandez76@hotmail.com"
, "adryortiz1028@gmail.com"
, "mariaeugeniagomezherrera@yahoo.com.mx"
, "anakaren.beltran.coello.94@gmail.com"
, "eduardopastrana045@gmail.com"
, "belinemilio@gmail.com"
, "bussy_zaid@hotmail.com"
, "joseluisbeniteezz@gmail.com"
, "vanecardenas@hotmail.com"
, "abriltellez_80@hotmail.com"
, "magdalena.hdzg@gmail.com"
, "andy_aco@hotmail.com"
, "lgccecilia@yahoo.com.mx"
, "mariajosepadilla08@gmail.com"
, "mardo1980@hotmail.com"
, "ale.corral06@gmail.com"
, "mendoza.mariana@hotmail.com"
, "lorenafc.garcia@gmail.com"
, "elisdiscente@gmail.com"
, "hornitorico.ventas@gmail.com"
, "adrianrivera@hotmail.com"
, "atapon.k@gmail.com"
, "luzelenanoe@gmail.com"
, "b_valens@hotmail.com"
, "mesinas@yahoo.com"
, "book@agatahotelboutiquespa.com"
, "mrazo@forr.mx"
, "mvallejo.plusplay@gmail.com"
, "jonathan.estanol@gmail.com"
, "marthazaepeda95@gmail.com"
, "diana.edith.ml@gmail.com"
, "moralesyuridia43@gmail.com"
, "valdezvaleria180@gmail.con"
, "alejandro1159@live.com"
, "ljimenezyar@gmail.com"
, "roxxanaromeroc@yahoo.com.mx"
, "claudiatt@hotmail.com"
, "SALFAGEME@YAHOO.COM"
, "olgamuchoskilates@gmail.com"
, "epine760@gmail.com"
, "mpmtzr55@yahoo.com.mx"
, "ricardorocha4@yahoo.com"
, "aciklik3@gmail.com"
, "anmoragonzalez@gmail.com"
, "zertuche35@gmail.com"
, "luisipert@gmail.com"
, "cbravolaborie@gmail.com"
, "mrociotz@gmail.com"
, "roxana909@gmail.com"
, "julio@ayurvedaurbana.com"
, "pmatence@me.com"
, "pame1989@gmail.com"
, "ilopiu2@gmail.com"
, "JESSICA.GRISELDA@HOTMAIL.COM"
, "halo8743@gmail.com"
, "loli_arnoldi@hotmail.com"
, "adanzuke@gmail.com"
, "tesss7717@gmail.com"
, "hmontanog90@gmail.com"
, "ednarivero91@gmail.com"
, "mgomezcangas@gmail.com"
, "akruiz23@hotmail.com"
, "liriofan@gmail.com"
, "mariajacarandazendejasmateos@gmail.com"
, "yannis_re1@hotmail.com"
, "katerin@leadershoes.com.mx"
, "sofiachavez@prodigy.net.mx"
, "aldetellez@yahoo.com.mx"
, "zabina.zepeda@gmail.com"
, "abrilroque90@gmail.com"
, "robertomomoa5@yopmail.com"
, "toricesl@gmail.com"
, "leonor@cmogue.com"
, "tenayy9019@hotmail.com"
, "mb30@live.com.mx"
, "ingrid_lore@hotmail.com"
, "elizabeth.camacho18@hotmail.com"
, "margaritapinet@hotmail.com"
, "alarcondiego7@gmail.com"
, "hmtt@prodigy.net.mx"
, "mariaalarap@gmail.com"
, "bxanath@bk.ru"
, "leoacostayarce@hotmail.com"
, "mierigallegos@hotmail.com"
, "anettecruz21@gmail.com"
, "tiona_15@hotmail.com"
, "alejandroattolini@hotmail.com"
, "nanojuarez01@gmail.com"
, "mohamedmorales@hotmail.com"
, "imp-salon52@outlook.com"
, "itzelorendain@gmail.com"
, "choppuchis@gmail.com"
, "daniel-apolo7@hotmail.com"
, "cagrijalva@basham.com.mx"
, "samanthaltorresmorales@gmail.com"
, "jaoliveira2834@gmail.com"
, "elisa.olivaresr@gmail.com"
, "flowerlul@yahoo.com.mx"
, "Janethreyes75@gmail.com"
, "horchisane@gmail.com"
, "d.justine.fonrouge7@gmail.com"
, "vigasa1967@gmail.com"
, "andrealeonr@icloud.com"
, "ilsi_cast_ram@hotmail.com"
, "gaya_nuri@hotmail.com"
, "alxsc.facturas@gmail.com"
, "lucienprada@gmail.com"
, "famordonezandrade@gmail.com"
, "betybop2000@hotmail.com"
, "vero_riv60@hotmail.com"
, "ricardosm85@gmail.com"
, "odriozola27.marisol@gmail.com"
, "liliana_gatito@hotmail.com"
, "monica.s.arguello@gmail.com"
, "xzibit23@hotmail.es"
, "christianramirez026@gmail.com"
, "munecosdemexico@gmail.com"
, "sujeila.alpuche@gmail.com"
, "avielesp@hotmail.com"
, "sideboranojesabel7@gmail.com"
, "irahicl@hotmail.com"
, "cmafernanda@gmail.com"
, "andrea_rejon@hotmail.com"
, "vanemata36@gmail.com"
, "bruno.vertti@gmail.com"
, "lopeznegreteangelo@gmail.com"
, "dionemodelos@gmail.com"
, "prelms334@gmail.com"
, "benjiordua@hotmail.com"
, "rubibonfil2@hotmail.com"
, "alejandra.barcena@gmail.com"
, "groarof@gmail.com"
, "lauraelenapgarcia@gmail.com"
, "vacunas.visas@gmail.com"
, "rebeca.verbitzky@gmail.com"
, "ajpenilla91@hotmail.com"
, "blanca.palafox@live.com"
, "danimrojas@gmail.com"
, "talli_29@hotmail.com"
, "faty.carmar05@gmail.com"
, "fpanelati2@gmail.com"
, "lindahc05@gmail.com"
, "evelynfrancesena@gmail.com"
, "adora09@gmail.com"
, "marcosalexisgon@gmail.com"
, "ana.kryon@gmail.com"
, "soymarianacb@gmail.com"
, "ale_elviraborja@hotmail.com"
, "veros1509@gmail.com"
, "oriardale@yahoo.com"
, "shelby.sanchez@igency.mx"
, "galiavz@gmail.com"
, "joseluis.werther@gmail.com"
, "karii.fj412@gmail.com"
, "alberto_zn@hotmail.com"
, "sandra.spjc.1309@gmail.com"
, "cocozare1@yahoo.com.mx"
, "aguaclara08@gmail.com"
, "chui.galvan@gmail.com"
, "dirgni.cisne@gmail.com"
, "sabedora@gmail.com"
, "roxanalavinviz@gmail.com"
, "solangealcab@gmail.com"
, "paulina.aguirre@consorciopa.com"
, "arantxa.grajales@rnmotion.com"
, "geralhoran7@gmail.com"
, "mvzjana@gmail.com"
, "picazojanette@gmail.com"
, "ochoaeliza@yahoo.com.mx"
, "s-aketzali@hotmail.com"
, "dian_osiris2195@hotmail.com"
, "cris.di098@gmail.com"
, "anareginalopez2003@gmail.com"
, "dulce.beltranllamas@hotmail.com"
, "alibotanic@gmail.com"
, "cristinariquelme@yahoo.com"
, "lyojob@hotmail.com"
, "lordavidson@gmail.com"
, "natalybautistagarcia@gmail.com"
, "jc.villalobos@outlook.es"
, "solavi.valc@gmail.com"
, "Paulompenna@gmail.com"
, "alejabosch@hotmail.com"
, "daniel.gomez.tobias@gmail.com"
, "asanabria@centralenlinea.com"
, "mllm7983@gmail.com"
, "edgarsgc@outlook.com"
, "chantys85@gmail.com"
, "psic.juancarlos.mtz@gmail.com"
, "aicitelvera1982@hotmail.com"
, "mcrozha@gmail.com"
, "carlos.valentin.jim@gmail.com"
, "monclozano@gmail.com"
, "masiosare.com@gmail.com"
, "xyme.garrido@hotmail.com"
, "joseherx911@gmail.com"
, "azotojcd@hotmail.com"
, "indi.also07@gmail.com"
, "crazy_fez.velasco@hotmail.com"
, "issleyva@outlook.com"
, "amadagpe13@hotmail.com"
, "glacerlopez@gmail.com"
, "bere_ro@hotmail.com"
, "javigonzalez_26@hotmail.com"
, "ceciliaperezpalacios@gmail.com"
, "macmeillon@hotmail.com"
, "isis_martpos@hotmail.com"
, "rodrigoel123@icloud.com"
, "marcoantonio_a_y@hotmail.com"
, "monky8956@gmail.com"
, "maclaey@hotmail.com"
, "aceromaq.sonia@gmail.com"
, "purusca@yahoo.com"
, "camilafrancom@hotmail.com"
, "vlogger348@gmail.com"
, "chiquisauer@gmail.com"
, "verozavala73@msn.com"
, "niuguiland@yahoo.com.mx"
, "aorozcog@clandi.com.mx"
, "carosn5@gmail.com"
, "jpmartinez@centralenlinea.com"
, "angelica.castilloj@gmail.com"
, "diaz_nuria@hotmail.com"
, "diramfu@gmail.com"
, "ari-raquel_1995@hotmail.com"
, "jfga87@gmail.com"
, "nytzia@hotmail.com"
, "mairavaldescodi@gmail.com"
, "magdalb@hotmail.com"
, "esmegonzalezluna@gmail.com"
, "rene.arvt@gmail.com"
, "pausimd94@gmail.com"
, "elgokumega@hotmail.com"
, "fidenciogonzalezbravo@gmail.com"
, "manuelreynaud@gmail.com"
, "michguzman@live.com"
, "413urrutia@gmail.com"
, "alanleonardo645@gmail.com"
, "lalo2226@live.com.mx"
, "plutonic57@hotmail.com"
, "teteragarces@yahoo.com.mx"
, "fgonzale28@hotmail.com"
, "aurachdezo@gmail.com"
, "valmont027@gmail.com"
, "cristinpv49@gmail.com"
, "roberto@fuxcorp.net"
, "noracastrejon@gmail.com"
, "sayepez2002@gmail.com"
, "hanygarcia3654@gmail.com"
, "iisebav@live.com.mx"
, "marissa_torresr@yahoo.com.mx"
, "dianarivera0919@gmail.com"
, "iris.salazar.rdz@gmail.com"
, "miacoluchi99@outlook.com"
, "mfuentes0100@hotmail.com"
, "kevindeusdedit.sl@gmail.com"
, "geomar_2009@hotmail.com"
, "rivera.perezpatricia@gmail.com"
, "feromeza@protonmail.com"
, "mariange2011@live.com.mx"
, "camposbedollap@hotmail.com"
, "mercedesreyes745@gmail.com"
, "adrianaramirezf@gmail.com"
, "joselito3564@gmail.com"
, "nataly_reyna@yahoo.com.mx"
, "mmolvecn@hotmail.com"
, "polel.ascencio@gmail.com"
, "lapetirallo@hotmail.com"
, "amiguel.sabbagh@hotmail.com"
, "jonlagunaf@gmail.com"
, "glarec02@gmail.com"
, "cima1709@outlook.com"
, "shotlow1@outlook.com"
, "gufles@gmail.com"
, "pb.post@yahoo.com.mx"
, "claferalarcarc@gmail.com"
, "mitzyperez15@gmail.com"
, "YELITZA.HERNANDEZ.PEREYRA@GMAIL.COM"
, "cafeteriasprint@gmail.com"
, "gibra_ramirez@hotmail.com"
, "josh.100995@gmail.com"
, "lolavvega@gmail.com"
, "mezalopezricardo333@gmail.com"
, "laura_liramoon@hotmail.com"
, "titiviladoms@hotmail.com"
, "tere731@live.com"
, "nilia_97@hotmail.com"
, "argus9012@gmail.com"
, "vacaosopollito@gmail.com"
, "sandravargas.1909@gmail.com"
, "isda921@gmail.com"
, "hernandezlicely@gmail.com"
, "sara19512.p.c@gmail.com"
, "adriarjonag@gmail.com"
, "titanioazul@gmx.com"
, "mmvs487@hotmail.com"
, "jhbeier@icloud.com"
, "taniagabrielachavolla@gmail.com"
, "milci.pena@gmail.com"
, "juanbasaguren@hotmail.com"
, "yocelinya94.ys@gmail.com"
, "ursumm@hotmail.com"
, "susana3303@gmail.com"
, "kcgretnagreen@gmail.com"
, "danhoca@gmail.com"
, "mariana2.ayala@outlook.com"
, "nuriagomezmx@yahoo.com"
, "dra.linaresvaldez@gmail.com"
, "famega_25@hotmail.com"
, "ivra_79@hotmail.com"
, "cutre72@hotmail.com"
, "johnsonpasiva@gmail.com"
, "paloma.xdharma@gmail.com"
, "nayeli_zo20@hotmail.com"
, "akimahau@gmail.com"
, "ren.aguilar@outlook.com"
, "redmiriam@gmail.com"
, "Miriam.resendiz@ttoscana.com"
, "jessy_baby1@msn.com"
, "vicario23@live.com.mx"
, "jazzquin26@gmail.com"
, "elke@elkegarda.com"
, "alondraviridianaflorescruz@gmail.com"
, "neto.suar@gmail.com"
, "a_aguirrezabal@yahoo.com"
, "vianettaquino@hotmail.com"
, "ana_karendr@hotmail.com"
, "jdavison@anforama.com"
, "jazminfloresaguayo@gmail.com"
, "valeriamohenolobato@gmail.com"
, "vallediazmario293@gmail.com"
, "elvia_cocho@hotmail.com"
, "danylosan@hotmail.com"
, "nefer2oo1@outlook.com"
, "irvin0828@gmail.com"
, "jovangilcastro@hotmail.com"
, "mariajosedege@gmail.com"
, "blancatellezpuertos@gmail.com"
, "carismza@gmail.com"
, "breen3009@gmail.com"
, "vidosz@gmail.com"
, "Asld@outlook.com"
, "dra.sofiadeleon@gmail.com"
, "adrianaariasros@gmail.com"
, "TOTOS.CAFETERIA@GMAIL.COM"
, "perezalvarezjosearmando@gmail.com"
, "gabysonrumbero@gmail.com"
, "silvia_06@live.com.mx"
, "anabama84@hotmail.com"
, "ameyalli_grande@hotmail.com"
, "dianarebollar@hotmail.com"
, "karina.i.vazqueza@gmail.com"
, "dayagiorda@gmail.com"
, "adrianperezj@gmail.com"
, "andperbaut@gmail.com"
, "darylahu@gmail.com"
, "noortumas@gmail.com"
, "ooca_kliz@hotmail.com"
, "ladma@hotmail.com"
, "mica.moca1109@gmail.com"
, "rakycamarena@gmail.com"
, "karlyvaquez05@gmail.com"
, "agalicia@europan.mx"
, "citla.belsal@gmail.com"
, "nadix.sipirilyermita@hotmail.com"
, "yordichavez@gmail.com"
, "cifa110@hotmail.com"
, "antonioflores.geologo@gmail.com"
, "hipatiavillegas.53@gmail.com"
, "vicjimenez007@gmail.com"
, "jmortega0609@hotmail.com"
, "apolopolo1@hotmail.com"
, "lulucervantes40@gmail.com"
, "gonzalezperla933@gmail.com"
, "nuezbalboa@gmail.com"
, "elsavazalc@gmail.com"
, "hernandez.danielaisab@gmail.com"
, "jazminalmenar@gmail.com"
, "carrillourbano3@gmail.com"
, "danymodo30@gmail.com"
, "elisa.f.arias@hotmail.com"
, "susana.ugalde@gmail.com"
, "kaitzelram@gmail.com"
, "pamelalaco95@gmail.com"
, "vanss.mota@gmail.com"
, "van_cos@hotmail.com"
, "alejandra.jauregui.alarcon@gmail.com"
, "adrianarangel23@hotmail.com"
, "genkerlin@yahoo.com"
, "mcchouza@gmail.com"
, "sanrosejo@gmail.com"
, "inma.maciel@gmail.com"
, "angy_benavidesb@live.com.mx"
, "lestdolo23@gmail.com"
, "mezaliliarocio@gmail.com"
, "mustri12@gmail.com"
, "cutzi_b@hotmail.com"
, "anemixt@hotmail.com"
, "auragran.ort@gmail.com"
, "tmgm220988@gmail.com"
, "kf.godinez@gmail.com"
, "ant_cr_89@hotmail.com"
, "edith.chavez.angeles@gmail.com"
, "galoutey@hotmail.com"
, "ivonnealondra09@gmail.com"
, "melisarodriguezperez@gmail.com"
, "yolanda.flores1509@gmail.com"
, "kika67.lupe@gmail.com"
, "akalismaron@gmail.com"
, "diana.real.99@gmail.com"
, "lizmendozamelo@gmail.com"
, "jimenezriveracarlosjavier@gmail.com"
, "adrianadaza973@gmail.com"
, "anapaulareca@gmail.com"
, "jessbravo@hotmail.com"
, "betzi_girlcute@hotmail.com"
, "wuiscos@gmail.com"
, "comercializadora.sagara@gmail.com"
, "esmedelmar23@gmail.com"
, "jackymuelita@gmail.com"
, "ira.marsch@gmail.com"
, "jprimavera13@gmail.com"
, "kafary@hotmail.com"
, "svillelaa@gmail.com"
, "the.awkwrd@gmail.com"
, "administracion@animaliamexico.mx"
, "Pilarbasa@gmail.com"
, "melimaiola@gmail.com"
, "yulianna_uribegtz@outlook.com"
, "pattytis4@hotmail.com"
, "hipatiavillegas.53@hotmail.com"
, "senties.guadalupe@gmail.com"
, "paolarubiod@gmail.com"
, "lilabellizzia@gmail.com"
, "info@foodiebox.com.mx"
, "brezocineteca@gmail.com"
, "liliandeco69@hotmail.com"
, "sandy180993@hotmail.com"
, "angel.gon90@hotmail.com"
, "vabbudz@yahoo.es"
, "angeleshilton@gmail.com"
, "jgarcia@jgarcialopez.com.mx"
, "Sdelsa.3693@gmail.com"
, "Nashlita911@hotmail.com"
, "mayrapadillays@gmail.com"
, "mf6368941@gmail.com"
, "paulinaproji@yahoo.com"
, "claudette.alvarado10@gmail.com"
, "fab0209@yahoo.com"
, "critinafontesg@gmail.com"
, "jaglezl@gmail.com"
, "julio.c_27@hotmail.com"
, "arturo.lule@outlook.com"
, "MAILFGTO@GMAIL.COM"
, "ing.iskra@gmail.com"
, "josehumbertovc@aol.com"
, "sabanathalie1@gmail.com"
, "romrom1313sha@gmail.com"
, "betsitalinda@gmail.com"
, "mayte8791@gmail.com"
, "grace.aldeco@gmail.com"
, "jsadeodegario@gmail.com"
, "andreagomezhuerta@gmail.com"
, "joel_enrik@hotmail.com"
, "filadelforos@gmail.com"
, "tenemostiempo@hotmail.com"
, "maggietorres333@gmail.com"
, "mgarciab@centralenlinea.com"
, "carolina1234fuentes1234@gmail.com"
, "rociol.sevilla@hotmail.com"
, "diana.arzate@anmintegra.com"
, "lorenaglezt57@gmail.com"
, "yazfer01@hotmail.com"
, "berededios@hotmail.com"
, "alejandro_caballero_lankenau@hotmail.com"
, "compras.cp@elcalifa.com.mx"
, "alug300993@gmail.com"
, "marisolre@hotmail.com"
, "lamonada@gmail.com"
, "helq.925@hotmail.com"
, "gerentezsur2005@gmail.com"
, "morareynoso@hotmail.com"
, "aleltd@hotmail.com"
, "jllozac@me.com"
, "ximenag09@hotmail.com"
, "assjben@hotmail.com"
, "mmoran@growarquitectos.mx"
, "avillamizar@gmail.com"
, "sosa09@gmail.com"
, "almisalmita@hotmail.com"
, "varog46176@slowimo.com"
, "fco_ngupi@hotmail.com"
, "anifer390@gmail.com"
, "katia_moguel@hotmail.com"
, "franciseses@yahoo.com.mx"
, "ruza_99@hotmail.com"
, "aurora_lpez@hotmail.com"
, "greenpinkcakes@gmail.com"
, "jesus_basaldua@hotmail.com"
, "herrommeli@gmail.com"
, "condesa@wandm.com.mx"
, "mariana.leonm96@gmail.com"
, "carlos_alberto2397@hotmail.com"
, "danyfigueroa0911@gmail.com"
, "Uribee77@yahoo.com.mx"
, "dianacabazo@yahoo.com"
, "maguiregil@gmail.com"
, "abel.vicencio@gmail.com"
, "anbell19@hotmail.com"
, "danylopezh@hotmail.com"
, "klopezpiera@yahoo.com"
, "dulcepolly@live.com"
, "gerlor50@gmail.com"
, "jacobolieberman@gmail.com"
, "danasso_007@hotmail.com"
, "sm273350@gmail.com"
, "faviola.guitron@gmail.com"
, "ahmed.bin.abdou@hotmail.com"
, "israel.aguilarl@hotmail.com"
, "lorena.lebe@gmail.com"
, "jessipaula120996@gmail.com"
, "mon.zurita@hotmail.com"
, "castropedroangeldaniel@gmail.com"
, "biolacmh@gmail.com"
, "regina_gzm@gmail.com"
, "veronicahuerta016@gmail.com"
, "linda.pacheco.c@gmail.com"
, "estebangonzalezromero@gmail.com"
, "rossmary19792005@gmail.com"
, "linazapata0@gmail.com"
, "aniluc_35@hotmail.com"
, "javi.i.am@live.com.mx"
, "andressolsanudo@gmail.com"
, "danielaoeu14@gmail.com"
, "holaerika@hotmail.com"
, "lsallan999@gmail.com"
, "josestc12@gmail.com"
, "lupitaavendano@yahoo.com"
, "patopatito9@gmail.com"
, "pmespinosa@gmail.com"
, "duartepaulina9@gmail.com"
, "hko@estratec.com"
, "anto.conti@gmail.com"
, "drtessan@gmail.com"
, "cgarciam10@gmail.com"
, "directodeusa.mx@gmail.com"
, "kcortesg89@gmail.com"
, "alejandrasarahig09@gmail.com"
, "jjesly160978@gmail.com"
, "jessoudjt@gmail.com"
, "aihwiekee@gmail.com"
, "heberjmendez@gmail.com"
, "jesus@cartilladigital.com"
, "dan@danielabustamante.com"
, "miguel.ot@hotmail.com"
, "gmo_80@hotmail.com"
, "alanriosr96@gmail.com"
, "adolfo.cid@outlook.com"
, "gary0567@hotmail.com"
, "yess@ufl.com.mx"
, "dan_m23@hotmail.com"
, "tonyhxclh@gmail.com"
, "karen.otzz@hotmail.com"
, "janet14jimenez@gmail.com"
, "rocioaniceto23@gmail.com"
, "gabriel.vigueras@gmail.com"
, "saramirandaicaza@gmail.com"
, "danyp_sue@yahoo.com.mx"
, "justin.engel97@gmail.com"
, "artecelula@hotmail.com"
, "mayosroque02@gmail.com"
, "roussalva29@gmail.com"
, "giebly@hotmail.com"
, "ca.vieyra@gmail.com"
, "monroy2708@gmail.com"
, "reyare@prodigy.net.mx"
, "elias.cve@gmail.com"
, "lunaamaya3@gmail.com"
, "rodolfoarroyo997@gmail.com"
, "marianahl_27@hotmail.com"
, "paurangarte@gmail.com"
, "yovanysantos16@gmail.com"
, "carlachaillo@gmail.com"
, "mariakellycastillo@gmail.com"
, "magagonzalezg@gmail.com"
, "el_morbid@hotmail.com"
, "edgar0137149@gmail.com"
, "afc1993@hotmail.com"
, "ianianian87@hotmail.com"
, "rz.adrianad9800@gmail.com"
, "papeleriacandy2@gmail.com"
, "yunuengp@gmail.com"
, "mimoga08@gmail.com"
, "agustin.arrangoiz@gmail.com"
, "rochoa@novidea.com.mx"
, "mariacarolinarodrigo@gmail.com"
, "basso253@yahoo.com"
, "ariadna_welsh@hotmail.com"
, "maru_fashion@yahoo.com"
, "carlavchaillo@gmail.com"
, "luis.baxin01@gmail.com"
, "claudiarzf@gmail.com"
, "galaxyace.aepg@gmail.com"
, "lunapinky1012@gmail.com"
, "castillo.ortiz.gerardo@gmail.com"
, "tejedapjj@gmail.com"
, "mariana.loliver@gmail.com"
, "pilar.1espindola@gmail.com"
, "aryoropeza@gmail.com"
, "ruster02@hotmail.com"
, "socodib@yahoo.com.mx"
, "jessy3232@hotmail.com"
, "pavonclaudia@yahoo.com"
, "karymd18@hotmail.com"
, "michikoyumibe@gmail.com"
, "nayeliluna160980@gmail.com"
, "misafra@hotmail.com"
, "ja.estaba@gmail.com"
, "ccureno@hotmail.com"
, "ericasolisjac@gmail.com"
, "sedecrem90@outlook.com"
, "keym_meh@yahoo.com.mx"
, "hmestre9@gmail.com"
, "jessy100990@gmail.com"
, "recepcionfilterfx@hotmail.com"
, "Karlaem123@gmail.com"
, "eleonora.perez@icloud.com"
, "jabazanmtz@gmail.com"
, "lromero26_@hotmail.com"
, "danword93@gmail.com"
, "isabela45@hotmail.com"
, "V.olayaferro@gmail.com"
, "gabrieladominguez2013@gmail.com"
, "livelygirly86@gmail.com"
, "elguerote1992@gmail.comPaco"
, "monserrat.garcia.jan3@outlook.com"
, "denniam25diaz@gmail.com"
, "diana_ortiz_g21@hotmail.com"
, "nefertiti_2110@hotmail.com"
, "diana.valadez.garcia@gmail.com"
, "froylan_garciaruiz@hotmail.com"
, "melissadiezdelapiedra@hotmail.com"
, "estefy_1001@outlook.com"
, "lvalencia@denumeris.com"
, "karli109g@gmail.com"
, "emo0_18ka@hotmail.com"
, "karla.gcp@gmail.com"
, "JAR51462@HOTMAIL.COM"
, "reynaglezh@yahoo.com.mx"
, "flormichelle@gmail.com"
, "milagrospc972@gmail.com"
, "nettojurado@gmail.com"
, "Hg74599@gmail.com"
, "albertoarteagagon3@gmail.com"
, "fcojuanf@gmail.com"
, "dianaula2015@gmail.com"
, "yunelly_montana@hotmail.com"
, "anaitirado@gmail.com"
, "mardebose@gmail.com"
, "kkiikkiinn880_@hotmail.com"
, "guadalupesoria64@gmail.com"
, "suluaygonzalez25@gmail.com"
, "fleal21@me.com"
, "monnyglez1@hotmail.com"
, "lupitachimalmorales@outlook.com"
, "sdelavg@hotmail.com"
, "janainajacomelli@gmail.com"
, "galebelmar@yahoo.com.mx"
, "teresitadejesusrangel@gmail.com"
, "angelsandpau@hotmail.com"
, "montanezisabel@hotmail.com"
, "fika8789@hotmail.com"
, "salvador195255@yahoo.com.mx"
, "pamska@gmail.com"
, "mariagracielalg@gmail.com"
, "bettygc5030@gmail.com"
, "dr_felix75@hotmail.com"
, "judithrebornbm@gmail.com"
, "siul.ms85@gmail.com"
, "davyford074@hotmail.com"
, "salvador_farfan09@hotmail.com"
, "jose.e.casillas@hotmail.com"
, "elymarquezanaya@gmail.com"
, "lothvernica22@gmail.com"
, "michaelsaucedo123@hotmail.com"
, "argueart@hotmail.com"
, "gomili79@gmail.com"
, "agovela1966@gmail.com"
, "marmebais@gmail.com"
, "financiero2016@gmail.com"
, "areli_martinon@hotmail.com"
, "dannymompala@gmail.com"
, "cambiolf2019@gmail.com"
, "benjamin.rosalesm@gmail.com"
, "luzmariacontacto@gmail.com"
, "nhoshiko@hotmail.com"
, "enrique@kctech.com.mx"
, "meyba26@gmail.com"
, "fatiux29@hotmail.com"
, "bkcrvt@hotmail.com"
, "bola.gcs@gmail.com"
, "Donnaskat45@gmail.com"
, "merr090961@gmail.com"
, "nitomike@gmail.com"
, "patricia.cabrera1594@gmail.com"
, "vianney_mm87@gmail.com"
, "ditoscano@gmail.com"
, "Estrada.anaa@gmail.com"
, "fatima.ocampo350@gmail.com"
, "betyemipooh@hotmail.com"
, "msommer@prodigy.net.mx"
, "gufles@yahoo.com.mx"
, "dianazam76@gmail.com"
, "mechehv@yahoo.com.mx"
, "limabolita@gmail.com"
, "alessitafenix@gmail.com"
, "izubra@hotmail.com"
, "torreacqua@yahoo.com.mx"
, "schavezm92@gmail.com"
, "almaraz0809@gmail.com"
, "eslavagomez.71@gmail.com"
, "lilizmoon@gmail.com"
, "krn_lopez@hotmail.com"
, "betzssey@gmail.com"
, "otiliamorarojas1959@gmail.com"
, "vianettaguilar.46@gmail.com"
, "talfie02@gmail.com"
, "edcruzta@gmail.com"
, "lafm5109@gmail.com"
, "andreacastelpa@gmail.com"
, "alfanets@gmail.com"
, "geovannitrejo93@hotmail.com"
, "vr306110@gmail.com"
, "abiangac@gmail.com"
, "erivasb@hotmail.com"
, "adrianabl_86@hotmail.com"
, "Starblak26@hotmail.com"
, "jdotor@centralenlinea.com"
, "ritual_rg@hotmail.com"
, "namayar@gmail.com"
, "sofhialfaro.dm@gmail.com"
, "eduardo.rs.95@outlook.com"
, "anglezvel1983@hotmail.com"
, "coco.guevara@hotmail.com"
, "pamela.aguilarj@gmail.com"
, "Mfernamda781@hotmail.com"
, "elvilly5@gmail.com"
, "Mfernanda781@hotmail.com"
, "jannetecarlomex@hotmail.com"
, "malule.rf@gmail.com"
, "daviuny@gmail.com"
, "Cecicov20@gmail.com"
, "lsf_sanchez@yahoo.com.mx"
, "evelynavalos@prodigy.net.mx"
, "omae5532348086@gmail.com"
, "giovannigaleano@hotmail.com"
, "karenaviles18@outlook.es"
, "saranel353@gmail.com"
, "oliviav@emailline.net"
, "valnattian@gmail.com"
, "marianax_47@hotmail.com"
, "jevelazquez211212@gmail.com"
, "veringuasjimenezg@gmail.com"
, "laletymar@gmail.com"
, "evespv@gmail.com"
, "dayraarrieta@gmail.com"
, "ortegakika2@gmail.com"
, "alberto7161@gmail.com"
, "diego.santis96@hotmail.com"
, "Bryan.Davila.lp@gmail.com"
, "acalessiorobles@gmail.com"
, "alejandragreene3@gmail.com"
, "mnancy82@hotmail.com"
, "bbtlapanco@hotmail.com"
, "apuente505@gmail.com"
, "ferespinolaa@gmail.com"
, "gabyvv28@gmail.com"
, "claupantojamac@gmail.com"
, "letyrlugo@gmail.com"
, "acmar.chf@gmail.com"
, "IAN.LORA@GAMEPLANET.COM"
, "albertosaavedrabuenrostro@outlook.com"
, "tatemar94@gmail.com"
, "dra.claudia.gt@gmail.com"
, "aura_rnj@hotmail.com"
, "realporche@hotmail.com"
, "an12121989@gmail.com"
, "drekorydrik@gmail.com"
, "baronrojo3412@gmail.com"
, "edgarbeltran@yahoo.com"
, "ed.anrodcam@gmail.com"
, "sharondelasancha@comunidad.unam.mx"
, "angie.lou255@gmail.com"
, "myrcha61@hotmail.com"
, "elia.eliaaldana@gmail.com"
, "tanyaschiefer@gmail.com"
, "mayaescuderog@gmail.com"
, "armando7710@hotmail.com"
, "alecayetano@gmail.com"
, "jdavidlopezh@gmail.com"
, "almis7504@gmail.com"
, "andymarrin0109@gmail.com"
, "olgasasson@gmail.com"
, "cramireze@live.com"
, "isabellwh03@hotmail.com"
, "yeni.chanez.garcia@hotmail.com"
, "pablocueva0809@gmail.com"
, "Delarebel_carsc@hotmail.com"
, "maryana7747@gmail.com"
, "churro731@hotmail.com"
, "lupihervaz@hotmail.com"
, "francisco_gzz_rmz@hotmail.com"
, "yba74@hotmail.com"
, "grafitaconsultores@gmail.com"
, "anapauhornelas@gmail.com"
, "marilu_21vg@hotmail.com"
, "vzesatti@gmail.com"
, "daniela.lara@quereressabormexicano.com.mx"
, "masomer1256@gmail.com"
, "jmofiscal@prodigy.net.mx"
, "marianamelieb@hotmail.com"
, "karemya01@hotmail.com"
, "raquel_aguirre2003@yahoo.com.mx"
, "mgarciadeleonvillacorta@gmail.com"
, "ap_cg@hotmail.com"
, "priscillaestradam@hotmail.com"
, "Valeriatatenco@gmail.com"
, "Leslie.1409@hotmail.com"
, "amr9008@hotmail.com"
, "ventasinfo80@gmail.com"
, "carolinameritzel@gmail.com"
, "monicasalgado14@gmail.com"
, "jalz67@yahoo.com"
, "moctezumamaldonadop@yahoo.com"
, "natalyslove173@gmail.com"
, "arellanomedinadiana@gmail.com"
, "kenniavv06@gmail.com"
, "canudaslorena@gmail.com"
, "garu1109rulas@gmail.com"
, "maritzaaa_@hotmail.com"
, "isaah1973@gmail.com"
, "steff.luque@gmail.com"
, "nathaliesiewicz@hotmail.com"
, "jesus.saja@gmail.com"
, "socradesh@yahoo.com"
, "erfernandezb75@gmail.com"
, "ashkanyla@gmail.com"
, "Vanesaglez3e@gmail.com"
, "contactograncafevictoria@gmail.com"
, "carlagutf@gmail.com"
, "alanivanhs@gmail.com"
, "braloo28@gmail.com"
, "La.princesaliz123@gmail.com"
, "elsaana1234@hotmail.com"
, "carlos.rocker22@hotmail.com"
, "gaby_chio0902@hotmail.com"
, "Ggoitia9@gmail.com"
, "lulo.pizza.homebrew@gmail.com"
, "vany.olea@hotmail.com"
, "luguzman.rojas@outlook.com"
, "dperezv03@gmail.com"
, "nicolettaarzate@gmail.com"
, "maxplanckreeds@gmail.com"
, "bsalazar@grupopaisano.com"
, "diseniosbaldit@gmail.com"
, "alejandrattolini@hotmail.com"
, "nataliesa30@hotmail.com"
, "eremore88@gmail.com"
, "daniel.losant@gmail.com"
, "perla_benitesluna@yahoo.com"
, "terinmx@hotmail.com"
, "ms.tania14@gmail.com"
, "mk.vega@hotmail.com"
, "davidvilasenor@gmail.com"
, "sandradiazvazquez@gmail.com"
, "ali0109861@gmail.com"
, "daniela.firstbr@gmail.com"
, "dinah.colin@gmail.com"
, "Eli.castillo@cegamex.com"
, "lopezortegamonica@gmail.com"
, "jonathan007kent@gmail.com"
, "alecerda1@gmail.com"
, "yazmin.almazan@gmail.com"
, "admin@chefencasa.mx"
, "vic.palapa@outlook.com"
, "lgmarrujo@gmail.com"
, "edichi50@gmail.com"
, "gibraltarconde@hotmail.com"
, "joceservin06@hotmail.com"
, "aav89@hotmail.com"
, "valle.perena@gmail.com"
, "osalpima10@gmail.com"
, "santiagomp11@hotmail.com"
, "valeria.salinas2316@gmail.com"
, "i.demedio@gmail.com"
, "marissepaxtian@gmail.com"
, "kibejavier@hotmail.com"
, "boshmis@hotmail.com"
, "sagesatos@gmail.com"
, "alberto_sergio70@hotmail.com"
, "mg_maricela@yahoo.com.mx"
, "Berermn@gmail.com"
, "analianosh@gmail.com"
, "marikita2509@gmail.com"
, "ronan_gallegos@hotmail.com"
, "miriamga.35@gmail.com"
, "gaguirre1224@gmail.com"
, "rehabilitalia.condesa@gmail.com"
, "turin2417@hotmail.com"
, "jemina.trejo@gmail.com"
, "pumainge25@hotmail.com"
, "gabriela.moreno3430@gmail.com"
, "vivianavazq_95@outlook.com"
, "mar5martinez@hotmail.com"
, "angyslm@gmail.com"
, "elopez@centralenlinea.com"
, "parrita_hey@hotmail.com"
, "pao.tani@hotmail.com"
, "Pattyrguizar@gmail.com"
, "jmbb0509@hotmail.com"
, "davidr.88@hotmail.com"
, "jesusmiranda64@hotmail.com"
, "hinatayneji96@gmail.com"
, "nicolelschwarz@gmail.com"
, "kali-r.r@hotmail.com"
, "sercasantos@hotmail.com"
, "setportes@hotmail.com"
, "cacaopastrymx@gmail.com"
, "esmtz90@gmail.com"
, "ap1nochin01@gmai.com"
, "mra425@hotmail.com"
, "cavidal76@gmail.com"
, "kevindroide@gmail.com"
, "frudomin@gmail.com"
, "alfredrosas@outlook.es"
, "malandrex77@hotmail.com"
, "liarcru@gmail.com"
, "ayumirebollar1@gmail.com"
, "gildamuriel@yahoo.com.mx"
, "ldth07@gmail.com"
, "Marina.sanchez.fischer@gmail.com"
, "esanchez140979@gmail.com"
, "Salvadorguo@gmail.com"
, "abiran.medina@gmail.com"
, "pilarcoso@hotmail.com"
, "cvega18@gmail.com"
, "octavio050988@gmail.com"
, "hhjcescamilla@hotmail.com"
, "alejandrojajoju@gmail.com"
, "ceeccafetales04918@gmail.com"
, "zecito@hotmail.com"
, "mariacfb20@gmail.com"
, "leynaax3@msn.com"
, "jardindelarcoiris402@gmail.com"
, "melizzamariscal@gmail.com"
, "ligia.guinea@gmail.com"
, "torresdegarzas@gmail.com"
, "vidalobo@live.com.mx"
, "gariosg@gmail.com"
, "DR.FCO.MEDRANO@GMAIL.COM"
, "alex.mtz.bec@gmail.com"
, "barbieme0309@hotmail.com"
, "effy.2d@gmail.com"
, "gemaem59@gmail.com"
, "shadaig@hotmail.com"
, "anamariafloreshernandez61@gmail.com"
, "sujeysilva154a@gmail.com"
, "israpau@yahoo.com.mx"
, "gebg1981@gmail.com"
, "mairym7@yahoo.com"
, "irma_11_99@yahoo.com"
, "zod18@live.com.mx"
, "alvarezjuanjose2309@gmail.com"
, "facturaciongrata@gmail.com"
, "solraygoza@gmail.com"
, "puccaviri17@gmail.com"
, "alejandro_m1@hotmail.com"
, "franjazul@live.com.mx"
, "jfcesj@gmail.com"
, "caroaguilar15@hotmail.com"
, "gabriel98gaps@gmail.com"
, "miri@sofiasalud.com"
, "capozziluz@gmail.com"
, "fannysolis37@hotmail.com"
, "Dudaelizabeth85@gmail.com"
, "sil.hernandezher@gmail.com"
, "vez04dan@gmail.com"
, "alexisjajati@gmail.com"
, "manuel09asprilla@gmail.com"
, "rossiluisa0@gmail.com"
, "alabama151.mr@gmail.com"
, "aizpuru.mariana@gmail.com"
, "eddyeddy672@gmail.com"
, "ms910773@gmail.com"
, "anag581@yahoo.es"
, "Jiime.trejo@gmail.com"
, "geovannih07@gmail.com"
, "Evanoradevil@hotmail.com"
, "Francoalbertc@gmail.com"
, "5585781605hd@gmail.com"
, "gianfranco.gentille@gmail.com"
, "rey.lop.mario@hotmail.com"
, "carolinnazg@gmail.com"
, "almodipio@hotmail.com"
, "cherrybabs@gmail.com"
, "gjavargas@gmail.com"
, "aurdialesl@hotmail.com"
, "alexandrazmz@gmail.com"
, "nadinneb@gmail.com"
, "anniesiller@outlook.com"
, "adanfrancisco.35@gmail.com"
, "rosalia.lopez12.r@gmail.com"
, "robertomatinez@centralenlinea.mx"
, "marionegro18r@gmail.com"
, "isarochin@hotmail.com"
, "marland_2@hotmail.com"
, "Rick7736@gmail.com"
, "alejandra_00072@hotmail.com"
, "zdh.maria@icloud.com"
, "marykuri@hotmail.com"
, "6145364028cindy1999@gmail.com"
, "annie.corral94@gmail.com"
, "omateozaes@gmail.com"
, "lara_roc@hotmail.com"
, "lara_roc@hotmai.com"
, "rafaleon92@gmail.com"
, "rviridiana1209@gmail.com"
, "espepiedra@gmail.com"
, "yankikus@gmail.com"
, "omarreyesmorales@gmail.com"
, "juarez.alfredo@gmail.com"
, "Healedspaces@gmail.com"
, "dlichsabores@gmail.com"
, "alftlahuit10@gmail.com"
, "kaplunjorge@gmail.com"
, "ricardocb28@hotmail.com"
, "mercadeotres75@gmail.com"
, "ili_gomez@yahoo.com"
, "vass21@hotmail.com"
, "augusto.f.rendon@gmail.com"
, "direccion@inmueblesyoperaciones.com.mx"
, "laura_macin@hotmail.com"
, "nallely84@gmail.com"
, "ferdz95@gmail.com"
, "maripaz_bh@hotmail.com"
, "fika8789@hotmail.com.mx"
, "dulcemap2013@gmail.com"
, "Diana.crim.nav@gmail.com"
, "manaurejp@gmail.com"
, "elviabenitez05@gmail.com"
, "jose@daeba.mx"
, "alvaradoflores300@gmail.com"
, "saaraivel@hotmail.com"
, "diana_gouri@hotmail.com"
, "relizjm@gmail.com"
, "studioauka@gmail.com"
, "havejja@hotmail.com"
, "daniel.covarai77@gmail.com"
, "Daniela.seligson@gmail.com"
, "maryfer3236@gmail.com"
, "elizabeth.alth@gmail.com"
, "fcedanolima@gmail.com"
, "queenmorgen@hotmail.com"
, "jvillarreal@estral.gg"
, "ddiaz.mpw@gmail.com"
, "azucenoaaron@gmail.com"
, "materialderecoleccion@gmail.com"
, "cristianandresgalindo4@gmail.com"
, "efs710920mex@yahoo.com.mx"
, "adri_gbu@hotmail.com"
, "a.nassau@gmail.com"
, "mauricioroiz@yahoo.com.mx"
, "boulot.metro.dodo@gmail.com"
, "montserrat.gonzalez.lopez@gmail.com"
, "factor3200@hotmail.com"
, "mvzdanielaencinas@hotmail.com"
, "hola@karenmorlet.com"
, "jgykse@hotmail.com"
, "marianahl.2707@gmail.com"
, "alma.guadarrama07@gmail.com"
, "crysaud@gmail.com"
, "daristeo666@gmail.com"
, "avila.m.ximena@gmail.com"
, "dorisguerra7@hotmail.com"
, "yookosunn@hotmail.com"
, "danycalzada.11@gmail.com"
, "montserdelgadoguizar@gmail.com"
, "carlos.duran@icat.unam.mx"
, "fdesayve@gmail.com"
, "Offyr.gut@gmail.com"
, "henestrosa7@live.com.mx"
, "jhzarate71@gmail.com"
, "gusfermag@gmail.com"
, "roberjt@hotmail.com"
, "prinss_viki@hotmail.com"
, "numenpedro@hotmail.com"
, "zunigapablo85@gmail.com"
, "marianar88@gmail.com"
, "ebekai23@live.com.mx"
, "Tuyapuesita@yahoo.com.mx"
, "Edgarmendozamty@gmail.com"
, "Mariana.matahari@gmail.com"
, "lickaren130989@gmail.com"
, "ericka.lozano@levinglobal.com"
, "tadequi@hotmail.com"
, "Jordan202510@hotmail.com"
, "chlesly05@gmail.com"
, "Gabrielaalv.ximsa@gmail.com"
, "israelfernadomendozahuerta0@gmail.com"
, "ale_rode@hotmail.com"
, "jazminlg.ts@gmail.com"
, "superrich.kidjpg@gmail.com"
, "virgo_imm@hotmail.com"
, "mihaout@yahoo.com"
, "carolacoga@hotmail.com"
, "Jhernandezalianza@gmail.com"
, "coral_eri@hotmail.com"
, "Sylvia.lizet@gmail.com"
, "compras@emilia.rest"
, "monicamge70@hotmail.com"
, "contact.fabiandirector@gmail.com"
, "lic.arturo.abogados@gmail.com"
, "AAHGws@gmail.com"
, "kael_mike67@hotmail.com"
, "Govavences@gmail.com"
, "hernandez.y.asociados22@gmail.com"
, "dulce.paola.romero@gmail.com"
, "migue.hs55@hotmail.com"
, "amilop24@gmail.com"
, "audiosistemas@live.com"
, "Rocksiriz@gmail.com"
, "andrea_yeya_c@hotmail.com"
, "sietecielos@live.com.mx"
, "Marianna.ibarra@gmail.com"
, "sierraandrealozano@gmail.com"
, "mons3rr4trivas@gmail.com"
, "braccorestaurante@gmail.com"
, "pflinares@gmail.com"
, "leslyejupa@gmail.com"
, "solhannah12@gmail.com"
, "dinorahhegewisch@hotmail.com"
, "ikeryadel375@gmail.com"
, "n.isillaure@gmail.com"
, "margaritaabuchard@outlook.com"
, "alanurdiales@gmail.com"
, "sandramillanm79@gmail.com"
, "valerymm23@hotmail.com"
, "yachavez1991@gmail.com"
, "Iris.raziel.170997@gmail.com"
, "ap1nochin01@gmail.com"
, "alvareztade88@gmail.com"
, "fundacionprodesarrollo@hotmail.com"
, "guadalupe.alfonso.go@gmail.com"
, "riossantana@hotmail.com"
, "claudia.pantoja@fotlinc.com"
, "fashionspiritjisura@live.com"
, "abisa04112000@hotmail.com"
, "pablodelatorre07@gmail.com"
, "encisosantana.gd@gmail.com"
, "gioytalita@gmail.com"
, "jax@clickster.mx"
, "elenarm12esco@gmail.com"
, "doc.ortega84jpg@gmail.com"
, "birohiro@gmail.com"
, "migueloyo@yahoo.com.mx"
, "joselyn.galvan@hotmail.com"
, "alexisgranobarrera2709@gmail.com"
, "brownstermx@gmail.com"
, "mancerafernando29@gmail.com"
, "emmanuel.cadena.sandoval@gmail.com"
, "Chefalexdelgado@gmail.com"
, "hiyarimeorozco@gmail.com"
, "martinezortizsa@gmail.com"
, "entrenamientoauditivomauricio@gmail.com"
, "marchelo_21@icloud.com"
, "urieldg2989@gmail.com"
, "liz19_conde@hotmail.com"
, "vrg-sept-1996@live.com.mx"
, "dgnietogil@hotmail.com"
, "giovana.garcu@gmail.com"
, "america_nita@hotmail.com"
, "abigailg798@gmail.com"
, "Lindaquijije@gmail.com"
, "romerolaura250514@gmail.com"
, "eduardomontestorres@hotmail.com"
, "pau.arias90@gmail.com"
, "marianasancheztrujillo@yahoo.com"
, "Sanyne2@hotmail.com"
, "cantoyexisto44@gmail.com"
, "elenaluisa@gmail.com"
, "yarenhydelcarmen@hotmail.com"
, "arqnapoles@hotmail.com"
, "Maferespin@hotmail.com"
, "ednaprime91@gmail.com"
, "Miguelaj44495@gmail.com"
, "anabelenf92@hotmail.com"
, "aldocain@hotmail.com"
, "ARTURO.ALDAZRAMIREZ@GMAIL.COM"
, "mayratrujano2509@gmail.com"
, "facturasdur@gmail.com"
, "selfiecafemx@gmail.com"
, "aidaherverth9@gmail.com"
, "josuecgonzalez16@gmail.com"
, "cbavau@gmail.com"
, "alex.ab99@gmail.com"
, "atemtz@outlook.com"
, "liliarobles454@gmail.com"
, "gabimun2867@gmail.com"
, "experienciasrobotanica@gmail.com"
, "luina20pe@gmail.com"
, "solissanty456@gmail.com"
, "gabyabua@hotmail.com"
, "pacey___@hotmail.com"
, "acaherrero@gmail.com"
, "dac9492@hotmail.com"
, "malui04@hotmail.com"
, "apiedrasactor@outlook.com"
, "paula.diaz.vargas@gmail.com"
, "Luuogg@gmail.com"
, "layala_72@hotmail.com"
, "hugvalle.81088kwn@gmail.com"
, "cintlicar@gmail.com"
, "ortizdiasperlaguadalupe@gmail.com"
, "ortegaricardo.ag@gmail.com"
, "amorsabor0209@gmail.com"
, "ayaneztacher@gmail.com"
, "skaalika890@gmail.com"
, "noleumalamilejeu@gmail.com"
, "opfranceschini@gmail.com"
, "adiazlaredo@gmail.com"
, "leas8709@gmail.com"
, "marioangel_perez@yahoo.com.mx"
,
            };
            var couponCodes = new List<string>
            {
                "FIESTAPICARD",
                "FIESTACONDE",
                "FIESTAMAC",
            };
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => emails.Contains(x.Email))
                .ToList();
            var discountIds = _discountService.GetAllDiscounts(showHidden: true)
                .Where(x => couponCodes.Contains(x.CouponCode?.ToUpper()))
                .Select(x => x.Id)
                .ToList();
            var discountUsage = _discountService.GetAllDiscountUsageHistoryQuery()
                .Where(x => discountIds.Contains(x.DiscountId))
                .ToList();
            var orderIds = discountUsage.Select(x => x.OrderId).ToList();
            var orders = _orderService.GetAllOrdersQuery()
                .Where(x => orderIds.Contains(x.Id) && emails.Contains(x.Customer.Email))
                .ToList()
                .Select(x => new { x.Id, x.CustomerId, DiscountUsageHistory = x.DiscountUsageHistory.Select(y => y.Discount?.CouponCode?.ToUpper() ?? "").ToList() })
                .GroupBy(x => x.CustomerId)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Nombre";
                    worksheet.Cells[row, 2].Value = "Teléfono";
                    worksheet.Cells[row, 3].Value = "Correo";
                    worksheet.Cells[row, 4].Value = "Códigos usados";

                    foreach (var customer in customers)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = customer.GetFullName();
                        var phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                        worksheet.Cells[row, 2].Value = string.IsNullOrEmpty(phone) ? customer.Addresses.Select(x => x.PhoneNumber).FirstOrDefault() : phone;
                        worksheet.Cells[row, 3].Value = customer.Email;

                        var innerCouponCodes = orders.Where(x => x.Key == customer.Id)
                            .SelectMany(x => x.SelectMany(y => y.DiscountUsageHistory))
                            .Where(x => couponCodes.Contains(x))
                            .Distinct()
                            .ToList();
                        worksheet.Cells[row, 4].Value = string.Join(",", innerCouponCodes);
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_usado_cupones_cumpleaños.xlsx");
            }
        }

        [HttpGet]
        public IActionResult GenerateExcel171(string pIds = "")
        {
            var productIds = pIds.Split(',').Select(x => int.Parse(x.Trim())).ToList();
            var customers = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.OrderItems.Where(y => productIds.Contains(y.ProductId)).Any())
                .GroupBy(x => x.Customer)
                .Select(x => x.Key)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Nombre";
                    worksheet.Cells[row, 2].Value = "Teléfono";
                    worksheet.Cells[row, 3].Value = "Correo";
                    worksheet.Cells[row, 4].Value = "Fecha de última orden";

                    foreach (var customer in customers)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = customer.GetFullName();
                        var phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                        worksheet.Cells[row, 2].Value = string.IsNullOrEmpty(phone) ? customer.Addresses.Select(x => x.PhoneNumber).FirstOrDefault() : phone;
                        worksheet.Cells[row, 3].Value = customer.Email;
                        worksheet.Cells[row, 4].Value = OrderUtils.GetFilteredOrders(_orderService)
                            .Where(x => x.CustomerId == customer.Id).OrderByDescending(x => x.SelectedShippingDate).FirstOrDefault()?.SelectedShippingDate;
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "dd-mm-yyyy";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_comprado_productos_especificos.xlsx");
            }
        }

        public class CostsInfo
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public DateTime ReportedDateUtc { get; set; }
            public decimal UpdatedUnitCost { get; set; }
        }

        protected virtual bool ProductOfOrderItemHasManufacturer(OrderItem orderItem, List<int> manufacturerIds)
        {
            var final = false;
            var ids = orderItem.Product.ProductManufacturers.Select(x => x.ManufacturerId).ToList();
            var similarIds = manufacturerIds.Intersect(ids);
            final = similarIds.Any();
            return final;
        }

        public virtual decimal CalculateGroceryCost(Product product, int qty, decimal unitCost)
        {
            if (product.EquivalenceCoefficient > 0 || product.WeightInterval > 0)
            {
                if (product.EquivalenceCoefficient > 0)
                {
                    return (qty * unitCost) / product.EquivalenceCoefficient;
                }
                else
                {
                    return ((qty * product.WeightInterval) * unitCost) / 1000;
                }

            }

            return qty * unitCost;
        }

        private string GenerateRandomString(int length)
        {
            const string numbers = "1234567890";
            //const string specialCharacters = "!@#$%^&*()_+!=";
            //string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(numbers[rnd.Next(numbers.Length)]);
            }
            return res.ToString();
        }

        private decimal GetMedian(List<decimal> pricesList)
        {
            if (pricesList.Count == 0) return 0;
            var count = (decimal)(pricesList.Count() - 1);
            var index = (int)Math.Ceiling(count / 2);
            return pricesList.OrderBy(x => x).ElementAt(index);
        }

        private void PrepareClientDaysData(ref int client30DaysAfterCount,
        ref int client60DaysAfterCount,
        ref int client90DaysAfterCount,
        ref int client120DaysAfterCount,
        ref int client150DaysAfterCount,
        ref int client180DaysAfterCount,
        ref int client210DaysAfterCount,
        ref int client240DaysAfterCount,
        ref int client270DaysAfterCount,
        ref int client300DaysAfterCount,
        ref int client330DaysAfterCount,
        ref int client360DaysAfterCount,
        ref int client390DaysAfterCount,
        ref int client420DaysAfterCount,
        ref int client450DaysAfterCount,
        ref int client480DaysAfterCount,
        ref int client510DaysAfterCount,
        ref int client540DaysAfterCount,
        ref int client570DaysAfterCount,
        ref int client600DaysAfterCount,
        ref int client630DaysAfterCount,
        ref int client660DaysAfterCount,
        ref int client690DaysAfterCount,
        ref int client720DaysAfterCount,
        ref decimal client30DaysAfterPercentage,
        ref decimal client60DaysAfterPercentage,
        ref decimal client90DaysAfterPercentage,
        ref decimal client120DaysAfterPercentage,
        ref decimal client150DaysAfterPercentage,
        ref decimal client180DaysAfterPercentage,
        ref decimal client210DaysAfterPercentage,
        ref decimal client240DaysAfterPercentage,
        ref decimal client270DaysAfterPercentage,
        ref decimal client300DaysAfterPercentage,
        ref decimal client330DaysAfterPercentage,
        ref decimal client360DaysAfterPercentage,
        ref decimal client390DaysAfterPercentage,
        ref decimal client420DaysAfterPercentage,
        ref decimal client450DaysAfterPercentage,
        ref decimal client480DaysAfterPercentage,
        ref decimal client510DaysAfterPercentage,
        ref decimal client540DaysAfterPercentage,
        ref decimal client570DaysAfterPercentage,
        ref decimal client600DaysAfterPercentage,
        ref decimal client630DaysAfterPercentage,
        ref decimal client660DaysAfterPercentage,
        ref decimal client690DaysAfterPercentage,
        ref decimal client720DaysAfterPercentage,
        ref decimal client30DaysAfterTicket,
        ref decimal client60DaysAfterTicket,
        ref decimal client90DaysAfterTicket,
        ref decimal client120DaysAfterTicket,
        ref decimal client150DaysAfterTicket,
        ref decimal client180DaysAfterTicket,
        ref decimal client210DaysAfterTicket,
        ref decimal client240DaysAfterTicket,
        ref decimal client270DaysAfterTicket,
        ref decimal client300DaysAfterTicket,
        ref decimal client330DaysAfterTicket,
        ref decimal client360DaysAfterTicket,
        ref decimal client390DaysAfterTicket,
        ref decimal client420DaysAfterTicket,
        ref decimal client450DaysAfterTicket,
        ref decimal client480DaysAfterTicket,
        ref decimal client510DaysAfterTicket,
        ref decimal client540DaysAfterTicket,
        ref decimal client570DaysAfterTicket,
        ref decimal client600DaysAfterTicket,
        ref decimal client630DaysAfterTicket,
        ref decimal client660DaysAfterTicket,
        ref decimal client690DaysAfterTicket,
        ref decimal client720DaysAfterTicket,
        ref decimal client30DaysAfterRecurrence,
        ref decimal client60DaysAfterRecurrence,
        ref decimal client90DaysAfterRecurrence,
        ref decimal client120DaysAfterRecurrence,
        ref decimal client150DaysAfterRecurrence,
        ref decimal client180DaysAfterRecurrence,
        ref decimal client210DaysAfterRecurrence,
        ref decimal client240DaysAfterRecurrence,
        ref decimal client270DaysAfterRecurrence,
        ref decimal client300DaysAfterRecurrence,
        ref decimal client330DaysAfterRecurrence,
        ref decimal client360DaysAfterRecurrence,
        ref decimal client390DaysAfterRecurrence,
        ref decimal client420DaysAfterRecurrence,
        ref decimal client450DaysAfterRecurrence,
        ref decimal client480DaysAfterRecurrence,
        ref decimal client510DaysAfterRecurrence,
        ref decimal client540DaysAfterRecurrence,
        ref decimal client570DaysAfterRecurrence,
        ref decimal client600DaysAfterRecurrence,
        ref decimal client630DaysAfterRecurrence,
        ref decimal client660DaysAfterRecurrence,
        ref decimal client690DaysAfterRecurrence,
        ref decimal client720DaysAfterRecurrence,
        List<Order> orders,
        List<int> firstOrderClientIds,
        int firstPedidosCount,
        DateTime endDate)
        {
            var controlDateInit = endDate.AddDays(1);
            var controlDateEnd = endDate.AddDays(30);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 1 y 30 días posteriores al periodo.
            client30DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 31 y 60 días posteriores al periodo.
            controlDateInit = endDate.AddDays(31);
            controlDateEnd = endDate.AddDays(60);
            client60DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 61 y 90 días posteriores al periodo.
            controlDateInit = endDate.AddDays(61);
            controlDateEnd = endDate.AddDays(90);
            client90DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 91 y 120 días posteriores al periodo.
            controlDateInit = endDate.AddDays(91);
            controlDateEnd = endDate.AddDays(120);
            client120DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 121 y 150 días posteriores al periodo.
            controlDateInit = endDate.AddDays(121);
            controlDateEnd = endDate.AddDays(150);
            client150DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 151 y 180 días posteriores al periodo.
            controlDateInit = endDate.AddDays(151);
            controlDateEnd = endDate.AddDays(180);
            client180DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 181 y 210 días posteriores al periodo.
            controlDateInit = endDate.AddDays(181);
            controlDateEnd = endDate.AddDays(210);
            client210DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 211 y 240 días posteriores al periodo.
            controlDateInit = endDate.AddDays(211);
            controlDateEnd = endDate.AddDays(240);
            client240DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 241 y 270 días posteriores al periodo.
            controlDateInit = endDate.AddDays(241);
            controlDateEnd = endDate.AddDays(270);
            client270DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 271 y 300 días posteriores al periodo.
            controlDateInit = endDate.AddDays(271);
            controlDateEnd = endDate.AddDays(300);
            client300DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 301 y 330 días posteriores al periodo.
            controlDateInit = endDate.AddDays(301);
            controlDateEnd = endDate.AddDays(330);
            client330DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 331 y 360 días posteriores al periodo.
            controlDateInit = endDate.AddDays(331);
            controlDateEnd = endDate.AddDays(360);
            client360DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 361 y 390 días posteriores al periodo.
            controlDateInit = endDate.AddDays(361);
            controlDateEnd = endDate.AddDays(390);
            client390DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 391 y 420 días posteriores al periodo.
            controlDateInit = endDate.AddDays(391);
            controlDateEnd = endDate.AddDays(420);
            client420DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 421 y 450 días posteriores al periodo.
            controlDateInit = endDate.AddDays(421);
            controlDateEnd = endDate.AddDays(450);
            client450DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 451 y 480 días posteriores al periodo.
            controlDateInit = endDate.AddDays(451);
            controlDateEnd = endDate.AddDays(480);
            client480DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 481 y 510 días posteriores al periodo.
            controlDateInit = endDate.AddDays(481);
            controlDateEnd = endDate.AddDays(510);
            client510DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 511 y 540 días posteriores al periodo.
            controlDateInit = endDate.AddDays(511);
            controlDateEnd = endDate.AddDays(540);
            client540DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 541 y 570 días posteriores al periodo.
            controlDateInit = endDate.AddDays(541);
            controlDateEnd = endDate.AddDays(570);
            client570DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 571 y 600 días posteriores al periodo.
            controlDateInit = endDate.AddDays(571);
            controlDateEnd = endDate.AddDays(600);
            client600DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 601 y 630 días posteriores al periodo.
            controlDateInit = endDate.AddDays(601);
            controlDateEnd = endDate.AddDays(630);
            client630DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 631 y 660 días posteriores al periodo.
            controlDateInit = endDate.AddDays(631);
            controlDateEnd = endDate.AddDays(660);
            client660DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 661 y 690 días posteriores al periodo.
            controlDateInit = endDate.AddDays(661);
            controlDateEnd = endDate.AddDays(690);
            client690DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 691 y 720 días posteriores al periodo.
            controlDateInit = endDate.AddDays(691);
            controlDateEnd = endDate.AddDays(720);
            client720DaysAfterCount = orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();

            client30DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client30DaysAfterCount / (decimal)firstPedidosCount;
            client60DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client60DaysAfterCount / (decimal)firstPedidosCount;
            client90DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client90DaysAfterCount / (decimal)firstPedidosCount;
            client120DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client120DaysAfterCount / (decimal)firstPedidosCount;
            client150DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client150DaysAfterCount / (decimal)firstPedidosCount;
            client180DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client180DaysAfterCount / (decimal)firstPedidosCount;
            client210DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client210DaysAfterCount / (decimal)firstPedidosCount;
            client240DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client240DaysAfterCount / (decimal)firstPedidosCount;
            client270DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client270DaysAfterCount / (decimal)firstPedidosCount;
            client300DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client300DaysAfterCount / (decimal)firstPedidosCount;
            client330DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client330DaysAfterCount / (decimal)firstPedidosCount;
            client360DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client360DaysAfterCount / (decimal)firstPedidosCount;
            client390DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client390DaysAfterCount / (decimal)firstPedidosCount;
            client420DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client420DaysAfterCount / (decimal)firstPedidosCount;
            client450DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client450DaysAfterCount / (decimal)firstPedidosCount;
            client480DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client480DaysAfterCount / (decimal)firstPedidosCount;
            client510DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client510DaysAfterCount / (decimal)firstPedidosCount;
            client540DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client540DaysAfterCount / (decimal)firstPedidosCount;
            client570DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client570DaysAfterCount / (decimal)firstPedidosCount;
            client600DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client600DaysAfterCount / (decimal)firstPedidosCount;
            client630DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client630DaysAfterCount / (decimal)firstPedidosCount;
            client660DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client660DaysAfterCount / (decimal)firstPedidosCount;
            client690DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client690DaysAfterCount / (decimal)firstPedidosCount;
            client720DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client720DaysAfterCount / (decimal)firstPedidosCount;

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 1 y 30 días posteriores al periodo.
            controlDateInit = endDate.AddDays(1);
            controlDateEnd = endDate.AddDays(30);
            var pedidos30 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client30DaysAfterTicket = pedidos30
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 31 y 60 días posteriores al periodo.
            controlDateInit = endDate.AddDays(31);
            controlDateEnd = endDate.AddDays(60);
            var pedidos60 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client60DaysAfterTicket = pedidos60
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 61 y 90 días posteriores al periodo.
            controlDateInit = endDate.AddDays(61);
            controlDateEnd = endDate.AddDays(90);
            var pedidos90 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client90DaysAfterTicket = pedidos90
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 91 y 120 días posteriores al periodo.
            controlDateInit = endDate.AddDays(91);
            controlDateEnd = endDate.AddDays(120);
            var pedidos120 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client120DaysAfterTicket = pedidos120
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 121 y 150 días posteriores al periodo.
            controlDateInit = endDate.AddDays(121);
            controlDateEnd = endDate.AddDays(150);
            var pedidos150 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client150DaysAfterTicket = pedidos150
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 151 y 180 días posteriores al periodo.
            controlDateInit = endDate.AddDays(151);
            controlDateEnd = endDate.AddDays(180);
            var pedidos180 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client180DaysAfterTicket = pedidos180
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 181 y 210 días posteriores al periodo.
            controlDateInit = endDate.AddDays(181);
            controlDateEnd = endDate.AddDays(210);
            var pedidos210 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client210DaysAfterTicket = pedidos210
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 211 y 240 días posteriores al periodo.
            controlDateInit = endDate.AddDays(211);
            controlDateEnd = endDate.AddDays(240);
            var pedidos240 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client240DaysAfterTicket = pedidos240
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 241 y 270 días posteriores al periodo.
            controlDateInit = endDate.AddDays(241);
            controlDateEnd = endDate.AddDays(270);
            var pedidos270 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client270DaysAfterTicket = pedidos270
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 271 y 300 días posteriores al periodo.
            controlDateInit = endDate.AddDays(271);
            controlDateEnd = endDate.AddDays(300);
            var pedidos300 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client300DaysAfterTicket = pedidos300
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 301 y 330 días posteriores al periodo.
            controlDateInit = endDate.AddDays(301);
            controlDateEnd = endDate.AddDays(330);
            var pedidos330 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client330DaysAfterTicket = pedidos330
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 331 y 360 días posteriores al periodo.
            controlDateInit = endDate.AddDays(331);
            controlDateEnd = endDate.AddDays(360);
            var pedidos360 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client360DaysAfterTicket = pedidos360
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 361 y 390 días posteriores al periodo.
            controlDateInit = endDate.AddDays(361);
            controlDateEnd = endDate.AddDays(390);
            var pedidos390 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client390DaysAfterTicket = pedidos390
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 391 y 420 días posteriores al periodo.
            controlDateInit = endDate.AddDays(391);
            controlDateEnd = endDate.AddDays(420);
            var pedidos420 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client420DaysAfterTicket = pedidos420
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 421 y 450 días posteriores al periodo.
            controlDateInit = endDate.AddDays(421);
            controlDateEnd = endDate.AddDays(450);
            var pedidos450 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client450DaysAfterTicket = pedidos450
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 451 y 480 días posteriores al periodo.
            controlDateInit = endDate.AddDays(451);
            controlDateEnd = endDate.AddDays(480);
            var pedidos480 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client480DaysAfterTicket = pedidos480
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 481 y 510 días posteriores al periodo.
            controlDateInit = endDate.AddDays(481);
            controlDateEnd = endDate.AddDays(510);
            var pedidos510 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client510DaysAfterTicket = pedidos510
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 511 y 540 días posteriores al periodo.
            controlDateInit = endDate.AddDays(511);
            controlDateEnd = endDate.AddDays(540);
            var pedidos540 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client540DaysAfterTicket = pedidos540
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 541 y 570 días posteriores al periodo.
            controlDateInit = endDate.AddDays(541);
            controlDateEnd = endDate.AddDays(570);
            var pedidos570 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client570DaysAfterTicket = pedidos570
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 571 y 600 días posteriores al periodo.
            controlDateInit = endDate.AddDays(571);
            controlDateEnd = endDate.AddDays(600);
            var pedidos600 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client600DaysAfterTicket = pedidos600
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 601 y 630 días posteriores al periodo.
            controlDateInit = endDate.AddDays(601);
            controlDateEnd = endDate.AddDays(630);
            var pedidos630 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client630DaysAfterTicket = pedidos630
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 631 y 660 días posteriores al periodo.
            controlDateInit = endDate.AddDays(631);
            controlDateEnd = endDate.AddDays(660);
            var pedidos660 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client660DaysAfterTicket = pedidos660
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 661 y 690 días posteriores al periodo.
            controlDateInit = endDate.AddDays(661);
            controlDateEnd = endDate.AddDays(690);
            var pedidos690 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client690DaysAfterTicket = pedidos690
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 691 y 720 días posteriores al periodo.
            controlDateInit = endDate.AddDays(691);
            controlDateEnd = endDate.AddDays(720);
            var pedidos720 = OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            client720DaysAfterTicket = pedidos720
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            client30DaysAfterRecurrence = (decimal)pedidos30.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client60DaysAfterRecurrence = (decimal)pedidos60.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client90DaysAfterRecurrence = (decimal)pedidos90.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client120DaysAfterRecurrence = (decimal)pedidos120.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client150DaysAfterRecurrence = (decimal)pedidos150.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client180DaysAfterRecurrence = (decimal)pedidos180.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client210DaysAfterRecurrence = (decimal)pedidos210.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client240DaysAfterRecurrence = (decimal)pedidos240.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client270DaysAfterRecurrence = (decimal)pedidos270.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client300DaysAfterRecurrence = (decimal)pedidos300.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client330DaysAfterRecurrence = (decimal)pedidos330.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client360DaysAfterRecurrence = (decimal)pedidos360.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client390DaysAfterRecurrence = (decimal)pedidos390.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client420DaysAfterRecurrence = (decimal)pedidos420.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client450DaysAfterRecurrence = (decimal)pedidos450.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client480DaysAfterRecurrence = (decimal)pedidos480.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client510DaysAfterRecurrence = (decimal)pedidos510.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client540DaysAfterRecurrence = (decimal)pedidos540.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client570DaysAfterRecurrence = (decimal)pedidos570.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client600DaysAfterRecurrence = (decimal)pedidos600.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client630DaysAfterRecurrence = (decimal)pedidos630.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client660DaysAfterRecurrence = (decimal)pedidos660.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client690DaysAfterRecurrence = (decimal)pedidos690.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client720DaysAfterRecurrence = (decimal)pedidos720.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
        }

        private void PrepareFirstAndLastOrders(List<Order> firstOrders,
            List<Order> notFirstOrders,
            List<Order> allOrders,
            DateTime initDate,
            List<IGrouping<int, Order>> dateOrdersGroupedByCustomer)
        {
            var previousOrders = allOrders.Where(x => x.SelectedShippingDate < initDate).ToList();
            foreach (var itemGroup in dateOrdersGroupedByCustomer)
            {
                if (!previousOrders.Where(x => x.CustomerId == itemGroup.Key).Any())
                {
                    var firstPedido = OrderUtils.GetPedidosGroupByList(itemGroup.Select(x => x).ToList())
                        .OrderBy(x => x.Key.SelectedShippingDate)
                        .FirstOrDefault();
                    firstOrders.AddRange(firstPedido.Select(x => x));
                }
                else
                {
                    notFirstOrders.AddRange(itemGroup.Select(x => x));
                }
            }
        }

        // Monto vendido desde siempre, total vendido en...
        public IActionResult GenerateValue5(int days = 0)
        {
            var orders = GetFilteredOrders();

            if (days > 0)
            {
                var controlDate = DateTime.Now.AddDays(days * -1).Date;
                var today = DateTime.Now.Date;
                orders = orders.Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate < today);
            }

            return Ok(orders.Sum(x => x.OrderTotal));
        }

        // Promedio venta diaria
        public IActionResult GenerateValue6(int days = 30)
        {
            var controlDate = DateTime.UtcNow.AddDays(days * -1);
            var filteredOrders = GetFilteredOrders()
                .Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= DateTime.UtcNow)
                .GroupBy(x => x.SelectedShippingDate);

            List<decimal> totals = new List<decimal>();
            foreach (var group in filteredOrders)
                totals.Add(group.Select(x => x.OrderTotal).DefaultIfEmpty().Sum(x => x));

            return Ok(totals.Average());
        }

        // órdenes con productos duplicados
        public IActionResult GenerateValue7()
        {
            var controlDate = DateTime.Now.Date;
            var filteredOrders = GetFilteredOrders()
                .Where(x => x.SelectedShippingDate > controlDate)
                .ToList();

            List<int> orderIds = new List<int>();
            foreach (var order in filteredOrders)
            {
                var orderItems = order.OrderItems;
                if (orderItems.GroupBy(x => x.ProductId).Where(x => x.Count() > 1).Any())
                    orderIds.Add(order.Id);
            }

            return Ok(orderIds);
        }

        // cantidad de ordenes en el mes
        public IActionResult GenerateValue8(int month)
        {
            int year = DateTime.Now.Year;
            var ordersCount = GetFilteredOrders()
                .Where(x => x.SelectedShippingDate.Value.Month == month && x.SelectedShippingDate.Value.Year == year)
                .Count();

            return Ok(new { Month = new DateTime(year, month, 1).ToString("MMMM", new CultureInfo("es-MX")), Value = ordersCount });
        }

        // Cuantos hicieron su primera compra y cuantos diferentes compraron, por mes
        [HttpGet]
        public IActionResult GenerateValue9(int month, int year)
        {
            var controlDate = new DateTime(year, month, 1);
            var endDate = controlDate.AddMonths(1);
            var groupedOrders = GetFilteredOrders().Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate < endDate).GroupBy(x => x.CustomerId).ToList();

            var pastOrders = GetFilteredOrders().Where(x => x.SelectedShippingDate < controlDate).ToList();

            int customersCount = groupedOrders.Count;
            int firstOrdersCount = 0;
            foreach (var group in groupedOrders)
            {
                bool hasPreviousOrder = pastOrders.Where(x => x.CustomerId == group.Key).Any();
                if (!hasPreviousOrder) firstOrdersCount += 1;
            }

            return Ok(new { customersCount, firstOrdersCount });
        }

        // total vendido en el mes
        public IActionResult GenerateValue11(int month, int year = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            var total = GetFilteredOrders()
                .Where(x => x.SelectedShippingDate.Value.Month == month && x.SelectedShippingDate.Value.Year == year)
                .Select(x => x.OrderTotal)
                .DefaultIfEmpty()
                .Sum();

            return Ok(new { Month = new DateTime(year, month, 1).ToString("MMMM yyyy", new CultureInfo("es-MX")), Value = total });
        }

        // total vendido por productos
        public IActionResult GenerateValue18(string productIds, int days = 30)
        {
            var today = DateTime.Now.Date;
            var controlDate = today.AddDays(-1 * days);
            var parsedProductIds = productIds.Split(',').Select(x => int.Parse(x.Trim()));

            var orderItems = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today)
                .SelectMany(x => x.OrderItems)
                .Where(x => parsedProductIds.Contains(x.ProductId))
                .ToList();

            return Ok(orderItems.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum());
        }

        // cantidad de ordenes growht hacking
        public IActionResult GenerateValue19(int days = 30)
        {
            var today = DateTime.Now.Date;
            var controlDate = new DateTime(2020, 7, 4).Date;
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today);

            return Ok(orders.Count());
        }

        // Compras socios
        public IActionResult GenerateValue20(int month, int year = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate.Value.Month == month && x.SelectedShippingDate.Value.Year == year && x.PaymentMethodSystemName == "Payments.Benefits");

            return Ok(ordersQuery.Select(x => x.OrderTotal).DefaultIfEmpty().Sum());
        }

        // total pedidos en el mes
        public IActionResult GenerateValue12(int month, int year = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate.Value.Month == month && x.SelectedShippingDate.Value.Year == year);
            var total = OrderUtils.GetPedidosOnly(ordersQuery).Count();

            return Ok(new { Month = new DateTime(year, month, 1).ToString("MMMM yyyy", new CultureInfo("es-MX")), Value = total });
        }

        // Compras socios
        public IActionResult GenerateValue13(int month, int year = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate.Value.Month == month && x.SelectedShippingDate.Value.Year == year && x.PaymentMethodSystemName == "Payments.Benefits");
            var grouped = ordersQuery.GroupBy(x => x.Customer).ToList().Select(x => new
            {
                C = x.Key.GetFullName(),
                A = x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()
            }).ToList();

            return Ok(grouped);
        }

        // Cantidad de órdenes en los ultimos días
        public IActionResult GenerateValue14(int days = 30)
        {
            var today = DateTime.Now.Date;
            var controlDate = DateTime.Now.AddDays(days * -1).Date;
            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today);

            return Ok(ordersQuery.Count());
        }

        // Cantidad de productos diferentes en los ultimos días
        public IActionResult GenerateValue15(int days = 30)
        {
            var today = DateTime.Now.Date;
            var controlDate = DateTime.Now.AddDays(days * -1).Date;
            var productIds = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today)
                .SelectMany(x => x.OrderItems)
                .GroupBy(x => x.ProductId)
                .Select(x => x.Key);

            return Ok(productIds.Count());
        }

        // Cantidad de productos en los ultimos días
        public IActionResult GenerateValue16(int days = 30)
        {
            var today = DateTime.Now.Date;
            var controlDate = DateTime.Now.AddDays(days * -1).Date;
            var orderItems = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today)
                .SelectMany(x => x.OrderItems);

            return Ok(orderItems.Count());
        }

        // Cantidad de codigos postales
        public IActionResult GenerateValue17()
        {
            var shippingZones = _shippingZoneService.GetAll().ToList();
            var postalCodes = string.Join(",", shippingZones.Select(x => x.PostalCodes)).Split(',').Select(x => x.Trim()).GroupBy(x => x).ToList();
            var postalCodesCount = postalCodes.Count();
            var additionalUniquePostalCodesCount = string.Join(",", shippingZones.Select(x => x.AdditionalPostalCodes)).Split(',').Select(x => x.Trim()).GroupBy(x => x).Where(x => !postalCodes.Contains(x)).Count();

            return Ok(new { postalCodesCount, additionalUniquePostalCodesCount, Total = additionalUniquePostalCodesCount + postalCodesCount });
        }

        //Cuantas ordenes y cuantas fueron primera compra, por dia
        [HttpGet]
        public IActionResult GenerateValue30(int days = 30)
        {
            var controlDate = DateTime.Now.AddDays(days * -1).Date;
            var now = DateTime.Now;
            var groupedOrders = GetFilteredOrders().Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate.Value <= now).GroupBy(x => x.SelectedShippingDate).ToList();
            var allOrders = GetFilteredOrders().ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Fecha";
                    worksheet.Cells[row, 2].Value = "Número de pedidos";
                    worksheet.Cells[row, 3].Value = "Número de primeras compras";

                    foreach (var group in groupedOrders)
                    {
                        var ordersByCustomer = group.GroupBy(x => x.CustomerId);
                        var previousOrders = allOrders.Where(x => x.SelectedShippingDate < group.Key.Value);

                        row++;
                        worksheet.Cells[row, 1].Value = group.Key.Value;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 2].Value = ordersByCustomer.Count();

                        int firstOrder = 0;
                        foreach (var customerGroup in ordersByCustomer)
                        {
                            firstOrder += previousOrders.Where(x => x.CustomerId == customerGroup.Key).Any() ? 0 : 1;
                        }

                        worksheet.Cells[row, 3].Value = firstOrder;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_cantidad_ordenes_primera_compra_{days}_dias.xlsx");
            }
        }

        //clientes que ordenaron por día y cuanto gastaron
        [HttpGet]
        public IActionResult GenerateValue31(int days = 30)
        {
            var controlDate = DateTime.Now.AddDays(days * -1).Date;
            var now = DateTime.Now;
            var ordersQuery = GetFilteredOrders()
                .Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate.Value <= now);
            var groupedOrders = ordersQuery.GroupBy(x => x.Customer).ToList();
            var dates = ordersQuery.GroupBy(x => x.SelectedShippingDate.Value).OrderBy(x => x.Key).Select(x => x.Key).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    int column = 1;
                    worksheet.Cells[row, column].Value = "Cliente";
                    foreach (var date in dates)
                    {
                        column++;
                        worksheet.Cells[row, column].Value = date;
                        worksheet.Cells[row, column].Style.Numberformat.Format = "dd-mm-yyyy";
                    }

                    foreach (var group in groupedOrders)
                    {
                        column = 1;
                        row++;
                        worksheet.Cells[row, column].Value = group.Key.Email;
                        foreach (var date in dates)
                        {
                            column++;
                            var spent = group.Where(x => x.SelectedShippingDate == date).Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                            worksheet.Cells[row, column].Value = spent;
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_monto_por_cliente_{days}_dias.xlsx");
            }
        }

        //reporte de entrega de ordenes
        [HttpGet]
        public IActionResult GenerateValue32(int days = 30)
        {
            var controlDate = DateTime.Now.AddDays(days * -1).Date;
            var now = DateTime.Now;
            var ordersQuery = GetFilteredOrders()
                .Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate.Value <= now);
            var groupedOrders = ordersQuery.GroupBy(x => x.SelectedShippingTime).ToList();
            var zones = _shippingZoneService.GetAll().ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    foreach (var groupByTime in groupedOrders)
                    {
                        var worksheet = xlPackage.Workbook.Worksheets.Add(groupByTime.Key);
                        int row = 1;
                        int column = 1;
                        worksheet.Cells[row, 1].Value = "Fecha";
                        worksheet.Cells[row, 2].Value = "Zona";
                        worksheet.Cells[row, 3].Value = "Entregas adelantadas";
                        worksheet.Cells[row, 4].Value = "Entregas a tiempo";
                        worksheet.Cells[row, 5].Value = "Entregas tarde (menos de 30 min)";
                        worksheet.Cells[row, 6].Value = "Entregas muy tarde (más de 30 min)";
                        worksheet.Cells[row, 7].Value = "Total de entregas zona";

                        var groupedByDate = groupByTime
                            .GroupBy(x => x.SelectedShippingDate)
                            .OrderBy(x => x.Key.Value);

                        foreach (var byDate in groupedByDate)
                        {
                            var groupedByZone = byDate.GroupBy(x => x.ZoneId).OrderBy(x => zones.Where(y => y.Id == x.Key).FirstOrDefault().ZoneName);
                            foreach (var gByZone in groupedByZone)
                            {
                                row++;

                                var result = GetDeliveryStatus(gByZone.ToList(), groupByTime.Key);
                                var zoneName = zones.Where(x => x.Id == gByZone.Key.Value).FirstOrDefault()?.ZoneName;
                                worksheet.Cells[row, 1].Value = byDate.Key.Value;
                                worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mm-yyyy";
                                worksheet.Cells[row, 2].Value = zoneName;
                                worksheet.Cells[row, 3].Value = result.Level1;
                                worksheet.Cells[row, 4].Value = result.Level2;
                                worksheet.Cells[row, 5].Value = result.Level3;
                                worksheet.Cells[row, 6].Value = result.Level4;
                                worksheet.Cells[row, 7].Value = result.Level1 + result.Level2 + result.Level3 + result.Level4;
                            }
                        }

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_reporte_entregas_{days}_dias.xlsx");
            }
        }

        // cuantos han comprado al menos una vez
        [HttpGet]
        public IActionResult GenerateValue10()
        {
            var orders = GetFilteredOrders().GroupBy(x => x.CustomerId);

            return Ok(orders.Count());
        }

        // cuantos han comprado al menos una vez
        [HttpGet]
        public IActionResult GenerateValue33()
        {
            var controlDate = DateTime.Now.AddMonths(-12);
            var now = DateTime.Now;
            var orders = GetFilteredOrders().Where(x => x.SelectedShippingDate.Value >= controlDate && x.SelectedShippingDate.Value <= now);//.GroupBy(x => x.CustomerId);
                                                                                                                                            //var count = 0;
                                                                                                                                            //foreach (var group in orders)
                                                                                                                                            //{
                                                                                                                                            //    var countExcludingFirst = group.Count() - 1;
                                                                                                                                            //    count += countExcludingFirst;
                                                                                                                                            //}

            return Ok(orders.Count());
        }

        // Ventas del mes sin considerar socios
        [HttpGet]
        public IActionResult GenerateValue34(int month, int year)
        {
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate.Value.Month == month && x.SelectedShippingDate.Value.Year == year && x.PaymentMethodSystemName != "Payments.Benefits");

            return Ok(orders.Select(x => x.OrderTotal).DefaultIfEmpty().Sum());
        }

        // Cantidad de usuarios que hicieron primera compra en el mes
        [HttpGet]
        public IActionResult GenerateValue35(int month, int year)
        {
            var controlDate = new DateTime(year, month, 1);
            var previousOrders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate < controlDate).ToList();
            var monthClientIds = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate.Value.Month == month && x.SelectedShippingDate.Value.Year == year)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .ToList();

            int firstOrder = 0;
            foreach (var clientId in monthClientIds)
            {
                if (!previousOrders.Where(x => x.CustomerId == clientId).Any())
                    firstOrder++;
            }

            return Ok(firstOrder);
        }

        // Reposicion
        [HttpGet]
        public IActionResult GenerateValue36(int days = 30)
        {
            var controlDate = DateTime.Now.AddDays(days * -1).Date;
            var ordersGroup = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate)
                .Where(x => x.PaymentMethodSystemName == "Payments.Replacement")
                .GroupBy(x => x.CustomerId)
                .ToList();

            var allOrders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate)
                .Where(x => x.PaymentMethodSystemName != "Payments.Replacement")
                .ToList();

            int otherOrdersCount = 0;
            foreach (var group in ordersGroup)
            {
                foreach (var item in group)
                {
                    var existing = allOrders.Where(x => x.CustomerId == item.CustomerId && x.SelectedShippingDate > item.SelectedShippingDate).Any();
                    if (existing)
                    {
                        otherOrdersCount++;
                        continue;
                    }
                }
            }

            return Ok(new { ClientsCount = ordersGroup.Count, otherOrdersCount });
        }

        // Cuanto se ha vendido de producto
        [HttpGet]
        public IActionResult GenerateValue37(int productId, int days = 30)
        {
            var controlDate = DateTime.Now.AddDays(days * -1).Date;
            var items = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate)
                .SelectMany(x => x.OrderItems)
                .Where(x => x.ProductId == productId)
                .ToList();

            var qty = GetQty(items.FirstOrDefault().Product, items.Select(x => x.Quantity).DefaultIfEmpty().Sum());

            return Ok(new { Product = items.FirstOrDefault().Product?.Name, Unit = qty.Item2, Total = qty.Item1, TotalSell = items.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum(), Days = days });
        }

        // Venta por fabricante durante mes
        [HttpGet]
        public IActionResult GenerateValue38(int fbId, int month)
        {
            var controlDate = new DateTime(DateTime.Now.Year, month, 1);
            var items = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate.Value.Month == controlDate.Month && x.SelectedShippingDate.Value.Year == controlDate.Year)
                .SelectMany(x => x.OrderItems)
                .Where(x => x.Product.ProductManufacturers.Where(y => y.ManufacturerId == fbId).Any())
                .ToList();

            return Ok(new { OrdersCount = items.GroupBy(x => x.OrderId).Count(), ProductsCount = items.GroupBy(x => x.ProductId).Count(), Amount = items.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum() });
        }

        // Cantidad de clientes que hicieron pedido por mes
        [HttpGet]
        public IActionResult GenerateValue39(int month, int year)
        {
            var customerCount = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate.Value.Year == year && x.SelectedShippingDate.Value.Month == month)
                .Select(x => x.CustomerId)
                .Distinct()
                .Count();

            return Ok(customerCount);
        }

        // Venta y costo del dia
        [HttpGet]
        public IActionResult GenerateValue40(string date)
        {
            var parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate == parsedDate);
            var sellAmount = orders.Select(x => x.OrderSubtotalInclTax + x.OrderShippingInclTax).DefaultIfEmpty().Sum();
            var groupedByProduct = orders.SelectMany(x => x.OrderItems).GroupBy(x => x.Product).ToList();
            var reports = _orderReportService.GetAll().Where(x => x.OrderShippingDate == parsedDate).ToList();
            decimal totalCost = 0;
            foreach (var item in groupedByProduct)
            {
                var productUnitCost = reports.Where(x => x.ProductId == item.Key.Id).Select(x => x.UpdatedUnitCost).FirstOrDefault();
                var cost = CalculateGroceryCost(item.Key, item.Select(x => x.Quantity).DefaultIfEmpty().Sum(), productUnitCost);
                totalCost += cost;
            }

            return Ok(new { TotalSubtotal = sellAmount, TotalCost = totalCost, Margin = 1 - (totalCost / sellAmount), Date = date });
        }

        // Ventas por año
        [HttpGet]
        public IActionResult GenerateValue41(int year)
        {
            var total = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate.Value.Year == year)
                .Select(x => x.OrderTotal)
                .DefaultIfEmpty()
                .Sum();
            return Ok(total);
        }

        private TimeSpan GetSelectedEndTimeSpan(string selectedShippingTime)
        {
            switch (selectedShippingTime)
            {
                case "1:00 PM - 3:00 PM":
                    return new TimeSpan(15, 0, 0);
                case "3:00 PM - 5:00 PM":
                    return new TimeSpan(17, 0, 0);
                case "5:00 PM - 7:00 PM":
                    return new TimeSpan(19, 0, 0);
                case "7:00 PM - 9:00 PM":
                    return new TimeSpan(21, 0, 0);
                default:
                    return default(TimeSpan);
            }
        }

        private DeliveryStatusResult GetDeliveryStatus(List<Order> orders, string shippingTime)
        {
            var result = new DeliveryStatusResult();
            foreach (var order in orders)
            {
                var deliveryDate = order.Shipments.FirstOrDefault()?.DeliveryDateUtc;
                if (!deliveryDate.HasValue) continue;
                deliveryDate = deliveryDate.Value.ToLocalTime();
                var selectedShippingDate = order.SelectedShippingDate.Value;
                // 0 = grey/pending, 1 = blue, 2 = green, 3 = yellow, 4 = red
                if (deliveryDate != DateTime.MinValue && selectedShippingDate != DateTime.MinValue)
                {
                    // Create times for comparing
                    var dateCompare1 = DateTime.MinValue;
                    var dateCompare2 = DateTime.MinValue;
                    if (shippingTime == "1:00 PM - 3:00 PM")
                    {
                        dateCompare1 = new DateTime(selectedShippingDate.Year, selectedShippingDate.Month, selectedShippingDate.Day, 13, 0, 0);
                        dateCompare2 = new DateTime(selectedShippingDate.Year, selectedShippingDate.Month, selectedShippingDate.Day, 15, 0, 0);
                    }
                    else if (shippingTime == "3:00 PM - 5:00 PM")
                    {
                        dateCompare1 = new DateTime(selectedShippingDate.Year, selectedShippingDate.Month, selectedShippingDate.Day, 15, 0, 0);
                        dateCompare2 = new DateTime(selectedShippingDate.Year, selectedShippingDate.Month, selectedShippingDate.Day, 17, 0, 0);
                    }
                    else if (shippingTime == "5:00 PM - 7:00 PM")
                    {
                        dateCompare1 = new DateTime(selectedShippingDate.Year, selectedShippingDate.Month, selectedShippingDate.Day, 17, 0, 0);
                        dateCompare2 = new DateTime(selectedShippingDate.Year, selectedShippingDate.Month, selectedShippingDate.Day, 19, 0, 0);
                    }
                    else if (shippingTime == "7:00 PM - 9:00 PM")
                    {
                        dateCompare1 = new DateTime(selectedShippingDate.Year, selectedShippingDate.Month, selectedShippingDate.Day, 19, 0, 0);
                        dateCompare2 = new DateTime(selectedShippingDate.Year, selectedShippingDate.Month, selectedShippingDate.Day, 21, 0, 0);
                    }
                    var dateCompare16Earlier = dateCompare1.AddMinutes(-16);
                    var dateCompare15Before = dateCompare1.AddMinutes(-15);
                    var dateCompare15After = dateCompare2.AddMinutes(15);
                    var dateCompare30Late = dateCompare2.AddMinutes(30);
                    // Check type of deliveryStatus
                    if (deliveryDate.Value < dateCompare1)
                    {
                        result.Level1 += 1;
                    }
                    else if (deliveryDate >= dateCompare1 && deliveryDate < dateCompare2)
                    {
                        result.Level2 += 1;
                    }
                    else if (deliveryDate >= dateCompare2 && deliveryDate < dateCompare2.AddMinutes(30))
                    {
                        result.Level3 += 1;
                    }
                    else if (deliveryDate >= dateCompare2.AddMinutes(30))
                    {
                        result.Level4 += 1;
                    }
                }
            }

            return result;
        }

        // Pedidos y ticket promedio por CP
        [HttpGet]
        public IActionResult GenerateExcel34(int months = 12)
        {
            var controlDate = DateTime.Now.AddMonths(months * -1).Date;
            var now = DateTime.Now;
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate.Value <= now).ToList();
            var groupedOrders = orders.GroupBy(x => x.ShippingAddress.ZipPostalCode).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Código postal";
                    worksheet.Cells[row, 2].Value = "Cantidad de pedidos";
                    worksheet.Cells[row, 3].Value = "Ticket promedio";

                    var data = new List<decimal>();
                    foreach (var item in groupedOrders)
                    {
                        row++;
                        var pedidos = OrderUtils.GetPedidosGroupByList(item.Select(x => x).ToList()).ToList();

                        worksheet.Cells[row, 1].Value = item.Key;
                        worksheet.Cells[row, 2].Value = pedidos.Count();
                        worksheet.Cells[row, 3].Value = pedidos.Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).Average();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_ticket_pedidos_cp_{months}_meses.xlsx");
            }
        }


        // Usuarios registrados que nunca han hecho un pedido/orden en los ultimos X meses 
        //[HttpGet]
        //public IActionResult GenerateExcel41_UsersWhitOutOrders(int months = 120, bool onlyActiveNotificationMovil = false)
        //{
        //    var controlDate = DateTime.Now.AddMonths(-months).Date;
        //    var today = DateTime.Now.Date;

        //    var orders = _orderService.GetAllOrdersQuery().Where(x => !x.Deleted && x.SelectedShippingDate >= controlDate);
        //    var usersIdsWhitOrders = orders.GroupBy(x => x.CustomerId).Select(x => x.Key).ToList();

        //    var usersWhitOutOrders = _customerService.GetAllCustomers(customerRoleIds: new int[] { 3 }).Where(x => !usersIdsWhitOrders.Contains(x.Id));
        //    if (onlyActiveNotificationMovil)
        //    {
        //        var firebaseCustomersIds = _customerSecurityTokenService.GetAll().Where(x => x.FirebaseToken != null).Select(x => x.CustomerId).ToList();
        //        usersWhitOutOrders = usersWhitOutOrders.Where(x => firebaseCustomersIds.Contains(x.Id));
        //    }
        //    using (var stream = new MemoryStream())
        //    {
        //        using (var xlPackage = new ExcelPackage(stream))
        //        {
        //            var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
        //            int row = 1;
        //            worksheet.Cells[row, 1].Value = "Id";
        //            worksheet.Cells[row, 2].Value = "Email";
        //            //worksheet.Cells[row, 2].Value = "Nombre";

        //            foreach (var user in usersWhitOutOrders)
        //            {
        //                row++;
        //                worksheet.Cells[row, 1].Value = user.Id;
        //                worksheet.Cells[row, 2].Value = user.Email;
        //            }
        //            xlPackage.Save();
        //        }

        //        return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_usuarios_sin_ordenes_ult_{months}_meses.xlsx");
        //    }
        //}


        //Pedidos de entre {from} a {to} pesos, codigos postales
        //Si se especifica Id de categoria:
        //Pedidos que hayan pedido más de {from} pesos de la categoría {categoryId} (especificar {to} como 0 en la URL para que de bien el resultado para este caso)
        [HttpGet]
        public IActionResult HeatMapData1(int days = 120, int from = 0, int to = 350,
            int categoryId = 0)
        {
            var controlDate = DateTime.Now.AddDays(days * -1).Date;
            var today = DateTime.Now.Date;
            var orders = GetFilteredOrders()
                .Where(x => x.SelectedShippingDate.HasValue &&
                controlDate <= DbFunctions.TruncateTime(x.SelectedShippingDate.Value) &&
                DbFunctions.TruncateTime(x.SelectedShippingDate.Value) <= today)
                .Where(x => !string.IsNullOrEmpty(x.ShippingAddress.ZipPostalCode))
                .ToList();
            if (from < 1)
                orders = orders.Where(x => x.OrderTotal <= to).ToList();
            else if (to < 1)
                orders = orders.Where(x => from <= x.OrderTotal).ToList();
            else
                orders = orders.Where(x => from <= x.OrderTotal && x.OrderTotal <= to).ToList();

            var category = new Category();
            if (categoryId > 0)
            {
                category = _categoryService.GetCategoryById(categoryId);
                if (category == null)
                    return BadRequest($"Category not found. Id: {categoryId}");
            }

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    if (categoryId > 0)
                        worksheet.Cells[row, 1].Value = $"Pedidos que hayan pedido más de {from} pesos en {category.Name} ultimos {days} días";
                    else
                        worksheet.Cells[row, 1].Value = $"Pedidos de {from} a {to} pesos, códigos postales ultimos {days} días";
                    row++;
                    worksheet.Cells[row, 1].Value = "Código Postal";

                    foreach (var order in orders)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = order.ShippingAddress.ZipPostalCode;

                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                        worksheet.Cells[2, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                if (categoryId > 0)
                    return File(stream.ToArray(), MimeTypes.TextXlsx,
                        $"cel_pedidos_que_hayan_pedido_ mas_de_{from}_pesos_en_{category.Name}_ultimos_{days}_dias.xlsx");
                else
                    return File(stream.ToArray(), MimeTypes.TextXlsx,
                        $"cel_pedidos_de_{from}_a_{to}_pesos_codigos_postales_ultimos_{days}_dias.xlsx");
            }
        }

        //Usuarios que hayan pedido {months} veces al mes.
        //Usar {moreThan} para que sea: Usuarios que hayan pedido más de {months} veces al mes.
        [HttpGet]
        public IActionResult HeatMapData2(int days = 120, int months = 1, bool moreThan = false)
        {
            var controlDate = DateTime.Now.AddDays(days * -1).Date;
            var today = DateTime.Now.Date;
            var ordersGroupedByCustomer = GetFilteredOrders()
                .Where(x => x.SelectedShippingDate.HasValue &&
                controlDate <= DbFunctions.TruncateTime(x.SelectedShippingDate.Value) &&
                DbFunctions.TruncateTime(x.SelectedShippingDate.Value) <= today)
                .GroupBy(x => x.Customer)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    if (moreThan)
                        worksheet.Cells[row, 1].Value = $"Usuarios que hayan pedido más de {months} veces al mes, ultimos {days} días";
                    else
                        worksheet.Cells[row, 1].Value = $"Usuarios que hayan pedido {months} veces al mes, ultimos {days} días";
                    row++;
                    worksheet.Cells[row, 1].Value = "Código Postal";

                    foreach (var orders in ordersGroupedByCustomer)
                    {
                        var customerOrdersByMonths = orders
                            .GroupBy(x => x.SelectedShippingDate?.Month)
                            .ToList();
                        if (months > 0)
                        {
                            if (moreThan)
                                customerOrdersByMonths = customerOrdersByMonths.Where(x => x.Count() > months).ToList();
                            else
                                customerOrdersByMonths = customerOrdersByMonths.Where(x => x.Count() == months).ToList();
                        }
                        foreach (var ordersForZipCodes in customerOrdersByMonths)
                        {
                            var zipCodes = ordersForZipCodes.ToList()
                                .Select(x => x.ShippingAddress.ZipPostalCode)
                                .Distinct()
                                .ToList();
                            foreach (var zipCode in zipCodes)
                            {
                                row++;
                                worksheet.Cells[row, 1].Value = zipCode;
                            }
                        }
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                        worksheet.Cells[2, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                if (moreThan)
                    return File(stream.ToArray(), MimeTypes.TextXlsx,
                        $"cel_usuarios_que_hayan_pedido_mas_de_{months}_veces_al_mes_ultimos_{days}_dias.xlsx");
                else
                    return File(stream.ToArray(), MimeTypes.TextXlsx,
                        $"cel_usuarios_que_hayan_pedido_{months}_veces_al_mes_ultimos_{days}_dias.xlsx");
            }
        }

        // Pedidos que hayan tenido un producto de fabricante {manufacturerId}
        // O
        // Productos que hayan tenido un {productName} (que incluye el string espeicifcado)
        [HttpGet]
        public IActionResult HeatMapData3(int days = 120, string productName = "", int manufacturerId = 0)
        {
            var controlDate = DateTime.Now.AddDays(days * -1).Date;
            var today = DateTime.Now.Date;
            var orders = GetFilteredOrders()
                .Where(x => x.SelectedShippingDate.HasValue &&
                controlDate <= DbFunctions.TruncateTime(x.SelectedShippingDate.Value) &&
                DbFunctions.TruncateTime(x.SelectedShippingDate.Value) <= today)
                .Where(x => !string.IsNullOrEmpty(x.ShippingAddress.ZipPostalCode))
                .ToList();

            var manufacturer = new Manufacturer();
            if (!string.IsNullOrEmpty(productName))
                orders = orders.SelectMany(x => x.OrderItems)
                    .Where(x => x.Product.Name.ToLower().Contains(productName.ToLower()))
                    .Select(y => y.Order).ToList();
            else if (manufacturerId > 0)
            {
                manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);
                if (manufacturer == null)
                    BadRequest("Manufacturer not found. id: " + manufacturerId);
                orders = orders.SelectMany(x => x.OrderItems)
                    .Where(x => x.Product.ProductManufacturers
                    .Select(y => y.ManufacturerId).Contains(manufacturerId))
                    .Select(y => y.Order)
                    .ToList();
            }
            else
                BadRequest("No name of product or manufacturer given");

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    if (manufacturerId > 0)
                        worksheet.Cells[row, 1].Value = $"Pedidos que hayan tenido un producto de {manufacturer.Name}, ultimos {days} días";
                    else
                        worksheet.Cells[row, 1].Value = $"Pedidos que hayan tenido un \"{productName}\", ultimos {days} días";
                    //orders = orders.Where(x => x.ShippingAddress.ZipPostalCode == "01220").ToList();
                    row++;
                    worksheet.Cells[row, 1].Value = "Código Postal";

                    foreach (var order in orders)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = order.ShippingAddress.ZipPostalCode;

                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                        worksheet.Cells[2, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                if (manufacturerId > 0)
                    return File(stream.ToArray(), MimeTypes.TextXlsx,
                        $"cel_pedidos_que_hayan_tenido_un_producto_de_{manufacturer.Name}_ultimos_{days}_dias.xlsx");
                else
                    return File(stream.ToArray(), MimeTypes.TextXlsx,
                        $"cel_pedidos_que_hayan_tenido_un_{productName}_ultimos_{days}_dias.xlsx");
            }
        }

        [HttpGet]
        public IActionResult GetOrderGroupsByDiscountsAndShippingInfo(string from = null, string until = null, string couponCodes = null)
        {
            DateTime? fromDate = null;
            if (!string.IsNullOrEmpty(from))
                fromDate = DateTime.ParseExact(from, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var untilDate = new DateTime(2021, 9, 30);
            if (!string.IsNullOrEmpty(until))
                untilDate = DateTime.ParseExact(until, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var coupons = couponCodes.Split(',');

            var orders = GetFilteredOrders();
            if (coupons.Any())
            {
                var filteredOrders = orders
                .Where(x => x.SelectedShippingDate <= untilDate);
                if (fromDate != null)
                    filteredOrders = filteredOrders
                        .Where(x => fromDate <= x.SelectedShippingDate);

                var ordersWithCoupons = filteredOrders
                    .Where(x =>
                    coupons.Any(y => x.DiscountUsageHistory.Select(z => z.Discount.CouponCode).Contains(y)))
                    .ToList();

                var pedidos = OrderUtils.GetPedidosGroupByList(ordersWithCoupons).ToList();
                var customerIds = pedidos.Select(x => x.FirstOrDefault().CustomerId).ToList();

                var customers = _customerService.GetAllCustomersQuery()
                    .Where(x => customerIds.Contains(x.Id)).ToList();

                using (var stream = new MemoryStream())
                {
                    using (var xlPackage = new ExcelPackage(stream))
                    {
                        var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                        int row = 1;
                        worksheet.Cells[row, 1].Value = "Id del cliente";
                        worksheet.Cells[row, 2].Value = "Número de orden(es)";
                        worksheet.Cells[row, 3].Value = "Fecha de entrega";
                        worksheet.Cells[row, 4].Value = "Monto total de la compra";
                        worksheet.Cells[row, 5].Value = "Código postal";
                        row++;

                        foreach (var pedido in pedidos)
                        {
                            var ordersOfCustomer = pedido.ToList();
                            worksheet.Cells[row, 1].Value = pedido.FirstOrDefault().CustomerId;
                            worksheet.Cells[row, 2].Value = string.Join(", ", ordersOfCustomer.Select(x => "#" + x.CustomOrderNumber));
                            worksheet.Cells[row, 3].Value = (pedido.FirstOrDefault().SelectedShippingDate ?? DateTime.Now).ToString("dd/MM/yyyy");
                            worksheet.Cells[row, 4].Value = ordersOfCustomer.Select(x => x.OrderSubtotalInclTax).DefaultIfEmpty().Sum();
                            worksheet.Cells[row, 5].Value = pedido.FirstOrDefault().ShippingAddress.ZipPostalCode;
                            row++;
                        }

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }

                        xlPackage.Save();
                    }

                    return File(stream.ToArray(), MimeTypes.TextXlsx, $"Reporte de resultados de Proyecto Especial de MKT en Unidades Habitacionales PE01 " + DateTime.Now.ToString("dd-MM-yyyy") + ".xlsx");
                }
            }

            return Ok();
        }

        [HttpGet]
        public IActionResult GetOrderCountByStreet()
        {
            var fromDate = new DateTime(2021, 3, 1).Date;
            var until = new DateTime(2021, 9, 30).Date;

            var orders = _orderService.GetAllOrdersQuery().Where(x => x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10))
                .Where(x => fromDate <= x.SelectedShippingDate && x.SelectedShippingDate <= until)
                .GroupBy(x => x.ShippingAddress.Address1).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Calle";
                    worksheet.Cells[row, 2].Value = "Número de ordenes";
                    row++;

                    foreach (var order in orders)
                    {
                        var ordersOfCustomer = order.ToList();
                        worksheet.Cells[row, 1].Value = order.Key;
                        worksheet.Cells[row, 2].Value = order.Count();
                        row++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"Reporte numero de ordenes por calle " + DateTime.Now.ToString("dd-MM-yyyy") + ".xlsx");
            }
        }


        [HttpGet]
        public IActionResult GetUserCountByDate(int year = 2021, int month = 5)
        {
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => x.CreatedOnUtc.Year == year &&
                x.CreatedOnUtc.Month == month &&
                x.Email != null && x.Email != "").ToList();

            return Ok(customers.Count());
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("GenerateExcelUserNewsletter")]
        public IActionResult GenerateExcelUserNewsletter()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.NewsLetter))
                return AccessDeniedView();

            string customerQuery = System.IO.File.ReadAllText("Plugins/Teed.Plugin.Groceries/src/files/sql/customer-complete-NewsLetter.sql");
            List<CustomerResults> customerQR = _dbContext.SqlQuery<CustomerResults>(customerQuery).ToList();

            string newsLetterQuery = System.IO.File.ReadAllText("Plugins/Teed.Plugin.Groceries/src/files/sql/only-newsLetter.sql");
            List<NewsletterResults> newsLetterQR = _dbContext.SqlQuery<NewsletterResults>(newsLetterQuery).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("Registrados");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "CustomerID";
                    worksheet.Cells[row, 2].Value = "Nombre";
                    worksheet.Cells[row, 3].Value = "Apellido";
                    worksheet.Cells[row, 4].Value = "Correo";
                    worksheet.Cells[row, 5].Value = "Género";
                    worksheet.Cells[row, 6].Value = "Fecha de registro";
                    worksheet.Cells[row, 7].Value = "ActiveNewsletter";

                    foreach (var user in customerQR)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = user.Id;
                        worksheet.Cells[row, 2].Value = user.Nombre;
                        worksheet.Cells[row, 3].Value = user.Apellido;
                        worksheet.Cells[row, 4].Value = user.Correo;
                        worksheet.Cells[row, 5].Value = user.Genero;
                        worksheet.Cells[row, 6].Value = user.Fecha_registro;
                        if (user.ActiveNewsletter.HasValue) { worksheet.Cells[row, 7].Value = user.ActiveNewsletter.Value; }
                        else { worksheet.Cells[row, 7].Value = "No activo"; }

                    }

                    worksheet = xlPackage.Workbook.Worksheets.Add("Newsletter");
                    row = 1;
                    worksheet.Cells[row, 1].Value = "CustomerID";
                    worksheet.Cells[row, 2].Value = "Activo";

                    foreach (var newsletterRow in newsLetterQR)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = newsletterRow.Email;
                        worksheet.Cells[row, 2].Value = newsletterRow.Active;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"Reporte_usuarios.xlsx");
            }
        }

        private IQueryable<Order> GetFilteredOrders()
        {
            return _orderService.GetAllOrdersQuery()
                .Where(x => !x.Deleted && x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10));
        }

        private IQueryable<Product> GetFilteredProducts()
        {
            return _productService.GetAllProductsQuery()
                .Where(x => !x.Deleted && x.Published);
        }

        private List<OrderItemReportModel> GetOrderItemReport()
        {
            var approvedReports = _orderReportStatusService.GetAll()
                //.Where(x => x.StatusTypeId == 2)
                .Select(x => new OrderReportFilterModel()
                {
                    BuyerId = x.BuyerId,
                    ShippingDate = x.ShippingDate
                }).ToList();

            var orderReports = _orderReportService.GetAll().Where(x => x.UnitCost > 0).ToList();

            List<OrderReport> filteredReports = new List<OrderReport>();
            filteredReports = filteredReports.Union(orderReports.Where(x => x.ProductId == 0).ToList()).ToList();
            foreach (var report in orderReports.Where(x => x.ProductId != 0))
            {
                var aprovedReport = approvedReports.Where(x => x.BuyerId == report.OriginalBuyerId && x.ShippingDate.Date == report.OrderShippingDate.Date).FirstOrDefault();
                if (aprovedReport != null)
                    filteredReports.Add(report);
            }

            var orderItems = _orderService.GetAllOrdersQuery().Select(x => x.OrderItems).SelectMany(x => x).ToList();
            return filteredReports.Join(orderItems, orderReport => orderReport.OrderItemId, orderItem => orderItem.Id, (orderReport, orderItem) => new OrderItemReportModel()
            {
                OrderItem = orderItem,
                OrderReport = orderReport
            }).ToList();
        }

        private decimal GetRequestedCost(OrderItem item, decimal unitCost)
        {
            if (item.EquivalenceCoefficient > 0)
            {
                var result = (item.Quantity * 1000) / item.EquivalenceCoefficient;
                return (result * unitCost) / 1000;
            }
            else if (item.WeightInterval > 0)
            {
                var result = item.Quantity * item.WeightInterval;
                if (result > 0)
                    return (result * unitCost) / 1000;
                else
                    return 0;
            }
            else
            {
                return unitCost * item.Quantity;
            }
        }
    }

    public class CustomerResults
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Genero { get; set; }
        public string CP { get; set; }
        public DateTime Fecha_registro { get; set; }
        public bool? ActiveNewsletter { get; set; }
    }

    public class GenerateExcel133Model
    {
        public DateTime OrderShippingDate { get; set; }
        public int ProductId { get; set; }
        public decimal? UpdatedRequestedQtyCost { get; set; }
        public int OriginalBuyerId { get; set; }
        public int? ManufacturerId { get; set; }
        public string ShoppingStoreId { get; set; }
    }

    public class NewsletterResults
    {
        public string Email { get; set; }
        public bool Active { get; set; }
    }

    public class MassiveData1
    {
        public int ProductId { get; set; }
        public string CarefulAttribute { get; set; }
        public string StringAttribute { get; set; }
    }

    public class WeightPriceCalculationResult
    {
        public string Weight { get; set; }
        public decimal SellPrice { get; set; }
    }

    public class DeliveryStatusResult
    {
        public int Level1 { get; set; }
        public int Level2 { get; set; }
        public int Level3 { get; set; }
        public int Level4 { get; set; }
    }

    public class MarketingDashboardData
    {
        public decimal Client30DaysAfterCount { get; set; }
        public decimal Client60DaysAfterCount { get; set; }
        public decimal Client90DaysAfterCount { get; set; }
        public decimal Client120DaysAfterCount { get; set; }
        public decimal Client150DaysAfterCount { get; set; }
        public decimal Client180DaysAfterCount { get; set; }
        public decimal Client210DaysAfterCount { get; set; }
        public decimal Client240DaysAfterCount { get; set; }
        public decimal Client270DaysAfterCount { get; set; }
        public decimal Client300DaysAfterCount { get; set; }
        public decimal Client330DaysAfterCount { get; set; }
        public decimal Client360DaysAfterCount { get; set; }
        public decimal Client390DaysAfterCount { get; set; }
        public decimal Client420DaysAfterCount { get; set; }
        public decimal Client450DaysAfterCount { get; set; }
        public decimal Client480DaysAfterCount { get; set; }
        public decimal Client510DaysAfterCount { get; set; }
        public decimal Client540DaysAfterCount { get; set; }
        public decimal Client570DaysAfterCount { get; set; }
        public decimal Client600DaysAfterCount { get; set; }
        public decimal Client630DaysAfterCount { get; set; }
        public decimal Client660DaysAfterCount { get; set; }
        public decimal Client690DaysAfterCount { get; set; }
        public decimal Client720DaysAfterCount { get; set; }
        public decimal Client30DaysAfterPercentage { get; set; }
        public decimal Client60DaysAfterPercentage { get; set; }
        public decimal Client90DaysAfterPercentage { get; set; }
        public decimal Client120DaysAfterPercentage { get; set; }
        public decimal Client150DaysAfterPercentage { get; set; }
        public decimal Client180DaysAfterPercentage { get; set; }
        public decimal Client210DaysAfterPercentage { get; set; }
        public decimal Client240DaysAfterPercentage { get; set; }
        public decimal Client270DaysAfterPercentage { get; set; }
        public decimal Client300DaysAfterPercentage { get; set; }
        public decimal Client330DaysAfterPercentage { get; set; }
        public decimal Client360DaysAfterPercentage { get; set; }
        public decimal Client390DaysAfterPercentage { get; set; }
        public decimal Client420DaysAfterPercentage { get; set; }
        public decimal Client450DaysAfterPercentage { get; set; }
        public decimal Client480DaysAfterPercentage { get; set; }
        public decimal Client510DaysAfterPercentage { get; set; }
        public decimal Client540DaysAfterPercentage { get; set; }
        public decimal Client570DaysAfterPercentage { get; set; }
        public decimal Client600DaysAfterPercentage { get; set; }
        public decimal Client630DaysAfterPercentage { get; set; }
        public decimal Client660DaysAfterPercentage { get; set; }
        public decimal Client690DaysAfterPercentage { get; set; }
        public decimal Client720DaysAfterPercentage { get; set; }

        public decimal Client30DaysAfterTicket { get; set; }
        public decimal Client60DaysAfterTicket { get; set; }
        public decimal Client90DaysAfterTicket { get; set; }
        public decimal Client120DaysAfterTicket { get; set; }
        public decimal Client150DaysAfterTicket { get; set; }
        public decimal Client180DaysAfterTicket { get; set; }
        public decimal Client210DaysAfterTicket { get; set; }
        public decimal Client240DaysAfterTicket { get; set; }
        public decimal Client270DaysAfterTicket { get; set; }
        public decimal Client300DaysAfterTicket { get; set; }
        public decimal Client330DaysAfterTicket { get; set; }
        public decimal Client360DaysAfterTicket { get; set; }
        public decimal Client390DaysAfterTicket { get; set; }
        public decimal Client420DaysAfterTicket { get; set; }
        public decimal Client450DaysAfterTicket { get; set; }
        public decimal Client480DaysAfterTicket { get; set; }
        public decimal Client510DaysAfterTicket { get; set; }
        public decimal Client540DaysAfterTicket { get; set; }
        public decimal Client570DaysAfterTicket { get; set; }
        public decimal Client600DaysAfterTicket { get; set; }
        public decimal Client630DaysAfterTicket { get; set; }
        public decimal Client660DaysAfterTicket { get; set; }
        public decimal Client690DaysAfterTicket { get; set; }
        public decimal Client720DaysAfterTicket { get; set; }
        public decimal Client30DaysAfterRecurrence { get; set; }
        public decimal Client60DaysAfterRecurrence { get; set; }
        public decimal Client90DaysAfterRecurrence { get; set; }
        public decimal Client120DaysAfterRecurrence { get; set; }
        public decimal Client150DaysAfterRecurrence { get; set; }
        public decimal Client180DaysAfterRecurrence { get; set; }
        public decimal Client210DaysAfterRecurrence { get; set; }
        public decimal Client240DaysAfterRecurrence { get; set; }
        public decimal Client270DaysAfterRecurrence { get; set; }
        public decimal Client300DaysAfterRecurrence { get; set; }
        public decimal Client330DaysAfterRecurrence { get; set; }
        public decimal Client360DaysAfterRecurrence { get; set; }
        public decimal Client390DaysAfterRecurrence { get; set; }
        public decimal Client420DaysAfterRecurrence { get; set; }
        public decimal Client450DaysAfterRecurrence { get; set; }
        public decimal Client480DaysAfterRecurrence { get; set; }
        public decimal Client510DaysAfterRecurrence { get; set; }
        public decimal Client540DaysAfterRecurrence { get; set; }
        public decimal Client570DaysAfterRecurrence { get; set; }
        public decimal Client600DaysAfterRecurrence { get; set; }
        public decimal Client630DaysAfterRecurrence { get; set; }
        public decimal Client660DaysAfterRecurrence { get; set; }
        public decimal Client690DaysAfterRecurrence { get; set; }
        public decimal Client720DaysAfterRecurrence { get; set; }

        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal FirstPedidosCount { get; set; }
    }

    public class CustomerIdssOfCouponPrefix
    {
        public CustomerIdssOfCouponPrefix()
        {
            DiscountIds = new List<int>();
        }

        public string Prefix { get; set; }
        public List<int> DiscountIds { get; set; }
    }

    public class TempModel
    {
        public virtual int Id { get; set; }
        public virtual DateTime SelectedShippingDate { get; set; }
        public virtual int CustomerId { get; set; }
    }
}