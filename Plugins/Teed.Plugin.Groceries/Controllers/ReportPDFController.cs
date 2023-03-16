using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Layout.Element;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Properties;
using iText.Kernel.Font;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Web.Framework.Controllers;
using System;
using System.IO;
using System.Linq;
using Nop.Core.Domain.Shipping;
using iText.IO.Font;
using Nop.Web.Framework;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Kendoui;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Nop.Services.Media;
using Nop.Core.Domain.Media;
using Nop.Services;
using Nop.Services.Orders;
using iText.IO.Image;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class ReportPDFController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly IShippingService _shippingService;
        private readonly ILogger _logger;
        private readonly IProductService _productService;
        private readonly IPriceLogService _priceLogService;
        private readonly IProductLogService _productLogService;
        private readonly ISettingService _settingService;
        private readonly TaxSettings _taxSettings;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IPictureService _pictureService;
        private readonly IOrderService _orderService;

        public ReportPDFController(IPermissionService permissionService, IWorkContext workContext,
            ICustomerService customerService,
            IShippingService shippingService, ILogger logger, IPriceLogService priceLogService,
            IProductService productService, IStoreContext storeContext,
            IProductLogService productLogService, ISettingService settingService,
            TaxSettings taxSettings, ITaxCategoryService taxCategoryService, IPictureService pictureService,
            IOrderService orderService)
        {
            _permissionService = permissionService;
            _workContext = workContext;
            _customerService = customerService;
            _shippingService = shippingService;
            _logger = logger;
            _priceLogService = priceLogService;
            _productService = productService;
            _storeContext = storeContext;
            _productLogService = productLogService;
            _settingService = settingService;
            _taxSettings = taxSettings;
            _taxCategoryService = taxCategoryService;
            _pictureService = pictureService;
            _orderService = orderService;
        }

        [HttpGet]
        [Route("ReportPDFProductos")]
        public IActionResult ReportPDFProductos()
        {

            //var parsDate = DateTime.Now;
            Table mainTable;
            var products = _productService.GetAllProductsQuery().ToList();
            var logoPicture = _pictureService.GetPictureById(1);
            var logoExists = logoPicture != null;


            string imageFile = $"wwwroot/images/logo_sp.png";
            ImageData data = ImageDataFactory.Create(imageFile);

            Image img = new Image(data);
            img.SetHeight(60);
            img.SetWidth(90);

            Paragraph Img = new Paragraph();
            Img.Add(img);
            Img.SetTextAlignment(TextAlignment.CENTER);
            Img.SetHorizontalAlignment(HorizontalAlignment.CENTER);

            MemoryStream ms = new MemoryStream();
            PdfWriter pw = new PdfWriter(ms);
            PdfDocument document = new PdfDocument(pw);
            Document doc = new Document(document, PageSize.A4);

            #region header

            mainTable = new Table(2);
            mainTable.SetWidth(UnitValue.CreatePercentValue(80));
            mainTable.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).SetWidth(UnitValue.CreatePercentValue(30))
                .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                .Add(img)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER));
            mainTable.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).SetWidth(UnitValue.CreatePercentValue(30))
                .Add(new Paragraph("Reporte de productos").SetBold().SetTextAlignment(TextAlignment.CENTER)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER)).SetHorizontalAlignment(HorizontalAlignment.CENTER));

            mainTable.AddCell(new Cell().SetBorder(Border.NO_BORDER)
                        .Add(new Paragraph(" ")));
            mainTable.AddCell(new Cell().SetBorder(Border.NO_BORDER)
                        .Add(new Paragraph(" ")));

            doc.Add(mainTable);


            //doc.Add(mainTable.SetBorder(new SolidBorder(ColorConstants.WHITE, 1)).SetWidth(UnitValue.CreatePercentValue(100)));
            #endregion header

            #region product
            mainTable = new Table(1);
            mainTable.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER)
                        .Add(new Paragraph(" ")));
            mainTable.AddCell(new Cell().SetBorder(Border.NO_BORDER)
                        .Add(new Paragraph(" ")));

            doc.Add(mainTable);

            mainTable = new Table(9);
            mainTable.SetWidth(UnitValue.CreatePercentValue(90)).SetBorderRadius(new BorderRadius(5));
            mainTable.AddHeaderCell(new Cell().SetBackgroundColor(WebColors.GetRGBColor("#064786")).SetBorder(Border.NO_BORDER)
                .SetWidth(UnitValue.CreatePercentValue(10))
                .Add(new Paragraph("PRODUCTO").SetBold().SetTextAlignment(TextAlignment.CENTER)).SetFontSize(10)
                .SetFontColor(WebColors.GetRGBColor("#FFFFFF")));

            mainTable.AddHeaderCell(new Cell().SetBackgroundColor(WebColors.GetRGBColor("#064786")).SetBorder(Border.NO_BORDER)
                .SetWidth(UnitValue.CreatePercentValue(10))
                .Add(new Paragraph("DESCRIPCIÓN").SetBold().SetTextAlignment(TextAlignment.CENTER)).SetFontSize(10)
                .SetFontColor(WebColors.GetRGBColor("#FFFFFF")));

            mainTable.AddHeaderCell(new Cell().SetBackgroundColor(WebColors.GetRGBColor("#064786")).SetBorder(Border.NO_BORDER)
                .SetWidth(UnitValue.CreatePercentValue(10))
                .Add(new Paragraph("CATEGORÍA").SetBold().SetTextAlignment(TextAlignment.CENTER)).SetFontSize(10)
                .SetFontColor(WebColors.GetRGBColor("#FFFFFF")));
            mainTable.AddHeaderCell(new Cell().SetBackgroundColor(WebColors.GetRGBColor("#064786")).SetBorder(Border.NO_BORDER)
                .SetWidth(UnitValue.CreatePercentValue(10))
                .Add(new Paragraph("GTIN").SetBold().SetTextAlignment(TextAlignment.CENTER)).SetFontSize(10)
                .SetFontColor(WebColors.GetRGBColor("#FFFFFF")));
            mainTable.AddHeaderCell(new Cell().SetBackgroundColor(WebColors.GetRGBColor("#064786")).SetBorder(Border.NO_BORDER)
                .SetWidth(UnitValue.CreatePercentValue(10))
                .Add(new Paragraph("SKU").SetBold().SetTextAlignment(TextAlignment.CENTER)).SetFontSize(10)
                .SetFontColor(WebColors.GetRGBColor("#FFFFFF")));
            mainTable.AddHeaderCell(new Cell().SetBackgroundColor(WebColors.GetRGBColor("#064786")).SetBorder(Border.NO_BORDER)
                .SetWidth(UnitValue.CreatePercentValue(10))
                .Add(new Paragraph("EXISTENCIAS").SetBold().SetTextAlignment(TextAlignment.CENTER)).SetFontSize(10)
                .SetFontColor(WebColors.GetRGBColor("#FFFFFF")));
            mainTable.AddHeaderCell(new Cell().SetBackgroundColor(WebColors.GetRGBColor("#064786")).SetBorder(Border.NO_BORDER)
                .SetWidth(UnitValue.CreatePercentValue(10))
                .Add(new Paragraph("PRECIO").SetBold().SetTextAlignment(TextAlignment.CENTER)).SetFontSize(10)
                .SetFontColor(WebColors.GetRGBColor("#FFFFFF")));
            mainTable.AddHeaderCell(new Cell().SetBackgroundColor(WebColors.GetRGBColor("#064786")).SetBorder(Border.NO_BORDER)
                .SetWidth(UnitValue.CreatePercentValue(10))
                .Add(new Paragraph("COLOR").SetBold().SetTextAlignment(TextAlignment.CENTER)).SetFontSize(10)
                .SetFontColor(WebColors.GetRGBColor("#FFFFFF")));
            mainTable.AddHeaderCell(new Cell().SetBackgroundColor(WebColors.GetRGBColor("#064786")).SetBorder(Border.NO_BORDER)
                .SetWidth(UnitValue.CreatePercentValue(10))
                .Add(new Paragraph("PUNTA").SetBold().SetTextAlignment(TextAlignment.CENTER)).SetFontSize(10)
                .SetFontColor(WebColors.GetRGBColor("#FFFFFF")));


            foreach(var product in products)
            {
                string stringCategory = "";
                foreach (var Categories in product.ProductCategories)
                {
                    
                    stringCategory += Categories.Category.Name + ", ";
                }

                mainTable.AddCell(new Cell().SetFontSize(8).SetBorder(Border.NO_BORDER)
                    .SetWidth(UnitValue.CreatePercentValue(10))
                        .Add(new Paragraph(product.Name)));
                mainTable.AddCell(new Cell().SetFontSize(8).SetBorder(Border.NO_BORDER)
                    .SetWidth(UnitValue.CreatePercentValue(10))
                        .Add(new Paragraph(product.ShortDescription ?? "")));
                mainTable.AddCell(new Cell().SetFontSize(8).SetBorder(Border.NO_BORDER)
                    .SetWidth(UnitValue.CreatePercentValue(10))
                        .Add(new Paragraph(stringCategory)));
                mainTable.AddCell(new Cell().SetFontSize(8).SetBorder(Border.NO_BORDER)
                    .SetWidth(UnitValue.CreatePercentValue(10))
                        .Add(new Paragraph(product.Gtin ?? "" )));
                mainTable.AddCell(new Cell().SetFontSize(8).SetBorder(Border.NO_BORDER)
                        .Add(new Paragraph(product.Sku ?? "")));
                mainTable.AddCell(new Cell().SetFontSize(8).SetBorder(Border.NO_BORDER)
                    .SetWidth(UnitValue.CreatePercentValue(10))
                        .Add(new Paragraph(product.StockQuantity.ToString())));
                mainTable.AddCell(new Cell().SetFontSize(8).SetBorder(Border.NO_BORDER)
                    .SetWidth(UnitValue.CreatePercentValue(10))
                        .Add(new Paragraph(product.Price.ToString("C"))));
                mainTable.AddCell(new Cell().SetFontSize(8).SetBorder(Border.NO_BORDER)
                    .SetWidth(UnitValue.CreatePercentValue(10))
                        .Add(new Paragraph("fdsfsfdsfdsfdsfsdfsdf")));
                mainTable.AddCell(new Cell().SetFontSize(8).SetBorder(Border.NO_BORDER)
                    .SetWidth(UnitValue.CreatePercentValue(10))
                        .Add(new Paragraph("$900")));
            }

            mainTable.AddCell(new Cell().SetBorder(Border.NO_BORDER)
                .Add(new Paragraph(" ")));
            doc.Add(mainTable);

            mainTable = new Table(2);
            mainTable.SetWidth(UnitValue.CreatePercentValue(100));
            mainTable.AddCell(new Cell().SetBorder(Border.NO_BORDER)
                    .Add(new Paragraph(" ")));
            mainTable.AddCell(new Cell().SetBorder(Border.NO_BORDER)
                    .Add(new Paragraph(" ")));
            mainTable.AddCell(new Cell().SetBorder(Border.NO_BORDER)
                    .Add(new Paragraph(" ")));
            doc.Add(mainTable);

            #endregion product


            mainTable = new Table(1);
            mainTable.SetWidth(UnitValue.CreatePercentValue(100));

            mainTable.AddCell(new Cell().SetBorder(Border.NO_BORDER)
                        .Add(new Paragraph(" ")));
            mainTable.AddCell(new Cell().SetBorder(Border.NO_BORDER)
                        .Add(new Paragraph(" ")));

            mainTable.AddCell(new Cell().SetBorder(Border.NO_BORDER)
                .Add(new Paragraph(DateTime.Now.ToString()).SetTextAlignment(TextAlignment.LEFT))).SetFontSize(8);

            doc.Add(mainTable);
            doc.Flush();
            doc.Close();

            return File(ms.ToArray(), MimeTypes.ApplicationPdf);//, $"lista_compras_{parsedDate.ToString("dd-MM-yyyy")}.pdf");
        }
    }
}