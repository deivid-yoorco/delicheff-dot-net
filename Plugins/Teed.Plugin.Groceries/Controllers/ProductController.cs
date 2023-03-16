using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Tasks;
using Nop.Services.Tax;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.OrderReports;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class ProductController : BasePluginController
    {
        private const string API_URL = "https://www.centralenlinea.com/api";
        //private const string API_URL = "https://localhost:44345/api";

        private readonly IPermissionService _permissionService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductService _productService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IWorkContext _workContext;
        private readonly IPriceLogService _priceLogService;
        private readonly IOrderService _orderService;
        private readonly VendorSettings _vendorSettings;
        private readonly Services.OrderReportService _orderReportService;
        private readonly OrderReportStatusService _orderReportStatusService;
        private readonly IProductLogService _productLogService;
        private readonly IShippingService _shippingService;
        private readonly IUrlRecordService _recordService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IProductTagService _productTagService;
        private readonly ICustomerService _customerService;
        private readonly IPaymentService _paymentService;
        private readonly ProductPricePendingUpdateService _productPricePendingUpdateService;
        private readonly NotDeliveredOrderItemService _notDeliveredOrderItemService;

        public ProductController(IPermissionService permissionService,
            ICategoryService categoryService, IProductService productService,
            IScheduleTaskService scheduleTaskService, IWorkContext workContext, IPriceLogService priceLogService,
            IOrderService orderService, Services.OrderReportService orderReportService, OrderReportStatusService orderReportStatusService,
            VendorSettings vendorSettings, IManufacturerService manufacturerService, ISpecificationAttributeService specificationAttributeService,
            IProductLogService productLogService, IShippingService shippingService, IUrlRecordService recordService,
            ITaxCategoryService taxCategoryService, IProductTagService productTagService, ProductPricePendingUpdateService productPricePendingUpdateService,
            ICustomerService customerService, IPaymentService paymentService, NotDeliveredOrderItemService notDeliveredOrderItemService)
        {
            _permissionService = permissionService;
            _categoryService = categoryService;
            _productService = productService;
            _scheduleTaskService = scheduleTaskService;
            _workContext = workContext;
            _priceLogService = priceLogService;
            _orderService = orderService;
            _orderReportService = orderReportService;
            _orderReportStatusService = orderReportStatusService;
            _vendorSettings = vendorSettings;
            _manufacturerService = manufacturerService;
            _specificationAttributeService = specificationAttributeService;
            _productLogService = productLogService;
            _shippingService = shippingService;
            _recordService = recordService;
            _taxCategoryService = taxCategoryService;
            _productTagService = productTagService;
            _productPricePendingUpdateService = productPricePendingUpdateService;
            _customerService = customerService;
            _paymentService = paymentService;
            _notDeliveredOrderItemService = notDeliveredOrderItemService;
        }

        [HttpGet]
        public IActionResult UpdatePrices()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UpdatePrices))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/UpdatePrices/Index.cshtml");
        }

        [HttpPost]
        public IActionResult UpdateCategoryPricesData()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UpdatePrices))
                return AccessDeniedView();

            var pendingToUpdateProductIds = _productPricePendingUpdateService.GetAll().Select(x => x.ProductId).ToList();
            var productsGroup = _productService.GetAllProductsQuery()
                .Where(x => x.Published && pendingToUpdateProductIds.Contains(x.Id))
                .GroupBy(x => x.ProductCategories.Where(y => y.Category.ParentCategoryId > 0).Select(y => y.Category).FirstOrDefault())
                .ToList();

            var model = productsGroup.Select(x =>
            {
                var productIds = x.Select(y => y.Id).ToList();
                var averageProductPrice = x.Select(y => y.Price).DefaultIfEmpty().Average();
                var averageProductCost = x.Select(y => y.ProductCost).DefaultIfEmpty().Average();
                var currentMargin = averageProductPrice == 0 ? 0 : (1 - (averageProductCost / averageProductPrice));

                return new
                {
                    CategoryName = x.Key == null ? "Sin categoría hijo" : x.Key.Name,
                    CategoryId = x.Key == null ? 0 : x.Key.Id,
                    CurrentMargin = (Math.Round(currentMargin, 2) * 100).ToString() + "%",
                    CurrentMarginValue = currentMargin
                };
            }).OrderBy(x => x.CurrentMarginValue).ToList();

            var gridModel = new DataSourceResult
            {
                Data = model,
                Total = model.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult UpdateProductPrices(UpdatePricesModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UpdatePrices))
                return AccessDeniedView();

            var productIds = model.Data.Select(x => x.ProductId).ToList();
            var products = _productService.GetAllProductsQuery().Where(x => productIds.Contains(x.Id)).ToList();
            foreach (var data in model.Data)
            {
                var product = products.Where(x => x.Id == data.ProductId).FirstOrDefault();
                if (product == null) continue;
                var log = string.Empty;
                if (data.UpdatedCost > 0 && Math.Round(data.UpdatedCost, 2) != Math.Round(product.ProductCost, 2))
                {
                    log += $" Actualizó el costo del producto de {product.ProductCost} a {data.UpdatedCost}.";
                    product.ProductCost = data.UpdatedCost;
                }

                if (data.UpdatedPrice > 0 && Math.Round(data.UpdatedPrice, 2) != Math.Round(product.Price, 2))
                {
                    log += $" Actualizó el precio del producto de {product.Price} a {data.UpdatedPrice}.";
                    product.Price = data.UpdatedPrice;

                    var pendingUpdatePrice = _productPricePendingUpdateService.GetAll().Where(x => x.ProductId == product.Id).FirstOrDefault();
                    if (pendingUpdatePrice != null)
                        _productPricePendingUpdateService.Delete(pendingUpdatePrice);
                }

                if (!string.IsNullOrEmpty(log))
                {
                    log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) actualizó el producto." + log;
                    _productLogService.InsertProductLog(new ProductLog()
                    {
                        Message = log,
                        ProductId = product.Id,
                        UserId = _workContext.CurrentCustomer.Id,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _productService.UpdateProduct(product);
                }
            }

            return NoContent();
        }

        [HttpGet]
        public IActionResult UpdateCategoryPrices(int categoryId)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UpdatePrices))
                return AccessDeniedView();

            var pendingToUpdateProductIds = _productPricePendingUpdateService.GetAll().Select(x => x.ProductId).ToList();
            var products = _productService.GetAllProductsQuery().Where(x => x.Published && pendingToUpdateProductIds.Contains(x.Id) && x.ProductCategories.Where(y => y.CategoryId == categoryId).Any()).ToList();

            var reportStatus = _orderReportStatusService.GetAll().Where(x => x.StatusTypeId == 2)
               .Select(x => DbFunctions.AddMilliseconds(x.ShippingDate, x.BuyerId))
               .Distinct()
               .ToList();
            var reports = _orderReportService.GetAll()
               .Where(x => pendingToUpdateProductIds.Contains(x.ProductId))
               .Select(x => new { x.ReportedByCustomerId, x.OrderShippingDate, x.ProductId, x.ReportedDateUtc, x.UpdatedUnitCost, x.OriginalBuyerId })
               .ToList()
               .Where(x => reportStatus.Contains(x.OrderShippingDate.AddMilliseconds(x.ReportedByCustomerId)))
               .ToList();

            var buyerIds = reports.GroupBy(x => x.ReportedByCustomerId).Select(x => x.Key).ToList();
            var customers = _customerService.GetAllCustomersQuery().Where(x => buyerIds.Contains(x.Id)).ToList();

            var model = products.Select(x =>
            {
                var buyerMargin = x.Price == 0 ? 0 : (1 - (reports.Where(y => y.ProductId == x.Id).OrderByDescending(y => y.ReportedDateUtc).Select(y => y.UpdatedUnitCost).FirstOrDefault() / x.Price));
                var currentMargin = x.Price == 0 ? 0 : (1 - (x.ProductCost / x.Price));
                var lastReported = reports.Where(y => y.ProductId == x.Id)
                .GroupBy(y => y.OrderShippingDate)
                .OrderByDescending(y => y.Key)
                .Take(10)
                .Select(y => y.FirstOrDefault())
                .Select(y => $"{y.ReportedDateUtc.ToLocalTime():dd-MM-yyyy} - {customers.Where(z => z.Id == y.OriginalBuyerId).FirstOrDefault().GetFullName()} - {y.UpdatedUnitCost:C}")
                .ToList();

                return new ProductPriceUpdateModel()
                {
                    ProductId = x.Id,
                    ProductName = x.Name,
                    CurrentCostValue = x.ProductCost,
                    BuyerMargin = Math.Round(buyerMargin * 100, 2).ToString() + "%",
                    CurrentMargin = Math.Round(currentMargin * 100, 2).ToString() + "%",
                    CurrentPrice = x.Price.ToString("C"),
                    CurrentPriceValue = x.Price,
                    LastReportedCosts = lastReported,
                    LastReportedCostValue = reports.Where(y => y.ProductId == x.Id).OrderByDescending(y => y.ReportedDateUtc).Select(y => y.UpdatedUnitCost).FirstOrDefault()
                };
            }).OrderBy(x => x.CurrentMargin).ToList();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/UpdatePrices/UpdateCategoryPrices.cshtml", model);
        }

        [HttpGet]
        public IActionResult GetSalesData(string from, string to)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            List<IGrouping<Category, OrderItem>> orderItemsByCategory = new List<IGrouping<Category, OrderItem>>();
            int[] orderIdsWithReport = _orderReportService.GetAll().Select(x => x.OrderId).Distinct().ToArray();

            //// Search by date
            DateTime? dateFirst = null;
            DateTime? dateLast = null;

            if (!string.IsNullOrEmpty(from))
            {
                dateFirst = DateTime.ParseExact(from.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(to))
            {
                dateLast = DateTime.ParseExact(to.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            var orderItemsQuery = _orderService.GetAllOrdersQuery()
                    .Where(x => !x.Deleted)
                    .Where(x => x.OrderStatusId == 30)
                    .Where(x => orderIdsWithReport.Contains(x.Id));

            if (dateFirst != null && dateLast != null)
            {
                bool isFirst = Nullable.Compare(dateFirst, dateLast) > 0;
                if (isFirst)
                {
                    DateTime? tempDate = new DateTime();
                    tempDate = dateLast;
                    dateLast = dateFirst;
                    dateFirst = tempDate;
                }

                orderItemsQuery = orderItemsQuery.Where(x => DbFunctions.TruncateTime(x.SelectedShippingDate.Value) >= dateFirst.Value &&
                    DbFunctions.TruncateTime(x.SelectedShippingDate.Value) <= dateLast.Value);
            }
            else if (dateFirst == null && dateLast != null)
            {
                orderItemsQuery = orderItemsQuery.Where(x => DbFunctions.TruncateTime(x.SelectedShippingDate.Value) <= dateLast.Value);
            }
            else if (dateFirst != null && dateLast == null)
            {
                orderItemsQuery = orderItemsQuery
                    .Where(x => DbFunctions.TruncateTime(x.SelectedShippingDate.Value) == dateFirst.Value);
            }
            else
            {
                orderItemsQuery = orderItemsQuery
                    .Where(x => DbFunctions.TruncateTime(x.SelectedShippingDate.Value) <= DbFunctions.TruncateTime(DateTime.Now));
            }

            orderItemsByCategory = orderItemsQuery
                                    .Select(x => x.OrderItems)
                                    .SelectMany(x => x)
                                    .GroupBy(x => x.Product.ProductCategories
                                        .Where(y => y.Category.ParentCategoryId == 0)
                                        .OrderBy(y => y.DisplayOrder)
                                        .Select(y => y.Category)
                                        .FirstOrDefault())
                                    .ToList();

            decimal totalSales = orderItemsByCategory.SelectMany(x => x)
                .Select(x => x.PriceInclTax)
                .DefaultIfEmpty()
                .Sum();
            decimal totalCost = GetTotalCost(orderItemsByCategory.SelectMany(x => x).Select(x => x.OrderId).ToArray());

            List<CategorySalesData> sales = new List<CategorySalesData>();
            List<string> errorMessages = new List<string>();

            foreach (var item in orderItemsByCategory)
            {
                if (item.Key == null)
                {
                    errorMessages.AddRange(item.Select(x => $"El producto {x.Product.Name} (SKU: {x.Product.Sku}) no tiene categoría padre.").Distinct().ToList());
                    continue;
                }

                decimal categorySales = item.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
                decimal categoryCost = GetItemsCost(item.Select(x => x.Id).ToArray());
                decimal categoryMargin = 1 - (categoryCost / categorySales);
                decimal salesPercentage = totalSales > 0 ? categorySales / totalSales : 0;

                sales.Add(new CategorySalesData()
                {
                    CategoryName = item.Key.Name,
                    CategoryId = item.Key.Id,
                    Sales = categorySales,
                    Cost = categoryCost,
                    SalesPercentage = salesPercentage,
                    AverageMargin = categoryMargin,
                    FinalMargin = salesPercentage * categoryMargin
                });
            }

            ResultModel result = new ResultModel()
            {
                Data = sales.OrderByDescending(x => x.FinalMargin).ToList(),
                TotalSales = totalSales,
                TotalCost = totalCost,
                CurrentPercentage = totalSales > 0 ? (1 - (totalCost / totalSales)) * 100 : 0,
                ErrorMessages = errorMessages
            };

            return Ok(result);
        }

        [HttpPost]
        public IActionResult ListDataLog(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UpdatePrices))
                return AccessDeniedView();

            var query = _priceLogService.GetAllQuery().OrderByDescending(x => x.CreatedOnUtc);
            var pagedList = new PagedList<PriceLog>(query, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.ToList().Select(x => new
                {
                    Date = x.CreatedOnUtc.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt"),
                    ProductName = x.Product,
                    Sku = x.SKU,
                    x.OldPrice,
                    x.NewPrice,
                    x.User
                }),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        //[HttpGet]
        //[AllowAnonymous]
        //public IActionResult CreateScheduleTask()
        //{
        //    _scheduleTaskService.InsertTask(new Nop.Core.Domain.Tasks.ScheduleTask()
        //    {
        //        Enabled = true,
        //        Name = "Set Automatic Shipping Route User",
        //        Seconds = 86400,
        //        StopOnError = false,
        //        Type = "Teed.Plugin.Groceries.ScheduleTasks.ShippingRouteScheduleTask",
        //        LastSuccessUtc = new DateTime(2019, 03, 12, 06, 00, 00)
        //    });

        //    return Ok();
        //}

        [HttpGet]
        public IActionResult DownloadCelProducts()
        {
            var products = _productService.GetAllProductsQuery().Where(x => !x.Deleted).ToList();
            var taxCategories = _taxCategoryService.GetAllTaxCategories().ToList();

            List<ProductSatCode> productSatCodesList = new List<ProductSatCode>();
            using (HttpClient client = new HttpClient())
            {
                string url = (Request.IsHttps ? "https://" : "http://") + $"{Request.Host}/Admin/Facturify/GetAllProductSatCode";
                var result = client.GetAsync(url).Result;
                if (!result.IsSuccessStatusCode)
                {
                    string json = result.Content.ReadAsStringAsync().Result;
                    Debugger.Break();
                }
                else
                {
                    var response = result.Content.ReadAsStringAsync();
                    productSatCodesList = JsonConvert.DeserializeObject<List<ProductSatCode>>(response.Result);
                }
            }

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Id";
                    worksheet.Cells[row, 2].Value = "Sku";
                    worksheet.Cells[row, 3].Value = "Nombre";
                    worksheet.Cells[row, 4].Value = "Categorías padre";
                    worksheet.Cells[row, 5].Value = "Categorías hijo";
                    worksheet.Cells[row, 6].Value = "Tags";
                    worksheet.Cells[row, 7].Value = "Manojo/Nevera";
                    worksheet.Cells[row, 8].Value = "Dureza";
                    worksheet.Cells[row, 9].Value = "Costo";
                    worksheet.Cells[row, 10].Value = "Impuesto";
                    worksheet.Cells[row, 11].Value = "Fabricantes";
                    worksheet.Cells[row, 12].Value = "Precio";
                    worksheet.Cells[row, 13].Value = "Precio anterior";
                    worksheet.Cells[row, 14].Value = "Publicado";
                    worksheet.Cells[row, 15].Value = "Intervalos de peso (gramos)";
                    worksheet.Cells[row, 16].Value = "Coeficiente de equivalencia";
                    worksheet.Cells[row, 17].Value = "Propiedades de producto";
                    worksheet.Cells[row, 18].Value = "Etiquetado manual";
                    worksheet.Cells[row, 19].Value = "Código del SAT";
                    worksheet.Cells[row, 20].Value = "Permitir actualización automática de precio";



                    foreach (var product in products)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = product.Id;
                        worksheet.Cells[row, 2].Value = product.Sku;
                        worksheet.Cells[row, 3].Value = product.Name;
                        worksheet.Cells[row, 4].Value = string.Join(";", product.ProductCategories.Where(x => !x.Category.Deleted && x.Category.ParentCategoryId == 0).Select(x => x.Category.Name));
                        worksheet.Cells[row, 5].Value = string.Join(";", product.ProductCategories.Where(x => !x.Category.Deleted && x.Category.ParentCategoryId != 0).Select(x => x.Category.Name));
                        worksheet.Cells[row, 6].Value = string.Join(";", product.ProductTags.Select(x => x.Name));
                        worksheet.Cells[row, 7].Value = string.Join(";", product.ProductSpecificationAttributes
                            .Select(x => x.SpecificationAttributeOption)
                            .Where(x => x.Name.ToLower().Trim().Contains("manojo") || x.Name.ToLower().Contains("nevera"))
                            .Select(x => x.Name));
                        worksheet.Cells[row, 8].Value = string.Join(";", product.ProductSpecificationAttributes
                            .Select(x => x.SpecificationAttributeOption)
                            .Where(x => x.Name.ToLower().Trim().Contains("dureza"))
                            .Select(x => x.Name));
                        worksheet.Cells[row, 9].Value = product.ProductCost;
                        worksheet.Cells[row, 10].Value = product.TaxCategoryId > 0 ?
                            taxCategories.Where(x => x.Id == product.TaxCategoryId).FirstOrDefault().Name :
                            "";
                        worksheet.Cells[row, 11].Value = string.Join(";", product.ProductManufacturers.Where(x => !x.Manufacturer.Deleted).Select(x => x.Manufacturer.Name));
                        worksheet.Cells[row, 12].Value = product.Price;
                        worksheet.Cells[row, 13].Value = product.OldPrice;
                        worksheet.Cells[row, 14].Value = product.Published;
                        worksheet.Cells[row, 15].Value = product.WeightInterval;
                        worksheet.Cells[row, 16].Value = product.EquivalenceCoefficient;
                        worksheet.Cells[row, 17].Value = product.PropertiesOptions;
                        worksheet.Cells[row, 18].Value = string.Join(";", product.ProductSpecificationAttributes
                            .Select(x => x.SpecificationAttributeOption)
                            .Where(x => x.SpecificationAttribute.Name.ToLower().Trim().Contains("etiquetado manual"))
                            .Select(x => x.Name));
                        worksheet.Cells[row, 19].Value = productSatCodesList.Where(x => x.ProductId == product.Id).Select(x => x.ProductCatalogId).FirstOrDefault();
                        worksheet.Cells[row, 20].Value = product.AllowAutoUpdatePrice;

                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_productos.xlsx");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportCelProductExcel(IFormFile importexcelfile)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (_workContext.CurrentVendor != null && !_vendorSettings.AllowVendorsToImportProducts)
                //a vendor can not import products
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    return await ProccessExcel(importexcelfile);
                }
                else
                {
                    return BadRequest("El archivo no es válido.");
                }
            }
            catch (Exception exc)
            {
                return BadRequest("Ocurrió un problema al cargar los productos: " + exc.Message);
            }
        }

        int currentRow = 0;
        private async Task<IActionResult> ProccessExcel(IFormFile file)
        {
            var taxCategories = _taxCategoryService.GetAllTaxCategories().ToList();
            var tags = _productTagService.GetAllProductTags().ToList();
            var categories = _categoryService.GetAllCategories();
            var manufacturers = _manufacturerService.GetAllManufacturers();
            var careAttributes = _specificationAttributeService.GetSpecificationAttributes()
                .Where(x => x.Id == 3) //tipo de cuidado
                .Select(x => x.SpecificationAttributeOptions)
                .SelectMany(x => x)
                .ToList();
            var hardnessAttributes = _specificationAttributeService.GetSpecificationAttributes()
                .Where(x => x.Id == 2) //dureza
                .Select(x => x.SpecificationAttributeOptions)
                .SelectMany(x => x)
                .ToList();
            var labelAttributes = _specificationAttributeService.GetSpecificationAttributes()
                .Where(x => x.Id == 4) //etiquetado manual
                .Select(x => x.SpecificationAttributeOptions)
                .SelectMany(x => x)
                .ToList();
            var products = _productService.GetAllProductsQuery().Where(x => !x.Deleted).ToList();

            List<ProductSatCode> productSatCodesList = new List<ProductSatCode>();

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream).ConfigureAwait(false);
                using (var package = new ExcelPackage(memoryStream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet != null)
                    {
                        int init = 0;
                        List<string> headers = new List<string>();
                        int totalRows = worksheet.Dimension.End.Row;
                        int totalColumns = worksheet.Dimension.End.Column;
                        var range = worksheet.Cells[1, 1, 1, totalColumns];

                        GetHeaders(ref init, ref headers, totalColumns, range, worksheet);
                        if (!headers.Contains("Id") ||
                            !headers.Contains("Sku") ||
                            !headers.Contains("Nombre") ||
                            !headers.Contains("Categorías padre") ||
                            !headers.Contains("Categorías hijo") ||
                            !headers.Contains("Tags") ||
                            !headers.Contains("Manojo/Nevera") ||
                            !headers.Contains("Dureza") ||
                            !headers.Contains("Costo") ||
                            !headers.Contains("Impuesto") ||
                            !headers.Contains("Fabricantes") ||
                            !headers.Contains("Precio") ||
                            !headers.Contains("Precio anterior") ||
                            !headers.Contains("Publicado") ||
                            !headers.Contains("Intervalos de peso (gramos)") ||
                            !headers.Contains("Coeficiente de equivalencia") ||
                            !headers.Contains("Propiedades de producto") ||
                            !headers.Contains("Etiquetado manual") ||
                            !headers.Contains("Código del SAT") ||
                            !headers.Contains("Permitir actualización automática de precio")
                            )
                        {
                            return BadRequest("El archivo no tiene las columnas correctas.");
                        }

                        try
                        {
                            var cells = worksheet.Cells.ToList();
                            var groups = GetCellGroups(cells, worksheet.Dimension.End.Row - 1);
                            if (groups == null) return BadRequest("Ocurrió un problema creando los grupos para la carga de datos");
                            var dataList = new List<CellObjectModel>();
                            for (int g = 0; g < groups.Count; g++)
                            {
                                currentRow = g;
                                int currentColumn = 0;
                                var data = new CellObjectModel();
                                data.Id = groups[g][currentColumn].Value != null && !string.IsNullOrWhiteSpace(groups[g][currentColumn].Value.ToString()) ? (int?)int.Parse(groups[g][currentColumn].Value.ToString()) : null;
                                currentColumn++;
                                data.Sku = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.Name = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.ParentCategories = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.ChildCategories = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.Tags = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.CareType = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.Hardness = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.Cost = groups[g][currentColumn].Value != null && !string.IsNullOrWhiteSpace(groups[g][currentColumn].Value.ToString()) ? (decimal?)decimal.Parse(groups[g][currentColumn].Value.ToString()) : null;
                                currentColumn++;
                                data.Tax = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.Manufacturer = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.Price = groups[g][currentColumn].Value != null && !string.IsNullOrWhiteSpace(groups[g][currentColumn].Value.ToString()) ? (decimal?)decimal.Parse(groups[g][currentColumn].Value.ToString()) : null;
                                currentColumn++;
                                data.OldPrice = groups[g][currentColumn].Value != null && !string.IsNullOrWhiteSpace(groups[g][currentColumn].Value.ToString()) ? (decimal?)decimal.Parse(groups[g][currentColumn].Value.ToString()) : null;
                                currentColumn++;
                                data.Published = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.WeightInterval = groups[g][currentColumn].Value != null && !string.IsNullOrWhiteSpace(groups[g][currentColumn].Value.ToString()) ? (decimal?)decimal.Parse(groups[g][currentColumn].Value.ToString()) : null;
                                currentColumn++;
                                data.EquivalenceOfCoefficient = groups[g][currentColumn].Value != null && !string.IsNullOrWhiteSpace(groups[g][currentColumn].Value.ToString()) ? (decimal?)decimal.Parse(groups[g][currentColumn].Value.ToString()) : null;
                                currentColumn++;
                                data.ProductProperties = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.Labels = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.SATCode = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.AllowAutoUpdatePrice = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                dataList.Add(data);
                            }

                            // Verify data
                            var errorMessages = new List<string>();
                            for (int i = 0; i < dataList.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(dataList[i].ParentCategories))
                                {
                                    var array = dataList[i].ParentCategories.Split(';');
                                    foreach (var item in array)
                                    {
                                        if (categories.Where(x => x.Name.Trim().ToLower() == item.Trim().ToLower()).FirstOrDefault() == null)
                                            errorMessages.Add($"La categoría {item} no existe. Fila: {i + 2}");
                                    }
                                }

                                if (!string.IsNullOrEmpty(dataList[i].ChildCategories))
                                {
                                    var array = dataList[i].ChildCategories.Split(';');
                                    foreach (var item in array)
                                    {
                                        if (categories.Where(x => x.Name.Trim().ToLower() == item.Trim().ToLower()).FirstOrDefault() == null)
                                            errorMessages.Add($"La categoría {item} no existe. Fila: {i + 2}");
                                    }
                                }

                                //if (!string.IsNullOrEmpty(dataList[i].Tags))
                                //{
                                //    var array = dataList[i].Tags.Split(';');
                                //    foreach (var item in array)
                                //    {
                                //        if (tags.Where(x => x.Name.Trim().ToLower() == item.Trim().ToLower()).FirstOrDefault() == null)
                                //            errorMessages.Add($"La tag {item} no existe. Fila: {i + 2}");
                                //    }
                                //}

                                if (!string.IsNullOrEmpty(dataList[i].Manufacturer))
                                {
                                    var array = dataList[i].Manufacturer.Split(';');
                                    foreach (var item in array)
                                    {
                                        if (manufacturers.Where(x => x.Name.Trim().ToLower() == item.Trim().ToLower()).FirstOrDefault() == null)
                                            errorMessages.Add($"El fabricante {item} no existe. Fila: {i + 2}");
                                    }
                                }

                                if (!string.IsNullOrEmpty(dataList[i].CareType))
                                {
                                    var array = dataList[i].CareType.Split(';');
                                    foreach (var item in array)
                                    {
                                        if (careAttributes.Where(x => x.Name.Trim().ToLower() == item.Trim().ToLower()).FirstOrDefault() == null)
                                            errorMessages.Add($"El atributo de tipo de cuidado {item} no existe. Fila: {i + 2}");
                                    }
                                }

                                if (!string.IsNullOrEmpty(dataList[i].Hardness))
                                {
                                    var array = dataList[i].Hardness.Split(';');
                                    foreach (var item in array)
                                    {
                                        if (hardnessAttributes.Where(x => x.Name.Trim().ToLower() == item.Trim().ToLower()).FirstOrDefault() == null)
                                            errorMessages.Add($"El atributo de dureza {item} no existe. Fila: {i + 2}");
                                    }
                                }

                                if (!string.IsNullOrEmpty(dataList[i].Labels))
                                {
                                    var array = dataList[i].Labels.Split(';');
                                    foreach (var item in array)
                                    {
                                        if (labelAttributes.Where(x => x.Name.Trim().ToLower() == item.Trim().ToLower()).FirstOrDefault() == null)
                                            errorMessages.Add($"El atributo de etiqueta {item} no existe. Fila: {i + 2}");
                                    }
                                }

                                if (dataList[i].Id.HasValue && products.Where(x => x.Id == dataList[i].Id.Value).FirstOrDefault() == null)
                                    errorMessages.Add($"El Id de producto {dataList[i].Id.Value} no existe. Fila: {i + 2}");

                                if (!dataList[i].Id.HasValue && string.IsNullOrEmpty(dataList[i].Name))
                                    errorMessages.Add($"Hay un nuevo producto sin nombre. Fila: {i + 2}");

                                //if (dataList[i].Cost.HasValue && dataList[i].Cost <= 0)
                                //    errorMessages.Add($"Hay un producto con costo menor o igual a 0. Fila: {i + 2}");

                                if (!string.IsNullOrEmpty(dataList[i].Tax) && !taxCategories
                                    .Where(x => x.Name.Trim().ToLower() == dataList[i].Tax.Trim().ToLower()).Any())
                                    errorMessages.Add($"Hay un producto con impuesto inválido. Fila: {i + 2}");

                                if (dataList[i].Price.HasValue && dataList[i].Price <= 0)
                                    errorMessages.Add($"Hay un producto con precio menor o igual a 0. Fila: {i + 2}");

                                if (!dataList[i].Id.HasValue && products.Where(x => x.Name == dataList[i].Name).Any())
                                    errorMessages.Add($"El producto {dataList[i].Name} ya esxiste pero se está cargando como un producto nuevo. Fila: {i + 2}");
                            }

                            if (errorMessages.Any())
                                return BadRequest(string.Join("\n", errorMessages));

                            // Start uploading data
                            int count = 0;
                            foreach (var data in dataList)
                            {
                                count++;
                                decimal previousPrice = -1;
                                decimal previousCost = -1;
                                if (data.Id.HasValue && data.Id.Value > 0)
                                {
                                    string log = $"{DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt")} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) actualizó el producto utilizando un excel.";
                                    var product = products.Where(x => x.Id == data.Id.Value).FirstOrDefault();

                                    if ((!string.IsNullOrEmpty(data.Sku) && product.Sku != data.Sku) || (string.IsNullOrEmpty(data.Sku) && string.IsNullOrEmpty(product.Sku)))
                                    {
                                        string newSku = string.IsNullOrEmpty(data.Sku) && string.IsNullOrEmpty(product.Sku) ? product.Id.ToString() : data.Sku;
                                        log += $" Actualizó el Sku de {product.Sku} a {newSku}";
                                        product.Sku = newSku;
                                    }

                                    if (!string.IsNullOrEmpty(data.Name) && product.Name != data.Name)
                                    {
                                        log += $" Actualizó el nombre del producto de {product.Name} a {data.Name}.";
                                        product.Name = data.Name;
                                    }

                                    if (!string.IsNullOrEmpty(data.ParentCategories))
                                    {
                                        var array = data.ParentCategories.Split(';').Select(x => x.Trim().ToLower());
                                        var newCategories = categories.Where(x => array.Contains(x.Name.Trim().ToLower())).ToList();
                                        var currentCategories = product.ProductCategories.Where(x => x.Category.ParentCategoryId == 0 && !x.Category.Deleted).ToList();

                                        if (newCategories.Count > 0 && (newCategories.Count != currentCategories.Count ||
                                            newCategories.Select(x => x.Id).ToList().Except(currentCategories.Select(y => y.CategoryId).ToList()).Any()))
                                        {
                                            log += $" Actualizó las categorías padre.";

                                            foreach (var existingCategory in currentCategories)
                                                _categoryService.DeleteProductCategory(existingCategory);

                                            foreach (var newCategory in newCategories)
                                            {
                                                _categoryService.InsertProductCategory(new ProductCategory
                                                {
                                                    ProductId = product.Id,
                                                    CategoryId = newCategory.Id,
                                                    DisplayOrder = 0
                                                });
                                            }
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(data.ChildCategories))
                                    {
                                        var array = data.ChildCategories.Split(';').Select(x => x.Trim().ToLower());
                                        var newCategories = categories.Where(x => array.Contains(x.Name.Trim().ToLower())).ToList();
                                        var currentCategories = product.ProductCategories.Where(x => x.Category.ParentCategoryId != 0 && !x.Category.Deleted).ToList();

                                        if (newCategories.Count > 0 && (newCategories.Count != currentCategories.Count ||
                                            newCategories.Select(x => x.Id).ToList().Except(currentCategories.Select(y => y.CategoryId).ToList()).Any()))
                                        {
                                            log += $" Actualizó las categorías hijo.";

                                            foreach (var existingCategory in currentCategories)
                                                _categoryService.DeleteProductCategory(existingCategory);

                                            foreach (var newCategory in newCategories)
                                            {
                                                _categoryService.InsertProductCategory(new ProductCategory
                                                {
                                                    ProductId = product.Id,
                                                    CategoryId = newCategory.Id,
                                                    DisplayOrder = 0
                                                });
                                            }
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(data.Tags))
                                    {
                                        var array = data.Tags.Split(';').Select(x => x.Trim().ToLower());
                                        foreach (var tag in array)
                                        {
                                            if (!tags.Where(x => x.Name.Trim().ToLower() == tag).Any())
                                                _productTagService.InsertProductTag(new ProductTag
                                                {
                                                    Name = tag
                                                });
                                        }
                                        tags = _productTagService.GetAllProductTags().ToList();
                                        var newTags = tags.Where(x => array.Contains(x.Name.Trim().ToLower())).ToList();
                                        if (newTags.Count > 0)
                                        {
                                            log += $" Actualizó las tags.";

                                            _productTagService.UpdateProductTags(product, ParseProductTags(string.Join(",", newTags.Select(x => x.Name))));
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(data.CareType))
                                    {
                                        var array = data.CareType.Split(';').Select(x => x.Trim().ToLower());
                                        var newCareTypes = careAttributes.Where(x => array.Contains(x.Name.Trim().ToLower())).ToList();
                                        var currentCareTypes = product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.SpecificationAttribute.Id == 3).ToList();

                                        if (newCareTypes.Count > 0 && (newCareTypes.Count != currentCareTypes.Count ||
                                            newCareTypes.Select(x => x.Id).ToList().Except(currentCareTypes.Select(y => y.SpecificationAttributeOptionId).ToList()).Any()))
                                        {
                                            log += $" Actualizó los atributos tipos de cuidado.";

                                            foreach (var existingSA in currentCareTypes)
                                                _specificationAttributeService.DeleteProductSpecificationAttribute(existingSA);

                                            foreach (var careType in newCareTypes)
                                            {
                                                var psa = new ProductSpecificationAttribute
                                                {
                                                    AttributeTypeId = (int)SpecificationAttributeType.Option,
                                                    SpecificationAttributeOptionId = careType.Id,
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

                                    if (!string.IsNullOrEmpty(data.Hardness))
                                    {
                                        var array = data.Hardness.Split(';').Select(x => x.Trim().ToLower());
                                        var newHardness = hardnessAttributes.Where(x => array.Contains(x.Name.Trim().ToLower())).ToList();
                                        var currentHardness = product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.SpecificationAttribute.Id == 2).ToList();
                                        if (newHardness.Count > 0 && (newHardness.Count != currentHardness.Count ||
                                            newHardness.Select(x => x.Id).ToList().Except(currentHardness.Select(y => y.SpecificationAttributeOptionId).ToList()).Any()))
                                        {
                                            log += $" Actualizó los atributos de dureza.";

                                            foreach (var existingSA in currentHardness)
                                                _specificationAttributeService.DeleteProductSpecificationAttribute(existingSA);

                                            foreach (var hardness in newHardness)
                                            {
                                                var psa = new ProductSpecificationAttribute
                                                {
                                                    AttributeTypeId = (int)SpecificationAttributeType.Option,
                                                    SpecificationAttributeOptionId = hardness.Id,
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

                                    if (data.Cost.HasValue && product.ProductCost != data.Cost && data.Cost > 0)
                                    {
                                        log += $" Actualizó el costo del producto de {product.ProductCost} a {data.Cost.Value}.";
                                        product.ProductCost = data.Cost.Value;
                                        previousCost = product.ProductCost;
                                    }

                                    if (string.IsNullOrEmpty(data.Tax))
                                        product.TaxCategoryId = 0;
                                    else
                                        product.TaxCategoryId = taxCategories
                                            .Where(x => x.Name.Trim().ToLower() == data.Tax.Trim().ToLower())
                                            .FirstOrDefault().Id;

                                    if (!string.IsNullOrEmpty(data.Manufacturer))
                                    {
                                        var array = data.Manufacturer.Split(';').Select(x => x.Trim().ToLower());
                                        var newManufacturers = manufacturers.Where(x => array.Contains(x.Name.Trim().ToLower())).ToList();
                                        var currentManufacturers = product.ProductManufacturers.ToList();
                                        if (newManufacturers.Count > 0 && (newManufacturers.Count != currentManufacturers.Count ||
                                            newManufacturers.Select(x => x.Id).ToList().Except(currentManufacturers.Select(y => y.ManufacturerId).ToList()).Any()))
                                        {
                                            log += $" Actualizó los fabricantes.";

                                            foreach (var manufacturer in currentManufacturers)
                                                _manufacturerService.DeleteProductManufacturer(manufacturer);

                                            foreach (var newManufacturer in newManufacturers)
                                            {
                                                _manufacturerService.InsertProductManufacturer(new ProductManufacturer()
                                                {
                                                    ManufacturerId = newManufacturer.Id,
                                                    ProductId = product.Id
                                                });
                                            }

                                        }
                                    }

                                    if (data.Price.HasValue && product.Price != data.Price && data.Price > 0)
                                    {
                                        log += $" Actualizó el precio del producto de {product.Price} a {data.Price.Value}.";
                                        previousPrice = product.Price;
                                        product.Price = data.Price.Value;
                                    }

                                    if (data.OldPrice.HasValue && product.OldPrice != data.OldPrice)
                                    {
                                        log += $" Actualizó el precio anterior del producto de {product.OldPrice} a {data.OldPrice.Value}.";
                                        product.OldPrice = data.OldPrice.Value;
                                    }

                                    if (!string.IsNullOrEmpty(data.Published))
                                    {
                                        bool boolValue = data.Published.ToLower().Trim() == "true";
                                        if (product.Published != boolValue)
                                        {
                                            log += $" Actualizó la publicación del producto de {product.Published.ToString()} a {boolValue.ToString()}.";
                                            product.Published = boolValue;
                                        }
                                    }

                                    if (data.WeightInterval.HasValue && data.WeightInterval.Value != product.WeightInterval)
                                    {
                                        log += $" Actualizó el intérvalo de peso de {product.WeightInterval} a {data.WeightInterval.Value}.";
                                        product.WeightInterval = data.WeightInterval.Value;
                                    }

                                    if (data.EquivalenceOfCoefficient.HasValue && data.EquivalenceOfCoefficient.Value != product.EquivalenceCoefficient)
                                    {
                                        log += $" Actualizó el coeficiente de equivalencia de {product.EquivalenceCoefficient} a {data.EquivalenceOfCoefficient.Value}.";
                                        product.EquivalenceCoefficient = data.EquivalenceOfCoefficient.Value;
                                    }

                                    if (!string.IsNullOrEmpty(data.ProductProperties) && data.ProductProperties != product.PropertiesOptions)
                                    {
                                        log += $" Actualizó las propiedades del producto de {product.PropertiesOptions} a {data.ProductProperties}.";
                                        product.PropertiesOptions = data.ProductProperties;
                                    }

                                    // Label
                                    var newLabels = new List<SpecificationAttributeOption>();
                                    if (!string.IsNullOrEmpty(data.Labels))
                                    {
                                        var array2 = data.Labels.Split(';').Select(x => x.Trim().ToLower());
                                        newLabels = labelAttributes.Where(x => array2.Contains(x.Name.Trim().ToLower())).ToList();
                                    }
                                    var currentSA = product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.SpecificationAttribute.Id == 4).ToList();

                                    if (newLabels.Count > 0 && (newLabels.Count != currentSA.Count ||
                                            newLabels.Select(x => x.Id).ToList().Except(currentSA.Select(y => y.SpecificationAttributeOptionId).ToList()).Any()))
                                    {
                                        log += $" Actualizó los atributos de etiquetas.";

                                        foreach (var existingSA in currentSA)
                                            _specificationAttributeService.DeleteProductSpecificationAttribute(existingSA);

                                        foreach (var label in newLabels)
                                        {
                                            var psa = new ProductSpecificationAttribute
                                            {
                                                AttributeTypeId = (int)SpecificationAttributeType.Option,
                                                SpecificationAttributeOptionId = label.Id,
                                                ProductId = product.Id,
                                                CustomValue = null,
                                                AllowFiltering = false,
                                                ShowOnProductPage = false,
                                                DisplayOrder = 0,
                                            };
                                            _specificationAttributeService.InsertProductSpecificationAttribute(psa);
                                        }
                                    }
                                    else if (newLabels.Count == 0)
                                    {
                                        log += $" Eliminó los atributos de etiquetas.";

                                        currentSA = product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.SpecificationAttribute.Id == 4).ToList();
                                        foreach (var existingSA in currentSA)
                                            _specificationAttributeService.DeleteProductSpecificationAttribute(existingSA);
                                    }

                                    if (!string.IsNullOrEmpty(data.SATCode))
                                    {
                                        ProductSatCode productSatCode = new ProductSatCode()
                                        {
                                            ProductId = product.Id,
                                            ProductCatalogId = data.SATCode
                                        };
                                        productSatCodesList.Add(productSatCode);
                                    }

                                    if (!string.IsNullOrEmpty(data.AllowAutoUpdatePrice))
                                    {
                                        bool boolValue = data.AllowAutoUpdatePrice.ToLower().Trim() == "true";
                                        if (product.AllowAutoUpdatePrice != boolValue)
                                        {
                                            log += $"Actualizó permitir actualización automática de precio de {product.AllowAutoUpdatePrice.ToString()} a {boolValue.ToString()}.";
                                            product.AllowAutoUpdatePrice = boolValue;
                                        }
                                    }

                                    _productService.UpdateProduct(product);
                                    if (previousPrice > -1)
                                    {
                                        decimal? newCost = null;
                                        decimal? oldCost = null;
                                        if (previousCost > -1)
                                        {
                                            newCost = product.ProductCost;
                                            oldCost = previousCost;
                                        }
                                        _priceLogService.InsertPriceLog(new PriceLog()
                                        {
                                            CreatedOnUtc = DateTime.UtcNow,
                                            NewPrice = product.Price,
                                            OldPrice = previousPrice,
                                            NewCost = newCost,
                                            OldCost = oldCost,
                                            ProductId = product.Id,
                                            SKU = product.Sku,
                                            User = _workContext.CurrentCustomer.GetFullName()
                                        });
                                    }

                                    _productLogService.InsertProductLog(new ProductLog()
                                    {
                                        ProductId = product.Id,
                                        Message = log,
                                        CreatedOnUtc = DateTime.UtcNow,
                                        UserId = _workContext.CurrentCustomer.Id
                                    });
                                }
                                else
                                {
                                    Product product = new Product()
                                    {
                                        Sku = data.Sku,
                                        Name = data.Name,
                                        ProductCost = data.Cost ?? 99999,
                                        Price = data.Price ?? 99999,
                                        Published = data.Published.Trim().ToLower() == "true",
                                        WeightInterval = data.WeightInterval ?? 0,
                                        EquivalenceCoefficient = data.EquivalenceOfCoefficient ?? 0,
                                        PropertiesOptions = data.ProductProperties,
                                        CreatedOnUtc = DateTime.UtcNow,
                                        UpdatedOnUtc = DateTime.UtcNow,
                                        VisibleIndividually = true,
                                        OrderMaximumQuantity = 9999,
                                        ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                                        StockQuantity = 99999,
                                        IsShipEnabled = true,
                                        AllowAutoUpdatePrice = true,
                                        ProductType = ProductType.SimpleProduct
                                    };
                                    _productService.InsertProduct(product);

                                    string seName = product.ValidateSeName(null, product.Name, true);
                                    _recordService.SaveSlug(product, seName, 0);

                                    if (string.IsNullOrWhiteSpace(product.Sku))
                                    {
                                        product.Sku = product.Id.ToString();
                                    }

                                    if (!string.IsNullOrEmpty(data.ParentCategories))
                                    {
                                        var array = data.ParentCategories.Split(';').Select(x => x.Trim().ToLower());
                                        var newCategories = categories.Where(x => array.Contains(x.Name.Trim().ToLower())).ToList();
                                        if (newCategories.Count > 0)
                                        {
                                            foreach (var newCategory in newCategories)
                                            {
                                                _categoryService.InsertProductCategory(new ProductCategory
                                                {
                                                    ProductId = product.Id,
                                                    CategoryId = newCategory.Id,
                                                    DisplayOrder = 0
                                                });
                                            }
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(data.ChildCategories))
                                    {
                                        var array = data.ChildCategories.Split(';').Select(x => x.Trim().ToLower());
                                        var newCategories = categories.Where(x => array.Contains(x.Name.Trim().ToLower())).ToList();
                                        if (newCategories.Count > 0)
                                        {
                                            foreach (var newCategory in newCategories)
                                            {
                                                _categoryService.InsertProductCategory(new ProductCategory
                                                {
                                                    ProductId = product.Id,
                                                    CategoryId = newCategory.Id,
                                                    DisplayOrder = 0
                                                });
                                            }
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(data.CareType))
                                    {
                                        var array = data.CareType.Split(';').Select(x => x.Trim().ToLower());
                                        var newCareTypes = careAttributes.Where(x => array.Contains(x.Name.Trim().ToLower())).ToList();
                                        if (newCareTypes.Count > 0)
                                        {
                                            foreach (var careType in newCareTypes)
                                            {
                                                var psa = new ProductSpecificationAttribute
                                                {
                                                    AttributeTypeId = (int)SpecificationAttributeType.Option,
                                                    SpecificationAttributeOptionId = careType.Id,
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

                                    if (!string.IsNullOrEmpty(data.Hardness))
                                    {
                                        var array = data.Hardness.Split(';').Select(x => x.Trim().ToLower());
                                        var newHardness = hardnessAttributes.Where(x => array.Contains(x.Name.Trim().ToLower())).ToList();
                                        if (newHardness.Count > 0)
                                        {
                                            foreach (var hardness in newHardness)
                                            {
                                                var psa = new ProductSpecificationAttribute
                                                {
                                                    AttributeTypeId = (int)SpecificationAttributeType.Option,
                                                    SpecificationAttributeOptionId = hardness.Id,
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

                                    if (!string.IsNullOrEmpty(data.Manufacturer))
                                    {
                                        var array = data.Manufacturer.Split(';').Select(x => x.Trim().ToLower());
                                        var newManufacturers = manufacturers.Where(x => array.Contains(x.Name.Trim().ToLower())).ToList();
                                        if (newManufacturers.Count > 0)
                                        {
                                            foreach (var newManufacturer in newManufacturers)
                                            {
                                                _manufacturerService.InsertProductManufacturer(new ProductManufacturer()
                                                {
                                                    ManufacturerId = newManufacturer.Id,
                                                    ProductId = product.Id
                                                });
                                            }
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(data.SATCode))
                                    {
                                        ProductSatCode productSatCode = new ProductSatCode()
                                        {
                                            ProductId = product.Id,
                                            ProductCatalogId = data.SATCode
                                        };
                                        productSatCodesList.Add(productSatCode);
                                    }
                                }
                            }

                            string jsonSatCodes = JsonConvert.SerializeObject(productSatCodesList).ToString();

                            using (HttpClient client = new HttpClient())
                            {
                                string url = (Request.IsHttps ? "https://" : "http://") + $"{Request.Host}/Admin/Facturify/UpdateManyProductSatCode?productSATCodesList=" + jsonSatCodes;
                                var result = client.PostAsync(url, null).Result;
                                if (!result.IsSuccessStatusCode)
                                {
                                    string json = result.Content.ReadAsStringAsync().Result;
                                    Debugger.Break();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            return BadRequest("Ocurrió un problema cargando los datos: " + e.Message);
                        }
                    }
                    else
                    {
                        return BadRequest("No fue posible cargar el excel");
                    }
                }
                return Ok("Los productos se actualizaron correctamente.");
            }
        }

        protected virtual string[] ParseProductTags(string productTags)
        {
            var result = new List<string>();
            if (!string.IsNullOrWhiteSpace(productTags))
            {
                var values = productTags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var val1 in values)
                    if (!string.IsNullOrEmpty(val1.Trim()))
                        result.Add(val1.Trim());
            }
            return result.ToArray();
        }

        private void GetHeaders(ref int init, ref List<string> headerList, int totalColumns, ExcelRange range, ExcelWorksheet sheet)
        {
            string[] initCol;
            for (int i = 1; i <= totalColumns; ++i)
            {
                if (!string.IsNullOrEmpty(sheet.Cells[1, i].Text))
                {
                    if (headerList.Count() == 0)
                    {
                        initCol = System.Text.RegularExpressions.Regex.Split(range[1, i].Address, @"\D+");
                        init = int.Parse(initCol[1]);
                    }
                    headerList.Add(sheet.Cells[1, i].Text);
                }
            }
        }

        private List<List<CellDataModel>> GetCellGroups(List<ExcelRangeBase> elements, int finalRow)
        {
            int i = 0;
            int g = 0;
            try
            {
                var list = new List<List<CellDataModel>>();
                var headerLetters = elements.Where(x => x.Start.Row == 1).Select(x => x.Address).Select(x => new String(x.Where(y => Char.IsLetter(y)).ToArray())).ToList();
                for (i = 0; i < finalRow; i++)
                {
                    var listData = new List<CellDataModel>();
                    for (g = 0; g < headerLetters.Count; g++)
                    {
                        var address = headerLetters[g] + (i + 2).ToString();
                        var element = elements.Where(x => x.Address == address).FirstOrDefault();
                        if (element == null || element.Value == null)
                        {
                            listData.Add(new CellDataModel() { Address = address, Value = null });
                        }
                        else
                        {
                            listData.Add(new CellDataModel() { Address = address, Value = element.Value.ToString() });
                        }
                    }
                    list.Add(listData);
                }

                return list;
            }
            catch (Exception w)
            {
                return null;
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ChangeProductPriceLower10Percent(bool createXls = false)
        {
            var products = _productService.GetAllProductsQuery()
                .Where(x => !x.Deleted).OrderBy(x => x.Name).ToList();
            var orderReportItems = GetOrderItemReport();

            if (createXls)
            {
                using (var stream = new MemoryStream())
                {
                    using (var xlPackage = new ExcelPackage(stream))
                    {
                        var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                        int row = 1;
                        worksheet.Cells[row, 1].Value = "Producto";
                        worksheet.Cells[row, 2].Value = "Precio actual";
                        worksheet.Cells[row, 3].Value = "Costo";
                        worksheet.Cells[row, 4].Value = "Nuevo Precio";
                        row++;

                        foreach (var product in products)
                        {
                            List<OrderItemReportModel> productReports =
                                GetOrderReportModels(orderReportItems, product.Id);

                            if (productReports.Any())
                            {
                                decimal productReportCost = productReports
                                    .Select(x => x.OrderReport)
                                    .OrderByDescending(x => x.UpdatedOnUtc)
                                    .FirstOrDefault().UpdatedUnitCost;
                                var porcentage = GetProductPorcentageCost(productReportCost, product.Price);
                                if (porcentage <= (decimal)0.1)
                                {
                                    var oldPrice = product.Price;
                                    var newPrice = productReportCost / (decimal)0.88;
                                    if (newPrice > oldPrice)
                                    {
                                        worksheet.Cells[row, 1].Value = product.Name;
                                        worksheet.Cells[row, 2].Value = oldPrice;
                                        worksheet.Cells[row, 3].Value = productReportCost;
                                        worksheet.Cells[row, 4].Value = newPrice;
                                        row++;
                                    }
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

                    return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_productos_costo_10_porciento.xlsx");
                }
            }
            else
            {

                foreach (var product in products)
                {
                    List<OrderItemReportModel> productReports =
                        GetOrderReportModels(orderReportItems, product.Id);
                    if (productReports.Any())
                    {
                        decimal productReportCost = productReports
                            .Select(x => x.OrderReport)
                            .OrderByDescending(x => x.UpdatedOnUtc)
                            .FirstOrDefault().UpdatedUnitCost;
                        var porcentage = GetProductPorcentageCost(productReportCost, product.Price);
                        if (porcentage <= (decimal)0.1)
                        {
                            var oldPrice = product.Price;
                            var newPrice = productReportCost / (decimal)0.88;
                            if (newPrice > oldPrice)
                            {
                                //product.Price = newPrice;
                                //_productService.UpdateProduct(product);
                                //_priceLogService.InsertPriceLog(new PriceLog()
                                //{
                                //    CreatedOnUtc = DateTime.UtcNow,
                                //    OldPrice = oldPrice,
                                //    NewPrice = product.Price,
                                //    ProductId = product.Id,
                                //    Product = product.Name,
                                //    SKU = product.Sku,
                                //    User = "Actualización forzada"
                                //});
                            }
                        }
                    }
                }
            }
            return Ok();
        }

        public decimal GetProductPorcentageCost(decimal productReportCost, decimal productPrice)
        {
            var moneyUtility = productPrice - productReportCost;
            return Math.Round(moneyUtility / productPrice, 4);
        }

        public List<OrderItemReportModel> GetOrderReportModels(List<OrderItemReportModel> orderReportItems, int productId)
        {
            return orderReportItems.Where(x => x.OrderItem.ProductId == productId
                                && x.OrderReport.UpdatedUnitCost > 0
                                ).OrderByDescending(x => x.OrderReport.CreatedOnUtc)
                                .ToList();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult UpdateProductPrices(string sku = null)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UpdatePrices))
                return AccessDeniedView();

            List<ProductUpdateModel> productsToUpdate = new List<ProductUpdateModel>();

            var products = _productService.GetAllProductsQuery().Where(x => x.Published && !x.Deleted);
            if (!string.IsNullOrWhiteSpace(sku))
            {
                products = products.Where(x => x.Sku == sku);
            }

            var orderReportItems = GetOrderItemReport();
            var manufacturers = _manufacturerService.GetAllManufacturers();

            DateTime controlDate = DateTime.Now.AddDays(-15);
            foreach (var product in products.ToList())
            {
                List<OrderItemReportModel> productReports = orderReportItems
                    .Where(x => x.OrderItem.ProductId == product.Id)
                    .OrderByDescending(x => x.OrderReport.CreatedOnUtc)
                    .ToList();

                OrderItemReportModel lastApprovedProductReport = productReports.FirstOrDefault();

                List<decimal> orderReportCostList = productReports
                    .Select(x => x.OrderReport.UpdatedUnitCost)
                    .Take(3)
                    .ToList();

                decimal orderReportCost = GetMedian(orderReportCostList);

                //List<decimal> prices = await GetWebScrapingHistory(controlDate, product.WalmartProductId, product.LaComerProductId, product.ChedrauiProductId, product.SuperamaProductId);
                //decimal wsCost = GetMedian(prices);

                if (orderReportCost > 0)
                {
                    decimal cost = orderReportCost;
                    decimal mupc = product.ProductCategories
                        .Where(x => x.Category.ParentCategoryId > 0)
                        .Select(x => x.Category.PercentageOfUtility)
                        .OrderByDescending(x => x)
                        .FirstOrDefault();

                    if (mupc > 0)
                    {
                        decimal oldPrice = product.Price;
                        decimal oldCost = product.ProductCost;
                        decimal mupcResult = (1 - (mupc / 100));
                        if (mupcResult == 0) continue;
                        decimal newPrice = cost / mupcResult;

                        if (newPrice <= 0) continue;
                        bool shouldUpdateManufacturer = lastApprovedProductReport.OrderReport.ManufacturerId.HasValue &&
                            !product.ProductManufacturers.Where(x => !x.Manufacturer.Deleted).Select(x => x.ManufacturerId)
                            .ToList().Contains(lastApprovedProductReport.OrderReport.ManufacturerId.Value);

                        if ((newPrice > 0 && Math.Round(oldPrice, 2) != Math.Round(newPrice, 2)) ||
                            (Math.Round(product.ProductCost, 2) != Math.Round(cost, 2)) ||
                            shouldUpdateManufacturer)
                        {
                            product.Price = product.AllowAutoUpdatePrice && newPrice > 0 ? newPrice : oldPrice;
                            product.ProductCost = product.AllowAutoUpdatePrice ? cost : product.ProductCost;
                            productsToUpdate.Add(new ProductUpdateModel()
                            {
                                Product = product,
                                OldPrice = oldPrice,
                                OldCost = oldCost,
                                NewManufacturerId = lastApprovedProductReport.OrderReport.ManufacturerId ?? 0,
                                ManufacturerName = lastApprovedProductReport.OrderReport.ShoppingStoreId
                            });
                        }
                    }
                }
            }

            foreach (var product in productsToUpdate)
            {
                bool manufacturerUpdated = false;
                if (!string.IsNullOrWhiteSpace(product.ManufacturerName) && product.NewManufacturerId == -1)
                {
                    var manufacturer = manufacturers.Where(x => x.Name.ToLower().Trim() == product.ManufacturerName.ToLower().Trim()).FirstOrDefault();
                    if (manufacturer == null)
                    {
                        manufacturer = new Manufacturer()
                        {
                            CreatedOnUtc = DateTime.UtcNow,
                            UpdatedOnUtc = DateTime.UtcNow,
                            DisplayTitle = true,
                            DisplayDescription = true,
                            DisplayPicture = true,
                            DisplayOrder = 0,
                            Published = true,
                            PageSize = 6,
                            Name = product.ManufacturerName,
                            ManufacturerTemplateId = 1,
                            PageSizeOptions = "6, 3, 9",
                            LimitedToStores = false,
                            Description = "Creado desde la actualización automática"
                        };
                        manufacturers.Add(manufacturer);
                        _manufacturerService.InsertManufacturer(manufacturer);
                    }

                    product.NewManufacturerId = manufacturer.Id;
                }

                if (product.NewManufacturerId > 0)
                {
                    var productHasManufacturer = product.Product.ProductManufacturers.Where(x => x.ManufacturerId == product.NewManufacturerId).Any();
                    if (!productHasManufacturer)
                    {
                        product.Product.ProductManufacturers.Add(new ProductManufacturer()
                        {
                            ManufacturerId = product.NewManufacturerId,
                            ProductId = product.Product.Id
                        });

                        _productLogService.InsertProductLog(new ProductLog()
                        {
                            CreatedOnUtc = DateTime.UtcNow,
                            Message = $"{DateTime.Now:dd-MM-yyyy hh:mm tt} - Se agregó el fabricante con ID {product.NewManufacturerId} al producto desde la actualización automática",
                            ProductId = product.Product.Id,
                            UserId = 0
                        });
                        manufacturerUpdated = true;
                    }
                }

                bool shouldUpdatePrice = Math.Round(product.OldPrice, 2) != Math.Round(product.Product.Price, 2) ||
                    (Math.Round(product.Product.ProductCost, 2) != Math.Round(product.OldCost, 2));

                if (manufacturerUpdated || shouldUpdatePrice)
                {

                    _productService.UpdateProduct(product.Product);
                    if (!shouldUpdatePrice) continue;
                    _priceLogService.InsertPriceLog(new PriceLog()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        OldPrice = product.OldPrice,
                        NewPrice = product.Product.Price,
                        ProductId = product.Product.Id,
                        OldCost = product.OldCost,
                        NewCost = product.Product.ProductCost,
                        Product = product.Product.Name,
                        SKU = product.Product.Sku,
                        User = "Actualización automática"
                    });
                }
            }

            return Ok(productsToUpdate.Count);
        }

        [HttpGet]
        public IActionResult DownloadPricesReport(int days)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UpdatePrices))
                return AccessDeniedView();

            var controlDate90Days = DateTime.Now.AddDays(-1 * days).Date;
            var products = _productService.GetAllProductsQuery().Where(x => !x.Deleted && x.Published).ToList();
            var orderItems = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate90Days)
                .SelectMany(x => x.OrderItems)
                .ToList();

            var reportStatus = _orderReportStatusService.GetAll()
                .Where(x => x.StatusTypeId == 2 && x.ShippingDate >= controlDate90Days)
                .Select(x => DbFunctions.AddMilliseconds(x.ShippingDate, x.BuyerId))
                .Distinct()
                .ToList();
            var reports = _orderReportService.GetAll()
                .Where(x => x.OrderShippingDate >= controlDate90Days && x.ProductId > 0)
                .Select(x => new { x.OrderShippingDate, x.ReportedByCustomerId, x.ProductId, x.UpdatedUnitCost })
                .ToList()
                .Where(x => reportStatus.Contains(x.OrderShippingDate.AddMilliseconds(x.ReportedByCustomerId)))
                .ToList();

            var categoryGroup = products.GroupBy(x => x.ProductCategories.Where(y => y.Category.ParentCategoryId > 0).Select(y => y.Category).FirstOrDefault()).ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var categoriesCount = categoryGroup.Count;
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Categoría hijo";
                    worksheet.Cells[row, 2].Value = $"Simulación de ventas de últimos {days} días con precios actuales";
                    worksheet.Cells[row, 3].Value = "Costo según plataforma";
                    worksheet.Cells[row, 4].Value = "Margen con costo según plataforma";
                    worksheet.Cells[row, 5].Value = "Costo según último reporte de compradores (liquidado)";
                    worksheet.Cells[row, 6].Value = "Margen con costo según último reporte de compradores (liquidado)";
                    worksheet.Cells[row, 7].Value = "Ponderación";
                    worksheet.Cells[row, 8].Value = "Contribución de margen con costo según plataforma";
                    worksheet.Cells[row, 9].Value = "Contribución de margen con costo según último reporte de compradores (liquidado)";

                    foreach (var group in categoryGroup)
                    {
                        row++;
                        var productIds = group.GroupBy(x => x.Id).Select(x => x.Key).ToList();
                        var filteredItemsGroup = orderItems.Where(x => productIds.Contains(x.ProductId)).GroupBy(x => x.Product).ToList();
                        var sellPrices = new List<decimal>();
                        var currentCosts = new List<decimal>();
                        var lastReportedCosts = new List<decimal>();
                        foreach (var itemGroup in filteredItemsGroup)
                        {
                            int qty = itemGroup.Select(x => x.Quantity).DefaultIfEmpty().Sum();
                            var price = _paymentService.CalculateGroceryPrice(itemGroup.Key, null, qty, false);
                            var lastReportedUnitCost = reports.Where(x => x.ProductId == itemGroup.Key.Id).OrderByDescending(x => x.OrderShippingDate).Select(x => x.UpdatedUnitCost).FirstOrDefault();
                            if (price == 0)
                                price = qty * itemGroup.Key.Price;
                            sellPrices.Add(price);

                            currentCosts.Add(CalculateGroceryCost(itemGroup.Key, qty, itemGroup.Key.ProductCost));
                            lastReportedCosts.Add(CalculateGroceryCost(itemGroup.Key, qty, lastReportedUnitCost));
                        }

                        worksheet.Cells[row, 1].Value = group.Key == null ? "Sin categoría hijo" : group.Key.Name;
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
                    }

                    row++;
                    worksheet.Cells[row, 1].Value = "Total";
                    worksheet.Cells[row, 2].Formula = $"=SUM(B2:B{categoryGroup.Count() + 1})";
                    worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                    worksheet.Cells[row, 3].Formula = $"=SUM(C2:C{categoryGroup.Count() + 1})";
                    worksheet.Cells[row, 3].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                    worksheet.Cells[row, 5].Formula = $"=SUM(E2:E{categoryGroup.Count() + 1})";
                    worksheet.Cells[row, 5].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                    worksheet.Cells[row, 7].Formula = $"=SUM(G2:G{categoryGroup.Count() + 1})";
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "0.0000%";
                    worksheet.Cells[row, 8].Formula = $"=SUM(H2:H{categoryGroup.Count() + 1})";
                    worksheet.Cells[row, 8].Style.Numberformat.Format = "0.0000%";
                    worksheet.Cells[row, 9].Formula = $"=SUM(I2:I{categoryGroup.Count() + 1})";
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

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"ReporteMargenBrutoPromedioPonderado_{days}dias_{DateTime.Now:dd-MM-yyyy}_{DateTime.Now:HHmmss}.xlsx");
            }
        }

        //[HttpGet]
        //public IActionResult AutoUpdateAllCost()
        //{
        //    var controlDate = new DateTime(2021, 9, 15);
        //    var reportStatus = _orderReportStatusService.GetAll().Where(x => x.StatusTypeId == 2 && x.ShippingDate < controlDate)
        //       .GroupBy(x => DbFunctions.AddMilliseconds(x.ShippingDate, x.BuyerId))
        //       .Select(x => x.Key)
        //       .ToList();
        //    var reportsGroup = _orderReportService.GetAll()
        //        .Where(x => reportStatus.Contains(DbFunctions.AddMilliseconds(x.OrderShippingDate, x.OriginalBuyerId)))
        //        .ToList()
        //        .GroupBy(x => x.ProductId)
        //        .ToList();
        //    var productIds = reportsGroup.Select(x => x.Key).ToList();
        //    var products = _productService.GetAllProductsQuery().Where(x => productIds.Contains(x.Id)).ToList();
        //    var productLogs = _productLogService.GetAll().Where(x => productIds.Contains(x.ProductId)).ToList();

        //    int count = 0;
        //    foreach (var group in reportsGroup)
        //    {
        //        count++;
        //        var shouldUpdate = productLogs
        //            .Where(x => x.ProductId == group.Key && x.CreatedOnUtc.ToLocalTime() >= controlDate && x.Message.ToLower().Contains("actualizó el precio del producto"))
        //            .Any();
        //        if (!shouldUpdate) continue;
        //        var product = products.Where(x => x.Id == group.Key).FirstOrDefault();
        //        if (product == null) continue;
        //        var lastReportedCost = group.Where(x => x.UpdatedUnitCost > 0).OrderByDescending(x => x.OrderShippingDate).Select(x => x.UpdatedUnitCost).FirstOrDefault();
        //        if (lastReportedCost == 0) continue;
        //        if (Math.Round(product.ProductCost, 2) == Math.Round(lastReportedCost, 2)) continue;
        //        string log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - Se actualizó automáticamente el costo del producto según el último costo reportado por un comprador. El costo pasó de {product.ProductCost:C} a {lastReportedCost:C}";
        //        product.ProductCost = lastReportedCost;
        //        _productLogService.InsertProductLog(new ProductLog()
        //        {
        //            Message = log,
        //            ProductId = product.Id,
        //            UserId = _workContext.CurrentCustomer.Id,
        //            CreatedOnUtc = DateTime.UtcNow
        //        });
        //        _productService.UpdateProduct(product);
        //    }

        //    return NoContent();
        //}

        [HttpPost]
        public IActionResult GetNotDeliveredHistoricData(DataSourceRequest command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var items = _notDeliveredOrderItemService.GetAll().Where(x => x.ProductId == productId).OrderByDescending(x => x.CreatedOnUtc);
            var pagedList = new PagedList<NotDeliveredOrderItem>(items, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    Date = x.CreatedOnUtc.ToLocalTime().ToString("dd-MM-yyyy"),
                    x.OrderId,
                    Reason = string.IsNullOrWhiteSpace(x.NotDeliveredReason) ? ((NotDeliveredReason)x.NotDeliveredReasonId).GetDisplayName() : x.NotDeliveredReason
                }).ToList(),
                Total = items.Count()
            };

            return Json(gridModel);
        }

        [HttpGet]
        public IActionResult DownloadPricesReportByProduct(int categoryId, int days)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UpdatePrices))
                return AccessDeniedView();

            var controlDate90Days = DateTime.Now.AddDays(-1 * days).Date;
            var products = _productService.GetAllProductsQuery()
                .Where(x => !x.Deleted && x.Published && x.ProductCategories.Where(y => y.CategoryId == categoryId).Any())
                .ToList();
            var category = products.SelectMany(x => x.ProductCategories)
                .Where(x => x.CategoryId == categoryId)
                .Select(x => x.Category)
                .FirstOrDefault();
            if (category == null) return NotFound();
            var productIds = products.Select(x => x.Id).ToList();
            var orderItems = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate90Days)
                .SelectMany(x => x.OrderItems)
                .Where(x => productIds.Contains(x.ProductId))
                .ToList();
            var reportStatus = _orderReportStatusService.GetAll()
                .Where(x => x.StatusTypeId == 2 && x.ShippingDate >= controlDate90Days)
                .Select(x => DbFunctions.AddMilliseconds(x.ShippingDate, x.BuyerId))
                .Distinct()
                .ToList();
            var reports = _orderReportService.GetAll()
                .Select(x => new { x.OrderShippingDate, x.ProductId, x.ReportedByCustomerId, x.UpdatedUnitCost })
                .Where(x => x.OrderShippingDate >= controlDate90Days && productIds.Contains(x.ProductId))
                .ToList()
                .Where(x => reportStatus.Contains(x.OrderShippingDate.AddMilliseconds(x.ReportedByCustomerId)))
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var categoriesCount = products.Count;
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

                    foreach (var product in products)
                    {
                        row++;
                        var filteredItems = orderItems.Where(x => product.Id == x.ProductId).ToList();
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
                    }

                    row++;
                    worksheet.Cells[row, 1].Value = "Total";
                    worksheet.Cells[row, 2].Formula = $"=SUM(B2:B{products.Count() + 1})";
                    worksheet.Cells[row, 2].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                    worksheet.Cells[row, 3].Formula = $"=SUM(C2:C{products.Count() + 1})";
                    worksheet.Cells[row, 3].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                    worksheet.Cells[row, 5].Formula = $"=SUM(E2:E{products.Count() + 1})";
                    worksheet.Cells[row, 5].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                    worksheet.Cells[row, 7].Formula = $"=SUM(G2:G{products.Count() + 1})";
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "0.0000%";
                    worksheet.Cells[row, 8].Formula = $"=SUM(H2:H{products.Count() + 1})";
                    worksheet.Cells[row, 8].Style.Numberformat.Format = "0.0000%";
                    worksheet.Cells[row, 9].Formula = $"=SUM(I2:I{products.Count() + 1})";
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

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"ReporteMargenBrutoPromedioPonderado_{days}dias_{category.Name}_{DateTime.Now:dd-MM-yyyy}_{DateTime.Now:HHmmss}.xlsx");
            }
        }

        private List<OrderItemReportModel> GetOrderItemReport()
        {
            var approvedControlDate = DateTime.UtcNow.AddHours(-24);
            var orderControlDate = DateTime.UtcNow.AddDays(-7);
            var approvedReports = _orderReportStatusService.GetAll()
                .Where(x => x.StatusTypeId == 2 && x.ClosedDateUtc > approvedControlDate && x.ShippingDate > orderControlDate)
                .Select(x => new OrderReportFilterModel()
                {
                    BuyerId = x.BuyerId,
                    ShippingDate = x.ShippingDate
                }).ToList();

            var orderReports = _orderReportService.GetAll().Where(x => x.UpdatedUnitCost > 0).ToList();

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

        private decimal GetTotalCost(int[] orderIds)
        {
            return _orderReportService.GetAll().Where(x => orderIds.Contains(x.OrderId)).Select(x => x.RequestedQtyCost).DefaultIfEmpty().Sum();
        }

        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> UpdateProductPrices()
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins) && _workContext.CurrentCustomer.LastIpAddress != "40.117.235.223" && _workContext.CurrentCustomer.LastIpAddress != "127.0.0.1")
        //    {
        //        return AccessDeniedView();
        //    }

        //    var categories = _categoryService.GetAllCategories().Where(x => x.Coefficient > 0 || x.PercentageOfUtility > 0);
        //    var parents = categories.Where(x => x.ParentCategoryId == 0).ToList();
        //    var children = categories.Where(x => x.ParentCategoryId > 0).ToList();

        //    var productsToUpdate = new List<ProductUpdateModel>();

        //    try
        //    {
        //        foreach (var category in children)
        //        {
        //            if (category.PercentageOfUtility > 0)
        //            {
        //                var productsInCategory = _productService.SearchProducts(categoryIds: new List<int>() { category.Id }).Where(x => x.Published && x.ProductCost > 0).ToList();
        //                foreach (var product in productsInCategory)
        //                {
        //                    var value = 1 - (category.PercentageOfUtility / 100);
        //                    if (value > 0)
        //                    {
        //                        var oldPrice = product.Price;
        //                        product.Price = product.ProductCost / value;
        //                        productsToUpdate.Add(new ProductUpdateModel() { Product = product, OldPrice = oldPrice });
        //                    }
        //                }
        //            }
        //        }

        //        foreach (var category in parents)
        //        {
        //            if (category.Coefficient > 0)
        //            {
        //                var productsInCategory = _productService.SearchProducts(categoryIds: new List<int>() { category.Id })
        //                    .Where(x => x.Published && (x.WalmartProductId > 0 || x.SuperamaProductId > 0 || x.ChedrauiProductId > 0 || x.LaComerProductId > 0))
        //                    .ToList();

        //                foreach (var product in productsInCategory)
        //                {
        //                    DateTime date = DateTime.UtcNow.AddDays(-30);
        //                    List<decimal> prices = await GetWebScrapingHistory(date, product.WalmartProductId, product.LaComerProductId, product.ChedrauiProductId, product.SuperamaProductId);

        //                    if (prices.Count() > 0)
        //                    {
        //                        decimal oldPrice = product.Price;
        //                        product.Price = GetMedian(prices) * category.Coefficient;
        //                        productsToUpdate.Add(new ProductUpdateModel() { Product = product, OldPrice = oldPrice });
        //                    }
        //                }
        //            }
        //        }

        //        foreach (var product in productsToUpdate)
        //        {
        //            _productService.UpdateProduct(product.Product);
        //            _priceLogService.InsertPriceLog(new PriceLog()
        //            {
        //                CreatedOnUtc = DateTime.UtcNow,
        //                OldPrice = product.OldPrice,
        //                NewPrice = product.Product.Price,
        //                ProductId = product.Product.Id,
        //                Product = product.Product.Name,
        //                SKU = product.Product.Sku,
        //                User = "Actualización automática"
        //            });
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest(e.Message);
        //    }

        //    return Ok(productsToUpdate.Count);
        //}

        private decimal GetMedian(List<decimal> pricesList)
        {
            if (pricesList.Count == 0) return 0;
            var count = (decimal)(pricesList.Count() - 1);
            var index = (int)Math.Ceiling(count / 2);
            return pricesList.OrderBy(x => x).ElementAt(index);
        }

        private async Task<List<decimal>> GetWebScrapingHistory(DateTime date, int walmartProductId, int laComerProductId, int chedrauiProductId, int superamaProductId)
        {
            string url = API_URL + "/WebScraping/GetWebScrapingHistory?date=" + date.ToString("dd-MM-yyyy") + $"&walmartProductId={walmartProductId}&laComerProductId={laComerProductId}&chedrauiProductId={chedrauiProductId}&superamaProductId={superamaProductId}";
            using (HttpClient client = new HttpClient())
            {
                var result = await client.GetAsync(url);
                if (result.IsSuccessStatusCode)
                {
                    var json = await result.Content.ReadAsStringAsync();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<List<decimal>>(json);
                }
            }
            return new List<decimal>();
        }

        private decimal GetItemsCost(int[] itemIds)
        {
            var purchaseHistory = _orderReportService.GetAll().Where(x => itemIds.Contains(x.OrderItemId)).Select(x => x.RequestedQtyCost);
            return purchaseHistory.DefaultIfEmpty().Sum();
        }
    }

    public class ProductUpdateModel
    {
        public Product Product { get; set; }
        public decimal OldPrice { get; set; }
        public decimal OldCost { get; set; }
        public int NewManufacturerId { get; set; }
        public string ManufacturerName { get; set; }
    }

    public class CategorySalesData
    {
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public decimal Sales { get; set; }
        public decimal Cost { get; set; }
        public decimal SalesPercentage { get; set; }
        public decimal AverageMargin { get; set; }
        public decimal FinalMargin { get; set; }
    }

    public class ResultModel
    {
        public List<CategorySalesData> Data { get; set; }
        public decimal CurrentPercentage { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalCost { get; set; }
        public List<string> ErrorMessages { get; set; }
    }

    public class OrderItemReportModel
    {
        public OrderItem OrderItem { get; set; }
        public OrderReport OrderReport { get; set; }
    }

    public class OrderReportFilterModel
    {
        public int BuyerId { get; set; }
        public DateTime ShippingDate { get; set; }
    }

    public class CellDataModel
    {
        public string Address { get; set; }
        public string Value { get; set; }
    }

    public class CellObjectModel
    {
        public int? Id { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public string ParentCategories { get; set; }
        public string ChildCategories { get; set; }
        public string Tags { get; set; }
        public string CareType { get; set; }
        public string Hardness { get; set; }
        public decimal? Cost { get; set; }
        public string Tax { get; set; }
        public string Manufacturer { get; set; }
        public decimal? Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string Published { get; set; }
        public decimal? WeightInterval { get; set; }
        public decimal? EquivalenceOfCoefficient { get; set; }
        public string ProductProperties { get; set; }
        public string Labels { get; set; }
        public string SATCode { get; set; }
        public string AllowAutoUpdatePrice { get; set; }

    }

    public class ProductSatCode
    {
        public int ProductId { get; set; }
        public string ProductCatalogId { get; set; }
    }

    public class ProductPriceUpdateModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public List<string> LastReportedCosts { get; set; }
        public string BuyerMargin { get; set; }
        public decimal CurrentCostValue { get; set; }
        public string CurrentMargin { get; set; }
        public string CurrentPrice { get; set; }
        public decimal CurrentPriceValue { get; set; }
        public decimal LastReportedCostValue { get; set; }
    }

    public class UpdatePricesModel
    {
        public List<UpdatePrices> Data { get; set; }
    }

    public class UpdatePrices
    {
        public int ProductId { get; set; }
        public decimal UpdatedPrice { get; set; }
        public decimal UpdatedCost { get; set; }
    }
}