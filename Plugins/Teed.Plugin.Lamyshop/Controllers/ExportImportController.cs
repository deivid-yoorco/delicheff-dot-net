using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
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
using System.Text.RegularExpressions;
using Nop.Services.Common;
using Nop.Core.Domain.Customers;
using Teed.Plugin.LamyShop.Components;
using Nop.Services.Security;
using Teed.Plugin.LamyShop.Security;
using Nop.Services.Discounts;
using Nop.Services.Tax;
using Nop.Services.Directory;

namespace Teed.Plugin.LamyShop.Controllers
{
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    public class ExportImportController : BasePluginController
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductTagService _productTagService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IOrderService _orderService;
        private readonly IShippingService _shippingService;
        private readonly IUrlRecordService _recordService;
        private readonly ICustomerService _customerService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IPermissionService _permissionService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IWorkContext _workContext;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;

        public ExportImportController(IProductService productService, ICategoryService categoryService,
            ISpecificationAttributeService specificationAttributeService,
            IProductTagService productTagService, IProductAttributeService productAttributeService,
            IOrderService orderService, IShippingService shippingService,
            IUrlRecordService recordService, ICustomerService customerService,
            IProductAttributeFormatter productAttributeFormatter, IProductAttributeParser productAttributeParser,
            IPermissionService permissionService, IPriceCalculationService priceCalculationService,
            IWorkContext workContext, ITaxService taxService, ICurrencyService currencyService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _specificationAttributeService = specificationAttributeService;
            _productTagService = productTagService;
            _productAttributeService = productAttributeService;
            _orderService = orderService;
            _shippingService = shippingService;
            _recordService = recordService;
            _customerService = customerService;
            _productAttributeFormatter = productAttributeFormatter;
            _productAttributeParser = productAttributeParser;
            _permissionService = permissionService;
            _priceCalculationService = priceCalculationService;
            _workContext = workContext;
            _taxService = taxService;
            _currencyService = currencyService;
        }

        [HttpGet]
        public IActionResult GetProductAttributeCombinationsXls(bool forPrices = false)
        {
            var products = _productService.GetAllProductsQuery()
                .Where(x => !x.Deleted)
                .OrderBy(x => x.Name).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Id del producto";
                    worksheet.Cells[row, 2].Value = "SKU del producto";
                    worksheet.Cells[row, 3].Value = "Producto";
                    worksheet.Cells[row, 4].Value = "Id de la combinacion";
                    worksheet.Cells[row, 5].Value = "SKU de la combinacion";
                    worksheet.Cells[row, 6].Value = "GTIN de la combinacion";
                    worksheet.Cells[row, 7].Value = "Combinación de atributos";
                    if (forPrices)
                        worksheet.Cells[row, 8].Value = "Precio total";
                    else
                        worksheet.Cells[row, 8].Value = "Cantidad";
                    var lastHadCombinations = false;

                    foreach (var product in products)
                    {
                        if (!lastHadCombinations)
                            row++;
                        else
                            lastHadCombinations = false;
                        worksheet.Cells[row, 1].Value = product.Id;
                        worksheet.Cells[row, 2].Value = product.Sku;
                        worksheet.Cells[row, 3].Value = product.Name;
                        worksheet.Cells[row, 3].Style.Font.Bold = true;
                        var productCombinations = product.ProductAttributeCombinations.ToList();
                        if (productCombinations.Any())
                        {
                            foreach (var productCombination in productCombinations)
                            {
                                worksheet.Cells[row, 4].Value = productCombination.Id;
                                worksheet.Cells[row, 5].Value = productCombination.Sku;
                                worksheet.Cells[row, 6].Value = productCombination.Gtin;
                                worksheet.Cells[row, 7].Value = _productAttributeFormatter.FormatAttributes(product, productCombination.AttributesXml, _workContext.CurrentCustomer, " - ", true, true, true, false);
                                if (forPrices)
                                {
                                    DateTime? rentalStartDate = null;
                                    DateTime? rentalEndDate = null;
                                    List<DiscountForCaching> scDiscounts;
                                    var finalWithoutDiscount = _priceCalculationService.GetUnitPrice(product,
                                        _workContext.CurrentCustomer,
                                        ShoppingCartType.ShoppingCart,
                                        1, productCombination.AttributesXml, 0,
                                        rentalStartDate, rentalEndDate,
                                        false, out decimal _, out scDiscounts);
                                    var finalPriceWithoutDiscountBase = _taxService.GetProductPrice(product, finalWithoutDiscount, out decimal _);
                                    var finalPriceWithoutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase, _workContext.WorkingCurrency);

                                    worksheet.Cells[row, 8].Value = finalPriceWithoutDiscount;

                                    var count = 0;
                                    var col = 8;
                                    var attributes = _productAttributeParser.ParseProductAttributeValues(productCombination.AttributesXml);
                                    foreach (var attribute in attributes)
                                    {
                                        count++;
                                        var attributeParent = attribute.ProductAttributeMapping.ProductAttribute.Name;
                                        col++;
                                        worksheet.Cells[1, col].Value = $"Id del {count}° atributo";
                                        worksheet.Cells[row, col].Value = attribute.Id;
                                        col++;
                                        worksheet.Cells[1, col].Value = $"{count}° atributo";
                                        worksheet.Cells[row, col].Value = attributeParent + ": " + attribute.Name;
                                        col++;
                                        worksheet.Cells[1, col].Value = $"{count}° precio de ajuste";
                                        worksheet.Cells[row, col].Value = attribute.PriceAdjustment;
                                    }
                                }
                                else
                                    worksheet.Cells[row, 8].Value = productCombination.StockQuantity;
                                row++;
                            }
                            lastHadCombinations = true;
                        }
                        else
                        {
                            if (forPrices)
                            {
                                worksheet.Cells[row, 8].Value = product.Price;
                            }
                            else
                            {
                                worksheet.Cells[row, 8].Value = product.StockQuantity;
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

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"lamy_productos_combinacion_atributos.xlsx");
            }
        }

