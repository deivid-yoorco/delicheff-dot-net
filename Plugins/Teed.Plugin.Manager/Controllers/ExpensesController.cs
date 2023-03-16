using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Manager.Domain.Expenses;
using Teed.Plugin.Manager.Domain.ExpensesCategories;
using Teed.Plugin.Manager.Helpers;
using Teed.Plugin.Manager.Models.Expenses;
using Teed.Plugin.Manager.Models.ExpensesCategories;
using Teed.Plugin.Manager.Services;
using Teed.Plugin.Medical.Security;

namespace Teed.Plugin.Manager.Controllers
{
    [Area(AreaNames.Admin)]
    public class ExpensesController : BasePluginController
    {
        private readonly ExpensesService _expensesService;
        private readonly ExpenseFileService _expenseFileService;
        private readonly ExpensesCategoriesService _expensesCategoriesService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;

        public ExpensesController(ExpensesService expensesService, IPermissionService permissionService, IWorkContext workContext, 
            ICustomerService customerService, ExpensesCategoriesService expensesCategoriesService, ExpenseFileService expenseFileService)
        {
            _expensesService = expensesService;
            _permissionService = permissionService;
            _workContext = workContext;
            _customerService = customerService;
            _expensesCategoriesService = expensesCategoriesService;
            _expenseFileService = expenseFileService;
        }

        public IActionResult List()
        {
            return View("~/Plugins/Teed.Plugin.Manager/Views/Expenses/List.cshtml");
        }

        public IActionResult Create()
        {
            return View("~/Plugins/Teed.Plugin.Manager/Views/Expenses/Create.cshtml");
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedManager))
                return AccessDeniedView();

            Expense expense = _expensesService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (expense == null) return NotFound();

            EditViewModel model = new EditViewModel()
            {
                Amount = expense.Amount,
                CategoryId = expense.CategoryId,
                Comments = expense.Comments,
                Concept = expense.Concept,
                UploadedFiles = _expenseFileService.GetAll().Where(x => x.ExpenseId == expense.Id).ToList(),
                PaymentType = expense.PaymentType,
                VoucherType = expense.VoucherType,
                Tax = expense.Tax,
                Total = expense.Total,
                Id = expense.Id,
                SelectedDate = expense.ExpenseDate.ToString("dd-MM-yyyy")
            };

