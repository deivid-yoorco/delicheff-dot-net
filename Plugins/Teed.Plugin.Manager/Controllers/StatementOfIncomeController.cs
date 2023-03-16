using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Orders;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Manager.Domain.Expenses;
using Teed.Plugin.Manager.Domain.ExpensesCategories;
using Teed.Plugin.Manager.Services;

namespace Teed.Plugin.Manager.Controllers
{
    [Area(AreaNames.Admin)]
    public class StatementOfIncomeController : BasePluginController
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ExpensesService _expensesService;
        private readonly ExpenseFileService _expenseFileService;
        private readonly ExpensesCategoriesService _expensesCategoriesService;

        public StatementOfIncomeController(IOrderService orderService, ExpensesService expensesService,
            ExpensesCategoriesService expensesCategoriesService, ExpenseFileService expenseFileService,
            IProductService productService)
        {
            _orderService = orderService;
            _expensesService = expensesService;
            _expensesCategoriesService = expensesCategoriesService;
            _expenseFileService = expenseFileService;
            _productService = productService;
        }

        public IActionResult Index()
        {
            return View("~/Plugins/Teed.Plugin.Manager/Views/StatementOfIncome/Index.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> GetData(int month, int year)
        {
            DateTime selectedDate = new DateTime(year, month, 1);
            
            IQueryable<Expense> expensesQuery = _expensesService.GetAll();
            List<ExpensesCategories> categories = _expensesCategoriesService.GetAll().OrderBy(x => x.Id).ToList();
            List<ResultModel> resultList = new List<ResultModel>();

            for (int i = 0; i < 3; i++)
            {
                DateTime date = selectedDate.AddMonths(-i);
                List<CategoryExpenseModel> data = new List<CategoryExpenseModel>();
                List<Expense> expenses = expensesQuery.Where(x => x.ExpenseDate.Month == date.Month && x.ExpenseDate.Year == date.Year).ToList();
                decimal incomes = _orderService.SearchOrders()
                    .Where(x => x.PaymentStatus == PaymentStatus.Paid)
                    .Where(x => x.PaidDateUtc.Value.ToLocalTime().Month == date.Month && x.PaidDateUtc.Value.ToLocalTime().Year == date.Year)
                    .Sum(x => x.OrderTotal);

                decimal? orderReportExpense = await GetExpenseByOrderReport(date);
                if (orderReportExpense.HasValue)
                {
                    data.Add(new CategoryExpenseModel()
                    {
                        CategoryId = -1,
                        CategoryName = "Costo de los productos vendidos",
                        CategoryTitle = "Margen bruto",
                        IsLastChild = true,
                        IsShoppingExpense = true,
                        Expenses = null,
                        ParentCategoryId = 0,
                        CategoryExpenseAmount = orderReportExpense ?? 0,
                        ExpensePercentage = incomes > 0 ? ((orderReportExpense ?? 0) / incomes) * 100 : 0
                    });
                }

                foreach (var category in categories)
                {
                    var expensesInCat = expenses.Where(x => x.CategoryId == category.ExpenseCategoryId);
                    decimal sum = expensesInCat.Sum(x => x.Total);
                    List<int> childrenIds = categories.Where(x => x.ParentId == category.ExpenseCategoryId).Select(x => x.ExpenseCategoryId).ToList();
                    while (childrenIds.Count() > 0)
                    {
                        sum += expenses.Where(x => childrenIds.Contains(x.CategoryId)).Sum(x => x.Total);
                        childrenIds = categories.Where(x => childrenIds.Contains(x.ParentId)).Select(x => x.ExpenseCategoryId).ToList();
                    }

                    var model = new CategoryExpenseModel()
                    {
                        CategoryName = category.Value,
                        CategoryTitle = string.IsNullOrWhiteSpace(category.ValueTitle) ? "" : category.ValueTitle,
                        CategoryExpenseAmount = sum,
                        CategoryId = category.ExpenseCategoryId,
                        IsLastChild = !categories.Where(x => x.ParentId == category.ExpenseCategoryId).Any(),
                        ParentCategoryId = category.ParentId,
                        ExpensePercentage = incomes > 0 ? (sum / incomes) * 100 : 0
                    };

                    if (model.IsLastChild)
                    {
                        model.Expenses = expensesInCat.Select(x => new ExpenseDataModel()
                        {
                            Amount = x.Amount,
                            Concept = x.Concept,
                            Date = x.ExpenseDate.ToString("dd-MM-yyyy"),
                            PaymentType = EnumHelper.GetDisplayName(x.PaymentType),
                            VoucherType = EnumHelper.GetDisplayName(x.VoucherType),
                            Tax = x.Tax,
                            Total = x.Total,
                            Files = _expenseFileService.GetAll().Where(y => y.ExpenseId == x.Id).Select(y => new { y.FileUrl, y.FileName }).ToArray()
                        }).ToList();
                    }

                    data.Add(model);
                }

                resultList.Add(new ResultModel()
                {
                    Date = date.ToString("MMMM, yyyy", System.Globalization.CultureInfo.GetCultureInfo("es-MX")).ToUpper(),
                    Incomes = incomes,
                    ExpensesByCategory = data
                });
            }

            resultList.Reverse();

            return Ok(resultList);
        }

        private async Task<decimal> GetExpenseByOrderReport(DateTime date)
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = new TimeSpan(1, 0, 0);
                string url = (Request.IsHttps ? "https" : "http") + $"://{Request.Host}/Admin/OrderReport/GetExpensesByReports?date=" + date;
                HttpResponseMessage result = await client.GetAsync(url);
                if (result.IsSuccessStatusCode)
                {
                    string amount = await result.Content.ReadAsStringAsync();
                    decimal.TryParse(amount, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal resultAmount);
                    return resultAmount;
                }

                return 0;
            }
        }
    }

    public class ResultModel
    {
        public string Date { get; set; }

        public decimal Incomes { get; set; }

        public List<CategoryExpenseModel> ExpensesByCategory { get; set; }
    }

    public class ShoppingExpenseResultModel
    {
        public string ExpenseDate { get; set; }

        public string OrderNumber { get; set; }

        public string ProductName { get; set; }

        public decimal UnitCost { get; set; }

        public decimal RequestedQtyCost { get; set; }

        public string Comments { get; set; }

        public object[] Files { get; set; }
    }

    public class CategoryExpenseModel
    {
        public string CategoryName { get; set; }

        public string CategoryTitle { get; set; }

        public int CategoryId { get; set; }

        public int ParentCategoryId { get; set; }

        public decimal CategoryExpenseAmount { get; set; }

        public bool IsLastChild { get; set; }

        public bool IsShoppingExpense { get; set; }

        public decimal ExpensePercentage { get; set; }

        public List<ExpenseDataModel> Expenses { get; set; }
    }

    public class ExpenseDataModel
    {
        public decimal Amount { get; set; }

        public decimal Tax { get; set; }

        public decimal Total { get; set; }

        public string VoucherType { get; set; }

        public string PaymentType { get; set; }

        public string Date { get; set; }

        public string Concept { get; set; }

        public object[] Files { get; set; }
    }
}