        [HttpGet]
        public IActionResult SetAttributesFromExcel(bool forPrices = false)
        {
            // path to your excel file
            string path = "C:/Users/Ivan Salazar/Desktop/attributes.xlsx";
            FileInfo fileInfo = new FileInfo(path);
            var errorList = new List<CellErrorModel1>();

            using (var memoryStream = new MemoryStream())
            {
                using (var package = new ExcelPackage(fileInfo.OpenRead()))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet != null)
                    {
                        int init = 0;
                        List<string> headers = new List<string>();
                        int totalRows = worksheet.Dimension.End.Row;
                        int totalColumns = worksheet.Dimension.End.Column;
                        var range = worksheet.Cells[1, 1, 1, totalColumns];

                        //GetHeaders(ref init, ref headers, totalColumns, range, worksheet);
                        //if (!headers.Contains("Id del producto") ||
                        //    !headers.Contains("SKU del producto") ||
                        //    !headers.Contains("Producto") ||
                        //    !headers.Contains("Id de la combinacion") ||
                        //    !headers.Contains("SKU de la combinacion") ||
                        //    !headers.Contains("GTIN de la combinacion") ||
                        //    !headers.Contains("Combinación de atributos") ||
                        //    !headers.Contains("Cantidad")
                        //    )
                        //{
                        //    return BadRequest("El archivo no tiene las columnas correctas.");
                        //}

                        try
                        {
                            var cells = worksheet.Cells.ToList();
                            var groups = GetCellGroups(cells, worksheet.Dimension.End.Row - 1);
                            if (groups == null) return BadRequest("Ocurrió un problema creando los grupos para la carga de datos");
                            var dataList = new List<CellObjectModel1>();
                            for (int g = 0; g < groups.Count; g++)
                            {
                                int currentColumn = 0;
                                var data = new CellObjectModel1();
                                data.ProductId = groups[g][currentColumn].Value != null && !string.IsNullOrWhiteSpace(groups[g][currentColumn].Value.ToString()) ? (int?)int.Parse(groups[g][currentColumn].Value.ToString()) : null;
                                currentColumn++;
                                data.ProductSku = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.ProductName = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.CombinationId = groups[g][currentColumn].Value != null && !string.IsNullOrWhiteSpace(groups[g][currentColumn].Value.ToString()) ? (int?)int.Parse(groups[g][currentColumn].Value.ToString()) : null;
                                currentColumn++;
                                data.CombinationSku = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.CombinationGtin = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                data.Combination = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                currentColumn++;
                                //data.TotalPrice = groups[g][currentColumn].Value != null && !string.IsNullOrWhiteSpace(groups[g][currentColumn].Value.ToString()) ? (decimal?)decimal.Parse(groups[g][currentColumn].Value.ToString()) : null;
                                //currentColumn++;
                                int value;
                                if (int.TryParse(groups[g][currentColumn].Value?.ToString(), out value))
                                {
                                    // Parse successful. value can be any integer
                                    data.Quantity = value;
                                }
                                else
                                {
                                    // Parse failed. value will be 0.
                                    data.Quantity = 0;
                                }
                                //data.FirstAttributeId = groups[g][currentColumn].Value != null && !string.IsNullOrWhiteSpace(groups[g][currentColumn].Value.ToString()) ? (int?)int.Parse(groups[g][currentColumn].Value.ToString()) : null;
                                //currentColumn++;
                                //data.FirstAttribute = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                //currentColumn++;
                                //data.FirstAttributePrice = groups[g][currentColumn].Value != null && !string.IsNullOrWhiteSpace(groups[g][currentColumn].Value.ToString()) ? (decimal?)decimal.Parse(groups[g][currentColumn].Value.ToString()) : null;
                                //currentColumn++;
                                //data.SecondAttributeId = groups[g][currentColumn].Value != null && !string.IsNullOrWhiteSpace(groups[g][currentColumn].Value.ToString()) ? (int?)int.Parse(groups[g][currentColumn].Value.ToString()) : null;
                                //currentColumn++;
                                //data.SecondAttribute = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString() : null;
                                //currentColumn++;
                                //data.SecondAttributePrice = groups[g][currentColumn].Value != null && !string.IsNullOrWhiteSpace(groups[g][currentColumn].Value.ToString()) ? (decimal?)decimal.Parse(groups[g][currentColumn].Value.ToString()) : null;
                                dataList.Add(data);
                            }

                            // Start uploading data
                            int count = 0;
                            foreach (var data in dataList)
                            {
                                count++;
                                try
                                {
                                    var attribute = _productAttributeService.GetProductAttributeCombinationById(data.CombinationId ?? 0);
                                    if (attribute != null)
                                    {
                                        if (attribute.StockQuantity != data.Quantity && data.Quantity != null)
                                        {
                                            attribute.StockQuantity = data.Quantity ?? 0;
                                            _productAttributeService.UpdateProductAttributeCombination(attribute);
                                        }
                                    }
                                    else
                                    {
                                        var product = _productService.GetProductById(data.ProductId ?? 0);
                                        if (product != null)
                                        {
                                            product.StockQuantity = data.Quantity ?? 0;
                                            if (data.Quantity < 1)
                                                product.Published = false;
                                            _productService.UpdateProduct(product);
                                        }
                                        else
                                        {
                                            errorList.Add(new CellErrorModel1
                                            {
                                                CellObjectModel = data,
                                                Error = "Product not found"
                                            });
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    errorList.Add(new CellErrorModel1
                                    {
                                        CellObjectModel = data,
                                        Error = e.Message
                                    });
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
            }
            if (errorList.Any())
                return Ok("Se actualizaron codigos del sat pero algunos tuvieron errores: \n"
                    + string.Join("\n", errorList.Select(x => x.Error)));
            else
                return Ok("Los codigos del sat se actualizaron correctamente.");
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
    }

    public class CellObjectModel1
    {
        public int? ProductId { get; set; }
        public string ProductSku { get; set; }
        public string ProductName { get; set; }
        public int? CombinationId { get; set; }
        public string CombinationSku { get; set; }
        public string CombinationGtin { get; set; }
        public string Combination { get; set; }
        public decimal? TotalPrice { get; set; }
        public int? Quantity { get; set; }
        //public int? FirstAttributeId { get; set; }
        //public string FirstAttribute { get; set; }
        //public decimal? FirstAttributePrice { get; set; }
        //public int? SecondAttributeId { get; set; }
        //public string SecondAttribute { get; set; }
        //public decimal? SecondAttributePrice { get; set; }

    }

    public class CellErrorModel1
    {
        public CellObjectModel1 CellObjectModel { get; set; }
        public string Error { get; set; }
    }

    public class CellDataModel
    {
        public string Address { get; set; }
        public string Value { get; set; }
    }
}