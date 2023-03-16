using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.MarketingDashboard.Services;

namespace Teed.Plugin.MarketingDashboard.Controllers
{
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    public class ReportController : BasePluginController
    {
        private readonly MarketingExpenseService _marketingExpenseService;

        public ReportController(MarketingExpenseService marketingExpenseService)
        {
            _marketingExpenseService = marketingExpenseService;
        }

        // Gasto publicitario mensual
        [HttpGet]
        public IActionResult GenerateMktExcel1()
        {
            var expenses = _marketingExpenseService.GetAll().ToList();
            var result = new List<DateAmountModel>();
            foreach (var expense in expenses)
            {
                var days = (expense.EndDate - expense.InitDate).TotalDays + 1;
                var dailyAmount = (decimal)expense.Amount / (decimal)days;
                for (DateTime i = expense.InitDate; i <= expense.EndDate; i = i.AddDays(1))
                {
                    result.Add(new DateAmountModel()
                    {
                        Date = i,
                        Amount = dailyAmount,
                    });
                }
            }

            var groupedResult = result.GroupBy(x => new DateTime(x.Date.Year, x.Date.Month, 1)).OrderBy(x => x.Key).ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Mes";
                    worksheet.Cells[row, 2].Value = "Gasto publicitario";

                    foreach (var item in groupedResult)
                    {
                        row++;

                        worksheet.Cells[row, 1].Value = item.Key;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "mmmm, yyyy";
                        worksheet.Cells[row, 2].Value = item.Select(x => x.Amount).DefaultIfEmpty().Sum();
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"gastos_publicitarios_mensuales.xlsx");
            }
        }
    }

    public class DateAmountModel
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }
}
