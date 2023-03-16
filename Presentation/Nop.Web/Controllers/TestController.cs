using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Controllers
{
    public class TestController : Controller
    {

        Table shoppingListProductTable;

        public IActionResult Pdf()
        {
            MemoryStream stream = new MemoryStream();
            PdfWriter pdfWriter = new PdfWriter(stream);
            PdfDocument pdfDoc = new PdfDocument(pdfWriter);
            var ps = pdfDoc.GetDefaultPageSize();
            var doc = new Document(pdfDoc, PageSize.LETTER, false);
            doc.SetMargins(20, 20, 50, 20);

            // ARRIBA
            doc.SetTopMargin(35);
            doc.SetLeftMargin(3);
            doc.SetRightMargin(0);
            doc.SetBottomMargin(0);

            // METODO
            Prepare();

            doc.Add(shoppingListProductTable);

            doc.Close();

            return File(stream.ToArray(), MimeTypes.ApplicationPdf);
        }

        private void Prepare()
        {
            shoppingListProductTable = new Table(3);
            shoppingListProductTable.SetBorderCollapse(BorderCollapsePropertyValue.SEPARATE);
            shoppingListProductTable.SetHorizontalBorderSpacing(10);
            shoppingListProductTable.SetWidth(PageSize.A4.GetWidth() + 12);

            for (int i = 0; i < 30; i++)
            {
                var cell = new Cell();
                cell.SetHeight(67);
                cell.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
                cell.SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                cell.SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE);

                var productName = "Nombre del producto de prueba laaargo muy largo, hasta llegar a tres líneas";
                productName = productName.Count() > 60 ? productName.Substring(0, 60) + "..." : productName;

                var product = new Paragraph(productName);
                var cantidad = new Paragraph("R2 - Orden #3698 - 1.35 KG");
                var propiedad = new Paragraph("Para la semana");

                cell.Add(product.SetBold().SetFontSize(12));
                cell.Add(cantidad.SetFontSize(10));
                cell.Add(propiedad.SetFontSize(10));
                
                shoppingListProductTable.AddCell(cell);
            }
        }
    }
}
