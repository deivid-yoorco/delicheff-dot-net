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
using Teed.Plugin.Api.Services;

namespace Teed.Plugin.Api.Controllers
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

        public ReportController(IProductService productService, ICategoryService categoryService,
            ISpecificationAttributeService specificationAttributeService,
            IProductTagService productTagService, IProductAttributeService productAttributeService,
            IOrderService orderService, IShippingService shippingService,
            IUrlRecordService recordService, ICustomerService customerService)
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
        }


        ////Reporte de total de pedidos y cuanto es el total del costo de ellos, por cliente.
        //[HttpGet]
        //[AllowAnonymous]
        //[Route("GenerateExcelReport")]
        //public IActionResult GenerateExcelReport()
        //{
        //    var filteredOrders = GetFilteredOrders().OrderBy(x => x.CustomerId)
        //        .GroupBy(x => x.Customer)
        //        .ToList();

        //    using (var stream = new MemoryStream())
        //    {
        //        using (var xlPackage = new ExcelPackage(stream))
        //        {
        //            var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
        //            int row = 1;
        //            worksheet.Cells[row, 1].Value = "Id de usuario";
        //            worksheet.Cells[row, 2].Value = "Correo electrónico";
        //            worksheet.Cells[row, 3].Value = "Cantidad de órdenes hechas por el usuario";
        //            worksheet.Cells[row, 4].Value = "Total de las compras hechas por el usuario";

        //            foreach (var order in filteredOrders)
        //            {
        //                var ordersOfClient = order.ToList();
        //                row++;
        //                worksheet.Cells[row, 1].Value = order.Key.Id;
        //                worksheet.Cells[row, 2].Value = order.Key.Email;
        //                worksheet.Cells[row, 3].Value = ordersOfClient.Count();
        //                worksheet.Cells[row, 4].Value = ordersOfClient.Sum(x => x.OrderTotal);
        //            }

        //            for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
        //            {
        //                worksheet.Column(i).AutoFit();
        //                worksheet.Cells[1, i].Style.Font.Bold = true;
        //            }

        //            xlPackage.Save();
        //        }

        //        return File(stream.ToArray(), MimeTypes.TextXlsx, $"Reporte de ventas y pedidos totales por usuario.xlsx");
        //    }
        //}

        ////Reporte de productos sin imagen
        //[HttpGet]
        //[AllowAnonymous]
        //[Route("GenerateExcelReport2")]
        //public IActionResult GenerateExcelReport2()
        //{
        //    var filteredOrders = _productService.SearchProducts().Where(x => x.ProductPictures.Count == 0).ToList();

        //    using (var stream = new MemoryStream())
        //    {
        //        using (var xlPackage = new ExcelPackage(stream))
        //        {
        //            var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
        //            int row = 1;
        //            worksheet.Cells[row, 1].Value = "SKU del producto";
        //            worksheet.Cells[row, 2].Value = "Nombre del producto";

        //            foreach (var order in filteredOrders)
        //            {
        //                row++;
        //                worksheet.Cells[row, 1].Value = order.Id;
        //                worksheet.Cells[row, 2].Value = order.Name;
        //            }

        //            for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
        //            {
        //                worksheet.Column(i).AutoFit();
        //                worksheet.Cells[1, i].Style.Font.Bold = true;
        //            }

        //            xlPackage.Save();
        //        }

        //        return File(stream.ToArray(), MimeTypes.TextXlsx, $"SKU de productos sin imagen.xlsx");
        //    }
        //}


        //private IQueryable<Order> GetFilteredOrders()
        //{
        //    return _orderService.GetAllOrdersQuery()
        //        .Where(x => !x.Deleted && x.OrderStatusId != 40 &&
        //        !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10));
        //}
    }
}