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

namespace Teed.Plugin.LamyShop.Controllers
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
        private readonly ICustomerService _customerService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IPermissionService _permissionService;

        public ReportController(IProductService productService, ICategoryService categoryService,
            ISpecificationAttributeService specificationAttributeService,
            IProductTagService productTagService, IProductAttributeService productAttributeService,
            IOrderService orderService, IShippingService shippingService,
            IUrlRecordService recordService, ICustomerService customerService,
            IProductAttributeFormatter productAttributeFormatter, IProductAttributeParser productAttributeParser,
            IPermissionService permissionService)
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
        }


        //Reporte de total de pedidos y cuanto es el total del costo de ellos, por cliente.
        [HttpGet]
        [Route("ReportSalesProducts")]
        public IActionResult ReportSalesProducts(string dateI, string dateE)
        {
            if (!_permissionService.Authorize(LamyShopPermissionProvider.ReportLamyShop))
                return AccessDeniedView();

            var i = 0;
            DateTime dateIni = DateTime.ParseExact(dateI, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime dateEnd = DateTime.ParseExact(dateE, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime dateByMoth = dateIni;

            var allOrders = GetFilteredOrdersSimple().Where(x => x.CreatedOnUtc >= dateIni && x.CreatedOnUtc <= dateEnd).ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {

                    #region VENTAS EN GENERAL
                    var worksheet = xlPackage.Workbook.Worksheets.Add("Ventas_General");
                    int row = 1;
                    var groupCustomerId = allOrders.GroupBy(x =>  x.CustomerId).ToList();

                    worksheet.Cells[row, 1].Value = "Número del artículo";
                    worksheet.Cells[row, 2].Value = "Descripción del artículo";
                    worksheet.Cells[row, 3].Value = "Código de barras";
                    worksheet.Cells[row, 4].Value = "Categoría";
                    worksheet.Cells[row, 5].Value = "Código de cliente";
                    worksheet.Cells[row, 6].Value = "Nombre del cliente";
                    worksheet.Cells[row, 7].Value = "Cantidad vendida";
                    worksheet.Cells[row, 8].Value = "Importe de venta";

                    foreach (var customerOrders in groupCustomerId)
                    {
                        var OrdersGroupByOrderItems = customerOrders.SelectMany(x => x.OrderItems).ToList();
                        var productsByGroupAttrib = OrdersGroupByOrderItems.GroupBy(x => x.ProductId).ToList();
                        row++;
                        foreach (var productByProduct in productsByGroupAttrib)
                        {
                            var xmlAttrib = productByProduct.Select(x => x.AttributesXml).Distinct().ToList();
                            foreach (var attrib in xmlAttrib)
                            {
                                var orderItemsOfXml = productByProduct.Where(x => x.AttributesXml == attrib).ToList();
                                var firstOrderItem = orderItemsOfXml.FirstOrDefault();
                                worksheet.Cells[row, 1].Value = firstOrderItem.ProductId;
                                worksheet.Cells[row, 2].Value = firstOrderItem.Product.Name + ", " + Regex.Replace(Regex.Replace(firstOrderItem.AttributeDescription, @"<[^>].+?>", "/"), @"\[[^\]]*\]", " ");
                                worksheet.Cells[row, 3].Value = firstOrderItem.Product.ProductAttributeCombinations.Select(x => x.Gtin).FirstOrDefault()?? firstOrderItem.Product.Sku;
                                worksheet.Cells[row, 4].Value = firstOrderItem.Product.ProductCategories.Select(x => x.Category.Name);
                                worksheet.Cells[row, 5].Value = firstOrderItem.Order.Customer.Id;
                                worksheet.Cells[row, 6].Value = string.IsNullOrEmpty(firstOrderItem.Order.Customer.GetFullName()) ? "Invitado" : firstOrderItem.Order.Customer.GetFullName();
                                worksheet.Cells[row, 7].Value = orderItemsOfXml.Count();
                                worksheet.Cells[row, 8].Value = orderItemsOfXml.Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
                            }
                        }
                        
                    }
                    
                    for (i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }
                    #endregion VENTAS EN GENERAL


                    #region PRODUCTOS VENDIDOS
                    worksheet = xlPackage.Workbook.Worksheets.Add("Productos_Vendidos");
                    row = 1;
                    var groupingProducts = allOrders.Select(x => x.OrderItems)
                        .SelectMany(x => x)
                        .GroupBy(x => x.Product)
                        .OrderByDescending(x => x.Key.DisplayOrder)
                        .ToList();
                    dateByMoth = dateIni;
                    var allOrdersDate = allOrders.GroupBy(x => x.CreatedOnUtc.Year, x => x.CreatedOnUtc.Month).ToList();

                    worksheet.Cells[row, 1].Value = "Código de producto";
                    worksheet.Cells[row, 2].Value = "SKU";
                    worksheet.Cells[row, 3].Value = "Nombre del producto";
                    worksheet.Cells[row, 4].Value = "Categoría";
                    worksheet.Cells[row, 5].Value = "Color";
                    worksheet.Cells[row, 6].Value = "Punta";
                    i = 0;
                    while (dateByMoth <= dateEnd)
                    {
                        worksheet.Cells[row, 7 + i].Value = "Monto total vendido sin IVA " + dateByMoth.ToString("dd/MM/yyyy");
                        worksheet.Cells[row, 7 + i].Style.Numberformat.Format = "mmmm, yyyy";
                        worksheet.Cells[row, 8 + i].Value = "Total de piezas vendidas " + dateByMoth.ToString("dd/MM/yyyy");
                        i += 2;
                        dateByMoth = dateByMoth.AddMonths(1);
                    }

                    foreach (var item in groupingProducts)
                    {
                        i = 0;
                        row++;
                        dateByMoth = dateIni;
                        var xml = item.Key.ProductAttributeCombinations.Select(x => x.AttributesXml).FirstOrDefault();
                        var attrib = _productAttributeParser.ParseProductAttributeValues(xml).ToList();
                        worksheet.Cells[row, 1].Value = item.Key.Id + "-" + attrib.Where(x => x.ProductAttributeMapping.ProductAttributeId == 1).FirstOrDefault()?.Id ?? "0" +
                            "-" + attrib.Where(x => x.ProductAttributeMapping.ProductAttributeId == 2).FirstOrDefault()?.Id ?? "0";
                        worksheet.Cells[row, 2].Value = item.Key.ProductAttributeCombinations.Select(x => x.Gtin).FirstOrDefault()?? item.Key.Sku;
                        worksheet.Cells[row, 3].Value = item.Key.Name + ", " + Regex.Replace(Regex.Replace(item.Select(x => x.AttributeDescription).FirstOrDefault(), @"<[^>].+?>", "/"), @"\[[^\]]*\]", " ");
                        worksheet.Cells[row, 4].Value = item.Key.ProductCategories.Select(x => x.Category.Name);
                        worksheet.Cells[row, 5].Value = attrib.Where(x => x.ProductAttributeMapping.ProductAttributeId == 1).FirstOrDefault()?.Name;
                        worksheet.Cells[row, 6].Value = attrib.Where(x => x.ProductAttributeMapping.ProductAttributeId == 2).FirstOrDefault()?.Name;


                        while (dateByMoth <= dateEnd)
                        {
                            var productOrderItemsByMonth = item
                            .Where(x => x.Order.CreatedOnUtc != null &&
                            x.Order.CreatedOnUtc.Month == dateByMoth.Month && x.Order.CreatedOnUtc.Year == dateByMoth.Year)
                            .ToList();
                            worksheet.Cells[row, 7 + i].Value = Math.Round(((productOrderItemsByMonth.Select(x => x.PriceExclTax).Sum()) / (decimal)1.16), 2);
                            worksheet.Cells[row, 8 + i].Value = productOrderItemsByMonth.Select(x => x.Quantity).DefaultIfEmpty().Sum();
                            i += 2;
                            dateByMoth = dateByMoth.AddMonths(1);
                        }
                    }

                    for (i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }
                    #endregion PRODUCTOS VENDIDOS

                    #region Ventas por clientes

                    var orderbyCustomer = allOrders.OrderBy(x => x.CustomerId).ToList();

                    worksheet = xlPackage.Workbook.Worksheets.Add("Ventas_Por_Cliente");

                    row = 1;
                    var dateCont = dateIni;
                    worksheet.Cells[row, 1].Value = "No Cliente";
                    worksheet.Cells[row, 2].Value = "Nombre de cliente";
                    worksheet.Cells[row, 3].Value = "Apellido";
                    worksheet.Cells[row, 4].Value = "Sexo";
                    worksheet.Cells[row, 5].Value = "Correo";
                    worksheet.Cells[row, 6].Value = "Teléfono";
                    worksheet.Cells[row, 7].Value = "Calle y número";
                    worksheet.Cells[row, 8].Value = "Colónia";
                    worksheet.Cells[row, 9].Value = "Delegación o municipio";
                    worksheet.Cells[row, 10].Value = "Ciudad";
                    worksheet.Cells[row, 11].Value = "Número de pedido";
                    worksheet.Cells[row, 12].Value = "Monto con IVA";
                    worksheet.Cells[row, 13].Value = "Número de Piezas";
                    worksheet.Cells[row, 14].Value = "Fecha de pedido";
                    foreach (var customer in orderbyCustomer)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = customer.CustomerId;
                        worksheet.Cells[row, 2].Value = customer.ShippingAddress.FirstName;
                        worksheet.Cells[row, 3].Value = customer.ShippingAddress.LastName;
                        worksheet.Cells[row, 4].Value = customer.Customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender);
                        worksheet.Cells[row, 5].Value = customer.ShippingAddress.Email;
                        worksheet.Cells[row, 6].Value = customer.ShippingAddress.PhoneNumber;
                        worksheet.Cells[row, 7].Value = customer.ShippingAddress.Address1;
                        worksheet.Cells[row, 8].Value = customer.ShippingAddress.Address2;
                        worksheet.Cells[row, 9].Value = customer.ShippingAddress?.City;
                        worksheet.Cells[row, 10].Value = customer.ShippingAddress?.Country.Name;
                        worksheet.Cells[row, 11].Value = customer.OrderItems.Select(x => x.OrderId);
                        worksheet.Cells[row, 12].Value = customer.OrderTotal;
                        worksheet.Cells[row, 13].Value = customer.OrderItems.Select(x => x.Quantity).DefaultIfEmpty().Sum();
                        worksheet.Cells[row, 14].Value = customer.CreatedOnUtc.ToLocalTime();
                        worksheet.Cells[row, 14].Style.Numberformat.Format = "dd-mm-yyyy";
                    }

                    for (i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    #endregion Ventas por clientes

                    #region Clientes registrados

                    var ordersGroupByCustomer = allOrders.GroupBy(x => x.CustomerId).ToList();
                    var customerIds = ordersGroupByCustomer.Select(x => x.Key).ToList();
                    var customers = _customerService.GetAllCustomersQuery().Where(x => x.Email != null && x.Email != "" && customerIds.Contains(x.Id)).ToList();

                    worksheet = xlPackage.Workbook.Worksheets.Add("Clientes registrados");
                    row = 1;

                    worksheet.Cells[row, 1].Value = "No cliente";
                    worksheet.Cells[row, 2].Value = "Nombre completo";
                    worksheet.Cells[row, 3].Value = "Correo";
                    worksheet.Cells[row, 4].Value = "Teléfono";
                    worksheet.Cells[row, 5].Value = "Sexo";
                    worksheet.Cells[row, 6].Value = "Delegación o municipio";
                    worksheet.Cells[row, 7].Value = "Ciudad";
                    worksheet.Cells[row, 8].Value = "Numero de pedidos";
                    worksheet.Cells[row, 9].Value = "Monto total comprado";

                    foreach (var customer in customers)
                    {
                        row++;
                        if (customer.IsRegistered())
                        {
                            var shippingAddress = customer.Addresses.FirstOrDefault();

                            worksheet.Cells[row, 1].Value = customer.Id;
                            worksheet.Cells[row, 2].Value = customer.GetFullName();
                            worksheet.Cells[row, 3].Value = customer.Email;
                            worksheet.Cells[row, 4].Value = shippingAddress?.PhoneNumber;
                            worksheet.Cells[row, 5].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender);
                            worksheet.Cells[row, 6].Value = shippingAddress?.City;
                            worksheet.Cells[row, 7].Value = shippingAddress?.Country.Name;
                            worksheet.Cells[row, 8].Value = ordersGroupByCustomer.Where(x => x.Key == customer.Id).FirstOrDefault().Count();
                            worksheet.Cells[row, 9].Value = ordersGroupByCustomer.Where(x => x.Key == customer.Id).Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Sum();
                        }
                    }

                    for ( i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    #endregion Reporte de clientes registrados

                    xlPackage.Save();
                }
                return File(stream.ToArray(), MimeTypes.TextXlsx, $"Reporte_de_ventas_ " + dateIni.ToString("dd_MM_yyyy") + " - " + dateEnd.ToString("dd_MM_yyyy") + ".xlsx");
            }
        }

        private IQueryable<Order> GetFilteredOrdersSimple()
        {
            return _orderService.GetAllOrdersQuery()
                .Where(x => !x.Deleted && x.OrderStatusId != 40 && x.PaymentStatusId != 10);
        }
    }
}