            return View("~/Plugins/Teed.Plugin.Manager/Views/Expenses/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var query = _expensesService.GetAll();
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedManager))
            {
                query = query.Where(x => x.CreatedByUserId == _workContext.CurrentCustomer.Id);
            }
            var queryList = query.OrderByDescending(m => m.ExpenseDate);
            var pagedList = new PagedList<Expense>(queryList, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new {
                    Date = x.ExpenseDate.ToString("dd-MM-yyyy"),
                    User = _customerService.GetCustomerById(x.CreatedByUserId).GetFullName(),
                    x.Total,
                    x.Concept,
                    PaymentType = EnumHelper.GetDisplayName(x.PaymentType),
                    x.Id
                }).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult CategoryListData()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var categories = _expensesCategoriesService.GetAll().ToList();
            var lastChildList = new List<ExpensesCategories>();
            foreach (var category in categories)
            {
                var hasChild = categories.Where(x => x.ParentId == category.ExpenseCategoryId).Any();
                if (!hasChild)
                {
                    lastChildList.Add(category);
                }
            }

            var elements = lastChildList.Select(x => new
            {
                Id = x.ExpenseCategoryId,
                Category = ExpenseCategoryHelper.GetFormattedBreadCrumb(x, _expensesCategoriesService)
            }).ToList();

            return Json(elements);
        }

        [HttpPost]
        public IActionResult UserListData()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();
            
            var users = _customerService.GetAllCustomers().Where(x => !string.IsNullOrWhiteSpace(x.Email)).Where(x => x.GetCustomerRoleIds().Count() > 1).ToList();
            var elements = users.Select(x => new
            {
                Id = x.Id,
                User = x.GetFullName()
            }).ToList();

            return Json(elements);
        }

        [HttpPost, ParameterBasedOnFormName("cash", "cashOnly")]
        public async Task<IActionResult> Create(CreateViewModel model, bool cashOnly)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            DateTime dateParsed = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(model.SelectedDate))
            {
                dateParsed = DateTime.ParseExact(model.SelectedDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            }

            var expense = new Expense()
            {
                Amount = model.Amount,
                CategoryId = model.CategoryId,
                Comments = model.Comments,
                Concept = model.Concept,
                CreatedByUserId = _workContext.CurrentCustomer.Id,
                ExpenseDate = dateParsed,
                PaymentType = model.PaymentType,
                Tax = model.Tax,
                Total = model.Total,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + " creó un registro de gasto.",
                VoucherType = model.VoucherType
            };
            _expensesService.Insert(expense);

            foreach (var file in model.Files)
            {
                string baseDirectoryName = "expenses-media";
                string directoryPath = $"./wwwroot/{baseDirectoryName}/{expense.Id}";
                Directory.CreateDirectory(directoryPath);

                string rawPath = $"{directoryPath}/{file.FileName}";
                string newFileName = $"{DateTime.Now.ToString("ddMMyyyyhhmmss")}-" + file.FileName;
                string fullPath = $"{directoryPath}/{newFileName}";
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                ExpenseFile expenseFile = new ExpenseFile()
                {
                    Extension = Path.GetExtension(rawPath),
                    ExpenseId = expense.Id,
                    FileName = file.FileName,
                    FileType = file.ContentType,
                    FileUrl = $"/{baseDirectoryName}/{expense.Id}/{newFileName}",
                    Size = (int)file.Length,
                    UploadedByUserId = _workContext.CurrentCustomer.Id
                };
                _expenseFileService.Insert(expenseFile);
            }

            if (cashOnly)
            {
                return RedirectToAction("List", "CashExpenses");
            }
            else
            {
                return RedirectToAction("List");
            }
        }

        [HttpPost, ParameterBasedOnFormName("cash", "cashOnly")]
        public async Task<IActionResult> Edit(EditViewModel model, bool cashOnly)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            Expense expense = _expensesService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (expense == null) return NotFound();

            DateTime dateParsed = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(model.SelectedDate))
            {
                dateParsed = DateTime.ParseExact(model.SelectedDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            }

            var log = string.Empty;
            if (expense.Amount != model.Amount)
            {
                log += $". Cambió el monto de {expense.Amount} a {model.Amount}";
            }

            if (expense.CategoryId != model.CategoryId)
            {
                var cat1 = _expensesCategoriesService.GetAll().Where(x => x.Id == expense.CategoryId).FirstOrDefault();
                var cat2 = _expensesCategoriesService.GetAll().Where(x => x.Id == model.CategoryId).FirstOrDefault();
                log += $". Cambió la categoria de {cat1?.Value} ({expense.CategoryId}) a {cat2?.Value} ({model.CategoryId})";
            }

            if (expense.Comments != model.Comments)
            {
                log += $". Cambió los comentarios de {expense.Comments} a {model.Comments}";
            }

            if (expense.Concept != model.Concept)
            {
                log += $". Cambió el concepto de {expense.Concept} a {model.Concept}";
            }

            if (expense.ExpenseDate.Date != dateParsed.Date)
            {
                log += $". Cambió la fecha del gasto de {expense.ExpenseDate.ToString("dd-MM-yyyy")} a {dateParsed.ToString("dd-MM-yyyy")}";
            }

            if (expense.PaymentType != model.PaymentType)
            {
                log += $". Cambió el tipo de pago de {EnumHelper.GetDisplayName(expense.PaymentType)} a {EnumHelper.GetDisplayName(model.PaymentType)}";
            }

            if (expense.Tax != model.Tax)
            {
                log += $". Cambió el impuesto de {expense.Tax} a {model.Tax}";
            }

            if (expense.Total != model.Total)
            {
                log += $". Cambió el impuesto de {expense.Total} a {model.Total}";
            }

            if (expense.VoucherType != model.VoucherType)
            {
                log += $". Cambió el tipo de pago de {EnumHelper.GetDisplayName(expense.VoucherType)} a {EnumHelper.GetDisplayName(model.VoucherType)}";
            }

            if (model.Files.Count() > 0)
            {
                log += $". Agregó nuevos archivos";
            }

            expense.Amount = model.Amount;
            expense.CategoryId = model.CategoryId;
            expense.Comments = model.Comments;
            expense.Concept = model.Concept;
            expense.ExpenseDate = dateParsed;
            expense.PaymentType = model.PaymentType;
            expense.Tax = model.Tax;
            expense.Total = model.Total;
            expense.VoucherType = model.VoucherType;
            expense.Log += "/n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + $"{_workContext.CurrentCustomer.GetFullName()} ({_workContext.CurrentCustomer.Id}) editó el registro de gastos" + log + ".";

            _expensesService.Update(expense);

            foreach (var file in model.Files)
            {
                string baseDirectoryName = "expenses-media";
                string directoryPath = $"./wwwroot/{baseDirectoryName}/{expense.Id}";
                Directory.CreateDirectory(directoryPath);

                string rawPath = $"{directoryPath}/{file.FileName}";
                string newFileName = $"{DateTime.Now.ToString("ddMMyyyyhhmmss")}-" + file.FileName;
                string fullPath = $"{directoryPath}/{newFileName}";
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                ExpenseFile expenseFile = new ExpenseFile()
                {
                    Extension = Path.GetExtension(rawPath),
                    ExpenseId = expense.Id,
                    FileName = file.FileName,
                    FileType = file.ContentType,
                    FileUrl = $"/{baseDirectoryName}/{expense.Id}/{newFileName}",
                    Size = (int)file.Length,
                    UploadedByUserId = _workContext.CurrentCustomer.Id
                };
                _expenseFileService.Insert(expenseFile);
            }

            if (cashOnly)
            {
                return RedirectToAction("List", "CashExpenses");
            }
            else
            {
                return RedirectToAction("List");
            }
        }
    }
}
