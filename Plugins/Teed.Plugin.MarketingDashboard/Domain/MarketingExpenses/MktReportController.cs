using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
using Teed.Plugin.MarketingDashboard.Services;

namespace Teed.Plugin.MarketingDashboard.Domain.MarketingExpenses
{
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    public class MktReportController : BasePluginController
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
        private readonly ICustomerService _customerService;
        private readonly IManufacturerService _manufacturerService;
        private readonly MarketingExpenseService _marketingExpenseService;
        private readonly MarketingExpenseTypeService _marketingExpenseTypeService;
        private readonly MarketingDashboardDataService _marketingDashboardDataService;

        public MktReportController(IProductService productService, ICategoryService categoryService,
            ISpecificationAttributeService specificationAttributeService,
            IProductTagService productTagService, IProductAttributeService productAttributeService,
            IOrderService orderService, IShippingService shippingService,
            IUrlRecordService recordService,
            ICustomerService customerService,
            IManufacturerService manufacturerService,
            MarketingExpenseService marketingExpenseService, MarketingExpenseTypeService marketingExpenseTypeService,
            MarketingDashboardDataService marketingDashboardDataService)
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

            _marketingExpenseService = marketingExpenseService;
            _marketingExpenseTypeService = marketingExpenseTypeService;

            _marketingDashboardDataService = marketingDashboardDataService;
        }

        [HttpGet]
        public IActionResult GenerateExcel1(int days = 30)
        {
            var today = DateTime.Now.Date;
            var controlDate = today.AddDays(-1 * days);
            while (controlDate.DayOfWeek > 0)
            {
                controlDate = controlDate.AddDays(-1);
            }

            var expenses = _marketingExpenseService.GetAll().ToList();
            var expenseTypes = _marketingExpenseTypeService.GetAll().ToList();

            var getAllTodo = _marketingDashboardDataService.GetAll().ToList();
            var allDashboardData = getAllTodo.GroupBy(x => x.GenerationDateUtc)
                .OrderByDescending(x => x.Key)
                .FirstOrDefault()
                .Select(x => x)
                .OrderByDescending(x => x.InitDate);
            var dashboardData10 = allDashboardData.Where(x => x.MarketingDashboardDataTypeId == 10);

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");

                    int row = 1;
                    int col = 1;

                    worksheet.Cells[row, col].Value = "Indicadores semanales de MKT";
                    worksheet.Cells[row + 1, col].Value = "ID";
                    worksheet.Cells[row + 1, col + 1].Value = "Tipo de gasto";

                    col = 3;
                    foreach (var item in dashboardData10)
                    {
                        worksheet.Cells[1, col].Value = item.InitDate;
                        worksheet.Cells[1, col].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[2, col].Value = item.EndDate;
                        worksheet.Cells[2, col].Style.Numberformat.Format = "dd-mm-yyyy";
                        col++;
                    }

                    row = 2;
                    foreach (var item in expenseTypes)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = "0";
                        worksheet.Cells[row, 2].Value = item.Name;
                        col = 3;
                        foreach (var item2 in dashboardData10)
                        {
                            var filtered = expenses.Where(x => x.ExpenseTypeId == item.Id).ToList();
                            decimal marketingExpenses = _marketingExpenseService.GetDailyExpense(filtered, item2.EndDate)
                                .Where(x => x.Date >= item2.InitDate && x.Date <= item2.EndDate)
                                .Select(x => x.Amount)
                                .DefaultIfEmpty()
                                .Sum();
                            worksheet.Cells[row, col].Value = marketingExpenses;
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

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"gasto_por_tipo_{days}_dias.xlsx");
            }
        }
    }
}
